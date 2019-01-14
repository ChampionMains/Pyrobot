var profileApiService = {
    state: {
        summoners: {},
        champions: {},
        subreddits: {},
        updatingSummoners: {}, // map from summonerId to lastUpdateTime.
        hiddenSummoners: {}, // map from summonerId to TRUE.
        updating: false,
        status: null
    },
    _pollTimeout: null,
    poll: function(i) {
        // default: polls 5 times with interval of 5 seconds (total ~20 seconds; fencepost counting)
        i = i === undefined ? 5 : i;
        clearTimeout(this._pollTimeout);
        this.state.updating = true;

        fetch('/profile/api/data')
            .then(function(res) {
                return res.json();
            })
            .then(function(data) {
                this.state.updating = false;
                Object.assign(this.state, data.result);
                // delay either 5 seconds or an hour
                var delay = (--i > 0) ? 5e3 : 3.6e6;
                this._pollTimeout = setTimeout(this.poll.bind(this), delay, i);
            }.bind(this))
            .catch(function(ex) {
                this.state.updating = false;
                console.log('updating data failed', ex);
                this.state.status = { error: 'Error updating summoners' };
            }.bind(this));
    },
    updateSummoner: function(summoner) {
        this.state.updatingSummoners[summoner.id] = summoner.lastUpdate;
        fetch('/profile/api/summoner/refresh', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(summoner),
            })
            .then(function(res) {
                this.poll();
//                // TODO cancel this timeout.
//                setTimeout(function() {
//                    delete this.state.updatingSummoners[summoner.id];
//                }.bind(this), 5000);
            }.bind(this))
            .catch(function(ex) {
                // TODO just pretend to update w/ failure message if failed.
                console.error('updating data failed', ex);
                this.state.status = { error: 'Error updating summoner' };
            }.bind(this));
    },
    removeSummoner: function(summoner) {
        this.state.hiddenSummoners[summoner.id] = true;
        fetch('/profile/api/summoner/delete', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(summoner),
            })
            .then(function(res) {
                Vue.delete(this.state.summoners, summoner.id);
            }.bind(this))
            .catch(function(ex) {
                Vue.delete(this.state.hiddenSummoners, summoner.id);
                console.error('deleting summoner failed', ex);
                this.state.status = { error: 'Error removing summoner' };
            }.bind(this));
    },
    isSummonerUpdating: function(summoner) {
        var updateTime = this.state.updatingSummoners[summoner.id]; // May be undefined.
        return summoner.lastUpdate === updateTime;
    },
    registerSummonerInfo: function(summonerModel) {
        return fetch('/profile/api/summoner/register', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(summonerModel)
            })
            .then(function(res) {
                return res.json();
            });
    },
    validateSummonerIcon: function(summonerModel) {
        var validationAttempts = 3;
        return (function tryValidate() {
            return fetch('/profile/api/summoner/validate',
                    {
                        method: 'POST',
                        headers: {
                            'Content-Type': 'application/json'
                        },
                        body: JSON.stringify(summonerModel)
                    })
                .then(function(res) {
                    if (res.ok)
                        return true;
                    if (res.status === 417) {
                        if (--validationAttempts) {
                            return Promise.delay(5000).then(tryValidate);
                        }
                        throw new Error('Validation failed. Please check your icon and try again.');
                    }
                    return res.json().then(function(data) {
                        throw new Error(data.error, JSON.stringify(data));
                    });
                });
        })();
    }
};

Vue.use(vueMoment);

