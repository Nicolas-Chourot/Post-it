// https://stackoverflow.com/questions/61319169/visual-studio-warning-eslint-failed-to-load-config-defaults-configurations-e

function DoPartialRefresh(serviceURL, divContainerId, callBack = null) {
    console.log("posts partial refresh");
    $.ajax({
        url: serviceURL,
        dataType: "html",
        success: function (htmlContent) {
            if (htmlContent !== "") {
                $("#" + divContainerId).html(htmlContent);
                callBack();
            }
        }
    })
}

function installPartialRefresh(serviceURL, divContainerId, refreshRate, callBack = null) {
    console.log("intallation of partial refresh");
    setInterval(() => { DoPartialRefresh(serviceURL, divContainerId, callBack); }, refreshRate * 1000);
}


