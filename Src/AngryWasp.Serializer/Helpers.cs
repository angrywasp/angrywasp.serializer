using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;

namespace AngryWasp.Serializer
{
    public static class Helpers
    {
        /// <summary>
        /// converts a linear array index (i) into a multidimensional array location
        /// </summary>
        /// <param name="dimensions"></param>
        /// <param name="lengths"></param>
        /// <param name="i"></param>
        /// <returns></returns>
        public static int[] GetArrayIndices(int dimensions, int[] lengths, int i)
        {
            int[] indices = new int[dimensions];
            for (int d = dimensions - 1; d >= 0; d--)
            {
                int l = lengths[d];
                indices[d] = i % l;
                i /= l;
            }
            return indices;
        }

        public static void GetPublicMembers(Type objType, out List<FieldInfo> fields, out List<PropertyInfo> properties)
        {
            Dictionary<string, FieldInfo> fd = new Dictionary<string, FieldInfo>();
            Dictionary<string, PropertyInfo> pd = new Dictionary<string, PropertyInfo>();

            GetFields(objType, fd);
            GetProperties(objType, pd);

            fields = new List<FieldInfo>(fd.Values);
            properties = new List<PropertyInfo>(pd.Values);
        }

        private static void GetFields(Type type, Dictionary<string, FieldInfo> fd)
        {
            GetFieldsInternal(type, fd);

            Type baseType = type.BaseType;

            while (baseType != null && baseType != typeof(object))
            {
                GetFieldsInternal(baseType, fd);
                baseType = baseType.BaseType;
            }
        }

        private static void GetFieldsInternal(Type type, Dictionary<string, FieldInfo> fd)
        {
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (FieldInfo field in fields)
            {
                if (fd.ContainsKey(field.Name))
                    continue;

                if (field.IsPublic)
                {
                    if (!Attribute.IsDefined(field, typeof(SerializerExcludeAttribute), true))
                        fd.Add(field.Name, field);
                }
                else
                {
                    if (Attribute.IsDefined(field, typeof(SerializerIncludeAttribute), true))
                        fd.Add(field.Name, field);
                }
            }
        }

        private static void GetProperties(Type type, Dictionary<string, PropertyInfo> pd)
        {
            GetPropertiesInternal(type, pd);
            Type baseType = type.BaseType;

            while (baseType != null && baseType != typeof(object))
            {
                GetPropertiesInternal(baseType, pd);
                baseType = baseType.BaseType;
            }
        }

        private static void GetPropertiesInternal(Type type, Dictionary<string, PropertyInfo> pd)
        {
            PropertyInfo[] properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            foreach (PropertyInfo property in properties)
            {
                if (pd.ContainsKey(property.Name))
                    continue;

                if (property.CanRead && property.CanWrite)
                    if (!Attribute.IsDefined(property, typeof(SerializerExcludeAttribute), true))
                        pd.Add(property.Name, property);
            }
        }
    }
}