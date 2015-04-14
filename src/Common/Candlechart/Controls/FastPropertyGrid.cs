using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Candlechart.Core;
using FastGrid;
using TradeSharp.Util;

namespace Candlechart.Controls
{
    // TODO: make it editable in designer
    public class FastPropertyGrid : SplitContainer
    {
        public delegate void OpenPropertyEditorDel(int rowIndex, FastColumn col);

        private readonly TabControl tabControl = new TabControl {Dock = DockStyle.Fill};

        private readonly SplitContainer detailPanel = new SplitContainer
            {
                BackColor = Color.White,
                Orientation = Orientation.Vertical,
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.FixedSingle
            };

        private readonly RichTextBox detailTextBox = new RichTextBox
            {
                //BackColor = Color.White,
                Dock = DockStyle.Fill,
                ReadOnly = true,
                BorderStyle = BorderStyle.None
            };

        private object selectedObject;
        [Browsable(false)]
        public object SelectedObject
        {
            get { return selectedObject; }
            set
            {
                selectedObject = value;
                selectedObjects = null;
                Rebuild();
            }
        }

        private List<object> selectedObjects;
        [Browsable(false)]
        public List<object> SelectedObjects
        {
            get { return selectedObjects; }
            set
            {
                selectedObject = null;
                selectedObjects = value;
                Rebuild();
            }
        }

        public OpenPropertyEditorDel OpenPropertyEditor;

        private int editingRowIndex;

        private FastColumn editingColumn;

        public FastPropertyGrid()
        {
            Orientation = Orientation.Horizontal;
            Panel1.Controls.Add(tabControl);
            tabControl.TabPages.Clear();
            Panel2.Controls.Add(detailPanel);
            detailPanel.Panel1.Controls.Add(detailTextBox);
            detailPanel.Panel2.BackgroundImageLayout = ImageLayout.Zoom;
            detailPanel.Panel2.Resize += OnSamplePanelResize;
        }

        private void OnSamplePanelResize(object o, EventArgs args)
        {
            RebuildSample();
        }

        private void Rebuild()
        {
            // ищем отображаемые свойства как пересечение множеств свойств выбранных объектов
            var selectedProperties = new List<PropertyInfo>();
            if (selectedObjects != null)
            {
                var chartObjectPropertiesDict = new Dictionary<object, List<PropertyInfo>>();
                var allProperties = new List<PropertyInfo>();
                foreach (var chartObject in selectedObjects)
                {
                    var chartObjectProperties = chartObject.GetType().GetProperties().Where(IsBrowsable).ToList();
                    chartObjectPropertiesDict.Add(chartObject, chartObjectProperties);
                    allProperties.AddRange(chartObjectProperties);
                }
                // ищем пересечение
                foreach (var property in allProperties)
                {
                    var add = true;
                    foreach (var chartObject in selectedObjects)
                    {
                        // одинаковые по сути свойства представлены разными объектами PropertyInfo,
                        // а IEquatable.Equals сравнивает ссылки, поэтому проверку на равенство выполняем самостоятельно
                        if (!chartObjectPropertiesDict[chartObject].Any(p => p.Name == property.Name && p.PropertyType == property.PropertyType))
                        {
                            add = false;
                            break;
                        }
                    }
                    if (add && !selectedProperties.Any(p => p.Name == property.Name && p.PropertyType == property.PropertyType))
                        selectedProperties.Add(property);
                }
            }
            else if (selectedObject != null)
                selectedProperties = selectedObject.GetType().GetProperties().Where(IsBrowsable).ToList();

            // определяем категории
            var categories = new List<Cortege2<string, int>>();
            foreach (var property in selectedProperties)
            {
                var category = GetCategory(property);
                if (!string.IsNullOrEmpty(category) && !categories.Exists(c => c.a == category))
                    categories.Add(new Cortege2<string, int>(category, GetOrder(property).b));
            }

            // сортируем категории
            categories.Sort((arg1, arg2) => arg1.b - arg2.b);

            // для каждой категории создаем по вкладке
            tabControl.TabPages.Clear();
            var blankRow = new FastPropertyGridRow();
            foreach (var category in categories)
            {
                tabControl.TabPages.Add(category.a, category.a);
                var fastGrid = new FastGrid.FastGrid {Dock = DockStyle.Fill, FitWidth = true, CaptionHeight = 0};
                fastGrid.Columns.Add(new FastColumn(blankRow.Property(p => p.Title)));
                fastGrid.Columns.Add(new FastColumn(blankRow.Property(p => p.StringValue)) {ColumnFont = new Font(Font, FontStyle.Bold)});
                fastGrid.Columns.Add(new FastColumn(blankRow.Property(p => p.Property)) {Visible = false});
                fastGrid.Columns.Add(new FastColumn(blankRow.Property(p => p.Value)) {Visible = false});
                fastGrid.UserHitCell += UserHitCell;
                tabControl.TabPages[category.a].Controls.Add(fastGrid);
                // определяем свойства внутри категории
                var propertiesInCategory = new List<Cortege2<PropertyInfo, int>>();
                foreach (var property in selectedProperties)
                {
                    if (GetCategory(property) != category.a)
                        continue;
                    propertiesInCategory.Add(new Cortege2<PropertyInfo, int>(property, GetOrder(property).a));
                }

                // сортируем свойства
                propertiesInCategory.Sort((arg1, arg2) => arg1.b - arg2.b);

                // создаем данные типа "имя-значение"
                var data = new List<FastPropertyGridRow>();
                foreach (var property in propertiesInCategory)
                {
                    // в случае множества объектов, null - фиктивное значение (значение, используемое для отображения в случае различных значений)
                    object value = null;
                    if (selectedObjects != null)
                    {
                        var allValues = GetDistinctPropertyValues(selectedObjects, property.a);
                        if (allValues.Count == 1)
                            value = allValues.First();
                    }
                    else if (selectedObject != null)
                        value = property.a.GetValue(selectedObject, null);

                    data.Add(new FastPropertyGridRow
                        {
                            Title = GetDisplayName(property.a),
                            Value = value,
                            Property = property.a,
                            StringValue = GetStringValue(value, property.a)
                        });
                }
                fastGrid.DataBind(data);

                // фиксируем ширину колонки с наименованием
                fastGrid.CheckSize(true);
            }
            RebuildSample();
        }

