angular.module('VirtoCommerce.Passkeys')
    .factory('VirtoCommerce.Passkeys.passkeysApi', ['$resource', function ($resource) {
        return $resource('api/passkeys', {}, {
            getRequestOptions: { method: 'POST', url: 'api/passkeys/request-options' },
            getCreationOptions: { method: 'POST', url: 'api/passkeys/creation-options' },
            create: { method: 'POST', url: 'api/passkeys' }
        });
    }]);
