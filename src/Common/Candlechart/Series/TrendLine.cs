using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using Candlechart.Chart;
using Candlechart.ChartMath;
using Candlechart.Controls;
using Candlechart.Core;
using Entity;
using TradeSharp.Util;

namespace Candlechart.Series
{
    public class TrendLine : IChartInteractiveObject
    {
        [Browsable(false)]
        public InteractiveObjectSeries Owner { get; set; }

        [Browsable(false)]
        public virtual string ClassName { get { return Localizer.GetString("TitleTrendLine"); } }

        [DisplayName("Magic")]
        [LocalizedCategory("TitleMain")]
        [PropertyOrder(1, 1)]
        public int Magic { get; set; }

        [LocalizedDisplayName("TitleLineType")]
        [LocalizedCategory("TitleMain")]
        [Editor(typeof(TrendLineStyleUIEditor), typeof(UITypeEditor))]
        public TrendLineStyle LineStyle
        {
            get { return lineStyle; }
            set { lineStyle = value; }
        }

        [LocalizedDisplayName("TitleLineColor")]
        [LocalizedCategory("TitleVisuals")]
        public Color LineColor
        {
            get { return lineColor; }
            set { lineColor = value; }
        }

        private float penWidth = 1;
        [LocalizedDisplayName("TitleLineWidth")]
        [LocalizedCategory("TitleVisuals")]
        public float PenWidth
        {
            get
            {
                return penWidth;
            }
            set { penWidth = value; }
        }

        private DashStyle penDashStyle = DashStyle.Solid;
        [LocalizedDisplayName("TitleLineStyle")]
        [LocalizedCategory("TitleVisuals")]
        public DashStyle PenDashStyle
        {
            get { return penDashStyle; }
            set { penDashStyle = value; }
        }

        /// <summary>
        /// цвет заливки фигуры
        /// </summary>
        [LocalizedDisplayName("TitleFilling")]
        [LocalizedCategory("TitleVisuals")]
        public Color ShapeFillColor
        {
            get { return shapeFillColor; }
            set { shapeFillColor = value; }
        }

        /// <summary>
        /// прозрачность фигуры
        /// </summary>
        [LocalizedDisplayName("TitleTransparency2550")]
        [LocalizedCategory("TitleVisuals")]
        [Editor(typeof(TransparencyUITypeWEditor), typeof(UITypeEditor))]
        public int ShapeAlpha
        {
            get { return shapeAlpha; }
            set { shapeAlpha = value; }
        }

        private double pixelScale = 1;
        [LocalizedDisplayName("TitlePatternScaleInPixels")]
        [LocalizedCategory("TitleVisuals")]
        public double PixelScale
        {
            get { return pixelScale; }
            set { pixelScale = value; }
        }

        [LocalizedDisplayName("TitleComment")]
        [LocalizedCategory("TitleMain")]
        public string Comment { get; set; }

        private static readonly Dictionary<TrendLineStyle, VectorGraphObject>
            styleSymbol = new Dictionary<TrendLineStyle, VectorGraphObject>();

        #region Графические символы и декорация линии

        private static readonly VectorGraphObject symbolArrow =
            new VectorGraphObject(new PointF(23, 60),
                                  new List<PointArray>
                                      {
                                          new Polygone
                                              {
                                                  points = new[]
                                                               {
                                                                   new PointF(23, 60),
                                                                   new PointF(17, 42),
                                                                   new PointF(104, 42),
                                                                   new PointF(104, 18),
                                                                   new PointF(146, 60),
                                                                   new PointF(104, 102),
                                                                   new PointF(104, 78),
                                                                   new PointF(17, 78)
                                                               }
                                              }
                                      }) { Alpha = 128 };

        private static readonly VectorGraphObject symbolArrowThin =
            new VectorGraphObject(new PointF(23, 60),
                                  new List<PointArray>
                                      {
                                          new Polygone
                                              {
                                                  points = new[]
                                                               {
                                                                   new PointF(8, 59),
                                                                   new PointF(8, 52),
                                                                   new PointF(155, 52),
                                                                   new PointF(134, 31),
                                                                   new PointF(219, 59),
                                                                   new PointF(134, 87),
                                                                   new PointF(155, 66),
                                                                   new PointF(8, 66)
                                                               }
                                              }
                                      }) { Alpha = 128 };

        private static readonly VectorGraphObject symbolArrowLite =
            new VectorGraphObject(new PointF(0, 15),
                                  new List<PointArray>
                                      {
                                          new Polygone
                                              {
                                                  points = new[]
                                                               {
                                                                   new PointF(0, 11),
                                                                   new PointF(212, 11),
                                                                   new PointF(201, 0),
                                                                   new PointF(240, 15),
                                                                   new PointF(201, 31),
                                                                   new PointF(212, 19),
                                                                   new PointF(0, 19)                                                                   
                                                               }
                                              }
                                      }) { Alpha = 128 };

