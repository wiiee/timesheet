$(function () {
    $.validate();
    $(document).tooltip();
    $("textarea").textareaAutoSize();
});

(function () {
    angular.module("project", ['ui.bootstrap', 'myFilters'])
    .controller("projectCtrl", function ($scope, $http) {
        $scope.projectId = _projectId;
        $scope.project = {
            Tasks: []
        };
        $scope.oriProject = {};
        $scope._isPublic = false;
        $scope.taskIndex = 0;
        $scope.hours = {};
        $scope.phases = _phases;
        $scope.statuses = _statuses;
        $scope.levels = _levels;
        $scope.groups = [];
        $scope.owners = [];
        $scope.isOpen = {
            publishDate: false
        };

        $scope.addTask = function () {
            $scope.isOpen[$scope.taskIndex] = {
                isStartDateOpen: false,
                isEndDateOpen: false
            };

            $scope.project.Tasks.push({
                Id: $scope.taskIndex,
                isTemplate: $scope.isTemplate,
                isEdit: true,
                isDelete: true,
                PlanDateRange: {}
            });

            $scope.taskIndex++;

            setTimeout(function () {
                $.validate();
            }, 300);
        }

        $scope.initProject = function () {
            if ($scope.projectId) {
                var url = _basePath + "/api/project/" + $scope.projectId;
                $http.get(url).then(function (response) {
                    $scope.project = response.data;
                    $scope.oriProject = angular.copy($scope.project);
                    $scope.taskIndex = _.max(_.pluck($scope.project.Tasks, "Id")) + 1;

                    $scope.initTemplate();
                });
            }
            else
            {
                $scope.initTemplate();
            }
        };

        $scope.initTemplate = function () {
            var url = _basePath + "/api/project/getTaskTemplates";
            $http.post(url).then(function (response) {
                $scope.templates = response.data;
                $scope.isTemplate = $scope.templates.length > 0;

                if ($scope.isTemplate) {
                    $.each($scope.templates, function (index, element) {
                        $scope.hours[element.Name] = element.Hours;
                    });

                    $scope.templates.push({
                        Name: "Other"
                    });
                }

                if (!$scope.projectId) {
                    $scope.addTask();
                }
                else {
                    $.each($scope.project.Tasks, function (index, element) {
                        $scope.isOpen[index] = {
                            isStartDateOpen: false,
                            isEndDateOpen: false
                        };

                        if ($scope.templates.length > 0 && _.findIndex($scope.templates, { Name: element.Name }) !== -1) {
                            element.isTemplate = true;
                        }
                        else {
                            element.isTemplate = false;
                        }

                        if (element.UserId === _userId || _userType !== "User") {
                            element.isEdit = true;
                            element.isDelete = true;
                        }
                        else {
                            element.isEdit = false;
                            element.isDelete = false;
                        }

                        if (element.Value === 0) {
                            element.Value = element.PlanHour;
                        }
                    });
                }
            });
        };

        $scope.initUsers = function () {
            var url = _basePath + "/api/user/getGroups?projectId=" + encodeURIComponent($scope.projectId);
            $http.post(url).then(function (response) {
                $scope.groups = response.data;
            });
        };

        $scope.initOwners = function () {
            var url = _basePath + "/api/user/getOwners?projectId=" + encodeURIComponent($scope.projectId);
            $http.post(url).then(function (response) {
                $scope.owners = response.data;
                setTimeout(function () {
                    $("#ownerIds").multiselect({
                        enableFiltering: true,
                        buttonWidth: "100%",
                        onChange: function (option, checked) {
                            $("#ownerIds").blur();
                            //$("form").isValid();
                        }
                    });
                }, 300);
            });
        };

        $scope.init = function () {
            $scope.initUsers();
            $scope.initOwners();
            $scope.initProject();
        };

        $scope.init();

        $scope.changeTaskName = function (task) {
            if (task.Name === "Other") {
                task.isTemplate = false;
                task.Name = "";
            }
            else {
                var template = _.findWhere($scope.templates, {
                    Name: task.Name
                });

                if (template) {
                    task.Phase = template.Phase;
                }
            }
        };

        $scope.changePlanValue = function (task) {
            task.Value = task.PlanHour;
        };

        $scope.removeTask = function (id) {
            $scope.project.Tasks = _.reject($scope.project.Tasks, { Id: id });
        };

        $scope.openCalendar = function (first, second) {
            if (second) {
                $scope.isOpen[first][second] = !$scope.isOpen[first][second];
            }
            else
            {
                $scope.isOpen[first] = !$scope.isOpen[first];
            }
        };

        $scope.getUserName = function (userId) {
            if ($scope.groups.length === 0) {
                return "";
            }
            var users = _.reduceRight(_.pluck($scope.groups, "Users"), function (a, b) { return a.concat(b); }, []);
            return _.findWhere(users, { Id: userId }).Name;
        };

        $scope.isPublic = function () {
            $scope._isPublic = !$scope._isPublic;
            
            if ($scope._isPublic)
            {
                $("#userIds").removeAttr('data-validation');
                $("#ownerIds").removeAttr('data-validation');
            }
            else
            {
                $("#userIds").attr('data-validation', 'required');
                $("#ownerIds").attr('data-validation', 'required');
            }
        };

        $scope.reset = function ($event) {
            $event.preventDefault();
            $event.stopPropagation();
            $scope.project = angular.copy($scope.oriProject);
        };

        $scope.submit = function () {
            if ($("form").isValid()) {
                if ($scope.projectId) {
                    var url = _basePath + "/api/Project";
                    $http.post(url, $scope.project).then(function (response) {
                        location = _basePath + "/Project/Index";
                    });
                }
                else {
                    var url = _basePath + "/api/Project";
                    $http.put(url, $scope.project).then(function (response) {
                        location = _basePath + "/Project/Index";
                    });
                }
            }
        };
    });
}());