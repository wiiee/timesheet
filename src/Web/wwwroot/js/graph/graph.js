$(function () {
    google.charts.load('current', { 'packages': ['corechart'] });
    google.charts.setOnLoadCallback(drawLineChart);
    //google.charts.setOnLoadCallback(drawPieChart);

    function drawLineChart() {
        var data = google.visualization.arrayToDataTable(_lineData);

        var options = {
            title: 'My Line Chart',
            curveType: 'function',
            legend: { position: 'bottom' }
        };

        var chart = new google.visualization.LineChart(document.getElementById("lineChart"));

        chart.draw(data, options);
    }

    //function drawPieChart() {
    //    var data = google.visualization.arrayToDataTable(_pieData);

    //    var options = {
    //        title: 'My Pie Chart'
    //    };

    //    var chart = new google.visualization.PieChart(document.getElementById('piechart'));

    //    chart.draw(data, options);
    //}
});