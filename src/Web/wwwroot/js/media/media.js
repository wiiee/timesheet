(function () {
    url = _basePath + "/api/userimg/" + _userId;

    angular.module('media', ['blueimp.fileupload'])
    .config(['$httpProvider', 'fileUploadProvider', function ($httpProvider, fileUploadProvider) {
        delete $httpProvider.defaults.headers.common['X-Requested-With'];

        fileUploadProvider.defaults.redirect = window.location.href.replace(
            /\/[^\/]*$/,
            '/cors/result.html?%s'
        );

        angular.extend(fileUploadProvider.defaults, {
            // Enable image resizing, except for Android and Opera,
            // which actually support image resizing, but fail to
            // send Blob objects via XHR requests:
            disableImageResize: /Android(?!.*Chrome)|Opera/.test(window.navigator.userAgent),
            maxFileSize: 999000,
            acceptFileTypes: /(\.|\/)(gif|jpe?g|png)$/i
        });
    }])
    .controller('ImgUploadController', ['$scope', '$http', '$filter', '$window', function ($scope, $http) {
        $scope.options = {
            url: url,
            type: "PUT"
        };

        $scope.loadingFiles = true;

        $http.get(_basePath + "/api/userimg/" + _userId).then(
            function (response) {
                $scope.loadingFiles = false;
                $scope.queue = response.data.files || [];
            },
            function () {
                $scope.loadingFiles = false;
            }
        );
    }])
    .controller('FileDestroyController', ['$scope', '$http', function ($scope, $http) {
            var file = $scope.file,
                state;
            if (file.url) {
                file.$state = function () {
                    return state;
                };

                file.$destroy = function () {
                    bootbox.confirm("Are you sure to delete this picture?", function (result) {
                        if (result) {
                            state = 'pending';
                            return $http({
                                url: file.deleteUrl,
                                method: file.deleteType
                            }).then(
                                function () {
                                    state = 'resolved';
                                    $scope.clear(file);
                                },
                                function () {
                                    state = 'rejected';
                                }
                            );
                        }
                    });
                };
            } 
            else if (!file.$cancel && !file._index) {
                file.$cancel = function () {
                    $scope.clear(file);
                };
            }
        }
    ]);
}());