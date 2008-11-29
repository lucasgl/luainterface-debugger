using System;
using System.Collections.Generic;
using System.Text;
using LuaInterface;
using LuaInterface.Debugger;

namespace LuaDebuggerTest
{
   public class LuaDbgEnableCommand : Command
   {
      public LuaDbgEnableCommand(LuaDebugApp app)
         : base(app)
      {
         Cmd = "enabledebugger";
         Cmd2 = "ed";
         AvailableWhileStoped = false;
      }

      public override void PrintHelp()
      {
         base.PrintHelp();
         Console.Out.WriteLine("  Enables / disables the debugger:");
         Console.Out.WriteLine("    enabledebugger [true/false]");
         Console.Out.WriteLine("    ed true - Turns the debugger on");
         Console.Out.WriteLine("    ed false - Turns the debugger off");
         Console.Out.WriteLine("    ed - Prints out if debugger is enbaled");
      }

      protected override void Process(string[] args)
      {
         if (args.Length < 2)
         {
            Console.Out.WriteLine(String.Format("Debugger enabled: {0}", App.Debugger.Enabled.ToString()));
         }
         else
         {
            try
            {
               App.Debugger.Enabled = Boolean.Parse(args[1]);
               Console.Out.WriteLine(String.Format("Debugger enabled: {0}", App.Debugger.Enabled.ToString()));
            }
            catch (FormatException ex)
            {
               Console.Out.WriteLine("Argument 1: FormatException");
               Console.Out.WriteLine(ex.Message);
            }
         }
      }
   }

   public class LuaDbgFullTraceCommand : Command
   {
      public LuaDbgFullTraceCommand(LuaDebugApp app)
         : base(app)
      {
         Cmd = "fulltrace";
         Cmd2 = "ft";
      }

      public override void PrintHelp()
      {
         base.PrintHelp();
         Console.Out.WriteLine("  Enables / disables the full trace feature of the debugger:");
         Console.Out.WriteLine("    fulltrace [true/false]");
         Console.Out.WriteLine("    ft true - Turns the full trace on");
         Console.Out.WriteLine("    ft false - Turns the full trace off");
         Console.Out.WriteLine("    ft - Prints out if full trace is enbaled");
      }

      protected override void Process(string[] args)
      {
         if (args.Length < 2)
         {
            Console.Out.WriteLine(String.Format("Full trace enabled: {0}", App.Debugger.FullTrace.ToString()));
         }
         else
         {
            try
            {
               App.Debugger.FullTrace = Boolean.Parse(args[1]);
               Console.Out.WriteLine(String.Format("Full trace enabled: {0}", App.Debugger.FullTrace.ToString()));
            }
            catch (FormatException ex)
            {
               Console.Out.WriteLine("Argument 1: FormatException");
               Console.Out.WriteLine(ex.Message);
            }
         }
      }
   }

   public class LuaDbgListBreakppointsCommand : Command
   {
      public LuaDbgListBreakppointsCommand(LuaDebugApp app)
         : base(app)
      {
         Cmd = "listbreakpoints";
         Cmd2 = "lb";
      }

      public override void PrintHelp()
      {
         base.PrintHelp();
         Console.Out.WriteLine("  Prints all breakpoints.");
      }

      protected override void Process(string[] args)
      {
         Console.Out.WriteLine("Breakpoints:");
         foreach (var file in App.Debugger.Files)
         {
            if (file.Breakpoints.Count > 0)
            {
               Console.Out.WriteLine(file.FileName);
               foreach (var breakpoint in file.Breakpoints)
               {
                  Console.Out.WriteLine(String.Format("  Line: {0}", breakpoint.Line));
               }
            }
         }
      }
   }

   public class LuaDbgSetBreakppointCommand : Command
   {
      public LuaDbgSetBreakppointCommand(LuaDebugApp app)
         : base(app)
      {
         Cmd = "setbreakpoint";
         Cmd2 = "sb";
      }

      public override void PrintHelp()
      {
         base.PrintHelp();
         Console.Out.WriteLine("  Sets a breakpoint.");
         Console.Out.WriteLine("    setbreakpoint <file name> <line>");
         Console.Out.WriteLine("    sb \"something stupid.Lua\" 5");
      }

      protected override void Process(string[] args)
      {
         if (args.Length < 2)
         {
            Console.Out.WriteLine("Argument <file name> is missing");
         }
         else if (args.Length < 3)
         {
            Console.Out.WriteLine("Argument <line> is missing");
         }
         else
         {
            try
            {
               LuaDebugBreakpoint breapoint = App.Debugger.AddBreakpoint(args[1], Int32.Parse(args[2]));
               Console.Out.WriteLine(String.Format("Added breakpoint at {0}:{1}", breapoint.File.FileName, breapoint.Line));
            }
            catch (FormatException ex)
            {
               Console.Out.WriteLine("Argument 2: FormatException");
               Console.Out.WriteLine(ex.Message);
            }
         }
      }
   }