        private static object GetStringValue(object value, PropertyInfo property)
        {
            var result = value;
            if (value != null && value.GetType().IsSubclassOf(typeof(Enum)))
            {
                var resourceKey = "Enum" + value.GetType().Name + value;
                result = Localizer.HasKey(resourceKey) ? Localizer.GetString(resourceKey) : value.ToString();
            }
            var formatAttr = property.GetCustomAttributes(true).FirstOrDefault(a => a is DisplayFormatAttribute) as DisplayFormatAttribute;
            if (formatAttr != null)
                result = string.Format(formatAttr.DataFormatString, value);
            return result;
        }

        private void RebuildSample()
        {
            if (selectedObject != null)
            {
                var chartObject = selectedObject as IChartInteractiveObject;
                if (chartObject != null)
                    detailPanel.Panel2.BackgroundImage = chartObject.CreateSample(detailPanel.Panel2.Size);
                else
                    detailPanel.Panel2Collapsed = true;
            }
        }

        private static List<object> GetDistinctPropertyValues(List<object> objects, PropertyInfo property)
        {
            var result = new List<object>();
            foreach (var chartObject in objects)
            {
                // ищем свойство у текущего объекта, равное property
                var currentProperty = chartObject.GetType().GetProperty(property.Name, property.PropertyType);
                if (currentProperty == null)
                {
                    result.Add(null);
                    continue;
                }
                var chartValue = currentProperty.GetValue(chartObject, null);
                if (!result.Contains(chartValue))
                    result.Add(chartValue);
            }
            return result;
        }

        private static string GetCategory(PropertyInfo property)
        {
            var attribute =
                property.GetCustomAttributes(true).FirstOrDefault(a => a is LocalizedCategoryAttribute) as
                CategoryAttribute;
            if (attribute == null)
                attribute =
                    property.GetCustomAttributes(true).FirstOrDefault(a => a is CategoryAttribute) as
                    CategoryAttribute;
            return attribute == null ? CategoryAttribute.Default.Category : attribute.Category;
        }

