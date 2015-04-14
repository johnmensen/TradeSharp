using System;

namespace Candlechart.Interface
{
    [AttributeUsage(System.AttributeTargets.Property, AllowMultiple = false)]
    public class ObjectGridBindAttribute : Attribute
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public ObjectGridEditorField editor = ObjectGridEditorField.Default;
        public string[] options;
        public ObjectGridBindAttribute() { }
        public ObjectGridBindAttribute(string name)
        {
            Name = name;
        }
        public ObjectGridBindAttribute(string name, string category)
        {
            Name = name;
            Category = category;
        }
        public ObjectGridBindAttribute(string name, string category, ObjectGridEditorField _editor, params string[] opts)
        {
            Name = name;
            Category = category;
            editor = _editor;
            options = opts;
        }
    }

    class ObjectDataRow
    {
        public string name, category;
        public Type type;
        public object initialValue;
        public ObjectGridEditorField editor = ObjectGridEditorField.Default;
        public string[] options;

        public ObjectDataRow() { }
        public ObjectDataRow(string _name, string _category, Type _type, object _initialValue, ObjectGridEditorField _editor, params string[] opts)
        {
            name = _name;
            category = _category;
            type = _type;
            initialValue = _initialValue;
            editor = _editor;
            options = opts;
        }
    }

    /// <summary>
    /// тип редактора для свойства объекта
    /// </summary>
    public enum ObjectGridEditorField
    {
        Default, SingleLineText, MultylineText, DateEdit, DateTimeEdit, ComboboxEdit
    }
}
