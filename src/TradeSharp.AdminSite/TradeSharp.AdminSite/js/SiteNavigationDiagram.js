var realArcs = new Array();

var currentElementId = "";

// названия "left", "top", "width" и "height" не менять они используются в "checkLineIntersectionSpan"
var pageWidth = 100; var pageHeight = 50;
var pageCx = 330; var pageCy = 400;
var mapPages =
    [
        { left: pageCx, top: pageCy, width: pageWidth, height: pageHeight, title: "Счета", color: "#f0e8e0", id: "Accounts" },
        { left: pageCx + 30, top: pageCy - 180, width: 1.5 * pageWidth, height: pageHeight, title: "Новый счёт", color: "#f0e8e0", id: "NewAccount" },
        { left: pageCx - 2 * pageWidth - 90, top: pageCy - 150, width: 2 * pageWidth, height: pageHeight, title: "Позиции (сделки)", color: "#f0e8e0", id: "Position" },
        { left: pageCx - 2 * pageWidth - 100, top: pageCy - 350, width: 2.5 * pageWidth, height: pageHeight, title: "Редактирование сделок", color: "#f0e8e0", id: "PositionEdit" },
        { left: pageCx, top: pageCy - 350, width: 2 * pageWidth, height: pageHeight, title: "Детали сделки", color: "#f0e8e0", id: "PositionDetails" },
        { left: pageCx + pageWidth + 90, top: pageCy + 10, width: 2 * pageWidth, height: pageHeight, title: "Торговые сигналы", color: "#f0e8e0", id: "TradeSignal" },
        { left: pageCx + pageWidth + 40, top: pageCy + pageHeight + 55, width: 2.5 * pageWidth, height: pageHeight, title: "Учётные данные клиента", color: "#f0e8e0", id: "OwnerDetails" },
        { left: pageCx - 300, top: pageCy + pageHeight - 30, width: 2 * pageWidth, height: pageHeight, title: "Детали счёта", color: "#f0e8e0", id: "AccountDetails" },
        { left: pageCx - 185, top: pageCy + 2 * pageHeight + 60, width: 2.5 * pageWidth, height: pageHeight, title: "Владельци счёта и их права", color: "#f0e8e0", id: "AccountOwner" }
        
    ];

var mapArcs =
    [
        { nodeFrom: "Accounts", nodeTo: "OwnerDetails", color: "maroon", pointFrom: null, pointTo: null },
        { nodeFrom: "Accounts", nodeTo: "AccountDetails", color: "maroon", pointFrom: null, pointTo: null },
        { nodeFrom: "Accounts", nodeTo: "TradeSignal", color: "maroon", pointFrom: null, pointTo: null },
        { nodeFrom: "Accounts", nodeTo: "Position", color: "maroon", pointFrom: null, pointTo: null },
        { nodeFrom: "Accounts", nodeTo: "NewAccount", color: "maroon", pointFrom: null, pointTo: null },
        { nodeFrom: "AccountDetails", nodeTo: "AccountOwner", color: "maroon", pointFrom: null, pointTo: null },
        { nodeFrom: "AccountDetails", nodeTo: "Position", color: "maroon", pointFrom: null, pointTo: null },
        { nodeFrom: "OwnerDetails", nodeTo: "AccountOwner", color: "maroon", pointFrom: null, pointTo: null },       
        { nodeFrom: "Position", nodeTo: "PositionEdit", color: "maroon", pointFrom: null, pointTo: null },
        { nodeFrom: "Position", nodeTo: "PositionDetails", color: "maroon", pointFrom: null, pointTo: null }
    ];


