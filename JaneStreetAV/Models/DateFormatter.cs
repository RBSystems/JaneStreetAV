using System;

namespace JaneStreetAV.Models
{
    public class DateFormatter : IFormatProvider, ICustomFormatter
    {
        public object GetFormat(Type formatType)
        {
            return formatType == typeof(ICustomFormatter) ? this : null;
        }

        public string Format(string format, object arg, IFormatProvider formatProvider)
        {
            if (!(arg is DateTime)) throw new NotSupportedException();

            return ((DateTime) arg).ToString("dddd, d MMMM, yyyy");
        }
    }
}