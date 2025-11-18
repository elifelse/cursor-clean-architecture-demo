// API Configuration
const API_BASE_URL = window.location.origin;
const API_VERSION = 'v1';

// Token storage
let authToken = localStorage.getItem('authToken');

// API Helper Functions
const api = {
    async request(endpoint, options = {}) {
        const url = `${API_BASE_URL}/api/${API_VERSION}${endpoint}`;
        
        const config = {
            ...options,
            headers: {
                'Content-Type': 'application/json',
                ...options.headers,
            }
        };

        // Add auth token if available
        if (authToken) {
            config.headers['Authorization'] = `Bearer ${authToken}`;
        }

        try {
            const response = await fetch(url, config);
            
            if (!response.ok) {
                const error = await response.json().catch(() => ({ message: 'An error occurred' }));
                throw new Error(error.message || `HTTP error! status: ${response.status}`);
            }

            return await response.json();
        } catch (error) {
            console.error('API Error:', error);
            throw error;
        }
    },

    // Auth
    async login(username, password) {
        const response = await this.request('/auth/login', {
            method: 'POST',
            body: JSON.stringify({ username, password })
        });
        
        authToken = response.token;
        localStorage.setItem('authToken', authToken);
        
        return response;
    },

    // Books
    async getBooks(page = 1, pageSize = 10, search = '', sortBy = '', desc = false) {
        const params = new URLSearchParams({
            page: page.toString(),
            pageSize: pageSize.toString(),
        });

        if (search) params.append('search', search);
        if (sortBy) params.append('sortBy', sortBy);
        if (desc) params.append('desc', 'true');

        return await this.request(`/books?${params.toString()}`);
    },

    async getBookById(id) {
        return await this.request(`/books/${id}`);
    },

    async createBook(book) {
        return await this.request('/books', {
            method: 'POST',
            body: JSON.stringify(book)
        });
    }
};

