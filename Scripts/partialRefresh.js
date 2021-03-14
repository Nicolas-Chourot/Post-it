function PartialRefresh(serviceURL, divContainerId, refreshRate) {
    setInterval(() => {
            $.ajax({
                    url: serviceURL,
                    dataType: "html",
                success: function (htmlContent) {
                    console.log(htmlContent);
                        if (htmlContent !== "")
                            $("#" + divContainerId).html(htmlContent);
                    }
                })
            }
            , refreshRate * 1000);
}