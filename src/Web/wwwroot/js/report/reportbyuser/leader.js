$(function () {
    googleChart.init(['corechart', 'timeline'], drawCharts);

    // The selection handler.
    // Loop through all items in the selection and concatenate
    // a single message from all of them.
    function selectHandlerCombo() {
        var selection = googleChart.getCombo("combo").getSelection();
        var name = '';
        for (var i = 0; i < selection.length; i++) {
            var item = selection[i];
            if (item.row !== null) {
                name = googleChart.getDt("combo").getFormattedValue(item.row, 0);
            }
        }

        if (name !== '') {
            var userId = _.findWhere(_pairs, { Value: name }).Key;
            var url = _basePath + "/Report/UserOverview?userId=" + encodeURIComponent(userId)
                + "&startDate=" + new Date(_searchDateRange.StartDate).toLocaleDateString()
                + "&endDate=" + new Date(_searchDateRange.EndDate).toLocaleDateString()
            // similar behavior as an HTTP redirect
            //window.location.replace(url);

            // similar behavior as clicking on a link
            window.location.href = url;
        }
    }

    function selectHandlerContributionCombo() {
        var selection = googleChart.getCombo("contributionCombo").getSelection();
        var name = '';
        for (var i = 0; i < selection.length; i++) {
            var item = selection[i];
            if (item.row !== null) {
                name = googleChart.getDt("contributionCombo").getFormattedValue(item.row, 0);
            }
        }

        if (name !== '') {
            var userId = _.findWhere(_pairs, { Value: name }).Key;
            var url = _basePath + "/Report/UserOverview?userId=" + encodeURIComponent(userId)
                + "&startDate=" + new Date(_searchDateRange.StartDate).toLocaleDateString()
                + "&endDate=" + new Date(_searchDateRange.EndDate).toLocaleDateString()
            // similar behavior as an HTTP redirect
            //window.location.replace(url);

            // similar behavior as clicking on a link
            window.location.href = url;
        }
    }

    function drawCharts() {
        var data = [];

        data.push(["User", "Plan", "Actual"]);

        $.each(_model.Combos, function (index, element) {
            data.push([element.Name, element.Items[0].Value, element.Items[1].Value]);
        });

        var dt = google.visualization.arrayToDataTable(data);

        var option = {
            title: 'Plan and Actual',
            vAxis: { title: 'Hour' },
            seriesType: 'bars',
            series: { 2: { type: 'line' } },
            legend: "top"
        };

        googleChart.drawComboChart("combo", dt, option, selectHandlerCombo);

        var contributionData = [];
        contributionData.push(["User", "Contribution"]);
        $.each(_model.Combos, function (index, element) {
            contributionData.push([element.Name, element.Items[2].Value]);
        });
        var contributionDt = google.visualization.arrayToDataTable(contributionData);
        var contributionOption = {
            title: 'Contribution',
            vAxis: { title: 'Point' },
            seriesType: 'bars',
            series: { 2: { type: 'line' } },
            legend: "none"
        };

        googleChart.drawComboChart("contributionCombo", contributionDt, contributionOption, selectHandlerContributionCombo);

        var deviationData = [];

        deviationData.push(["User", "Deviation", "Standard"]);

        $.each(_model.Combos, function (index, element) {
            var percentage = element.Items[1].Value == 0 ? 200 : element.Items[0].Value / element.Items[1].Value * 100;
            deviationData.push([element.Name, percentage, 100]);
        });

        var deviationDt = google.visualization.arrayToDataTable(deviationData);

        var deviationOption = {
            title: 'Plan/Actual',
            legend: { position: 'top' }
        };

        googleChart.drawLineChart("deviationCombo", deviationDt, deviationOption);
    }
});

(function () {
    angular.module('reportByUser', ['ui.bootstrap'])
        .controller('userCtrl', ['$scope', function ($scope) {
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
                var url = _basePath + "/Report/ReportByUser?";
                if (groupId != null) {
                    url += "&groupId=" + groupId;
                }
                url += "&startDate=" + new Date(_searchDateRange.StartDate).toLocaleDateString()
                    + "&endDate=" + new Date(_searchDateRange.EndDate).toLocaleDateString();

                // similar behavior as clicking on a link
                window.location.href = url;
            }

            $scope.exportData = function () {
                utility.exportData($scope.models, $scope.searchDateRange, "ReportByUser");
            }
        }]);
}());