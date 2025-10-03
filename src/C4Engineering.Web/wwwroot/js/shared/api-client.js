/**
 * API Client for C4Engineering Platform
 * Provides centralized HTTP communication with REST endpoints
 */
export class ApiClient {
    constructor(baseUrl = '') {
        this.baseUrl = baseUrl;
        this.defaultHeaders = {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
        };
    }

    /**
     * GET request
     */
    async get(url, params = {}) {
        const queryString = new URLSearchParams(params).toString();
        const fullUrl = `${this.baseUrl}${url}${queryString ? '?' + queryString : ''}`;
        
        const response = await fetch(fullUrl, {
            method: 'GET',
            headers: this.defaultHeaders
        });

        return this._handleResponse(response);
    }

    /**
     * POST request
     */
    async post(url, data = {}) {
        const response = await fetch(`${this.baseUrl}${url}`, {
            method: 'POST',
            headers: this.defaultHeaders,
            body: JSON.stringify(data)
        });

        return this._handleResponse(response);
    }

    /**
     * PUT request
     */
    async put(url, data = {}) {
        const response = await fetch(`${this.baseUrl}${url}`, {
            method: 'PUT',
            headers: this.defaultHeaders,
            body: JSON.stringify(data)
        });

        return this._handleResponse(response);
    }

    /**
     * DELETE request
     */
    async delete(url) {
        const response = await fetch(`${this.baseUrl}${url}`, {
            method: 'DELETE',
            headers: this.defaultHeaders
        });

        return this._handleResponse(response);
    }

    /**
     * Handle HTTP response
     */
    async _handleResponse(response) {
        if (!response.ok) {
            const errorData = await response.json().catch(() => ({}));
            const error = new Error(errorData.message || `HTTP ${response.status}: ${response.statusText}`);
            error.status = response.status;
            error.data = errorData;
            throw error;
        }

        const contentType = response.headers.get('content-type');
        if (contentType && contentType.includes('application/json')) {
            return await response.json();
        }

        return await response.text();
    }

    /**
     * Upload file
     */
    async uploadFile(url, file, additionalData = {}) {
        const formData = new FormData();
        formData.append('file', file);
        
        Object.keys(additionalData).forEach(key => {
            formData.append(key, additionalData[key]);
        });

        const response = await fetch(`${this.baseUrl}${url}`, {
            method: 'POST',
            body: formData
        });

        return this._handleResponse(response);
    }
}