(function () {
    angular.module("importTimeSheet", ['ui.bootstrap'])
    .controller("importTimeSheetCtrl", function ($scope, $http) {
        $scope.request = "";
        $scope.response = "";

        $scope.importProjects = function () {
            var url = _basePath + "/api/import/importTimeSheets";
            $http.post(url, JSON.parse($scope.request)).then(function (response) {
                $scope.response = JSON.stringify(response.data);
            });
        };
    });
}());