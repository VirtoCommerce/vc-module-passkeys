// Call this to register your module to main application
var moduleName = 'VirtoCommerce.Passkeys';

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .run(['$window', 'platformWebApp.authService', 'platformWebApp.signInMethodService', 'VirtoCommerce.Passkeys.passkeysApi',
        function ($window, authService, signInMethodService, passkeysApi) {
            signInMethodService.extendSignInMethod('Passkey', {
                title: 'passkeys.labels.title',
                logoUrl: 'Modules/$(VirtoCommerce.Passkeys)/Content/passkey-logo.svg',
                keyTemplateUrl: 'Modules/$(VirtoCommerce.Passkeys)/Scripts/templates/passkey-sign-in-key.html',
                signIn: signIn,
                createKey: createKey
            });

            async function signIn() {
                const options = await passkeysApi.getRequestOptions().$promise;
                const publicKey = $window.PublicKeyCredential.parseRequestOptionsFromJSON(options.toJSON());
                const credential = await $window.navigator.credentials.get({ publicKey: publicKey });

                await signInMethodService.signInWithParameters({ grant_type: 'passkey', credential: JSON.stringify(credential) });
                await authService.fillAuthData();
            }

            async function createKey() {
                const options = await passkeysApi.getCreationOptions().$promise;
                const publicKey = $window.PublicKeyCredential.parseCreationOptionsFromJSON(options.toJSON());
                const credential = await $window.navigator.credentials.create({ publicKey: publicKey });

                await passkeysApi.create({ credential: JSON.stringify(credential) }).$promise;
            }
        }]);