        private static readonly VectorGraphObject symbolCircle =
            new VectorGraphObject(new PointF(-100, 0),
                                  new List<PointArray>
                                      {
                                          new Polygone
                                              {
                                                  points = new[]
                                                               {
                                                                   new PointF((float) 100.0, (float) 0.0),
                                                                   new PointF((float) 99.6, (float) 8.7),
                                                                   new PointF((float) 98.5, (float) 17.4),
                                                                   new PointF((float) 96.6, (float) 25.9),
                                                                   new PointF((float) 94.0, (float) 34.2),
                                                                   new PointF((float) 90.6, (float) 42.3),
                                                                   new PointF((float) 86.6, (float) 50.0),
                                                                   new PointF((float) 81.9, (float) 57.4),
                                                                   new PointF((float) 76.6, (float) 64.3),
                                                                   new PointF((float) 70.7, (float) 70.7),
                                                                   new PointF((float) 64.3, (float) 76.6),
                                                                   new PointF((float) 57.4, (float) 81.9),
                                                                   new PointF((float) 50.0, (float) 86.6),
                                                                   new PointF((float) 42.3, (float) 90.6),
                                                                   new PointF((float) 34.2, (float) 94.0),
                                                                   new PointF((float) 25.9, (float) 96.6),
                                                                   new PointF((float) 17.4, (float) 98.5),
                                                                   new PointF((float) 8.7, (float) 99.6),
                                                                   new PointF((float) 0.0, (float) 100.0),
                                                                   new PointF((float) -8.7, (float) 99.6),
                                                                   new PointF((float) -17.4, (float) 98.5),
                                                                   new PointF((float) -25.9, (float) 96.6),
                                                                   new PointF((float) -34.2, (float) 94.0),
                                                                   new PointF((float) -42.3, (float) 90.6),
                                                                   new PointF((float) -50.0, (float) 86.6),
                                                                   new PointF((float) -57.4, (float) 81.9),
                                                                   new PointF((float) -64.3, (float) 76.6),
                                                                   new PointF((float) -70.7, (float) 70.7),
                                                                   new PointF((float) -76.6, (float) 64.3),
                                                                   new PointF((float) -81.9, (float) 57.4),
                                                                   new PointF((float) -86.6, (float) 50.0),
                                                                   new PointF((float) -90.6, (float) 42.3),
                                                                   new PointF((float) -94.0, (float) 34.2),
                                                                   new PointF((float) -96.6, (float) 25.9),
                                                                   new PointF((float) -98.5, (float) 17.4),
                                                                   new PointF((float) -99.6, (float) 8.7),
                                                                   new PointF((float) -100.0, (float) 0.0),
                                                                   new PointF((float) -99.6, (float) -8.7),
                                                                   new PointF((float) -98.5, (float) -17.4),
                                                                   new PointF((float) -96.6, (float) -25.9),
                                                                   new PointF((float) -94.0, (float) -34.2),
                                                                   new PointF((float) -90.6, (float) -42.3),
                                                                   new PointF((float) -86.6, (float) -50.0),
                                                                   new PointF((float) -81.9, (float) -57.4),
                                                                   new PointF((float) -76.6, (float) -64.3),
                                                                   new PointF((float) -70.7, (float) -70.7),
                                                                   new PointF((float) -64.3, (float) -76.6),
                                                                   new PointF((float) -57.4, (float) -81.9),
                                                                   new PointF((float) -50.0, (float) -86.6),
                                                                   new PointF((float) -42.3, (float) -90.6),
                                                                   new PointF((float) -34.2, (float) -94.0),
                                                                   new PointF((float) -25.9, (float) -96.6),
                                                                   new PointF((float) -17.4, (float) -98.5),
                                                                   new PointF((float) -8.7, (float) -99.6),
                                                                   new PointF((float) 0.0, (float) -100.0),
                                                                   new PointF((float) 8.7, (float) -99.6),
                                                                   new PointF((float) 17.4, (float) -98.5),
                                                                   new PointF((float) 25.9, (float) -96.6),
                                                                   new PointF((float) 34.2, (float) -94.0),
                                                                   new PointF((float) 42.3, (float) -90.6),
                                                                   new PointF((float) 50.0, (float) -86.6),
                                                                   new PointF((float) 57.4, (float) -81.9),
                                                                   new PointF((float) 64.3, (float) -76.6),
                                                                   new PointF((float) 70.7, (float) -70.7),
                                                                   new PointF((float) 76.6, (float) -64.3),
                                                                   new PointF((float) 81.9, (float) -57.4),
                                                                   new PointF((float) 86.6, (float) -50.0),
                                                                   new PointF((float) 90.6, (float) -42.3),
                                                                   new PointF((float) 94.0, (float) -34.2),
                                                                   new PointF((float) 96.6, (float) -25.9),
                                                                   new PointF((float) 98.5, (float) -17.4),
                                                                   new PointF((float) 99.6, (float) -8.7)
                                                               }
                                              }
                                      }) { Alpha = 128 };

        private static readonly VectorGraphObject symbolTurtle =
            new VectorGraphObject(new PointF(2, 40),
                                  new List<PointArray>
                                      {
                                          new Polygone
                                              {
                                                  points = new[]
                                                               {
                                                                   new PointF(11, 29),
                                                                   new PointF(18, 20),
                                                                   new PointF(32, 14),
                                                                   new PointF(38, 16),
                                                                   new PointF(41, 30),
                                                                   new PointF(40, 36),
                                                                   new PointF(17, 38)                                                                
                                                               }
                                              },
                                          new Polygone
                                              {
                                                  points = new[]
                                                               {                                                                   
                                                                   new PointF(17, 41),
                                                                   new PointF(40, 43),
                                                                   new PointF(41, 49),
                                                                   new PointF(38, 63),
                                                                   new PointF(31, 65),
                                                                   new PointF(18, 59),
                                                                   new PointF(11, 50)
                                                               }
                                              },
                                          new Polygone
                                              {
                                                  points = new[]
                                                               {                                                                   
                                                                   new PointF(42, 13),
                                                                   new PointF(54, 11),
                                                                   new PointF(67, 14),
                                                                   new PointF(64, 35),
                                                                   new PointF(47, 34)                                                                  
                                                               }
                                              },
                                          new Polygone
                                              {
                                                  points = new[]
                                                               {                                                                   
                                                                   new PointF(47, 45),
                                                                   new PointF(64, 44),
                                                                   new PointF(67, 65),
                                                                   new PointF(53, 68),
                                                                   new PointF(42, 66)                                                                 
                                                               }
                                              },
                                          new Polygone
                                              {
                                                  points = new[]
                                                               {                                                                   
                                                                   new PointF(74, 16),
                                                                   new PointF(82, 20),
                                                                   new PointF(88, 27),
                                                                   new PointF(88, 37),
                                                                   new PointF(70, 37)                                                                  
                                                               }
                                              },
                                          new Polygone
                                              {
                                                  points = new[]
                                                               {                                                                   
                                                                   new PointF(70, 42),
                                                                   new PointF(88, 42),
                                                                   new PointF(88, 52),
                                                                   new PointF(82, 59),
                                                                   new PointF(74, 63)                                                                  
                                                               }
                                              },                                          
                                          new Polygone
                                              {
                                                  points = new[]
                                                               {                                                                   
                                                                   new PointF(1, 20),
                                                                   new PointF(4, 15),
                                                                   new PointF(9, 14),
                                                                   new PointF(17, 17),
                                                                   new PointF(10, 25)                                                                   
                                                               }
                                              },
                                          new Polygone
                                              {
                                                  points = new[]
                                                               {                                                                   
                                                                   new PointF(10, 54),
                                                                   new PointF(17, 62),
                                                                   new PointF(9, 65),
                                                                   new PointF(4, 64),
                                                                   new PointF(1, 59)                                                                                                                                    
                                                               }
                                              },
                                          new Polygone
                                              {
                                                  points = new[]
                                                               {                                                                   
                                                                   new PointF(77, 13),
                                                                   new PointF(78, 8),
                                                                   new PointF(78, 2),
                                                                   new PointF(85, 2),
                                                                   new PointF(90, 12),
                                                                   new PointF(89, 21)
                                                               }
                                              },
                                          new Polygone
                                              {
                                                  points = new[]
                                                               {                                                                   
                                                                   new PointF(89, 58),
                                                                   new PointF(90, 69),
                                                                   new PointF(85, 77),
                                                                   new PointF(78, 77),
                                                                   new PointF(79, 73),                                                                                                                                     
                                                                   new PointF(77, 66)
                                                               }
                                              },
                                          new Polygone
                                              {
                                                  points = new[]
                                                               {                                                                   
                                                                   new PointF(92, 32),
                                                                   new PointF(92, 47),
                                                                   new PointF(100, 47),
                                                                   new PointF(105, 45),
                                                                   new PointF(107, 40),                                                                                                                                     
                                                                   new PointF(105, 35),
                                                                   new PointF(100, 32),                                                                                                                                     
                                                                   new PointF(92, 32)
                                                               }
                                              }
                                      }) { Alpha = 128 };

