using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TradeSharp.Contract.Entity;
using TradeSharp.Util;

namespace TradeSharp.SiteBridge.Lib.Contract
{
    /// <summary>
    /// "упаковывает" данные части графика в массив byte[] - используется сервером
    /// превращает массив byte[] в массив PointF[] для отображения на картинке указанного размера - используется клиентом
    /// </summary>
    public static class MiniChartPacker
    {
        /// <summary>
        /// сколько дней отсчитывать назад
        /// </summary>
        private static readonly int profitChartInterval = 
            AppConfig.GetIntParam("profitChartInterval", 60);

        /// <summary>
        /// точек в мини-графике
        /// </summary>
        public static readonly int profitChartPointCount = 
            AppConfig.GetIntParam("profitChartPointCount", 28);
            
        public static byte[] PackChartInArray(List<EquityOnTime> lastProfit1000)
        {
            var data = new byte[profitChartPointCount];
            if (lastProfit1000.Count < 2) return data;
            if (profitChartPointCount < 2) return data;

            // выбрать точку отсчета
            //var beginProfitChartInterval = DateTime.Now.AddDays(-profitChartInterval);
            const int startIndex = 0;
            //    lastProfit1000.Count > profitChartPointCount 
            //    ? lastProfit1000.FindIndex(e => e.time >= beginProfitChartInterval) : 0;
            //if (startIndex < 0) startIndex = 0;
            var pointsTotal = lastProfit1000.Count - startIndex;

            // сделать выборку, пока не масштабируя ось Y
            var valuesY = lastProfit1000.Skip(startIndex).Select(e => (double)e.equity).ResizeAndResample(
                pointsTotal, profitChartPointCount);

            // вот теперь масштабировать от 0 до 255
            var min = valuesY.Min();
            var max = valuesY.Max();
            var range = max - min;

            for (var i = 0; i < profitChartPointCount; i++)
            {
                var y = range == 0
                            ? (byte) 0
                            : (byte) Math.Round(255*(valuesY[i] - min)/range);
                data[i] = y;
            }
            
            return data;
        }

        public static PointF[] MakePolyline(byte[] chartBytes, int destW, int destH, int padX, int padY)
        {
            var points = new PointF[chartBytes.Length];
            var scaleX = (destW - padX * 2) / (float)chartBytes.Length;
            var scaleY = (float)(destH - padY * 2) / 255;

            for (var i = 0; i < chartBytes.Length; i++)
            {
                points[i] = new PointF(padX + i * scaleX, destH - padY - chartBytes[i] * scaleY);
            }

            return points;
        }

        public static PointF[] MakePolygon(byte[] chartBytes, int destW, int destH, int padX, int padY)
        {
            var points = new PointF[chartBytes.Length + 2];
            var scaleX = (destW - padX * 2) / (float) chartBytes.Length;
            var scaleY = (float) (destH - padY * 2) / 255;

            for (var i = 0; i < chartBytes.Length; i++)
            {
                points[i] = new PointF(padX + i * scaleX, destH - padY - chartBytes[i] * scaleY);
            }
            points[chartBytes.Length] = new PointF(padX + (chartBytes.Length - 1) * scaleX, destH - padY);
            points[chartBytes.Length + 1] = new PointF(padX, destH - padY);

            return points;
        }
    }
}
