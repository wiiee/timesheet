(function () {
    angular.module('reportByPerformance', ['ui.bootstrap', 'myFilters'])
        .controller('reportByPerformanceCtrl', function ($scope, $http) {
            $scope.pairs = _pairs;
            $scope.isStartDateOpen = false;
            $scope.isEndDateOpen = false;
            $scope.items = [];
            $scope.currentItem = {};
            $scope.isPull = true;

            $scope.openStartDate = function () {
                $scope.isPull = true;
                $scope.isStartDateOpen = !$scope.isStartDateOpen;
            };

            $scope.openEndDate = function () {
                $scope.isPull = true;
                $scope.isEndDateOpen = !$scope.isEndDateOpen;
            };

            $scope.init = function () {
                $scope.getSample();
                $scope.getItems();
            };

            $scope.getItems = function () {
                var url = _basePath + "/api/userperformance/getItems";
                $http.post(url).then(function (response) {
                    $scope.items = response.data;
                });
            };

            $scope.getSample = function () {
                var url = _basePath + "/api/userperformance/getSample";
                $http.post(url).then(function (response) {
                    $scope.currentItem = response.data;
                });
            };

            $scope.pull = function () {
                var url = _basePath + "/api/userperformance/pull";
                $http.post(url, $scope.currentItem).then(function (response) {
                    $scope.currentItem = response.data;
                    $scope.isPull = false;
                });
            };

            $scope.calculate = function () {
                var url = _basePath + "/api/userperformance/calculate";
                $http.post(url, $scope.currentItem).then(function (response) {
                    $scope.currentItem = response.data;
                });
            };

            $scope.addItem = function () {
                var url = _basePath + "/api/userperformance/addItem";
                $http.post(url, $scope.currentItem).then(function (response) {
                    if (response.data.successMsg) {
                        location.reload();
                    }
                });
            };

            $scope.removeItem = function (itemId) {
                bootbox.confirm("Are you sure to remove this performance?", function (result) {
                    if (result) {
                        var url = _basePath + "/api/userperformance/removeItem?itemId=" + itemId;
                        $http.post(url).then(function (response) {
                            location.reload();
                        });
                    }
                });
            };

            $scope.init();
        });
}());