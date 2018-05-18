(function () {
    angular.module("importProject", ['ui.bootstrap'])
    .controller("importProjectCtrl", function ($scope, $http) {
        $scope.request = "";
        $scope.response = "";

        $scope.importProjects = function () {
            var url = _basePath + "/api/import/importProjects";
            $http.post(url, JSON.parse($scope.request)).then(function (response) {
                $scope.response = JSON.stringify(response.data);
            });
        };
    });
}());