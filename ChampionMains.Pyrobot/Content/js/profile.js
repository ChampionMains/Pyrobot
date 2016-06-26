(function(app) {
    app.factory('summoners', function($q, $timeout, ajax) {
        return {
            items: [],

            indexOf: function(region, summonerName) {
                for (var i = 0; i < this.items.length; i++) {
                    var s = this.items[i];
                    if (s.region === region && s.summonerName === summonerName)
                        return i;
                }
                return -1;
            },
            
            remove: function(region, summonerName) {
                var i = this.indexOf(region, summonerName);
                if (i > -1) {
                    this.items.splice(i, 1);
                    return true;
                }
                return false;
            },

            pollPromise: null,
            poll: function() {
                var $this = this;
                $timeout.cancel($this.pollPromise);
                $this.loading = true;
                ajax.get('/profile/api/summoners', function(ok, data) {
                    $this.loading = false;
                    if (!ok) {
                        $this.status = { error: 'Error loading summoners' };
                        return;
                    }
                    $this.items = data.result;

                    $this.pollPromise = $timeout(function () {
                        $this.poll();
                    }, 6000);
                });
            },

            getChampionMastery: function(championId) {
                var points = 0;
                var level = 0;
                for(var i = 0; i < this.items.length; i++) {
                    var summoner = this.items[i];
                    if(summoner.hidden)
                        continue;
                    var champ = summoner.champions[championId];
                    if (champ == null)
                        continue;

                    points += champ.points;
                    if (champ.level > level)
                        level = champ.level;
                }
                return {
                    points: points,
                    level: level
                };
            }
        };
    }).factory('subreddits', function ($q, $timeout, ajax) {
        return {
            items: [],

            indexOf: function(subreddit) {
                for (var i = 0; i < this.items.length; i++) {
                    var s = this.items[i];
                    if (s.name.toLowerCase() === subreddit.toLowerCase())
                        return i;
                }
                return -1;
            },

            remove: function(subreddit) {
                var i = this.indexOf(subreddit);
                if (i > -1) {
                    this.items.splice(i, 1);
                    return true;
                }
                return false;
            },

            update: function() {
                var $this = this;
                $this.loading = true;
                ajax.get('/profile/api/subreddits', function (ok, data) {
                    $this.loading = false;
                    if (!ok) {
                        $this.status = { error: 'Error loading subreddits' };
                        return;
                    }
                    $this.items = data.result;
                });
            },
        };
    }).directive('champImg', function() {
        var baseUrl = Riot.DDragon.m.cdn + '/' + Riot.DDragon.m.n.champion + '/img/champion/';
        return {
            scope: { champImg: '=' },
            link: function(scope, element, attrs) {
                scope.$watch('champImg', function(nVal) {
                    attrs.$set('src', baseUrl + scope.champImg + '.png');
                });
            }
        }
    });;

    app.controller('MainController', function($scope, $timeout, ajax, modal, summoners, subreddits) {
        $scope.summoners = summoners;
        $scope.summoners.poll();

        $scope.subreddits = subreddits;
        $scope.subreddits.update();

        $scope.subredditSorter = function(subreddit) {
            return $scope.summoners.getChampionMastery(subreddit.championId);
        };

        var modalDelete = modal('#modal-confirm-delete');
        var modalRegister = modal('#modal-register');

        $scope.refreshSummoner = function(summoner) {
            var data = {
                region: summoner.region,
                summonerName: summoner.summonerName
            };
            ajax.post('/profile/api/summoner/refresh', data, function(success, data) {
                    $scope.summoners.poll();
                });
        }

        $scope.deleteSummoner = function(summoner) {
            modalDelete.data = {
                region: summoner.region,
                summonerName: summoner.summonerName
            };
            modalDelete.show();
        };

        $scope.registerSummoner = function() {
            modalRegister.show();
        };
    });

    app.controller('DeleteController', function($scope, ajax, modal) {
        var dialog = modal('#modal-confirm-delete');

        $scope.dialog = dialog;
        $scope.confirm = function() {
            var i = $scope.summoners.indexOf(dialog.data.region, dialog.data.summonerName);
            $scope.summoners.items[i].hidden = true;

            $scope.busy = true;
            ajax.post('/profile/api/summoner/delete', dialog.data, function(success, data) {
                $scope.busy = false;
                dialog.hide();

                if (success)
                    $scope.summoners.remove(data.region, data.summonerName);
                else
                    $scope.summoners.list[i].hidden = false;
            });
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

                if (status == 417) {
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
                $scope.summoners.poll();
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