function drawSitemap() {
    var canvas = $('div#sitemapCanvas');
    var height = $(document).height() - 80;
    var canvasInnerHtml = '<svg xmlns="http://www.w3.org/2000/svg" version="1.1" height="' + height + '">\n';

    // узлы
    for (var k = 0; k < mapPages.length; k++) {
        var pageNode = mapPages[k];
        // контур  ' + pageName
        //
        canvasInnerHtml = canvasInnerHtml + '<rect x="' + pageNode.left + '" y="' + pageNode.top + '" rx="10" ry="10" width="' + pageNode.width + '" height="' + pageNode.height + '"';
        canvasInnerHtml = canvasInnerHtml + ' onclick="tooltipPageAppear(&quot;' + pageNode.id + '&quot;)" ';
        canvasInnerHtml = canvasInnerHtml + 'style="fill:' + pageNode.color + ';stroke:black;stroke-width:2; cursor:pointer"/>';

        // текст
        var cx = pageNode.left + 10;
        var cy = pageNode.top + 20;

        canvasInnerHtml = canvasInnerHtml + '\n<a  xlink:href="/Help/ElementDescriptionPageHTML?pageName=' + pageNode.id + '" target="_blank">';
        canvasInnerHtml = canvasInnerHtml + '\n<text x="' + cx + '" y="' + cy + '" ';
        canvasInnerHtml = canvasInnerHtml + 'font-family="Verdana" font-size="15" fill="blue" text-decoration="underline";>';
        canvasInnerHtml = canvasInnerHtml + pageNode.title + '</text>';
        canvasInnerHtml = canvasInnerHtml + '</a>';


    }
  
    var mapSideNames = ['left', 'right', 'top', 'bottom'];  
    //Заполняем массив длин возможных дуг между двумя элементами
    for (var i = 0; i < mapArcs.length; i++) {
        var virtualArcs = new Array(); 
        var arc = mapArcs[i];

        for (var j = 0; j < mapSideNames.length; j++) {
            var pFrom = makeArcPoint(arc.nodeFrom, mapSideNames[j]);
            for (var k = 0; k < mapSideNames.length; k++) {
                var pTo = makeArcPoint(arc.nodeTo, mapSideNames[k]);

                virtualArcs.push({
                    pointFrom: pFrom,
                    pointTo: pTo,
                    length: (pTo.x - pFrom.x) * (pTo.x - pFrom.x) + (pTo.y - pFrom.y) * (pTo.y - pFrom.y),
                    countIntersect: 0
                });
            }
        }
        
        
        //Сортируем 16 дуг
        virtualArcs.sort(compareMapArcsLength); 
        var realArcPoints = new Array();
        //Заполняем массив пересечений для каждой из 16 дуг
        for (var j = 0; j < virtualArcs.length; j++) {
 
            for (var k = 0; k < mapPages.length; k++) {
                virtualArcs[j].countIntersect += checkLineIntersectionSpan(virtualArcs[j].pointFrom.x, virtualArcs[j].pointFrom.y,
                                                            virtualArcs[j].pointTo.x, virtualArcs[j].pointTo.y, mapPages[k]);
            }
            if (virtualArcs[j].countIntersect == 0) {
                realArcPoints.push(virtualArcs[j].pointFrom);
                realArcPoints.push(virtualArcs[j].pointTo);
                break;
            }
        }
    
        if (realArcPoints.length < 2)
        {
            virtualArcs.sort(compareMapArcsCountIntersect);
            realArcPoints.push(virtualArcs[0].pointFrom);
            realArcPoints.push(virtualArcs[0].pointTo);
        }
        if (realArcPoints.length < 2) continue;

        

        arc.pointFrom = realArcPoints[0];
        arc.pointTo = realArcPoints[1];

        canvasInnerHtml = canvasInnerHtml + '\n<g stroke="' + arc.color + '">';
        canvasInnerHtml = canvasInnerHtml + '<line stroke-width="3" ';
        canvasInnerHtml = canvasInnerHtml + 'x1="' + realArcPoints[0].x + '" y1="' + realArcPoints[0].y + '" ';
        canvasInnerHtml = canvasInnerHtml + 'x2="' + realArcPoints[1].x + '" y2="' + realArcPoints[1].y + '" ';
        canvasInnerHtml = canvasInnerHtml + '</line>';

        // Треугольник
        var l = Math.sqrt((arc.pointTo.x - arc.pointFrom.x) * (arc.pointTo.x - arc.pointFrom.x) + (arc.pointTo.y - arc.pointFrom.y) * (arc.pointTo.y - arc.pointFrom.y));
        var cx1 = arc.pointFrom.x + (65 * (arc.pointTo.x - arc.pointFrom.x) / l);
        var cy1 = arc.pointFrom.y + (65 * (arc.pointTo.y - arc.pointFrom.y) / l);

        var cx2 = arc.pointFrom.x + (30 * (arc.pointTo.x - arc.pointFrom.x) / l);
        var cy2 = arc.pointFrom.y + (30 * (arc.pointTo.y - arc.pointFrom.y) / l);


        //canvasInnerHtml = canvasInnerHtml + '<path d="M' + realArcPoints[0].x + ' ' + realArcPoints[0].y + ' ';
        //canvasInnerHtml = canvasInnerHtml + 'C' + (realArcPoints[0].x + 40) + ', ' + (realArcPoints[0].y) + ' ';
        //canvasInnerHtml = canvasInnerHtml + (realArcPoints[1].x - 40) + ', ' + (realArcPoints[1].y) + ' ';
        //canvasInnerHtml = canvasInnerHtml + realArcPoints[1].x + ',' + realArcPoints[1].y + ' ';
        //canvasInnerHtml = canvasInnerHtml + 'fill="none" stroke-width="5"  />';
       
        
        canvasInnerHtml = canvasInnerHtml + ' </g>';
   
        canvasInnerHtml = canvasInnerHtml + '\n<polygon points="' + makeTrianglePathPoints({ x: cx1, y: cy1 }, { x: cx2, y: cy2 }) + '" ';
        canvasInnerHtml = canvasInnerHtml + '" onclick="tooltipLinckAppear(&quot;' + arc.nodeFrom + '&quot;, &quot;' + arc.nodeTo + '&quot;)" ';
        canvasInnerHtml = canvasInnerHtml + 'style="fill:red; stroke:black;stroke-width:1;cursor:pointer" />';
    }

    canvasInnerHtml = canvasInnerHtml + '\n</svg>';
    // заполнить DIV
    canvas.html(canvasInnerHtml);
}

