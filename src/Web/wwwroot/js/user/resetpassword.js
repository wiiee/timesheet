$(function () {
    $.validate({
        onSuccess: function ($form) {
            var rawOldPassword = $("#rawOldPassword").val();
            var rawNewPassword = $("#rawNewPassword").val();

            $("#oldPassword").val(utility.getMd5(rawOldPassword));
            $("#newPassword").val(utility.getMd5(rawNewPassword));
        }
    });
});