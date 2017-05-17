$(function () {
    $("body").on("click", "a.delete", function (evt) {
        //evt.stopPropagation();
        evt.stopImmediatePropagation();
        var element = $(this);
        bootbox.confirm("Are you sure to delete this department?", function (result) {
            if (result)
            {
                location = _basePath + "/Department/DeleteDepartment?id=" + encodeURIComponent(element.attr("data-id"));
            }
        });
    });

    $("body").on("click", "a.deleteGroup", function (evt) {
        //evt.stopPropagation();
        evt.stopImmediatePropagation();
        var element = $(this);
        bootbox.confirm("Are you sure to delete this userGroup?", function (result) {
            if (result) {
                location = _basePath + "/Department/DeleteGroup?id=" + encodeURIComponent(element.attr("data-id"));
            }
        });
    });

    $(document).tooltip();
});