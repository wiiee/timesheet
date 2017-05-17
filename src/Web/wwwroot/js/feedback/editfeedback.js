$(function () {
    $("#imgs").fileinput({
        uploadUrl: _basePath + "/api/img/uploads",
        overwriteInitial: false,
        initialPreview: _initialPreview,
        initialPreviewConfig: _initialPreviewConfig,
        uploadExtraData: {
            feedbackId: _feedbackId,
        },
        uploadAsync: false,
        showUpload: false, // hide upload button
        showRemove: false, // hide remove button
        previewFileType: "image",
        allowedFileExtensions: ["jpg", "png", "gif"],
        resizePreference: 'height',
        maxFileCount: 3,
        maxFileSize: 100,
        resizeImage: true
    }).on("filebatchselected", function (event, files) {
        // trigger upload method immediately after files are selected
        $("#imgs").fileinput("upload");
    }).on('filebatchuploadsuccess', function (event, data) {
        $.each(data.response.initialPreviewConfig, function (index, element) {
            $("form").prepend('<input type="hidden" name="imgIds" value=' + element.key + ' />');
        });
    }).on("filepredelete", function (event, key) {
        var abort = true;

        if (confirm("Are you sure you want to delete this image?")) {
            abort = false;
        }

        return abort; // you can also send any data/object that you can receive on `filecustomerror` event
    }).on('filedeleted', function (event, key) {
        $("input[value='" + key + "']").remove();
    });

    $.validate();
});