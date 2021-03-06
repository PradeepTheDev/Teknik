$(document).ready(function () {
    $("#top_msg").css('display', 'none', 'important');

    // Opt-in for tooltips
    $('[data-toggle="tooltip"]').tooltip();

    $('#loginButton').removeClass('hide');

    $('#loginModal').on('shown.bs.modal', function (e) {
        $("#loginStatus").css('display', 'none', 'important');
        $("#loginStatus").html('');
        $('#loginUsername').focus();
    });

    $("#loginSubmit").click(function () {
        // Reset login status messages
        $("#loginStatus").css('display', 'none', 'important');
        $("#loginStatus").html('');

        // Disable the login button
        disableButton('#loginSubmit', 'Signing In...');

        var form = $('#loginForm');
        $.ajax({
            type: "POST",
            url: form.attr('action'),
            data: form.serialize(),
            headers: {'X-Requested-With': 'XMLHttpRequest'},
            xhrFields: {
                withCredentials: true
            },
            success: function (html) {
                if (html.result) {
                    window.location = html.result;
                }
                else {
                    $("#loginStatus").css('display', 'inline', 'important');
                    $("#loginStatus").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(html) + '</div>');
                }

                // Re-enable the login button
                enableButton('#loginSubmit', 'Sign In');
            }
        });
        return false;
    });

    $('#registerButton').removeClass('hide');

    $('#registerModal').on('shown.bs.modal', function (e) {
        $("#registerStatus").css('display', 'none', 'important');
        $("#registerStatus").html('');
        $('#registerUsername').focus();
    });

    $("#registerSubmit").click(function () {
        // Reset register status messages
        $("#registerStatus").css('display', 'none', 'important');
        $("#registerStatus").html('');

        // Disable the register button
        disableButton('#registerSubmit', 'Signing In...');

        var form = $('#registrationForm');
        $.ajax({
            type: "POST",
            url: form.attr('action'),
            data: form.serialize(),
            headers: {'X-Requested-With': 'XMLHttpRequest'},
            xhrFields: {
                withCredentials: true
            },
            success: function (html) {
                if (html.result) {
                    window.location.reload();
                }
                else {
                    $("#registerStatus").css('display', 'inline', 'important');
                    $("#registerStatus").html('<div class="alert alert-danger alert-dismissable"><button type="button" class="close" data-dismiss="alert" aria-hidden="true">&times;</button>' + parseErrorMessage(html) + '</div>');
                }

                // Re-enable the register button
                enableButton('#registerSubmit', 'Sign Up');
            }
        });
        return false;
    });


});

$(function () {
    // Setup drop down menu
    $('.dropdown-toggle').dropdown();

    $(".alert").alert();

    $("#blocker").hide();

    // Fix input element click problem
    $('.dropdown input, .dropdown label').click(function (e) {
        e.stopPropagation();
    });

    // for bootstrap 3 use 'shown.bs.tab', for bootstrap 2 use 'shown' in the next line
    $('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
        // save the latest tab; use cookies if you like 'em better:
        localStorage.setItem('lastTab', $(this).attr('href'));
    });

    // go to the latest tab, if it exists:
    var lastTab = localStorage.getItem('lastTab');
    if (lastTab) {
        $('[href="' + lastTab + '"]').tab('show');
    }

    // Auo-select bitcoin address
    $('#bitcoin_address_footer').click(function() {
        SelectAll('bitcoin_address_footer');
    });

    // Setup anti-forgery functions
    $.appendAntiForgeryToken = function (data, token) {
        // Converts data if not already a string.
        if (data && typeof data !== "string") {
            data = $.param(data);
        }

        // Gets token from current window by default.
        token = token ? token : $.getAntiForgeryToken(); // $.getAntiForgeryToken(window).

        data = data ? data + "&" : "";
        // If token exists, appends {token.name}={token.value} to data.
        return token ? data + encodeURIComponent(token.name) + "=" + encodeURIComponent(token.value) : data;
    };

    $.getAntiForgeryToken = function (tokenWindow, appPath) {
        // HtmlHelper.AntiForgeryToken() must be invoked to print the token.
        tokenWindow = tokenWindow && typeof tokenWindow === typeof window ? tokenWindow : window;

        appPath = appPath && typeof appPath === "string" ? "_" + appPath.toString() : "";
        // The name attribute is either __RequestVerificationToken,
        // or __RequestVerificationToken_{appPath}.
        var tokenName = "__RequestVerificationToken" + appPath;

        // Finds the <input type="hidden" name={tokenName} value="..." /> from the specified window.
        // var inputElements = tokenWindow.$("input[type='hidden'][name=' + tokenName + "']");
        var inputElements = tokenWindow.document.getElementsByTagName("input");
        for (var i = 0; i < inputElements.length; i++) {
            var inputElement = inputElements[i];
            if (inputElement.type === "hidden" && inputElement.name === tokenName) {
                return {
                    name: tokenName,
                    value: inputElement.value
                };
            }
        }
    };
});

function disableButton(btn, text) {
    $(btn).addClass('disabled');
    $(btn).text(text);
}

function enableButton(btn, text) {
    $(btn).removeClass('disabled');
    $(btn).text(text);
}

function removeAmp(code) {
    code = code.replace(/&amp;/g, '&');
    return code;
}

String.prototype.hashCode = function () {
    var hash = 0, i, chr, len;
    if (this.length == 0) return hash;
    for (i = 0, len = this.length; i < len; i++) {
        chr = this.charCodeAt(i);
        hash = ((hash << 5) - hash) + chr;
        hash |= 0; // Convert to 32bit integer
    }
    return hash;
};

