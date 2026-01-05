// Mess Management System - API & Utility Functions
// Global configuration and helper functions

const MessApp = {
    // API Base URL
    apiBaseUrl: '/api',
    
    // JWT Token Management
    token: {
        get: function() {
            return localStorage.getItem('jwt_token');
        },
        set: function(token) {
            localStorage.setItem('jwt_token', token);
        },
        remove: function() {
            localStorage.removeItem('jwt_token');
        },
        isValid: function() {
            const token = this.get();
            if (!token) return false;
            try {
                const payload = JSON.parse(atob(token.split('.')[1]));
                return payload.exp * 1000 > Date.now();
            } catch (e) {
                return false;
            }
        },
        getPayload: function() {
            const token = this.get();
            if (!token) return null;
            try {
                return JSON.parse(atob(token.split('.')[1]));
            } catch (e) {
                return null;
            }
        }
    },

    // HTTP Request Helper
    request: async function(url, options = {}) {
        const defaultOptions = {
            headers: {
                'Content-Type': 'application/json'
            }
        };

        // Add JWT token if available
        const token = this.token.get();
        if (token) {
            defaultOptions.headers['Authorization'] = `Bearer ${token}`;
        }

        const mergedOptions = {
            ...defaultOptions,
            ...options,
            headers: {
                ...defaultOptions.headers,
                ...options.headers
            }
        };

        try {
            const response = await fetch(url, mergedOptions);
            const data = await response.json().catch(() => null);

            if (!response.ok) {
                throw {
                    status: response.status,
                    message: data?.message || data?.error || 'An error occurred',
                    data: data
                };
            }

            return { success: true, data, status: response.status };
        } catch (error) {
            if (error.status) {
                return { success: false, error: error.message, status: error.status, data: error.data };
            }
            return { success: false, error: 'Network error. Please check your connection.', status: 0 };
        }
    },

    // API Methods
    api: {
        // Authentication
        login: async function(username, password, role = null) {
            const body = { username, password };
            if (role) body.role = role;
            
            const result = await MessApp.request(`${MessApp.apiBaseUrl}/AuthApi/login`, {
                method: 'POST',
                body: JSON.stringify(body)
            });

            if (result.success && result.data.token) {
                MessApp.token.set(result.data.token);
            }

            return result;
        },

        validateToken: async function() {
            return await MessApp.request(`${MessApp.apiBaseUrl}/AuthApi/validate`);
        },

        changePassword: async function(oldPassword, newPassword) {
            return await MessApp.request(`${MessApp.apiBaseUrl}/AuthApi/change-password`, {
                method: 'POST',
                body: JSON.stringify({ oldPassword, newPassword })
            });
        },

        // Teachers
        getTeachers: async function() {
            return await MessApp.request(`${MessApp.apiBaseUrl}/TeachersApi`);
        },

        getTeacher: async function(id) {
            return await MessApp.request(`${MessApp.apiBaseUrl}/TeachersApi/${id}`);
        },

        getMyProfile: async function() {
            return await MessApp.request(`${MessApp.apiBaseUrl}/TeachersApi/my-profile`);
        },

        createTeacher: async function(teacherData) {
            return await MessApp.request(`${MessApp.apiBaseUrl}/TeachersApi`, {
                method: 'POST',
                body: JSON.stringify(teacherData)
            });
        },

        // Attendance
        getAttendance: async function(date = null, teacherId = null) {
            let url = `${MessApp.apiBaseUrl}/AttendanceApi`;
            const params = new URLSearchParams();
            if (date) params.append('date', date);
            if (teacherId) params.append('teacherId', teacherId);
            if (params.toString()) url += `?${params.toString()}`;
            return await MessApp.request(url);
        },

        getMyAttendance: async function(startDate = null, endDate = null) {
            let url = `${MessApp.apiBaseUrl}/AttendanceApi/my-attendance`;
            const params = new URLSearchParams();
            if (startDate) params.append('startDate', startDate);
            if (endDate) params.append('endDate', endDate);
            if (params.toString()) url += `?${params.toString()}`;
            return await MessApp.request(url);
        },

        markAttendance: async function(attendanceData) {
            return await MessApp.request(`${MessApp.apiBaseUrl}/AttendanceApi`, {
                method: 'POST',
                body: JSON.stringify(attendanceData)
            });
        },

        updateAttendance: async function(id, attendanceData) {
            return await MessApp.request(`${MessApp.apiBaseUrl}/AttendanceApi/${id}`, {
                method: 'PUT',
                body: JSON.stringify(attendanceData)
            });
        },

        // Menu
        getMenu: async function(dayOfWeek = null, mealType = null) {
            let url = `${MessApp.apiBaseUrl}/MenuApi`;
            const params = new URLSearchParams();
            if (dayOfWeek) params.append('dayOfWeek', dayOfWeek);
            if (mealType) params.append('mealType', mealType);
            if (params.toString()) url += `?${params.toString()}`;
            return await MessApp.request(url);
        },

        getTodayMenu: async function() {
            return await MessApp.request(`${MessApp.apiBaseUrl}/MenuApi/today`);
        },

        createMenuItem: async function(menuData) {
            return await MessApp.request(`${MessApp.apiBaseUrl}/MenuApi`, {
                method: 'POST',
                body: JSON.stringify(menuData)
            });
        },

        updateMenuItem: async function(id, menuData) {
            return await MessApp.request(`${MessApp.apiBaseUrl}/MenuApi/${id}`, {
                method: 'PUT',
                body: JSON.stringify(menuData)
            });
        },

        deleteMenuItem: async function(id) {
            return await MessApp.request(`${MessApp.apiBaseUrl}/MenuApi/${id}`, {
                method: 'DELETE'
            });
        }
    },

    // Toast Notifications
    toast: {
        container: null,
        
        init: function() {
            if (!this.container) {
                this.container = document.createElement('div');
                this.container.id = 'toast-container';
                this.container.className = 'toast-container position-fixed top-0 end-0 p-3';
                this.container.style.zIndex = '9999';
                document.body.appendChild(this.container);
            }
        },

        show: function(message, type = 'info', duration = 5000) {
            this.init();
            
            const icons = {
                success: 'fa-check-circle',
                error: 'fa-times-circle',
                warning: 'fa-exclamation-triangle',
                info: 'fa-info-circle'
            };

            const bgColors = {
                success: 'bg-success',
                error: 'bg-danger',
                warning: 'bg-warning',
                info: 'bg-info'
            };

            const toastId = 'toast-' + Date.now();
            const toastHtml = `
                <div id="${toastId}" class="toast align-items-center text-white ${bgColors[type]} border-0" role="alert">
                    <div class="d-flex">
                        <div class="toast-body">
                            <i class="fas ${icons[type]} me-2"></i>
                            ${message}
                        </div>
                        <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
                    </div>
                </div>
            `;

            this.container.insertAdjacentHTML('beforeend', toastHtml);
            const toastElement = document.getElementById(toastId);
            const toast = new bootstrap.Toast(toastElement, { delay: duration });
            toast.show();

            toastElement.addEventListener('hidden.bs.toast', () => {
                toastElement.remove();
            });
        },

        success: function(message, duration) {
            this.show(message, 'success', duration);
        },

        error: function(message, duration) {
            this.show(message, 'error', duration);
        },

        warning: function(message, duration) {
            this.show(message, 'warning', duration);
        },

        info: function(message, duration) {
            this.show(message, 'info', duration);
        }
    },

    // Form Validation
    validation: {
        rules: {
            required: (value) => value && value.toString().trim() !== '',
            email: (value) => !value || /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value),
            minLength: (value, min) => !value || value.length >= min,
            maxLength: (value, max) => !value || value.length <= max,
            phone: (value) => !value || /^[\d\s\-+()]{10,}$/.test(value),
            match: (value, matchValue) => value === matchValue,
            numeric: (value) => !value || /^\d+$/.test(value),
            decimal: (value) => !value || /^\d+(\.\d{1,2})?$/.test(value)
        },

        messages: {
            required: 'This field is required',
            email: 'Please enter a valid email address',
            minLength: 'Minimum length is {min} characters',
            maxLength: 'Maximum length is {max} characters',
            phone: 'Please enter a valid phone number',
            match: 'Fields do not match',
            numeric: 'Please enter a valid number',
            decimal: 'Please enter a valid decimal number'
        },

        validateField: function(field, rules) {
            const value = field.value;
            const errors = [];

            for (const [rule, param] of Object.entries(rules)) {
                if (rule === 'required' && param && !this.rules.required(value)) {
                    errors.push(this.messages.required);
                } else if (rule === 'email' && param && !this.rules.email(value)) {
                    errors.push(this.messages.email);
                } else if (rule === 'minLength' && !this.rules.minLength(value, param)) {
                    errors.push(this.messages.minLength.replace('{min}', param));
                } else if (rule === 'maxLength' && !this.rules.maxLength(value, param)) {
                    errors.push(this.messages.maxLength.replace('{max}', param));
                } else if (rule === 'phone' && param && !this.rules.phone(value)) {
                    errors.push(this.messages.phone);
                } else if (rule === 'numeric' && param && !this.rules.numeric(value)) {
                    errors.push(this.messages.numeric);
                } else if (rule === 'decimal' && param && !this.rules.decimal(value)) {
                    errors.push(this.messages.decimal);
                }
            }

            return errors;
        },

        showError: function(field, message) {
            this.clearError(field);
            field.classList.add('is-invalid');
            const feedback = document.createElement('div');
            feedback.className = 'invalid-feedback';
            feedback.textContent = message;
            field.parentNode.appendChild(feedback);
        },

        clearError: function(field) {
            field.classList.remove('is-invalid');
            field.classList.remove('is-valid');
            const feedback = field.parentNode.querySelector('.invalid-feedback');
            if (feedback) feedback.remove();
        },

        showSuccess: function(field) {
            this.clearError(field);
            field.classList.add('is-valid');
        }
    },

    // Loading Indicator
    loading: {
        overlay: null,

        show: function(message = 'Loading...') {
            if (!this.overlay) {
                this.overlay = document.createElement('div');
                this.overlay.id = 'loading-overlay';
                this.overlay.innerHTML = `
                    <div class="loading-content">
                        <div class="spinner-border text-primary" role="status">
                            <span class="visually-hidden">Loading...</span>
                        </div>
                        <p class="loading-message mt-3">${message}</p>
                    </div>
                `;
                document.body.appendChild(this.overlay);
            } else {
                this.overlay.querySelector('.loading-message').textContent = message;
                this.overlay.style.display = 'flex';
            }
        },

        hide: function() {
            if (this.overlay) {
                this.overlay.style.display = 'none';
            }
        }
    },

    // Confirmation Dialog
    confirm: function(message, title = 'Confirm') {
        return new Promise((resolve) => {
            const modalId = 'confirm-modal-' + Date.now();
            const modalHtml = `
                <div class="modal fade" id="${modalId}" tabindex="-1">
                    <div class="modal-dialog modal-dialog-centered">
                        <div class="modal-content">
                            <div class="modal-header">
                                <h5 class="modal-title"><i class="fas fa-question-circle text-warning"></i> ${title}</h5>
                                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
                            </div>
                            <div class="modal-body">
                                <p>${message}</p>
                            </div>
                            <div class="modal-footer">
                                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                                <button type="button" class="btn btn-primary" id="${modalId}-confirm">Confirm</button>
                            </div>
                        </div>
                    </div>
                </div>
            `;

            document.body.insertAdjacentHTML('beforeend', modalHtml);
            const modalElement = document.getElementById(modalId);
            const modal = new bootstrap.Modal(modalElement);

            document.getElementById(`${modalId}-confirm`).addEventListener('click', () => {
                modal.hide();
                resolve(true);
            });

            modalElement.addEventListener('hidden.bs.modal', () => {
                modalElement.remove();
                resolve(false);
            });

            modal.show();
        });
    },

    // Form Helpers
    form: {
        serialize: function(form) {
            const formData = new FormData(form);
            const data = {};
            formData.forEach((value, key) => {
                if (data[key]) {
                    if (!Array.isArray(data[key])) {
                        data[key] = [data[key]];
                    }
                    data[key].push(value);
                } else {
                    data[key] = value;
                }
            });
            return data;
        },

        populate: function(form, data) {
            for (const [key, value] of Object.entries(data)) {
                const field = form.querySelector(`[name="${key}"]`);
                if (field) {
                    if (field.type === 'checkbox') {
                        field.checked = value;
                    } else if (field.type === 'radio') {
                        const radio = form.querySelector(`[name="${key}"][value="${value}"]`);
                        if (radio) radio.checked = true;
                    } else {
                        field.value = value;
                    }
                }
            }
        },

        reset: function(form) {
            form.reset();
            form.querySelectorAll('.is-invalid, .is-valid').forEach(field => {
                field.classList.remove('is-invalid', 'is-valid');
            });
            form.querySelectorAll('.invalid-feedback').forEach(el => el.remove());
        },

        disable: function(form) {
            form.querySelectorAll('input, select, textarea, button').forEach(el => {
                el.disabled = true;
            });
        },

        enable: function(form) {
            form.querySelectorAll('input, select, textarea, button').forEach(el => {
                el.disabled = false;
            });
        }
    },

    // Date Formatting
    formatDate: function(date, format = 'YYYY-MM-DD') {
        const d = new Date(date);
        const year = d.getFullYear();
        const month = String(d.getMonth() + 1).padStart(2, '0');
        const day = String(d.getDate()).padStart(2, '0');
        const hours = String(d.getHours()).padStart(2, '0');
        const minutes = String(d.getMinutes()).padStart(2, '0');

        return format
            .replace('YYYY', year)
            .replace('MM', month)
            .replace('DD', day)
            .replace('HH', hours)
            .replace('mm', minutes);
    },

    // Currency Formatting
    formatCurrency: function(amount, currency = 'PKR') {
        return new Intl.NumberFormat('en-PK', {
            style: 'currency',
            currency: currency
        }).format(amount);
    }
};

// Initialize on DOM ready
document.addEventListener('DOMContentLoaded', function() {
    MessApp.toast.init();
});

// Global shortcut functions for easier access
MessApp.showToast = function(message, type = 'info', duration = 5000) {
    this.toast.show(message, type, duration);
};

MessApp.showLoading = function(message) {
    this.loading.show(message);
};

MessApp.hideLoading = function() {
    this.loading.hide();
};

// Export for use in modules
if (typeof module !== 'undefined' && module.exports) {
    module.exports = MessApp;
}
