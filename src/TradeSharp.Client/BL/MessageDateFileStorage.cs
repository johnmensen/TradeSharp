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
    class MessageDateFileStorage
    {
        private readonly string filePath;

        private DateTime end;

        public DateTime Date
        {
            get { return end; }
            set
            {
                end = value;
                try
                {
                    using (var sw = new StreamWriter(filePath, false, Encoding.ASCII))
                    {
                        sw.WriteLine(end.ToStringUniformMils());
                    }
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat("MessageDateFileStorage - ошибка записи в файл \"{0}\": {1}", filePath, ex);
                    throw;
                }
            }
        }

        public MessageDateFileStorage(string path)
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
                    Logger.ErrorFormat("MessageDateFileStorage - ошибка создания директории \"{0}\"", dirName);
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
                    end = line.ToDateTimeUniformSafeMils() ?? DateTime.Now.Date.AddDays(-30);
                }
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat("MessageDateFileStorage - ошибка чтения файла \"{0}\": {1}", path, ex);
            }
        }        
    }
}
