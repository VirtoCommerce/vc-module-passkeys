angular.module('VirtoCommerce.Passkeys')
    .factory('VirtoCommerce.Passkeys.webApi', ['$resource', function ($resource) {
        return $resource('api/passkeys');
    }]);