        private static readonly VectorGraphObject symbolTrendUp =
            new VectorGraphObject(new PointF(0, 26),
                                  new List<PointArray>
                                      {
                                          new Polyline
                                              {
                                                  points = new[]
                                                               {
                                                                   new PointF(0, 26),
                                                                   new PointF(40, 0),
                                                                   new PointF(47, 26),
                                                                   new PointF(85, 6)                                                                   
                                                               }
                                              },
                                          new Polygone
                                              {
                                                  points = new[]
                                                               {
                                                                   new PointF(83, 4),
                                                                   new PointF(85, 6),
                                                                   new PointF(85, 8),
                                                                   new PointF(90, 3)                                                                   
                                                               }
                                              }
                                      }) { Alpha = 128 };

        private static readonly VectorGraphObject symbolTrendDown =
            new VectorGraphObject(new PointF(0, -26),
                                  new List<PointArray>
                                      {
                                          new Polyline
                                              {
                                                  points = new[]
                                                               {
                                                                   new PointF(0, -26),
                                                                   new PointF(40, 0),
                                                                   new PointF(47, -26),
                                                                   new PointF(85, -6)                                                                   
                                                               }
                                              },
                                          new Polygone
                                              {
                                                  points = new[]
                                                               {
                                                                   new PointF(83, -4),
                                                                   new PointF(85, -6),
                                                                   new PointF(85, -8),
                                                                   new PointF(90, -3)                                                                   
                                                               }
                                              }
                                      }) { Alpha = 128 };

        private static readonly VectorGraphObject symbolSmoothArrow =
            new VectorGraphObject(new PointF(0, -26),
                                  new List<PointArray>
                                      {
                                          new Polyline
                                              {
                                                  points = new[]
                                                               {
                                                                   new PointF(0, 48),
                                                                   new PointF(11, 44)
                                                               }
                                              },                                          
                                          new Polyline
                                              {
                                                  points = new[]
                                                               {
                                                                   new PointF(16, 41),
                                                                   new PointF(24, 34)
                                                               }
                                              },
                                          new Polyline
                                              {
                                                  points = new[]
                                                               {
                                                                   new PointF(28, 31),
                                                                   new PointF(34, 25)
                                                               }
                                              },
                                          new Polyline
                                              {
                                                  points = new[]
                                                               {
                                                                   new PointF(38, 22),
                                                                   new PointF(41, 18)
                                                               }
                                              },
                                          new Polyline
                                              {
                                                  points = new[]
                                                               {
                                                                   new PointF(44, 16),
                                                                   new PointF(47, 13)
                                                               }
                                              },
                                          new Polyline
                                              {
                                                  points = new[]
                                                               {
                                                                   new PointF(50, 11),
                                                                   new PointF(52, 9)
                                                               }
                                              },
                                          new Polyline
                                              {
                                                  points = new[]
                                                               {
                                                                   new PointF(55, 7),
                                                                   new PointF(57, 6)
                                                               }
                                              },
                                          new Polyline
                                              {
                                                  points = new[]
                                                               {
                                                                   new PointF(60, 4),
                                                                   new PointF(62, 3)
                                                               }
                                              },
                                          new Polyline
                                              {
                                                  points = new[]
                                                               {
                                                                   new PointF(65, 2),
                                                                   new PointF(67, 1)
                                                               }
                                              },
                                          new Polygone
                                              {
                                                  points = new[]
                                                               {
                                                                   new PointF(65, 0),
                                                                   new PointF(71, 0),
                                                                   new PointF(67, 4),
                                                                   new PointF(67, 1)
                                                               }

                                              },
                                      }) { Alpha = 128 };

        private static readonly VectorGraphObject packmanFrame1 =
            new VectorGraphObject(new PointF(0, 0),
                                  new List<PointArray>
                                      {
                                          new Polygone
                                              {
                                                  points = new[]
                                                               {
                                                                   new PointF(126, 59),
                                                                   new PointF(122, 41),
                                                                   new PointF(115, 28),
                                                                   new PointF(104, 17),
                                                                   new PointF(94, 9),
                                                                   new PointF(82, 4),
                                                                   new PointF(68, 2),
                                                                   new PointF(52, 2),
                                                                   new PointF(39, 6),
                                                                   new PointF(27, 12),
                                                                   new PointF(17, 21),
                                                                   new PointF(8, 34),
                                                                   new PointF(3, 45),
                                                                   new PointF(0, 59),
                                                                   new PointF(0, 69),
                                                                   new PointF(2, 80),
                                                                   new PointF(6, 90),
                                                                   new PointF(13, 102),
                                                                   new PointF(23, 112),
                                                                   new PointF(35, 120),
                                                                   new PointF(49, 125),
                                                                   new PointF(61, 127),
                                                                   new PointF(76, 126),
                                                                   new PointF(90, 121),
                                                                   new PointF(103, 112),
                                                                   new PointF(114, 101),
                                                                   new PointF(120, 91),
                                                                   new PointF(124, 80),
                                                                   new PointF(126, 70),
                                                                   new PointF(64, 64)
                                                               }, 
                                                               FillColor = Color.Moccasin,
                                                               LineColor = Color.Brown
                                              }
                                      });

        private static readonly VectorGraphObject packmanFrame2 =
            new VectorGraphObject(new PointF(0, 0),
                                  new List<PointArray>
                                      {
                                          new Polygone
                                              {
                                                  points = new[]
                                                               {
                                                                   new PointF(122, 41),
                                                                   new PointF(115, 28),
                                                                   new PointF(104, 17),
                                                                   new PointF(94, 9),
                                                                   new PointF(82, 4),
                                                                   new PointF(68, 2),
                                                                   new PointF(52, 2),
                                                                   new PointF(39, 6),
                                                                   new PointF(27, 12),
                                                                   new PointF(17, 21),
                                                                   new PointF(8, 34),
                                                                   new PointF(3, 45),
                                                                   new PointF(0, 59),
                                                                   new PointF(0, 69),
                                                                   new PointF(2, 80),
                                                                   new PointF(6, 90),
                                                                   new PointF(13, 102),
                                                                   new PointF(23, 112),
                                                                   new PointF(35, 120),
                                                                   new PointF(49, 125),
                                                                   new PointF(61, 127),
                                                                   new PointF(76, 126),
                                                                   new PointF(90, 121),
                                                                   new PointF(103, 112),
                                                                   new PointF(114, 101),
                                                                   new PointF(120, 91),
                                                                   new PointF(124, 80),
                                                                   new PointF(64, 64)
                                                               }, 
                                                               FillColor = Color.Moccasin,
                                                               LineColor = Color.Brown
                                              }
                                      });

