// Deserializer.cs
// Author: H3x0R
// Note: Lua 5.1 Bytecode Deserializer

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuaBytecodeUtils
{
    enum LuaTypes
    {
        LUA_TNONE = -1,
        LUA_TNIL,
        LUA_TBOOLEAN,
        LUA_TLIGHTUSERDATA,
        LUA_TNUMBER,
        LUA_TSTRING,
        LUA_TTABLE,
        LUA_TFUNCTION,
        LUA_TUSERDATA,
        LUA_TTHREAD,
        LUA_TWSTRING
    }
    enum InstructionTypes
    {
        iABC,
        iABx,
        iAsBx
    }
    class Lua51Data
    {
        public static string[] INames = new string[] {
            "MOVE",
            "LOADK",
            "LOADBOOL",
            "LOADNIL",
            "GETUPVAL",
            "GETGLOBAL",
            "GETTABLE",
            "SETGLOBAL",
            "SETUPVAL",
            "SETTABLE",
            "NEWTABLE",
            "SELF",
            "ADD",
            "SUB",
            "MUL",
            "DIV",
            "MOD",
            "POW",
            "UNM",
            "NOT",
            "LEN",
            "CONCAT",
            "JMP",
            "EQ",
            "LT",
            "LE",
            "TEST",
            "TESTSET",
            "CALL",
            "TAILCALL",
            "RETURN",
            "FORLOOP",
            "FORPREP",
            "TFORLOOP",
            "SETLIST",
            "CLOSE",
            "CLOSURE",
            "VARARG"
        };

        public static InstructionTypes[] ITypes = new InstructionTypes[] {
            InstructionTypes.iABC,
            InstructionTypes.iABx,
            InstructionTypes.iABC,
            InstructionTypes.iABC,
            InstructionTypes.iABC,
            InstructionTypes.iABx,
            InstructionTypes.iABC,
            InstructionTypes.iABx,
            InstructionTypes.iABC,
            InstructionTypes.iABC,
            InstructionTypes.iABC,
            InstructionTypes.iABC,
            InstructionTypes.iABC,
            InstructionTypes.iABC,
            InstructionTypes.iABC,
            InstructionTypes.iABC,
            InstructionTypes.iABC,
            InstructionTypes.iABC,
            InstructionTypes.iABC,
            InstructionTypes.iABC,
            InstructionTypes.iABC,
            InstructionTypes.iABC,
            InstructionTypes.iAsBx,
            InstructionTypes.iABC,
            InstructionTypes.iABC,
            InstructionTypes.iABC,
            InstructionTypes.iABC,
            InstructionTypes.iABC,
            InstructionTypes.iABC,
            InstructionTypes.iABC,
            InstructionTypes.iABC,
            InstructionTypes.iAsBx,
            InstructionTypes.iAsBx,
            InstructionTypes.iABC,
            InstructionTypes.iABC,
            InstructionTypes.iABC,
            InstructionTypes.iABx,
            InstructionTypes.iABC,
        };
    }
    class Lua51Constant
    {
        public LuaTypes Type;
        public object Data;
    }
    class Lua51Instruction
    {
        public int Line;
        public string Name;
        public byte Opcode;
        public InstructionTypes Type;

        public int A = -1;
        public int B = -1;
        public int C = -1;
        public int Bx = -1;
        public int sBx = -1;
    }
    class Lua51ProtoDebugInfo
    {
        public List<string> LocalNames;
        public List<string> UpvalueNames;
        public Lua51ProtoDebugInfo(List<string> LN, List<string> UN)
        {
            LocalNames = LN;
            UpvalueNames = UN;
        }
    }
    class Lua51Proto
    {
        public string Name;
        public ulong StartLine;
        public ulong FinishLine;
        public ulong SizeK;
        public ulong SizeP;
        public ulong SizeI;
        public byte UpvalueCount;
        public byte ArgumentCount;
        public byte MaxStackSize;
        public byte IntSize;
        public byte Size_T;
        public byte FormatVersion;
        public byte SizeInstruction;
        public byte SizeLuaNumber;
        public bool IsVararg;
        public bool IsIntegral;
        public bool IsLittleEndian;
        public List<Lua51Instruction> Instructions;
        public List<Lua51Constant> Constants;
        public List<Lua51Proto> Protos;
        public Lua51ProtoDebugInfo Debug;
    }
    class Deserializer
    {
        private static string Bytecode;
        private static Reader R;
        private static LuaTypes GetKstType(byte T)
        {
            if (T == 0)
            {
                return LuaTypes.LUA_TNIL;
            }
            else if (T == 1)
            {
                return LuaTypes.LUA_TBOOLEAN;
            }
            else if (T == 3)
            {
                return LuaTypes.LUA_TNUMBER;
            }
            else if (T == 4)
            {
                return LuaTypes.LUA_TSTRING;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public static void DeserializeInfo(ref Lua51Proto Proto)
        {
            Proto.Name = R.ReadString();
            Proto.StartLine = Convert.ToUInt64(R.ReadInt()); // ulong because it can be two seperate data types.
            Proto.FinishLine = Convert.ToUInt64(R.ReadInt());
            Proto.UpvalueCount = R.ReadByte();
            Proto.ArgumentCount = R.ReadByte();
            Proto.IsVararg = (R.ReadByte() != 0x00);
            Proto.MaxStackSize = R.ReadByte();
        }

        public static void DeserializeInstructions(ref Lua51Proto Proto)
        {
            var Instructions = new List<Lua51Instruction>();
            ulong InstructionCount = Convert.ToUInt64(R.ReadInt());
            for (ulong Idx = 1; Idx <= InstructionCount; Idx++)
            {
                Lua51Instruction Instruction = new Lua51Instruction();
                ulong Data = Convert.ToUInt64(R.ReadInt(Proto.SizeInstruction));
                Instruction.Opcode = Convert.ToByte(Data & 0x3F);
                Instruction.Name = Lua51Data.INames[Instruction.Opcode];
                Instruction.A = Convert.ToInt32((Data >> 6) & 0xFF);

                InstructionTypes Type = Lua51Data.ITypes[Instruction.Opcode];
                Instruction.Type = Type;
                if (Type == InstructionTypes.iABC)
                {
                    Instruction.B = Convert.ToInt32((Data >> 23) & 0x1FF);
                    Instruction.C = Convert.ToInt32((Data >> 14) & 0x1FF);
                }
                else if (Type == InstructionTypes.iABx)
                {
                    Instruction.Bx = Convert.ToInt32((Data >> 14) & 0x3FFFF);
                }
                else if (Type == InstructionTypes.iAsBx)
                {
                    Instruction.sBx = Convert.ToInt32(((Data >> 14) & 0x3FFFF) - 0x1FFFF);
                }

                Instructions.Add(Instruction);
            }
            
            Proto.SizeI = InstructionCount;
            Proto.Instructions = Instructions;
        }

        public static void DeserializeConstants(ref Lua51Proto Proto)
        {
            ulong SizeK = Convert.ToUInt64(R.ReadInt());
            var Constants = new List<Lua51Constant>();
            for (ulong Idx = 1; Idx <= SizeK; Idx++)
            {
                Lua51Constant Constant = new Lua51Constant();
                Constant.Type = GetKstType(R.ReadByte());
                if (Constant.Type == LuaTypes.LUA_TNIL)
                {
                    Constant.Data = "nil";
                }
                else if (Constant.Type == LuaTypes.LUA_TBOOLEAN)
                {
                    Constant.Data = R.ReadByte() == 1;
                }
                else if (Constant.Type == LuaTypes.LUA_TNUMBER)
                {
                    Constant.Data = R.ReadDouble();
                }
                else if (Constant.Type == LuaTypes.LUA_TSTRING)
                {
                    Constant.Data = R.ReadString();
                }
                else
                {
                    throw new NotImplementedException();
                }

                Constants.Add(Constant);
            }

            Proto.SizeK = SizeK;
            Proto.Constants = Constants;
        }

        public static void DeserializeProtos(ref Lua51Proto Proto)
        {
            ulong ProtoCount = Convert.ToUInt64(R.ReadInt());
            for (ulong Idx = 1; Idx <= ProtoCount; Idx++)
            {
                Lua51Proto NewProto = new Lua51Proto();
                NewProto.FormatVersion = Proto.FormatVersion;
                NewProto.IsLittleEndian = Proto.IsLittleEndian;
                NewProto.IntSize = Proto.IntSize;
                NewProto.Size_T = Proto.Size_T;
                NewProto.SizeInstruction = Proto.SizeInstruction;
                NewProto.SizeLuaNumber = Proto.SizeLuaNumber;
                NewProto.IsIntegral = Proto.IsIntegral;

                Proto.Protos.Add(DeserializeProto(NewProto));
            }

            Proto.SizeP = ProtoCount;
        }

        public static void DeserializeDebugInfo(ref Lua51Proto Proto)
        {
            var LocalNames = new List<string>();
            var UpvalueNames = new List<string>();
            ulong InstructionLines = Convert.ToUInt64(R.ReadInt());
            for (ulong Idx = 1; Idx <= InstructionLines; Idx++)
            {
                Proto.Instructions[Convert.ToInt32(Idx - 1)].Line = R.ReadInt32();
            }

            ulong LocalCount = Convert.ToUInt64(R.ReadInt());
            for (ulong Idx = 1; Idx <= LocalCount; Idx++)
            {
                LocalNames.Add(R.ReadString());
                R.JumpBytes(8);
            }

            ulong UpvalueCount = Convert.ToUInt64(R.ReadInt());
            for (ulong Idx = 1; Idx <= UpvalueCount; Idx++)
            {
                UpvalueNames.Add(R.ReadString());
            }

            Proto.Debug = new Lua51ProtoDebugInfo(LocalNames, UpvalueNames);
        }

        public static Lua51Proto DeserializeProto(Lua51Proto Proto)
        {
            DeserializeInfo(ref Proto);
            DeserializeInstructions(ref Proto);
            DeserializeConstants(ref Proto);
            DeserializeProtos(ref Proto);
            DeserializeDebugInfo(ref Proto);
            return Proto;
        }

        public static Lua51Proto Deserialize(string BC)
        {
            Bytecode = BC;
            R = new Reader(Bytecode);

            // Verify Signature
            if (R.ReadString(4) != "\x001BLua")
            {
                throw new Exception("Invalid Lua signature.");
            }
            else if (R.ReadByte() != 0x51)
            {
                throw new Exception("Invalid Lua version.");
            }

            Lua51Proto Proto = new Lua51Proto();
            byte FormatVersion = R.ReadByte();
            bool IsLittleEndian = (R.ReadByte() == 0x01);
            byte IntSize = R.ReadByte();
            byte Size_T = R.ReadByte();
            byte SizeInstruction = R.ReadByte();
            byte SizeLuaNumber = R.ReadByte();
            bool IsIntegral = (R.ReadByte() == 0x01);

            // Setup Proto
            Proto.FormatVersion = FormatVersion;
            Proto.IsLittleEndian = IsLittleEndian;
            Proto.IntSize = IntSize;
            Proto.Size_T = Size_T;
            Proto.SizeInstruction = SizeInstruction;
            Proto.SizeLuaNumber = SizeLuaNumber;
            Proto.IsIntegral = IsIntegral;

            // Setup Stream
            R.IsLittleEndian = IsLittleEndian;
            R.IntSize = IntSize;
            R.Size_T = Size_T;

            Lua51Proto MainChunk = DeserializeProto(Proto);
            R = null;
            Bytecode = null;
            return MainChunk;
        }
    }
}
