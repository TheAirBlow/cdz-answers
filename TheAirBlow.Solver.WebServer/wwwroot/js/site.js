/* Copyright © 2022 TheAirBlow - https://github.com/theairblow/cdz-answers */

setInterval(function() {
    // Удалим всю рекламу нахуй
    $("center, script:contains(aScr), div[style=\"height: 65px;\"], div[style=\"opacity: 0.9; z-index: 2147483647; position: fixed; left: 0px; bottom: 0px; height: 65px; right: 0px; display: block; width: 100%; background-color: #202020; margin: 0px; padding: 0px;\"], div[onmouseover=\"S_ssac();\"], script[src=\"http://ads.mgmt.somee.com/serveimages/ad2/WholeInsert5.js\"]").remove();
}, 150);

// Query-параметры
const params = new URLSearchParams(window.location.search)

// Отправить GET запрос
function sendRequest(link, func) {
    $.get(link, func);
}

$(function() {
    // JQuery "contains", но не case-sensitive
    jQuery.expr[':'].icontains = function(a, i, m) {
        return jQuery(a).text().toUpperCase()
            .indexOf(m[3].toUpperCase()) >= 0;
    };
    $("body").delegate(".mainbox", "click", function() {
        // Делаем бокс зеленым или обычного цвета
        if (window.getSelection().toString() !== "") return;
        if ($(this).hasClass("mainbox-normal")) {
            $(this).removeClass("mainbox-normal")
            $(this).addClass("mainbox-clicked")
        } else if ($(this).hasClass("mainbox-clicked")) {
            $(this).removeClass("mainbox-clicked")
            $(this).addClass("mainbox-normal")
        }
    }).on("input", ".search", function() {
        // Поиск текста во всех боксах
        $(`.mainbox:not(:icontains(${$(this).val()})):not(.template)`).hide();
        $(`.mainbox:icontains(${$(this).val()}):not(.template)`).show();
    });
});