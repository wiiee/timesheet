(function () {
    angular.module('reportByPerformance', ['ui.bootstrap'])
        .controller('reportByPerformanceCtrl', function ($scope, $http) {
            $scope.isStartDateOpen = false;
            $scope.isEndDateOpen = false;
            $scope.items = [];
            $scope.currentItem = {};
            $scope.isPull = true;

            $scope.openStartDate = function () {
                $scope.isStartDateOpen = !$scope.isStartDateOpen;
            };

            $scope.openEndDate = function () {
                $scope.isEndDateOpen = !$scope.isEndDateOpen;
            };

            $scope.init = function () {
                var url = _basePath + "/api/userperformance/getSample";
                $http.post(url).then(function (response) {
                    $scope.currentItem = response.data;
                });

                url = _basePath + "/api/userperformance/getItems";
                $http.post(url).then(function (response) {
                    $scope.items = response.data;
                });
            };

            $scope.pull = function () {
                var url = _basePath + "/api/userperformance/pull";
                $http.post(url, $scope.currentItem).then(function (response) {
                    $scope.currentItem = response.data;
                    $scope.isPull = !$scope.isPull;
                });
            };

            $scope.calculate = function () {
                var url = _basePath + "/api/userperformance/calculate";
                $http.post(url, $scope.currentItem).then(function (response) {
                    $scope.currentItem = response.data;
                    $scope.currentItem.DateRange = _searchDateRange;
                });
            };

            $scope.save = function () {
                var url = _basePath + "/api/userperformance/save";
                $http.post(url, $scope.currentItem).then(function (response) {
                    $scope.currentItem = response.data;
                    $scope.currentItem.DateRange = _searchDateRange;
                });
            };

            $scope.init();
        });
}());