        private static Cortege2<int, int> GetOrder(PropertyInfo property)
        {
            var attribute =
                property.GetCustomAttributes(true).FirstOrDefault(a => a is PropertyOrderAttribute) as
                PropertyOrderAttribute;
            return attribute == null
                       ? new Cortege2<int, int>(int.MaxValue, int.MaxValue)
                       : new Cortege2<int, int>(attribute.Order, attribute.CategoryOrder);
        }

        private static string GetDisplayName(PropertyInfo property)
        {
            var attribute =
                property.GetCustomAttributes(true).FirstOrDefault(a => a is LocalizedDisplayNameAttribute) as
                DisplayNameAttribute;
            if (attribute == null)
                attribute =
                    property.GetCustomAttributes(true).FirstOrDefault(a => a is DisplayNameAttribute) as
                    DisplayNameAttribute;
            return attribute == null ? property.Name : attribute.DisplayName;
        }

        private static bool IsBrowsable(PropertyInfo property)
        {
            if (!property.GetCustomAttributes(true).Any(a => a is DisplayNameAttribute) &&
                !property.GetCustomAttributes(true).Any(a => a is LocalizedDisplayNameAttribute))
                return false;
            var attribute = property.GetCustomAttributes(true).FirstOrDefault(a => a is BrowsableAttribute) as BrowsableAttribute;
            return attribute == null || attribute.Browsable;
        }

        private void UserHitCell(object sender, MouseEventArgs mouseEventArgs, int rowIndex, FastColumn col)
        {
            if (tabControl.SelectedTab == null)
                return;
            var fastGrid = tabControl.SelectedTab.Controls[0] as FastGrid.FastGrid;
            if (fastGrid == null)
                return;
            // отображение описания
            var rowObject = (FastPropertyGridRow) fastGrid.rows[rowIndex].ValueObject;
            if (rowObject.Property == null)
                return;
            detailTextBox.SelectionFont = Font;
            detailTextBox.Text = rowObject.Title;
            var attrs = rowObject.Property.GetCustomAttributes(true);
            var descriptionAttribute = attrs.FirstOrDefault(a => a is LocalizedDescriptionAttribute) as DescriptionAttribute;
            if (descriptionAttribute == null)
                descriptionAttribute = attrs.FirstOrDefault(a => a is DescriptionAttribute) as DescriptionAttribute;
            if (descriptionAttribute != null)
                detailTextBox.Text += Environment.NewLine + descriptionAttribute.Description;
            detailTextBox.Select(0, rowObject.Title.Length);
            detailTextBox.SelectionFont = new Font(Font, FontStyle.Bold);
            // открытие редактора
            if (fastGrid.Columns.IndexOf(col) != 1)
                return;
            editingRowIndex = rowIndex;
            editingColumn = col;
            var editorAttribute = attrs.FirstOrDefault(a => a is EditorAttribute) as EditorAttribute;
            var editorType = editorAttribute != null
                                 ? Type.GetType(editorAttribute.EditorTypeName)
                                 : GetStandardEditorType(rowObject.Property.PropertyType);
            if (editorType == null)
            {
                OpenBaseEditor(rowObject.Property, fastGrid, rowIndex, col);
                return;
            }
            OpenSpecialEditor();
        }

        // открытие стандартного редактора в зависимости от типа свойства
        private void OpenBaseEditor(PropertyInfo property, FastGrid.FastGrid fastGrid, int rowIndex, FastColumn col)
        {
            var coords = fastGrid.GetCellCoords(col, rowIndex);
            var blankRow = new FastPropertyGridRow();
            var cellValue =
                fastGrid.rows[rowIndex].cells[fastGrid.Columns.FindIndex(c => c.PropertyName == blankRow.Property(p => p.Value))].CellValue;
            var propType = property.PropertyType;
            if ((propType == typeof (bool) || propType.IsEnum))
            {
                var pop = new PopupListBox(cellValue, rowIndex, col, propType, UpdateObject, fastGrid);
                pop.ShowOptions(coords.X, coords.Y);
                return;
            }
            if ((propType == typeof (string) || Converter.IsConvertable(propType)))
            {
                // редактор подставляется в FastGrid.PopupTextBox
                try
                {
                    var pop = new PopupTextBox(cellValue, col.ResultedWidth, fastGrid.CellHeight, rowIndex, col, property, null);
                    pop.OnValueUpdated += UpdateObject;
                    pop.Show(fastGrid, coords);
                }
                catch(Exception ex)
                {
                    Logger.Error("FastPropertyGrid.OpenBaseEditor", ex);
                }
                return;
            }
        }

