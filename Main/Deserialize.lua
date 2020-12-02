-- Lua 5.1 Bytecode Deserializer
-- Author: H3x0R
-- Note: Depends on BinaryStream.lua and BytecodeTypes.lua (both written by me)

-- Custom Patches
local function require(Module) -- require with full path support
    local LuaFile = io.open(Module..".lua", "rb") or io.open(Module..".luac", "rb") or error("module '"..Module.."' not found.'", 2)
    local ToLoad = LuaFile:read("*all")
    io.close(LuaFile)

    return loadstring(ToLoad)()
end

-- Define Names/Types
local Enums = require("Enums")
local ConstantTypes = Enums.ConstantTypes
local InstructionTypes = Enums.InstructionTypes
local InstructionNames = Enums.InstructionNames

local BinaryLib = require("../Data/BinaryStream")
local Stream;
local function DeserializeChunkInfo(Chunk)
    -- Basic Header Info
    Chunk.Name = Stream:ReadString()
    Chunk.StartLine = Stream:ReadInt()
    Chunk.FinishLine = Stream:ReadInt()

    -- Advanced Header Info
    Chunk.UpvalueCount = Stream:ReadByte()
    Chunk.ArgumentCount = Stream:ReadByte()
    Chunk.IsVararg = Stream:ReadByte() ~= 0
    Chunk.MaxStack = Stream:ReadByte()

    return Chunk
end

local function DeserializeInstructions(Instructions, _DEBUG)
    local InstructionCount = Stream:ReadInt()
    for Idx = 1, InstructionCount do
        local Operation = {
            Line = 0;
            Name = nil;
            Type = "";

            A = 0;
            B = 0;
            C = 0;
            Bx = 0;
            sBx = 0;
        }
        
        local Data = Stream:ReadInt32()
        local Opcode = BinaryLib.ExtractBits(Data, 1, 6); Operation.Opcode = Opcode
        local Type = InstructionTypes[Opcode]; Operation.Type = Type
        if _DEBUG then
            Operation.Name = InstructionNames[Opcode]
        end

        Operation.A = BinaryLib.ExtractBits(Data, 7, 14)
        if Type == "ABC" then
            Operation.B = BinaryLib.ExtractBits(Data, 24, 32)
            Operation.C = BinaryLib.ExtractBits(Data, 15, 23)
        elseif Type == "ABx" then
            Operation.Bx = BinaryLib.ExtractBits(Data, 15, 32)
        elseif Type ==  "AsBx" then
            Operation.sBx = BinaryLib.ExtractBits(Data, 15, 32)
        end

        Instructions[Idx] = Operation
    end

    return Instructions
end

local function DeserializeConstants(Chunk)
    local SizeK = Stream:ReadInt()
    Chunk.SizeK = SizeK
    for Idx = 1, SizeK do
        local Constant = {
            Type = Stream:ReadByte();
            Data = nil;
        }

        if Constant.Type == ConstantTypes.LUA_TNIL then -- 0
            Constant.Data = "nil"
        elseif Constant.Type == ConstantTypes.LUA_TBOOLEAN then -- 1
            Constant.Data = Stream:ReadByte() == 1
        elseif Constant.Type == ConstantTypes.LUA_TNUMBER then -- 3
            Constant.Data = Stream:ReadDouble()
        elseif Constant.Type == ConstantTypes.LUA_TSTRING then -- 4
            Constant.Data = Stream:ReadString()
        end

        Chunk.Constants[Idx - 1] = Constant
    end

    return Chunk
end

local function DeserializeDebugInfo(Debug, Instructions)
    local InstLines = Stream:ReadInt()
    for Idx = 1, InstLines do
        Instructions[Idx].Line = Stream:ReadInt32()
    end

    local LocalCount = Stream:ReadInt()
    for Idx = 1, LocalCount do
        Debug.LocalNames[Idx] = Stream:ReadString()
        Stream:JumpBytes(8)
    end

    local UpvalueCount = Stream:ReadInt()
    for Idx = 1, UpvalueCount do
        Debug.UpvalueNames[Idx] = Stream:ReadString()
    end

    return Debug, Instructions
end

local function DeserializeChunk(_DEBUG) -- i mean its also a proto but chunk applies for all of them
    local Chunk = {
        Name = "";
        StartLine = 0;
        FinishLine = 0;
        SizeK = 0;
        SizeP = 0;

        Instructions = {};
        Constants = {};
        Protos = {};

        Debug = {
            LocalNames = {};
            UpvalueNames = {};
        };
    }

    Chunk = DeserializeChunkInfo(Chunk)
    Chunk.Instructions = DeserializeInstructions(Chunk.Instructions, _DEBUG)
    Chunk = DeserializeConstants(Chunk)
    local ProtoCount = Stream:ReadInt(); Chunk.SizeP = ProtoCount
    for Idx = 1, ProtoCount do
        Chunk.Protos[Idx - 1] = DeserializeChunk(_DEBUG)
    end
    Chunk.Debug, Chunk.Instructions = DeserializeDebugInfo(Chunk.Debug, Chunk.Instructions)

    if _DEBUG then
        local LargestNameCount = 0
        local LargestIndexCount = 0
        local LargestLineCount = 0
        for Idx, Value in pairs(Chunk.Instructions) do
            if #Value.Name > LargestNameCount then
                LargestNameCount = #Value.Name + 1
            end

            local LineLen = #tostring(Value.Line)
            if LineLen > LargestLineCount then
                LargestLineCount = LineLen + 1
            end

            local IdxLen = #tostring(Idx)
            if IdxLen > LargestIndexCount then
                LargestIndexCount = IdxLen + 1
            end
        end

        table.foreach(Chunk.Instructions, function(Idx, Value)
            print("Inst: "..Idx..","..string.rep(" ", LargestIndexCount - #tostring(Idx)).."Line: "..Value.Line..string.rep(" ", LargestLineCount - #tostring(Value.Line)).."| "..Value.Name..string.rep(" ", LargestNameCount - #Value.Name)..Value.A.." "..Value.B.." "..Value.C.." "..Value.Bx.." "..Value.sBx)
        end)
    end
    return Chunk
end

local function Deserialize(Bytecode, IsDebugging)
    assert(type(Bytecode) == "string", "Expected a string.")
    Stream = BinaryLib.new(Bytecode)

    -- Decode header and setup stream.
    if Stream:ReadString(4) ~= "\27Lua" then
        error("Invalid bytecode header.")
    elseif Stream:ReadString(1) ~= "Q" then
        error("Bytecode does not match version 5.1.")
    end

    Stream:JumpBytes(1) -- lua signing leftovers
    Stream.BigEndian = Stream:ReadByte() == 0 -- 0=big, 1=little
    Stream.IntSize = Stream:ReadByte()
    Stream.Size_T = Stream:ReadByte()
    Stream:JumpBytes(3) -- Assuming \4\8\0 because im too lazy to make conversions
    return DeserializeChunk(IsDebugging)
end

return Deserialize
