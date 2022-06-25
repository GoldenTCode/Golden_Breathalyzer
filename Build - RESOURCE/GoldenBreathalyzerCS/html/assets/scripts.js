$(document).ready(function() {
    window.addEventListener('message', (event) => {
        var breathalyzer = event.data;

        if (breathalyzer.action === "showBreathUI") {
            if (breathalyzer.showUI == false) {
                $(".breathText").animate({opacity: "0.0"}, "100", function() {
                    $(".breathUI").animate({bottom: "-420px"}, "slow", function() {
                        $(".breathText").text(breathalyzer.showBAC);
                    });
                });
            } else {
                $(".breathUI").animate({bottom: "-4px"}, "slow", function() {
                    $(".breathText").text(breathalyzer.showBAC);
                    $(".breathText").animate({opacity: "1.0"}, "100");
                });
            }
        }
    });
});