angular.module('VirtoCommerce.Passkeys')
    .controller('VirtoCommerce.Passkeys.passkeySignInKeyController', ['$scope', function ($scope) {
        var keyStates = {
            DeviceOnly: { icon: 'fa fa-laptop', title: 'passkeys.key-states.device-only' },
            BackedUp: { icon: 'fa fa-cloud', title: 'passkeys.key-states.backed-up' },
            NotBackedUp: { icon: 'fa fa-cloud', title: 'passkeys.key-states.not-backed-up' }
        };

        $scope.keyState = keyStates[$scope.key.state] || {};
    }]);
