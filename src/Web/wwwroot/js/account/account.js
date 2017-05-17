$(function () {
    $("body").on("click", "#signUp", function (a) {
        var url = _basePath + "/api/account/signup";
        var data = {
            Id: $("#emailSignUp").val(),
            Password: $("#passwordSignUp").val()
        };

        $.ajax({
            url: url,
            type: "POST",
            dataType: "json",
            data: JSON.stringify(data),
            contentType: "application/json",
            success: function (result) {
                if (result.Status == 1) {
                    $("#signUpModal").modal("hide");
                }
            }
        });
    });

    $("body").on("click", "#logIn", function (a) {
        var url = _basePath + "/api/account/signup";
        var data = {
            id: $("#emailSignUp").val(),
            password: $("#passwordSignUp").val()
        };

        $.ajax({
            url: url,
            type: "POST",
            dataType: "json",
            data: data,
            contentType: "application/json",
            success: function (result) {
                if (result.Status == 1) {
                    $("#signUpModal").modal("hide");
                }
            }
        });
    });

    $.validate({
        form: "#signUpForm"
    });
});