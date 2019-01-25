using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Xml.Linq;
using AngryWasp.Logger;
using AngryWasp.Helpers;

namespace AngryWasp.Serializer
{
    public delegate void SerializeCollectionCallback(object obj,XElement parent,object value,bool preferBinarySerialization);

    public delegate object DeserializeCollectionCallback(XElement e,bool isBinarySerialized);

    public class SerializerCommon
    {
        protected XElement assetElement;
        protected XElement sharedElement;
        protected Dictionary<string, Ns> deserializeNamespaceLookup;
        protected Dictionary<Type, Ns> reverseNsLookup;

        private Ns Lookup(Type type)
        {
            Object_Type ot = ReflectionHelper.Instance.GetObjectType(type);

            if (ot == Object_Type.Enum)
            {
                Type t = Enum.GetUnderlyingType(type);
                Ns ns = reverseNsLookup[t];
                return new Ns(ns.Name, t, Object_Type.Enum);
            }

            return reverseNsLookup[type];
        }

        #region Deserialize

        private Dictionary<string, object> sharedObjects = new Dictionary<string, object>();
        private List<string> sharedObjects_DeferredDeserialization = new List<string>();

        protected void SetSharedObjects()
        {
            if (sharedElement == null)
                return;

            foreach (XElement e in sharedElement.Elements())
            {
                Ns ns = deserializeNamespaceLookup[e.GetPrefixOfNamespace(e.Name.Namespace)];
                object obj = null;
                bool bs = e.FirstNode.NodeType == System.Xml.XmlNodeType.CDATA;

                if (bs)
                {
                    if (Serializer.HasBinarySerializer(ns.Type))
                    {
                        XCData cdataNode = e.FirstNode as XCData;
                        int length = int.Parse(e.Attribute("Length").Value);
                        byte[] bytes = Convert.FromBase64String(cdataNode.Value);
                        obj = Serializer.BinaryDeserialize(ns.Type, new BinarySerializerData(bytes, length));
                    }
                }
                else
                {
                    if (Serializer.HasXmlSerializer(ns.Type))
                    {
                        XAttribute valA = e.Attribute("Value");
                        obj = Serializer.Deserialize(ns.Type, valA.Value);
                    }
                    else
                    {
                        obj = Activator.CreateInstance(ns.Type);
                        sharedObjects_DeferredDeserialization.Add(e.Name.LocalName);
                    }
                }

                if (obj == null)
                    throw new Exception("Could not resolve object type " + ns.Type.Name);

                sharedObjects.Add(e.Name.LocalName, obj);
            }

            foreach (XElement e in sharedElement.Elements())
            {
                if (sharedObjects_DeferredDeserialization.Contains(e.Name.LocalName))
                {
                    object obj = sharedObjects[e.Name.LocalName];
                    SetMembers(obj, e);
                }
            }
        }

        protected void SetMembers(object obj, XElement e)
        {
            List<XElement> elements = e.Elements().ToList();

            XElement fieldsElement = elements[0];
            XElement propertiesElement = elements[1];

            Dictionary<string, FieldInfo> fields = ReflectionHelper.Instance.GetFields(obj.GetType(), Field_Access_Mode.Private | Field_Access_Mode.Public);
            Dictionary<string, PropertyInfo> properties = ReflectionHelper.Instance.GetProperties(obj.GetType(), Property_Access_Mode.Read | Property_Access_Mode.Write);

            foreach (XElement el in fieldsElement.Elements())
            {
                FieldInfo fi;
                if (!fields.TryGetValue(el.Name.LocalName, out fi))
                    continue;

                if (fi == null)
                    continue;

                bool isBinarySerialized = Attribute.IsDefined(fi, typeof(SerializeAsBinaryAttribute), true);

                fi.SetValue(obj, SetMember(el, isBinarySerialized));
            }

            foreach (XElement el in propertiesElement.Elements())
            {
                PropertyInfo pi = null;
                
                if (!properties.TryGetValue(el.Name.LocalName, out pi))
                    continue; //property does not exist for the serialized item. can ignore this

                if (pi == null) //this probably can't happen, otherwise we would have already continued
                    continue;

                bool isBinarySerialized = Attribute.IsDefined(pi, typeof(SerializeAsBinaryAttribute), true);
                pi.SetValue(obj, SetMember(el, isBinarySerialized), null);
            }
        }

