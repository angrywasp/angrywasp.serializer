using System;

namespace AngryWasp.Serializer
{
    public interface ISerializer<T>
    {
        string Serialize(T value);

        T Deserialize(string value);
    }

    public interface IBinarySerializer<T>
    {
        BinarySerializerData Serialize(T value);

        T Deserialize(BinarySerializerData data);
    }

    public struct BinarySerializerData
    {
        public byte[] Data;
        public int Length;

        public BinarySerializerData(byte[] d, int l)
        {
            Data = d;
            Length = l;
        }
    }

    /// <summary>
    /// Public members (fields/properties) are serialized by default.
    /// This attribute forces the serializer to skip these members
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class SerializerExcludeAttribute : Attribute
    {
    }

    /// <summary>
    /// Private members are skipped by default.
    /// This attribute forces the serializer to include these members
    /// Note that it is not valid to specify this attribute on properties with only a get or set accessor
    /// In such cases, this attribute will be ignored. Instead assign it to the property backing field
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class SerializerIncludeAttribute : Attribute
    {
    }

    /// <summary>
    /// This attribute tells the serializer to save the particular member as binary data
    /// Any member using this attribute MUST implement IBinarySerializer for the type you wish to serialize
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class SerializeAsBinaryAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class SerializeAsSharedObject : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public sealed class SerializerPriorityAttribute : Attribute
    {
        private Serializer_Priority priority;

        public Serializer_Priority Priority
        {
            get { return priority; }
        }

        public SerializerPriorityAttribute(Serializer_Priority priority)
        {
            this.priority = priority;
        }
    }

    public enum Serializer_Priority
    {
        None,
        Lowest,
        VeryLow,
        Low,
        Medium,
        High,
        VeryHigh,
        Highest
    }
}