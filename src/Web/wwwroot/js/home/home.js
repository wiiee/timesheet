(function () {
    angular.module("home", ['ui.bootstrap'])
    .controller("homeCtrl", function ($scope, $http, $modal, $log) {
        $scope.model = _model;

        $scope.open = function (size, userId, commentUserId) {
            if (!_.findWhere($scope.model, { Id: userId }).IsCommentActive) {
                return;
            }

            var modalInstance = $modal.open({
                templateUrl: 'myModalContent.html',
                controller: 'ModalInstanceCtrl',
                size: size,
                resolve: {
                    userId: function() {
                        return userId;
                    },
                    commentUserId: function () {
                        return commentUserId;
                    },
                    model: function () {
                        return $scope.model;
                    }
                }
            });

            //modalInstance.result.then(function (result) {
            //    $log.info('Modal close at: ' + new Date());
            //}, function (reason) {
            //    $log.info('Modal dismissed at: ' + new Date());
            //});
        };

        $scope.thumbsUp = function (userId, commentUserId) {
            if (!_.findWhere($scope.model, { Id: userId }).IsThumbsUpActive) {
                return;
            }

            var url = _basePath + "/api/user/ThumbsUp?userId=" + userId + "&commentUserId=" + commentUserId;

            $http.post(encodeURI(url)).then(function (response) {
                _.findWhere($scope.model, { Id: userId }).ThumbsUp++;
                _.findWhere($scope.model, { Id: userId }).IsThumbsUpActive = false;
            });
        };

        $scope.thumbsDown = function (userId, commentUserId) {
            if (!_.findWhere($scope.model, { Id: userId }).IsThumbsDownActive)
            {
                return;
            }

            var url = _basePath + "/api/user/ThumbsDown?userId=" + userId + "&commentUserId=" + commentUserId;
            $http.post(encodeURI(url)).then(function (response) {
                _.findWhere($scope.model, { Id: userId }).ThumbsDown++;
                _.findWhere($scope.model, { Id: userId }).IsThumbsDownActive = false;
            });
        };

        $scope.getThumbsUp = function (userId) {
            return _.findWhere($scope.model, { Id: userId }).ThumbsUp;
        };

        $scope.getThumbsDown = function (userId) {
            return _.findWhere($scope.model, { Id: userId }).ThumbsDown;
        };

        $scope.getComment = function (userId) {
            return _.findWhere($scope.model, { Id: userId }).Comment;
        };

        $scope.isThumbsUpActive = function (userId) {
            return _.findWhere($scope.model, { Id: userId }).IsThumbsUpActive;
        };

        $scope.isThumbsDownActive = function (userId) {
            return _.findWhere($scope.model, { Id: userId }).IsThumbsDownActive;
        };

        $scope.isCommentActive = function (userId) {
            return _.findWhere($scope.model, { Id: userId }).IsCommentActive;
        };
    })
    .controller('ModalInstanceCtrl', function ($scope, $modalInstance, $http, userId, commentUserId, model) {
        $scope.model = model;

        $scope.ok = function () {
            if ($scope.comment) {
                var url = _basePath + "/api/user/addComment?userId=" + userId + "&comment=" + encodeURIComponent($scope.comment) + "&commentUserId=" + commentUserId;
                $http.post(url).then(function (response) {
                    return _.findWhere($scope.model, { Id: userId }).Comment++;
                });
            }

            $modalInstance.close($scope.comment);
        };

        $scope.cancel = function () {
            $modalInstance.dismiss('cancel');
        };
    });
}());