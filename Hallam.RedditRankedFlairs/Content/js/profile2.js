﻿(function (app) {
    app.factory('summoners', function (ajax) {
        return {
            items: [],

            indexOf: function (region, summonerName) {
                for (var i = 0; i < this.items.length; i += 1) {
                    var s = this.items[i];
                    if (s.region == region && s.summonerName == summonerName) {
                        return i;
                    }
                }
                return -1;
            },
            
            remove: function (region, summonerName) {
                var i = this.indexOf(region, summonerName);
                if (i > -1) {
                    this.items.splice(i, 1);
                    return true;
                }
            },

            update: function () {
                var $this = this;
                $this.loading = true;
                $this.items = [];
                ajax.get('/profile/api/summoners', function (ok, data) {
                    $this.loading = false;
                    if (!ok) {
                        $this.status = { error: 'Error loading summoners' };
                        return;
                    }
                    $this.items = data.result;
                });
            }
        };
    });

    app.controller('MainController', function ($scope, modal, summoners) {
        $scope.summoners = summoners;
        summoners.update();

        var modalDelete = modal('#modal-confirm-delete');
        var modalRegister = modal('#modal-register');

        $scope.deleteSummoner = function (summoner) {
            modalDelete.data = {
                region: summoner.region,
                summonerName: summoner.summonerName
            };
            modalDelete.show();
        };

        $scope.registerSummoner = function () {
            modalRegister.show();
        };
    });

    app.controller('DeleteController', function ($scope, ajax, modal) {
        var dialog = modal('#modal-confirm-delete');

        $scope.dialog = dialog;
        $scope.confirm = function () {
            $scope.busy = true;
            ajax.post('/profile/api/delete', dialog.data, function (success, data) {
                $scope.busy = false;
                dialog.hide();
                if (success) {
                    $scope.summoners.remove(dialog.data.region, dialog.data.summonerName);
                }
            });
        };
    });

    app.controller('RegisterController', function ($scope, ajax, modal) {
        var dialog = modal('#modal-register');

        dialog.shown(function () {
            $scope.code = null;
            $scope.alert = null;
            window.setTimeout(function () { $('#summonerName').focus(); }, 100);
        });

        function executeValidation() {
            var data = {
                region: $scope.region,
                summonerName: $scope.summonerName
            };
            ajax.post('/profile/api/validate', data, function (success, data) {
                $scope.busy = false;
                if (!success) {
                    $scope.alert = { text: data.error };
                    return;
                }
                dialog.hide();
                $scope.summoners.update();
            });
        }

        function executeRegistration() {
            var data = {
                region: $scope.region,
                summonerName: $scope.summonerName
            };
            ajax.post('/profile/api/register', data, function (success, data) {
                $scope.busy = false;
                if (!success) {
                    $scope.alert = { text: data.error };
                    return;
                }
                $scope.code = data.result.code;
            });
        }

        $scope.dialog = dialog;
        $scope.confirm = function () {
            $scope.busy = true;
            $scope.alert = null;
            
            if ($scope.code) {
                executeValidation();
            }
            else {
                executeRegistration();
            }
        };
    });
})(window.app);