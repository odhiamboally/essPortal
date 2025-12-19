class FrontendEncryptionService {
    constructor(key) {
        this.key = this.normalizeKey(key);
        this.isEnabled = key && key.trim().length > 0;
    }

    normalizeKey(key) {
        if (!key) return '';
        // Ensure 32 bytes for AES-256
        return key.padEnd(32, '0').substring(0, 32);
    }

    async encrypt(payload) {
        try {
            if (!payload || typeof payload !== 'string' || !this.isEnabled) {
                return payload;
            }

            // Convert key to Uint8Array
            const secret = new TextEncoder().encode(this.key);

            // Create JWE
            const jwt = await new jose.EncryptJWT({ data: payload })
                .setProtectedHeader({ alg: 'A256KW', enc: 'A256GCM' })
                .setIssuedAt()
                .setExpirationTime('1h')
                .encrypt(secret);

            // Base64 encode for transport
            return btoa(jwt);
        } catch (error) {
            console.warn('Encryption failed:', error);
            // Return original payload if encryption fails
            return payload;
        }
    }

    async decrypt(encryptedPayload) {
        try {
            if (!encryptedPayload || !this.isEncrypted(encryptedPayload) || !this.isEnabled) {
                return encryptedPayload;
            }

            // Decode from base64
            const jwt = atob(encryptedPayload);

            // Convert key to Uint8Array
            const secret = new TextEncoder().encode(this.key);

            // Decrypt JWE
            const { payload } = await jose.jwtDecrypt(jwt, secret);

            return payload.data;
        } catch (error) {
            console.warn('Decryption failed:', error);
            // Return original payload if decryption fails
            return encryptedPayload;
        }
    }

    isEncrypted(payload) {
        try {
            if (!payload || typeof payload !== 'string') {
                return false;
            }

            // Check if it's base64 encoded JWE
            const decoded = atob(payload);
            return decoded.startsWith('eyJ') && decoded.split('.').length === 5;
        } catch {
            return false;
        }
    }

    // Method to encrypt form data before AJAX submission
    async encryptFormData(formData) {
        const serialized = JSON.stringify(formData);
        return await this.encrypt(serialized);
    }

    // Method to decrypt response data
    async decryptResponse(responseText) {
        try {
            const decrypted = await this.decrypt(responseText);
            return JSON.parse(decrypted);
        } catch {
            // If decryption or parsing fails, return original
            return responseText;
        }
    }
}

// Initialize encryption service - key will be set via server-side rendering
let encryptionKey = '';

// Function to initialize with key from server
function initializeEncryption(key) {
    encryptionKey = key;
    window.encryptionService = new FrontendEncryptionService(encryptionKey);
}

// Default initialization (will be overridden by server-rendered key)
window.encryptionService = new FrontendEncryptionService('');

// jQuery AJAX interceptor for automatic encryption/decryption
if (typeof jQuery !== 'undefined') {
    $(document).ajaxSend(async function (event, xhr, settings) {
        // Skip for GET requests and excluded endpoints
        const excludedPaths = ['/health', '/swagger', '/api/auth/login', '/Account/Login'];
        const isExcluded = excludedPaths.some(path =>
            settings.url && settings.url.toLowerCase().includes(path.toLowerCase())
        );

        if (settings.type !== 'GET' && !isExcluded && settings.data && window.encryptionService.isEnabled) {
            try {
                // Encrypt request data
                settings.data = await window.encryptionService.encrypt(settings.data);
            } catch (error) {
                console.warn('Failed to encrypt AJAX request:', error);
            }
        }
    });
}

// Fetch API interceptor
const originalFetch = window.fetch;
window.fetch = async function (resource, options = {}) {
    // Skip for GET requests and excluded endpoints
    const excludedPaths = ['/health', '/swagger', '/api/auth/login', '/Account/Login'];
    const url = resource.toString().toLowerCase();
    const isExcluded = excludedPaths.some(path => url.includes(path.toLowerCase()));

    if (options.method && options.method !== 'GET' && !isExcluded && options.body && window.encryptionService.isEnabled) {
        try {
            // Encrypt request body
            options.body = await window.encryptionService.encrypt(options.body);
        } catch (error) {
            console.warn('Failed to encrypt fetch request:', error);
        }
    }

    const response = await originalFetch(resource, options);

    // Only modify response if encryption is enabled
    if (window.encryptionService.isEnabled) {
        // Create a new response with decryption capability
        const originalText = response.text.bind(response);
        response.text = async function () {
            const text = await originalText();
            return await window.encryptionService.decrypt(text);
        };

        const originalJson = response.json.bind(response);
        response.json = async function () {
            const text = await response.text();
            return JSON.parse(text);
        };
    }

    return response;
};