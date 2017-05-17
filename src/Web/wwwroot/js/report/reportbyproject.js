$(function () {
    $(document).tooltip();
});

(function () {
    angular.module('reportByProject', ['smart-table', 'ui.bootstrap', 'myFilters'])
        .controller('projectCtrl', ['$scope', '$http', function ($scope, $http) {
            $scope.rowCollection = _rowCollection;
            $scope.color = _color;
            $scope.displayCollection = [].concat($scope.rowCollection);

            $scope.searchDateRange = _searchDateRange;
            $scope.isCollapsed = true;
            $scope.itemsByPage = _pageSize;
            $scope.searchTexts = ["Show Condition", "Hide Condition"];
            $scope.searchText = $scope.searchTexts[0];
            $scope.currentTextIndex = 0;

            $scope.isStartDateOpen = false;
            $scope.isEndDateOpen = false;

            $scope.openStartDate = function () {
                $scope.isStartDateOpen = !$scope.isStartDateOpen;
            };

            $scope.openEndDate = function () {
                $scope.isEndDateOpen = !$scope.isEndDateOpen;
            };

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
        }])
        .directive('stDateRange', ['$timeout', function ($timeout) {
            return {
                restrict: 'E',
                require: '^stTable',
                scope: {
                    before: '=',
                    after: '=',
                    beforepredicate: '=',
                    afterpredicate: '=',
                    name: '=',
                    text: '='
                },
                templateUrl: '../template/stDateRange.html',

                link: function (scope, element, attr, table) {
                    var beforepredicate = attr.beforepredicate;
                    var afterpredicate = attr.afterpredicate;
                    var name = beforepredicate + afterpredicate;
                    scope.beforepredicate = beforepredicate;
                    scope.afterpredicate = afterpredicate;
                    scope.name = name;
                    scope.text = attr.labeltext;

                    var inputs = element.find('input');
                    var inputBefore = angular.element(inputs[0]);
                    var inputAfter = angular.element(inputs[1]);

                    [inputBefore, inputAfter].forEach(function (input) {
                        input.bind('blur', function () {
                            var query = {};

                            if (!scope.isBeforeOpen && !scope.isAfterOpen) {
                                if (scope.before && scope.before[name]) {
                                    query.before = scope.before[name];
                                }

                                if (scope.after && scope.after[name]) {
                                    query.after = scope.after[name];
                                }

                                scope.$apply(function () {
                                    table.search(query, beforepredicate);
                                });

                                scope.$apply(function () {
                                    table.search(query, afterpredicate);
                                });
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