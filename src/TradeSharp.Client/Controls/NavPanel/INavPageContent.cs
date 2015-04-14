using System;

namespace TradeSharp.Client.Controls.NavPanel
{
    interface INavPageContent
    {
        event Action<int> ContentHeightChanged;
    }
}
