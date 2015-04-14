namespace TradeSharp.Client.Controls.QuoteTradeControls
{
    public enum DragDropState { Normal = 0, Washy = 1, InFrame = 2 }

    /// <summary>
    /// Ключи для словаря цветов в выпадающем списке выбора объёмов
    /// </summary>
    public enum DropDownListColor
    {
        ForeGroundBrushNormal, ForeGroundBrushCovered, TriangleBrushNormal, TriangleBrushCovered, TrianglePen, TextBrushNormal, TextBrushCovered
    }

    public enum ButtonFigureColorScheme { Normal = 1, Light = 2, Pressed = 3 }
    public enum DropDownListFigures { Triangle = 1, Container = 2, Item = 3, LastItem = 4 }
    public enum ArrowTrand { Empty = 0, Up = 1, Down = 2 }
    public enum ObjectsToPaint { All = 0, RightB, LeftB }
}