        private static readonly VectorGraphObject packmanFrame3 =
            new VectorGraphObject(new PointF(0, 0),
                                  new List<PointArray>
                                      {
                                          new Polygone
                                              {
                                                  points = new[]
                                                               {
                                                                   new PointF(115, 28),
                                                                   new PointF(104, 17),
                                                                   new PointF(94, 9),
                                                                   new PointF(82, 4),
                                                                   new PointF(68, 2),
                                                                   new PointF(52, 2),
                                                                   new PointF(39, 6),
                                                                   new PointF(27, 12),
                                                                   new PointF(17, 21),
                                                                   new PointF(8, 34),
                                                                   new PointF(3, 45),
                                                                   new PointF(0, 59),
                                                                   new PointF(0, 69),
                                                                   new PointF(2, 80),
                                                                   new PointF(6, 90),
                                                                   new PointF(13, 102),
                                                                   new PointF(23, 112),
                                                                   new PointF(35, 120),
                                                                   new PointF(49, 125),
                                                                   new PointF(61, 127),
                                                                   new PointF(76, 126),
                                                                   new PointF(90, 121),
                                                                   new PointF(103, 112),
                                                                   new PointF(114, 101),
                                                                   new PointF(120, 91),
                                                                   new PointF(64, 64)
                                                               }, 
                                                               FillColor = Color.Moccasin,
                                                               LineColor = Color.Brown
                                              }
                                      });

        private static readonly VectorGraphObject[] packmanFrames = { packmanFrame1, packmanFrame2, packmanFrame3 };
        private static int nextPackmanCurrentFrame;
        private int packmanCurrentFrame;


        private static readonly PointF[] pointsMarkersTriangles =
            new[] { new PointF(0, -4), new PointF(4, 2), new PointF(-4, 2) };

        private static PointF[] pointsArrowCandle =
            new[]
                {
                    new PointF(0, 0), 
                    new PointF(-0.5f, -0.5f), 
                    new PointF(-0.3f, -0.5f), 
                    new PointF(-0.3f, -1), 
                    new PointF(0.3f, -1), 
                    new PointF(0.3f, -0.5f),
                    new PointF(0.5f, -0.5f)
                };

        #endregion

        #region TrendLineStyle enum

        /// <summary>
        /// тип отображения отрезка
        /// </summary>        
        [TypeConverter(typeof(TrendLineStyleTypeConverter))]
        public enum TrendLineStyle
        {
            Отрезок, // отрезок
            Линия, // линия
            Стрелка, // отрезок со стрелкой на конце
            ТолстаяСтрелка, // толстая стрелка
            СредняяСтрелка, // тонкая стрелка
            СтрелкаLite, // аккуратная тощая стрелочка
            Окружность, // окружность
            Прямоугольник, // Прямоугольник
            Рост, // восходящий тренд (зигзаг)
            Спад, // нисходящий тренд (зигзаг)
            Черепаха, // черепашка
            Волна, // сплайн со стрелкой на конце
            ОтрезокСМаркерами, // отрезок с зубцами фикс. масштаба, в пикс
            ЛинияСМаркерами, // линия с зубцами фикс. масштаба, в пикс
            Пэкмен, // пекмен
            СвечнаяСтрелка // маленькая стрелка размером со свечку
        }

        #endregion

        private static int nextLineNum = 1;

        /// <summary>
        /// точки линии (X index, Y - price)
        /// </summary>
        public readonly List<PointD> linePoints = new List<PointD>();

        protected ChartObjectMarker[] markers =
            new[]
            {
                new ChartObjectMarker { action = ChartObjectMarker.MarkerAction.Move },
                new ChartObjectMarker { action = ChartObjectMarker.MarkerAction.Resize },
                new ChartObjectMarker { action = ChartObjectMarker.MarkerAction.Resize }
            };

        private Color lineColor = Color.DarkBlue;
        private TrendLineStyle lineStyle = TrendLineStyle.СтрелкаLite;
        private int shapeAlpha = 128;

        private Color shapeFillColor = Color.White;

        public PointF screenPointA, screenPointB;

        /// <summary>
        /// если при редактировании пользователь держал кнопку Shift,
        /// линия будет скопирована
        /// </summary>
        private bool copyModeOn;

        /// <summary>
        /// в начале и в конце отрезка показывать (в квадратиках) начало - конец
        /// линии в координатах цена - время, расстояние в пунктах
        /// </summary>
        public bool ShowTags { get; set; }

        static TrendLine()
        {
            styleSymbol.Add(TrendLineStyle.ТолстаяСтрелка, symbolArrow);
            styleSymbol.Add(TrendLineStyle.СредняяСтрелка, symbolArrowThin);
            styleSymbol.Add(TrendLineStyle.СтрелкаLite, symbolArrowLite);
            styleSymbol.Add(TrendLineStyle.Окружность, symbolCircle);
            styleSymbol.Add(TrendLineStyle.Черепаха, symbolTurtle);
            styleSymbol.Add(TrendLineStyle.Рост, symbolTrendUp);
            styleSymbol.Add(TrendLineStyle.Спад, symbolTrendDown);
            styleSymbol.Add(TrendLineStyle.Волна, symbolSmoothArrow);
            styleSymbol.Add(TrendLineStyle.Пэкмен, packmanFrames[0]);
        }

        public TrendLine()
        {
            Name = string.Format("{0} {1}", ClassName, nextLineNum++);
            nextPackmanCurrentFrame++;
            if (nextPackmanCurrentFrame >= packmanFrames.Length)
                nextPackmanCurrentFrame = 0;
            packmanCurrentFrame = nextPackmanCurrentFrame;
        }

        public TrendLine(TrendLine spc)
        {
            Name = string.Format("Линия {0}", nextLineNum++);
            nextPackmanCurrentFrame++;
            if (nextPackmanCurrentFrame >= packmanFrames.Length)
                nextPackmanCurrentFrame = 0;
            packmanCurrentFrame = nextPackmanCurrentFrame;
            Comment = spc.Comment;
            lineColor = spc.lineColor;
            lineStyle = spc.lineStyle;
            linePoints = spc.linePoints.ToList();
            Owner = spc.Owner;
            penDashStyle = spc.penDashStyle;
            penWidth = spc.penWidth;
            pixelScale = spc.pixelScale;
            selected = spc.selected;
            shapeAlpha = spc.shapeAlpha;
            shapeFillColor = spc.shapeFillColor;
            Comment = spc.Comment;
        }

