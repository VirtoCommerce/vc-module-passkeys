// Call this to register your module to main application
var moduleName = 'VirtoCommerce.Passkeys';

if (AppDependencies !== undefined) {
    AppDependencies.push(moduleName);
}

angular.module(moduleName, [])
    .config(['$stateProvider',
        function ($stateProvider) {
            $stateProvider
                .state('workspace.PasskeysState', {
                    url: '/passkeys',
                    templateUrl: '$(Platform)/Scripts/common/templates/home.tpl.html',
                    controller: [
                        'platformWebApp.bladeNavigationService',
                        function (bladeNavigationService) {
                            var newBlade = {
                                id: 'blade1',
                                controller: 'VirtoCommerce.Passkeys.helloWorldController',
                                template: 'Modules/$(VirtoCommerce.Passkeys)/Scripts/blades/hello-world.html',
                                isClosingDisabled: true,
                            };
                            bladeNavigationService.showBlade(newBlade);
                        }
                    ]
                });
        }
    ])
    .run(['platformWebApp.mainMenuService', '$state',
        function (mainMenuService, $state) {
            //Register module in main menu
            var menuItem = {
                path: 'browse/passkeys',
                icon: 'fa fa-cube',
                title: 'Passkeys',
                priority: 100,
                action: function () { $state.go('workspace.PasskeysState'); },
                permission: 'passkeys:access',
            };
            mainMenuService.addMenuItem(menuItem);
        }
    ]);
