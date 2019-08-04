using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            string[] chunks = value.Split(4).ToArray();
            ushort[] ret = new ushort[chunks.Length];

            for (int i = 0; i < chunks.Length; i++)
                ret[i] = BitShifter.ToUShort(chunks[i].FromByteHex());

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
            string[] chunks = value.Split(4).ToArray();
            short[] ret = new short[chunks.Length];

            for (int i = 0; i < chunks.Length; i++)
                ret[i] = BitShifter.ToShort(chunks[i].FromByteHex());

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
            string[] chunks = value.Split(8).ToArray();
            uint[] ret = new uint[chunks.Length];

            for (int i = 0; i < chunks.Length; i++)
                ret[i] = BitShifter.ToUInt(chunks[i].FromByteHex());

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
            string[] chunks = value.Split(8).ToArray();
            int[] ret = new int[chunks.Length];

            for (int i = 0; i < chunks.Length; i++)
                ret[i] = BitShifter.ToInt(chunks[i].FromByteHex());

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
            string[] chunks = value.Split(16).ToArray();
            ulong[] ret = new ulong[chunks.Length];

            for (int i = 0; i < chunks.Length; i++)
                ret[i] = BitShifter.ToULong(chunks[i].FromByteHex());

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
            string[] chunks = value.Split(16).ToArray();
            long[] ret = new long[chunks.Length];

            for (int i = 0; i < chunks.Length; i++)
                ret[i] = BitShifter.ToLong(chunks[i].FromByteHex());

            return ret;
        }
    }

    public static class StringExtensions
    {    
        public static IEnumerable<string> Split(this string str, int length)
        {
            if (String.IsNullOrEmpty(str)) throw new ArgumentException();
            if (length < 1) throw new ArgumentException();

            for (int i = 0; i < str.Length; i += length)
            {
                if (length + i > str.Length)
                    length = str.Length - i;

                yield return str.Substring(i, length);
            }
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