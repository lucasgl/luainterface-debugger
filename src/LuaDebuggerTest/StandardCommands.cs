using System;
using System.Collections.Generic;
using System.Text;

namespace LuaDebuggerTest
{
   public abstract class Command
   {
      private string m_Cmd = String.Empty;
      public string Cmd
      {
         get { return m_Cmd; }
         protected set { m_Cmd = value; }
      }

      private string m_Cmd2 = String.Empty;
      public string Cmd2
      {
         get { return m_Cmd2; }
         protected set { m_Cmd2 = value; }
      }

      private LuaDebugApp m_App;
      public LuaDebugApp App
      {
         get { return m_App; }
      }

      private bool m_AvailableWhileRunning = true;
      public bool AvailableWhileRunning
      {
         get { return m_AvailableWhileRunning; }
         set { m_AvailableWhileRunning = value; }
      }

      private bool m_AvailableWhileStoped = true;
      public bool AvailableWhileStoped
      {
         get { return m_AvailableWhileStoped; }
         set { m_AvailableWhileStoped = value; }
      }

      public Command(LuaDebugApp app)
      {
         m_App = app;
      }

      public virtual void PrintHelp()
      {
         if (m_Cmd2 == String.Empty)
         {
            Console.Out.WriteLine(String.Format("{0}:", m_Cmd));
         }
         else
         {
            Console.Out.WriteLine(String.Format("{0} ({1}):", m_Cmd, m_Cmd2));
         }
      }

      public virtual bool CheckCmdMatch(string cmd)
      {
         int n = cmd.IndexOf(' ');
         if (n >= 0)
         {
            cmd = cmd.Substring(0, n);
         }
         return (String.Compare(m_Cmd, cmd, StringComparison.Ordinal) == 0)
            || (m_Cmd2 != String.Empty && String.Compare(m_Cmd2, cmd, StringComparison.Ordinal) == 0);
      }

      public virtual bool Exec(string cmd)
      {
         if (CheckCmdMatch(cmd))
         {
            if (m_App.Debugger.State == LuaInterface.Debugger.LuaDebuggerState.Stoped && !m_AvailableWhileStoped)
            {
               Console.Out.WriteLine("Command not available while debugger is stoped!");
            }
            else if (m_App.Debugger.State != LuaInterface.Debugger.LuaDebuggerState.Stoped && !m_AvailableWhileRunning)
            {
               Console.Out.WriteLine("Command not available while debugger is not stoped!");
            }
            else
            {
               try
               {
                  Process(ParseArgs(cmd));
               }
               catch (Exception ex)
               {
                  Console.Out.WriteLine(String.Format("Unhandled exception: {0}", ex.GetType().Name));
                  Console.Out.WriteLine(ex.Message);
               }
            }
            return true;
         }
         return false;
      }

      protected virtual string[] ParseArgs(string cmd)
      {
         List<string> args = new List<string>();

         bool inCaps = false;
         char startCap = '\"';
         int n = 0;
         StringBuilder arg = new StringBuilder();
         while (n < cmd.Length)
         {
            if (inCaps)
            {
               if (cmd[n] == startCap)
               {
                  args.Add(arg.ToString());
                  arg = new StringBuilder();
                  inCaps = false;
               }
               else
               {
                  arg.Append(cmd[n]);
               }
            }
            else
            {
               if (arg.Length == 0 && (cmd[n] == '\"' || cmd[n] == '\''))
               {
                  startCap = cmd[n];
                  inCaps = true;
               }
               else if (cmd[n] == ' ')
               {
                  if (arg.Length > 0)
                  {
                     args.Add(arg.ToString());
                     arg = new StringBuilder();
                  }
               }
               else
               {
                  arg.Append(cmd[n]);
               }
            }
            ++n;
         }
         if (arg.Length > 0)
         {
            args.Add(arg.ToString());
         }
         return args.ToArray();
      }

      protected abstract void Process(string[] args);
   }

   /// <summary>
   /// 
   /// </summary>
   public class HelpCommand : Command
   {
      public HelpCommand(LuaDebugApp app)
         : base(app)
      {
         Cmd = "help";
         Cmd2 = "?";
      }

      public override void PrintHelp()
      {
         base.PrintHelp();
         Console.Out.WriteLine("  Shows this help info.");
      }

      protected override void Process(string[] args)
      {
         foreach (var command in App.Commands)
         {
            command.PrintHelp();
         }
         Console.Out.WriteLine();
      }
   }

   /// <summary>
   /// 
   /// </summary>
   public class ExitCommand : Command
   {
      public ExitCommand(LuaDebugApp app)
         : base(app)
      {
         Cmd = "exit";
         Cmd2 = "e";
         AvailableWhileStoped = false;
      }

      public override void PrintHelp()
      {
         base.PrintHelp();
         Console.Out.WriteLine("  Exits the lua debug test app.");
      }

      protected override void Process(string[] args)
      {
         App.DoExit = true;
      }
   }
}