        /// <summary>
        /// линия в процессе создания
        /// </summary>
        public bool IsBeingCreated { get; set; }

        public bool Completed
        {
            get { return linePoints.Count == 2; }
        }

        public void AddPoint(double index, double price)
        {
            if (linePoints.Count < 2)
                linePoints.Add(new PointD(index, price));
            else
                linePoints[1] = new PointD(index, price);
        }

        public void Draw(Graphics g, RectangleD worldRect, Rectangle canvasRect,
            PenStorage dicPen, BrushesStorage brushes)
        {
            DrawObjectOnly(g, worldRect, canvasRect, dicPen, brushes);
            if (selected)
            {
                DrawMarkers(g, worldRect, canvasRect);
                DrawCommentForSelected(g, worldRect, canvasRect, dicPen, brushes);
            }
        }

        public void DrawObjectOnly(Graphics g, RectangleD worldRect, Rectangle canvasRect, PenStorage dicPen,
            BrushesStorage brushes)
        {
            var pen = dicPen.GetPen(LineColor, Selected 
                ? PenWidth + 2 
                : PenWidth, 
                penDashStyle);

            if (linePoints.Count == 2)
            {
                if (LineStyle == TrendLineStyle.Линия || LineStyle == TrendLineStyle.Отрезок ||
                    LineStyle == TrendLineStyle.ОтрезокСМаркерами || LineStyle == TrendLineStyle.ЛинияСМаркерами)
                {
                    DrawLine(worldRect, canvasRect, g, pen, brushes, false);
                    return;
                }

                if (LineStyle == TrendLineStyle.Стрелка)
                {
                    DrawLine(worldRect, canvasRect, g, pen, brushes, true);
                    return;
                }

                // пэкмен (анимация)
                if (LineStyle == TrendLineStyle.Пэкмен)
                {
                    packmanCurrentFrame++;
                    if (packmanCurrentFrame >= packmanFrames.Length) packmanCurrentFrame = 0;
                    DrawObject(worldRect, canvasRect, g, pen, packmanFrames[packmanCurrentFrame],
                        dicPen);
                    return;
                }

                // спец символ
                if (styleSymbol.ContainsKey(LineStyle))
                {
                    DrawObject(worldRect, canvasRect, g, pen, styleSymbol[LineStyle], dicPen);
                    return;
                }

                if (LineStyle == TrendLineStyle.Прямоугольник)
                {
                    DrawRectangle(worldRect, canvasRect, g, pen);
                    return;
                }

                if (LineStyle == TrendLineStyle.СвечнаяСтрелка)
                {
                    DrawBarSizedArrow(worldRect, canvasRect, g, pen, symbolSmoothArrow);
                    return;
                }
            }
            else
            {// нарисовать крестиком первую и единственную точку линии
                PointD pt = Conversion.WorldToScreen(linePoints[0], worldRect, canvasRect);
                const int cruaSz = 8, cruaGap = 3;
                g.DrawLine(pen, (float)pt.X, (float)pt.Y - cruaGap, (float)pt.X, (float)pt.Y - cruaSz);
                g.DrawLine(pen, (float)pt.X, (float)pt.Y + cruaGap, (float)pt.X, (float)pt.Y + cruaSz);
                g.DrawLine(pen, (float)pt.X - cruaGap, (float)pt.Y, (float)pt.X - cruaSz, (float)pt.Y);
                g.DrawLine(pen, (float)pt.X + cruaGap, (float)pt.Y, (float)pt.X + cruaSz, (float)pt.Y);
            }
        }

        protected void DrawObject(RectangleD worldRect, Rectangle canvasRect, Graphics g, Pen pen,
            VectorGraphObject obj, PenStorage penDic)
        {
            PointD a = Conversion.WorldToScreen(linePoints[0], worldRect, canvasRect);
            PointD b = Conversion.WorldToScreen(linePoints[1], worldRect, canvasRect);
            var len = (float)Geometry.GetSpanLength(a, b);

            VectorGraphObject smb = obj.Copy();
            smb.PaintLines(lineColor);
            smb.PaintFills(ShapeFillColor);
            smb.Alpha = ShapeAlpha;
            float scale = len / smb.Width;
            // масштабирование
            smb.Scale(scale, scale);
            // перенос
            smb.Move2Point(a.ToPointF());
            // наклон
            smb.Rotate((float)Math.Atan2(b.Y - a.Y, b.X - a.X));
            // нарисовать
            smb.Draw(g, penDic, Selected ? PenWidth + 2 : PenWidth);
        }

