-- Lua 5.1 File Dumper
-- Author: H3x0R
-- Note: Kinda emulates luac.exe

local Args = {...}
local File = Args[1] or error("First argument is required.")
local Output = Args[2] or "bytecode.out"

local Stream = io.open(File, "rb") or error("File does not exist.")
local Script = Stream:read("*all")
Stream:close()

local OutStream = io.open(Output, "wb")
local BinaryData = string.dump(loadstring(Script))
OutStream:write(BinaryData)
OutStream:close()
