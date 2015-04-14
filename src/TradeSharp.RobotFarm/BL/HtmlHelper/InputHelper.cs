using System;
using System.Text;

namespace TradeSharp.RobotFarm.BL.HtmlHelper
{
    class InputHelper : HtmlHelper
    {
        private readonly HtmlInputType inputType;

        public InputHelper(HtmlInputType htmlInputType)
        {
            inputType = htmlInputType;
        }

        /// <summary>
        /// Возвращает разметку в виде строки
        /// </summary>
        public override string GetHtmlMarkup()
        {
            return GetHtmlMarkupSB().ToString();
        }

        /// <summary>
        /// Возвращает разметку в виде объекта StringBuilder
        /// </summary>
        public override StringBuilder GetHtmlMarkupSB()
        {
            MakeHtmlMarkup();
            return markupSB;
        }

        /// <summary>
        /// Вспомогательный метод - генерирует всю разметку
        /// </summary>
        protected override void MakeHtmlMarkup()
        {
            MakeBaseHtmlMarkup();
            MakeExtendedHtmlMarkup();
            markupSB.Append(" />");
        }

        /// <summary>
        /// В зависимости от переданного в конструкторе типа Helper-а генерирует основу разметки
        /// </summary>
        private void MakeBaseHtmlMarkup()
        {
            switch (inputType)
            {
                case HtmlInputType.Text:
                    markupSB.Append("<input type=\"text\" ");
                    break;
                case HtmlInputType.Submit:
                    markupSB.Append(" <input type=\"submit\" ");
                    break;
                case HtmlInputType.Password:
                    markupSB.Append("<input type=\"password\" ");
                    break;
                case HtmlInputType.Hidden:
                    markupSB.Append("<input type=\"hidden\" ");
                    break;
                case HtmlInputType.Checkbox:
                    markupSB.Append("<input type=\"checkbox\" ");
                    break;
                case HtmlInputType.Button:
                    markupSB.Append("<input type=\"button\" ");
                    break;
                case HtmlInputType.Image:
                    markupSB.Append("<input type=\"image\" ");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// В зависимости от additionalAttribute генерирует дополнительные атрибуты разметки
        /// </summary>
        private void MakeExtendedHtmlMarkup()
        {
            if (additionalAttribute.Count == 0) return;
            foreach (var attribute in additionalAttribute)
            {
                switch (attribute.Key)
                {
                    case HtmlAttribute.Id:
                        markupSB.Append(" id=\"" + attribute.Value + "\" ");
                        break;
                    case HtmlAttribute.Style:
                        markupSB.Append(" style=\"" + attribute.Value + "\" ");
                        break;
                    case HtmlAttribute.Name:
                        markupSB.Append(" name=\"" + attribute.Value + "\" ");
                        break;
                    case HtmlAttribute.Value:
                        markupSB.Append(" value=\"" + attribute.Value + "\" ");
                        break;
                    case HtmlAttribute.OnClick:
                        markupSB.Append(" onclick=\"" + attribute.Value + "\" ");
                        break;
                    case HtmlAttribute.Checked:
                        if (inputType == HtmlInputType.Checkbox)
                            markupSB.Append(" checked ");
                        break;
                    case HtmlAttribute.Src:
                        if (inputType == HtmlInputType.Image)
                            markupSB.Append(" src=\"" + attribute.Value + "\" ");
                        break;
                    case HtmlAttribute.Class:
                            markupSB.Append(" class=\"" + attribute.Value + "\" ");
                        break;
                    case HtmlAttribute.Alt:
                        if (inputType == HtmlInputType.Image)
                            markupSB.Append(" alt=\"" + attribute.Value + "\" ");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
