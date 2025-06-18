// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Accessibility enhancements

document.addEventListener('DOMContentLoaded', function() {
    initAccessibilityFeatures();
});

function initAccessibilityFeatures() {
    // Make cards focusable and navigable with keyboard
    makeCardsFocusable();
    
    // Initialize the back to top button
    initBackToTop();
    
    // Add keyboard shortcut hints to elements with accesskey attributes
    addAccessKeyHints();
    
    // Toggle high contrast mode
    initHighContrastMode();
    
    // Add ARIA roles to elements that might need them
    enhanceAriaAttributes();
}

// Make cards focusable for keyboard navigation
function makeCardsFocusable() {
    const cards = document.querySelectorAll('.card');
    cards.forEach(function(card) {
        // Only add these attributes if they don't already exist
        if (!card.hasAttribute('tabindex')) {
            card.setAttribute('tabindex', '0');
        }
        
        if (!card.hasAttribute('role')) {
            card.setAttribute('role', 'article');
        }
        
        // Add keyboard event for clicking the card's main action
        card.addEventListener('keydown', function(e) {
            if (e.key === 'Enter' || e.key === ' ') {
                const mainAction = card.querySelector('a[asp-action="Details"], a.btn-primary, button.btn-primary');
                if (mainAction) {
                    e.preventDefault();
                    mainAction.click();
                }
            }
        });
    });
}

// Initialize the back to top button
function initBackToTop() {
    const backToTopBtn = document.getElementById('backToTopBtn');
    
    if (backToTopBtn) {
        // Show/hide button based on scroll position
        window.addEventListener('scroll', function() {
            if (document.body.scrollTop > 20 || document.documentElement.scrollTop > 20) {
                backToTopBtn.style.display = 'block';
            } else {
                backToTopBtn.style.display = 'none';
            }
        });
        
        // Scroll to top when clicked
        backToTopBtn.addEventListener('click', function() {
            document.body.scrollTop = 0; // For Safari
            document.documentElement.scrollTop = 0; // For Chrome, Firefox, IE and Opera
            
            // Focus on main content for better accessibility
            const mainContent = document.getElementById('main-content');
            if (mainContent) {
                mainContent.focus();
            }
        });
    }
}

// Add visual hints for access keys
function addAccessKeyHints() {
    const accessKeyElements = document.querySelectorAll('[accesskey]');
    
    accessKeyElements.forEach(element => {
        const key = element.getAttribute('accesskey').toUpperCase();
        
        // Check if the hint already exists
        let hintExists = false;
        element.querySelectorAll('span').forEach(span => {
            if (span.textContent.includes(`(Alt+${key})`)) {
                hintExists = true;
            }
        });
        
        if (!hintExists) {
            const hintText = document.createElement('span');
            hintText.classList.add('visually-hidden');
            hintText.textContent = ` (Alt+${key})`;
            element.appendChild(hintText);
        }
    });
}

// Initialize high contrast mode toggle
function initHighContrastMode() {
    // Check if high contrast mode is enabled in localStorage
    const highContrastEnabled = localStorage.getItem('highContrastMode') === 'true';
    
    if (highContrastEnabled) {
        document.body.classList.add('high-contrast-mode');
    }
    
    // Add high contrast toggle to footer if it doesn't exist
    if (!document.getElementById('high-contrast-toggle')) {
        const footer = document.querySelector('footer .container .row .col');
        
        if (footer) {
            const toggleLink = document.createElement('a');
            toggleLink.id = 'high-contrast-toggle';
            toggleLink.href = '#';
            toggleLink.className = 'text-decoration-none ms-3';
            toggleLink.setAttribute('aria-label', highContrastEnabled ? 'Disable high contrast mode' : 'Enable high contrast mode');
            toggleLink.textContent = highContrastEnabled ? 'Standard Contrast' : 'High Contrast';
            
            toggleLink.addEventListener('click', function(e) {
                e.preventDefault();
                toggleHighContrastMode();
            });
            
            // Add the toggle to the footer
            const accessibilityLinks = footer.querySelector('div');
            if (accessibilityLinks) {
                accessibilityLinks.appendChild(toggleLink);
            }
        }
    }
}

// Toggle high contrast mode
function toggleHighContrastMode() {
    const body = document.body;
    const isHighContrast = body.classList.contains('high-contrast-mode');
    const toggleLink = document.getElementById('high-contrast-toggle');
    
    if (isHighContrast) {
        body.classList.remove('high-contrast-mode');
        localStorage.setItem('highContrastMode', 'false');
        if (toggleLink) {
            toggleLink.textContent = 'High Contrast';
            toggleLink.setAttribute('aria-label', 'Enable high contrast mode');
        }
    } else {
        body.classList.add('high-contrast-mode');
        localStorage.setItem('highContrastMode', 'true');
        if (toggleLink) {
            toggleLink.textContent = 'Standard Contrast';
            toggleLink.setAttribute('aria-label', 'Disable high contrast mode');
        }
    }
}

// Enhance ARIA attributes for better screen reader support
function enhanceAriaAttributes() {
    // Add missing ARIA labels to form controls
    document.querySelectorAll('input, select, textarea').forEach(element => {
        if (!element.hasAttribute('aria-label') && !element.hasAttribute('aria-labelledby')) {
            const label = document.querySelector(`label[for="${element.id}"]`);
            if (label) {
                if (!label.id) {
                    label.id = `label-${element.id}`;
                }
                element.setAttribute('aria-labelledby', label.id);
            }
        }
    });
    
    // Make sure all buttons have accessible names
    document.querySelectorAll('button').forEach(button => {
        if (!button.hasAttribute('aria-label') && !button.textContent.trim()) {
            const icon = button.querySelector('i.bi');
            if (icon) {
                const iconClass = Array.from(icon.classList)
                    .find(className => className.startsWith('bi-'));
                
                if (iconClass) {
                    const iconName = iconClass.replace('bi-', '').replace(/-/g, ' ');
                    button.setAttribute('aria-label', iconName);
                }
            }
        }
    });
}
