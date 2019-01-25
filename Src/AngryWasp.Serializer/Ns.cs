using System;
using AngryWasp.Helpers;

namespace AngryWasp.Serializer
{
    public class Ns
    {
        private string name;
        private Type type;
        private Object_Type objType;

        public string Name => name;

        public Object_Type ObjectType => objType;

        public Type Type
        {
            get { return type; }
            set
            {
                type = value;
                objType = ReflectionHelper.Instance.GetObjectType(type);
            }
        }

        public Ns(string name)
        {
            this.name = name;
        }

        public Ns(string name, Type type)
            : this(name)
        {
            if (type == null)
                throw new Exception("Type " + name + " does not resolve to a type");

            this.type = type;
            this.objType = ReflectionHelper.Instance.GetObjectType(type);
        }

        public Ns(string name, Type type, Object_Type ot)
            : this(name)
        {
            this.type = type;
            this.objType = ot;
        }
    }
}