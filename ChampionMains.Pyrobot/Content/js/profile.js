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
                    }, 10e3);
                });
            },

            getChampionMastery: function(championId) {
                var points = 0;
                var level = 0;
                var prestige = 0;
                for (var i = 0; i < this.items.length; i++) {
                    var summoner = this.items[i];
                    if(summoner.hidden)
                        continue;
                    var champ = summoner.champions[championId];
                    if (champ == null)
                        continue;

                    points += champ.points;
                    if (champ.level > level)
                        level = champ.level;
                    if (champ.prestige > prestige)
                        prestige = champ.prestige;
                }
                return {
                    points: points,
                    level: level,
                    prestige: prestige
                };
            },

            getTopTier: function() {
                var tier = 0;
                var tierString = "";
                for (var i = 0; i < this.items.length; i++) {
                    var summoner = this.items[i];
                    if(summoner.tier <= tier)
                        continue;
                    tier = summoner.tier;
                    tierString = summoner.tierString;
                }
                return tierString;
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

            refresh: function() {
                var $this = this;
                ajax.post('/profile/api/subreddits/refresh', function(ok, data) {
                    if(!ok) {
                        $this.status = { error: 'Error setting subreddit flairs' };
                        return;
                    }
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

    app.controller('MainController', function($scope, $timeout, ajax, modal, summoners, subreddits) {
        $scope.summoners = summoners;
        $scope.summoners.poll();

        $scope.subreddits = subreddits;
        $scope.subreddits.update();

        $scope.prestiges = [];
        ajax.get('/profile/api/prestiges', function(ok, data) {
            if (ok)
                $scope.prestiges = data.result;
        });
        $scope.getNextPrestige = function(points, p) {
            for (var i = 0; i < p.length; i++)
                if (p[i] > points)
                    return (p[i] / 1e3) + 'k';
            return '?';
        };

        $scope.subredditSorter = function(subreddit) {
            var points = $scope.summoners.getChampionMastery(subreddit.championId).points;
            //if (!subreddit.rankEnabled && !subreddit.championMasteryEnabled)
            //    points -= 2e9;
            return points;
        };

        var modalDelete = modal('#modal-confirm-delete');
        var modalRegister = modal('#modal-register');

        $scope.refreshSummoner = function(summoner) {
            ajax.post('/profile/api/summoner/refresh', {
                region: summoner.region,
                summonerName: summoner.summonerName
            }, function(ok, data) {
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

        $scope.formData = {};
        $scope.updateSubredditFormSubmit = function(data) {
            console.log(data);
            ajax.post('/profile/api/subreddit/update', data, function(ok, data) {
                //ok
            });
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