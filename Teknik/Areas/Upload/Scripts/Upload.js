﻿$(document).ready(function () {
    $("#upload-links").css('display', 'none', 'important');
    $("#upload-links").html('');

    $("[name='saveKey']").bootstrapSwitch();
    $("[name='serverSideEncrypt']").bootstrapSwitch();
});

function linkSaveKey(selector, uploadID, key, fileID) {
    $(selector).click(function () {
        $.ajax({
            type: "POST",
            url: saveKeyToServerURL,
            data: { file: uploadID, key: key },
            success: function (html) {
                if (html.result) {
                    $('#key-link-' + fileID).html('<button type="button" class="btn btn-default btn-sm" id="remove-key-link-' + fileID + '">Remove Key From Server</button>');
                    $('#upload-link-' + fileID).html('<p><a href="' + html.result + '" id="full-url-link-' + fileID + '" target="_blank" class="alert-link">' + html.result + '</a></p>');
                    linkRemoveKey('#remove-key-link-' + fileID + '', uploadID, key, fileID);
                    $('#shortenUrl-button-' + fileID).prop('disabled', false);
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
                }
            }
        });
        return false;
    });
}

function linkRemoveKey(selector, uploadID, key, fileID) {
    $(selector).click(function () {
        $.ajax({
            type: "POST",
            url: removeKeyFromServerURL,
            data: { file: uploadID, key: key },
            success: function (html) {
                if (html.result) {
                    $('#key-link-' + fileID).html('<button type="button" class="btn btn-default btn-sm" id="save-key-link-' + fileID + '">Save Key To Server</button>');
                    $('#upload-link-' + fileID).html('<p><a href="' + html.result + '#' + key + '" id="full-url-link-' + fileID + '" target="_blank" class="alert-link">' + html.result + '#' + key + '</a></p>');
                    linkSaveKey('#save-key-link-' + fileID + '', uploadID, key, fileID);
                    $('#shortenUrl-button-' + fileID).prop('disabled', false);
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
                }
            }
        });
        return false;
    });
}

function linkUploadDelete(selector, uploadID) {
    $(selector).click(function () {
        $.ajax({
            type: "POST",
            url: generateDeleteKeyURL,
            data: { file: uploadID },
            success: function (html) {
                if (html.result) {
                    bootbox.dialog({
                        title: "Direct Deletion URL",
                        message: '<input type="text" class="form-control" id="deletionLink" onClick="this.select();" value="' + html.result + '">'
                    });

                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
                }
            }
        });
        return false;
    });
}

function linkShortenUrl(selector, fileID, url) {
    $(selector).click(function () {
        var url = $('#full-url-link-' + fileID).html();
        $.ajax({
            type: "POST",
            url: shortenURL,
            data: { url: url },
            success: function (html) {
                if (html.result) {
                    $(selector).prop('disabled', true);
                    $('#upload-link-' + fileID).html('<p><a href="' + html.result.shortUrl + '" id="full-url-link-' + fileID + '" target="_blank" class="alert-link">' + html.result.shortUrl + '</a></p>');
                }
                else {
                    $("#top_msg").css('display', 'inline', 'important');
                    $("#top_msg").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + html.error + '</div>');
                }
            }
        });
        return false;
    });
}

function linkRemove(selector, fileID) {
    $(selector).click(function () {
        $('#link-' + fileID).remove();
        return false;
    });
}

function linkCancel(selector, fileID) {
    $(selector).click(function () {
        $('#link-' + fileID).remove();
        return false;
    });
}

var fileCount = 0;

var dropZone = new Dropzone(document.body, {
    url: uploadFileURL, 
    maxFilesize: maxUploadSize, // MB
    addRemoveLinks: true,
    autoProcessQueue: false,
    clickable: "#uploadButton",
    previewTemplate: function () { },
    addedfile: function (file) {
        // Create the UI element for the new item
        var fileID = fileCount;
        fileCount++;
        // save ID to the file object
        file.ID = fileID;
        $("#upload-links").css('display', 'inline', 'important');
        $("#upload-links").prepend(' \
                <div class="panel panel-default" id="link-' + fileID + '"> \
                    <div class="panel-heading text-center" id="link-header-' + fileID + '">'+ file.name + '</div> \
                    <div class="panel-body" id="link-panel-' + fileID + '"> \
                        <div class="row"> \
                            <div class="col-sm-12 text-center" id="upload-link-' + fileID + '"></div> \
                        </div> \
                        <div class="row"> \
                            <div class="col-sm-12 text-center"> \
                                <div class="progress" id="progress-' + fileID + '"> \
                                    <div class="progress-bar progress-bar-success progress-bar-striped active" role="progressbar" aria-valuenow="100" aria-valuemin="0" aria-valuemax="100" style="width: 0%">0%</div> \
                                </div> \
                            </div> \
                        </div> \
                    </div> \
                    <div class="panel-footer" id="link-footer-' + fileID + '"> \
                        <div class="row"> \
                            <div class="col-sm-12 text-center"> \
                                <button type="button" class="btn btn-default btn-sm" id="remove-link-' + fileID + '">Cancel Upload</button> \
                            </div> \
                        </div> \
                    </div> \
                </div> \
              ');

        linkRemove('#remove-link-' + fileID + '', fileID);

        // Check the file size
        if (file.size <= maxUploadSize) {
            // Encrypt the file and upload it
            encryptFile(file, uploadFile);
        }
        else
        {
            // An error occured
            $("#progress-" + fileID).children('.progress-bar').css('width', '100%');
            $("#progress-" + fileID).children('.progress-bar').removeClass('progress-bar-success');
            $("#progress-" + fileID).children('.progress-bar').removeClass('progress-bar-striped');
            $("#progress-" + fileID).children('.progress-bar').removeClass('active');
            $("#progress-" + fileID).children('.progress-bar').addClass('progress-bar-danger');
            $("#progress-" + fileID).children('.progress-bar').html('File Too Large');
        }
        this.removeFile(file);
    }
});

