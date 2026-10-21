angular.module('VirtoCommerce.Passkeys')
    .controller('VirtoCommerce.Passkeys.helloWorldController', ['$scope', 'VirtoCommerce.Passkeys.webApi', function ($scope, api) {
        var blade = $scope.blade;
        blade.title = 'Passkeys';

        blade.refresh = function () {
            api.get(function (data) {
                blade.title = 'passkeys.blades.hello-world.title';
                blade.data = data.result;
                blade.isLoading = false;
            });
        };

        blade.refresh();
    }]);