        protected object SetMember(XElement e, bool isBinarySerialized)
        {
            Ns ns = deserializeNamespaceLookup[e.GetPrefixOfNamespace(e.Name.Namespace)];
            Object_Type ot = ns.ObjectType;

            switch (ot)
            {
                case Object_Type.Struct:
                case Object_Type.Class:
                    {
                        bool isShared = Attribute.IsDefined(ns.Type, typeof(SerializeAsSharedObject), true);

                        if (isShared)
                        {
                            string val = e.Attribute("ID").Value;

                            if (val == "null")
                                return null;
                            else
                                return sharedObjects[val];
                        }

                        if (!Serializer.HasSerializer(ns.Type, isBinarySerialized))
                        {
                            //In this case the serialized object had a value of null, so we just return null
                            if (e.Attribute("Value") != null && e.Attribute("Value").Value == "null")
                                return null;
                            else
                            {
                                object instance = Activator.CreateInstance(ns.Type);
                                SetMembers(instance, e);
                                return instance;
                            }
                        }
                        else
                        {
                            if (isBinarySerialized)
                            {
                                XCData cdataNode = e.FirstNode as XCData;
                                int length = int.Parse(e.Attribute("Length").Value);
                                byte[] bytes = Convert.FromBase64String(cdataNode.Value);

                                return Serializer.BinaryDeserialize(ns.Type, new BinarySerializerData(bytes, length));
                            }
                            else
                            {
                                if (e.Attribute("Value") == null)
                                {
                                    Log.Instance.Write(Log_Severity.Warning, $"{ns.Type.Name} does not appear to be using the built in serializer. Save to update file output");
                                    object instance = Activator.CreateInstance(ns.Type);
                                    SetMembers(instance, e);
                                    return instance;
                                }

                                return Serializer.Deserialize(ns.Type, e.Attribute("Value").Value);
                            }
                        }
                    }
                case Object_Type.Primitive:
                    {
                        return Serializer.Deserialize(ns.Type, e.Attribute("Value").Value);
                    }
                case Object_Type.Enum:
                    {
                        return Enum.ToObject(ns.Type, Serializer.Deserialize(typeof(int), e.Attribute("Value").Value));
                    }
                case Object_Type.List:
                case Object_Type.Dictionary:
                case Object_Type.Array:
                    {
                        if (Serializer.HasSerializer(ns.Type, isBinarySerialized))
                        {
                            if (isBinarySerialized)
                            {
                                XCData cdataNode = e.FirstNode as XCData;
                                int length = int.Parse(e.Attribute("Length").Value);
                                byte[] bytes = Convert.FromBase64String(cdataNode.Value);

                                return Serializer.BinaryDeserialize(ns.Type, new BinarySerializerData(bytes, length));
                            }
                            else
                                return Serializer.Deserialize(ns.Type, e.Attribute("Value").Value);
                        }
                        else
                            return DeserializeCollection(ns, e, isBinarySerialized);
                    }
                default:
                    return null;
            }
        }

