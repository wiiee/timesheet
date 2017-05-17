$(function () {
    var validate = function () {
        $.validate();
    };

    $("#userIds, #ownerIds").multiselect({
        enableFiltering: true,
        buttonWidth: "100%",
        onChange: function (option, checked) {
            validate();
            $("form").isValid();
        }
    });

    validate();
    $("textarea").textareaAutoSize();
});

(function () {
    angular.module("editProject", ['ui.bootstrap', 'myFilters'])
    .controller("editProjectCtrl", ['$scope', '$http', function ($scope, $http) {
        $scope.isStartDateOpen = false;
        $scope.isEndDateOpen = false;
        $scope.startDate = new Date(_dateRange.StartDate);
        $scope.endDate = new Date(_dateRange.EndDate);
        $scope.planHours = _planHours;
        $scope.userIds = _.keys(_planHours);
        $scope.users = _users;
        $scope.totalHour = 0;
        $scope.userLimits = {};

        $scope.openStartDate = function () {
            $scope.isStartDateOpen = !$scope.isStartDateOpen;
        }

        $scope.openEndDate = function () {
            $scope.isEndDateOpen = !$scope.isEndDateOpen;
        }

        $scope.getWorkingHour = function () {
            if ($scope.endDate && $scope.startDate) {
                $scope.totalHour = utility.getWorkingDays($scope.startDate, $scope.endDate) * 8;

                if ($scope.userLimits.startDate !== $scope.startDate || $scope.userLimits.endDate !== $scope.endDate) {
                    $scope.getLimits();
                }
            }
            else {
                $scope.totalHour = 0;
            }

            return $scope.totalHour;
        };

        $scope.getLimits = function () {
            if ($scope.endDate && $scope.startDate && $scope.userIds) {
                $scope.userLimits.startDate = $scope.startDate;
                $scope.userLimits.endDate = $scope.endDate;
                $.each($scope.userIds, function (index, userId) {
                    if (!$scope.userLimits[userId] || $scope.userLimits[userId].startDate !== $scope.startDate || $scope.userLimits[userId].endDate !== $scope.endDate) {
                        $scope.userLimits[userId] = {};
                        $scope.userLimits[userId].startDate = $scope.startDate;
                        $scope.userLimits[userId].endDate = $scope.endDate;
                        $http.post(_basePath + "/api/project/getPlanHour?userId=" + userId + "&startDate=" + utility.getDateString($scope.startDate) + "&endDate=" + utility.getDateString($scope.endDate))
                            .then(function (response) {
                                var name = "planHours." + userId;
                                var rawHour = ($scope.project.PlanHours && $scope.project.PlanHours[userId]) ? $scope.project.PlanHours[userId] : 0;
                                var limit = Math.round($scope.totalHour * _effortMax + rawHour - response.data);

                                if (limit < 0) {
                                    limit = 0;
                                }

                                $scope.userLimits[userId] = limit;

                                $("input[name='" + name + "']").attr("data-validation-error-msg", "Limit is " + limit + ", no more effort.");
                                $("input[name='" + name + "']").attr("data-validation-allowing", "range[0;" + limit + "]");

                                $("form").isValid();
                            });
                    }
                });
            }
        };

        $scope.getUserName = function (userId) {
            return _.findWhere($scope.users, { id: userId }).name;
        };

        $scope.isShowHour = function () {
            return $scope.endDate && $scope.startDate;
        };
    }]);
}());
