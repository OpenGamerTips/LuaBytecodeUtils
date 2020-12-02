-- BinaryStream
-- Author: H3x0R
-- Note: Reads data from binary files
local byte = string.byte
local Export = {}

local function ExtractBits(Number, Start, Finish)
    assert(type(Number) == "number", "Expected a number for argument 1.")
    assert(type(Start) == "number", "Expected a number for argument 2.")
    if Finish then
        local Sum = 0
        local DigiCount = 0
        for Idx = Start, Finish do
            Sum = Sum + 2 ^ DigiCount * ExtractBits(Number, Idx)
            DigiCount = DigiCount + 1
        end
        
        return Sum
    else
        local PowNum = 2 ^ (Start - 1)
        return (Number % (PowNum + PowNum) >= PowNum) and 1 or 0
    end
end
Export.ExtractBits = ExtractBits

function Export.new(Bin)
    assert(type(Bin) == "string", "Expected binary data.")
    return {
        Position = 1;
        BigEndian = true;
        Size_T = 4;
        IntSize = 4;

        ReadSizeT = function(self)
            if self.Size_T == 4 then
                return self:ReadInt32()
            elseif self.Size_T == 8 then
                return self:ReadInt64()
            end
        end;
        JumpBytes = function(self, Len)
            Len = Len or 1
            self.Position = self.Position + Len
        end;
        ReadByte = function(self) -- byte
            local Byte = byte(Bin:sub(self.Position, self.Position))
            self.Position = self.Position + 1
            return Byte;
        end;
        ReadInt32 = function(self, RespectLittleEndian) -- uint
            RespectLittleEndian = RespectLittleEndian or true
            local ByteOne, ByteTwo, ByteThree, ByteFour = byte(Bin, self.Position, self.Position + 3)
            self.Position = self.Position + 4

            if RespectLittleEndian and (BigEndian == false) then
                return ByteOne * 0x1000000 + ByteTwo * 0x10000 + ByteThree * 0x100 + ByteFour
            else
                return ByteFour * 0x1000000 + ByteThree * 0x10000 + ByteTwo * 0x100 + ByteOne
            end
        end;
        ReadInt64 = function(self, RespectLittleEndian) -- ulong
            RespectLittleEndian = RespectLittleEndian or true
            local Int1, Int2 = self:ReadInt32(RespectLittleEndian), self:ReadInt32(RespectLittleEndian)
            return Int2 * 0x100000000 + Int1;
        end;
        ReadDouble = function(self) -- double
            local Int1, Int2 = self:ReadInt32(), self:ReadInt32()
            return (-2 * ExtractBits(Int2, 32) + 1) * (2 ^ (ExtractBits(Int2, 21, 31) - 1023)) * ((ExtractBits(Int2, 1, 20) * (0x100000000) + Int1) / (0x10000000000000) + 1)
        end;
        ReadInt = function(self, RespectLittleEndian) -- uint/or double
            RespectLittleEndian = RespectLittleEndian or true
            if self.IntSize == 4 then
                return self:ReadInt32(RespectLittleEndian);
            elseif self.IntSize == 8 then
                return self:ReadInt64(RespectLittleEndian);
            end
        end;
        ReadString = function(self, Len)
            local CutLast = false
            if not Len then
                CutLast = true
                Len = self:ReadSizeT()
            end

            if Len == 0 then return "" end
            Len = Len - 1

            local Str = Bin:sub(self.Position, (self.Position + Len))
            self.Position = self.Position + Len + 1
            return CutLast and Str:sub(1, -2) or Str
        end;
    }
end

--[[
local function Bin2Str(Binary)
    local String = ""
    for Idx = 1, #Binary, 8 do
        local Start = Idx
        local End = Idx + 7
        local Data = Binary:sub(Start, End)
        local Integer = tonumber(Data, 2)
        String = String..string.char(Integer)
    end

    return String
end]]

return Export
