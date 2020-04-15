using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk;

namespace ApiPlugin
{
    public static class Extensions
    {
        public static T GetValue<T>(this Entity entity, Entity preImage, string field) where T : struct
        {
            return entity != null && entity.Contains(field)
                ? entity.GetAttributeValue<T>(field)
                : preImage?.GetAttributeValue<T>(field) ?? default;
        }

        public static T GetFieldValue<T>(this Entity entity, Entity preImage, string field) where T : class
        {
            return entity != null && entity.Contains(field)
                ? entity.GetAttributeValue<T>(field)
                : preImage?.GetAttributeValue<T>(field);
        }

        public static T GetExtensibleFieldValue<T>(this Entity entity, Entity preImage, string field) where T : IExtensibleDataObject
        {
            return entity != null && entity.Contains(field)
                ? entity.GetAttributeValue<T>(field)
                : preImage != null ? preImage.GetAttributeValue<T>(field) : default;
        }

        public static T GetFormattedValue<T>(this Entity entity, string field) where T : class
        {
            T formattedValue = entity.FormattedValues.ContainsKey(field)
                ? entity.FormattedValues[field] as T
                : default;

            return formattedValue;
        }

        public static DateTime EOMonth(this DateTime dateTime, int months = 0)
        {
            var firstDayOfTheMonth = new DateTime(dateTime.Year, dateTime.Month, 1);
            return firstDayOfTheMonth.AddMonths(1 + months).AddDays(-1);
        }

        public static bool ContainsAny(this Entity entity, IEnumerable<string> attributes)
        {
            return attributes.Any(attribute => entity.Contains(attribute));
        }
    }
}
