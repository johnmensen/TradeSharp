using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TradeSharp.Client.Controls.QuoteTradeControls.DropDownListControls
{
    /// <summary>
    /// Класс содержит всплывающее окно для элемента QuoteTradeDropDownList и логику заполнения его элементами
    /// </summary>
    sealed class QuoteTradeDropDownListContainer : ToolStripDropDown
    {
        /// <summary>
        /// Выбор элемента в этом контейнере. В качестве параметра передаётся выбранный элемент
        /// </summary>
        public event Action<QuoteTradeListItem> OnItemSelect;

        /// <summary>
        /// Индекс элемента, по которому сейчас был произведён клик
        /// </summary>
        private int currentSelectedIndex;

        public QuoteTradeDropDownListContainer(IEnumerable<QuoteTradeListItem> items)
        {
            AutoSize = false;            
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);
            BackColor= Color.Transparent;

            foreach (var quoteTradeListItem in items)
            {
                quoteTradeListItem.OnSelect += QuoteTradeListItemOnSelect;
                Items.Add(quoteTradeListItem);
            }               
        }

        /// <summary>
        /// Метод, подписанный на событие клика по элементу списка - объекта типа "QuoteTradeListItem"
        /// </summary>
        /// <param name="selectedIndex">Индекс элемента, по которому был произведён клик</param>
        void QuoteTradeListItemOnSelect(int selectedIndex)
        {
            currentSelectedIndex = selectedIndex;
        }

        /// <summary>
        /// Обработчик клика по самосу элементу. Вызывается сразу после "QuoteTradeListItemOnSelect"
        /// </summary>
        /// <param name="mea"></param>
        protected override void OnMouseDown(MouseEventArgs mea)
        {
            base.OnMouseDown(mea);

            if (OnItemSelect != null) 
                OnItemSelect(Items[currentSelectedIndex] as QuoteTradeListItem);
        }

        /// <summary>
        /// Обновление размеров элементов в выпадающем списке при изменении размеров всего контрола "QuoteTradeDropDownList"
        /// </summary>
        /// <param name="width">Новая ширина QuoteTradeDropDownList</param>
        /// <param name="height">Новая высота QuoteTradeDropDownList</param>
        /// <param name="textSize">Новый размер шрифта</param>
        public void ItemUpdateSize(int width, int height, int textSize)
        {
            foreach (QuoteTradeListItem tradingVolumeItem in Items)
            {
                tradingVolumeItem.Width = width;
                tradingVolumeItem.Height = height;

                tradingVolumeItem.TextSize = textSize;
            }
        }
    }
}