        private object DeserializeCollection(Ns ns, XElement e, bool isBinarySerialized)
        {
            //todo: handle null values for dictrionary and array the same way they are handled for list
            DeserializeCollectionCallback callback = new DeserializeCollectionCallback(DeserializeListCallback);
            switch (ns.ObjectType)
            {
                case Object_Type.List:
                    {
                        XElement valuesElement = e.Element("Values");
                        if (valuesElement == null)
                        {
                            XAttribute valueAttribute = e.Attribute("Value");

                            if (valueAttribute != null)
                            {
                                if (valueAttribute.Value.ToLower() == "null")
                                    return null;
                                else
                                {
                                    throw new Exception("Malformed document");
                                }
                            }
                            else
                            {
                                //no values element to define values for the collection and no value element for a serialzier or null value. bad
                                throw new Exception("Collection Values element missing, Malformed document");
                            }
                        }
                        else
                        {
                            List<XElement> values = valuesElement.Elements().ToList();
                            IList il = Activator.CreateInstance(ns.Type) as IList;

                            foreach (XElement item in values)
                                il.Add(callback(item, isBinarySerialized));

                            return il;
                        }
                    }
                case Object_Type.Dictionary:
                    {
                        List<XElement> keys = e.Element("Keys").Elements().ToList();
                        List<XElement> values = e.Element("Values").Elements().ToList();
                        IDictionary id = Activator.CreateInstance(ns.Type) as IDictionary;

                        if (keys.Count != values.Count)
                        {
                            throw new Exception("Dictionary.Keys.Count != Dictionary.Values.Count, Malformed document");
                        }

                        for (int i = 0; i < keys.Count; i++)
                        {
                            object key = callback(keys[i], isBinarySerialized);
                            object value = callback(values[i], isBinarySerialized);

                            id.Add(key, value);
                        }

                        return id;
                    }
                case Object_Type.Array:
                    {
                        int dimensions = ns.Type.GetArrayRank();

                        string[] dimensionLengthStrings = e.Attribute("Dimensions").Value.Split(new char[] { ';' });
                        int[] dimensionLengths = new int[dimensionLengthStrings.Length];

                        for (int i = 0; i < dimensions; i++)
                            dimensionLengths[i] = int.Parse(dimensionLengthStrings[i]);

                        Type itemType = ns.Type.GetElementType();
                        List<XElement> values = e.Element("Values").Elements().ToList();
                        Array a = Array.CreateInstance(itemType, dimensionLengths);

                        for (int i = 0; i < a.Length; i++)
                            a.SetValue(callback(values[i], isBinarySerialized), Helpers.GetArrayIndices(dimensions, dimensionLengths, i));

                        return a;
                    }
            }

            return null;
        }

        private object DeserializeListCallback(XElement e, bool isBinarySerialized) => SetMember(e, isBinarySerialized);

        #endregion

        #region Serialize

        protected void GetMembers(object obj, XElement e)
        {
            List<FieldInfo> fields;
            List<PropertyInfo> properties;
            Helpers.GetPublicMembers(obj.GetType(), out fields, out properties);

            XElement fe = XHelper.CreateElement(e, null, "Fields");
            XElement pe = XHelper.CreateElement(e, null, "Properties");

            List<MemberInfo> members = new List<MemberInfo>();

            foreach (MemberInfo m in fields)
                members.Add(m);

            foreach (MemberInfo m in properties)
                members.Add(m);

            //create a list for each priority level
            List<List<MemberInfo>> priorityLevels = new List<List<MemberInfo>>();
            for (int i = 0; i <= (int)Serializer_Priority.Highest; i++)
                priorityLevels.Add(new List<MemberInfo>());

            //sort the members into their respective priority level lists
            foreach (MemberInfo m in members)
            {
                object[] a = m.GetCustomAttributes(typeof(SerializerPriorityAttribute), false);
                if (a.Length == 0)
                    priorityLevels[(int)Serializer_Priority.None].Add(m);
                else
                    priorityLevels[(int)((SerializerPriorityAttribute)a[0]).Priority].Add(m);
            }

            //clear the original members list and repopulate based on sorted prioritised members
            //we decrement in this loop cause the highest priority is the last in the list, but should be the first
            //to be processed. another way would be to reverse priorityLevels and use an incrementing loop
            members.Clear();

            for (int i = (int)Serializer_Priority.Highest; i >= 0; i--)
                foreach (MemberInfo m in priorityLevels[i])
                    members.Add(m);

            //now we process the final list of prioritised members
            foreach (MemberInfo m in members)
            {
                Type memberType = null;
                object value = null;
                XElement xe = null;

                bool preferBinarySerialization = Attribute.IsDefined(m, typeof(SerializeAsBinaryAttribute), true);

                { //Field
                    FieldInfo i = m as FieldInfo;

                    if (i != null)
                    {
                        memberType = i.FieldType;
                        value = i.GetValue(obj);
                        xe = fe;
                    }
                }

                { //Property
                    PropertyInfo i = m as PropertyInfo;

                    if (i != null)
                    {
                        memberType = i.PropertyType;
                        value = i.GetValue(obj, null);
                        xe = pe;
                    }
                }

                GetMember(obj, m.Name, memberType, value, xe, preferBinarySerialization);
            }
        }

