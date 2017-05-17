$(function () {
    googleChart.init(['timeline', 'corechart'], drawCharts);

    // The selection handler.
    // Loop through all items in the selection and concatenate
    // a single message from all of them.
    function selectTimelineHandler() {
        var selection = googleChart.getTimeline("timeline").getSelection();
        var name = '';
        for (var i = 0; i < selection.length; i++) {
            var item = selection[i];
            if (item.row !== null) {
                name = googleChart.getDt("timeline").getFormattedValue(item.row, 0);
            }
        }

        if (name !== '') {
            var projectId = _.findWhere(_pairs, { Value: name }).Key;
            var url = _basePath + "/Report/ProjectOverview?projectId=" + encodeURIComponent(projectId)
                + "&startDate=" + new Date(_searchDateRange.StartDate).toLocaleDateString()
                + "&endDate=" + new Date(_searchDateRange.EndDate).toLocaleDateString()
            // similar behavior as an HTTP redirect
            //window.location.replace(url);

            // similar behavior as clicking on a link
            window.location.href = url;
        }
    }

    function selectBubbleHandler() {
        var selection = googleChart.getBubble("bubble").getSelection();
        var name = '';
        for (var i = 0; i < selection.length; i++) {
            var item = selection[i];
            if (item.row !== null) {
                name = googleChart.getDt("bubble").getFormattedValue(item.row, 0);
            }
        }

        if (name !== '') {
            var projectId = _pairs[parseInt(name)].Key;
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
        var dt = new google.visualization.DataTable();
        dt.addColumn({ type: 'string', id: 'Name' });
        dt.addColumn({ type: 'string', id: 'Part' });
        dt.addColumn({ type: 'date', id: 'Start' });
        dt.addColumn({ type: 'date', id: 'End' });

        $.each(_model.Timelines, function (index, element) {
            $.each(element.Parts, function (pIndex, part) {
                dt.addRow([element.Name, part.Name, new Date(part.StartDate), new Date(part.EndDate)]);
            });
        });

        var option = {
            height: 41 * _model.Timelines.length + 50
        };

        googleChart.drawTimelineChart("timeline", dt, option, selectTimelineHandler);

        var dtBubble = new google.visualization.DataTable();
        dtBubble.addColumn({ type: 'string', id: 'Name' });
        dtBubble.addColumn({ type: 'number', id: 'Percentage' });
        dtBubble.addColumn({ type: 'number', id: 'Risk Factor' });
        dtBubble.addColumn({ type: 'string', id: 'Category' });
        dtBubble.addColumn({ type: 'number', id: 'Size' });

        $.each(_bubbleModel.Bubbles, function (index, element) {
            dtBubble.addRow([index.toString(), element.H, element.V, element.Category, element.Size]);
        });

        var optionBubble = {
            title: 'Risk',
            width: 1168,
            hAxis: { title: 'Percentage' },
            vAxis: { title: 'Risk Factor' },
            bubble: {
                textStyle: {
                    fontSize: 1,
                    auraColor: 'none',
                }
            }
        };

        googleChart.drawBubbleChart("bubble", dtBubble, optionBubble, selectBubbleHandler);
    }
});

(function () {
    angular.module('reportByProject', ['ui.bootstrap'])
        .controller('projectCtrl', ['$scope', function ($scope) {
            $scope.searchDateRange = _searchDateRange;
            $scope.models = _exportEntities;
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
                redirectToNewPeriod();
            };

            $scope.nextWeek = function () {
                var startdate = new Date($scope.searchDateRange.StartDate);
                $scope.searchDateRange.StartDate = startdate.setDate(startdate.getDate() + 7);
                var enddate = new Date($scope.searchDateRange.EndDate);
                $scope.searchDateRange.EndDate = enddate.setDate(enddate.getDate() + 7);
                redirectToNewPeriod();
            };

            function redirectToNewPeriod() {
                var groupId = utility.getParameterByName("groupId", window.location.href);
                var url = _basePath + "/Report/ReportByProject?";
                if (groupId != null) {
                    url += "&groupId=" + groupId;
                }
                url += "&startDate=" + new Date(_searchDateRange.StartDate).toLocaleDateString()
                    + "&endDate=" + new Date(_searchDateRange.EndDate).toLocaleDateString();

                // similar behavior as clicking on a link
                window.location.href = url;
            }

            $scope.exportData = function () {
                utility.exportData($scope.models, $scope.searchDateRange, "ReportByProject");
            }
        }]);
}());