using System;
using System.IO;
using System.Reflection;

namespace AngryWasp.Serializer.Serializers
{
    public class BoolSerializer : ISerializer<bool>
    {
        public string Serialize(bool value)
        {
            return value.ToString();
        }

        public bool Deserialize(string value)
        {
            return bool.Parse(value);
        }
    }

    public class ByteSerializer : ISerializer<byte>
    {
        public string Serialize(byte value)
        {
            return value.ToString();
        }

        public byte Deserialize(string value)
        {
            return byte.Parse(value);
        }
    }

    public class SbyteSerializer : ISerializer<sbyte>
    {
        public string Serialize(sbyte value)
        {
            return value.ToString();
        }

        public sbyte Deserialize(string value)
        {
            return sbyte.Parse(value);
        }
    }

    public class CharSerializer : ISerializer<char>
    {
        public string Serialize(char value)
        {
            return value.ToString();
        }

        public char Deserialize(string value)
        {
            return char.Parse(value);
        }
    }

    public class UshortSerializer : ISerializer<ushort>
    {
        public string Serialize(ushort value)
        {
            return value.ToString();
        }

        public ushort Deserialize(string value)
        {
            return ushort.Parse(value);
        }
    }

    public class ShortSerializer : ISerializer<short>
    {
        public string Serialize(short value)
        {
            return value.ToString();
        }

        public short Deserialize(string value)
        {
            return short.Parse(value);
        }
    }

    public class UintSerializer : ISerializer<uint>
    {
        public string Serialize(uint value)
        {
            return value.ToString();
        }

        public uint Deserialize(string value)
        {
            return uint.Parse(value);
        }
    }

    public class IntSerializer : ISerializer<int>
    {
        public string Serialize(int value)
        {
            return value.ToString();
        }

        public int Deserialize(string value)
        {
            return int.Parse(value);
        }
    }

    public class UlongSerializer : ISerializer<ulong>
    {
        public string Serialize(ulong value)
        {
            return value.ToString();
        }

        public ulong Deserialize(string value)
        {
            return ulong.Parse(value);
        }
    }

    public class LongSerializer : ISerializer<long>
    {
        public string Serialize(long value)
        {
            return value.ToString();
        }

        public long Deserialize(string value)
        {
            return long.Parse(value);
        }
    }

    public class FloatSerializer : ISerializer<float>
    {
        public string Serialize(float value)
        {
            return value.ToString("0.0######");
        }

        public float Deserialize(string value)
        {
            return float.Parse(value);
        }
    }

    public class DoubleSerializer : ISerializer<double>
    {
        public string Serialize(double value)
        {
            return value.ToString("0.0###############");
        }

        public double Deserialize(string value)
        {
            return double.Parse(value);
        }
    }

    public class DecimalSerializer : ISerializer<decimal>
    {
        public string Serialize(decimal value)
        {
            return value.ToString("0.0#############################");
        }

        public decimal Deserialize(string value)
        {
            return decimal.Parse(value);
        }
    }

    public class StringSerialzier : ISerializer<string>
    {
        public string Serialize(string value)
        {
            return value;
        }

        public string Deserialize(string value)
        {
            return value;
        }
    }

    public class TimeSpanSerializer : ISerializer<TimeSpan>
    {
        public string Serialize(TimeSpan value)
        {
            return value.ToString();
        }

        public TimeSpan Deserialize(string value)
        {
            return TimeSpan.Parse(value);
        }
    }

    public class DateTimeSerializer : ISerializer<DateTime>
    {
        public string Serialize(DateTime value)
        {
            return value.ToString();
        }

        public DateTime Deserialize(string value)
        {
            return DateTime.Parse(value);
        }
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