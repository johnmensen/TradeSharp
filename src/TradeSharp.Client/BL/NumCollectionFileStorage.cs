using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TradeSharp.Util;

namespace TradeSharp.Client.BL
{
    /// <summary>
    /// хранит диапазон коллекции чисел (начало и конец диапазона)
    /// сравнивает, есть ли в переданном параметре новые значения (вне диапазона)
    /// удаляет старые значения из переданного списка
    /// если в переданном списке есть новые значения, сохраняет обновленный диапазон в файл
    /// </summary>
    class NumCollectionFileStorage
    {
        private readonly string filePath;

        private int start, end;

        public NumCollectionFileStorage(string path)
        {
            filePath = path;

            // попытаться прочитать диапазон
            var dirName = Path.GetDirectoryName(path);
            if (!Directory.Exists(dirName))
            {
                try
                {
                    Directory.CreateDirectory(dirName);
                }
                catch (Exception)
                {
                    Logger.ErrorFormat("NumCollectionFileStorage - ошибка создания директории \"{0}\"", dirName);
                    throw;
                }
            }

            if (!File.Exists(path)) return;
            try
            {
                using (var sr = new StreamReader(path, Encoding.ASCII))
                {
                    var line = sr.ReadLine();
                    if (string.IsNullOrEmpty(line)) return;
                    var numbers = line.ToIntArrayUniform();
                    if (numbers.Length == 2)
                    {
                        start = numbers[0];
                        end = numbers[1];
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("NumCollectionFileStorage - ошибка чтения файла \"{0}\": {1}", path, ex);
            }
        }

        /// <summary>
        /// проверить попадания в диапазон
        /// отфильтровать попавшие на диапазон значения
        /// </summary>
        public void CheckNumbers(ref List<int> numbers)
        {
            // обновить диапазон и сохранить новый диапазон в файл (маленький получится файлик - ну и хрен то с ним)
            var maxNum = numbers.Count == 0 ? 0 : numbers.Max();
            // отфильтровать старые
            numbers = numbers.Where(n => n > end).ToList();
            
            if (end == maxNum) return;
            end = maxNum;
            try
            {
                using (var sw = new StreamWriter(filePath, false, Encoding.ASCII))
                {
                    sw.WriteLine("{0} {1}", start, end);
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("NumCollectionFileStorage - ошибка записи в файл \"{0}\": {1}", filePath, ex);
                throw;
            }
        }
    }
}
