// Call this to register your module to main application
var moduleName = 'VirtoCommerce.Passkeys';

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .run(['$window', '$translate', 'platformWebApp.authService', 'platformWebApp.signInMethodService', 'platformWebApp.signInLogTypes',
        'VirtoCommerce.Passkeys.passkeysApi',
        function ($window, $translate, authService, signInMethodService, signInLogTypes, passkeysApi) {
            const extension = {
                title: 'passkeys.labels.title',
                logoUrl: 'Modules/$(VirtoCommerce.Passkeys)/Content/passkey-logo.svg',
                keyTemplateUrl: 'Modules/$(VirtoCommerce.Passkeys)/Scripts/templates/passkey-sign-in-key.html'
            };

            if (isPasskeySupported()) {
                extension.signIn = signIn;
                extension.createKey = createKey;
            }

            signInMethodService.extendSignInMethod('Passkey', extension);
            signInLogTypes.register({ label: 'passkeys.sign-in-log.type-passkey', value: 'Passkey' });

            function isPasskeySupported() {
                const publicKeyCredential = $window.PublicKeyCredential;

                return !!publicKeyCredential &&
                    angular.isFunction(publicKeyCredential.parseCreationOptionsFromJSON) &&
                    angular.isFunction(publicKeyCredential.parseRequestOptionsFromJSON);
            }

            async function signIn() {
                try {
                    const options = await passkeysApi.getRequestOptions().$promise;
                    const publicKey = $window.PublicKeyCredential.parseRequestOptionsFromJSON(options.toJSON());
                    const credential = await $window.navigator.credentials.get({ publicKey: publicKey });

                    await signInMethodService.signInWithParameters({ grant_type: 'passkey', credential: JSON.stringify(credential) });
                    await authService.fillAuthData();
                } catch (error) {
                    if (!isPasskeyPromptCancelled(error)) {
                        throw error;
                    }
                }
            }

            async function createKey() {
                try {
                    const options = await passkeysApi.getCreationOptions().$promise;
                    const publicKey = $window.PublicKeyCredential.parseCreationOptionsFromJSON(options.toJSON());
                    const credential = await $window.navigator.credentials.create({ publicKey: publicKey });

                    await passkeysApi.create({ credential: JSON.stringify(credential) }).$promise;
                } catch (error) {
                    if (isPasskeyPromptCancelled(error)) {
                        return;
                    }

                    if (error instanceof DOMException && error.name === 'InvalidStateError') {
                        throw $translate.instant('passkeys.errors.already-registered');
                    }

                    throw error instanceof Error ? error.message : error;
                }
            }

            function isPasskeyPromptCancelled(error) {
                return error instanceof DOMException && error.name === 'NotAllowedError';
            }
        }]);
