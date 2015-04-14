using System;

namespace FastGrid
{
    [AttributeUsage(AttributeTargets.Property)]
    public class ValueListAttribute : Attribute
    {
        private bool isEditable;
        public bool IsEditable
        {
            get { return isEditable; }
        }

        private object[] values;
        public object[] Values
        {
            get { return values; }
        }

        public ValueListAttribute(bool isEditable, params object[] values)
        {
            this.isEditable = isEditable;
            this.values = values;
        }
    }
}
