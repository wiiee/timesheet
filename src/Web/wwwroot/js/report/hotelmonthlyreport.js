(function () {
    angular.module('hotelWeeklyReport', ['ui.bootstrap', 'myFilters'])
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
                var projects = $("td li");

                $.each(projects, function (index, element) {
                    var text = $(element).text();
                    $(element).text(text);
                });

                $("#clearLink").remove();
            }
        }]);
}());