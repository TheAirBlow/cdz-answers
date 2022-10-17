/* Copyright © 2022 TheAirBlow - https://github.com/theairblow/cdz-answers */

setInterval(function() {
    // Удалим всю рекламу нахуй (somee.com)
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
    }).delegate(".search", "input", function() {
        // Поиск текста во всех боксах
        $(`.mainbox:not(:icontains(${$(this).val()})):not(.template)`).slideUp("slow");
        $(`.mainbox:icontains(${$(this).val()}):not(.template)`).slideDown("slow");
    });
    
    $(".images").click(function() {
        // Скрываем картинки
        let input = $(".images-input");
        if (input.is(':checked')) {
            input.removeAttr('checked');
            $(`.image`).slideDown({ speed: "slow", queue: false });
        } else {
            input.attr('checked',
                true);
            $(`.image`).slideUp({ speed: "slow", queue: false });
        }
    });
    
    $(".videos").click(function() {
        // Скрываем видео
        let input = $(".videos-input");
        if (input.is(':checked')) {
            input.removeAttr('checked');
            $(`vim-video>div`).animate({
                'padding-bottom': '56.25%'
            }, { speed: "slow", queue: false });
            $(`iframe.video`).attr("style", "");
        } else {
            input.attr('checked',
                true);
            $(`iframe.video`).slideUp({ speed: "slow", queue: false });
            $(`vim-video>div`).animate({
                'padding-bottom': 0
            }, { speed: "slow", queue: false });
        }
    });

    let subst = window.location.href.substring(
        window.location.href.lastIndexOf('/') + 1);
    switch(subst.substring(0, subst.indexOf('?'))) {
        case "View1":
            sendRequest(`/API/RemoveFinishedItem?uuid=${params.get("uuid")}`,
                function(json) {
                    json = JSON.parse(json);
                    json.SolverOutput.forEach(function (i) {
                        let item = $(".template").clone();
                        item.appendTo(".answers");
                        item.find(".question").text($(i.Question).text());
                        item.find(".answer").text($(i.Answer).text());
                        item.removeClass("template");
                        item.show();
                    });
                    MathJax.Hub.Queue(["Typeset", MathJax.Hub, "TFS"]);
                    renderMathInElement(document.body);
                    $(".overlay").fadeOut(1000);
                });
            $("vim-example").each(function(i, e) {
                var div = jQuery('<div>', {
                    class: 'example'
                });
                div.insertAfter(e);
                $(this).appendTo(div);
            })
            break;
        case "View2":
            sendRequest(`/API/RemoveFinishedItem?uuid=${params.get("uuid")}`,
                function(json) {
                    json = JSON.parse(json);
                    $(".teacher").text(json.SolverOutput.TeacherName);
                    $(".subject").text(json.SolverOutput.SubjectName);
                    json.SolverOutput.Answers.forEach(function (i) {
                        let item = $(".template").clone();
                        item.appendTo(".answers");
                        item.find(".interactive").text(i.IsInteractive);
                        item.find(".random").text(i.IsRandom);
                        item.find(".title").text(i.Title);
                        item.find(".data").html(i.Data);
                        item.find(".uuid").text(i.Uuid);
                        item.removeClass("template");
                        item.show();
                    });
                    MathJax.Hub.Queue(["Typeset", MathJax.Hub, "TFS"]);
                    renderMathInElement(document.body);
                    $(".overlay").fadeOut(1000);
                });
            break;
        case "View3":
            sendRequest(`/API/RemoveFinishedItem?uuid=${params.get("uuid")}`,
                function(json) {
                    json = JSON.parse(json);
                    json.SolverOutput.forEach(function (i) {
                        if (i.includes("<div class=\"scores\">")) return;
                        let item = $(".template").clone();
                        item.removeClass("template");
                        item.appendTo(".answers");
                        item.html(i);
                        item.show();
                    });
                    MathJax.Hub.Queue(["Typeset", MathJax.Hub, "TFS"]);
                    renderMathInElement(document.body);
                    $(".overlay").fadeOut(1000);
                });
            break;
    }
});