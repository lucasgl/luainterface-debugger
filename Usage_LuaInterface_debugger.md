# Introduction #

This page should give you a short overview on how to use the LuaInterface-debugger.

# Details #

First of all you need to use the correct namespaces:
```
using LuaInterface;
using LuaInterface.Debugger;
```

Next you have to create an instance of LuaInterface and LuaInterface-debugger,
and handle the necessarry events.
```
Lua lua = new Lua();
lua.HookException += Lua_HookException;

LuaDebugger debugger = new LuaDebugger(lua);
debugger.Stoping += Debugger_Stoping;
debugger.WaitingForAction += Debugger_WaitingForAction;
```

Create a instance of `Lua` as always or use an existing one.
You should handle the `Lua.HookException` event, in case that any Exceptions are thrown inside your debug code. If you don't handle this event, the exceptions will get to .net heaven (the garbage collector).

Then create a instance of `LuaDebugger` and pass your lua interpreter to it in the constructor.
You need to handle the two events `LuaDebugger.Stoping` and `LuaDebugger.WaitingForAction`.

`LuaDebugger.Stoping` is called every time the debugger stops.
`LuaDebugger.WaitingForAction` is called every time the debugger needs some user interaction.

How to implement the event handler will be discussed later.

Finally you need to enable the debugger:
```
debugger.Enable = true;
```
As long as the debugger is not enabled, your lua code will run exactly as if no debugger is attached to the interpreter. Even the performance is the same.


## Handling the Stoping event ##

```
void Debugger_Stoping(object sender, StopingEventArgs e)
{
   string reason;
   if (e.Breakpoint != null)
   {
      reason = "Breakpoint";
   }
   else
   {
      reason = e.Action.ToString();
   }
   Console.Out.WriteLine(String.Format("{0}:{1}: Stopping because of {2}", e.FileName, e.Line, reason));
}
```
This implementation simply prints the reason for stoping to the console.
In an IDE debugger you can jump to the correct line in the lua file.

## Handling the WaitingForAction event ##

```
void Debugger_WaitingForAction(object sender, EventArgs e)
{
   Console.Write(">");
   string cmd = Console.In.ReadLine();
   if (cmd != String.Empty)
   {
      ExecCommand(cmd);
   }
}
```

This implementation waits for an input on the console.
What you need to do here is to call one of the following methos:
  * `Run()`
  * `StepInto()`
  * `StepOver()`
  * `StepOut()`

As long as you do not call one of the methods above, the event gets called again and again after the event handler returns.

## Get the debugger to stop ##
You can stop the execution of a running lua script by doing the following.
  * Call the `LuaDebugger.Stop()` method at any time, and the script will stop at the next executed line.
  * Call `LuaDebugger.AddBreakpoint(...)` and the debugger will stop at the specified line in the given file.
```
debugger.AddBreakpoint("test.lua", 20);
lua.DoFile("test.lua");
```

The LuaDebugger manages all files and breakpoints.
You can add as many files and breakpoints as you wish.
The debugger does not check if the files exist and if the given line can contain a breakpoint.

The files and breakpoints are represented by the classes
  * `LuaDebugFile`
  * `LuaDebugBreakpoint`
You can "subclass" these classes by implementing the two class factories `LuaDebugFileFactory` and `LuaDebugBreakpointFactory`.
Simply assign the factories to the LuaDebugger instance.