        protected void DrawLine(RectangleD worldRect, Rectangle canvasRect, Graphics g,
            Pen pen, BrushesStorage brushes, bool isArrow)
        {
            // получить точки линии (как проекции от края до края экрана или просто точки отрезка)
            PointD[] points;
            if (LineStyle == TrendLineStyle.Отрезок || LineStyle == TrendLineStyle.Стрелка ||
                LineStyle == TrendLineStyle.ОтрезокСМаркерами)
            {
                points = new[] { linePoints[0], linePoints[1] };
            }
            else
            {
                points = new PointD[2];
                if (linePoints[0].X == linePoints[1].X)
                {
                    points[0] = new PointD(linePoints[0].X, worldRect.Bottom);
                    points[1] = new PointD(linePoints[0].X, worldRect.Top);
                }
                else
                {
                    double k = (linePoints[1].Y - linePoints[0].Y) /
                               (linePoints[1].X - linePoints[0].X);
                    double b = linePoints[1].Y - k * linePoints[1].X;
                    points[0] = new PointD(worldRect.Left, worldRect.Left * k + b);
                    points[1] = new PointD(worldRect.Right, worldRect.Right * k + b);
                }
            }
            screenPointA = Conversion.WorldToScreenF(points[0], worldRect, canvasRect);
            screenPointB = Conversion.WorldToScreenF(points[1], worldRect, canvasRect);
            // провести линию
            if (!isArrow)
                g.DrawLine(pen, screenPointA, screenPointB);
            // стрелочка на конце
            else
                DrawArrow(g, pen, screenPointA, screenPointB);
            // если линия декорирована
            if (LineStyle == TrendLineStyle.ОтрезокСМаркерами || LineStyle == TrendLineStyle.ЛинияСМаркерами)
            {
                var length = Math.Sqrt((screenPointA.X - screenPointB.X) * (screenPointA.X - screenPointB.X) +
                             (screenPointA.Y - screenPointB.Y) * (screenPointA.Y - screenPointB.Y));
                if (length > 0)
                {
                    var brushMarker = brushes.GetBrush(pen.Color);
                    var patternStep = 50 * Math.Abs((int)PixelScale);
                    var wholeVector = new PointD(screenPointB.X - screenPointA.X, screenPointB.Y - screenPointA.Y);

                    for (var i = 0; i <= (length - patternStep); i += patternStep)
                    {
                        var a = screenPointA + wholeVector.MultiplyScalar(i / length);
                        var b = screenPointA + wholeVector.MultiplyScalar((i + 1) / length);
                        // нарисовать декоративный элемент, заключенный между точками a-b
                        DrawDecoration(a, b, g, brushMarker);
                    }
                }
            }

            // нарисовать квадратики с текстом в начале и в конце отрезка
            if (ShowTags)
            {
                var brushText = brushes.GetBrush(LineColor);
                var brushFill = brushes.GetBrush(Color.FromArgb(128, ShapeFillColor));

                // начало линии - две строки вида "10.07.2013 22:02, 1.5132", "18.07.2013 13:40, 1.4989"
                const int textPadding = 4;
                var textStart = new List<string>
                    {
                        Owner.Owner.StockSeries.GetCandleOpenTimeByIndex((int) Math.Abs(points[0].X)).ToString("dd.MM.yyyy HH:mm") +
                        ", " +
                        points[0].Y.ToStringUniformPriceFormat(),
                        Owner.Owner.StockSeries.GetCandleOpenTimeByIndex((int) Math.Abs(points[1].X)).ToString("dd.MM.yyyy HH:mm") +
                        ", " +
                        points[1].Y.ToStringUniformPriceFormat()
                    };

                var rect = GetTagRectangle(screenPointA, canvasRect, g, textStart, textPadding,
                    new PointD(points[0].X - points[1].X, points[0].Y - points[1].Y));
                g.FillRectangle(brushFill, rect);
                g.DrawRectangle(pen, rect);

                // текст...
                DrawTagTextLines(g, rect, textPadding, textStart, brushText);

                // кончик линии - текст вида "-0.00206", "-206 пп"
                var delta = points[1].Y - points[0].Y;
                textStart = new List<string>
                    {
                        delta.ToString("f" + DalSpot.Instance.GetPrecision(Owner.Owner.Owner.Symbol)),
                        DalSpot.Instance.GetPointsValue(Owner.Owner.Owner.Symbol, (float)delta).ToStringUniform(1) + " пп"
                    };
                rect = GetTagRectangle(screenPointB, canvasRect, g, textStart, textPadding,
                    new PointD(points[1].X - points[0].X, points[1].Y - points[0].Y));
                g.FillRectangle(brushFill, rect);
                g.DrawRectangle(pen, rect);

                // текст...
                DrawTagTextLines(g, rect, textPadding, textStart, brushText);
            }
        }

        private void DrawTagTextLines(Graphics g, Rectangle rect, int textPadding,
            IEnumerable<string> textStart, Brush brushText)
        {
            var top = rect.Top + textPadding;
            foreach (var line in textStart)
            {
                g.DrawString(line, Owner.Owner.Owner.Font, brushText, rect.Left + textPadding, top);
                top += textPadding;
                top += (int)g.MeasureString(line, Owner.Owner.Owner.Font).Height;
            }
        }

        private Rectangle GetTagRectangle(PointF screenPoint, RectangleD canvasRect, Graphics g,
            List<string> textLines, int textPadding, PointD vector)
        {
            var ht = textPadding;
            var wd = 0;
            foreach (var line in textLines)
            {
                var sz = g.MeasureString(line, Owner.Owner.Owner.Font);
                if (sz.Width > wd) wd = (int)sz.Width;
                ht += (int)(sz.Height + textPadding);
            }
            wd += textPadding * 2;

            // определить позицию - в зависимости от свободного места
            var spaceLeft = screenPoint.X > wd;
            var spaceTop = screenPoint.Y > ht;
            var spaceRight = (float)canvasRect.Width - screenPoint.X > wd;
            var spaceBottom = (float)canvasRect.Height - screenPoint.Y > ht;

            var x = screenPoint.X;
            if (spaceLeft && (!spaceRight || (vector.X <= 0))) 
                x -= wd;

            var y = screenPoint.Y;
            if (spaceTop && (!spaceBottom || (vector.Y >= 0)))
                y -= ht;
                
            return new Rectangle((int)Math.Round(x), (int)Math.Round(y), wd, ht);
        }

        private void DrawRectangle(RectangleD worldRect, Rectangle canvasRect, Graphics g, Pen pen)
        {
            var points = new PointD[2];
            points[0] = Conversion.WorldToScreen(linePoints[0], worldRect, canvasRect);
            points[1] = Conversion.WorldToScreen(linePoints[1], worldRect, canvasRect);
            var wd = Math.Abs(points[0].X - points[1].X);
            var ht = Math.Abs(points[0].Y - points[1].Y);
            var topLeft = new PointD(Math.Min(points[0].X, points[1].X), Math.Min(points[0].Y, points[1].Y));
            g.DrawRectangle(pen, (float)topLeft.X, (float)topLeft.Y, (float)wd, (float)ht);
        }

        protected void DrawBarSizedArrow(RectangleD worldRect, Rectangle canvasRect,
            Graphics g, Pen pen, VectorGraphObject obj)
        {
            var endPoint = Conversion.WorldToScreen(linePoints[1], worldRect, canvasRect);
            var marginPoint = Conversion.WorldToScreen(new PointD(linePoints[1].X + 1, linePoints[1].Y),
                worldRect, canvasRect);
            var width = (int)Math.Abs(marginPoint.X - endPoint.X);
            var scaleY = linePoints[0].Y > linePoints[1].Y ? 1 : -1;
            // нарисовать собственно стрелку - пересчитать координаты
            var points = new Point[pointsArrowCandle.Length];
            for (var i = 0; i < pointsArrowCandle.Length; i++)
            {
                var x = (int)endPoint.X + (int)(pointsArrowCandle[i].X * width);
                var y = (int)endPoint.Y + (int)(pointsArrowCandle[i].Y * width * scaleY);
                points[i] = new Point(x, y);
            }
            // нарисовать
            using (var b = new SolidBrush(Color.FromArgb(ShapeAlpha, ShapeFillColor)))
                g.FillPolygon(b, points);
            using (var p = new Pen(lineColor))
                g.DrawPolygon(p, points);
        }

        private void DrawDecoration(PointD a, PointD b, Graphics g, Brush brush)
        {
            if (LineStyle == TrendLineStyle.ЛинияСМаркерами ||
                LineStyle == TrendLineStyle.ОтрезокСМаркерами)
            {
                var center = new PointF((float)(a.X + b.X) * 0.5f, (float)(a.Y + b.Y) * 0.5f);
                var markersScaled = pointsMarkersTriangles.Select(m => new PointF(
                    m.X * (float)pixelScale + center.X, m.Y * (float)pixelScale + center.Y)).ToArray();
                g.FillPolygon(brush, markersScaled);
            }
        }

