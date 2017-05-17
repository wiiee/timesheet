$(function () {
    $("body").on("click", "a.return", function (evt) {
        //evt.stopPropagation();
        evt.stopImmediatePropagation();
        var element = $(this);
        bootbox.confirm("Are you sure to return this timesheet?", function (result) {
            if (result) {
                location = _basePath + "/TimeSheet/ReturnTimeSheet?monday=" + element.attr("data-monday") + "&userId=" + element.attr("data-userId");
            }
        });
    });
});