        private List<string> checksums = new List<string>();

        protected void GetMember(object objectToSerialize, string name, Type type, object value, XElement e, bool preferBinarySerialization)
        {
            //this happens when the type of the member is different to the type of the value assigned to it
            //for example a member might be declared as a base class or interface and have a derived class assigned to it
            Type valType;
            if (value != null && (valType = value.GetType()) != type)
                type = valType;

            Ns ns = Lookup(type);

            Object_Type ot = ns.ObjectType;

            switch (ot)
            {
                case Object_Type.Struct:
                case Object_Type.Class:
                    {
                        bool isShared = Attribute.IsDefined(ns.Type, typeof(SerializeAsSharedObject), true);
                        if (isShared)
                        {
                            //first thing we do is create an element that links this value to the shared object
                            XElement assetObjectElement = XHelper.CreateElement(e, ns.Name, name);

                            if (value == null)
                            {
                                XHelper.CreateAttribute(assetObjectElement, "ID", "null");
                                return;
                            }

                            byte[] hash = MD5.Create().ComputeHash(BitShifter.ToByte(value.GetHashCode()));
                            //prefix with an X to make sure first character is a letter in accordance with XML standard
                            string s = "X" + string.Concat(hash.Select(x => x.ToString("X2")));

                            XHelper.CreateAttribute(assetObjectElement, "ID", s);

                            //then we actually create the shared object if it has not already been created
                            if (!checksums.Contains(s))
                            {
                                checksums.Add(s);

                                XElement sharedObjectElement = XHelper.CreateElement(sharedElement, ns.Name, s);

                                if (!Serializer.HasSerializer(type, preferBinarySerialization))
                                {
                                    if (value != null)
                                        GetMembers(value, sharedObjectElement);
                                    else
                                        CreateValue(sharedObjectElement, type, value, false);
                                }
                                else
                                    CreateValue(sharedObjectElement, type, value, preferBinarySerialization);
                            }
                        }
                        else
                        {
                            //if this class has a serializer, use it, otherwise we recurse through the objects members and serialize the items individually
                            if (!Serializer.HasSerializer(type, preferBinarySerialization))
                            {
                                if (value != null)
                                    GetMembers(value, XHelper.CreateElement(e, ns.Name, name));
                                else
                                    CreateValue(XHelper.CreateElement(e, ns.Name, name), type, value, false);
                            }
                            else
                                CreateValue(XHelper.CreateElement(e, ns.Name, name), type, value, preferBinarySerialization);
                        }
                    }
                    break;

                case Object_Type.Primitive:
                    {
                        //any types that are Object_Type.Primitive have a built in serializer. so we know they can always be serialized
                        CreateValue(XHelper.CreateElement(e, ns.Name, name), type, value, preferBinarySerialization);
                    }
                    break;

                case Object_Type.Enum:
                    {
                        //any types that are Object_Type.Enum use the built in IntSerializer. so we know they can always be serialized
                        CreateValue(XHelper.CreateElement(e, ns.Name, name), ns.Type, value, preferBinarySerialization);
                    }
                    break;

                case Object_Type.List:
                case Object_Type.Dictionary:
                case Object_Type.Array:
                    {
                        //for collections. we need to iterate through the items and serialize each one
                        //but we need to set this up recursively, in case the items in a collection are themselves collections
                        if (value != null)
                        {
                            if (!Serializer.HasSerializer(type, preferBinarySerialization))
                                SerializeCollection(objectToSerialize, e, name, type, value, preferBinarySerialization);
                            else
                                CreateValue(XHelper.CreateElement(e, ns.Name, name), type, value, preferBinarySerialization);
                        }
                        else
                            CreateValue(XHelper.CreateElement(e, ns.Name, name), type, value, preferBinarySerialization);
                    }
                    break;

                default:
                    CreateValue(XHelper.CreateElement(e, ns.Name, name), type, null, preferBinarySerialization); //if we can't determine the object type, just serialize it as null
                    break;
            }
        }

