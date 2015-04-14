using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace FastGrid
{
    public partial class FastGrid
    {
        private static readonly Dictionary<Type, int> defaultColumnWidth =
            new Dictionary<Type, int>
                {
                    { typeof(int), 50 },
                    { typeof(int?), 50 },
                    { typeof(decimal), 60 },
                    { typeof(decimal?), 60 },
                    { typeof(double), 60 },
                    { typeof(double?), 60 },
                    { typeof(float), 60 },
                    { typeof(float?), 60 },
                    { typeof(Enum), 60 },
                    { typeof(string), 80 },
                    { typeof(char), 35 },
                    { typeof(DateTime), 80 }
                };

        /// <summary>
        /// bind to list of some object
        /// do not generate columns info auto
        /// </summary>        
        public void DataBind(IList objects)
        {
            ClearCells();
            if (objects.Count == 0) return;
            var specimenType = objects[0].GetType();
            DataBind(objects, specimenType);
        }

        /// <summary>
        /// bind to a list of objects, set up the dictionary that stores
        /// propertyinfos for columns
        /// </summary>        
        public void DataBind(IList objects, Type specimenType)
        {
            ClearCells();
            
            // fill the dictionary (column - property info)
            columnProperty = new Dictionary<FastColumn, PropertyInfo>();
            var allProps = specimenType.GetProperties();

            foreach (var col in columns)
            {
                // find such property
                var colPropName = col.PropertyName;
                var prop = allProps.FirstOrDefault(p => p.Name == colPropName);
                if (string.IsNullOrEmpty(col.Title))
                {
                    // get property display name
                    var attrs = prop.GetCustomAttributes(typeof(DisplayNameAttribute), false);
                    if (attrs.Length == 1)
                        col.PropertyDisplayName = ((DisplayNameAttribute)attrs[0]).DisplayName;
                }
                columnProperty.Add(col, prop);
            }

            AddRows(objects);

            // calculate ResultedWidth 4 the 1st time
            CheckSize();
        }

        /// <summary>
        /// make columns auto and fill rows
        /// </summary>
        /// <param name="objects">rows' data</param>
        /// <param name="specimenType">type of value object</param>
        /// <param name="skipPropertiesWoDisplayName">do not add column where no DsiplayName attribute provided</param>
        /// <param name="boolSubstitutions">null or substitution for boolean: true, false, null</param>
        /// <param name="defaultColumnMinWidth">default column min width, pixels</param>
        /// <param name="dateTimeFormatString">format string for DateTime columns (may be null)</param>
        public void DataBind(IList objects, Type specimenType, 
            bool skipPropertiesWoDisplayName, string[] boolSubstitutions, int defaultColumnMinWidth,
            string dateTimeFormatString)
        {
            // make columns by public instance props
            foreach (var prop in specimenType.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                var attrs = prop.GetCustomAttributes(true);
                var browsAttr = attrs.FirstOrDefault(a => a is BrowsableAttribute);
                if (browsAttr != null)
                    if (!((BrowsableAttribute)browsAttr).Browsable) continue;

                // get column title from DisplayName attr
                var columnTitle = prop.Name;
                var propName = columnTitle;

                var propNameAtr = attrs.FirstOrDefault(a => a is DisplayNameAttribute);
                if (propNameAtr != null)
                    columnTitle = ((DisplayNameAttribute) propNameAtr).DisplayName;
                else
                {
                    if (skipPropertiesWoDisplayName) continue;
                }

                // get column width by property type
                int columnWidth;
                if (!defaultColumnWidth.TryGetValue(prop.PropertyType, out columnWidth))
                    columnWidth = defaultColumnMinWidth;
                
                // make column record
                var col = new FastColumn(propName, columnTitle)
                              {
                                  Title = columnTitle, 
                                  ColumnMinWidth = columnWidth
                              };

                // add column - to - property association
                columnProperty.Add(col, prop);

                // format boolean ...
                if (prop.PropertyType == typeof(bool) || prop.PropertyType == typeof(bool?))
                if (boolSubstitutions != null && boolSubstitutions.Length > 1)
                {
                    col.ColumnMinWidth = 0;
                    var strTrue = boolSubstitutions[0];
                    var strFalse = boolSubstitutions[1];
                    var strNull = boolSubstitutions.Length == 2 ? strFalse : boolSubstitutions[2];
                    
                    // measure column width (fixed!)
                    using (var g = CreateGraphics())
                    {
                        var fnt = FontHeader ?? Font;
                        var maxWd = g.MeasureString(col.Title, fnt).Width + 6;
                        foreach (var str in boolSubstitutions)
                        {
                            var wd = g.MeasureString(str, fnt).Width;
                            if (wd > maxWd) maxWd = wd;
                        }
                        col.ColumnWidth = (int)maxWd + 4;
                    }

                    // formatter
                    if (prop.PropertyType == typeof(bool))
                        col.formatter = c => ((bool) c) ? strTrue : strFalse;
                    else
                        col.formatter = c => ((bool?)c).HasValue ? 
                            (((bool?)c).Value ? strTrue : strFalse) : strNull;
                }

                // format DateTime ...
                if (prop.PropertyType == typeof(DateTime) || prop.PropertyType == typeof(DateTime?))
                    if (!string.IsNullOrEmpty(dateTimeFormatString))
                        col.FormatString = dateTimeFormatString;

                columns.Add(col);
            }
            
            // calc min table width
            CalcSetTableMinWidth(defaultColumnMinWidth);

            // add rows
            AddRows(objects);

            CheckSize();
        }

        public IEnumerable<T> GetRowValues<T>(bool selectedOnly)
        {
            var fastRows = selectedOnly ? rows.Where(r => r.Selected) : rows;
            return fastRows.Select(r => (T) r.ValueObject);
        }

        private void AddRow(object obj)
        {
            var row = new FastRow(this, obj, columnProperty);
            if (userDrawCellText != null)
                row.UserDrawCellText += OnUserDrawCellText;
            rows.Add(row);
        }

        private void AddRows(IList objects)
        {
            if (objects.Count == 0) return;

            // add rows
            foreach (var obj in objects)
                AddRow(obj);

            // ancor first and last rows
            if (rows.Count > 0)
            {
                if (StickFirst) rows[0].anchor = FastRow.AnchorPosition.AnchorTop;
                if (StickLast) rows[rows.Count - 1].anchor = FastRow.AnchorPosition.AnchorBottom;
            }

            // group & order rows
            Regroup();
            OrderRows();
            Invalidate();
        }

        private void ClearCells()
        {
            if (rows.Count != 0) rows.Clear();
        }

        /// <summary>
        /// обновить одну строку
        /// вернуть флаг необходимости перерисовки
        /// </summary>
        public bool UpdateRow(int rowIndex, object boundValue)
        {
            if (rowIndex >= rows.Count || rowIndex < 0) return false;
            var selected = rows[rowIndex].Selected;
            var row = new FastRow(this, boundValue, columnProperty) { Selected = selected };
            if (userDrawCellText != null)
                row.UserDrawCellText += OnUserDrawCellText;
            rows[rowIndex] = row;
            return true;
        }
    }
}
