$(function () {
    $("body").on("click", "a.delete", function (evt) {
        //evt.stopPropagation();
        evt.stopImmediatePropagation();
        var element = $(this);
        bootbox.confirm("Are you sure to delete this user?", function (result) {
            if (result) {
                location = _basePath + "/User/DeleteUser?id=" + encodeURIComponent(element.attr("data-id"));
            }
        });
    });
});