        // открытие специального редактора, указанного для свойства
        private void OpenSpecialEditor()
        {
            if (OpenPropertyEditor != null)
            {
                OpenPropertyEditor(editingRowIndex, editingColumn);
                return;
            }
            var fastGrid = tabControl.SelectedTab.Controls[0] as FastGrid.FastGrid;
            if (fastGrid == null)
                return;
            var rowObject = (FastPropertyGridRow) fastGrid.rows[editingRowIndex].ValueObject;
            var editorAttribute = rowObject.Property.GetCustomAttributes(true).FirstOrDefault(a => a is EditorAttribute) as EditorAttribute;
            var editorType = editorAttribute != null ? Type.GetType(editorAttribute.EditorTypeName) : GetStandardEditorType(rowObject.Property.PropertyType);
            if (editorType == typeof (MultilineStringEditor))
                editorType = typeof (TextUItypeWEditor);
            if (editorType == null)
                return;
            var constructor = editorType.GetConstructor(new Type[0]);
            if (constructor == null)
                return;
            var editor = (UITypeEditor) constructor.Invoke(new object[0]);
            // пока нет возможности вызsвfnm popup-редакторs
            if (editor.GetEditStyle() != UITypeEditorEditStyle.Modal)
            {
                // попробуем показать хотя бы базовый
                OpenBaseEditor(rowObject.Property, fastGrid, editingRowIndex, editingColumn);
                return;
            }
            try
            {
                var newValue = editor.EditValue(null, null, rowObject.Value);
                // если старое значение равно новому, то, возможно, от редактирования отказались
                if (newValue != rowObject.Value)
                    UpdateObject(editingColumn, editingRowIndex, newValue);
            }
            catch(Exception ex)
            {
                Logger.Error("FastpropertyGrid.OpenSpecialEditor: editor error", ex);
            }
        }

        private static Type GetStandardEditorType(Type objectType)
        {
            Type editorType = null;
            if (objectType == typeof (Color))
                editorType = typeof (ColorUITypeWEditor);
            else if (objectType == typeof (ContentAlignment))
                editorType = typeof (ContentAlignmentEditor);
            else if (objectType == typeof (Cursor))
                editorType = typeof (CursorEditor);
            else if (objectType == typeof (Font))
                editorType = typeof (FontEditor);
            else if (objectType == typeof (Icon))
                editorType = typeof (IconEditor);
            else if (objectType == typeof (Image))
                editorType = typeof (ImageEditor);
            else if (objectType == typeof (DateTime))
                editorType = typeof (DateTimeEditor);
            return editorType;
        }

        private void UpdateObject(FastColumn col, int rowIndex, object newValue)
        {
            if (tabControl.SelectedTab == null)
                return;
            var fastGrid = tabControl.SelectedTab.Controls[0] as FastGrid.FastGrid;
            if (fastGrid == null)
                return;

            // updating real object
            var rowObject = (FastPropertyGridRow) fastGrid.rows[rowIndex].ValueObject;
            var objProp = rowObject.Property;
            if (objProp == null)
                return;
            try
            {
                if(selectedObjects != null)
                    foreach (var chartObject in selectedObjects)
                        objProp.SetValue(chartObject, newValue, null);
                else if(selectedObject != null)
                    objProp.SetValue(selectedObject, newValue, null);
            }
            catch
            {
            }

            // updating grid object
            RebuildSample();
            var valueProp = rowObject.GetType().GetProperty(rowObject.Property(p => p.Value));
            var stringProp = rowObject.GetType().GetProperty(rowObject.Property(p => p.StringValue));
            valueProp.SetValue(rowObject, newValue, null);
            stringProp.SetValue(rowObject, GetStringValue(newValue, rowObject.Property), null);
            fastGrid.UpdateRow(rowIndex, rowObject);
            fastGrid.InvalidateRow(rowIndex);
        }
    }

    class FastPropertyGridRow
    {
        public string Title { get; set; }

        public object Value { get; set; }

        public PropertyInfo Property { get; set; }

        public object StringValue { get; set; }
    }
}
