using System;

namespace Entity
{
    [Serializable]
    public class CandleChartDroppingObject
    {
        public object value;

        public enum ValueType { Indicator = 0, Script = 1 }

        public ValueType TypeOfValue { get; set; }

        public CandleChartDroppingObject()
        {            
        }

        public CandleChartDroppingObject(object val, ValueType valType)
        {
            value = val;
            TypeOfValue = valType;
        }
    }
}
