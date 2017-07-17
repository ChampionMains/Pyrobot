(function(app) {
    'use strict';

    app.directive('ngTooltip', function () {
        return {
            restrict: 'A',
            link: function (scope, element, attrs) {
                var tooltip = attrs.ngTooltip;
                var match;
                var placement;
                if (!tooltip) {
                    return;
                }
                if ((match = /^(top|right|bottom|left),/.exec(tooltip))) {
                    placement = match[1];
                    tooltip = tooltip.substr(placement.length + 1);
                }
                element.tooltip({
                    'placement': placement,
                    'title': tooltip
                });
            }
        };
    });

    app.factory('ajax', function ($http) {
        function wrap(promise, handler) {
            function interceptHandler(response) {
                if (handler) {
                    var success = response.status >= 200 && response.status <= 299;
                    handler(success, response.data, response.status, response.statusText);
                }
                if (response.data && response.data.exception) {
                    console.log(response.data.exception);
                }
            }
            promise.then(interceptHandler, interceptHandler);
        }
        return {
            get: function (uri, data, handler) {
                if (handler === undefined) {
                    handler = data;
                    data = undefined;
                }
                if (data) {
                    data = { config: { params: data } };
                }
                return wrap($http.get(uri, data), handler);
            },
            post: function (uri, data, handler) {
                if (handler === undefined) {
                    handler = data;
                    data = undefined;
                }
                return wrap($http.post(uri, data), handler);
            }
        };
    });

    app.factory('modal', function () {
        var modalDictionary = {};

        function Modal(id) {
            this.dialog = $(id);
        }
        Modal.prototype = {
            hide: function() { this.dialog.modal('hide'); },
            onHide: function(handler) { this.dialog.on('hide.bs.modal', handler); },
            onHidden: function(handler) { this.dialog.on('hidden.bs.modal', handler); },
            show: function() { this.dialog.modal('show'); },
            onShow: function(handler) { this.dialog.on('show.bs.modal', handler); },
            onShown: function(handler) { this.dialog.on('shown.bs.modal', handler); }
        };
        return function(id) {
            if (!modalDictionary[id]) {
                modalDictionary[id] = new Modal(id);
            }
            return modalDictionary[id];
        };
    });
})(window.app);