(function () {
    angular.module('flightWeeklyReport', ['ui.bootstrap', 'myFilters'])
        .controller('projectCtrl', ['$scope', function ($scope) {
            $scope.searchDateRange = _searchDateRange;
            $scope.isStartDateOpen = false;
            $scope.isEndDateOpen = false;
            $scope.models = _models;

            $scope.openStartDate = function () {
                $scope.isStartDateOpen = !$scope.isStartDateOpen;
            }

            $scope.openEndDate = function () {
                $scope.isEndDateOpen = !$scope.isEndDateOpen;
            }

            $scope.previousWeek = function () {
                var startdate = new Date($scope.searchDateRange.StartDate);
                $scope.searchDateRange.StartDate = startdate.setDate(startdate.getDate() - 7);
                var enddate = new Date($scope.searchDateRange.EndDate);
                $scope.searchDateRange.EndDate = enddate.setDate(enddate.getDate() - 7);
                redirectToNewDate();
            };

            $scope.nextWeek = function () {
                var startdate = new Date($scope.searchDateRange.StartDate);
                $scope.searchDateRange.StartDate = startdate.setDate(startdate.getDate() + 7);
                var enddate = new Date($scope.searchDateRange.EndDate);
                $scope.searchDateRange.EndDate = enddate.setDate(enddate.getDate() + 7);
                redirectToNewDate();
            };

            function redirectToNewDate() {
                var url = _basePath + "/Report/WeeklyReport?isShowImportant=true"
                    + "&startDate=" + new Date($scope.searchDateRange.StartDate).toLocaleDateString()
                    + "&endDate=" + new Date($scope.searchDateRange.EndDate).toLocaleDateString();

                // similar behavior as clicking on a link
                window.location.href = url;
            }
            
            //导出周报
            $scope.exportData = function () {
                utility.exportData($scope.models, $scope.searchDateRange, "FlightWeeklyReport");
            }
            
        }]);
}());