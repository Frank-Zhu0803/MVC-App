using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AwesomeTickets.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace AwesomeTickets.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Cart
        public async Task<IActionResult> Index()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            
            var cart = await GetOrCreateCartAsync(userId);
            
            // Include Event data for each cart item
            await _context.Entry(cart)
                .Collection(c => c.Items)
                .LoadAsync();
                
            foreach (var item in cart.Items)
            {
                await _context.Entry(item)
                    .Reference(i => i.Event)
                    .LoadAsync();
            }
            
            return View(cart);
        }
        
        // POST: Cart/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart(int eventId, int quantity = 1)
        {
            if (quantity <= 0)
            {
                TempData["Error"] = "Quantity must be greater than zero";
                return RedirectToAction("Details", "Browse", new { id = eventId });
            }
            
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var eventItem = await _context.Events.FindAsync(eventId);
            
            if (eventItem == null)
            {
                return NotFound("Event not found");
            }

            if (eventItem.AvailableQuantity < quantity)
            {
                TempData["Error"] = "Not enough tickets available";
                return RedirectToAction("Details", "Browse", new { id = eventId });
            }
            
            var cart = await GetOrCreateCartAsync(userId);
            
            var existingItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartId == cart.CartId && ci.EventId == eventId);
                
            if (existingItem != null)
            {
                if (eventItem.AvailableQuantity < existingItem.Quantity + quantity)
                {
                    TempData["Error"] = "Not enough tickets available";
                    return RedirectToAction("Details", "Browse", new { id = eventId });
                }
                existingItem.Quantity += quantity;
            }
            else
            {
                var newItem = new CartItem
                {
                    CartId = cart.CartId,
                    EventId = eventId,
                    Quantity = quantity,
                    UnitPrice = eventItem.Price
                };
                
                _context.CartItems.Add(newItem);
            }
            
            await _context.SaveChangesAsync();
            TempData["Success"] = "Item added to cart successfully";
            return RedirectToAction(nameof(Index));
        }
        
        // POST: Cart/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int cartItemId, int quantity)
        {
            if (quantity <= 0)
            {
                return await RemoveFromCart(cartItemId);
            }
            
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var cart = await GetCartForUserAsync(userId);
            
            if (cart == null)
            {
                return NotFound("Cart not found");
            }
            
            var cartItem = await _context.CartItems
                .Include(ci => ci.Event)
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId && ci.CartId == cart.CartId);
                
            if (cartItem == null)
            {
                return NotFound("Cart item not found");
            }

            if (cartItem.Event.AvailableQuantity < quantity)
            {
                TempData["Error"] = "Not enough tickets available";
                return RedirectToAction(nameof(Index));
            }
            
            cartItem.Quantity = quantity;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Cart updated successfully";
            return RedirectToAction(nameof(Index));
        }
        
        // POST: Cart/RemoveFromCart
        [HttpPost]
        public async Task<IActionResult> RemoveFromCart(int cartItemId)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var cart = await GetCartForUserAsync(userId);
            
            if (cart == null)
            {
                return NotFound("Cart not found");
            }
            
            var cartItem = await _context.CartItems
                .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId && ci.CartId == cart.CartId);
                
            if (cartItem == null)
            {
                return NotFound("Cart item not found");
            }
            
            _context.CartItems.Remove(cartItem);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Item removed from cart";
            return RedirectToAction(nameof(Index));
        }
        
        // POST: Cart/Clear
        [HttpPost]
        public async Task<IActionResult> Clear()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var cart = await GetCartForUserAsync(userId);
            
            if (cart == null)
            {
                return NotFound("Cart not found");
            }
            
            var cartItems = await _context.CartItems
                .Where(ci => ci.CartId == cart.CartId)
                .ToListAsync();
                
            _context.CartItems.RemoveRange(cartItems);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Cart cleared successfully";
            return RedirectToAction(nameof(Index));
        }
        
        [HttpPost]
        public async Task<IActionResult> Checkout()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value);
            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Event)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
            {
                return RedirectToAction(nameof(Index));
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                foreach (var item in cart.Items)
                {
                    var @event = await _context.Events.FindAsync(item.EventId);
                    if (@event == null || @event.AvailableQuantity < item.Quantity)
                    {
                        TempData["Error"] = $"Not enough tickets available for {@event?.Title ?? "Unknown Event"}";
                        return RedirectToAction(nameof(Index));
                    }
                    @event.AvailableQuantity -= item.Quantity;
                }

                var order = new EventOrder
                {
                    UserId = userId,
                    OrderDate = DateTime.Now,
                    TotalAmount = cart.Items.Sum(i => i.Quantity * i.Event.Price)
                };

                _context.EventOrders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var item in cart.Items)
                {
                    var orderItem = new EventOrderItem
                    {
                        OrderId = order.OrderId,
                        EventId = item.EventId,
                        Quantity = item.Quantity,
                        UnitPrice = item.Event.Price
                    };
                    _context.EventOrderItems.Add(orderItem);
                }

                _context.CartItems.RemoveRange(cart.Items);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["Success"] = "Order placed successfully!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                TempData["Error"] = "An error occurred while processing your order.";
                return RedirectToAction(nameof(Index));
            }
        }
        
        // Helper methods
        private async Task<Cart> GetOrCreateCartAsync(int userId)
        {
            var cart = await GetCartForUserAsync(userId);
            
            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId
                };
                
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }
            
            return cart;
        }
        
        private async Task<Cart> GetCartForUserAsync(int userId)
        {
            return await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);
        }
    }
} 