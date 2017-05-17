$(function () {
    googleChart.init(['corechart', 'timeline', 'line'], drawCharts);

    function drawPieChart(id, model, title, isPlan) {
        var data = [];
        data.push(["Name", "Hour"]);

        $.each(model, function (property, value) {
            if (isPlan) {
                data.push(_.findWhere(_pairs, {Id: property}).Value, value.Key);
            }
            else {
                data.push(_.findWhere(_pairs, {Id: property}).Value, value.Value);
            } 
        });

        var option = {
            title: title,
            legend: 'right',
            pieHole: 0.3
        };

        googleChart.drawPieChart(id, google.visualization.arrayToDataTable(data), option);
    }

    // The selection handler.
    // Loop through all items in the selection and concatenate
    // a single message from all of them.
    function selectHandler() {
        var selection = googleChart.getTimeline("timeline").getSelection();
        var name = '';
        for (var i = 0; i < selection.length; i++) {
            var item = selection[i];
            if (item.row !== null) {
                name = googleChart.getDt("timeline").getFormattedValue(item.row, 0);
            }
        }

        if (name !== '') {
            var index = parseInt(name);
            var projectId = _pairs[index - 1].Key;
            var url = _basePath + "/Report/ProjectOverview?projectId=" + encodeURIComponent(projectId)
                + "&startDate=" + new Date(_searchDateRange.StartDate).toLocaleDateString()
                + "&endDate=" + new Date(_searchDateRange.EndDate).toLocaleDateString()
            // similar behavior as an HTTP redirect
            //window.location.replace(url);

            // similar behavior as clicking on a link
            window.location.href = url;
        }
    }

    function drawCharts() {
        var lineData = [];
        lineData.push(["Date", "Plan", "Actual"]);
        $.each(_model.PlanActualLine.Names, function (index, value) {
            lineData.push([value, _model.PlanActualLine.Items[0].Values[index], _model.PlanActualLine.Items[1].Values[index]]);
        });

        var lineOption = {
            title: "Plan/Actual Chart",
            legend: { position: 'top' }
        };

        googleChart.drawLineChart("lineChart", google.visualization.arrayToDataTable(lineData), google.charts.Line.convertOptions(lineOption));

        var planPublicHour = _.reduce(_model.PublicProjects, function (memo, hour) { return memo + hour.Key; }, 0);
        var planProjectHour = _.reduce(_model.Projects, function (memo, hour) { return memo + hour.Key; }, 0);
        var planCrHour = _.reduce(_model.Crs, function (memo, hour) { return memo + hour.Key; }, 0);

        if (planPublicHour > 0 || planProjectHour > 0 || planCrHour > 0){
            var planData = [];
            planData.push(["Name", "Hour"]);

            planData.push(["Public Project", planPublicHour]);
            planData.push(["Project", planProjectHour]);
            planData.push(["Cr", planCrHour]);

            var planOption = {
                title: 'Overall Plan Hours Distribution',
                legend: 'right',
                pieHole: 0.3
            };

            googleChart.drawPieChart("planPieChart", google.visualization.arrayToDataTable(planData), planOption);
        }

        var actualPublicHour = _.reduce(_model.PublicProjects, function (memo, hour) { return memo + hour.Value; }, 0);
        var actualProjectHour = _.reduce(_model.Projects, function (memo, hour) { return memo + hour.Value; }, 0);
        var actualCrHour = _.reduce(_model.Crs, function (memo, hour) { return memo + hour.Value; }, 0);

        if (actualPublicHour > 0 || actualProjectHour > 0 || actualCrHour > 0) {
            var actualData = [];

            actualData.push(["Name", "Hour"]);

            actualData.push(["Public Project", actualPublicHour]);
            actualData.push(["Project", actualProjectHour]);
            actualData.push(["Cr", actualCrHour]);

            var actualOption = {
                title: 'Overall Actual Hours Distribution',
                legend: 'right',
                pieHole: 0.3
            };

            googleChart.drawPieChart("actualPieChart", google.visualization.arrayToDataTable(actualData), actualOption);
        }

        if (planPublicHour > 0) {
            drawPieChart("planPublicPieChart", _model.PublicProjects, 'Overall Actual Hours Distribution', true);
        }

        if (actualPublicHour > 0) {
            drawPieChart("actualPublicPieChart", _model.PublicProjects, 'Overall Actual Hours Distribution', false);
        }

        if (planProjectHour > 0) {
            drawPieChart("planProjectPieChart", _model.Projects, 'Project Plan Hours Distribution', true);
        }

        if (actualProjectHour > 0) {
            drawPieChart("actualProjectPieChart", _model.Projects, 'Overall Actual Hours Distribution', false);
        }

        if (planCrHour > 0) {
            drawPieChart("planCrPieChart", _model.Crs, 'Cr Plan Hours Distribution', true);
        }

        if (actualCrHour > 0) {
            drawPieChart("actualCrPieChart", _model.Crs, 'Cr Actual Hours Distribution', false);
        }

        if (_timelineModel.Timelines.length > 0) {
            var dt = new google.visualization.DataTable();
            dt.addColumn({ type: 'string', id: 'Name' });
            dt.addColumn({ type: 'string', id: 'Part' });
            dt.addColumn({ type: 'date', id: 'Start' });
            dt.addColumn({ type: 'date', id: 'End' });

            $.each(_timelineModel.Timelines, function (index, element) {
                $.each(element.Parts, function (pIndex, part) {
                    dt.addRow([(index + 1).toString(), part.Name, new Date(part.StartDate), new Date(part.EndDate)]);
                });
            });

            var option = {
                height: 41 * _timelineModel.Timelines.length + 50
            };

            googleChart.drawTimelineChart("timeline", dt, option, selectHandler);
        }
    }
});

(function () {
    angular.module('userOverview', ['ui.bootstrap', 'myFilters'])
        .controller('userCtrl', ['$scope', function ($scope) {
            $scope.searchDateRange = _searchDateRange;
            $scope.model = _model;
            $scope.isStartDateOpen = false;
            $scope.isEndDateOpen = false;

            $scope.openStartDate = function () {
                $scope.isStartDateOpen = !$scope.isStartDateOpen;
            };

            $scope.openEndDate = function () {
                $scope.isEndDateOpen = !$scope.isEndDateOpen;
            };

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
                var url = _basePath + "/Report/UserOverview?userId=" + encodeURIComponent($scope.model.Id)
                    + "&startDate=" + new Date(_searchDateRange.StartDate).toLocaleDateString()
                    + "&endDate=" + new Date(_searchDateRange.EndDate).toLocaleDateString();

                // similar behavior as clicking on a link
                window.location.href = url;
            }

            $scope.isShowSearchHours = function () {
                return _.reduce($scope.model.Hours, function (memo, hour) { return memo + hour.Search; }, 0) > 0;
            };
        }]);
}());