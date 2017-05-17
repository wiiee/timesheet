$(function () {
    $.validate();
    $(document).tooltip();
});

(function () {
    angular.module("postponeProject", ['ui.bootstrap'])
    .controller("postponeProjectCtrl", ['$scope', function ($scope) {
        $scope.isEndDateOpen = false;

        $scope.openEndDate = function () {
            $scope.isEndDateOpen = !$scope.isEndDateOpen;
        }
    }]);
}());