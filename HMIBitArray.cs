using System;
using System.Collections;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace QuelleHMI
{
    //  This class is designed to assist in serialization/deserialization of AVXSearchBits
    //
    class AVXSearchBitsSerializable
    {
        public string segments;
        public string fragments;

        public AVXSearchBitsSerializable(BitArray segBits, BitArray fragBits)
        {
            this.segments = ConvertToString(segBits);
            this.fragments = ConvertToString(fragBits);
        }
        public AVXSearchBitsSerializable(AVXSearchBits searchBits)
        {
            this.segments = ConvertToString(searchBits.segments);
            this.fragments = ConvertToString(searchBits.fragments);
        }
        public BitArray SegmentBits
        {
            get
            {
                return ConvertToBitArray(this.segments);
            }
            set
            {
                this.segments = ConvertToString(value);
            }
        }
        public BitArray FragmentBits
        {
            get
            {
                return ConvertToBitArray(this.fragments);
            }
            set
            {
                this.fragments = ConvertToString(value);
            }
        }
        public AVXSearchBits SearchBits
        {
            get
            {
                return new AVXSearchBits(this);
            }
            set
            {
                this.segments = ConvertToString(value.segments);
                this.fragments = ConvertToString(value.fragments);
            }
        }
        protected static string ConvertToString(BitArray bitArray)
        {
            int cnt = bitArray.Count;
            int cntPlus = cnt % 4 == 0 ? cnt : cnt + 4 - (cnt % 4);
            int cntDiff = cntPlus - cnt;

            bool[] array = new bool[cntPlus];

            for (int i = 0; i < cntDiff; i++)
                array[i] = false;
            bitArray.CopyTo(array, cntDiff);
 
            StringBuilder text = new StringBuilder(1 + (cnt / 4));
            int cursor = cnt - 1;

            byte digit = 0;
            byte bit = 0x8;
            char hex;
            foreach (var on in array)
            {
                if (on)
                    digit |= bit;

                if (bit == 0x1)
                {
                    hex = (digit <= 9) ? '0' : 'A';
                    if (digit != 0)
                        hex += (digit <= 9) ? (char)digit : (char)(digit - 0xA);
                    text.Append(hex);

                    digit = 0;
                    bit = 0x8;
                }
                else
                {
                    bit = (byte)(bit >> 1);
                }
            }
            return text.ToString();
        }
        protected static BitArray ConvertToBitArray(string hex)
        {
            if (hex == null)
                return null;

            BitArray bits = new BitArray(4 * hex.Length);
            for (int i = 0; i < hex.Length; i++)
            {
                byte b = byte.Parse(hex[i].ToString(), System.Globalization.NumberStyles.HexNumber);
                for (int j = 0; j < 4; j++)
                {
                    bits.Set(i * 4 + j, (b & (1 << (3 - j))) != 0);
                }
            }
            return bits;
        }
    }
    class AVXSearchBits
    {
        private static JsonSerializer serializer = null;

        public BitArray segments { get; private set; }
        public BitArray fragments { get; private set; }

        public AVXSearchBits (BitArray segBits, BitArray fragBits)
        {
            this.segments = segBits;
            this.fragments = fragBits;
        }
        public AVXSearchBits(AVXSearchBitsSerializable serialiableBits)
        {
            this.segments = serialiableBits.SegmentBits;
            this.fragments = serialiableBits.FragmentBits;
        }
        public AVXSearchBits()
        {
            this.segments = new BitArray(16);
            this.fragments = new BitArray(16);
        }

        public string SerializeToJson()
        {
            string result = null;

            if (serializer == null)
                serializer = new JsonSerializer();

            var serializable = new AVXSearchBitsSerializable(this);
            using (var ms = new MemoryStream())
            using (StreamWriter sw = new StreamWriter(ms))
            using (JsonWriter writer = new JsonTextWriter(sw))
            {
                serializer.Serialize(writer, serializable);
                ms.Seek(0, SeekOrigin.Begin);
                result = ms.ToString();
            }
            return result;
        }
    }
}