var dialogApp = new Vue({
    el: '#dialogApp',
    data: {
        summonerToRemove: null,
        addSummonerData: {
            busy: false,
            page: 1,
            summonerModel: {
                summonerName: null,
                region: null,
                token: null
            },
            summonerInfoError: null,
            summonerValidError: null,
            profileIcon: null,
            alert: null
        }
    },
    computed: {
        addSummonerDisableNext: function() {
            var model = this.addSummonerData.summonerModel;
            return this.addSummonerData.busy || !model.summonerName || !model.region;
        }
    },
    methods: {
        showDialog: function(dialogId) { // TODO inconsistant api.
            document.body.style.overflow = 'hidden';
            this._dialogs[dialogId].showModal();
        },
        closeDialog: function(el) {
            document.body.style.overflow = null;
            el.closest('dialog').close();
        },
        removeSummoner: function(event) {
            this.closeDialog(event.target);
            profileApiService.removeSummoner(this.summonerToRemove);
        },
        summonerCancel: function(el) {
            this.closeDialog(el);
            this.addSummonerData.busy = false;
            this.addSummonerData.page = 1;
            this.addSummonerData.summonerModel.token = null;
            this.addSummonerData.summonerInfoError = null;
            this.addSummonerData.summonerValidError = null;
            this.addSummonerData.profileIcon = null;
            this.addSummonerData.alert = null;
        },
        summonerNextStep: function(summonerModel, el) {
            this.addSummonerData.busy = true;
            this.addSummonerData.alert = null;

            if (!this.addSummonerData.summonerModel.token) {
                // Step 1: post summoner info.
                profileApiService.registerSummonerInfo(summonerModel)
                    .then(function(data) {
                        if (data.error) {
                            console.log('Error getting summoner.', data);
                            this.addSummonerData.summonerInfoError = data.error;
                            return;
                        }
                        this.addSummonerData.summonerModel.token = data.result.token;
                        this.addSummonerData.profileIcon = data.result.profileIcon;
                        setTimeout(function() {
                            this.addSummonerData.busy = false;
                            this.addSummonerData.page = 2;
                        }.bind(this), 500);
                    }.bind(this))
                    .catch(function(ex) {
                        this.addSummonerData.busy = false;
                        console.error(ex);
                    });
            }
            else {
                // Step 2: validate summoner icon.
                this.addSummonerData.summonerValidError = null;
                this.addSummonerData.busy = true;
                profileApiService.validateSummonerIcon(summonerModel)
                    .then(function() {
                        this.summonerCancel(el);
                        profileApiService.poll();
                    }.bind(this))
                    .catch(function(ex) {
                        console.log('Error validation', ex);
                        this.addSummonerData.summonerValidError = ex.message;
                        this.addSummonerData.busy = false;
                    }.bind(this)); //TODO
            }
        }
    },
    watch: {
        'summonerInfoError': function(val) {
                document.getElementById('summonerName').parentElement.className += ' is-invalid';
        }
    },
    created: function() {
        this._dialogs = {};
        this.promptAddSummoner = function() {
            this.showDialog('modal-add-summoner');
        };
        this.promptRemoveSummoner = function(summoner) {
            this.summonerToRemove = summoner;
            this.showDialog('modal-remove-summoner');
        };
    },
    mounted: function() {
        var dialogs = this._dialogs;
        Array.from(this.$el.getElementsByTagName('dialog')).forEach(function(dialog) {
            dialogs[dialog.id] = dialog;
            if (!dialog.showModal) dialogPolyfill.registerDialog(dialog);
        });
    }
});

var app = new Vue({
    el: '#app',
    data: {
        profileState: profileApiService.state,
        now: moment() // Updating current time.
    },
    computed: {
        sortedChampions: function() {
            return Object.values(this.profileState.champions)
                .sort(function(a, b) { return b.points - a.points; })
                .filter(function(a) { return a.points > 0; }); //desc
        },
        filteredSummoners: function() {
            return Object.values(this.profileState.summoners)
                .filter(function(a) { return !this.profileState.hiddenSummoners[a.id]; }.bind(this));
        },
        topMasterySummoners: function() {
            return this.filteredSummoners.slice()
                .sort(function(a, b) { return b.totalPoints - a.totalPoints; });
        },
        topRankedSummoners: function() {
            return this.filteredSummoners.slice()
                .sort(function(a, b) { return (b.tier - a.tier) || (a.division - b.division); });
        },
        topMasterySubreddits: function() {
            return Object.values(this.profileState.subreddits)
                .sort(function(a, b) {
                    return this.profileState.champions[b.championId].points - this.profileState.champions[a.championId].points;
                }.bind(this));
        }
    },
    methods: {
        updateSummoner: function(summoner, event) {
            event.target.disabled = 'disabled'; // Disable immediately, don't wait for vue.
            profileApiService.updateSummoner(summoner);
        },
        promptAddSummoner: function() {
            dialogApp.promptAddSummoner();
        },
        promptRemoveSummoner: function(summoner, event) {
            dialogApp.promptRemoveSummoner(summoner);
        },
        isSummonerUpdating: function(summoner) {
            return profileApiService.isSummonerUpdating(summoner);
        }
    },
    filters: {
        prestige: function(num) {
            if (typeof num !== 'number')
                return num;
            if (num > 1e6)
                return num / 1e6 + 'M';
            return num / 1e3 + 'k';
        }
    },
    created: function() {
        window.addEventListener('load', function(event) { profileApiService.poll(); });
        setInterval(function() {
            this.now = moment();
        }.bind(this), 1000); // Update every second.
    }
});
