using System;
using System.Collections.Generic;
using System.Text;

namespace CSLibrary
{
    /// <summary>
    /// ArgumentValidationAttribute
    /// </summary>
    public abstract class ArgumentValidationAttribute : Attribute
    {
        public abstract void Validate(object value, string argumentName);
    }
    /// <summary>
    /// NotNullAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class NotNull : ArgumentValidationAttribute
    {
        public override void Validate(object value, string argumentName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(argumentName);
            }
        }
    }
    /// <summary>
    /// InRangeAttribute
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class Range : ArgumentValidationAttribute
    {
        private int min;
        private int max;

        public Range(int min, int max)
        {
            this.min = min;
            this.max = max;
        }

        public override void Validate(object value, string argumentName)
        {
            int intValue = (int)value;
            if (intValue < min || intValue > max)
            {
                throw new ArgumentOutOfRangeException(argumentName, string.Format("min={0}, max={1}", min, max));
            }
        }
    }
}
