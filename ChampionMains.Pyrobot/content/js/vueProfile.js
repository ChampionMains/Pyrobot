var profileApiService = {
    state: {
        summoners: {},
        champions: {},
        subreddits: {},
        loading: false,
        status: null
    },
    _pollPromise: null,
    poll: function(i) {
        // default: polls 5 times with interval of 5 seconds (total ~20 seconds; fencepost counting)
        i = i === undefined ? 5 : i;
        clearTimeout(this._pollPromise);
        this.state.loading = true;

        fetch('/profile/api/data')
            .then(function(response) {
                return response.json();
            })
            .then(function(data) {
                this.state.loading = false;
                Object.assign(this.state, data.result);
                // delay either 5 seconds or an hour
                var delay = (--i > 0) ? 5e3 : 3.6e6;
                this._pollPromise = setTimeout(this.poll.bind(this), delay, i);
            }.bind(this))
            .catch(function(ex) {
                this.state.loading = false;
                console.log('loading data failed', ex);
                this.state.status = { error: 'Error loading summoners' };
            }.bind(this));
    }
};

var app = new Vue({
    el: '#app',
    data: {
        profileState: profileApiService.state
    },
    computed: {
        sortedChampions: function() {
            return Object.values(this.profileState.champions)
                .sort(function(a, b) { return b.points - a.points; }); //desc
        }
    },
    mounted: function() {
        profileApiService.poll();
    }
});
