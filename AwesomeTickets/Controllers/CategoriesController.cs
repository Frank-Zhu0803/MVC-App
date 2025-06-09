using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using AwesomeTickets.Models;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;

namespace AwesomeTickets.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            return View(await _context.Categories.ToListAsync());
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.Events)
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CategoryId,Title")] Category category)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Console.WriteLine($"Creating category with title: {category.Title}");
                    category.Events = new List<Event>(); // Initialize Events collection
                    _context.Add(category);
                    await _context.SaveChangesAsync();
                    Console.WriteLine("Category created successfully with ID: " + category.CategoryId);
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    Console.WriteLine("ModelState is invalid:");
                    foreach (var modelState in ModelState.Values)
                    {
                        foreach (var error in modelState.Errors)
                        {
                            Console.WriteLine(error.ErrorMessage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating category: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("CategoryId,Title")] Category category)
        {
            if (id != category.CategoryId)
            {
                return NotFound();
            }

            try
            {
                if (ModelState.IsValid)
                {
                    Console.WriteLine($"Updating category with ID: {category.CategoryId}, Title: {category.Title}");
                    
                    // Get the existing category to preserve the Events collection
                    var existingCategory = await _context.Categories
                        .Include(c => c.Events)
                        .FirstOrDefaultAsync(c => c.CategoryId == id);
                    
                    if (existingCategory != null)
                    {
                        // Update only the Title
                        existingCategory.Title = category.Title;
                        _context.Update(existingCategory);
                        await _context.SaveChangesAsync();
                        Console.WriteLine("Category updated successfully");
                    }
                    else
                    {
                        Console.WriteLine("Category not found");
                        return NotFound();
                    }
                    
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    Console.WriteLine("ModelState is invalid:");
                    foreach (var modelState in ModelState.Values)
                    {
                        foreach (var error in modelState.Errors)
                        {
                            Console.WriteLine(error.ErrorMessage);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating category: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
            
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(m => m.CategoryId == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                // Check if there are related events
                var hasEvents = await _context.Events.AnyAsync(e => e.CategoryId == id);
                if (hasEvents)
                {
                    ModelState.AddModelError(string.Empty, "Cannot delete category because it has related events.");
                    return View(category);
                }
                
                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Categories.Any(e => e.CategoryId == id);
        }
    }
} 