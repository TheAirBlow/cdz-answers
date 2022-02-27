const params = new URLSearchParams(window.location.search)
function sendRequest(link, func) {
    $.get(link, func);
}

$(function() {
    jQuery.expr[':'].icontains = function(a, i, m) {
        return jQuery(a).text().toUpperCase()
            .indexOf(m[3].toUpperCase()) >= 0;
    };
    $("body").delegate(".mainbox", "click", function() {
        if (window.getSelection().toString() !== "") return;
        if ($(this).hasClass("mainbox-normal")) {
            $(this).removeClass("mainbox-normal")
            $(this).addClass("mainbox-clicked")
        } else if ($(this).hasClass("mainbox-clicked")) {
            $(this).removeClass("mainbox-clicked")
            $(this).addClass("mainbox-normal")
        }
    }).on("input", ".search", function() {
        $(`.mainbox:not(:icontains(${$(this).val()})):not(.template)`).hide();
        $(`.mainbox:icontains(${$(this).val()}):not(.template)`).show();
    });
});