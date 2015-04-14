using System;

namespace TradeSharp.RobotFarm.BL.HtmlHelper
{
    class FormHelper : HtmlHelper
    {
        private readonly string formMethod = "POST";


        public FormHelper()
        {
        }

        public FormHelper(string formMethod)
        {
            switch (formMethod.ToUpper())
            {
                case "POST":
                    this.formMethod = "POST";
                    break;
                case "GET":
                    this.formMethod = "GET";
                    break;
            }
        }

        /// <summary>
        /// Вспомогательный метод - генерирует всю разметку формы.
        /// </summary>
        protected override void MakeHtmlMarkup()
        {
            markupSB.Append("<form ");
            MakeExtendedHtmlMarkup();
            markupSB.Append(" method=\"" + formMethod + "\" ");
            markupSB.Append(" >");

            foreach (var childHtmlHelper in childElements)
                markupSB.Append(childHtmlHelper.GetHtmlMarkup());

            markupSB.Append(" </form>");
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
