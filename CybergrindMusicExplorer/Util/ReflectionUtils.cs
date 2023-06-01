using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CybergrindMusicExplorer.Util
{
    public class ReflectionUtils
    {
        private static readonly BindingFlags BindingFlags = BindingFlags.Instance | BindingFlags.Public |
                                                            BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;

        private static readonly BindingFlags BindingFlagsFields =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        public static void CloneObsoleteInstance<TNewType, TOldType>(
            TOldType oldInstance,
            TNewType newInstance,
            List<string> privateFieldsToCopy = null,
            List<string> fieldsToIgnore = null)
        {
            var oldInstanceFields = typeof(TOldType).GetFields(BindingFlags);
            var newInstanceFields = typeof(TNewType).GetFields(BindingFlags);

            privateFieldsToCopy?.ForEach(field => ClonePrivateBaseField(field, oldInstance, newInstance));

            foreach (var field in oldInstanceFields)
            {
                if (!newInstanceFields.Select(f => f.Name).Any(n => n == field.Name))
                    continue;

                if (field.IsStatic)
                    continue;
                var newField = newInstanceFields.FirstOrDefault(f => f.Name == field.Name);

                if (fieldsToIgnore != null && fieldsToIgnore.Contains(newField.Name))
                    continue;

                newInstanceFields.First(f => f.Name == field.Name).SetValue(newInstance, field.GetValue(oldInstance));
            }
        }

        public static object GetPrivate<T>(T instance, Type classType, string field)
        {
            FieldInfo privateField = classType.GetField(field, BindingFlagsFields);
            return privateField.GetValue(instance);
        }

        public static void SetPrivate<T, V>(T instance, Type classType, string field, V value)
        {
            FieldInfo privateField = classType.GetField(field, BindingFlagsFields);
            privateField.SetValue(instance, value);
        }

        private static void ClonePrivateBaseField<TNewType, TOldType>(string fieldName, TOldType fromObj,
            TNewType toObj)
        {
            Type source = typeof(TOldType);
            Type derived = typeof(TNewType);

            bool found = false;
            do
            {
                var field1 = source.GetFields(BindingFlagsFields).FirstOrDefault(f => f.Name == fieldName);
                var field2 = derived.GetFields(BindingFlagsFields).FirstOrDefault(f => f.Name == fieldName);
                if (field1 != default && field2 != default)
                {
                    var value = field1.GetValue(fromObj);
                    field2.SetValue(toObj, value);
                    found = true;
                }
                else
                {
                    source = source.BaseType;
                    derived = derived.BaseType;
                }
            } while (!found && source != null && derived != null);
        }
    }
}