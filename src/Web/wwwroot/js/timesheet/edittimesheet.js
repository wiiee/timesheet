$(function () {
    $.validate();
    $(document).tooltip();
});

(function () {
    angular.module("timeSheet", ['ui.bootstrap', 'myFilters', 'angular.filter'])
    .controller("TimeSheetController", ['$scope', '$http', '$modal', function ($scope, $http, $modal) {
        $scope.rowCollection = _rowCollection;
        $scope.oriRowCollection = angular.copy(_rowCollection);
        $scope.murmurs = _murmurs || [];
        $scope.currentMurmurs = [];
        $scope.currentTick = 0;
        $scope.buttonText = _buttonText;
        $scope.currentProjectId;
        $scope.weekDays = [0,1,2,3,4,5,6];
        $scope.isSelectAll = false;
        $scope.isCollapsed = {};

        $.each($scope.rowCollection, function (index, element) {
            $scope.isCollapsed[element.ProjectId] = false;
        });

        $scope.collapse = function (projectId) {
            $scope.isCollapsed[projectId] = !$scope.isCollapsed[projectId];
        };

        $scope.isShowTable = function (projectId) {
            return _.indexOf(_.values(_.pluck($scope.rowCollection, "IsSelected")), true) != -1;
        };

        $scope.getColumn = function (column) {
            var total = 0;

            $.each($scope.rowCollection, function (index, value) {
                if (value.IsSelected) {
                    if ($.isNumeric(value.Week[column])) {
                        total += parseFloat(value.Week[column]);
                    }
                }
            });

            return total;
        };

        $scope.getTotal = function () {
            var total = 0;

            $.each($scope.rowCollection, function (index, value) {
                if (value.IsSelected)
                {
                    var result = _.reduce(value.Week, function (memo, num) {
                        var number = $.isNumeric(num) ? parseFloat(num) : 0
                        return memo + number;
                    }, 0);
                    total += result;
                }
            });

            return total;
        }

        $scope.selectAll = function () {
            $scope.isSelectAll = !$scope.isSelectAll;

            $.each($scope.rowCollection, function (index, value) {
                if (!value.IsDone) {
                    value.IsSelected = $scope.isSelectAll;
                }
            });
        };

        $scope.reset = function ($event) {
            $event.preventDefault();
            $event.stopPropagation();
            $scope.rowCollection = angular.copy($scope.oriRowCollection);
        };

        $scope.$on('currentMurmurs', function (event, data) {
            var index = _.findIndex($scope.currentMurmurs, { Id: data.Id });
            if (index >= 0) {
                $scope.currentMurmurs[index] = data;
            }
            else {
                $scope.currentMurmurs.push(data);
            }
            updateGlobalMurmurs();
        });

        $scope.$on('updateMurmurs', function (event, data) {
            $scope.currentMurmurs = data;
            updateGlobalMurmurs();
        });

        function updateGlobalMurmurs() {
            for (var i = 0; i < $scope.murmurs.length; i++) {
                if ($scope.murmurs[i].Id === $scope.currentProjectId) {
                    $scope.murmurs[i].Murmurs = $scope.currentMurmurs;
                    break;
                }
            }
        }

        $scope.open = function (size, userId, projectId) {
            $scope.currentProjectId = projectId;
            var modalInstance = $modal.open({
                templateUrl: 'murmurs.html',
                controller: 'ModalInstanceCtrl',
                size: size,
                resolve: {
                    userId: function () {
                        return userId;
                    },
                    projectId: function () {
                        return projectId;
                    },
                    tick: function () {
                        return 0;
                    },
                    content: function () {
                        return "";
                    },
                    murmurs: function () {
                        for (var i = 0; i < $scope.murmurs.length; i++) {
                            if ($scope.murmurs[i].Id === projectId) {
                                $scope.currentMurmurs = $scope.murmurs[i].Murmurs || [];
                                return $scope.currentMurmurs;
                            }
                        }
                    },
                    buttonText: function () {
                        return $scope.buttonText;
                    }
                }
            });
        };
    }])
    .controller('ModalInstanceCtrl', function ($rootScope, $scope, $modalInstance, $http, userId, projectId, tick, content, murmurs, buttonText) {
        $scope.currentMurmurs = murmurs;
        $scope.buttonText = buttonText;

        setTimeout(function () {
            $("textarea").textareaAutoSize();
        }, 300);

        $scope.ok = function () {
            if ($scope.content) {
                var tickVal = tick;
                if($scope.currentTick > 0) {
                    tickVal = $scope.currentTick;
                }
                var url = _basePath + "/api/project/Murmur?userId=" + encodeURIComponent(userId) + "&projectId=" + encodeURIComponent(projectId) + "&tick=" +tickVal + "&content=" +encodeURIComponent($scope.content);
                $http.post(url).then(function (response) {
                    if (!response.data.errorMsg) {
                        var data = {
                            Id: response.data.Id,
                            ProjectId: $scope.currentProjectId,
                            Time: response.data.Time,
                            UserId: userId,
                            UserName: _userName,
                            Content: $scope.content
                        };
                        $rootScope.$broadcast("currentMurmurs", data);
                        initModal();
                    }
                });
            }
        };

        $scope.editMurmur = function (projectId, tick, context) {
            $scope.currentTick = tick;
            $scope.content = context;
            $scope.buttonText = "Save";
        };

        function initModal() {
            $scope.buttonText = "Add";
            $scope.currentTick = 0;
            $scope.content = "";
        }

        $scope.deleteMurmur = function (projectId, tick) {
            bootbox.confirm("Are you sure to delete this murmur?", function (result) {
                if (result) {
                    var url = _basePath + "/api/project/DeleteMurmur?projectId=" + encodeURIComponent(projectId) + "&tick=" + encodeURIComponent(tick);
                    $http.post(url).then(function (response) {
                        $scope.currentMurmurs = _.reject($scope.currentMurmurs, function (element) {
                            return element.Id === tick;
                        });
                        $rootScope.$broadcast("updateMurmurs", $scope.currentMurmurs);
                        initModal();
                    });
                }
            });
        };

        $scope.cancel = function () {
            $modalInstance.dismiss('cancel');
        };
        $scope.close = function () {
            $modalInstance.dismiss('close');
        };
    });
}());