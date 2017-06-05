$(function () {
    $("body").on("click", "a.return", function (evt) {
        //evt.stopPropagation();
        evt.stopImmediatePropagation();
        var element = $(this);
        bootbox.confirm("Are you sure to return this timesheet?", function (result) {
            if (result) {
                location = _basePath + "/TimeSheet/ReturnTimeSheet?monday=" + element.attr("data-monday") + "&userId=" + element.attr("data-userId");
            }
        });
    });
});

(function () {
    angular.module('manageTimeSheet', [])
        .controller('manageTimeSheetCtrl', function ($scope, $http) {
            $scope.users = [];
            $scope.names = {};

            var url = _basePath + "/api/TimeSheet/GetManageTimeSheetModel";
            $http.post(url).then(function (response) {
                $scope.users = response.data;
            });

            url = _basePath + "/api/user/GetUserNames";
            $http.post(url).then(function (response) {
                $scope.names = response.data;
            });

            $scope.showTexts = ["Show All", "Show Recently"];
            $scope.currentTextIndex = 0;
            $scope.showText = $scope.showTexts[0];

            $scope.switchShowAll = function () {
                $scope.currentTextIndex = ($scope.currentTextIndex + 1) % 2;
                $scope.showText = $scope.showTexts[$scope.currentTextIndex];
            };
        });
}());