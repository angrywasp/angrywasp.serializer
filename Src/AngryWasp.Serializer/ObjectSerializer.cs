using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using AngryWasp.Logger;
using AngryWasp.Helpers;

namespace AngryWasp.Serializer
{
	public enum Deserialize_Result
    {
        Unknown = 0,
        OK = 1,
        WrongType = 2,
        Error = 3
    }

    /// <summary>
    /// The object serialiazer class is used to serialize/deserialize a class instance
    /// </summary>
    public class ObjectSerializer : SerializerCommon
    {
        #region Serialize

        //todo: consider saving all files to content directory and accept fileName as a relative path to content directory
        //rather than having to specify an absolute file path
        public void Serialize(object obj, string fileName)
        {
            XDocument document = Serialize(obj);
            XHelper.Save(document, fileName);
        }

        public XDocument Serialize(object obj)
        {
			NamespaceBuilder nb = new NamespaceBuilder();
            nb.Run(obj);
            reverseNsLookup = nb.ReverseNamespaceLookup;

            XDocument document = XHelper.CreateDocument("Content", nb.Namespaces,  null, new string[]
            {
                "Created by Angry Wasp's XML Serializer",
                "Copyright 2015 Angry Wasp",
                "https://bitbucket.org/angrywasp/angrywasp.serializer"
            });
            assetElement = XHelper.CreateElement(document.Root, reverseNsLookup[obj.GetType()].Name, "Asset");
            sharedElement = XHelper.CreateElement(document.Root, null, "Shared");

            //start the recursive serialization process
            GetMembers(obj, assetElement);

            return document;
        }

        #endregion

        #region Deserialize

        public T Deserialize<T>(XDocument document)
        {
            T result = (T)Activator.CreateInstance(typeof(T));

            Deserialize<T>(document, ref result);

            return result;
        }

        public Deserialize_Result Deserialize<T>(XDocument document, ref T instance)
        {
            try
            {
                //get the root nodes
                assetElement = XHelper.GetNodeByName(document.Root, "Asset");
                sharedElement = XHelper.GetNodeByName(document.Root, "Shared");

                //build list of namespaces
                List<XAttribute> xmlNamespaces = document.Root.Attributes().ToList();
                deserializeNamespaceLookup = new Dictionary<string, Ns>();

                foreach (XAttribute a in xmlNamespaces)
                    deserializeNamespaceLookup.Add(a.Name.LocalName, new Ns(a.Name.LocalName, GetType(a.Value)));

                //check if the file we are trying to deserialize is ther same as the type we are deserializing to
                Type documentType = deserializeNamespaceLookup[assetElement.GetPrefixOfNamespace(assetElement.Name.Namespace)].Type;

                if (documentType != typeof(T))
                    return Deserialize_Result.WrongType;

                SetSharedObjects();
                SetMembers(instance, assetElement);
            }
            catch (Exception ex)
            {
                Log.Instance.Write(Log_Severity.Error, $"{ex.Message}\r\n\t{ex.StackTrace}");
                return Deserialize_Result.Error;
            }

            return Deserialize_Result.OK;
        }

        public Deserialize_Result Deserialize(XDocument document, ref object instance)
        {
            try
            {
                //get the root nodes
                assetElement = XHelper.GetNodeByName(document.Root, "Asset");
                sharedElement = XHelper.GetNodeByName(document.Root, "Shared");

                //build list of namespaces
                List<XAttribute> xmlNamespaces = document.Root.Attributes().ToList();
                deserializeNamespaceLookup = new Dictionary<string, Ns>();

                foreach (XAttribute a in xmlNamespaces)
                    deserializeNamespaceLookup.Add(a.Name.LocalName, new Ns(a.Name.LocalName, GetType(a.Value)));

                //check if the file we are trying to deserialize is ther same as the type we are deserializing to
                Type documentType = deserializeNamespaceLookup[assetElement.GetPrefixOfNamespace(assetElement.Name.Namespace)].Type;

                if (documentType != instance.GetType())
                    return Deserialize_Result.WrongType;

                SetSharedObjects();
                SetMembers(instance, assetElement);
            }
            catch (Exception ex)
            {
                Log.Instance.Write(Log_Severity.Error, $"{ex.Message}\r\n\t{ex.StackTrace}");
                return Deserialize_Result.Error;
            }

            return Deserialize_Result.OK;
        }

        #endregion

        public Type GetType(string assemblyQualifiedName)
        {
            //first check the types that are loaded with this assembly
            Type t = Type.GetType(assemblyQualifiedName);

            if (t != null)
                return t;

            //if that fails, we look in the assembly type cache
            if (ReflectionHelper.Instance.AssemblyTypeCache.TryGetValue(assemblyQualifiedName, out t))
                return t;

            //failed. we do not know what this type is
            Log.Instance.Write(Log_Severity.Error, $"Failed to file type '{assemblyQualifiedName}' in assembly cache");
            return null;
        }

        public Type GetFileType(string file)
        {
            XDocument document = XHelper.LoadDocument(file);
            XElement n = XHelper.GetNodeByName(document.Root, "Asset");
            string p = n.GetPrefixOfNamespace(n.Name.Namespace);
            List<XAttribute> xn = document.Root.Attributes().ToList();

            foreach (var x in xn)
            {
                if (p == x.Name.LocalName)
                {
                    string typeString = x.Value;
                    Type t = GetType(typeString);

                    if (t == null)
                        return null; //we matched up the type to the namespace but couldn't find the type, so return null

                    return t;
                }
            }

            return null;
        }
    }
}