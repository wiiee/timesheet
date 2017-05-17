angular
    .module('myFilters', [])
    .filter('escape', function () {
        return window.encodeURIComponent;
    })
    .filter('percentage', ['$filter', function ($filter) {
        return function (input, decimals) {
            return $filter('number')(input * 100, decimals) + '%';
        };
    }])
    .filter('progresstype', function () {
        return function (input) {
            input = input || 1;
            var out = "";

            number = parseFloat(input) * 100;

            if (number >= 90) {
                return "danger";
            }
            else if (number >= 60) {
                return "warning";
            }
            else {
                return "info";
            }
        };
    })
    .filter('timesheetid', function () {
        return function (input) {
            var date = new Date(input);
            return date.getFullYear() + "/" + ("0" + (date.getMonth() + 1)).slice(-2) + "/" + ("0" + date.getDate()).slice(-2);
        };
    })
    .filter('unique', function () {
        return function (arr, field) {
            var o = {}, i, l = arr.length, r = [];
            for (i = 0; i < l; i += 1) {
                o[arr[i][field]] = arr[i];
            }
            for (i in o) {
                r.push(o[i]);
            }
            return r;
        };
    })
    .filter('uniquesingle', function () {
        return function (arr, field) {
            var o = {}, i, l = arr.length, r = [];
            for (i = 0; i < l; i += 1) {
                $.each(arr[i][field].split(","), function (index, value) {
                    if (value) {
                        o[value] = arr[i];
                    }
                });
            }
            for (i in o) {
                r.push(i);
            }
            return r;
        };
    })
    .filter('getdate', function () {
        return function (date) {
            if (date) {
                return ("0" + (date.getMonth() + 1)).slice(-2) + "/" + ("0" + date.getDate()).slice(-2) + "/" + date.getFullYear();
            }

            return "";
        }  
    });