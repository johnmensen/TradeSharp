using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace TradeSharp.RobotFarm.BL.HtmlHelper
{
    /// <summary>
    /// базовый класс HtmlHelper
    /// </summary>
    public abstract class HtmlHelper
    {
        /// <summary>
        /// список дочерних елементов
        /// </summary>
        public List<HtmlHelper> childElements = new List<HtmlHelper>();

        /// <summary>
        /// Основные атрибуты в размеке HtmlHelper-а кроме ide
        /// </summary>
        public Dictionary<HtmlAttribute, string> additionalAttribute = new Dictionary<HtmlAttribute, string>();

        /// <summary>
        /// Объект "накапливающий" разметку Html helper-а в процессе его формирования
        /// </summary>
        protected readonly StringBuilder markupSB = new StringBuilder();

        abstract protected void MakeHtmlMarkup();

        /// <summary>
        /// Возвращает разметку в виде строки
        /// </summary>
        public virtual string GetHtmlMarkup()
        {
            return GetHtmlMarkupSB().ToString();
        }

        /// <summary>
        /// Возвращает разметку в виде объекта StringBuilder
        /// </summary>
        public virtual StringBuilder GetHtmlMarkupSB()
        {
            MakeHtmlMarkup();
            return markupSB;
        }



        /// <summary>
        /// перепривязывает ресурсный список с новыми данными и, если нужно,  возвращает разметку в виде строки
        /// </summary>
        public virtual string GetHtmlMarkup(IList source)
        {
            return GetHtmlMarkupSB(source).ToString();
        }

        /// <summary>
        ///  перепривязывает ресурсный список с новыми данными и, если нужно, возвращает разметку в виде объекта StringBuilder
        /// </summary>
        public virtual StringBuilder GetHtmlMarkupSB(IList source)
        {
            MakeHtmlMarkup(source);
            return markupSB;
        }

        /// <summary>
        /// Этот метод переопределён в наследнике FastGridTableHelper. В остальных наследниках этот метод не переопределён и даже если при использовании скажем InputHelper
        /// кто то передаст что лбо в параметре "source", всё равно отработает ЭТА реализация - вызовется "MakeHtmlMarkup()" и "source" просто проигнорируется.
        /// В случа если же в наследнике (например, как в FastGridTableHelper) этот метод переопределён,  в "source" уже может быть использован (в новой реализации).
        /// </summary>
        virtual protected void MakeHtmlMarkup(IList source)
        {
            MakeHtmlMarkup();
        }
    }
}
