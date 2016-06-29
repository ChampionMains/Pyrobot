(function(app) {
    app.factory('api', function($q, $timeout, ajax) {
        return {
            summoners: [],
            champions: {},
            subreddits: [],
            loading: false,

            pollPromise: null,
            poll: function(i) {
                // default: polls 4 times every 5 seconds (total ~15 seconds; fencepost counting)
                i = i == undefined ? 4 : 0;
                var $this = this;
                $timeout.cancel($this.pollPromise);
                $this.loading = true;
                ajax.get('/profile/api/data', function(ok, data) {
                    $this.loading = false;
                    if(!ok) {
                        $this.status = { error: 'Error loading summoners' };
                        return;
                    }
                    var keys = Object.keys(data.result);
                    for (var j = 0; j < keys.length; j++)
                        $this[keys[j]] = data.result[keys[j]];

                    // delay either 5 seconds or an hour
                    var delay = (--i > 0) ? 5e3 : 3.6e6;
                    $this.pollPromise = $timeout(function(i) {
                        $this.poll(i);
                    }, delay, true, i);
                });
            }
        };
    }).directive('champImg', function() {
        var baseUrl = Riot.DDragon.m.cdn + '/' + Riot.DDragon.m.n.champion + '/img/champion/';
        return {
            scope: { champImg: '=' },
            link: function(scope, element, attrs) {
                scope.$watch('champImg', function(val) {
                    attrs.$set('src', baseUrl + scope.champImg + '.png');
                });
            }
        }
    });

    app.controller('MainController', function($scope, $timeout, ajax, modal, api) {
        $scope.api = api;
        $scope.api.poll();

        var modalDelete = modal('#modal-confirm-delete');
        var modalRegister = modal('#modal-register');

        $scope.refreshSummoner = function(summoner) {
            ajax.post('/profile/api/summoner/refresh', summoner, function(ok, data) {
                $scope.api.poll();
            });
            return false;
        }

        $scope.deleteSummoner = function(summoner) {
            modalDelete.data = summoner;
            modalDelete.show();
            return false;
        };

        $scope.registerSummoner = function() {
            modalRegister.show();
        };

        $scope.updateSubredditFormSubmit = function(data, subreddit) {
            subreddit.busy = true;
            ajax.post('/profile/api/subreddit/update', data, function(ok, data) {
                subreddit.busy = false;
            });
        };
    });

    app.controller('DeleteController', function($scope, ajax, modal) {
        var dialog = modal('#modal-confirm-delete');

        $scope.dialog = dialog;
        $scope.confirm = function() {
            var data = dialog.data;
            var i = $scope.api.summoners.length;
            while (i > 0 && $scope.api.summoners[--i].id !== data.id);

            $scope.api.summoners[i].hidden = true;

            $scope.busy = true;
            ajax.post('/profile/api/summoner/delete', data, function(success, data) {
                $scope.busy = false;
                dialog.hide();

                var i = $scope.api.summoners.length;
                while(i > 0 && $scope.api.summoners[--i].id !== data.id);

                if(success) {
                    $scope.api.summoners.splice(i, 1);
                } else
                    $scope.api.summoners[i].hidden = false;
            })
        };
    });

    app.controller('RegisterController', function($scope, $timeout, ajax, modal) {
        var dialog = modal('#modal-register');

        dialog.shown(function() {
            $scope.code = null;
            $scope.alert = null;
            window.setTimeout(function() { $('#summonerName').focus(); }, 100);
        });

        var validationAttempts;

        function executeValidation() {
            var data = {
                region: $scope.region,
                summonerName: $scope.summonerName
            };
            ajax.post('/profile/api/summoner/validate', data, function(success, data, status) {
                console.log(arguments);

                if (status === 417) {
                    if (--validationAttempts) {
                        $timeout(executeValidation, 5000);
                        return;
                    }
                    $scope.alert = { text: 'Validation was unsuccessful. Please double check the rune page.' };
                    $scope.busy = false;
                    return;
                }

                $scope.busy = false;

                if (!success) {
                    $scope.alert = { text: data.error || 'Error validating summoner.' };
                    return;
                }

                dialog.hide();
                $scope.api.poll();
            });
        }

        function executeRegistration() {
            var data = {
                region: $scope.region,
                summonerName: $scope.summonerName
            };
            ajax.post('/profile/api/summoner/register', data, function(success, data) {
                $scope.busy = false;
                if (!success) {
                    $scope.alert = { text: data.error };
                    return;
                }
                $scope.code = data.result.code;
            });
        }

        $scope.dialog = dialog;
        $scope.confirm = function() {
            $scope.busy = true;
            $scope.alert = null;
            
            if ($scope.code) {
                validationAttempts = 3;
                executeValidation();
            }
            else {
                executeRegistration();
            }
        };
    });
})(window.app);