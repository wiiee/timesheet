$(function () {
    $("#ownerIds, #userGroupIds").multiselect({
        enableFiltering: true,
        buttonWidth: "100%"
    });

    $.validate({
        onSuccess: function ($form) {
            var isTest = $("#isTest").val();
            if (isTest == "on" || isTest == "true" || isTest == "1")
                $("#isTest").val("true");
        }
    });
});