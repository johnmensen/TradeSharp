/* Модальный диалог с заданным текстом */
function showModalDialog(title, text) {
    var dlg = $('#dialog-modal');
    dlg.attr('title', title);
    dlg.show();
    dlg.html(text);
    dlg.dialog({
        height: 140,
        modal: true
    });
}

/* Показать DIV, привязать его к центру экрана */
function letDivFloat(divId) {
    var divEdit = $('#' + divId);
    divEdit.css("position", "fixed");
    divEdit.css("left", "30%");
    divEdit.css("top", "30%");
    divEdit.css("margin-top:", "-" + Math.round(divEdit.height() / 2) + "px");
    divEdit.css("margin-left:", "-" + Math.round(divEdit.width() / 2) + "px");
    divEdit.css("display", "block");
}

/* Показать DIV, привязать его к переданному элементу */
function letDivBeSticked(divId, objectToStick) {
    var divEdit = $('#' + divId);
    divEdit.css("position", "absolute");
    divEdit.css("margin-top:", "0px");
    divEdit.css("margin-left:", "0px");
    divEdit.css(objectToStick.offset());
    divEdit.css("display", "block");
}

/* Форматировать время в виде, понятном для сервера терминала (dd.MM.yyyy HH:mm:ss) */
function formatDateTime(date) {
    var parts = [date.getDate(), date.getMonth() + 1, date.getFullYear(), date.getHours(),
        date.getMinutes(), date.getSeconds()];
    var separators = ['.', '.', ' ', ':', ':'];

    var str = '';
    $.each($(parts), function (index) {
        var strPart = this.toString();
        if (strPart.length < 2)
            strPart = '0' + strPart;
        str = str + strPart;
        if (index < separators.length)
            str = str + separators[index];
    });

    return str;
}