$(function () {
    $("body").on("click", "a.delete", function (evt) {
        //evt.stopPropagation();
        evt.stopImmediatePropagation();
        var element = $(this);
        bootbox.confirm("Are you sure to delete this feedback?", function (result) {
            if (result) {
                location = _basePath + "/Feedback/DeleteFeedback?id=" + encodeURIComponent(element.attr("data-id"));
            }
        });
    });

    $(document).tooltip();
});