        private static void DrawArrow(Graphics g, Pen pen, PointF a, PointF b)
        {
            const int maxArrowLen = 7, minArrowLen = 4, minLen4Arrow = 12;
            const double angle = 20 * Math.PI / 180;

            var abLen = Math.Sqrt((a.X - b.X) * (a.X - b.X) + (a.Y - b.Y) * (a.Y - b.Y));
            if (abLen < minLen4Arrow)
            {
                g.DrawLine(pen, a, b);
                return;
            }
            var arLen = abLen / 4;
            if (arLen > maxArrowLen) arLen = maxArrowLen;
            if (arLen < minArrowLen) arLen = minArrowLen;
            // arrow points
            var pB = new PointF((float)(b.X + (a.X - b.X) * arLen / abLen),
                (float)(b.Y + (a.Y - b.Y) * arLen / abLen));
            var pR = new PointF((float)(b.X + (a.X - b.X) * arLen / abLen * 1.3),
                 (float)(b.Y + (a.Y - b.Y) * arLen / abLen * 1.3));
            PointF[] points = { pB, RotatePoint(pR, b, angle), b, RotatePoint(pR, b, -angle) };

            g.DrawLine(pen, a, pB);
            g.DrawPolygon(pen, points);
        }

        private static PointF RotatePoint(PointF p, PointF pivotPoint, double angle)
        {
            var pX = p.X - pivotPoint.X;
            var pY = p.Y - pivotPoint.Y;
            var x = (float)(pX * Math.Cos(angle) - pY * Math.Sin(angle));
            var y = (float)(pX * Math.Sin(angle) + pY * Math.Cos(angle));
            return new PointF(x + pivotPoint.X, y + pivotPoint.Y);
        }

        public bool IsInObject(PointD a, PointD b, Point ptClient, float tolerance)
        {
            var obj = styleSymbol[LineStyle];
            return IsInObject(a, b, obj, ptClient, tolerance);
        }

        protected bool IsInObject(PointD a, PointD b,
            VectorGraphObject obj, Point ptClient, float tolerance)
        {
            var len = (float)Geometry.GetSpanLength(a, b);
            VectorGraphObject smb = obj.Copy();
            float scale = len / smb.Width;
            // масштабирование
            smb.Scale(scale, scale);
            // перенос
            smb.Move2Point(a.ToPointF());
            // наклон
            smb.Rotate((float)Math.Atan2(b.Y - a.Y, b.X - a.X));
            // проверить попадание
            return smb.IsPointIn(ptClient, tolerance);
        }

        #region IChartInteractiveObject

        private bool selected;
        public bool Selected
        {
            get { return selected; }
            set
            {
                if (!selected && value)
                    SetupMarkers();
                selected = value;
            }
        }

        [LocalizedDisplayName("TitleName")]
        [LocalizedCategory("TitleMain")]
        public string Name { get; set; }

        public DateTime? DateStart
        {
            get { return null; }
            set { }
        }

        public int IndexStart
        {
            get
            {
                if (linePoints.Count > 0)
                    return (int)linePoints[0].X;
                return 0;
            }
        }

        public virtual void SaveInXML(XmlElement parentNode, CandleChartControl owner)
        {
            var node = parentNode.AppendChild(parentNode.OwnerDocument.CreateElement("TrendLine"));

            var nameAttr = node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("name"));
            nameAttr.Value = Name;

            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("magic")).Value = Magic.ToString();

