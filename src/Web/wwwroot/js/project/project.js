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
        $scope.taskValues = {};
        $scope.phases = _phases;
        $scope.statuses = _statuses;
        $scope.levels = _levels;
        $scope.userId = _userId;
        $scope.userType = _userType;
        $scope.groups = [];
        $scope.users = {};
        $scope.owners = [];
        $scope.isOpen = {
            publishDate: false
        };
        $scope.reviewTasks = [];

        $scope.isSubmitted = false;

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
                PlanDateRange: {},
                Class: "glyphicon glyphicon-chevron-down"
            });

            $scope.taskIndex++;

            setTimeout(function () {
                $.validate();
            }, 300);
        };

        $scope.initProject = function () {
            if ($scope.projectId) {
                var url = _basePath + "/api/project/" + $scope.projectId;
                $http.get(url).then(function (response) {
                    $scope.project = response.data;
                    $scope.oriProject = angular.copy($scope.project);
                    $scope.taskIndex = _.max(_.pluck($scope.project.Tasks, "Id")) + 1;
                    $scope.isPublic();

                    if ($scope.project.Tasks !== null) {
                        $.each($scope.project.Tasks, function (index, element) {
                            element.Class = "glyphicon glyphicon-chevron-down";
                        });
                    }
                    $scope.initTemplate();
                });

                $scope.initTaskValues($scope.projectId);

                url = _basePath + "/api/project/getReviewTasks?projectId=" + $scope.projectId;
                $http.post(url).then(function (response) {
                    $scope.reviewTasks = response.data;
                });
            }
            else
            {
                $scope.initTemplate();
            }
        };

        $scope.initTaskValues = function (projectId) {
            var url = _basePath + "/api/project/getTaskValues?projectId=" + encodeURIComponent($scope.projectId);
            $http.post(url).then(function (response) {
                $scope.taskValues = response.data;
            });
        };

        $scope.initTemplate = function () {
            var url = _basePath + "/api/project/getTaskTemplates";
            $http.post(url).then(function (response) {
                $scope.templates = response.data;
                $scope.isTemplate = $scope.project.Tasks !== null && $scope.templates.length > 0;

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
                else if ($scope.project.Tasks !== null) {
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
                    });
                }
            });
        };

        $scope.initUsers = function () {
            var url = _basePath + "/api/user/GetUserNames";
            $http.post(url).then(function (response) {
                $scope.users = response.data;
            });
        };

        $scope.initGroups = function () {
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
            $scope.initGroups();
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

        $scope.changeUser = function (task) {
            if(!task.UserId){
                task.Values = {};
            }

            //不存在或者换到另外一个组的时候，就初始化
            if (!task.Values || !_.contains(_.values(task.Values), task.UserId)) {
                $scope.initTaskValues(task);
            }
        };

        $scope.initTaskValues = function (task) {
            var oriValues = task.Values ? angular.copy(task.Values) : {};
            if (!task.Values || task.Status === 0) {
                oriValues[$scope.userId] = task.PlanHour;
            }
            task.Values = {};

            $.each($scope.groups, function (index, element) {
                var ids = _.pluck(element.Users, 'Id');
                if(_.contains(ids, task.UserId)) {
                    $.each(ids, function (i, e) {
                        task.Values[e] = oriValues[e] ? oriValues[e] : 0;
                    });

                    //退出循环
                    return false;
                }
            });
        };

        //编辑时修改value值
        $scope.changePlanValue = function (task) {
            var ids = _.keys(task.Values);
            if (_.contains(ids, $scope.userId)) {
                task.Values[$scope.userId] = task.PlanHour;
            }
        };

        $scope.collapseTask = function (task) {
            if (task.Class === "glyphicon glyphicon-chevron-down") {
                task.Class = "glyphicon glyphicon-chevron-up";
            }
            else {
                task.Class = "glyphicon glyphicon-chevron-down";
            }
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
            //if ($scope.groups.length === 0) {
            //    return "";
            //}
            //var users = _.reduceRight(_.pluck($scope.groups, "Users"), function (a, b) { return a.concat(b); }, []);
            //return _.findWhere(users, { Id: userId }).Name;
            return $scope.users[userId];
        };

        $scope.isUserExist = function (userId) {
            if (userId === undefined) {
                return true;
            }

            if ($scope.groups.length === 0) {
                return false;
            }

            var users = _.reduceRight(_.pluck($scope.groups, "Users"), function (a, b) { return a.concat(b); }, []);

            return _.findWhere(users, { Id: userId }) !== undefined;
        };

        $scope.isPublic = function () {
            //$scope._isPublic = !$scope._isPublic;

            if ($scope.project.IsPublic)
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

        $scope.review = function () {
            url = _basePath + "/api/project/reviewTasks";
            $http.post(url, $scope.reviewTasks).then(function (response) {
                if (response.data.successMsg || response.data.errorMsg) {
                    var fieldName = response.data.successMsg ? "successMsg=" : "errorMsg=";
                    var fieldValue = response.data.successMsg ? response.data.successMsg : response.data.errorMsg;
                    location = _basePath + "/Project/Project?projectId=" + encodeURIComponent($scope.projectId) + "&" + fieldName + encodeURIComponent(fieldValue);
                }
            });
        };

        $scope.submit = function () {
            if ($("form").isValid()) {
                if (!$scope.isSubmitted) {
                    $scope.isSubmitted = true;
                    var url = _basePath + "/api/Project";

                    if ($scope.projectId) {
                        $http.post(url, $scope.project).then(function (response) {
                            if (response.data.successMsg || response.data.errorMsg) {
                                var fieldName = response.data.successMsg ? "successMsg=" : "errorMsg=";
                                var fieldValue = response.data.successMsg ? response.data.successMsg : response.data.errorMsg;
                                location = _basePath + "/Project/Project?projectId=" + encodeURIComponent($scope.projectId) + "&" + fieldName + encodeURIComponent(fieldValue);
                            }
                        });
                    }
                    else {
                        $http.put(url, $scope.project).then(function (response) {
                            if (response.data.successMsg || response.data.errorMsg) {
                                var fieldName = response.data.successMsg ? "successMsg=" : "errorMsg=";
                                var fieldValue = response.data.successMsg ? response.data.successMsg : response.data.errorMsg;
                                var projectId = response.data.successMsg.substring(response.data.successMsg.indexOf("(")+1, response.data.successMsg.indexOf(":"));
                                location = _basePath + "/Project/Project?projectId=" + encodeURIComponent(projectId) + "&" + fieldName + encodeURIComponent(fieldValue);
                            }
                        });
                    }
                }

            }
        };
    });
}());