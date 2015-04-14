jQuery(document).ready(function ($) {
    makeValidationMarks();
});

function makeValidationMarks() {
    $(":root").find('input[data-validate]').parent().append('<div style="color:Red; display:none" data-validate-result="true"></div>');
}

function releaseValidateResult(divId) {
    $('#' + divId).find('[data-validate-result]').each(function () {
        $(this).hide();
    });;
    
    $('#' + divId).hide();
}

function validateInputs(divId) {
    var isOk = true;
    // получить все поля ввода внутри элемента
    var inputs = $('#' + divId).find('input');
    for (var i = 0; i < inputs.length; i++) {
        // отобрать те, где есть атрибут data-validate
        var validateAttr = $(inputs[i]).attr('data-validate');
        if (!validateAttr) continue;

        var inpVal = $(inputs[i]).val();
        var inputErrorStr = validateInput(inpVal, validateAttr);
        var $resultNode = $(inputs[i]).parent().find('div[data-validate-result]');

        if (inputErrorStr.length > 0)
            isOk = false;

        if ($resultNode) {
            if (inputErrorStr.length > 0) {
                // показать комментарий - ошибка
                $resultNode.text(inputErrorStr);
                $resultNode.show();
            } else {
                // спрятать сообщение об ошибке
                $resultNode.text('');
                $resultNode.hide();
            }
        }
    }
    return isOk;
}

function validateInput(inpVal, validateArgs) {
    validateArgs = validateArgs.replace(/'/g, '"');
    var valArgsJson = JSON.parse(validateArgs);
    if (!valArgsJson) return '';
    var dataType = valArgsJson.fieldType;
    if (!dataType) return '';

    var i;
    
    if (dataType == 'float') {
        if (!inpVal.match(/^[+-]?\d+(\.\d+)?$/))
            return errorMessageIncorrectRecordNumber + '. ' + titleExampleOfRecords + ': -0.05, 990.31';
        var fVal = parseFloat(inpVal);
        if (valArgsJson.conditions)
            for (i = 0; i < valArgsJson.conditions.length; i++) {
                if (valArgsJson.conditions[i].positive)
                    if (fVal <= 0)
                        return errorMessageMustPositive;
                if (valArgsJson.conditions[i].negative)
                    if (fVal >= 0)
                        return errorMessageMustNegative;
                if (valArgsJson.conditions[i].notNegative)
                    if (fVal < 0)
                        return errorMessageMustNonNegative;
                if (valArgsJson.conditions[i].notPositive)
                    if (fVal > 0)
                        return errorMessageMustNonPositive;
            }
    }
    if (dataType == 'int') {
        var iVal = parseInt(inpVal);
        if (isNaN(iVal))
            return errorMessageIncorrectRecordNumber + '. ' + titleExampleOfRecords + ': 0, 150, -2';
        if (valArgsJson.conditions)
            for (i = 0; i < valArgsJson.conditions.length; i++) {
                if (valArgsJson.conditions[i].positive)
                    if (iVal <= 0)
                        return errorMessageMustPositive;
                if (valArgsJson.conditions[i].negative)
                    if (iVal >= 0)
                        return errorMessageMustNegative;
                if (valArgsJson.conditions[i].notNegative)
                    if (iVal < 0)
                        return errorMessageMustNonNegative;
                if (valArgsJson.conditions[i].notPositive)
                    if (iVal > 0)
                        return errorMessageMustNonPositive;
            }
    }
    if (dataType == 'string') {
        if (valArgsJson.conditions)
            for (i = 0; i < valArgsJson.conditions.length; i++) {
                if (valArgsJson.conditions[i].min_length)
                    if (valArgsJson.conditions[i].min_length > inpVal.length)
                        return errorMessageMinNumberCharacters + ' ' + valArgsJson.conditions[i].min_length;
                if (valArgsJson.conditions[i].max_length)
                    if (valArgsJson.conditions[i].max_length < inpVal.length)
                        return errorMessageMaxNumberCharacters + ' ' + valArgsJson.conditions[i].max_length;
            }
    }

    return '';
}