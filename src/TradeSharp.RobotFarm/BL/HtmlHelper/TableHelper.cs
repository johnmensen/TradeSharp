using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TradeSharp.RobotFarm.BL.HtmlHelper
{

    class TableHelper :  HtmlHelper
    {


        protected override void MakeHtmlMarkup()
        {
            markupSB.Append("<table ");
            MakeExtendedHtmlMarkup();
            markupSB.Append(" >");

            foreach (var childHtmlHelper in childElements)
                markupSB.Append(childHtmlHelper.GetHtmlMarkup());

            markupSB.Append(" </table>");
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
                    case HtmlAttribute.Class:
                        markupSB.Append(" class=\"" + attribute.Value + "\" ");
                        break;
                    case HtmlAttribute.Name:
                        markupSB.Append(" name=\"" + attribute.Value + "\" ");
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }
    }
}
