// ByteStream.cs
// Author: H3x0R
// Note: Custom memory stream class.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaBytecodeUtils
{
    class Reader
    {
        private string Data;
        public int StreamPosition = 0;
        public bool IsLittleEndian = true;
        public uint Size_T = 4;
        public uint IntSize = 4;

        public void JumpBytes(object By = null)
        {
            if (By == null)
            {
                By = 1;
            }

            StreamPosition = StreamPosition + (int)By;
        }
        public byte[] ReadBytes(uint Count, bool RespectLittleEndian = true)
        {
            int Start = StreamPosition;
            int End = Start + (int)Count;
            string SplitData = Data.Substring(Start, (End - Start));
            byte[] Bytes = Encoding.ASCII.GetBytes(SplitData);
            JumpBytes((int)Count);
            if (!(IsLittleEndian))
            {
                Array.Reverse(Bytes);
            }

            return Bytes;
        }
        public byte ReadByte()
        {
            return ReadBytes(1)[0];
        }
        public short ReadInt16()
        {
            return BitConverter.ToInt16(ReadBytes(2), 0);
        }
        public uint ReadUInt32()
        {
            return BitConverter.ToUInt32(ReadBytes(4), 0);
        }
        public int ReadInt32()
        {
            return BitConverter.ToInt32(ReadBytes(4), 0);
        }
        public ulong ReadUInt64()
        {
            return BitConverter.ToUInt64(ReadBytes(8), 0);
        }
        public long ReadInt64()
        {
            return BitConverter.ToInt64(ReadBytes(8), 0);
        }
        public float ReadFloat()
        {
            return BitConverter.ToSingle(ReadBytes(4), 0);
        }
        public double ReadDouble()
        {
            return BitConverter.ToDouble(ReadBytes(8), 0);
        }
        public object ReadInt(int Size = -1)
        {
            if (Size == -1)
            {
                if (IntSize == 4)
                {
                    return ReadUInt32();
                }
                else if (IntSize == 8)
                {
                    return ReadUInt64();
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            else
            {
                if (Size == 4)
                {
                    return ReadUInt32();
                }
                else if (Size == 8)
                {
                    return ReadUInt64();
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
        }
        public object GetSize_T()
        {
            if (Size_T == 4)
            {
                return ReadUInt32();
            }
            else if (Size_T == 8)
            {
                return ReadUInt64();
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        public string ReadString(object Length = null)
        {
            if (Length == null)
            {
                Length = GetSize_T();
            }

            byte[] Buffer = ReadBytes(Convert.ToUInt32(Length), false);
            return Encoding.ASCII.GetString(Buffer);
        }
        public Reader(string Bytecode)
        {
            Data = Bytecode;
        }
    }
    class Writer
    {
        public string Data;
        public bool IsLittleEndian = true;
        public uint Size_T = 4;
        public uint IntSize = 4;

        public bool WriteBytes(byte[] Bytes, bool RespectLittleEndian = true)
        {
            if (!(IsLittleEndian))
            {
                Array.Reverse(Bytes);
            }

            Data += Encoding.ASCII.GetString(Bytes);
            return true;
        }
        public bool WriteByte(byte ToWrite)
        {
            return WriteBytes(BitConverter.GetBytes(ToWrite));
        }
        public bool WriteInt16(short ToWrite)
        {
            return WriteBytes(BitConverter.GetBytes(ToWrite));
        }
        public bool WriteUInt32(uint ToWrite)
        {
            return WriteBytes(BitConverter.GetBytes(ToWrite));
        }
        public bool WriteInt32(int ToWrite)
        {
            return WriteBytes(BitConverter.GetBytes(ToWrite));
        }
        public bool WriteUInt64(ulong ToWrite)
        {
            return WriteBytes(BitConverter.GetBytes(ToWrite));
        }
        public bool WriteInt64(long ToWrite)
        {
            return WriteBytes(BitConverter.GetBytes(ToWrite));
        }
        public bool WriteFloat(float ToWrite)
        {
            return WriteBytes(BitConverter.GetBytes(ToWrite));
        }
        public bool WriteDouble(double ToWrite)
        {
            return WriteBytes(BitConverter.GetBytes(ToWrite));
        }
        public bool WriteInt(object Data)
        {
            if (IntSize == 4)
            {
                return WriteUInt32((uint)Data);
            }
            else if (IntSize == 8)
            {
                return WriteUInt64((ulong)Data);
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        public bool WriteString(string Data, bool SizeT = false)
        {
            byte[] Bytes = Encoding.ASCII.GetBytes(Data);
            if (SizeT == true) // Write a size_t to go with the string.
            {
                if (Size_T == 4)
                {
                    WriteUInt32((uint)Bytes.Length);
                }
                else if (Size_T == 8)
                {
                    WriteUInt64((uint)Bytes.Length);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            return WriteBytes(Bytes, false);
        }
        public Writer(string Bytecode = "")
        {
            Data = Bytecode;
        }
    }
}
