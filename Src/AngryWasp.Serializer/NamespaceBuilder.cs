using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using AngryWasp.Helpers;

namespace AngryWasp.Serializer
{
    public class NamespaceBuilder
    {
        public delegate void ProcessCollectionCallback(object obj, Type type, object value, bool preferBinarySerialization);

        private List<Type> allTypes = new List<Type>();
        private Dictionary<string, object> namespaces = new Dictionary<string, object>();
        private Dictionary<Type, Ns> reverseNsLookup = new Dictionary<Type, Ns>();
        //private Serializer serializer;

        public Dictionary<string, object> Namespaces
        {
            get { return namespaces; }
        }

        public Dictionary<Type, Ns> ReverseNamespaceLookup
        {
            get { return reverseNsLookup; }
        }

        //public NamespaceBuilder(Serializer serializer)
        //{
            //this.serializer = serializer;
        //}

        public void Run(object obj)
        {
            allTypes.Add(obj.GetType());
            GetNamespaces(obj);

            int i = 0;
            foreach (Type t in allTypes)
            {
                string name = "ns" + (i++).ToString();
                namespaces.Add(name, t.AssemblyQualifiedName);
                reverseNsLookup.Add(t, new Ns(name, t));
            }
        }

        protected void GetNamespaces(object obj)
        {
            List<FieldInfo> fields;
            List<PropertyInfo> properties;
            Helpers.GetPublicMembers(obj.GetType(), out fields, out properties);

            foreach (FieldInfo f in fields)
            {
                bool preferBinarySerialization = Attribute.IsDefined(f, typeof(SerializeAsBinaryAttribute), true);

                object val = f.GetValue(obj);

                Type valType;

                if (val != null && (valType = val.GetType()) != f.FieldType)
                    GetNamespace(obj, valType, val, preferBinarySerialization);
                else
                    GetNamespace(obj, f.FieldType, val, preferBinarySerialization);
            }

            foreach (PropertyInfo p in properties)
            {
                bool preferBinarySerialization = Attribute.IsDefined(p, typeof(SerializeAsBinaryAttribute), true);

                object val = p.GetValue(obj, null);

                Type valType;
                if (val != null && (valType = val.GetType()) != p.PropertyType)
                    GetNamespace(obj, valType, val, preferBinarySerialization); //this happens if valType is a derived type of p.PropertyType
                else
                    GetNamespace(obj, p.PropertyType, val, preferBinarySerialization);
            }
        }

        protected void GetNamespace(object objectToSerialize, Type type, object value, bool preferBinarySerialization)
        {
            //we do this because the type of value may not be the type 
            //if (value != null)
            //    type = value.GetType();

            Object_Type ot = ReflectionHelper.Instance.GetObjectType(type);

            switch (ot)
            {
                case Object_Type.Struct:
                case Object_Type.Class:
                    {
                        if (!allTypes.Contains(type))
                            allTypes.Add(type);

                        if (!Serializer.HasSerializer(type, preferBinarySerialization))
                            if (value != null)
                                GetNamespaces(value);
                    }
                    break;

                case Object_Type.Primitive:
                    {
                        if (!allTypes.Contains(type))
                            allTypes.Add(type);
                    }
                    break;

                case Object_Type.Enum:
                    {
                        Type t = Enum.GetUnderlyingType(type);
                        if (!allTypes.Contains(t))
                            allTypes.Add(t);
                    }
                    break;

                case Object_Type.List:
                case Object_Type.Dictionary:
                case Object_Type.Array:
                    {
                        if (!allTypes.Contains(type))
                            allTypes.Add(type);

                        if (value != null)
                            ProcessCollection(objectToSerialize, type, value, preferBinarySerialization);
                    }
                    break;

                default: //only time this is known to happen if the property is an interface and is assigned a value of null
                    if (!allTypes.Contains(type))
                        allTypes.Add(type);
                    break;
            }
        }

        private void ProcessCollection(object obj, Type type, object value, bool preferBinarySerialization)
        {
            Object_Type ot = ReflectionHelper.Instance.GetObjectType(type);

            ProcessCollectionCallback callback = new ProcessCollectionCallback(GetNamespace);
            switch (ot)
            {
                case Object_Type.Array:
                case Object_Type.List:
                    {
                        IEnumerator e = ((IList)value).GetEnumerator();
                        while (e.MoveNext())
                            callback(obj, e.Current.GetType(), e.Current, preferBinarySerialization);
                    }
                    break;

                case Object_Type.Dictionary:
                    {
                        IDictionaryEnumerator e = ((IDictionary)value).GetEnumerator();
                        while (e.MoveNext())
                        {
                            callback(obj, e.Key.GetType(), e.Key, preferBinarySerialization);
                            callback(obj, e.Value.GetType(), e.Value, preferBinarySerialization);
                        }
                    }
                    break;

                default:
                    {
                        throw new Exception("Collection type not implemented");
                    }
            }
        }
    }
}