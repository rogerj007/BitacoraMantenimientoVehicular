using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BitacoraMantenimientoVehicular.Web.Helpers
{
    public static class EnumExtensions
    {
        public static string GetDisplayName(this Enum enu)
        {
            var attr = GetDisplayAttribute(enu);
            return attr != null ? attr.Name : enu.ToString();
        }
        public static string GetDescription(this Enum enu)
        {
            var attr = GetDisplayAttribute(enu);
            return attr != null ? attr.Description : enu.ToString();
        }
        private static DisplayAttribute GetDisplayAttribute(object value)
        {
            var type = value.GetType();
            if (!type.IsEnum)
            {
                throw new ArgumentException($"Type {type} is not an enum");
            }
            // Get the enum field.  
            var field = type.GetField(value.ToString() ?? string.Empty);
            return field == null ? null : field.GetCustomAttribute<DisplayAttribute>();
        }
    }
}
