function GetFilename(url) 
{
    if (!url) return url;
    var parts = url.split('/');
    if (!parts.length) return url;
    return parts[parts.length - 1];
}

function GetFilePath(url) {
    if (!url) return url;
    var pos = url.lastIndexOf('/');
    if (pos < 0) return url;
    return url.substring(0, pos + 1);
} 

function showHideContent(divCtx, icon) 
{    
    var dv = document.getElementById(divCtx);    
    if (dv.style.display == 'none')
        dv.style.display = 'block';
    else
        dv.style.display = 'none';

    var fname = GetFilename(icon.src);
    var fpath = GetFilePath(icon.src);
    
    if (fname == 'small_arrow_dn.png.png')
        icon.src = fpath + 'picts\small_arrow_up.png';
    else 
        icon.src = fpath + 'picts\small_arrow_dn.png'
}

////////////////////////////////////////////////////////////////
// работа с оглавлением
////////////////////////////////////////////////////////////////

function collapseTopics() {
    var headers = document.getElementsByTagName("div");
    for (var i = 0; i < headers.length; i++) {
        var header = headers[i];
        if (header.id.indexOf('topic') == 0)
            $(header).hide();
    }
}

// перейти на заголовок вида "<h3>2.11 Бла-бла ..."
function gotoTopic(sectionNum, topicNum) {
    var container = $('#topic' + sectionNum);
    if (container == null) return;
    container.show();

    var preffix = sectionNum + '.' + topicNum;
    $(container).find('h3').each(function () {
        if ($(this).text().indexOf(preffix) != 0) return;
        $('html, body').animate({
            scrollTop: $(this).offset().top
        }, 1500);
    });
}

// сбацать оглавление
function makeContentsTable(containerDivId) {
    var container = document.getElementById(containerDivId);
    if (container == null) return;
    var topics = [];

    var regexTopic = /^\d+\.\d+/;
    $('[id^=topic]').each(function () {
        var headerText = $(this).prev().text();
        topics.push({ header: headerText });

        // все пункты по теме
        $(this).find('h3').each(function () {
            var headerText = $(this).text();

            // текст должен начинаться с "2.8 текст ..."
            var match = regexTopic.exec(headerText);
            if (!match) return;

            // выбрать номера
            var numbers = [];
            var matchParts = match[0].split('.');
            for (var j = 0; j < matchParts.length; j++)
                numbers.push(parseInt(matchParts[j], 10));
            //match[0].split('.').forEach(function (e) { numbers.push(parseInt(e, 10)) });
            topics.push({ header: headerText, section: numbers[0], topic: numbers[1] });
        });
    });

    // сделать из топиков оглавление            
    for (var i = 0; i < topics.length; i++) {
        var topic = topics[i];
        var sibling = document.createElement('li');
        if (!topic.section)
            sibling.innerHTML = topic.header;
        else
            sibling.innerHTML = '<a href="#" onclick="gotoTopic(' + topic.section + ', ' + topic.topic +
                ')">' + topic.header + '</a>';
        container.appendChild(sibling);
    }
}