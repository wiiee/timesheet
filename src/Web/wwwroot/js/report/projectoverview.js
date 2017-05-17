$(function () {
    googleChart.init(['corechart'], drawCharts);

    function drawCharts() {
        var lineDt = new google.visualization.DataTable();
        lineDt.addColumn('string', 'Date');
        lineDt.addColumn('number', 'Hour')

        $.each(_model.Burndown.Names, function (index, value) {
            lineDt.addRow([value, _model.Burndown.Items[0].Values[index]]);
        });

        var lineOption = {
            title: "Chart",
            legend: "none"
        };

        googleChart.drawLineChart("lineChart", lineDt, lineOption);

        var overallDt = new google.visualization.DataTable();
        overallDt.addColumn('string', 'Name');
        overallDt.addColumn('number', 'Hour')

        $.each(_model.Hours, function (property, value) {
            overallDt.addRow([property, value.Actual]);
        });

        var overallOption = {
            title: 'Overall Hours Distribution',
            legend: 'right',
            pieHole: 0.3
        };

        googleChart.drawPieChart("overallPieChart", overallDt, overallOption);

        var searchDt = new google.visualization.DataTable();
        searchDt.addColumn('string', 'Name');
        searchDt.addColumn('number', 'Hour')

        $.each(_model.Hours, function (property, value) {
            searchDt.addRow([property, value.Search]);
        });

        var searchOption = {
            title: 'Search Hours Distribution',
            pieHole: 0.3
        };

        googleChart.drawPieChart("searchPieChart", searchDt, searchOption);
    }
});

(function () {
    angular.module('projectOverview', ['ui.bootstrap', 'myFilters'])
        .controller('projectOverviewCtrl', function ($scope, $modal, $http) {
            $scope.searchDateRange = _searchDateRange;
            $scope.model = _model;
            $scope.murmurs = _murmurs ? _murmurs : [];
            $scope.isStartDateOpen = false;
            $scope.isEndDateOpen = false;

            $scope.openStartDate = function () {
                $scope.isStartDateOpen = !$scope.isStartDateOpen;
            };

            $scope.openEndDate = function () {
                $scope.isEndDateOpen = !$scope.isEndDateOpen;
            };

            $scope.isShowSearchHours = function () {
                return _.reduce($scope.model.Hours, function (memo, hour) { return memo + hour.Search; }, 0) > 0;
            };

            $scope.$on('murmur', function (event, data) {
                var index = _.findIndex($scope.murmurs, { Id: data.Id });
                if (index >= 0) {
                    $scope.murmurs[index] = data;
                }
                else {
                    $scope.murmurs.push(data);
                }
            });

            $scope.deleteMurmur = function (projectId, tick) {
                bootbox.confirm("Are you sure to delete this murmur?", function (result) {
                    if (result) {
                        var url = _basePath + "/api/project/DeleteMurmur?projectId=" + encodeURIComponent(projectId) + "&tick=" + encodeURIComponent(tick);
                        $http.post(url).then(function (response) {
                            $scope.murmurs = _.reject($scope.murmurs, function (element) {
                                return element.Id === tick;
                            });
                        });
                    }
                });
            };

            $scope.open = function (size, userId, projectId, tick, content) {
                var modalInstance = $modal.open({
                    templateUrl: 'murmur.html',
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
                            return tick;
                        },
                        content: function () {
                            return content;
                        }
                    }
                });
            };
        })
        .controller('ModalInstanceCtrl', function ($rootScope, $scope, $modalInstance, $http, userId, projectId, tick, content) {
            $scope.content = content;

            setTimeout(function () {
                $("textarea").textareaAutoSize();
            }, 300);

            $scope.ok = function () {
                if ($scope.content) {
                    var url = _basePath + "/api/project/Murmur?userId=" + encodeURIComponent(userId) + "&projectId=" + encodeURIComponent(projectId) + "&tick=" + tick + "&content=" + encodeURIComponent($scope.content);
                    $http.post(url).then(function (response) {
                        if (!response.data.errorMsg) {
                            var data = {
                                Id: response.data.Id,
                                Time: response.data.Time,
                                UserId: userId,
                                UserName: _userName,
                                Content: $scope.content
                            };

                            $rootScope.$broadcast("murmur", data);
                        }
                    });
                }

                $modalInstance.close($scope.comment);
            };

            $scope.cancel = function () {
                $modalInstance.dismiss('cancel');
            };
        });
}());