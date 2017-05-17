$(function () {
    $("#status").multiselect({
        buttonWidth: "100%"
    });
});

(function () {
    angular.module('myTimeSheet', ['smart-table', 'ui.bootstrap', 'myFilters'])
        .controller('myTimeSheetCtrl', function ($scope, $http) {
            $scope.rowCollection = [];

            $scope.changeStatus = function () {
                $.each($scope.rowCollection, function (index, value) {
                    if (_.indexOf($scope.statuses, value.Status) != -1) {
                        value.IsShow = true;
                    }
                    else {
                        value.IsShow = false;
                    }
                });
            };

            $scope.init = function () {
                var url = _basePath + "/api/TimeSheet/GetTimeSheetModels";
                $http.post(url).then(function (response) {
                    $scope.rowCollection = response.data;
                    $scope.changeStatus();
                });
            };

            $scope.init();

            $scope.returnTimeSheet = function (monday, userId) {
                var url = _basePath + "/api/TimeSheet/ReturnTimeSheet?monday=" + utility.getTimeSheetId(monday) + "&userId=" + userId;
                $http.post(url).then(function (response) {
                    if (response.data.successMsg) {
                        var msg = $("#successMsg").html();
                        msg = msg.replace("replaceText", response.data.successMsg);
                        $("#messageContainer").append(msg);

                        var item = _.find($scope.rowCollection, function (row) {
                            return row.Monday === monday;
                        });

                        item.IsReturn = false;

                        if (item.WeekHours > 0) {
                            item.Status = "Ongoing";
                        } else {
                            item.Status = "Pending";
                        }

                        $scope.changeStatus();
                        //index = _.findIndex($scope.displayCollection, { "Id": projectId });
                        //$scope.displayCollection.splice(index, 1);
                    }
                    else if (response.data.errorMsg) {
                        var msg = $("#errorMsg").html();
                        msg = msg.replace("replaceText", response.data.errorMsg);
                        $("#messageContainer").append(msg);
                    }
                });
            };

            $scope.itemsByPage = _pageSize;
            $scope.isCollapsed = true;
            $scope.searchTexts = ["Show Condition", "Hide Condition"];
            $scope.searchText = $scope.searchTexts[0];
            $scope.currentTextIndex = 0;
            $scope.statuses = ["Pending", "Ongoing"];

            $scope.switchSearch = function () {
                $scope.isCollapsed = !$scope.isCollapsed;
                $scope.currentTextIndex = ($scope.currentTextIndex + 1) % 2;
                $scope.searchText = $scope.searchTexts[$scope.currentTextIndex];
            };

            $scope.changeSize = function () {
                var data = {
                    key: "pageSize",
                    value: $scope.itemsByPage
                };

                $http.post(_basePath + "/api/preference", data);
            };
        })
        .directive('stDateRangeAlone', ['$timeout', function ($timeout) {
            return {
                restrict: 'E',
                require: '^stTable',
                scope: {
                    before: '=',
                    after: '='
                },
                templateUrl: '../template/stDateRangeAlone.html',

                link: function (scope, element, attr, table) {

                    var inputs = element.find('input');
                    var inputBefore = angular.element(inputs[0]);
                    var inputAfter = angular.element(inputs[1]);
                    var predicateName = attr.predicate;


                    [inputBefore, inputAfter].forEach(function (input) {

                        input.bind('blur', function () {


                            var query = {};

                            if (!scope.isBeforeOpen && !scope.isAfterOpen) {

                                if (scope.before) {
                                    query.before = scope.before;
                                }

                                if (scope.after) {
                                    query.after = scope.after;
                                }

                                scope.$apply(function () {
                                    table.search(query, predicateName);
                                })
                            }
                        });
                    });

                    function open(before) {
                        return function ($event) {
                            $event.preventDefault();
                            $event.stopPropagation();

                            if (before) {
                                scope.isBeforeOpen = !scope.isBeforeOpen;
                            } else {
                                scope.isAfterOpen = !scope.isAfterOpen;
                            }
                        }
                    }

                    scope.openBefore = open(true);
                    scope.openAfter = open();
                }
            }
        }])
        .directive('stNumberRange', ['$timeout', function ($timeout) {
            return {
                restrict: 'E',
                require: '^stTable',
                scope: {
                    lower: '=',
                    higher: '=',
                    predicateName: '=',
                    text: '='
                },
                templateUrl: '../template/stNumberRange.html',
                link: function (scope, element, attr, table) {
                    var inputs = element.find('input');
                    var inputLower = angular.element(inputs[0]);
                    var inputHigher = angular.element(inputs[1]);
                    var predicateName = attr.predicate;
                    scope.predicateName = predicateName;
                    scope.text = attr.labeltext;

                    [inputLower, inputHigher].forEach(function (input, index) {

                        input.bind('blur', function () {
                            var query = {};

                            if (scope.lower && scope.lower[predicateName]) {
                                query.lower = scope.lower[predicateName];
                            }

                            if (scope.higher && scope.higher[predicateName]) {
                                query.higher = scope.higher[predicateName];
                            }

                            scope.$apply(function () {
                                table.search(query, predicateName)
                            });
                        });
                    });
                }
            };
        }])
        .filter('customFilter', ['$filter', function ($filter) {
            var filterFilter = $filter('filter');
            var standardComparator = function standardComparator(obj, text) {
                text = ('' + text).toLowerCase();
                return ('' + obj).toLowerCase().indexOf(text) > -1;
            };

            return function customFilter(array, expression) {
                function customComparator(actual, expected) {
                    var isBeforeActivated = expected.before;
                    var isAfterActivated = expected.after;
                    var isLower = expected.lower;
                    var isHigher = expected.higher;
                    var higherLimit;
                    var lowerLimit;
                    var itemDate;
                    var queryDate;

                    if (angular.isObject(expected)) {

                        //date range
                        if (expected.before || expected.after) {
                            try {
                                if (isBeforeActivated) {
                                    higherLimit = expected.before;

                                    itemDate = new Date(actual);
                                    queryDate = new Date(higherLimit);

                                    if (itemDate > queryDate) {
                                        return false;
                                    }
                                }

                                if (isAfterActivated) {
                                    lowerLimit = expected.after;

                                    itemDate = new Date(actual);
                                    queryDate = new Date(lowerLimit);

                                    if (itemDate < queryDate) {
                                        return false;
                                    }
                                }

                                return true;
                            } catch (e) {
                                return false;
                            }

                        } else if (isLower || isHigher) {
                            //number range
                            if (isLower) {
                                higherLimit = expected.lower;

                                if (actual > higherLimit) {
                                    return false;
                                }
                            }

                            if (isHigher) {
                                lowerLimit = expected.higher;
                                if (actual < lowerLimit) {
                                    return false;
                                }
                            }

                            return true;
                        }
                        //etc
                        return true;

                    }
                    return standardComparator(actual, expected);
                }

                var output = filterFilter(array, expression, customComparator);
                return output;
            };
        }]);
}());