using System;
using System.Collections.Generic;
using System.Text;
using LuaInterface;

namespace LuaDebuggerTest
{
   public class LuaDoFileCommand : Command
   {
      public LuaDoFileCommand(LuaDebugApp app)
         : base(app)
      {
         Cmd = "dofile";
         Cmd2 = "df";
         AvailableWhileStoped = false;
      }

      public override void PrintHelp()
      {
         base.PrintHelp();
         Console.Out.WriteLine("  Runs a lua file:");
         Console.Out.WriteLine("    dofile <filename>");
      }

      protected override void Process(string[] args)
      {
         if (args.Length < 2)
         {
            Console.Out.WriteLine("Argument <filename> is missing");
         }
         else
         {
            try
            {
               App.Lua.DoFile(args[1]);
            }
            catch (LuaException ex)
            {
               Console.Out.WriteLine("LuaException:");
               Console.Out.WriteLine(ex.Message);
            }
         }
      }
   }

   public class LuaDoStringCommand : Command
   {
      public LuaDoStringCommand(LuaDebugApp app)
         : base(app)
      {
         Cmd = "dostring";
         Cmd2 = "ds";
         AvailableWhileStoped = false;
      }

      public override void PrintHelp()
      {
         base.PrintHelp();
         Console.Out.WriteLine("  Runs a lua string:");
         Console.Out.WriteLine("    dostring <lua code> [<lua code>]*");
      }

      protected override void Process(string[] args)
      {
         if (args.Length < 2)
         {
            Console.Out.WriteLine("Argument <lua code> is missing");
         }
         else
         {
            try
            {
               for (int n = 1; n < args.Length; ++n)
               {
                  App.Lua.DoString(args[n]);
               }
            }
            catch (LuaException ex)
            {
               Console.Out.WriteLine("LuaException:");
               Console.Out.WriteLine(ex.Message);
            }
         }
      }
   }
}
