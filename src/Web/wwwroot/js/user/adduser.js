$(function () {
    $.validate({
        onSuccess: function ($form) {
            var rawPassword = $("#rawPassword").val();
            $("#password").val(utility.getMd5(rawPassword));
        }
    });
});