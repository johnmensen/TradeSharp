function FilterDialogInit(titleAccept, titleClean) {
    var filterDivArray = [["#filterIdDialog", "#FilterIdOpener"],
        ["#filterGroupDialog", "#FilterGroupOpener"],
        ["#filterBalanceDialog", "#FilterBalanceOpener"],
        ["#filterOwnersDialog", "#FilterOwnersOpener"]];

    for (var i = 0; i < filterDivArray.length; i++) {
        $(filterDivArray[i][0]).dialog({
            autoOpen: false,
            modal: true,
            appendTo: "#TableForm",
            buttons: [{
                    text: titleAccept,
                    click: function() {
                        $('#TableForm').submit();
                    }
                },
                {
                    text: titleClean,
                    click: function() {
                        $(this).find("input:text").val("");
                        $(this).find("select option:first").attr("selected", true);
                        $('#TableForm').submit();
                        ;
                    }
                }],
            show: {
                effect: "blind",
                duration: 200
            },
            hide: {
                effect: "blind",
                duration: 200
            }
        });
    }

    $(filterDivArray[0][1]).click(function () {
        $(filterDivArray[0][0]).dialog("open");
    });
    
    $(filterDivArray[1][1]).click(function () {
        $(filterDivArray[1][0]).dialog("open");
    });
    
    $(filterDivArray[2][1]).click(function () {
        $(filterDivArray[2][0]).dialog("open");
    });

    $(filterDivArray[3][1]).click(function () {
        $(filterDivArray[3][0]).dialog("open");
    });
}
