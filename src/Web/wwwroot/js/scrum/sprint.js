$(function () {
    $.validate();
});

(function () {
    angular.module("sprint", ['ui.bootstrap', 'myFilters'])
    .controller("sprintCtrl", function ($scope, $http) {
        $scope.groupId = _groupId;
        $scope.sprintId = _sprintId;
        $scope.sprint = {
            DateRange: {}
        };

        $scope.isCollapsed = {};

        $scope.rowCollection = [];
        $scope.selectedTasks = [];

        $scope.isStartDateOpen = false;
        $scope.isEndDateOpen = false;

        $scope.getTasks = function () {
            if ($scope.sprint.DateRange.StartDate && $scope.sprint.DateRange.EndDate) {
                var url = _basePath + "/api/project/getTasks?groupId=" + $scope.groupId + "&startDate=" + utility.getDateString($scope.sprint.DateRange.StartDate) +
                    "&endDate=" + utility.getDateString($scope.sprint.DateRange.EndDate);
                $http.post(url).then(function (response) {
                    $scope.rowCollection = response.data;

                    $.each($scope.rowCollection, function (index, element) {
                        $scope.isCollapsed[element.ProjectId] = false;
                    });

                    if ($scope.sprintId) {
                        $scope.selectedTasks = [];
                        $.each($scope.rowCollection, function (rIndex, row) {
                            $.each(row.Tasks, function (tIndex, task) {
                                if (_.findIndex($scope.sprint.TaskIds, { Key: row.ProjectId, Value: task.Name }) !== -1) {
                                    task.IsSelected = true;
                                    $scope.selectedTasks.push({
                                        ProjectId: row.ProjectId,
                                        TaskName: task.Name
                                    });
                                }
                            });
                        });
                    }
                });
            }
        }

        $scope.refreshSelectedTasks = function () {
            $scope.selectedTasks = [];
            $.each($scope.rowCollection, function (rIndex, row) {
                $.each(row.Tasks, function (tIndex, task) {
                    if(task.IsSelected){
                        $scope.selectedTasks.push({
                            ProjectId: row.ProjectId,
                            TaskName: task.Name
                        });
                    }
                });
            });
        }

        if ($scope.sprintId) {
            var url = _basePath + "/api/scrum/getSprint?sprintId=" + encodeURIComponent($scope.sprintId);
            $http.post(url).then(function(response){
                $scope.sprint = response.data;

                if (typeof $scope.sprint.DateRange.StartDate === "string") {
                    $scope.sprint.DateRange.StartDate = new Date($scope.sprint.DateRange.StartDate);
                }

                if (typeof $scope.sprint.DateRange.EndDate === "string") {
                    $scope.sprint.DateRange.EndDate = new Date($scope.sprint.DateRange.EndDate);
                }

                $scope.getTasks();
            });
        }

        $scope.collapse = function (projectId) {
            $scope.isCollapsed[projectId] = !$scope.isCollapsed[projectId];
        };

        $scope.openStartDate = function () {
            $scope.isStartDateOpen = !$scope.isStartDateOpen;
        }

        $scope.openEndDate = function () {
            $scope.isEndDateOpen = !$scope.isEndDateOpen;
        }
    });
}());