/**
 * Shared Device Fingerprint Generator
 * Generates a consistent hash based on browser properties.
 */
window.DeviceFingerprint = (function () {
    'use strict';

    async function generate() {
        try {
            // Use a combination of properties for a robust fingerprint
            const canvas = document.createElement('canvas');
            const ctx = canvas.getContext('2d');
            ctx.textBaseline = 'top';
            ctx.font = '14px Arial';
            ctx.fillText('ESS_Portal_Device_Fingerprint', 2, 2);

            const fingerprintData = [
                navigator.userAgent,
                navigator.language,
                screen.width + 'x' + screen.height,
                new Date().getTimezoneOffset(),
                canvas.toDataURL(),
                navigator.hardwareConcurrency,
                (navigator.deviceMemory || 'unknown')
            ].join('|||'); // Use a unique separator

            // Use the SubtleCrypto API for a secure hash (SHA-256)
            const encoder = new TextEncoder();
            const data = encoder.encode(fingerprintData);
            const hashBuffer = await crypto.subtle.digest('SHA-256', data);

            // Convert buffer to hex string
            const hashArray = Array.from(new Uint8Array(hashBuffer));
            const hashHex = hashArray.map(b => b.toString(16).padStart(2, '0')).join('');

            console.log('✅ Device fingerprint generated:', hashHex);
            return hashHex;

        } catch (error) {
            console.warn('⚠️ Could not generate robust device fingerprint, using fallback:', error);
            // Fallback for older browsers or security restrictions
            let hash = 0;
            const fallbackData = navigator.userAgent + navigator.language;
            for (let i = 0; i < fallbackData.length; i++) {
                const char = fallbackData.charCodeAt(i);
                hash = ((hash << 5) - hash) + char;
                hash = hash & hash; // Convert to 32bit integer
            }
            return 'fallback_' + Math.abs(hash).toString(16);
        }
    }

    /**
     * Sets the generated fingerprint on a hidden input field within a form.
     * @param {string} formId The ID of the form.
     * @param {string} inputName The 'name' attribute of the hidden input field.
     */
    async function setOnForm(formId, inputName) {
        const form = document.getElementById(formId);
        if (!form) {
            console.error(`❌ Form with ID '${formId}' not found.`);
            return;
        }

        let input = form.querySelector(`input[name="${inputName}"]`);
        if (!input) {
            console.log(`🔍 Creating hidden input for '${inputName}' in form '${formId}'.`);
            input = document.createElement('input');
            input.type = 'hidden';
            input.name = inputName;
            form.appendChild(input);
        }

        input.value = await generate();
    }

    return {
        generate: generate,
        setOnForm: setOnForm
    };
})();