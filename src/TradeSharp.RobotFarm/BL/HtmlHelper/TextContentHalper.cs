using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TradeSharp.RobotFarm.BL.HtmlHelper
{
    /// <summary>
    /// Реализует HtmlHelper, который представляет собой обычный текст. Это нужно например для HyperlinkHelper, который содержит дочерним элементом просто текст
    /// </summary>
    class TextContentHalper : HtmlHelper
    {
        private readonly string textContent = string.Empty;

        public TextContentHalper(string textContent)
        {
            this.textContent = textContent;
        }

        public override string GetHtmlMarkup()
        {
            return textContent;
        }

        protected override void MakeHtmlMarkup()
        {
            markupSB.Append(textContent);
        }
    }
}
