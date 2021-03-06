﻿(function(app) {
    'use strict';

    app.factory('api', function($q, $timeout, ajax) {
        return {
            summoners: {},
            champions: {},
            subreddits: {},
            loading: false,

            status: null,

            pollPromise: null,
            poll: function(i) {
                // default: polls 5 times with interval of 5 seconds (total ~20 seconds; fencepost counting)
                i = i === undefined ? 5 : i;
                var $this = this;
                $timeout.cancel($this.pollPromise);
                $this.loading = true;

                ajax.get('/profile/api/data', function(ok, data) {
                    $this.loading = false;

                    if(ok) {
                        $this.status = null;
                        var keys = Object.keys(data.result);
                        for(var j = 0; j < keys.length; j++)
                            $this[keys[j]] = data.result[keys[j]];
                    }
                    else
                        $this.status = { error: 'Error loading summoners' };

                    // delay either 5 seconds or an hour
                    var delay = (--i > 0) ? 5e3 : 3.6e6;
                    $this.pollPromise = $timeout(function(i) {
                        $this.poll(i);
                    }, delay, true, i);
                });
            }
        };
    }).directive('flair', function() {
        return {
            scope: {
                flair: '=',
                flairRank: '=',
                flairMastery: '=',
                flairPrestige: '=',
                flairMasteryPoints: '='
            },
            template: '<span class="flair" ng-class="classes" ng-attr-title="{{innerText}}" ng-bind="innerText"></span>',
            link: function(scope, element, attrs) {
                function update() {
                    var flair = scope.flair;
                    var text = flair.flairText || '';
                    var classes = [];

                    if (flair.rankEnabled)
                        classes.push('flair-rank-' + scope.flairRank);
                    if (flair.championMasteryEnabled && +scope.flairMastery > 0)
                        classes.push('flair-mastery-' + scope.flairMastery);
                    if (flair.prestigeEnabled && +scope.flairPrestige >= 1000)
                        classes.push('flair-prestige-' + (+scope.flairPrestige / 1000));
                    if (flair.championMasteryTextEnabled) {
                        classes.push('flair-masteryText');
                        text = scope.flairMasteryPoints.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ',') + ' ' + text;
                    }
                    scope.classes = classes.join(' ');
                    scope.innerText = text;
                }
                update();
                scope.$watch('flair', update, true);
            }
        };
    }).directive('champImg', function() {
        var baseUrl = window.Riot.DDragon.m.cdn.replace('http://', '//') + '/' + window.Riot.DDragon.m.n.champion + '/img/champion/';
        return {
            scope: { champImg: '=' },
            link: function(scope, element, attrs) {
                scope.$watch('champImg',
                    function(val) {
                        attrs.$set('src', baseUrl + val + '.png');
                    });
            }
        };
    }).directive('summonerImg', function() {
        var baseUrl = window.Riot.DDragon.m.cdn.replace('http://', '//') + '/' + window.Riot.DDragon.m.n.profileicon + '/img/profileicon/';
        return {
            scope: { summonerImg: '=' },
            link: function (scope, element, attrs) {
                scope.$watch('summonerImg',
                    function(val) {
                        attrs.$set('src', baseUrl + val + '.png');
                    });
            }
        };
    }).filter('filterHidden', function() {
        return function(items) {
            var result = {};
            angular.forEach(items, function(value, key) {
                if (!value.hidden) {
                    result[key] = value;
                }
            });
            return result;
        };
    }).filter('orderObjectBy', function() {
        return function(items, field, reverse) {
            var filtered = [];
            angular.forEach(items, function(item) {
                filtered.push(item);
            });
            filtered.sort(function(a, b) {
                return a[field] - b[field];
            });
            if(reverse) filtered.reverse();
            return filtered;
        };
    }).filter('prestige', function() {
        return function(num) {
            if (typeof num !== 'number')
                return num;
            if (num > 1e6)
                return num / 1e6 + 'M';
            return num / 1e3 + 'k';
        };
    });

    app.controller('MainController', function($scope, $timeout, ajax, modal, api) {
        $scope.api = api;
        $scope.api.poll();

        var modalDelete = modal('#modal-confirm-delete');
        var modalRegister = modal('#modal-register');
        var modalEditFlair = modal('#modal-edit-flair');

        $scope.orderSubreddits = function(subreddits) {
            var filtered = [];
            angular.forEach(subreddits, function(i) {
                filtered.push(i);
            });
            filtered.sort(function(a, b) {
                return $scope.api.champions[b.championId].points - $scope.api.champions[a.championId].points;
            });
            return filtered;
            //return $scope.api.champions[subreddit.championId].points;
        };

        $scope.updatingSummoners = [];
        $scope.isSummonerUpdating = function(summoner) {
            if (!summoner.lastUpdate)
                return true;
            return $scope.updatingSummoners.indexOf(summoner.id) >= 0;
        };

        $scope.refreshSummoner = function(summoner) {
            $scope.updatingSummoners.push(summoner.id);
            ajax.post('/profile/api/summoner/refresh', summoner, function(success, data) {
                var complete = function() {
                    var index = $scope.updatingSummoners.indexOf(summoner.id);
                    $scope.updatingSummoners.splice(index, 1);
                };

                if (success) {
                    var clearWatch = $scope.$watch('api.summoners[' + summoner.id + '].lastUpdate',
                        function(newVal, oldVal) {
                            if (newVal === oldVal)
                                return;
                            complete();
                            clearWatch();
                        });
                    $scope.api.poll();
                } else {
                    $timeout(function() {
                        complete();
                    }, 5000);
                }
            });
            return false;
        };

        $scope.deleteSummoner = function(summoner) {
            modalDelete.data = summoner;
            modalDelete.show();
            return false;
        };

        $scope.registerSummoner = function() {
            modalRegister.show();
        };

        $scope.editSubredditFlair = function(id, subreddit) {
            modalEditFlair.data = { id: id, subreddit: subreddit };
            modalEditFlair.show();
        };
    });

    app.controller('EditFlairController', function($scope, $timeout, modal, api, ajax) {
        var dialog = modal('#modal-edit-flair');

        $scope.api = api;
        $scope.dialog = dialog;

        dialog.onShow(function(e) {
            $scope.id = dialog.data.id;
            $scope.subreddit = dialog.data.subreddit;
            $scope.flairData = JSON.parse(JSON.stringify(dialog.data.subreddit.flair));
        });

        $scope.updateSubredditFormSubmit = function(data, subreddit) {
            subreddit.busy = true;
            ajax.post('/profile/api/subreddit/update', data, function(ok, data) {
                dialog.hide();
                $scope.api.poll();
                $timeout(function() {
                    subreddit.busy = false;
                }, 5000);
            });
        };
    });

    app.controller('DeleteController', function($scope, ajax, modal) {
        var dialog = modal('#modal-confirm-delete');

        $scope.dialog = dialog;
        $scope.confirm = function() {
            var data = dialog.data;
            $scope.api.summoners[data.id].hidden = true;

            $scope.busy = true;
            ajax.post('/profile/api/summoner/delete', data, function(success, data) {
                $scope.busy = false;
                dialog.hide();

                if (success)
                    delete $scope.api.summoners[data.id];
                else
                    $scope.api.summoners[data.id].hidden = false;
            });
        };
    });

    app.controller('RegisterController', function($scope, $timeout, ajax, modal) {
        var dialog = modal('#modal-register');

        dialog.onShow(function() {
            //$scope.code = null;
            $scope.profileIcon = null;
            $scope.token = null;

            $scope.alert = null;
            window.setTimeout(function() { $('#summonerName').focus(); }, 100);
        });

        var validationAttempts;

        function executeValidation() {
            var data = {
                region: $scope.region,
                summonerName: $scope.summonerName,
                token: $scope.token
            };
            ajax.post('/profile/api/summoner/validate', data, function(success, data, status) {
                //console.log(arguments);

                if ((status === 417)) {
                    if (--validationAttempts) {
                        $timeout(executeValidation, 5000);
                        return;
                    }
                    $scope.alert = { text: 'Validation was unsuccessful. Please double check your summoner icon.' };
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
                $scope.profileIcon = data.result.profileIcon;
                $scope.token = data.result.token;
                //$scope.code = data.result.code;
            });
        }

        $scope.dialog = dialog;
        $scope.confirm = function() {
            $scope.busy = true;
            $scope.alert = null;

            // if ($scope.code) {
            if ($scope.token) {
                validationAttempts = 3;
                executeValidation();
            }
            else {
                executeRegistration();
            }
        };
    });
})(window.app);
