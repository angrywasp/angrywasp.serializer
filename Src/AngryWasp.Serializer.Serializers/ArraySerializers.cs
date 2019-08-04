using System;
using System.IO;
using System.Reflection;
using System.Text;
using AngryWasp.Helpers;

namespace AngryWasp.Serializer.Serializers
{
    /*public class BoolArraySerializer : ISerializer<bool[]>
    {
        public string Serialize(bool[] value) => throw new NotImplementedException();

        public bool[] Deserialize(string value) => throw new NotImplementedException();
    }*/

    public class ByteArraySerializer : ISerializer<byte[]>
    {
        public string Serialize(byte[] value) => value.ToHex();

        public byte[] Deserialize(string value) => value.FromByteHex();
    }

    public class SbyteArraySerializer : ISerializer<sbyte[]>
    {
        public string Serialize(sbyte[] value) => value.ToHex();

        public sbyte[] Deserialize(string value) => value.FromSByteHex();
    }

    //public class CharArraySerializer : ISerializer<char[]>
    //do not impelement. a char[] is a string

    public class UshortArraySerializer : ISerializer<ushort[]>
    {
        public string Serialize(ushort[] value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var v in value)
                sb.Append(BitShifter.ToByte(v).ToHex());

            return sb.ToString();
        }

        public ushort[] Deserialize(string value)
        {
            int stride = 4;

            ushort[] ret = new ushort[value.Length / stride];
            for (int i = 0; i < value.Length; i += stride)
            {
                string sub = value.Substring(i, i + stride);
                ret[i] = BitShifter.ToUShort(sub.FromByteHex());
            }

            return ret;
        }
    }

    public class ShortArraySerializer : ISerializer<short[]>
    {
        public string Serialize(short[] value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var v in value)
                sb.Append(BitShifter.ToByte(v).ToHex());

            return sb.ToString();
        }

        public short[] Deserialize(string value)
        {
            int stride = 4;

            short[] ret = new short[value.Length / stride];
            for (int i = 0; i < value.Length; i += stride)
            {
                string sub = value.Substring(i, i + stride);
                ret[i] = BitShifter.ToShort(sub.FromByteHex());
            }

            return ret;
        }
    }

    public class UintArraySerializer : ISerializer<uint[]>
    {
        public string Serialize(uint[] value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var v in value)
                sb.Append(BitShifter.ToByte(v).ToHex());

            return sb.ToString();
        }

        public uint[] Deserialize(string value)
        {
            int stride = 8;

            uint[] ret = new uint[value.Length / stride];
            for (int i = 0; i < value.Length; i += stride)
            {
                string sub = value.Substring(i, i + stride);
                ret[i] = BitShifter.ToUInt(sub.FromByteHex());
            }

            return ret;
        }
    }

    public class IntArraySerializer : ISerializer<int[]>
    {
        public string Serialize(int[] value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var v in value)
                sb.Append(BitShifter.ToByte(v).ToHex());

            return sb.ToString();
        }

        public int[] Deserialize(string value)
        {
            int stride = 8;

            int[] ret = new int[value.Length / stride];
            for (int i = 0; i < value.Length; i += stride)
            {
                string sub = value.Substring(i, i + stride);
                ret[i] = BitShifter.ToInt(sub.FromByteHex());
            }

            return ret;
        }
    }

    public class UlongArraySerializer : ISerializer<ulong[]>
    {
        public string Serialize(ulong[] value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var v in value)
                sb.Append(BitShifter.ToByte(v).ToHex());

            return sb.ToString();
        }

        public ulong[] Deserialize(string value)
        {
            int stride = 16;

            ulong[] ret = new ulong[value.Length / stride];
            for (int i = 0; i < value.Length; i += stride)
            {
                string sub = value.Substring(i, i + stride);
                ret[i] = BitShifter.ToULong(sub.FromByteHex());
            }

            return ret;
        }
    }

    public class LongArraySerializer : ISerializer<long[]>
    {
        public string Serialize(long[] value)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var v in value)
                sb.Append(BitShifter.ToByte(v).ToHex());

            return sb.ToString();
        }

        public long[] Deserialize(string value)
        {
            int stride = 16;

            long[] ret = new long[value.Length / stride];
            for (int i = 0; i < value.Length; i += stride)
            {
                string sub = value.Substring(i, i + stride);
                ret[i] = BitShifter.ToLong(sub.FromByteHex());
            }

            return ret;
        }
    }

    /*public class FloatArraySerializer : ISerializer<float[]>
    {
        public string Serialize(float[] value) => throw new NotImplementedException();

        public float[] Deserialize(string value) => throw new NotImplementedException();
    }

    public class DoubleArraySerializer : ISerializer<double[]>
    {
        public string Serialize(double[] value) => throw new NotImplementedException();

        public double[] Deserialize(string value) => throw new NotImplementedException();
    }

    public class DecimalArraySerializer : ISerializer<decimal[]>
    {
        public string Serialize(decimal[] value) => throw new NotImplementedException();

        public decimal[] Deserialize(string value) => throw new NotImplementedException();
    }*/
}