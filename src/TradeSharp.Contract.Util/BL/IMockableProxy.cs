using System;
using System.Collections.Generic;

namespace TradeSharp.Contract.Util.BL
{
    public interface IMockableProxy
    {
        Dictionary<string, Delegate> MockMethods { get; set; }

        /// <summary>
        /// Добавляет / заменяе терализацию указанног метода
        /// </summary>
        /// <param name="methodName">имя метода, реализацию которого нужно добавить / заменить</param>
        /// <param name="methodImplementation">новая реализация метода</param>
        void IncludeMockMethod(string methodName, Delegate methodImplementation);
    }
}