// Function to encrypt a file, overide the file's data attribute with the encrypted value, and then call a callback function if supplied
function encryptFile(file, callback) {
    var filetype = file.type;
    var fileID = file.ID;
    var fileExt = getFileExtension(file.name);

    // Get session settings
    var saveKey = $('#saveKey').is(':checked');
    var serverSideEncrypt = $('#serverSideEncrypt').is(':checked');

    // Start the file reader
    var reader = new FileReader();

    // When the file has been loaded, encrypt it
    reader.onload = (function (callback) {
        return function (e) {
            // Create random key and iv (divide size by 8 for character length)
            var keyStr = randomString((keySize / 8), '#aA');
            var ivStr = randomString((blockSize / 8), '#aA');

            // Encrypt on the server side if they ask for it
            if (serverSideEncrypt) {
                callback(e.target.result, keyStr, ivStr, filetype, fileExt, fileID, saveKey, serverSideEncrypt);
            }
            else {
                var worker = new Worker(GenerateBlobURL(encScriptSrc));

                worker.addEventListener('message', function (e) {
                    switch (e.data.cmd) {
                        case 'progress':
                            var percentComplete = Math.round(e.data.processed * 100 / e.data.total);
                            $("#progress-" + fileID).children('.progress-bar').css('width', (percentComplete * (2 / 5)) + 20 + '%');
                            $("#progress-" + fileID).children('.progress-bar').html(percentComplete + '% Encrypted');
                            break;
                        case 'finish':
                            if (callback != null) {
                                // Finish 
                                callback(e.data.buffer, keyStr, ivStr, filetype, fileExt, fileID, saveKey, serverSideEncrypt);
                            }
                            break;
                    }
                });

                worker.onerror = function (err) {
                    // An error occured
                    $("#progress-" + fileID).children('.progress-bar').css('width', '100%');
                    $("#progress-" + fileID).children('.progress-bar').removeClass('progress-bar-success');
                    $("#progress-" + fileID).children('.progress-bar').removeClass('progress-bar-striped');
                    $("#progress-" + fileID).children('.progress-bar').removeClass('active');
                    $("#progress-" + fileID).children('.progress-bar').addClass('progress-bar-danger');
                    $("#progress-" + fileID).children('.progress-bar').html('Error Occured');
                }

                // Generate the script to include as a blob
                var scriptBlob = GenerateBlobURL(aesScriptSrc);

                // Execute worker with data
                var objData =
                    {
                        cmd: 'encrypt',
                        script: scriptBlob,
                        key: keyStr,
                        iv: ivStr,
                        chunkSize: chunkSize,
                        file: e.target.result
                    };
                worker.postMessage(objData, [objData.file]);
            }
        };
    })(callback);

    reader.onprogress = function (data) {
        if (data.lengthComputable) {
            var progress = parseInt(((data.loaded / data.total) * 100), 10);
            $('#progress-' + fileID).children('.progress-bar').css('width', (progress / 5) + '%');
            $('#progress-' + fileID).children('.progress-bar').html(progress + '% Loaded');
        }
    }

    // Start async read
    var blob = file.slice(0, file.size);
    reader.readAsArrayBuffer(blob);
}

function uploadFile(data, key, iv, filetype, fileExt, fileID, saveKey, serverSideEncrypt)
{
    $('#key-' + fileID).val(key);
    var blob = new Blob([data]);
    // Now we need to upload the file
    var fd = new FormData();
    fd.append('fileType', filetype);
    fd.append('fileExt', fileExt);
    if (saveKey)
    {
        fd.append('key', key);
    }
    fd.append('iv', iv);
    fd.append('keySize', keySize);
    fd.append('blockSize', blockSize);
    fd.append('data', blob);
    fd.append('saveKey', saveKey);
    fd.append('encrypt', serverSideEncrypt);
    fd.append('__RequestVerificationToken', $('#__AjaxAntiForgeryForm input[name=__RequestVerificationToken]').val());

    var xhr = new XMLHttpRequest();
    xhr.upload.addEventListener("progress", uploadProgress.bind(null, fileID), false);
    xhr.addEventListener("load", uploadComplete.bind(null, fileID, key, saveKey, serverSideEncrypt), false);
    xhr.addEventListener("error", uploadFailed.bind(null, fileID), false);
    xhr.addEventListener("abort", uploadCanceled.bind(null, fileID), false);
    xhr.open("POST", uploadFileURL);
    xhr.send(fd);
}

