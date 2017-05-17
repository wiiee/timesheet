/*
* trip.utility is a helper module
* containing common functions/variables.
*
*/

var timeSheet = timeSheet || {};

utility = (function () {
    var instance = {
    };

    instance.helpers =
    {
        compare: function (left, operator, right, options) {
            var operators = {
                // Need to ignore in order to include double equals
                /* jshint ignore:start */
                '==': function (l, r) { return l == r; },
                '===': function (l, r) { return l === r; },
                '!=': function (l, r) { return l != r; },
                '<': function (l, r) { return l < r; },
                '>': function (l, r) { return l > r; },
                '<=': function (l, r) { return l <= r; },
                '>=': function (l, r) { return l >= r; },
                '||': function (l, r) { return l || r; },
                '&&': function (l, r) { return l && r; },
                'typeof': function (l, r) { return typeof l == r; }
                /* jshint ignore:end */
            };

            if (!operators[operator]) {
                throw new Error("Handlebars Helper 'compare' doesn't know the operator " + operator);
            }

            var result = operators[operator](left, right);

            if (result) {
                return options.fn(this);
            }
            else {
                return options.inverse(this);
            }
        },

        math: function (left, operator, right, options) {
            var operators = {
                // Need to ignore in order to include double equals
                /* jshint ignore:start */
                '+': function (l, r) { return l + r; },
                '-': function (l, r) { return l - r; },
                '*': function (l, r) { return l * r; },
                '/': function (l, r) { return l / r; },
                '%': function (l, r) { return l % r; }
                /* jshint ignore:end */
            };

            if (!operators[operator]) {
                throw new Error("Handlebars Helper 'compare' doesn't know the operator " + operator);
            }

            return operators[operator](left, right);
        },

        times: function (n, block) {
            var accum = '';
            for (var i = 0; i < n; ++i)
                accum += block.fn(i);
            return accum;
        },

        displayDate: function (date) {
            var d = new Date(date);
            return ("0" + (d.getUTCMonth() + 1)).slice(-2) + "/" + ("0" + d.getUTCDate()).slice(-2) + "/" + d.getUTCFullYear();
        },

        showValue: function (text) {
            return text ? "value=" + text : "";
        },

        showIfExist: function (text) {
            return text ? text : "";
        },

        showPrice: function (basePrice, seasonalPrice) {
            if(seasonalPrice && seasonalPrice < basePrice){
                return '<span class="seasonal">' + seasonalPrice + '</span> <span class="base"><s>' + basePrice + "</s></span>";
            }
            else{
                return '<span class="seasonal hide">' + basePrice + '</span> <span class="base">' + basePrice + "</span>";
            }
        },

        debug: function (optionalValue) {
            console.log("Current Context");
            console.log("====================");
            console.log(this);

            if (optionalValue) {
                console.log("Value");
                console.log("====================");
                console.log(optionalValue);
            }
        }
    };

    instance.getHandlebarContent = function (id, data) {
        var source = $("#" + id).html();
        var template = Handlebars.compile(source);
        return template(data);
    };

    instance.removeClass = function (element, className) {
        if (element instanceof jQuery) {
            if (element.hasClass(className)) {
                element.removeClass(className);
            }
        }
        else {
            throw "the first input parameter is not a jQuery object!!!";
        }
    };

    instance.addClass = function (element, className) {
        if (element instanceof jQuery) {
            if (!element.hasClass(className)) {
                element.addClass(className);
            }
        }
        else {
            throw "the first input parameter is not a jQuery object!!!";
        }
    };

    instance.hide = function (element) {
        instance.addClass(element, "hide");
    };

    instance.show = function (element) {
        instance.removeClass(element, "hide");
    };

    instance.getMd5 = function (value) {
        return CryptoJS.MD5(value).toString();
    };

    instance.generateForm = function (url, obj) {
        var form = "<form class='hide' id='submitForm' method='POST' novalidate='novalidate' action='" + url + "'>";

        $.each(obj, function (key, value) {
            form += "<input type='hidden' name='" + key + "' value='" + value + "' />";
        });

        form += "</form>";

        return form;
    };

    instance.escapeRegExp = function (str) {
        return str.replace(/([.*+?^=!:${}()|\[\]\/\\])/g, "\\$1");
    };

    instance.replaceAll = function (str, find, replace) {
        return str.replace(new RegExp(escapeRegExp(find), 'g'), replace);
    };

    // -1 means date1 < date2, 0 means date1 == date2, else means date1 > date2 
    instance.compareDateWithoutTime = function (date1, date2) {
        date1.setHours(0, 0, 0, 0);
        date2.setHours(0, 0, 0, 0);
        return date1 < date2;
    };

    //大于0的float
    instance.isFloat = function (value) {
        var result = $.isNumeric(value);

        return result ? parseFloat(value) > 0 : false;
    };

    instance.getDateString = function (date) {
        return ("0" + (date.getMonth() + 1)).slice(-2) + "/" + ("0" + date.getDate()).slice(-2) + "/" + date.getFullYear();
    };

    instance.getTimeSheetId = function (input) {
        var date = input;

        if (typeof date === "string") {
            var date = new Date(input);
        }

        return date.getFullYear() + "/" + ("0" + (date.getMonth() + 1)).slice(-2) + "/" + ("0" + date.getDate()).slice(-2);
    };

    instance.getWorkingDays = function (startDate, endDate) {
        var result = 0;

        var currentDate = new Date(startDate.toDateString());
        while (currentDate <= endDate) {
            var weekDay = currentDate.getDay();
            if (weekDay != 0 && weekDay != 6)
                result++;

            currentDate.setDate(currentDate.getDate() + 1);
        }

        return result;
    };

    //大于0的整数
    instance.isInt = function (value) {
        return /^([0-9]+|Infinity)$/.test(value);
    };

    instance.getParameterByName = function (name, url) {
        if (!url) url = window.location.href;
        name = name.replace(/[\[\]]/g, "\\$&");
        var regex = new RegExp("[?&]" + name + "(=([^&#]*)|&|#|$)"),
            results = regex.exec(url);
        if (!results) return null;
        if (!results[2]) return '';
        return decodeURIComponent(results[2].replace(/\+/g, " "));
    }

    instance.exportData = function (models, dateRange, filename) {
        var saveFileName = filename || "default";
        function datenum(v, date1904) {
            if (date1904) v += 1462;
            var epoch = Date.parse(v);
            return (epoch - new Date(Date.UTC(1899, 11, 30))) / (24 * 60 * 60 * 1000);
        }

        function sheet_from_array_of_arrays(data) {
            var ws = {};
            var range = { s: { c: 10000000, r: 10000000 }, e: { c: 0, r: 0 } };
            for (var R = 0; R != data.length; ++R) {
                for (var C = 0; C != data[R].length; ++C) {
                    if (range.s.r > R) range.s.r = R;
                    if (range.s.c > C) range.s.c = C;
                    if (range.e.r < R) range.e.r = R;
                    if (range.e.c < C) range.e.c = C;
                    var cell = { v: data[R][C] };
                    if (cell.v == null) continue;
                    var cell_ref = XLSX.utils.encode_cell({ c: C, r: R });

                    if (typeof cell.v === 'number') cell.t = 'n';
                    else if (typeof cell.v === 'boolean') cell.t = 'b';
                    else if (cell.v instanceof Date) {
                        cell.t = 'n'; cell.z = XLSX.SSF._table[14];
                        cell.v = datenum(cell.v);
                    }
                    else {
                        cell.t = 's';
                    }
                            
                    ws[cell_ref] = cell;
                }
            }
            if (range.s.c < 10000000) ws['!ref'] = XLSX.utils.encode_range(range);
            return ws;
        }

        function Workbook() {
            if (!(this instanceof Workbook)) return new Workbook();
            this.SheetNames = [];
            this.Sheets = {};
        }

        /* original data */
        var data = models;
        var ws_name = dateRange.StartDate.substr(0, 10) + "~" + dateRange.EndDate.substr(0, 10);
        var wb = new Workbook(), ws = sheet_from_array_of_arrays(data);

        /* add worksheet to workbook */
        wb.SheetNames.push(ws_name);
        wb.Sheets[ws_name] = ws;
        var wbout = XLSX.write(wb, { bookType: 'xlsx', bookSST: true, type: 'binary' });

        function s2ab(s) {
            var buf = new ArrayBuffer(s.length);
            var view = new Uint8Array(buf);
            for (var i = 0; i != s.length; ++i) view[i] = s.charCodeAt(i) & 0xFF;
            return buf;
        }
        saveAs(new Blob([s2ab(wbout)], { type: "application/octet-stream" }), saveFileName+".xlsx");
    }

    return instance;
})();