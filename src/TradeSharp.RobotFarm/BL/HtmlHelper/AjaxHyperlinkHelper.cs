using System;
using System.Text;

namespace TradeSharp.RobotFarm.BL.HtmlHelper
{
    class HyperlinkHelper : HtmlHelper
    {
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
            markupSB.Append("<a ");
            MakeExtendedHtmlMarkup();
            markupSB.Append(" >");

            foreach (var childHtmlHelper in childElements)
                markupSB.Append(childHtmlHelper.GetHtmlMarkup());


            markupSB.Append(" </a>");
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
                    case HtmlAttribute.OnClick:
                        markupSB.Append(" onclick=\"" + attribute.Value + "\" ");
                        break;
                    case HtmlAttribute.Href:
                        markupSB.Append(" href=\"" + attribute.Value + "\" ");
                        break;
                    case HtmlAttribute.Class:
                        markupSB.Append(" class=\"" + attribute.Value + "\" ");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