function randomString(length, chars) {
    var mask = '';
    if (chars.indexOf('a') > -1) mask += 'abcdefghijklmnopqrstuvwxyz';
    if (chars.indexOf('A') > -1) mask += 'ABCDEFGHIJKLMNOPQRSTUVWXYZ';
    if (chars.indexOf('#') > -1) mask += '0123456789';
    if (chars.indexOf('!') > -1) mask += '~`!@#$%^&*()_+-={}[]:";\'<>?,./|\\';
    var result = '';
    for (var i = length; i > 0; --i) result += mask[Math.floor(Math.random() * mask.length)];
    return result;
}

function getFileExtension(fileName) {
    var index = fileName.lastIndexOf('.');
    if (index >= 0 && fileName.length > 0) {
        return '.' + fileName.substr(index + 1);
    }
    return '';
}

function SelectAll(id) {
    document.getElementById(id).focus();
    document.getElementById(id).select();
}

function getAnchor() {
    var currentUrl = document.URL,
    urlParts = currentUrl.split('#');

    return (urlParts.length > 1) ? urlParts[1] : null;
}

function GenerateBlobURL(url) {
    var cachedBlob = null;
    jQuery.ajax({
        url: url,
        success: function (result) {
            var workerJSBlob = new Blob([result], {
                type: "text/javascript"
            });
            cachedBlob = window.URL.createObjectURL(workerJSBlob);
        },
        async: false
    });
    return cachedBlob;
}

AddAntiForgeryToken = function (data) {
    data.__RequestVerificationToken = $('#__AjaxAntiForgeryForm input[name=__RequestVerificationToken]').val();
    return data;
};

function copyTextToClipboard(text) {
    var copyFrom = document.createElement("textarea");
    copyFrom.textContent = text;
    var body = document.getElementsByTagName('body')[0];
    body.appendChild(copyFrom);
    copyFrom.select();
    document.execCommand('copy');
    body.removeChild(copyFrom);
}

function getReadableBandwidthString(bandwidth) {

    var i = -1;
    var byteUnits = [' Kbps', ' Mbps', ' Gbps', ' Tbps', 'Pbps', 'Ebps', 'Zbps', 'Ybps'];
    do {
        bandwidth = bandwidth / 1024;
        i++;
    } while (bandwidth > 1024);

    return Math.max(bandwidth, 0.1).toFixed(1) + byteUnits[i];
}

function getReadableFileSizeString(fileSizeInBytes) {

    var i = -1;
    var byteUnits = [' KB', ' MB', ' GB', ' TB', 'PB', 'EB', 'ZB', 'YB'];
    do {
        fileSizeInBytes = fileSizeInBytes / 1024;
        i++;
    } while (fileSizeInBytes > 1024);

    return Math.max(fileSizeInBytes, 0.1).toFixed(1) + byteUnits[i];
};

function moveUp(item) {
    var prev = item.prev();
    if (prev.length == 0)
        return;
    prev.css('z-index', 999).css('position', 'relative').animate({ top: item.height() }, 250);
    item.css('z-index', 1000).css('position', 'relative').animate({ top: '-' + prev.height() }, 300, function () {
        prev.css('z-index', '').css('top', '').css('position', '');
        item.css('z-index', '').css('top', '').css('position', '');
        item.insertBefore(prev);
    });
}

function moveDown(item) {
    var next = item.next();
    if (next.length == 0)
        return;
    next.css('z-index', 999).css('position', 'relative').animate({ top: '-' + item.height() }, 250);
    item.css('z-index', 1000).css('position', 'relative').animate({ top: next.height() }, 300, function () {
        next.css('z-index', '').css('top', '').css('position', '');
        item.css('z-index', '').css('top', '').css('position', '');
        item.insertAfter(next);
    });
}

function addParamsToUrl(origUrl, params) {
    var paramStr = $.param(params);
    var hasQuery = origUrl.indexOf("?") + 1;
    var hasHash = origUrl.indexOf("#") + 1;
    var appendix = (hasQuery ? "&" : "?") + paramStr;
    return hasHash ? origUrl.replace("#", appendix + "#") : origUrl + appendix;
}

function sleep(ms) {
    return new Promise(resolve => setTimeout(resolve, ms));
}

function parseErrorMessage(errorObj) {
    var errorMsg = "";
    if (errorObj === null || errorObj === undefined) {
        // Do nothing
    }
    else if (errorObj.constructor === {}.constructor) {
        if (errorObj.error) {
            errorMsg = errorObj.error;
            if (errorObj.error.message) {
                errorMsg = errorObj.error.message;
            }
        }
    }
    else {
        try {
            var json = $.parseJSON(errorObj);
            if (json.error) {
                errorMsg = json.error;
                if (json.error.message) {
                    errorMsg = json.error.message;
                }
            }
        }
        catch (err) {
            // Don't do anything
        }
    }
    return errorMsg;
}

/***************************** TIMER Page Load *******************************/
var loopTime;
var startTime = new Date();
var pageGenerationTime = "0.0";

function pageloadTimerCount() {
    loopTime = setTimeout("pageloadTimerCount()", 100);
}

function pageloadDoTimer() {
    pageloadTimerCount();
}

function pageloadStopTimer() {
    var timeMs = Math.floor((new Date() - startTime));

    $('#loadtime').html(timeMs);
    $('#generatetime').html(pageGenerationTime);
    $('#pagetime').show();

    clearTimeout(loopTime);
}
