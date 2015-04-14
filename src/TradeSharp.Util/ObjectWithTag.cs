namespace TradeSharp.Util
{
    /// <summary>
    /// Для представления элементов в списках типа ComboBox
    /// </summary>
    public class ObjectWithTag<T>
    {
        public T value;
        public string title;

        public ObjectWithTag() {}
        public ObjectWithTag(string _title, T _value)
        {
            value = _value;
            title = _title;
        }
        public override string ToString()
        {
            return title;
        }
    }
}
