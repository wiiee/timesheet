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
            $scope.currentTextIndex = {};
            $scope.showText = {};

            $scope.showTexts = ["Show All", "Show Recently"];

            var url = _basePath + "/api/TimeSheet/GetManageTimeSheetModel";
            $http.post(url).then(function (response) {
                $scope.users = response.data;

                $.each($scope.users, function (key, value) {
                    $scope.currentTextIndex[key] = 0;
                    $scope.showText[key] = $scope.showTexts[0];
                });
            });

            url = _basePath + "/api/user/GetUserNames";
            $http.post(url).then(function (response) {
                $scope.names = response.data;
            });

            $scope.switchShowAll = function (userId) {
                $scope.currentTextIndex[userId] = ($scope.currentTextIndex[userId] + 1) % 2;
                $scope.showText[userId] = $scope.showTexts[$scope.currentTextIndex[userId]];
            };
        });
}());