function getCenterLine(point1, point2)
{
    var cx = point2.x - point1.x;
    if (cx < 0) cx = point2.x - cx / 2;
    else cx = point1.x + cx / 2;

    var cy = point2.y - point1.y;
    if (cy < 0) cy = point2.y - cy / 2;
    else cy = point1.y + cy / 2;

    return { x: cx, y: cy };
}

function makeTrianglePathPoints(point1, point2) {

    var x1 = point1.x + (point2.x - point1.x) * 0.966 - (point2.y - point1.y) * 0.258;
    var y1 = point1.y + (point2.y - point1.y) * 0.966 + (point2.x - point1.x) * 0.258;

    var x2 = point1.x + (point2.x - point1.x) * 0.966 - (point2.y - point1.y) * -0.258;
    var y2 = point1.y + (point2.y - point1.y) * 0.966 + (point2.x - point1.x) * -0.258;

    return point1.x + ', ' + point1.y + ' ' + x1 + ', ' + y1 + ' ' + x2 + ', ' + y2;
}


function makeArcPoint(mapPageId, side) {
    var page = getMapPageById(mapPageId);

    if (side == 'left')
        return { x: page.left, y: page.top + page.height / 2 };
    if (side == 'right')
        return { x: page.left + page.width, y: page.top + page.height / 2 };
    if (side == 'top')
        return { x: page.left + page.width / 2, y: page.top };
    if (side == 'bottom')
        return { x: page.left + page.width / 2, y: page.top + page.height };
}

