using System;

namespace TradeSharp.Robot.BL
{
    public class RuntimeAccessAttribute : Attribute
    {
        private bool browsable;
        
        public RuntimeAccessAttribute(bool browsable)
        {
            this.browsable = browsable;
        }
    }
}
