namespace ForexSiteDailyQuoteParser.CommonClass
{
    /// <summary>
    /// Способы объединения списков котировок
    /// </summary>
    public enum MergeMode
    {
        /// <summary>
        /// пересекающиеся данные нового источника затирают записи в старом файле
        /// </summary>
        ResourseMain, 

        /// <summary>
        /// пересекающиеся записи в старом файле затирают данные нового источника
        /// </summary>
        OutputMain
    }
}
