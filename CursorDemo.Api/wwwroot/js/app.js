// Application State
let currentPage = 1;
let pageSize = 10;
let currentSearch = '';
let currentSortBy = '';
let currentDesc = false;

// DOM Elements
const loginSection = document.getElementById('loginSection');
const mainContent = document.getElementById('mainContent');
const loginForm = document.getElementById('loginForm');
const loginError = document.getElementById('loginError');
const booksList = document.getElementById('booksList');
const pagination = document.getElementById('pagination');
const searchInput = document.getElementById('searchInput');
const sortBySelect = document.getElementById('sortBy');
const sortDescCheckbox = document.getElementById('sortDesc');
const searchBtn = document.getElementById('searchBtn');
const addBookBtn = document.getElementById('addBookBtn');
const addBookModal = document.getElementById('addBookModal');
const addBookForm = document.getElementById('addBookForm');
const cancelBtn = document.getElementById('cancelBtn');
const loading = document.getElementById('loading');
const formError = document.getElementById('formError');

// Check if user is already logged in
if (authToken) {
    showMainContent();
    loadBooks();
} else {
    showLogin();
}

// Login
loginForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    loginError.classList.remove('show');
    
    const username = document.getElementById('username').value;
    const password = document.getElementById('password').value;

    try {
        await api.login(username, password);
        showMainContent();
        loadBooks();
    } catch (error) {
        loginError.textContent = error.message || 'Login failed. Please check your credentials.';
        loginError.classList.add('show');
    }
});

// Search
searchBtn.addEventListener('click', () => {
    currentPage = 1;
    currentSearch = searchInput.value;
    currentSortBy = sortBySelect.value;
    currentDesc = sortDescCheckbox.checked;
    loadBooks();
});

searchInput.addEventListener('keypress', (e) => {
    if (e.key === 'Enter') {
        searchBtn.click();
    }
});

// Add Book
addBookBtn.addEventListener('click', () => {
    addBookModal.style.display = 'block';
    addBookForm.reset();
    formError.classList.remove('show');
});

cancelBtn.addEventListener('click', () => {
    addBookModal.style.display = 'none';
});

// Close modal when clicking outside
window.addEventListener('click', (e) => {
    if (e.target === addBookModal) {
        addBookModal.style.display = 'none';
    }
});

addBookForm.addEventListener('submit', async (e) => {
    e.preventDefault();
    formError.classList.remove('show');

    const book = {
        title: document.getElementById('bookTitle').value,
        author: document.getElementById('bookAuthor').value,
        isbn: document.getElementById('bookISBN').value,
        publishedDate: document.getElementById('bookPublishedDate').value
    };

    try {
        await api.createBook(book);
        addBookModal.style.display = 'none';
        currentPage = 1;
        loadBooks();
    } catch (error) {
        formError.textContent = error.message || 'Failed to add book. Please check the form.';
        formError.classList.add('show');
    }
});

// Load Books
async function loadBooks() {
    loading.style.display = 'block';
    booksList.innerHTML = '';

    try {
        const result = await api.getBooks(currentPage, pageSize, currentSearch, currentSortBy, currentDesc);
        
        if (result.items && result.items.length > 0) {
            displayBooks(result.items);
            displayPagination(result);
        } else {
            booksList.innerHTML = '<div class="empty-state"><h3>No books found</h3><p>Try adjusting your search criteria.</p></div>';
            pagination.innerHTML = '';
        }
    } catch (error) {
        booksList.innerHTML = `<div class="error-message show">Error loading books: ${error.message}</div>`;
        pagination.innerHTML = '';
    } finally {
        loading.style.display = 'none';
    }
}

// Display Books
function displayBooks(books) {
    booksList.innerHTML = books.map(book => `
        <div class="book-card">
            <h3>${escapeHtml(book.title)}</h3>
            <p><strong>Author:</strong> ${escapeHtml(book.author)}</p>
            <p><strong>ISBN:</strong> ${escapeHtml(book.isbn)}</p>
            <p><strong>Published:</strong> ${formatDate(book.publishedDate)}</p>
        </div>
    `).join('');
}

// Display Pagination
function displayPagination(result) {
    const totalPages = result.totalPages || 1;
    const page = result.page || 1;

    let html = '';

    // Previous button
    html += `<button ${page === 1 ? 'disabled' : ''} onclick="goToPage(${page - 1})">Previous</button>`;

    // Page info
    html += `<span class="page-info">Page ${page} of ${totalPages} (${result.totalCount} total)</span>`;

    // Next button
    html += `<button ${page === totalPages ? 'disabled' : ''} onclick="goToPage(${page + 1})">Next</button>`;

    pagination.innerHTML = html;
}

// Go to Page
function goToPage(page) {
    currentPage = page;
    loadBooks();
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

// Show/Hide Sections
function showLogin() {
    loginSection.style.display = 'block';
    mainContent.style.display = 'none';
}

function showMainContent() {
    loginSection.style.display = 'none';
    mainContent.style.display = 'block';
}

// Utility Functions
function escapeHtml(text) {
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}

function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('tr-TR', { 
        year: 'numeric', 
        month: 'long', 
        day: 'numeric' 
    });
}

// Make goToPage available globally
window.goToPage = goToPage;