        private void SerializeCollection(object obj, XElement parent, string name, Type type, object value, bool preferBinarySerialization)
        {
            Ns ns = Lookup(type);
            Object_Type ot = ns.ObjectType;

            XElement collectionElement = null;

            //first thing to do is create the base element that will hold the collection items
            switch (ot)
            {
                case Object_Type.List:
                case Object_Type.Dictionary:
                    {
                        collectionElement = XHelper.CreateElement(parent, ns.Name, name);
                    }
                    break;

                case Object_Type.Array:
                    {
                        int dimensions = type.GetArrayRank();
                        Array a = (Array)value;
                        string dimensionsLengthString = string.Empty;

                        for (int i = 0; i < dimensions; i++)
                            dimensionsLengthString += (a.GetLength(i).ToString() + ";");

                        dimensionsLengthString = dimensionsLengthString.TrimEnd(new char[] { ';' });
                        collectionElement = XHelper.CreateElement(parent, ns.Name, name, null, new Dictionary<string, object>() { { "Dimensions", dimensionsLengthString } });
                    }
                    break;

                default:
                    {
                        throw new Exception("Collection type not implemented");
                    }
            }

            SerializeCollectionCallback callback = new SerializeCollectionCallback(SerializeListCallback);
            //now iterate through the items and call a callback on each one
            switch (ot)
            {
                case Object_Type.Array:
                case Object_Type.List:
                    {
                        XElement valuesElement = XHelper.CreateElement(collectionElement, null, "Values");
                        IEnumerator e = ((IList)value).GetEnumerator();
                        while (e.MoveNext())
                            callback(obj, valuesElement, e.Current, preferBinarySerialization);
                    }
                    break;

                case Object_Type.Dictionary:
                    {
                        XElement keysElement = XHelper.CreateElement(collectionElement, null, "Keys");
                        XElement valuesElement = XHelper.CreateElement(collectionElement, null, "Values");
                        IDictionaryEnumerator e = ((IDictionary)value).GetEnumerator();
                        while (e.MoveNext())
                        {
                            callback(obj, keysElement, e.Key, preferBinarySerialization);
                            callback(obj, valuesElement, e.Value, preferBinarySerialization);
                        }
                    }
                    break;

                default:
                    {
                        throw new Exception("Collection type not implemented");
                    }
            }
        }

        private void SerializeListCallback(object obj, XElement parent, object value, bool preferBinarySerialization) =>
            GetMember(obj, "Item", value.GetType(), value, parent, preferBinarySerialization);

        private void CreateValue(XElement element, Type type, object value, bool preferBinarySerialization)
        {
            if (preferBinarySerialization)
            {
                BinarySerializerData serializedData = Serializer.BinarySerialize(type, value);
                XHelper.CreateAttribute(element, "Length", serializedData.Length.ToString());
                element.Add(new XCData(Convert.ToBase64String(serializedData.Data)));
            }
            else
            { //if not using binary serialization, we just use the regular serializer
                if (value != null)
                    XHelper.CreateAttribute(element, "Value", Serializer.Serialize(type, value));
                else
                    XHelper.CreateAttribute(element, "Value", "null");
            }
        }

        #endregion
    }
}