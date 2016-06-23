(function (app) {
    app.controller('EventLogController', function ($scope, ajax, notify, panels) {
        function getEventsHandler(success, data) {

        }

        panels('eventlog').register('opening', function () {
            ajax.get('/adminPanel/api/events', getEventsHandler);
        });
    });
})(angular.module('Admin'));