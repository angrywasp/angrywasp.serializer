using System;
using System.Collections.Generic;
using System.Linq;
using AngryWasp.Helpers;

namespace AngryWasp.Serializer.Serializers
{
    public class ByteArraySerializer : IBinarySerializer<byte[]>
    {
        public BinarySerializerData Serialize(byte[] value) => new BinarySerializerData(FileHelper.Compress(value), value.Length);

        public byte[] Deserialize(BinarySerializerData data) => FileHelper.Decompress(data.Data, data.Length);
    }

    public class Int32ArraySerializer : IBinarySerializer<int[]>
    {
        private const int stride = sizeof(int);

        public BinarySerializerData Serialize(int[] value)
        {
            byte[] b = new byte[value.Length * stride];
            for (int i = 0; i < value.Length; i++)
                Buffer.BlockCopy(BitShifter.ToByte(value[i]), 0, b, i * stride, stride);

            return new BinarySerializerData(FileHelper.Compress(b), value.Length * stride);
        }

        public int[] Deserialize(BinarySerializerData data)
        {
            byte[] b = FileHelper.Decompress(data.Data, data.Length);
            int[] returnValue = new int[data.Length / stride];
            int x = 0, start = 0;

            for (int i = 0; i < data.Length; i += stride)
                returnValue[x++] = BitShifter.ToInt(b, ref start);

            return returnValue;
        }
    }

    public class UInt16ListSerializer : IBinarySerializer<List<ushort>>
    {
        private const int stride = sizeof(short);

        public BinarySerializerData Serialize(List<ushort> value)
        {
            byte[] b = new byte[value.Count * stride];
            for (int i = 0; i < value.Count; i++)
                Buffer.BlockCopy(BitShifter.ToByte(value[i]), 0, b, i * stride, stride);

            return new BinarySerializerData(FileHelper.Compress(b), value.Count * stride);
        }

        public List<ushort> Deserialize(BinarySerializerData data)
        {
            byte[] b = FileHelper.Decompress(data.Data, data.Length);
            ushort[] returnValue = new ushort[data.Length / stride];
            int x = 0, start = 0;

            for (int i = 0; i < data.Length; i += stride)
                returnValue[x++] = BitShifter.ToUShort(b, ref start);

            return returnValue.ToList();
        }
    }
}
