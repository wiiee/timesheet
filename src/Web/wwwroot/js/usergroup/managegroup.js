$(function () {
    $("body").on("click", "a.delete", function (evt) {
        //evt.stopPropagation();
        evt.stopImmediatePropagation();
        var element = $(this);
        bootbox.confirm("Are you sure to delete this group?", function (result) {
            if (result)
            {
                location = _basePath + "/UserGroup/DeleteGroup?id=" + encodeURIComponent(element.attr("data-id"));
            }
        });
    });
});