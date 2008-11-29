using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using LuaInterface;
using LuaInterface.Debugger;

namespace LuaDebuggerTest
{
   class Program
   {
      static void Main(string[] args)
      {
         LuaDebugApp app = new LuaDebugApp();
         app.Run(args);
      }
   }

   public class LuaDebugApp
   {
      private Lua m_Lua;
      public Lua Lua
      {
         get { return m_Lua; }
      }

      private LuaDebugger m_Debugger;
      public LuaDebugger Debugger
      {
         get { return m_Debugger; }
      }

      private bool m_DoExit = false;
      public bool DoExit
      {
         get { return m_DoExit; }
         internal set { m_DoExit = value; }
      }

      private List<Command> m_Commands = new List<Command>();
      public List<Command> Commands
      {
         get { return m_Commands; }
      }

      private LuaDebug m_LuaDebugAtStop;
      public LuaDebug LuaDebugAtStop
      {
         get { return m_LuaDebugAtStop; }
      }

      public LuaDebugApp()
      {
         m_Lua = new Lua();
         m_Lua.HookException += Lua_HookException;
         m_Debugger = new LuaDebugger(m_Lua);
         m_Debugger.FullTraceData += Debugger_FullTraceData;
         m_Debugger.Stoping += Debugger_Stoping;
         m_Debugger.WaitingForAction += Debugger_WaitingForAction;

         m_Debugger.Enabled = true;
      }

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
         m_LuaDebugAtStop = e.LuaDebug;
      }

      void Debugger_WaitingForAction(object sender, EventArgs e)
      {
         Console.Write(">");
         string cmd = Console.In.ReadLine();
         if (cmd != String.Empty)
         {
            ExecCommand(cmd);
         }
      }

      void Lua_HookException(object sender, HookExceptionEventArgs e)
      {
         Console.Out.WriteLine(String.Format("Unhandeld Exception in hook callback: {0}", e.Exception.GetType().Name));
         Console.Out.WriteLine(e.Exception.ToString());
      }

      void Debugger_FullTraceData(object sender, DebugHookEventArgs e)
      {
         Console.Out.Write(String.Format("FT {0} ", e.LuaDebug.eventCode.ToString()));
         Console.Out.WriteLine(e.LuaDebug.currentline);

         LuaDebug luaDebug = e.LuaDebug;
         if (m_Lua.GetInfo("nS", ref luaDebug) == 0)
         {
            Console.Out.WriteLine("   Lua.GetInfo failed!");
         }
         else
         {
            if (luaDebug.source.Length > 0 && luaDebug.source[0] == '@')
            {
               Console.Out.WriteLine(String.Format("   n:{0} nw:{1} w:{2} s:{3} ss:{4} ld:{5} lld:{6} cl:{7}",
                  luaDebug.name, luaDebug.namewhat,
                  luaDebug.what, luaDebug.source, luaDebug.shortsrc,
                  luaDebug.linedefined, luaDebug.lastlinedefined, luaDebug.currentline));
            }
         }
      }

      private void CreateCommands()
      {
         // enumerate through all types of this assembly
         // all sub classes of Command will be instanced and added to the m_Commands list
         Type[] types = Assembly.GetExecutingAssembly().GetTypes();
         foreach (var type in types)
         {
            if (type.IsSubclassOf(typeof(Command)))
            {
               m_Commands.Add(Assembly.GetExecutingAssembly().CreateInstance(type.FullName, false, BindingFlags.Default, null, new object[] {this}, null, null) as Command);
            }
         }
      }

      public void Run(string[] args)
      {
         CreateCommands();

         Console.Out.WriteLine("Lua Debugger test app");
         Console.Out.WriteLine();

         ExecCommand("?");

         foreach (var arg in args)
         {
            Console.Out.WriteLine(String.Format(">{0}", arg));
            ExecCommand(arg);
         }

         string cmd;
         do
         {
            Console.Out.Write(">");
            cmd = Console.In.ReadLine();
            if(cmd != String.Empty)
            {
               ExecCommand(cmd);
            }
         }
         while (!m_DoExit);

         Console.Out.WriteLine("bye");
      }

      private void ExecCommand(string cmd)
      {
         foreach (var command in m_Commands)
         {
            if (command.Exec(cmd))
            {
               return;
            }
         }
         Console.Out.WriteLine("Unknwon command");
      }
   }
}