function uploadProgress(fileID, evt) {
    var serverSideEncrypt = $('#serverSideEncrypt').is(':checked');
    if (evt.lengthComputable) {
        var percentComplete = Math.round(evt.loaded * 100 / evt.total);
        if (serverSideEncrypt && percentComplete == 100) {
            $('#progress-' + fileID).children('.progress-bar').css('width', '100%');
            $('#progress-' + fileID).children('.progress-bar').html('Encrypting');
        }
        else {
            $('#progress-' + fileID).children('.progress-bar').css('width', (percentComplete * (2 / 5)) + 60 + '%');
            $('#progress-' + fileID).children('.progress-bar').html(percentComplete + '% Uploaded');
        }
    }
}

function uploadComplete(fileID, key, saveKey, serverSideEncrypt, evt) {
    obj = JSON.parse(evt.target.responseText);
    if (obj.result != null) {
        var name = obj.result.name;
        var fullName = obj.result.url;
        if (obj.result.key != null)
            key = obj.result.key;
        if (!saveKey) {
            fullName = fullName + '#' + key;
        }
        $('#progress-' + fileID).children('.progress-bar').css('width', '100%');
        $("#progress-" + fileID).children('.progress-bar').removeClass('progress-bar-striped');
        $("#progress-" + fileID).children('.progress-bar').removeClass('active');
        $('#progress-' + fileID).children('.progress-bar').html('Complete');
        $('#upload-link-' + fileID).html('<p><a href="' + fullName + '" id="full-url-link-' + fileID + '" target="_blank" class="alert-link">' + fullName + '</a></p>');
        var keyBtn = '<div class="col-sm-4 text-center" id="key-link-' + fileID + '"> \
                    <button type="button" class="btn btn-default btn-sm" id="save-key-link-' + fileID + '">Save Key On Server</button> \
                </div>';
        if (saveKey) {
            keyBtn = '<div class="col-sm-4 text-center" id="key-link-' + fileID + '"> \
                    <button type="button" class="btn btn-default btn-sm" id="remove-key-link-' + fileID + '">Remove Key From Server</button> \
                </div>';
        }
        $('#link-footer-' + fileID).html(' \
                    <div class="row"> \
                        ' + keyBtn + ' \
                        <div class="col-sm-4 text-center"> \
                            <button type="button" class="btn btn-default btn-sm" id="generate-delete-link-' + fileID + '">Generate Deletion URL</button> \
                        </div> \
                        <div class="col-sm-4 text-center"> \
                            <button type="button" class="btn btn-default btn-sm" id="shortenUrl-button-' + fileID + '">Shorten Url</button> \
                        </div> \
                    </div> \
              ');
        if (saveKey) {
            linkRemoveKey('#remove-key-link-' + fileID + '', name, key, fileID);
        }
        else {
            linkSaveKey('#save-key-link-' + fileID + '', name, key, fileID);
        }
        linkUploadDelete('#generate-delete-link-' + fileID + '', name);
        linkShortenUrl('#shortenUrl-button-' + fileID + '', fileID, fullName);
    }
    else
    {
        $('#progress-' + fileID).children('.progress-bar').css('width', '100%');
        $("#progress-" + fileID).children('.progress-bar').removeClass('progress-bar-success');
        $("#progress-" + fileID).children('.progress-bar').removeClass('progress-bar-striped');
        $("#progress-" + fileID).children('.progress-bar').removeClass('active');
        $("#progress-" + fileID).children('.progress-bar').addClass('progress-bar-danger');
        $('#remove-link-' + fileID).text('Clear Upload');
        if (obj.error != null) {
            $('#progress-' + fileID).children('.progress-bar').html(obj.error.message);
        }
        else {
            $('#progress-' + fileID).children('.progress-bar').html('Unable to Upload File');
        }
    }
}

function uploadFailed(fileID, evt) {
    $('#progress-' + fileID).children('.progress-bar').css('width', '100%');
    $("#progress-" + fileID).children('.progress-bar').removeClass('progress-bar-success');
    $("#progress-" + fileID).children('.progress-bar').removeClass('progress-bar-striped');
    $("#progress-" + fileID).children('.progress-bar').removeClass('active');
    $("#progress-" + fileID).children('.progress-bar').addClass('progress-bar-danger');
    $('#progress-' + fileID).children('.progress-bar').html('Upload Failed');
}

function uploadCanceled(fileID, evt) {
    $('#progress-' + fileID).children('.progress-bar').css('width', '100%');
    $("#progress-" + fileID).children('.progress-bar').removeClass('progress-bar-success');
    $("#progress-" + fileID).children('.progress-bar').removeClass('progress-bar-striped');
    $("#progress-" + fileID).children('.progress-bar').removeClass('active');
    $("#progress-" + fileID).children('.progress-bar').addClass('progress-bar-warning');
    $('#progress-' + fileID).children('.progress-bar').html('Upload Canceled');
}