function getMapPageById(mapPageId) {
    var result = mapPages[0];
    for (var i = 0; i < mapPages.length; i++) {
        if (mapPages[i].id == mapPageId) {
            result = mapPages[i];
            break;
        }
    }
    return result;
}

function checkLineIntersectionSpan(lineStartX, lineStartY, lineEndX, lineEndY, mapPage) {
    var countIntersection = 0;
    var checkResult = checkLinesIntersection(lineStartX, lineStartY, lineEndX, lineEndY, mapPage.left, mapPage.top, mapPage.left + mapPage.width, mapPage.top);
    if (checkResult.onLine1 == true && checkResult.onLine2 == true)
        countIntersection++;

    checkResult = checkLinesIntersection(lineStartX, lineStartY, lineEndX, lineEndY, mapPage.left + mapPage.width, mapPage.top, mapPage.left + mapPage.width, mapPage.top + mapPage.height);
    if (checkResult.onLine1 == true && checkResult.onLine2 == true)
        countIntersection++;

    checkResult = checkLinesIntersection(lineStartX, lineStartY, lineEndX, lineEndY, mapPage.left, mapPage.top + mapPage.height, mapPage.left + mapPage.width, mapPage.top + mapPage.height);
    if (checkResult.onLine1 == true && checkResult.onLine2 == true)
        countIntersection++;

    checkResult = checkLinesIntersection(lineStartX, lineStartY, lineEndX, lineEndY, mapPage.left, mapPage.top, mapPage.left, mapPage.top + mapPage.height);
    if (checkResult.onLine1 == true && checkResult.onLine2 == true)
        countIntersection++;

    return countIntersection;
}

function checkLinesIntersection(line1StartX, line1StartY, line1EndX, line1EndY, line2StartX, line2StartY, line2EndX, line2EndY) {
    var denominator, a, b, numerator1, numerator2, result = {
        x: null,
        y: null,
        onLine1: false,
        onLine2: false
    };
    denominator = ((line2EndY - line2StartY) * (line1EndX - line1StartX)) - ((line2EndX - line2StartX) * (line1EndY - line1StartY));
    if (denominator == 0) {
        return result;
    }
    a = line1StartY - line2StartY;
    b = line1StartX - line2StartX;
    numerator1 = ((line2EndX - line2StartX) * a) - ((line2EndY - line2StartY) * b);
    numerator2 = ((line1EndX - line1StartX) * a) - ((line1EndY - line1StartY) * b);
    a = numerator1 / denominator;
    b = numerator2 / denominator;

    result.x = line1StartX + (a * (line1EndX - line1StartX));
    result.y = line1StartY + (a * (line1EndY - line1StartY));

    if (a > 0 && a < 1) {
        result.onLine1 = true;
    }
    if (b > 0 && b < 1) {
        result.onLine2 = true;
    }
    return result;
};

//Для сортировок массива объектов
function compareMapArcsLength(a, b) {
    if (a.length < b.length)
        return -1;
    if (a.length > b.length)
        return 1;
    return 0;
}
function compareMapArcsCountIntersect(a, b) {
    if (a.countIntersect < b.countIntersect)
        return -1;
    if (a.countIntersect > b.countIntersect)
        return 1;
    return 0;
}

function tooltipLinckAppear(from, to) {
    toolTipFill(from + '-' + to);
}

function tooltipPageAppear(id) {
    toolTipFill(id);
}

function toolTipFill(imgName) {
    if (currentElementId == imgName) return;
    currentElementId = imgName;
    
    var imgAvatar = $('#imgAvatar');
    imgAvatar.attr('src', '../../images/HelpSection/Avatars/' + imgName + '.png');
    
    var tooltipdiv = $('#tooltipText');
    tooltipdiv.empty();
    $.ajax({
        url: '/Help/ElementDescription?imgName=' + imgName,
        type: 'GET',
        success: function (result) {     
            tooltipdiv.html(result);
        },
        error: function () {
            tooltipdiv.html('Error');
        }
    });
}