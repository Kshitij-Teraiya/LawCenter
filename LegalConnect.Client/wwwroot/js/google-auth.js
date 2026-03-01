// Google Identity Services interop for Blazor WASM

window.googleAuth = {
    /**
     * Initialize Google Sign-In and render the button in a target element.
     * @param {string} clientId - Google OAuth Client ID
     * @param {object} dotNetRef - .NET object reference for callback
     * @param {string} elementId - DOM element ID to render the button in
     */
    initialize: function (clientId, dotNetRef, elementId) {
        // Wait for the GIS library to load
        if (typeof google === "undefined" || !google.accounts) {
            // Retry after a short delay if library hasn't loaded yet
            setTimeout(function () {
                window.googleAuth.initialize(clientId, dotNetRef, elementId);
            }, 200);
            return;
        }

        google.accounts.id.initialize({
            client_id: clientId,
            callback: function (response) {
                dotNetRef.invokeMethodAsync("OnGoogleSignIn", response.credential);
            }
        });

        var targetElement = document.getElementById(elementId);
        if (targetElement) {
            google.accounts.id.renderButton(targetElement, {
                theme: "outline",
                size: "large",
                width: "100%",
                text: "signin_with",
                shape: "rectangular",
                logo_alignment: "center"
            });
        }
    }
};
