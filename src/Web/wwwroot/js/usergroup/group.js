$(function () {
    $("#userIds, #ownerIds").multiselect({
        enableFiltering: true,
        buttonWidth: "100%"
    });

    $.validate();
});