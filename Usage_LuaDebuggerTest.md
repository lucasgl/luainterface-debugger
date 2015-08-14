# Introduction #

To check out the features provided by the LuaDebugger you can use the LuaDebuggerTest application.
It is a simple console application where you can enter your commands into the console.


# Details #

Start LuaDebuggerTest.exe and you will se the following screen:
```
Lua Debugger test app

dofile (df):
  Runs a lua file:
    dofile <filename>
dostring (ds):
  Runs a lua string:
    dostring <lua code> [<lua code>]*
enabledebugger (ed):
  Enables / disables the debugger:
    enabledebugger [true/false]
    ed true - Turns the debugger on
    ed false - Turns the debugger off
    ed - Prints out if debugger is enbaled
fulltrace (ft):
  Enables / disables the full trace feature of the debugger:
    fulltrace [true/false]
    ft true - Turns the full trace on
    ft false - Turns the full trace off
    ft - Prints out if full trace is enbaled
listbreakpoints (lb):
  Prints all breakpoints.
setbreakpoint (sb):
  Sets a breakpoint.
    setbreakpoint <file name> <line>
    sb "something stupid.Lua" 5
removebreakpoint (rb):
  Removes a breakpoint.
    removebreakpoint <file name> <line>
    rb "something stupid.Lua" 5
stop (s):
  Stops the execution at the next executed line.
run (r):
  Continues execution.
stepinto (si):
  Executes the next line and steps into funtions if possible.
stepover (so):
  Executes the next line.
stepout (st):
  Executes until returning from this function.
callstack (cs):
  Prints the current call stack.
localvars (lv):
  Prints the local vars.
setlocalvar (slv):
  Sets the value of a local var.
    setlocalvar <name> <type> <value>
    <name>: the name of the variable
    <type>: type of the value
            null: set to null (<value> is ignored)
            b: boolean (possible values: true, false)
            n: number
            s: string
    <value>: value, must match type
    slv someBoolVar b true
    slv someStringVar s "hello there"
upvalues (uv):
  Prints the up values.
help (?):
  Shows this help info.
exit (e):
  Exits the lua debug test app.

>
```
You see all commands available. You can use the full command name or the short one (shown in braces).
Some of the commands are only available when the debugger has stoped and vice versa.

To load a script call:
```
>dofile test.lua
>
```

Now add a breapoint to it:
```
>setbreakpoint test.lua 20
Added breakpoint at test.lua:20
>
```

Call a function of the script:
```
>dostring dodo()
test.lua:20: Stopping because of Breakpoint
>
```
The script stops at line 20 of test.lua.

Now you can step throug the script:
```
>stepinto
test.lua:9: Stopping because of StepInto
>stepover
test.lua:10: Stopping because of StepOver
>stepout
test.lua:21: Stopping because of StepOut
>
```

You can check the values of the local vars:
```
>localvars
<name>  - <value> [<type>]
l       - heinz abc [System.String]
(*temporary)    - c [System.String]
(*temporary)    - c [System.String]
(*temporary)    - <null> [<null>]
>
```

You can change the value of any local variable:
```
>setlocalvar l s "new value for this string"
>localvars
<name>  - <value> [<type>]
l       - new value for this string [System.String]
(*temporary)    - c [System.String]
(*temporary)    - c [System.String]
(*temporary)    - <null> [<null>]
>setlocalvar l n 6
>localvars
<name>  - <value> [<type>]
l       - 6 [System.Double]
(*temporary)    - c [System.String]
(*temporary)    - c [System.String]
(*temporary)    - <null> [<null>]
>setlocalvar l s "final value"
>localvars
<name>  - <value> [<type>]
l       - final value [System.String]
(*temporary)    - c [System.String]
(*temporary)    - c [System.String]
(*temporary)    - <null> [<null>]
>
```
As you can see you have to specify the type as well.
Now we run the final part of the script and see how the new value of the local var "l" gets printed to console:
```
>run
final value
>
```

Now the script has finished execution and we can exit the LuaDebuggerTest application:
```
>exit
bye
```