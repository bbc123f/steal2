using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Steal.Attributes
{
    internal static class Reflection
    {
        public static Type GetTypeByName(string typeName, string assembly)
        {
            try
            {
                Assembly ass = Assembly.Load(assembly);
                if (ass != null)
                {
                    return ass.GetType(typeName);
                }
            }
            catch { }
            return null;
        }

        public static MethodInfo GetMethodByName(this Type type, string name)
        {
            try
            {
                return type.GetMethod(name);
            }
            catch { }
            return null;
        }

        public static bool SetValue(this FieldInfo field, object value)
        {
            try
            {
                field.SetValue(value);
                return true;
            }
            catch { return false; }
        }

        public static FieldInfo GetField(this Type type, string fieldName, BindingFlags flags = BindingFlags.Default)
        {
            try
            {
                return type.GetField(fieldName, flags);
            }
            catch { }
            return null;
        }

        public static object Invoke(this Type type, string method, params object[] args)
        {
            try
            {
                return type.GetMethod(method, BindingFlags.Default).Invoke(type, args);
            }
            catch
            {
                return null;
            }
        }

        public static object Invoke(this Type type, string method, BindingFlags flags = BindingFlags.Default, params object[] args)
        {
            try
            {
                return type.GetMethod(method, flags).Invoke(type, args);
            }
            catch
            {
                return null;
            }
        }
    }
}
