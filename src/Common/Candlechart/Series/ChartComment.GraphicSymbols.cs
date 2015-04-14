using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Candlechart.Core;

namespace Candlechart.Series
{
    public partial class ChartComment
    {
        #region Графические шаблоны
        private static VectorGraphObject symbolBuySell =
            new VectorGraphObject(new PointF(61, 95),
                                  new List<PointArray>
                                      {
                                          new Polyline
                                              {
                                                  points = new[]
                                                               {
                                                                   new PointF(61, 95),
                                                                   new PointF(61, 33)
                                                               }
                                              },
                                          new Polygone
                                              {
                                                  points = new[]
                                                               {
                                                                   new PointF(46, 33),
                                                                   new PointF(76, 33),
                                                                   new PointF(61, 2)
                                                               },
                                                               FillColor = Color.Black
                                              }
                                      }) { Alpha = 255 };

        #endregion
    }
}
