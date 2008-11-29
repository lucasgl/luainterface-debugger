luanet.load_assembly("System")

Console=luanet.import_type("System.Console")

function c()
  return "c"
end

function ab()
  s = s .. "ab"

  return s
end

function reset()
  s = "heinz "
end

function dodo()
  local l = ab() .. c()
  Console.Out:WriteLine(l)
end

reset()