   public class LuaDbgRemoveBreakppointCommand : Command
   {
      public LuaDbgRemoveBreakppointCommand(LuaDebugApp app)
         : base(app)
      {
         Cmd = "removebreakpoint";
         Cmd2 = "rb";
      }

      public override void PrintHelp()
      {
         base.PrintHelp();
         Console.Out.WriteLine("  Removes a breakpoint.");
         Console.Out.WriteLine("    removebreakpoint <file name> <line>");
         Console.Out.WriteLine("    rb \"something stupid.Lua\" 5");
      }

      protected override void Process(string[] args)
      {
         if (args.Length < 2)
         {
            Console.Out.WriteLine("Argument <file name> is missing");
         }
         else if (args.Length < 3)
         {
            Console.Out.WriteLine("Argument <line> is missing");
         }
         else
         {
            try
            {
               App.Debugger.RemoveBreakpoint(args[1], Int32.Parse(args[2]));
               Console.Out.WriteLine(String.Format("Removed breakpoint at {0}:{1}", args[1], args[2]));
            }
            catch (FormatException ex)
            {
               Console.Out.WriteLine("Argument 2: FormatException");
               Console.Out.WriteLine(ex.Message);
            }
         }
      }
   }

   public class LuaDbgStopCommand : Command
   {
      public LuaDbgStopCommand(LuaDebugApp app)
         : base(app)
      {
         Cmd = "stop";
         Cmd2 = "s";
         AvailableWhileStoped = false;
      }

      public override void PrintHelp()
      {
         base.PrintHelp();
         Console.Out.WriteLine("  Stops the execution at the next executed line.");
      }

      protected override void Process(string[] args)
      {
         App.Debugger.Stop();
      }
   }

   public class LuaDbgRunCommand : Command
   {
      public LuaDbgRunCommand(LuaDebugApp app)
         : base(app)
      {
         Cmd = "run";
         Cmd2 = "r";
         AvailableWhileRunning = false;
      }

      public override void PrintHelp()
      {
         base.PrintHelp();
         Console.Out.WriteLine("  Continues execution.");
      }

      protected override void Process(string[] args)
      {
         App.Debugger.Run();
      }
   }

   public class LuaDbgStepIntoCommand : Command
   {
      public LuaDbgStepIntoCommand(LuaDebugApp app)
         : base(app)
      {
         Cmd = "stepinto";
         Cmd2 = "si";
         AvailableWhileRunning = false;
      }

      public override void PrintHelp()
      {
         base.PrintHelp();
         Console.Out.WriteLine("  Executes the next line and steps into funtions if possible.");
      }

      protected override void Process(string[] args)
      {
         App.Debugger.StepInto();
      }
   }

   public class LuaDbgStepOverCommand : Command
   {
      public LuaDbgStepOverCommand(LuaDebugApp app)
         : base(app)
      {
         Cmd = "stepover";
         Cmd2 = "so";
         AvailableWhileRunning = false;
      }

      public override void PrintHelp()
      {
         base.PrintHelp();
         Console.Out.WriteLine("  Executes the next line.");
      }

      protected override void Process(string[] args)
      {
         App.Debugger.StepOver();
      }
   }

   public class LuaDbgStepOutCommand : Command
   {
      public LuaDbgStepOutCommand(LuaDebugApp app)
         : base(app)
      {
         Cmd = "stepout";
         Cmd2 = "st";
         AvailableWhileRunning = false;
      }

      public override void PrintHelp()
      {
         base.PrintHelp();
         Console.Out.WriteLine("  Executes until returning from this function.");
      }

      protected override void Process(string[] args)
      {
         App.Debugger.StepOut();
      }
   }

   public class LuaDbgCallStackCommand : Command
   {
      public LuaDbgCallStackCommand(LuaDebugApp app)
         : base(app)
      {
         Cmd = "callstack";
         Cmd2 = "cs";
         AvailableWhileRunning = false;
      }

      public override void PrintHelp()
      {
         base.PrintHelp();
         Console.Out.WriteLine("  Prints the current call stack.");
      }

      protected override void Process(string[] args)
      {
         Console.Out.WriteLine("<function>\t- <file>:<line>");
         CallStackEntry[] entries = App.Debugger.GetCallStack();
         foreach (var entry in entries)
         {
            Console.WriteLine(String.Format("{0}\t- {1}:{2}", entry.FunctionName, entry.FileName, entry.Line));
         }
      }
   }