            var colorAttr = node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("color"));
            colorAttr.Value = lineColor.ToArgb().ToString();

            var colorFillAttr = node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("colorFill"));
            colorFillAttr.Value = ShapeFillColor.ToArgb().ToString();

            var alphaAttr = node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("alpha"));
            alphaAttr.Value = ShapeAlpha.ToString();

            var typeAttr = node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("type"));
            typeAttr.Value = ((int)LineStyle).ToString();

            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("pixelScale")).Value =
                PixelScale.ToStringUniform();

            node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("comment")).Value =
                Comment;

            if (linePoints.Count > 0)
            {
                var time = owner.chart.StockSeries.GetCandleOpenTimeByIndex((int)linePoints[0].X);
                XmlAttribute axAttr = node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("firstX"));
                axAttr.Value = time.ToString("ddMMyyyy HHmmss", CultureProvider.Common);
                XmlAttribute ayAttr = node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("firstY"));
                ayAttr.Value = linePoints[0].Y.ToString(CultureProvider.Common);
            }

            if (linePoints.Count > 1)
            {
                var time = owner.chart.StockSeries.GetCandleOpenTimeByIndex((int)linePoints[1].X);
                XmlAttribute axAttr = node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("secondX"));
                axAttr.Value = time.ToString("ddMMyyyy HHmmss", CultureProvider.Common);
                XmlAttribute ayAttr = node.Attributes.Append(parentNode.OwnerDocument.CreateAttribute("secondY"));
                ayAttr.Value = linePoints[1].Y.ToString(CultureProvider.Common);
            }
        }

        public virtual void LoadFromXML(XmlElement itemNode, CandleChartControl owner)
        {
            if (itemNode.Attributes["name"] != null) Name = itemNode.Attributes["name"].Value;
            if (itemNode.Attributes["magic"] != null) Magic = itemNode.Attributes["magic"].Value.ToIntSafe() ?? 0;
            if (itemNode.Attributes["color"] != null)
                lineColor = Color.FromArgb(int.Parse(itemNode.Attributes["color"].Value));
            if (itemNode.Attributes["colorFill"] != null)
                ShapeFillColor = Color.FromArgb(int.Parse(itemNode.Attributes["colorFill"].Value));
            if (itemNode.Attributes["alpha"] != null)
                ShapeAlpha = int.Parse(itemNode.Attributes["alpha"].Value);
            if (itemNode.Attributes["type"] != null)
                lineStyle = (TrendLineStyle)int.Parse(itemNode.Attributes["type"].Value);
            if (itemNode.Attributes["pixelScale"] != null)
                PixelScale = itemNode.Attributes["pixelScale"].Value.ToDoubleUniform();
            if (itemNode.Attributes["comment"] != null)
                Comment = itemNode.Attributes["comment"].Value;

            if (itemNode.Attributes["firstX"] != null)
            {
                var time = DateTime.ParseExact(itemNode.Attributes["firstX"].Value, "ddMMyyyy HHmmss",
                                               CultureProvider.Common);
                var index = owner.chart.StockSeries.GetIndexByCandleOpen(time);
                linePoints.Add(new PointD(index, double.Parse(itemNode.Attributes["firstY"].Value,
                    CultureProvider.Common)));
            }

            if (itemNode.Attributes["secondX"] != null)
            {
                var time = DateTime.ParseExact(itemNode.Attributes["secondX"].Value, "ddMMyyyy HHmmss",
                                               CultureProvider.Common);
                var index = owner.chart.StockSeries.GetIndexByCandleOpen(time);
                linePoints.Add(new PointD(index, double.Parse(itemNode.Attributes["secondY"].Value,
                    CultureProvider.Common)));
            }
        }

        public ChartObjectMarker IsInMarker(int screenX, int screenY, Keys modifierKeys)
        {
            copyModeOn = false;
            var ptClient = Owner.Owner.PointToClient(new Point(screenX, screenY));
            var selMarker = markers.FirstOrDefault(marker => marker.IsIn(ptClient.X, ptClient.Y,
                Owner.Owner.WorldRect, Owner.Owner.CanvasRect));
            if (selMarker == null) return null;
            // если нажата кнопка Shift, запомнить: при редактировании скопировать объект
            if ((modifierKeys & Keys.Shift) == Keys.Shift)
                copyModeOn = true;
            return selMarker;
        }

        public virtual void OnMarkerMoved(ChartObjectMarker marker)
        {
            marker.RecalculateModelCoords(Owner.Owner.WorldRect, Owner.Owner.CanvasRect);

            if (copyModeOn)
            {
                // копировать исходную линию
                ((TrendLineSeries)Owner).CopyLine(this);
                copyModeOn = false;
            }

            // параллельный перенос
            if (marker.action == ChartObjectMarker.MarkerAction.Move)
            {
                var mid = linePoints.Count == 2
                              ? new PointD(
                                    (linePoints[0].X + linePoints[1].X) * 0.5,
                                    (linePoints[0].Y + linePoints[1].Y) * 0.5) : linePoints[0];
                var vx = marker.centerModel.X - mid.X;
                var vy = marker.centerModel.Y - mid.Y;
                for (var i = 1; i < 3; i++)
                {
                    markers[i].centerModel = new PointD(markers[i].centerModel.X + vx,
                        markers[i].centerModel.Y + vy);
                }
                linePoints[0] = markers[1].centerModel;
                linePoints[linePoints.Count - 1] = markers[2].centerModel;

                return;
            }

            // растягивание
            var markerIndex = 0;
            for (var i = 0; i < markers.Length; i++)
            {
                if (markers[i] != marker) continue;
                markerIndex = i - 1;
                break;
            }
            var pointIndex = markerIndex >= linePoints.Count ? 0 : markerIndex;
            linePoints[pointIndex] = marker.centerModel;
            markers[0].centerModel = new PointD(
                (markers[1].centerModel.X + markers[2].centerModel.X) * 0.5,
                (markers[1].centerModel.Y + markers[2].centerModel.Y) * 0.5);
        }

        protected virtual void SetupMarkers()
        {
            var firstPoint = linePoints[0];
            var lastPoint = linePoints.Count > 1 ? linePoints[1] : linePoints[0];
            markers[0].centerModel = new PointD((firstPoint.X + lastPoint.X) * 0.5f,
                                                (firstPoint.Y + lastPoint.Y) * 0.5f);
            markers[1].centerModel = firstPoint;
            markers[2].centerModel = lastPoint;
        }

        protected void DrawMarkers(Graphics g, RectangleD worldRect, Rectangle canvasRect)
        {
            using (var penStorage = new PenStorage())
            using (var brushStorage = new BrushesStorage())
                foreach (var marker in markers)
                {
                    marker.CalculateScreenCoords(worldRect, canvasRect);
                    marker.Draw(g, penStorage, brushStorage);
                }
        }

        public void AjustColorScheme(CandleChartControl chart)
        {
            var clBack = chart.chart.visualSettings.ChartBackColor;
            lineColor = ChartControl.ChartVisualSettings.AdjustColor(lineColor, clBack);
        }

        public Image CreateSample(Size sizeHint)
        {
            if (linePoints.Count < 2)
                return null;
            var result = new Bitmap(sizeHint.Width, sizeHint.Height);
            // должна уместиться окружность с диаметром в длину отрезка и с центром, совпадающим с центром отрезка
            // но для этого решение не найдено
            var minX = linePoints.Select(p => p.X).Min();
            var minY = linePoints.Select(p => p.Y).Min();
            var maxX = linePoints.Select(p => p.X).Max();
            var maxY = linePoints.Select(p => p.Y).Max();
            // margins
            var marginX = (maxX - minX) / 10;
            var marginY = (maxY - minY);
            minX -= marginX;
            maxX += marginX;
            minY -= marginY;
            maxY += marginY;
            var worldRect = new RectangleD(minX, minY, maxX - minX, maxY - minY);
            Draw(Graphics.FromImage(result), worldRect, new Rectangle(new Point(0, 0), sizeHint), new PenStorage(),
                 new BrushesStorage());
            return result;
        }

        #endregion
    
        private void DrawCommentForSelected(Graphics g,
            RectangleD worldRect, Rectangle canvasRect,
            PenStorage pens, BrushesStorage brushes)
        {
            if (string.IsNullOrEmpty(Comment)) return;

            var commentBox = new ChartComment
                {
                    Color = LineColor,
                    ColorFill = ShapeFillColor,
                    Text = Comment,
                    HideArrow = true,
                    FillTransparency = 160
                };
            var textRect = commentBox.GetCommentRectangle(g, worldRect, canvasRect);

            // прикинуть, куда лучше захреначить комментарий - в центр или в конец
            // спроецировать координаты курсора на линию
            var cursorPoint = Owner.Owner.PointToClient(Control.MousePosition);
            var screenPoints = linePoints.Select(p => Conversion.WorldToScreenF(p, worldRect, canvasRect)).ToList();
            var pivot = Geometry.GetProjectionPointOnLine(cursorPoint, screenPoints[0], screenPoints[1]);
            
            // пробуем два варианта размещения - привязываем левый верхний или
            // правый нижний углы комментария, смотрим, какой вариант ближе к центру экрана
            var potentialPivots = new []
                {
                    new PointF(pivot.X,
                               pivot.Y + textRect.Height/2),
                    new PointF(pivot.X - textRect.Width,
                               pivot.Y - textRect.Height/2)
                };
            var cx = canvasRect.Left + canvasRect.Width / 2;
            var cy = canvasRect.Top + canvasRect.Height / 2;
            var bestPivotIndex = potentialPivots.IndexOfMin(p => (p.X - cx)*(p.X - cx) + (p.Y - cy)*(p.Y - cy));
            pivot = potentialPivots[bestPivotIndex];
            var worldPivotPoint = Conversion.ScreenToWorld(pivot, worldRect, canvasRect);

            commentBox.PivotPrice = worldPivotPoint.Y;
            commentBox.PivotIndex = worldPivotPoint.X;

            // таки нарисовать
            using (var fonts = new FontStorage(Owner.Owner.Owner.Font))
                commentBox.DrawComment(g, fonts, worldRect, canvasRect, pens, brushes,
                    new List<Rectangle>());
        }
    }
}
