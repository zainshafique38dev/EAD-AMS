/**
 * ==========================================================================
 * HCI/UX ENHANCED JAVASCRIPT - Mess Management System
 * Implementing Usability Heuristics & Accessibility Best Practices
 * ==========================================================================
 */

(function() {
    'use strict';

    // ==========================================================================
    // 1. TOAST NOTIFICATION SYSTEM
    // ==========================================================================
    window.ToastManager = {
        container: null,

        init: function() {
            if (!this.container) {
                this.container = document.createElement('div');
                this.container.className = 'toast-container-enhanced';
                this.container.setAttribute('aria-live', 'polite');
                this.container.setAttribute('aria-atomic', 'true');
                this.container.setAttribute('role', 'region');
                this.container.setAttribute('aria-label', 'Notifications');
                document.body.appendChild(this.container);
            }
        },

        show: function(options) {
            this.init();

            const defaults = {
                type: 'info', // success, warning, danger, info
                title: '',
                message: '',
                duration: 5000,
                dismissible: true,
                icon: null
            };

            const settings = { ...defaults, ...options };

            const icons = {
                success: '<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"></path><polyline points="22 4 12 14.01 9 11.01"></polyline></svg>',
                warning: '<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path><line x1="12" y1="9" x2="12" y2="13"></line><line x1="12" y1="17" x2="12.01" y2="17"></line></svg>',
                danger: '<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><line x1="15" y1="9" x2="9" y2="15"></line><line x1="9" y1="9" x2="15" y2="15"></line></svg>',
                info: '<svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><circle cx="12" cy="12" r="10"></circle><line x1="12" y1="16" x2="12" y2="12"></line><line x1="12" y1="8" x2="12.01" y2="8"></line></svg>'
            };

            const toast = document.createElement('div');
            toast.className = `toast-enhanced alert-${settings.type}-enhanced`;
            toast.setAttribute('role', 'alert');
            toast.setAttribute('aria-live', settings.type === 'danger' ? 'assertive' : 'polite');

            toast.innerHTML = `
                <div class="d-flex align-items-start gap-3">
                    <div class="toast-icon" style="color: var(--color-${settings.type})">
                        ${settings.icon || icons[settings.type]}
                    </div>
                    <div class="toast-content flex-grow-1">
                        ${settings.title ? `<div class="toast-title fw-semibold">${settings.title}</div>` : ''}
                        <div class="toast-message">${settings.message}</div>
                    </div>
                    ${settings.dismissible ? `
                        <button type="button" class="btn-close toast-dismiss" aria-label="Dismiss notification"></button>
                    ` : ''}
                </div>
            `;

            this.container.appendChild(toast);

            // Dismiss handler
            const dismissBtn = toast.querySelector('.toast-dismiss');
            if (dismissBtn) {
                dismissBtn.addEventListener('click', () => this.dismiss(toast));
            }

            // Auto dismiss
            if (settings.duration > 0) {
                setTimeout(() => this.dismiss(toast), settings.duration);
            }

            return toast;
        },

        dismiss: function(toast) {
            toast.classList.add('toast-exit');
            setTimeout(() => {
                if (toast.parentNode) {
                    toast.parentNode.removeChild(toast);
                }
            }, 300);
        },

        success: function(message, title = 'Success') {
            return this.show({ type: 'success', title, message });
        },

        warning: function(message, title = 'Warning') {
            return this.show({ type: 'warning', title, message });
        },

        error: function(message, title = 'Error') {
            return this.show({ type: 'danger', title, message });
        },

        info: function(message, title = 'Information') {
            return this.show({ type: 'info', title, message });
        }
    };

    // ==========================================================================
    // 2. CONFIRMATION DIALOG SYSTEM
    // ==========================================================================
    window.ConfirmDialog = {
        show: function(options) {
            const defaults = {
                title: 'Confirm Action',
                message: 'Are you sure you want to proceed?',
                confirmText: 'Confirm',
                cancelText: 'Cancel',
                confirmClass: 'btn-danger-enhanced',
                icon: 'warning',
                onConfirm: null,
                onCancel: null
            };

            const settings = { ...defaults, ...options };

            return new Promise((resolve) => {
                // Create backdrop
                const backdrop = document.createElement('div');
                backdrop.className = 'modal-backdrop-enhanced';
                backdrop.setAttribute('aria-hidden', 'true');

                // Create modal
                const modal = document.createElement('div');
                modal.className = 'modal-enhanced';
                modal.setAttribute('role', 'alertdialog');
                modal.setAttribute('aria-modal', 'true');
                modal.setAttribute('aria-labelledby', 'confirm-dialog-title');
                modal.setAttribute('aria-describedby', 'confirm-dialog-desc');

                const iconSvg = {
                    warning: '<svg class="text-warning" xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"></path><line x1="12" y1="9" x2="12" y2="13"></line><line x1="12" y1="17" x2="12.01" y2="17"></line></svg>',
                    danger: '<svg class="text-danger" xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"></circle><line x1="15" y1="9" x2="9" y2="15"></line><line x1="9" y1="9" x2="15" y2="15"></line></svg>',
                    info: '<svg class="text-info" xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"></circle><line x1="12" y1="16" x2="12" y2="12"></line><line x1="12" y1="8" x2="12.01" y2="8"></line></svg>',
                    question: '<svg class="text-primary" xmlns="http://www.w3.org/2000/svg" width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"></circle><path d="M9.09 9a3 3 0 0 1 5.83 1c0 2-3 3-3 3"></path><line x1="12" y1="17" x2="12.01" y2="17"></line></svg>'
                };

                modal.innerHTML = `
                    <div class="modal-body-enhanced text-center py-4">
                        <div class="mb-4">${iconSvg[settings.icon] || iconSvg.warning}</div>
                        <h3 id="confirm-dialog-title" class="h4 mb-3">${settings.title}</h3>
                        <p id="confirm-dialog-desc" class="text-muted mb-0">${settings.message}</p>
                    </div>
                    <div class="modal-footer-enhanced justify-content-center">
                        <button type="button" class="btn-enhanced btn-outline-enhanced cancel-btn">
                            ${settings.cancelText}
                        </button>
                        <button type="button" class="btn-enhanced ${settings.confirmClass} confirm-btn">
                            ${settings.confirmText}
                        </button>
                    </div>
                `;

                document.body.appendChild(backdrop);
                document.body.appendChild(modal);

                // Focus management
                const confirmBtn = modal.querySelector('.confirm-btn');
                const cancelBtn = modal.querySelector('.cancel-btn');
                confirmBtn.focus();

                // Trap focus
                modal.addEventListener('keydown', (e) => {
                    if (e.key === 'Tab') {
                        if (e.shiftKey && document.activeElement === cancelBtn) {
                            e.preventDefault();
                            confirmBtn.focus();
                        } else if (!e.shiftKey && document.activeElement === confirmBtn) {
                            e.preventDefault();
                            cancelBtn.focus();
                        }
                    }
                    if (e.key === 'Escape') {
                        cleanup(false);
                    }
                });

                const cleanup = (result) => {
                    backdrop.remove();
                    modal.remove();
                    document.body.style.overflow = '';
                    resolve(result);
                    if (result && settings.onConfirm) settings.onConfirm();
                    if (!result && settings.onCancel) settings.onCancel();
                };

                confirmBtn.addEventListener('click', () => cleanup(true));
                cancelBtn.addEventListener('click', () => cleanup(false));
                backdrop.addEventListener('click', () => cleanup(false));

                document.body.style.overflow = 'hidden';
            });
        },

        delete: function(itemName) {
            return this.show({
                title: 'Delete Confirmation',
                message: `Are you sure you want to delete "${itemName}"? This action cannot be undone.`,
                confirmText: 'Delete',
                cancelText: 'Cancel',
                confirmClass: 'btn-danger-enhanced',
                icon: 'danger'
            });
        }
    };

    // ==========================================================================
    // 3. FORM VALIDATION ENHANCEMENT
    // ==========================================================================
    window.FormValidator = {
        init: function(form) {
            if (!form) return;

            const inputs = form.querySelectorAll('input, select, textarea');
            
            inputs.forEach(input => {
                // Real-time validation
                input.addEventListener('blur', () => this.validateField(input));
                input.addEventListener('input', () => {
                    if (input.classList.contains('is-invalid')) {
                        this.validateField(input);
                    }
                });
            });

            // Form submission
            form.addEventListener('submit', (e) => {
                let isValid = true;
                inputs.forEach(input => {
                    if (!this.validateField(input)) {
                        isValid = false;
                    }
                });

                if (!isValid) {
                    e.preventDefault();
                    // Focus first invalid field
                    const firstInvalid = form.querySelector('.is-invalid');
                    if (firstInvalid) {
                        firstInvalid.focus();
                        // Announce error for screen readers
                        this.announceError('Please correct the errors in the form before submitting.');
                    }
                }
            });
        },

        validateField: function(input) {
            const rules = {
                required: (value) => value.trim() !== '',
                email: (value) => !value || /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value),
                minLength: (value, len) => !value || value.length >= parseInt(len),
                maxLength: (value, len) => !value || value.length <= parseInt(len),
                pattern: (value, pattern) => !value || new RegExp(pattern).test(value),
                phone: (value) => !value || /^[\d\s\-+()]{10,}$/.test(value)
            };

            const messages = {
                required: 'This field is required',
                email: 'Please enter a valid email address',
                minLength: (len) => `Minimum ${len} characters required`,
                maxLength: (len) => `Maximum ${len} characters allowed`,
                pattern: 'Please match the required format',
                phone: 'Please enter a valid phone number'
            };

            let isValid = true;
            let errorMessage = '';

            // Check required
            if (input.hasAttribute('required') && !rules.required(input.value)) {
                isValid = false;
                errorMessage = messages.required;
            }

            // Check email
            if (isValid && input.type === 'email' && !rules.email(input.value)) {
                isValid = false;
                errorMessage = messages.email;
            }

            // Check minlength
            if (isValid && input.hasAttribute('minlength')) {
                const minLen = input.getAttribute('minlength');
                if (!rules.minLength(input.value, minLen)) {
                    isValid = false;
                    errorMessage = messages.minLength(minLen);
                }
            }

            // Check maxlength
            if (isValid && input.hasAttribute('maxlength')) {
                const maxLen = input.getAttribute('maxlength');
                if (!rules.maxLength(input.value, maxLen)) {
                    isValid = false;
                    errorMessage = messages.maxLength(maxLen);
                }
            }

            // Check pattern
            if (isValid && input.hasAttribute('pattern')) {
                if (!rules.pattern(input.value, input.getAttribute('pattern'))) {
                    isValid = false;
                    errorMessage = input.getAttribute('data-error-message') || messages.pattern;
                }
            }

            // Check phone type
            if (isValid && input.type === 'tel' && input.value && !rules.phone(input.value)) {
                isValid = false;
                errorMessage = messages.phone;
            }

            this.setFieldState(input, isValid, errorMessage);
            return isValid;
        },

        setFieldState: function(input, isValid, errorMessage) {
            const container = input.closest('.form-group-enhanced') || input.parentElement;
            let feedback = container.querySelector('.form-feedback');

            if (!feedback) {
                feedback = document.createElement('div');
                feedback.className = 'form-feedback';
                input.parentNode.insertBefore(feedback, input.nextSibling);
            }

            input.classList.remove('is-valid', 'is-invalid');
            feedback.classList.remove('valid', 'invalid');

            if (input.value) {
                if (isValid) {
                    input.classList.add('is-valid');
                    feedback.classList.add('valid');
                    feedback.textContent = '';
                    input.setAttribute('aria-invalid', 'false');
                } else {
                    input.classList.add('is-invalid');
                    feedback.classList.add('invalid');
                    feedback.textContent = errorMessage;
                    input.setAttribute('aria-invalid', 'true');
                    input.setAttribute('aria-describedby', feedback.id || '');
                }
            }
        },

        announceError: function(message) {
            const announcer = document.createElement('div');
            announcer.setAttribute('role', 'alert');
            announcer.setAttribute('aria-live', 'assertive');
            announcer.className = 'sr-only';
            announcer.textContent = message;
            document.body.appendChild(announcer);
            setTimeout(() => announcer.remove(), 1000);
        }
    };

    // ==========================================================================
    // 4. LOADING STATE MANAGER
    // ==========================================================================
    window.LoadingManager = {
        showButton: function(button, text = 'Loading...') {
            button.disabled = true;
            button.dataset.originalText = button.innerHTML;
            button.classList.add('btn-loading');
            button.innerHTML = `<span class="visually-hidden">${text}</span>`;
        },

        hideButton: function(button) {
            button.disabled = false;
            button.classList.remove('btn-loading');
            if (button.dataset.originalText) {
                button.innerHTML = button.dataset.originalText;
            }
        },

        showOverlay: function(container = document.body, message = 'Loading...') {
            const overlay = document.createElement('div');
            overlay.className = 'loading-overlay';
            overlay.setAttribute('role', 'progressbar');
            overlay.setAttribute('aria-label', message);
            overlay.innerHTML = `
                <div class="loading-content text-center">
                    <div class="loading-spinner lg mb-3"></div>
                    <p class="mb-0">${message}</p>
                </div>
            `;
            container.style.position = 'relative';
            container.appendChild(overlay);
            return overlay;
        },

        hideOverlay: function(overlay) {
            if (overlay && overlay.parentNode) {
                overlay.parentNode.removeChild(overlay);
            }
        },

        showSkeleton: function(container, rows = 5) {
            const skeleton = document.createElement('div');
            skeleton.className = 'skeleton-container';
            
            for (let i = 0; i < rows; i++) {
                skeleton.innerHTML += `
                    <div class="d-flex align-items-center gap-3 mb-3">
                        <div class="skeleton skeleton-avatar"></div>
                        <div class="flex-grow-1">
                            <div class="skeleton skeleton-text" style="width: ${70 + Math.random() * 30}%"></div>
                            <div class="skeleton skeleton-text" style="width: ${40 + Math.random() * 30}%"></div>
                        </div>
                    </div>
                `;
            }
            
            container.appendChild(skeleton);
            return skeleton;
        }
    };

    // ==========================================================================
    // 5. PROGRESS TRACKER
    // ==========================================================================
    window.ProgressTracker = {
        create: function(container, steps) {
            const tracker = document.createElement('div');
            tracker.className = 'progress-tracker';
            tracker.setAttribute('role', 'progressbar');
            tracker.setAttribute('aria-valuemin', '0');
            tracker.setAttribute('aria-valuemax', steps.length);
            tracker.setAttribute('aria-valuenow', '0');

            tracker.innerHTML = `
                <ol class="onboarding-steps" role="list">
                    ${steps.map((step, index) => `
                        <li class="onboarding-step ${index === 0 ? 'active' : ''}" data-step="${index}">
                            <span class="onboarding-step-number">${index + 1}</span>
                            <span class="onboarding-step-title">${step}</span>
                        </li>
                    `).join('')}
                </ol>
            `;

            container.appendChild(tracker);
            return tracker;
        },

        setStep: function(tracker, stepIndex) {
            const steps = tracker.querySelectorAll('.onboarding-step');
            steps.forEach((step, index) => {
                step.classList.remove('active', 'completed');
                if (index < stepIndex) {
                    step.classList.add('completed');
                } else if (index === stepIndex) {
                    step.classList.add('active');
                }
            });
            tracker.setAttribute('aria-valuenow', stepIndex);
        }
    };

    // ==========================================================================
    // 6. KEYBOARD NAVIGATION ENHANCEMENT
    // ==========================================================================
    window.KeyboardNav = {
        init: function() {
            // ESC to close modals, dropdowns
            document.addEventListener('keydown', (e) => {
                if (e.key === 'Escape') {
                    // Close any open modals
                    const modal = document.querySelector('.modal-enhanced');
                    if (modal) {
                        const backdrop = document.querySelector('.modal-backdrop-enhanced');
                        modal.remove();
                        if (backdrop) backdrop.remove();
                    }

                    // Close dropdowns
                    document.querySelectorAll('.dropdown-menu.show').forEach(menu => {
                        menu.classList.remove('show');
                    });
                }
            });

            // Arrow key navigation for tables
            document.querySelectorAll('.table-enhanced').forEach(table => {
                table.addEventListener('keydown', (e) => {
                    if (['ArrowUp', 'ArrowDown'].includes(e.key)) {
                        const rows = Array.from(table.querySelectorAll('tbody tr'));
                        const currentRow = e.target.closest('tr');
                        const currentIndex = rows.indexOf(currentRow);

                        let nextIndex;
                        if (e.key === 'ArrowDown') {
                            nextIndex = Math.min(currentIndex + 1, rows.length - 1);
                        } else {
                            nextIndex = Math.max(currentIndex - 1, 0);
                        }

                        const nextRow = rows[nextIndex];
                        const focusable = nextRow.querySelector('button, a, input, [tabindex]');
                        if (focusable) {
                            e.preventDefault();
                            focusable.focus();
                        }
                    }
                });
            });
        }
    };

    // ==========================================================================
    // 7. ACCESSIBILITY ANNOUNCER
    // ==========================================================================
    window.A11yAnnouncer = {
        container: null,

        init: function() {
            if (!this.container) {
                this.container = document.createElement('div');
                this.container.className = 'sr-only';
                this.container.setAttribute('aria-live', 'polite');
                this.container.setAttribute('aria-atomic', 'true');
                this.container.setAttribute('role', 'status');
                document.body.appendChild(this.container);
            }
        },

        announce: function(message, priority = 'polite') {
            this.init();
            this.container.setAttribute('aria-live', priority);
            this.container.textContent = '';
            setTimeout(() => {
                this.container.textContent = message;
            }, 100);
        },

        assertive: function(message) {
            this.announce(message, 'assertive');
        }
    };

    // ==========================================================================
    // 8. DATA TABLE ENHANCEMENT
    // ==========================================================================
    window.DataTableEnhancer = {
        init: function(table) {
            if (!table) return;

            // Add sorting functionality
            const headers = table.querySelectorAll('th.sortable');
            headers.forEach(header => {
                header.addEventListener('click', () => {
                    this.sortTable(table, header);
                });

                header.addEventListener('keypress', (e) => {
                    if (e.key === 'Enter' || e.key === ' ') {
                        e.preventDefault();
                        this.sortTable(table, header);
                    }
                });

                header.setAttribute('tabindex', '0');
                header.setAttribute('role', 'button');
                header.setAttribute('aria-sort', 'none');
            });

            // Add search functionality if search input exists
            const searchInput = table.closest('.card')?.querySelector('.table-search');
            if (searchInput) {
                searchInput.addEventListener('input', (e) => {
                    this.filterTable(table, e.target.value);
                });
            }
        },

        sortTable: function(table, header) {
            const tbody = table.querySelector('tbody');
            const rows = Array.from(tbody.querySelectorAll('tr'));
            const columnIndex = Array.from(header.parentNode.children).indexOf(header);
            const currentOrder = header.classList.contains('asc') ? 'desc' : 'asc';

            // Reset other headers
            table.querySelectorAll('th.sortable').forEach(th => {
                th.classList.remove('asc', 'desc');
                th.setAttribute('aria-sort', 'none');
            });

            header.classList.add(currentOrder);
            header.setAttribute('aria-sort', currentOrder === 'asc' ? 'ascending' : 'descending');

            rows.sort((a, b) => {
                const aValue = a.children[columnIndex]?.textContent.trim() || '';
                const bValue = b.children[columnIndex]?.textContent.trim() || '';

                // Try numeric comparison first
                const aNum = parseFloat(aValue.replace(/[^0-9.-]/g, ''));
                const bNum = parseFloat(bValue.replace(/[^0-9.-]/g, ''));

                if (!isNaN(aNum) && !isNaN(bNum)) {
                    return currentOrder === 'asc' ? aNum - bNum : bNum - aNum;
                }

                // Fall back to string comparison
                return currentOrder === 'asc' 
                    ? aValue.localeCompare(bValue)
                    : bValue.localeCompare(aValue);
            });

            rows.forEach(row => tbody.appendChild(row));

            A11yAnnouncer.announce(`Table sorted by ${header.textContent} in ${currentOrder === 'asc' ? 'ascending' : 'descending'} order`);
        },

        filterTable: function(table, query) {
            const tbody = table.querySelector('tbody');
            const rows = tbody.querySelectorAll('tr');
            const lowerQuery = query.toLowerCase();
            let visibleCount = 0;

            rows.forEach(row => {
                const text = row.textContent.toLowerCase();
                const isVisible = text.includes(lowerQuery);
                row.style.display = isVisible ? '' : 'none';
                if (isVisible) visibleCount++;
            });

            A11yAnnouncer.announce(`${visibleCount} results found`);
        }
    };

    // ==========================================================================
    // 9. AUTO-SAVE INDICATOR
    // ==========================================================================
    window.AutoSaveIndicator = {
        show: function(status = 'saving') {
            let indicator = document.querySelector('.autosave-indicator');
            
            if (!indicator) {
                indicator = document.createElement('div');
                indicator.className = 'autosave-indicator';
                indicator.style.cssText = `
                    position: fixed;
                    bottom: 20px;
                    right: 20px;
                    padding: 8px 16px;
                    background: var(--color-bg-primary);
                    border: 1px solid var(--color-border);
                    border-radius: var(--radius-md);
                    box-shadow: var(--shadow-md);
                    font-size: var(--font-size-sm);
                    z-index: 1000;
                    display: flex;
                    align-items: center;
                    gap: 8px;
                `;
                document.body.appendChild(indicator);
            }

            const states = {
                saving: { icon: '<div class="loading-spinner sm"></div>', text: 'Saving...', color: 'var(--color-info)' },
                saved: { icon: '✓', text: 'All changes saved', color: 'var(--color-success)' },
                error: { icon: '✗', text: 'Failed to save', color: 'var(--color-danger)' }
            };

            const state = states[status];
            indicator.innerHTML = `
                <span style="color: ${state.color}">${state.icon}</span>
                <span>${state.text}</span>
            `;
            indicator.setAttribute('role', 'status');
            indicator.setAttribute('aria-live', 'polite');

            if (status === 'saved') {
                setTimeout(() => {
                    indicator.style.opacity = '0';
                    setTimeout(() => indicator.remove(), 300);
                }, 2000);
            }
        }
    };

    // ==========================================================================
    // 10. MEAL ATTENDANCE HELPERS
    // ==========================================================================
    window.MealAttendance = {
        markMeal: async function(teacherId, date, mealType, taken) {
            LoadingManager.showButton(event.target);
            
            try {
                // Simulate API call - replace with actual endpoint
                await new Promise(resolve => setTimeout(resolve, 500));
                
                ToastManager.success(
                    `${mealType} ${taken ? 'marked' : 'unmarked'} successfully`,
                    'Attendance Updated'
                );
                
                A11yAnnouncer.announce(`${mealType} attendance ${taken ? 'marked' : 'unmarked'}`);
                
                return true;
            } catch (error) {
                ToastManager.error('Failed to update attendance. Please try again.');
                return false;
            } finally {
                LoadingManager.hideButton(event.target);
            }
        },

        confirmBulkAction: async function(action, count) {
            return await ConfirmDialog.show({
                title: `Confirm ${action}`,
                message: `This will ${action.toLowerCase()} attendance for ${count} teachers. Continue?`,
                confirmText: action,
                icon: 'question'
            });
        }
    };

    // ==========================================================================
    // 11. HELP SYSTEM
    // ==========================================================================
    window.HelpSystem = {
        showTip: function(element, message) {
            const tip = document.createElement('div');
            tip.className = 'help-tip-popup';
            tip.setAttribute('role', 'tooltip');
            tip.innerHTML = message;
            
            const rect = element.getBoundingClientRect();
            tip.style.cssText = `
                position: fixed;
                top: ${rect.bottom + 8}px;
                left: ${rect.left}px;
                max-width: 300px;
                padding: 12px 16px;
                background: var(--color-text-primary);
                color: white;
                font-size: var(--font-size-sm);
                border-radius: var(--radius-md);
                box-shadow: var(--shadow-lg);
                z-index: 10000;
                animation: fade-in 0.2s ease;
            `;
            
            document.body.appendChild(tip);
            
            const dismiss = () => {
                tip.remove();
                element.removeEventListener('blur', dismiss);
                document.removeEventListener('click', outsideClick);
            };
            
            const outsideClick = (e) => {
                if (!tip.contains(e.target) && e.target !== element) {
                    dismiss();
                }
            };
            
            element.addEventListener('blur', dismiss);
            setTimeout(() => document.addEventListener('click', outsideClick), 100);
        },

        initTooltips: function() {
            document.querySelectorAll('[data-help]').forEach(element => {
                element.addEventListener('click', (e) => {
                    e.preventDefault();
                    this.showTip(element, element.dataset.help);
                });
                
                element.addEventListener('keypress', (e) => {
                    if (e.key === 'Enter' || e.key === ' ') {
                        e.preventDefault();
                        this.showTip(element, element.dataset.help);
                    }
                });
            });
        }
    };

    // ==========================================================================
    // INITIALIZATION
    // ==========================================================================
    document.addEventListener('DOMContentLoaded', function() {
        // Initialize keyboard navigation
        KeyboardNav.init();

        // Initialize form validation on all forms with validation class
        document.querySelectorAll('form.needs-validation, form[data-validate]').forEach(form => {
            FormValidator.init(form);
        });

        // Initialize data tables
        document.querySelectorAll('.table-enhanced').forEach(table => {
            DataTableEnhancer.init(table);
        });

        // Initialize help tooltips
        HelpSystem.initTooltips();

        // Handle skip link
        const skipLink = document.querySelector('.skip-to-main');
        if (skipLink) {
            skipLink.addEventListener('click', (e) => {
                e.preventDefault();
                const main = document.getElementById('main-content');
                if (main) {
                    main.setAttribute('tabindex', '-1');
                    main.focus();
                    main.removeAttribute('tabindex');
                }
            });
        }

        // Announce page load for screen readers
        setTimeout(() => {
            const title = document.querySelector('h1, [role="heading"]');
            if (title) {
                A11yAnnouncer.announce(`Page loaded: ${title.textContent}`);
            }
        }, 500);

        console.log('HCI Enhancements initialized successfully');
    });

})();
