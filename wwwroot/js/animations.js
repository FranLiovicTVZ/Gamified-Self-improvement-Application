// Advanced JavaScript Animations for Gamified Self Improvement Application
// Koristi se za UX animacije, validaciju i interaktivne elemente

(function () {
    'use strict';

    // Animacije za Activity cards
    function animateActivityCards() {
        const cards = document.querySelectorAll('.activity-card');
        cards.forEach((card, index) => {
            card.style.opacity = '0';
            card.style.transform = 'translateY(20px)';
            card.style.transition = `all 0.5s ease-out ${index * 0.1}s`;

            // Trigger animation
            setTimeout(() => {
                card.style.opacity = '1';
                card.style.transform = 'translateY(0)';
            }, 10);

            // Hover effect
            card.addEventListener('mouseenter', function () {
                this.style.transform = 'translateY(-5px)';
                this.style.boxShadow = '0 8px 16px rgba(0, 0, 0, 0.15)';
            });

            card.addEventListener('mouseleave', function () {
                this.style.transform = 'translateY(0)';
                this.style.boxShadow = '0 2px 4px rgba(0, 0, 0, 0.1)';
            });
        });
    }

    // Animacije za User cards
    function animateUserCards() {
        const cards = document.querySelectorAll('.user-card');
        cards.forEach((card, index) => {
            card.style.opacity = '0';
            card.style.scale = '0.95';
            card.style.transition = `all 0.6s cubic-bezier(0.34, 1.56, 0.64, 1) ${index * 0.08}s`;

            setTimeout(() => {
                card.style.opacity = '1';
                card.style.scale = '1';
            }, 10);

            // Hover effect
            card.addEventListener('mouseenter', function () {
                this.style.transform = 'scale(1.02)';
                this.style.boxShadow = '0 12px 24px rgba(0, 0, 0, 0.12)';
            });

            card.addEventListener('mouseleave', function () {
                this.style.transform = 'scale(1)';
                this.style.boxShadow = '0 2px 8px rgba(0, 0, 0, 0.08)';
            });
        });
    }

    // XP Progress bar animation
    function animateXPBar() {
        const xpBars = document.querySelectorAll('.xp-bar');
        xpBars.forEach(bar => {
            const width = bar.style.width;
            bar.style.width = '0';
            bar.style.transition = 'width 1s cubic-bezier(0.25, 0.46, 0.45, 0.94)';

            setTimeout(() => {
                bar.style.width = width;
            }, 100);

            // Pulse effect
            const animation = setInterval(() => {
                bar.style.opacity = '0.8';
                setTimeout(() => {
                    bar.style.opacity = '1';
                }, 500);
            }, 2000);
        });
    }

    // Form validation with animation
    function animateFormValidation() {
        const formInputs = document.querySelectorAll('.form-control, .form-select');
        formInputs.forEach(input => {
            input.addEventListener('blur', function () {
                if (this.value.trim() === '') {
                    this.classList.add('is-invalid');
                    this.style.animation = 'shake 0.3s';
                } else {
                    this.classList.remove('is-invalid');
                    this.classList.add('is-valid');
                    this.style.animation = 'slideIn 0.3s';
                }
            });

            input.addEventListener('focus', function () {
                this.classList.remove('is-invalid');
                this.style.borderColor = '#007bff';
            });
        });
    }

    // Button click animation
    function animateButtons() {
        const buttons = document.querySelectorAll('.btn');
        buttons.forEach(btn => {
            btn.addEventListener('click', function (e) {
                const ripple = document.createElement('span');
                ripple.style.position = 'absolute';
                ripple.style.width = '20px';
                ripple.style.height = '20px';
                ripple.style.backgroundColor = 'rgba(255, 255, 255, 0.5)';
                ripple.style.borderRadius = '50%';
                ripple.style.pointerEvents = 'none';
                ripple.style.animation = 'ripple 0.6s ease-out';

                const rect = this.getBoundingClientRect();
                ripple.style.left = (e.clientX - rect.left - 10) + 'px';
                ripple.style.top = (e.clientY - rect.top - 10) + 'px';

                this.style.position = 'relative';
                this.style.overflow = 'hidden';
                this.appendChild(ripple);

                setTimeout(() => ripple.remove(), 600);
            });
        });
    }

    // Number animation (counter)
    function animateNumbers() {
        const numbers = document.querySelectorAll('[data-animate-number]');
        numbers.forEach(element => {
            const targetValue = parseInt(element.textContent);
            const duration = 1000;
            const startTime = performance.now();

            const animate = (currentTime) => {
                const elapsed = currentTime - startTime;
                const progress = Math.min(elapsed / duration, 1);
                const currentValue = Math.floor(targetValue * progress);
                element.textContent = currentValue;

                if (progress < 1) {
                    requestAnimationFrame(animate);
                }
            };

            requestAnimationFrame(animate);
        });
    }

    // Fade in on scroll
    function animateFadeOnScroll() {
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.style.opacity = '1';
                    entry.target.style.transform = 'translateY(0)';
                } else {
                    entry.target.style.opacity = '0';
                    entry.target.style.transform = 'translateY(20px)';
                }
            });
        }, { threshold: 0.1 });

        document.querySelectorAll('.fade-on-scroll').forEach(el => {
            el.style.transition = 'all 0.5s ease-out';
            observer.observe(el);
        });
    }

    // Search bar animation
    function animateSearchBar() {
        const searchInput = document.getElementById('searchInput');
        if (searchInput) {
            searchInput.addEventListener('focus', function () {
                this.style.transform = 'scale(1.02)';
                this.style.boxShadow = '0 4px 12px rgba(0, 123, 255, 0.2)';
            });

            searchInput.addEventListener('blur', function () {
                this.style.transform = 'scale(1)';
                this.style.boxShadow = 'none';
            });
        }
    }

    // Tooltip animation
    function animateTooltips() {
        const tooltips = document.querySelectorAll('[data-tooltip]');
        tooltips.forEach(el => {
            el.addEventListener('mouseenter', function () {
                const tooltip = document.createElement('div');
                tooltip.textContent = this.getAttribute('data-tooltip');
                tooltip.style.position = 'absolute';
                tooltip.style.backgroundColor = '#333';
                tooltip.style.color = 'white';
                tooltip.style.padding = '5px 10px';
                tooltip.style.borderRadius = '4px';
                tooltip.style.fontSize = '12px';
                tooltip.style.zIndex = '1000';
                tooltip.style.opacity = '0';
                tooltip.style.transform = 'translateY(-10px)';
                tooltip.style.transition = 'all 0.2s';
                tooltip.className = 'temp-tooltip';

                document.body.appendChild(tooltip);

                setTimeout(() => {
                    tooltip.style.opacity = '1';
                    tooltip.style.transform = 'translateY(0)';
                }, 10);
            });

            el.addEventListener('mouseleave', function () {
                const tooltip = document.querySelector('.temp-tooltip');
                if (tooltip) {
                    tooltip.style.opacity = '0';
                    setTimeout(() => tooltip.remove(), 200);
                }
            });
        });
    }

    // Notification animation
    window.showNotification = function (message, type = 'success') {
        const notification = document.createElement('div');
        notification.style.position = 'fixed';
        notification.style.top = '20px';
        notification.style.right = '20px';
        notification.style.backgroundColor = type === 'success' ? '#28a745' : '#dc3545';
        notification.style.color = 'white';
        notification.style.padding = '15px 20px';
        notification.style.borderRadius = '4px';
        notification.style.boxShadow = '0 4px 12px rgba(0, 0, 0, 0.2)';
        notification.style.zIndex = '2000';
        notification.style.opacity = '0';
        notification.style.transform = 'translateX(400px)';
        notification.style.transition = 'all 0.3s cubic-bezier(0.34, 1.56, 0.64, 1)';
        notification.textContent = message;

        document.body.appendChild(notification);

        setTimeout(() => {
            notification.style.opacity = '1';
            notification.style.transform = 'translateX(0)';
        }, 10);

        setTimeout(() => {
            notification.style.opacity = '0';
            notification.style.transform = 'translateX(400px)';
            setTimeout(() => notification.remove(), 300);
        }, 3000);
    };

    // CSS Animations
    const style = document.createElement('style');
    style.textContent = `
        @keyframes shake {
            0%, 100% { transform: translateX(0); }
            25% { transform: translateX(-10px); }
            75% { transform: translateX(10px); }
        }

        @keyframes slideIn {
            from { transform: translateX(-20px); opacity: 0; }
            to { transform: translateX(0); opacity: 1; }
        }

        @keyframes ripple {
            to {
                transform: scale(4);
                opacity: 0;
            }
        }

        @keyframes pulse {
            0%, 100% { transform: scale(1); }
            50% { transform: scale(1.05); }
        }

        @keyframes fadeIn {
            from { opacity: 0; }
            to { opacity: 1; }
        }

        .is-valid {
            border-color: #28a745 !important;
            animation: slideIn 0.3s ease-out;
        }

        .is-invalid {
            border-color: #dc3545 !important;
            animation: shake 0.3s ease-out;
        }

        .pulse {
            animation: pulse 2s infinite;
        }

        .fade-in {
            animation: fadeIn 0.5s ease-out;
        }
    `;
    document.head.appendChild(style);

    // Inicijalizacija nakon što je DOM učitan
    document.addEventListener('DOMContentLoaded', function () {
        animateActivityCards();
        animateUserCards();
        animateXPBar();
        animateFormValidation();
        animateButtons();
        animateNumbers();
        animateFadeOnScroll();
        animateSearchBar();
        animateTooltips();
    });
})();
