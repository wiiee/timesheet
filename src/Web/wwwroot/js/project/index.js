$(function () {
    //$(document).tooltip();
});

(function () {
    angular.module('project', ['smart-table', 'ui.bootstrap', 'myFilters'])
        .controller('projectCtrl', function ($rootScope, $scope, $http, $modal, $timeout) {
            $scope.status = "";
            $scope.rowCollection = [];
            //$scope.displayCollection = [].concat($scope.rowCollection);

            $scope.isCollapsed = true;
            $scope.itemsByPage = _pageSize;
            $scope.searchTexts = ["Show Condition", "Hide Condition"];
            $scope.searchText = $scope.searchTexts[0];
            $scope.currentTextIndex = 0;
            $scope.userNames = [];
            $scope.ownerNames = [];

            $scope.init = function (status) {
                $scope.status = status;

                var url = _basePath + "/api/Project/GetProjectModels?statusText=" + $scope.status;

                $http.post(url).then(function (response) {
                    $scope.rowCollection = response.data;
                    _ajaxProjectSize++;
                    //$rootScope.$broadcast("dataIsReady");

                    if (_ajaxProjectSize === 3) {
                        $timeout(function () {
                            $(".userNames, .ownerNames").multiselect({
                                buttonWidth: "100%"
                            });
                        }, 500);
                    }
                });
            }

            //$scope.$on('dataIsReady', function (event, data) {
            //    $scope.rowCollection = _.filter(_rowCollection, function (row) {
            //        return row.Status === $scope.status;
            //    });
            //});

            $scope.switchSearch = function () {
                $scope.isCollapsed = !$scope.isCollapsed;
                $scope.currentTextIndex = ($scope.currentTextIndex + 1) % 2;
                $scope.searchText = $scope.searchTexts[$scope.currentTextIndex];
            };

            $scope.changeOwnerNames = function () {
                if ($scope.ownerNames.length === 0 || ($scope.ownerNames.length === 1 && $scope.ownerNames[0] === "")) {
                    _.map($scope.rowCollection, function (element) {
                        element.IsShow = true;
                        return element;
                    });
                }
                else {
                    $.each($scope.rowCollection, function (index, value) {
                        var ownerNames = value.OwnerNames.split(",");
                        var union = _.union($scope.ownerNames, ownerNames);

                        if (union.length < ownerNames.length + $scope.ownerNames.length) {
                            value.IsShow = true;
                        }
                        else {
                            value.IsShow = false;
                        }
                    });
                }
            };

            $scope.changeUserNames = function () {
                if ($scope.userNames.length === 0 || ($scope.userNames.length === 1 && $scope.userNames[0] === "")) {
                    _.map($scope.rowCollection, function (element) {
                        element.IsShow = true;
                        return element;
                    });
                }
                else {
                    $.each($scope.rowCollection, function (index, value) {
                        var userNames = value.UserNames.split(",");
                        var union = _.union($scope.userNames, userNames);

                        if (union.length < userNames.length + $scope.userNames.length) {
                            value.IsShow = true;
                        }
                        else {
                            value.IsShow = false;
                        }
                    });
                }
            };

            $scope.changeSize = function () {
                var data = {
                    key: "pageSize",
                    value: $scope.itemsByPage
                };

                $http.post(_basePath + "/api/preference", data);
            };

            $scope.closeProject = function (projectId) {
                bootbox.confirm("Are you sure to close this project?", function (result) {
                    if (result) {
                        var url = _basePath + "/api/Project/CloseProject?projectId=" + encodeURIComponent(projectId);
                        $http.post(url).then(function (response) {
                            if (response.data.successMsg) {
                                var msg = $("#successMsg").html();
                                msg = msg.replace("replaceText", response.data.successMsg);
                                $("#messageContainer").append(msg);

                                $scope.updateProject(projectId);
                            }
                            else if (response.data.errorMsg) {
                                var msg = $("#errorMsg").html();
                                msg = msg.replace("replaceText", response.data.errorMsg);
                                $("#messageContainer").append(msg);
                            }
                        });
                    }
                });
            };

            $scope.deleteProject = function (projectId) {
                bootbox.confirm("Are you sure to delete this project?", function (result) {
                    if (result) {
                        url = _basePath + "/api/Project/" + projectId;
                        $http.delete(url).then(function (response) {
                            if (response.data.successMsg) {
                                var msg = $("#successMsg").html();
                                msg = msg.replace("replaceText", response.data.successMsg);
                                $("#messageContainer").append(msg);

                                var index = _.findIndex($scope.rowCollection, { "Id": projectId });
                                $scope.rowCollection.splice(index, 1);

                                //index = _.findIndex($scope.displayCollection, { "Id": projectId });
                                //$scope.displayCollection.splice(index, 1);
                            }
                            else if (response.data.errorMsg) {
                                var msg = $("#errorMsg").html();
                                msg = msg.replace("replaceText", response.data.errorMsg);
                                $("#messageContainer").append(msg);
                            }
                        });
                    }
                });
            };

            $scope.updateProject = function (projectId) {
                var url = _basePath + "/api/Project/GetProjectModel?projectId=" + encodeURIComponent(projectId);

                $http.post(url).then(function (response) {
                    var index = _.findIndex($scope.rowCollection, { "Id": projectId });
                    $scope.rowCollection[index] = response.data;
                    $scope.rowCollection = [].concat($scope.rowCollection);

                    //index = _.findIndex($scope.rowCollection, { "Id": projectId });
                    //$scope.rowCollection[index] = response.data;
                });
            };

            $scope.$on('updateProject', function (event, data) {
                $scope.updateProject(data);
            });

            $scope.editProject = function (size, projectId) {
                var data = {};

                var modalInstance = $modal.open({
                    templateUrl: 'editProject.html',
                    controller: 'editProjectCtrl',
                    size: size,
                    resolve: {
                        projectRs: function () {
                            var url = _basePath + "/api/project/getProject?projectId=" + encodeURIComponent(projectId);
                            return $http.post(url);
                        },
                        ownersRs: function () {
                            var url = _basePath + "/api/user/getOwners?projectId=" + encodeURIComponent(projectId);
                            return $http.post(url);
                        },
                        groupsRs: function () {
                            var url = _basePath + "/api/user/getGroups?projectId=" + encodeURIComponent(projectId);
                            return $http.post(url);
                        },
                        templatesRs: function () {
                            var url = _basePath + "/api/project/getTaskTemplates";
                            return $http.post(url);
                        }
                    }
                });

                //modalInstance.result.then(function (result) {
                //    $log.info('Modal close at: ' + new Date());
                //}, function (reason) {
                //    $log.info('Modal dismissed at: ' + new Date());
                //});
            };

            $scope.editForUser = function (size, projectId) {
                var data = {};

                var modalInstance = $modal.open({
                    templateUrl: 'editForUser.html',
                    controller: 'editForUserCtrl',
                    size: size,
                    resolve: {
                        projectRs: function () {
                            var url = _basePath + "/api/project/getProject?projectId=" + encodeURIComponent(projectId);
                            return $http.post(url);
                        }
                    }
                });
            };

            $scope.postponeProject = function (size, projectId) {
                var data = {};

                var modalInstance = $modal.open({
                    templateUrl: 'postponeProject.html',
                    controller: 'postponeProjectCtrl',
                    size: size,
                    resolve: {
                        projectRs: function () {
                            var url = _basePath + "/api/project/getProject?projectId=" + encodeURIComponent(projectId);
                            return $http.post(url);
                        },
                        projectId: function () {
                            return projectId;
                        }
                    }
                });
            };
        })
        .directive('pageSelect', function () {
            return {
                restrict: 'E',
                template: '<input type="text" class="form-control" ng-model="inputPage" ng-change="selectPage(inputPage)">',
                link: function (scope, element, attrs) {
                    scope.$watch('currentPage', function (c) {
                        scope.inputPage = c;
                    });
                }
            }
        })
        .directive('stDateRange', ['$timeout', function ($timeout) {
            return {
                restrict: 'E',
                require: '^stTable',
                scope: {
                    before: '=',
                    after: '=',
                    beforepredicate: '=',
                    afterpredicate: '=',
                    name: '=',
                    text: '='
                },
                templateUrl: '../template/stDateRange.html',

                link: function (scope, element, attr, table) {
                    var beforepredicate = attr.beforepredicate;
                    var afterpredicate = attr.afterpredicate;
                    var name = beforepredicate + afterpredicate;
                    scope.beforepredicate = beforepredicate;
                    scope.afterpredicate = afterpredicate;
                    scope.name = name;
                    scope.text = attr.labeltext;

                    var inputs = element.find('input');
                    var inputBefore = angular.element(inputs[0]);
                    var inputAfter = angular.element(inputs[1]);

                    [inputBefore, inputAfter].forEach(function (input) {
                        input.bind('blur', function () {
                            var query = {};

                            if (!scope.isBeforeOpen && !scope.isAfterOpen) {

                                if (scope.before && scope.before[name]) {
                                    query.before = scope.before[name];
                                }


                                if (scope.after && scope.after[name]) {
                                    query.after = scope.after[name];
                                }

                                scope.$apply(function () {
                                    table.search(query, beforepredicate);
                                });

                                scope.$apply(function () {
                                    table.search(query, afterpredicate);
                                });
                            }
                        });
                    });

                    function open(before) {
                        return function ($event) {
                            $event.preventDefault();
                            $event.stopPropagation();

                            if (before) {
                                scope.isBeforeOpen = !scope.isBeforeOpen;
                            } else {
                                scope.isAfterOpen = !scope.isAfterOpen;
                            }
                        }
                    }

                    scope.openBefore = open(true);
                    scope.openAfter = open();
                }
            }
        }])
        .directive('stDateRangeAlone', ['$timeout', function ($timeout) {
            return {
                restrict: 'E',
                require: '^stTable',
                scope: {
                    before: '=',
                    after: '=',
                    predicateName: '=',
                    isBefore: '=',
                    isAfter: '='
                },
                templateUrl: '../template/stDateRangeAlone.html',

                link: function (scope, element, attr, table) {
                    var predicateName = attr.predicate;
                    scope.predicateName = predicateName;

                    if (attr.before) {
                        scope.isBefore = true;
                    }

                    if (attr.after) {
                        scope.isAfter = true;
                    }

                    var inputs = element.find('input');
                    var inputBefore = angular.element(inputs[0]);
                    var inputAfter = angular.element(inputs[1]);

                    [inputBefore, inputAfter].forEach(function (input) {
                        input.bind('blur', function () {
                            var query = {};

                            if (!scope.isBeforeOpen && !scope.isAfterOpen) {

                                if (scope.before && scope.before[predicateName]) {
                                    query.before = scope.before[predicateName];
                                }

                                if (scope.after && scope.after[predicateName]) {
                                    query.after = scope.after[predicateName];
                                }

                                scope.$apply(function () {
                                    table.search(query, predicateName);
                                })
                            }
                        });
                    });

                    function open(before) {
                        return function ($event) {
                            $event.preventDefault();
                            $event.stopPropagation();

                            if (before) {
                                scope.isBeforeOpen = !scope.isBeforeOpen;
                            } else {
                                scope.isAfterOpen = !scope.isAfterOpen;
                            }
                        }
                    }

                    scope.openBefore = open(true);
                    scope.openAfter = open();
                }
            }
        }])
        .directive('stNumberRange', ['$timeout', function ($timeout) {
            return {
                restrict: 'E',
                require: '^stTable',
                scope: {
                    lower: '=',
                    higher: '=',
                    predicateName: '=',
                    text: '='
                },
                templateUrl: '../template/stNumberRange.html',
                link: function (scope, element, attr, table) {
                    var inputs = element.find('input');
                    var inputLower = angular.element(inputs[0]);
                    var inputHigher = angular.element(inputs[1]);
                    var predicateName = attr.predicate;
                    scope.predicateName = predicateName;
                    scope.text = attr.labeltext;

                    [inputLower, inputHigher].forEach(function (input, index) {

                        input.bind('blur', function () {
                            var query = {};

                            if (scope.lower && scope.lower[predicateName]) {
                                query.lower = scope.lower[predicateName];
                            }

                            if (scope.higher && scope.higher[predicateName]) {
                                query.higher = scope.higher[predicateName];
                            }

                            scope.$apply(function () {
                                table.search(query, predicateName)
                            });
                        });
                    });
                }
            };
        }])
        .directive("stResetSearch", function () {
            return {
                restrict: 'EA',
                require: '^stTable',
                link: function (scope, element, attrs, ctrl) {
                    return element.bind('click', function () {
                        return scope.$apply(function () {
                            var tableState;
                            tableState = ctrl.tableState();
                            tableState.search.predicateObject = {};
                            tableState.pagination.start = 0;
                            return ctrl.pipe();
                        });
                    });
                }
            };
        })
        .filter('customFilter', function ($filter) {
            var filterFilter = $filter('filter');
            var standardComparator = function standardComparator(obj, text) {
                text = ('' + text).toLowerCase();
                return ('' + obj).toLowerCase().indexOf(text) > -1;
            };

            return function customFilter(array, expression) {
                function customComparator(actual, expected) {
                    var isBeforeActivated = expected.before;
                    var isAfterActivated = expected.after;
                    var isLower = expected.lower;
                    var isHigher = expected.higher;
                    var higherLimit;
                    var lowerLimit;
                    var itemDate;
                    var queryDate;

                    if (angular.isObject(expected)) {

                        //date range
                        if (expected.before || expected.after) {
                            try {
                                if (isBeforeActivated) {
                                    higherLimit = expected.before;

                                    itemDate = new Date(actual);
                                    queryDate = new Date(higherLimit);

                                    if (itemDate > queryDate) {
                                        return false;
                                    }
                                }

                                if (isAfterActivated) {
                                    lowerLimit = expected.after;

                                    itemDate = new Date(actual);
                                    queryDate = new Date(lowerLimit);

                                    if (itemDate < queryDate) {
                                        return false;
                                    }
                                }

                                return true;
                            } catch (e) {
                                return false;
                            }

                        } else if (isLower || isHigher) {
                            //number range
                            if (isLower) {
                                higherLimit = expected.lower;

                                if (actual > higherLimit) {
                                    return false;
                                }
                            }

                            if (isHigher) {
                                lowerLimit = expected.higher;
                                if (actual < lowerLimit) {
                                    return false;
                                }
                            }

                            return true;
                        }
                        //etc
                        return true;

                    }
                    return standardComparator(actual, expected);
                }

                var output = filterFilter(array, expression, customComparator);
                return output;
            };
        })
        .controller('editProjectCtrl', function ($rootScope, $scope, $modalInstance, $http, projectRs, ownersRs, groupsRs, templatesRs) {
            $scope.userId = _userId;
            $scope.userType = _userType;
            $scope.levels = _levels;
            $scope.statuses = _statuses;
            $scope.phases = _phases;
            $scope.project = projectRs.data;
            $scope.projectId = $scope.project.Id;
            $scope.groups = groupsRs.data;
            $scope.owners = ownersRs.data;
            $scope.totalHour = 0;
            $scope.userLimits = {};

            $scope.project.Tasks = $scope.project.Tasks ? $scope.project.Tasks : [];

            $scope.templates = templatesRs.data;

            $scope.isTemplate = $scope.templates.length > 0;
            if ($scope.isTemplate) {
                $scope.templates.push({
                    Name: "Other"
                });
            }

            $scope.isOpen = {
                publishDate: false
            };

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

            $scope.taskIndex = _.max(_.pluck($scope.project.Tasks, "Id")) + 1;

            $scope.hours = {};

            $.each($scope.templates, function (index, element) {
                $scope.hours[element.Name] = element.Hours;
            });

            $scope.changePlanValue = function (task) {
                if (task.isNew) {
                    task.Value = task.PlanHour;
                }
            };

            $scope.changeTaskName = function (task) {
                if(task.Name === "Other") {
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

            setTimeout(function() {
                $("#ownerIds").multiselect({
                    enableFiltering: true,
                    buttonWidth: "100%"
                });

                $.validate();

                $("textarea").textareaAutoSize();
            }, 300);

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
                    isNew: true,
                    PlanDateRange: {}
                });

                $scope.taskIndex++;

                setTimeout(function () {
                    $.validate();
                }, 300);
            };

            $scope.removeTask = function (id) {
                $scope.project.Tasks = _.reject($scope.project.Tasks, { Id: id });
            };

            $scope.openCalendar = function (first, second) {
                if(second) {
                    $scope.isOpen[first][second]= !$scope.isOpen[first][second];
                }
                else {
                    $scope.isOpen[first]= !$scope.isOpen[first];
                }
            };

            $scope.ok = function () {
                if(!$("form").isValid()) {
                    return;
                }

                var url = _basePath + "/api/project";
                $http.post(url, $scope.project).then(function(response) {
                    if(response.data.successMsg) {
                        var msg = $("#successMsg").html();
                        msg = msg.replace("replaceText", response.data.successMsg);
                        $("#messageContainer").append(msg);

                        $rootScope.$broadcast("updateProject", $scope.project.Id);
                    }
                    else if (response.data.errorMsg) {
                        var msg = $("#errorMsg").html();
                        msg = msg.replace("replaceText", response.data.errorMsg);
                        $("#messageContainer").append(msg);
                    }
                });

                $modalInstance.close();
            };

            $scope.cancel = function () {
                $modalInstance.dismiss('cancel');
            };

            $scope.getUserName = function (userId) {
                var users = _.reduceRight(_.pluck($scope.groups, "Users"), function (a, b) { return a.concat(b); }, []);
                return _.findWhere(users, { Id: userId }).Name;
            };

        }).controller('postponeProjectCtrl', function ($rootScope, $scope, $modalInstance, $http, projectRs, projectId) {
            $scope.projectId = projectId;
            $scope.project = projectRs.data;
            $scope.postponeReasons = _postponeReasons;
            $scope.comment = $scope.project.Comment;

            var date = new Date($scope.project.PlanDateRange.EndDate);
            $scope.startDate = new Date(date.getFullYear(), date.getMonth(), date.getDate() + 1);
            $scope.endDate = $scope.startDate;

            $scope.isEndDateOpen = false;

            setTimeout(function() {
                $("textarea").textareaAutoSize();
            }, 300);

            $scope.openEndDate = function () {
                $scope.isEndDateOpen = !$scope.isEndDateOpen;
            }

            $scope.ok = function () {
                if(!$("form").isValid()) {
                    var msg = $("#errorMsg").html();
                    msg = msg.replace("replaceText", "Validation failed, please check it.");
                    $("#messageContainer").append(msg);
                    $modalInstance.close();
                    return;
                }

                var url = _basePath + "/api/project/PostponeProject?projectId=" + encodeURIComponent($scope.projectId)
                    + "&postponeReason=" + $scope.postponeReason
                    + "&comment=" + encodeURIComponent($scope.comment)
                    + "&endDate=" + utility.getDateString($scope.endDate);

                $http.post(url).then(function(response) {
                    if(response.data.successMsg) {
                        var msg = $("#successMsg").html();
                        msg = msg.replace("replaceText", response.data.successMsg);
                        $("#messageContainer").append(msg);

                        $rootScope.$broadcast("updateProject", $scope.projectId);
                    }
                    else if (response.data.errorMsg) {
                        var msg = $("#errorMsg").html();
                        msg = msg.replace("replaceText", response.data.errorMsg);
                        $("#messageContainer").append(msg);
                    }
                });

                $modalInstance.close();
            };

            $scope.cancel = function () {
                $modalInstance.dismiss('cancel');
            };
        });
}());