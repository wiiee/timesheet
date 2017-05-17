(function () {
    angular.module('flightWeeklyReport', ['ui.bootstrap', 'myFilters'])
        .controller('projectCtrl', ['$scope', function ($scope) {
            $scope.searchDateRange = _searchDateRange;
            $scope.isStartDateOpen = false;
            $scope.isEndDateOpen = false;

            $scope.openStartDate = function () {
                $scope.isStartDateOpen = !$scope.isStartDateOpen;
            }

            $scope.openEndDate = function () {
                $scope.isEndDateOpen = !$scope.isEndDateOpen;
            }

            $scope.clearLink = function () {
                var projects = $("td a");

                $.each(projects, function (index, element) {
                    $(element).parent().text($(element).text());
                });

                $("#clearLink").remove();
            }

            $scope.previousMonth = function () {
                var startdate = new Date($scope.searchDateRange.StartDate);
                var enddate = new Date($scope.searchDateRange.StartDate);
                $scope.searchDateRange.StartDate = startdate.setMonth(startdate.getMonth() - 1);
                //enddate = enddate.setMonth(enddate.getMonth() - 1);
                $scope.searchDateRange.EndDate = enddate.setDate(enddate.getDate() - 1);
                redirectToNewPeriod();
            };

            $scope.nextMonth = function () {
                var startdate = new Date($scope.searchDateRange.StartDate);
                $scope.searchDateRange.StartDate = startdate.setMonth(startdate.getMonth() + 1);
                var enddate = new Date($scope.searchDateRange.StartDate);
                $scope.searchDateRange.EndDate = enddate.setMonth(enddate.getMonth() + 1);
                $scope.searchDateRange.EndDate = enddate.setDate(enddate.getDate() - 1);
                redirectToNewPeriod();
            };

            function redirectToNewPeriod() {
                var url = _basePath + "/Report/MonthlyReport?isShowImportant=true"
                    + "&startDate=" + new Date($scope.searchDateRange.StartDate).toLocaleDateString()
                    + "&endDate=" + new Date($scope.searchDateRange.EndDate).toLocaleDateString();

                // similar behavior as clicking on a link
                window.location.href = url;
            }
        }]);
}());