   public class LuaDbgLocalVarsCommand : Command
   {
      public LuaDbgLocalVarsCommand(LuaDebugApp app)
         : base(app)
      {
         Cmd = "localvars";
         Cmd2 = "lv";
         AvailableWhileRunning = false;
      }

      public override void PrintHelp()
      {
         base.PrintHelp();
         Console.Out.WriteLine("  Prints the local vars.");
      }

      protected override void Process(string[] args)
      {
         Console.Out.WriteLine("<name>\t- <value> [<type>]");
         LuaVar[] vars = App.Debugger.GetLocalVars(App.LuaDebugAtStop);
         foreach (var var in vars)
         {
            if (var.Value != null)
            {
               Console.WriteLine(String.Format("{0}\t- {1} [{2}]", var.Name, var.Value, var.Value.GetType().FullName));
            }
            else
            {
               Console.WriteLine(String.Format("{0}\t- <null> [<null>]", var.Name));
            }
         }
      }
   }

   public class LuaDbgSetLocalVarCommand : Command
   {
      public LuaDbgSetLocalVarCommand(LuaDebugApp app)
         : base(app)
      {
         Cmd = "setlocalvar";
         Cmd2 = "slv";
         AvailableWhileRunning = false;
      }

      public override void PrintHelp()
      {
         base.PrintHelp();
         Console.Out.WriteLine("  Sets the value of a local var.");
         Console.Out.WriteLine("    setlocalvar <name> <type> <value>");
         Console.Out.WriteLine("    <name>: the name of the variable");
         Console.Out.WriteLine("    <type>: type of the value");
         Console.Out.WriteLine("            null: set to null (<value> is ignored)");
         Console.Out.WriteLine("            b: boolean (possible values: true, false)");
         Console.Out.WriteLine("            n: number");
         Console.Out.WriteLine("            s: string");
         Console.Out.WriteLine("    <value>: value, must match type");
         Console.Out.WriteLine("    slv someBoolVar b true");
         Console.Out.WriteLine("    slv someStringVar s \"hello there\"");
      }

      protected override void Process(string[] args)
      {
         if (args.Length < 2)
         {
            Console.Out.WriteLine("Argument <name> is missing");
         }
         else if (args.Length < 3)
         {
            Console.Out.WriteLine("Argument <type> is missing");
         }
         else
         {
            object newValue;
            try
            {
               if (String.Compare("null", args[2], StringComparison.OrdinalIgnoreCase) == 0)
               {
                  newValue = null;
               }
               else if (args.Length < 4)
               {
                  Console.Out.WriteLine("Argument <value> is missing");
                  return;
               }
               else if (String.Compare("b", args[2], StringComparison.OrdinalIgnoreCase) == 0)
               {
                  newValue = Boolean.Parse(args[3]);
               }
               else if (String.Compare("n", args[2], StringComparison.OrdinalIgnoreCase) == 0)
               {
                  newValue = Double.Parse(args[3]);
               }
               else if (String.Compare("s", args[2], StringComparison.OrdinalIgnoreCase) == 0)
               {
                  newValue = args[3];
               }
               else
               {
                  Console.Out.WriteLine("Argument 2: Invalid value");
                  return;
               }
            }
            catch (FormatException ex)
            {
               Console.Out.WriteLine("Argument 3: FormatException");
               Console.Out.WriteLine(ex.Message);
               return;
            }
            if (!App.Debugger.SetLocalVar(App.LuaDebugAtStop, args[1], newValue))
            {
               Console.Out.WriteLine("Argument 1: Local var not found");
            }
         }
      }
   }

   public class LuaDbgUpValuesCommand : Command
   {
      public LuaDbgUpValuesCommand(LuaDebugApp app)
         : base(app)
      {
         Cmd = "upvalues";
         Cmd2 = "uv";
         AvailableWhileRunning = false;
      }

      public override void PrintHelp()
      {
         base.PrintHelp();
         Console.Out.WriteLine("  Prints the up values.");
      }

      protected override void Process(string[] args)
      {
         Console.Out.WriteLine("<name>\t- <value> [<type>]");
         LuaVar[] values = App.Debugger.GetUpValues(1);
         foreach (var value in values)
         {
            if (value.Value != null)
            {
               Console.WriteLine(String.Format("{0}\t- {1} [{2}]", value.Name, value.Value, value.Value.GetType().FullName));
            }
            else
            {
               Console.WriteLine(String.Format("{0}\t- <null> [<null>]", value.Name));
            }
         }
      }
   }
}
