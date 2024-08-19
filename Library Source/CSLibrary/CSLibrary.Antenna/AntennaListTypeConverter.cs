/*
Copyright (c) 2023 Convergence Systems Limited

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

//#if CS468
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

#if NETCFDESIGNTIME


namespace CSLibrary
{
    using CSLibrary.Constants;
    using CSLibrary.Structures;

    public class AntennaListTypeConverter
        :
        System.ComponentModel.TypeConverter
    {
        /// <summary>
        /// Returns whether this converter can convert an object of the given type to
        ///     the type of this converter, using the specified context.
        /// </summary>
        /// <param name="context">An System.ComponentModel.ITypeDescriptorContext that provides a format context.</param>
        /// <param name="sourceType">A System.Type that represents the type you want to convert from.</param>
        /// <returns>true if this converter can perform the conversion; otherwise, false.</returns>
        public override bool CanConvertFrom
        (
            System.ComponentModel.ITypeDescriptorContext context,
            Type sourceType
        )
        {
            return
                   typeof(string) == sourceType
                || base.CanConvertFrom(context, sourceType);
        }
        /// <summary>
        /// Converts the given object to the type of this converter, using the specified
        ///     context and culture information.
        /// </summary>
        /// <param name="context">An System.ComponentModel.ITypeDescriptorContext that provides a format context.</param>
        /// <param name="culture">The System.Globalization.CultureInfo to use as the current culture.</param>
        /// <param name="value">The System.Object to convert.</param>
        /// <returns>An System.Object that represents the converted value.</returns>
        public override object ConvertFrom
        (
            System.ComponentModel.ITypeDescriptorContext context,
            System.Globalization.CultureInfo culture,
            Object value
        )
        {
            AntennaList antennaList = new AntennaList();

            if (String.IsNullOrEmpty(value as string))
            {
                return antennaList;
            }

            String[] antennaStrings = (value as String).Split(new Char[] { ';' });

            if (null == antennaStrings)
            {
                return antennaList;
            }

            foreach (String s in antennaStrings)
            {
                Object obj = TypeDescriptor.GetConverter(typeof(Antenna)).ConvertFromString(s);

                if (null == obj)
                {
                    // TODO : supply err msg || rely on Source_Antenna converter for msg
                }
                else
                {
                    antennaList.Add(obj as Antenna);
                }
            }

            return antennaList;
        }

        /// <summary>
        /// Converts the given value object to the specified type, using the specified
        ///     context and culture information.
        /// </summary>
        /// <param name="context">An System.ComponentModel.ITypeDescriptorContext that provides a format context.</param>
        /// <param name="culture">A System.Globalization.CultureInfo. If null is passed, the current culture
        /// is assumed.</param>
        /// <param name="value">The System.Object to convert.</param>
        /// <param name="destinationType">The System.Type to convert the value parameter to.</param>
        /// <returns>An System.Object that represents the converted value.</returns>
        public override object ConvertTo
        (
            System.ComponentModel.ITypeDescriptorContext context,
            System.Globalization.CultureInfo culture,
            object value,
            Type destinationType
        )
        {
            if (typeof(string) == destinationType)
            {
                AntennaList antennaList = value as AntennaList;

                if (null == antennaList)
                {
                    throw new ArgumentException("Expected a Source_AntennaList", "value");
                }

                StringBuilder sb = new StringBuilder();

                foreach (Antenna antenna in antennaList)
                {
                    Object obj = TypeDescriptor.GetConverter(typeof(Antenna)).ConvertToString(antenna);

                    if (null == obj)
                    {
                        // Should NOT be possible ~ should get exception for bad arg
                        // before seeing a null == obj return value
                    }
                    else
                    {
                        sb.Append(obj as String);
                        sb.Append(';');
                    }
                }

                return sb.ToString();
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }



    } // END class AntennaListTypeConverter



} // END namespace CSLibrary
#endif
//#endif