-- TODO: WORK IN PROGRESS
local Script = "print('hi')"
local Deserialize = require("../Main/Deserialize")
local Chunk = Deserialize(string.dump(loadstring(Script)))

local ConstantTypes = {
    LUA_TNIL = 0;
    LUA_TBOOLEAN = 1;
    LUA_TNUMBER = 3;
    LUA_TSTRING = 4;
}

local function CreateWriter()
    return {
        Data = "";
        ShowHexDump = function(self)
            io.write("\n")
            local Idx = 0
            for Character in self.Data:gmatch(".") do
                Idx = Idx + 1
                if Idx == 13 then
                    Idx = 1
                    io.write("\n")
                end
                io.write(string.format("%02X ", string.byte(Character)))
            end
            io.write("\n")
        end;
        WriteByte = function(self, Byte)
            self.Data = self.Data..string.char(Byte)
        end;
    }
end

local function Transpile(Chunk)
    local Writer = CreateWriter()
    Writer:WriteByte(0x01) --> Compilation status
    Writer:WriteByte(Chunk.SizeK) --> Size_K (Constant pool)
    for Idx = 0, #Chunk.Constants do
        local Const = Chunk.Constants[Idx]
        local CLen;
        if Const.Type == ConstantTypes.LUA_TNIL then

        elseif Const.Type == ConstantTypes.LUA_TBOOLEAN then
            
        elseif Const.Type == ConstantTypes.LUA_TNUMBER then

        elseif Const.Type == ConstantTypes.LUA_TSTRING then
            Writer:WriteByte(#Const.Data)
            for Character in Const.Data:gmatch(".") do
                Writer:WriteByte(string.byte(Character))
            end
        end
    end

    Writer:WriteByte(Chunk.SizeP + 1) --> SizeP (Protos including main chunk)
    Writer:WriteByte(Chunk.MaxStack) --> MaxStackSize
    Writer:WriteByte(Chunk.ArgumentCount) --> NumParams
    Writer:WriteByte(Chunk.UpvalueCount) --> #Upvalues
    Writer:WriteByte(Chunk.IsVararg and 1 or 0) --> is_vararg
    Writer:WriteByte(#Chunk.Instructions)
    

    Writer:ShowHexDump()
    return Code
end

Transpile(Chunk)
