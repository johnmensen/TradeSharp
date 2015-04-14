namespace TradeSharp.RobotFarm.BL.HtmlHelper
{
    /// <summary>
    /// Возможные типы Html тега Input для Html хелпера InputHelper
    /// </summary>
    public enum HtmlInputType
    {
        Text, Submit, Password, Hidden, Checkbox, Button, Image
    }

    /// <summary>
    /// Возможные типы атрибутов для Html хелпера. Это перечисление содержит атрибуты РАЗНЫХ html тегов (и для input и для ссылки). В связи
    /// с этим в методе MakeExtendedHtmlMarkup, какого либо HtmlHelper-а, нужно строго следить, что бы в операторе switch/case были описаны только 
    /// HtmlAttribute, тоторые реально принадлежат этому тегу в Html.
    /// </summary>
    public enum HtmlAttribute
    {
        Id, Name, Style, Class, Value, OnClick, Href, Alt, Src, Checked
    }
}
