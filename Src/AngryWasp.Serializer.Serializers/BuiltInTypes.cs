using System;
using System.IO;
using System.Reflection;

namespace AngryWasp.Serializer.Serializers
{
    public class BoolSerializer : ISerializer<bool>
    {
        public string Serialize(bool value) => value.ToString();

        public bool Deserialize(string value) => bool.Parse(value);
    }

    public class ByteSerializer : ISerializer<byte>
    {
        public string Serialize(byte value) => value.ToString();

        public byte Deserialize(string value) => byte.Parse(value);
    }

    public class SbyteSerializer : ISerializer<sbyte>
    {
        public string Serialize(sbyte value) => value.ToString();

        public sbyte Deserialize(string value) => sbyte.Parse(value);
    }

    public class CharSerializer : ISerializer<char>
    {
        public string Serialize(char value) => value.ToString();

        public char Deserialize(string value) => char.Parse(value);
    }

    public class UshortSerializer : ISerializer<ushort>
    {
        public string Serialize(ushort value) => value.ToString();

        public ushort Deserialize(string value) => ushort.Parse(value);
    }

    public class ShortSerializer : ISerializer<short>
    {
        public string Serialize(short value) => value.ToString();

        public short Deserialize(string value) => short.Parse(value);
    }

    public class UintSerializer : ISerializer<uint>
    {
        public string Serialize(uint value) => value.ToString();

        public uint Deserialize(string value) => uint.Parse(value);
    }

    public class IntSerializer : ISerializer<int>
    {
        public string Serialize(int value) => value.ToString();

        public int Deserialize(string value) => int.Parse(value);
    }

    public class UlongSerializer : ISerializer<ulong>
    {
        public string Serialize(ulong value) => value.ToString();

        public ulong Deserialize(string value) => ulong.Parse(value)
    }

    public class LongSerializer : ISerializer<long>
    {
        public string Serialize(long value) => value.ToString();

        public long Deserialize(string value) => long.Parse(value);
    }

    public class FloatSerializer : ISerializer<float>
    {
        public string Serialize(float value) => value.ToString("0.0######");

        public float Deserialize(string value) => float.Parse(value);
    }

    public class DoubleSerializer : ISerializer<double>
    {
        public string Serialize(double value) => value.ToString("0.0###############");

        public double Deserialize(string value) => double.Parse(value);
    }

    public class DecimalSerializer : ISerializer<decimal>
    {
        public string Serialize(decimal value) => value.ToString("0.0#############################");

        public decimal Deserialize(string value) => decimal.Parse(value);
    }

    public class StringSerialzier : ISerializer<string>
    {
        public string Serialize(string value) => value;

        public string Deserialize(string value) => value;
    }

    public class TimeSpanSerializer : ISerializer<TimeSpan>
    {
        public string Serialize(TimeSpan value) => value.ToString();

        public TimeSpan Deserialize(string value) => TimeSpan.Parse(value);
    }

    public class DateTimeSerializer : ISerializer<DateTime>
    {
        public string Serialize(DateTime value) => value.ToString();

        public DateTime Deserialize(string value) => DateTime.Parse(value);
    }

    public class TypeSerializer : ISerializer<Type>
    {
        public string Serialize(Type value)
        {
            string path = Path.GetFileName(value.Assembly.Location);
            return value.FullName + " " + path;
        }

        public Type Deserialize(string value)
        {
            int split = value.IndexOf(' ', 0) + 1;
            string type = value.Substring(0, split).Trim();
            string path = value.Substring(split).Trim();

            Assembly a = Assembly.LoadFile(path);
            Type retVal = a.GetType(type);

            return retVal;
        }
    }
}