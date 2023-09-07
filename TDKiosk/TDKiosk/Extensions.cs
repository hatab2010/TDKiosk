using System;
using System.ComponentModel;
using System.Reflection;

namespace TDKiosk
{
    public static class Extensions
    {
        public static string GetDescription(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field.GetCustomAttribute<DescriptionAttribute>();

            return attribute == null ? value.ToString() : attribute.Description;
        }

        public static int GetIndex(this Enum value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = field.GetCustomAttribute<IndexAttribute>();

            return attribute == null ? throw new Exception() : attribute.Index;
        }

        public static T GetEnumValueFromDescription<T>(string description) where T : Enum
        {
            foreach (var field in typeof(T).GetFields())
            {
                if (Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) is DescriptionAttribute attribute)
                    if (attribute.Description == description)                    
                        return (T)field.GetValue(null);
                    
                
                else if (field.Name == description)                
                    return (T)field.GetValue(null);                
            }

            throw new ArgumentException("Not found.", nameof(description));
            // или возвращать значения по умолчанию
        }
    }
}
