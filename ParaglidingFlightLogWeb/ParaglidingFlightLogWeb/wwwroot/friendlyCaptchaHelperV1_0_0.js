// Import the SDK from CDN
import { FriendlyCaptchaSDK } from 'https://cdn.jsdelivr.net/npm/@friendlycaptcha/sdk@0.1.36/+esm';

let sdkInstance = null;

export function initializeFriendlyCaptcha() {
    if (!sdkInstance) {
        // Create SDK instance
        sdkInstance = new FriendlyCaptchaSDK({
            startAgent: true
        });

        // Attach to all .frc-captcha elements on the page
        sdkInstance.attach();
    }
}

export function getCaptchaResponse() {
    if (!sdkInstance) {
        console.error('SDK not initialized');
        return null;
    }

    // Get all widgets
    const widgets = sdkInstance.getAllWidgets();
    if (widgets.length === 0) {
        console.error('No widgets found');
        return null;
    }

    // Get the response from the first widget
    const widget = widgets[0];
    const response = widget.getResponse();

    if (!response || response.startsWith('.')) {
        console.warn('Captcha not completed yet. Response:', response);
        return null;
    }

    return response;
}

export function resetFriendlyCaptcha() {
    if (!sdkInstance) {
        console.error('SDK not initialized');
        return;
    }

    const widgets = sdkInstance.getAllWidgets();
    if (widgets.length > 0) {
        widgets[0].reset();
    }
}
export function supportsESModules() {
    const script = document.createElement('script');
    return 'noModule' in script;
}
export function subscribeCaptchaComplete(dotNetRef) {
    document.addEventListener('frc:widget.complete', function (event) {
        // Pass only the response string to .NET
        if (event && event.detail && event.detail.response) {
            dotNetRef.invokeMethodAsync('OnCaptchaComplete', event.detail.response);
        }
    });
}