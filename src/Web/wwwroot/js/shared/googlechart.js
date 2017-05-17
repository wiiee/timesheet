/*
* trip.utility is a helper module
* containing common functions/variables.
*
*/

var timeSheet = timeSheet || {};

googleChart = (function () {
    var instance = {
        pies: {},
        timelines: {},
        combos: {},
        lines: {},
        bubbles: {},
        dts: {}
    };

    instance.getPie = function (id) {
        if (instance.pies[id]) {
            return instance.pies[id];
        }
        else {
            instance.pies[id] = new google.visualization.PieChart(document.getElementById(id));
            return instance.pies[id];
        }
    };

    instance.getTimeline = function (id) {
        if (instance.timelines[id]) {
            return instance.timelines[id];
        }
        else {
            instance.timelines[id] = new google.visualization.Timeline(document.getElementById(id));
            return instance.timelines[id];
        }
    };

    instance.getCombo = function (id) {
        if (instance.combos[id]) {
            return instance.combos[id];
        }
        else {
            instance.combos[id] = new google.visualization.ComboChart(document.getElementById(id));
            return instance.combos[id];
        }
    };

    instance.getLine = function (id) {
        if (instance.lines[id]) {
            return instance.lines[id];
        }
        else {
            instance.lines[id] = new google.visualization.LineChart(document.getElementById(id));
            return instance.lines[id];
        }
    };

    instance.getBubble = function (id) {
        if (instance.bubbles[id]) {
            return instance.bubbles[id];
        }
        else {
            instance.bubbles[id] = new google.visualization.BubbleChart(document.getElementById(id));
            return instance.bubbles[id];
        }
    };

    instance.getDt = function (id) {
        if (instance.dts[id]) {
            return instance.dts[id];
        }

        return null;
    };

    instance.setDt = function (id, dt) {
        instance.dts[id] = dt;
    };

    instance.init = function (module, drawCharts) {
        google.charts.load('current', { 'packages': module});
        google.charts.setOnLoadCallback(drawCharts);
    }

    instance.drawPieChart = function (id, dt, option) {
        if (!document.getElementById(id)) {
            return;
        }

        var chart = instance.getPie(id);
        instance.setDt(id, dt);
        chart.draw(dt, option);
    };

    instance.drawLineChart = function (id, dt, option) {
        if (!document.getElementById(id)) {
            return;
        }

        if (dt.length < 2) {
            return;
        }

        var chart = instance.getLine(id);
        instance.setDt(id, dt);
        chart.draw(dt, option);
    };

    //instance.drawLineChart = function (id, dt, option) {
    //    if (!document.getElementById(id)) {
    //        return;
    //    }

    //    if (dt.length < 2) {
    //        return;
    //    }

    //    var chart = new google.charts.Line(document.getElementById(id));
    //    chart.draw(dt, option);
    //};

    instance.drawComboChart = function (id, dt, option, selectHandler) {
        if (!document.getElementById(id)) {
            return;
        }

        if (dt.length < 2) {
            return;
        }

        var chart = instance.getCombo(id);
        instance.setDt(id, dt);
        chart.draw(dt, option);

        if (selectHandler){
            google.visualization.events.addListener(chart, 'select', selectHandler);
        }
    };

    instance.drawTimelineChart = function (id, dt, option, selectHandler) {
        if (!document.getElementById(id)) {
            return;
        }

        if (dt.length < 2) {
            return;
        }

        var chart = instance.getTimeline(id);
        instance.setDt(id, dt);
        chart.draw(dt, option);

        if (selectHandler) {
            google.visualization.events.addListener(chart, 'select', selectHandler);
        }
    };

    instance.drawBubbleChart = function (id, dt, option, selectHandler) {
        if (!document.getElementById(id)) {
            return;
        }

        if (dt.length < 2) {
            return;
        }

        var chart = instance.getBubble(id);
        instance.setDt(id, dt);
        chart.draw(dt, option);

        if (selectHandler) {
            google.visualization.events.addListener(chart, 'select', selectHandler);
        }
    };

    return instance;
})();