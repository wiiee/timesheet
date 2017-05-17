$(function () {

});

(function () {
    angular.module("overview", [])
    .controller("overviewCtrl", function ($scope, $http) {
        $scope.sprintId = _sprintId;

        $scope.rowCollection = [];
    });
}());