using System;
using System.Collections.Generic;
using System.Text;
using LuaInterface;

namespace LuaInterface.Debugger
{
   /// <summary>
   /// Debugger extension for LuaInterface
   /// </summary>
   /// <remarks>
   /// Implements also the default factories for file- and breakpoint- objects.
   /// </remarks>
   public class LuaDebugger : ILuaDebugFileFactory, ILuaDebugBreakpointFactory
   {
      /// <summary>
      /// Gets the lua interpreter.
      /// </summary>
      public Lua Lua
      {
         get { return m_Lua; }
      }
      private Lua m_Lua;

      /// <summary>
      /// Gets and sets the LuaDebugFile factory.
      /// </summary>
      /// <remarks>
      /// The default file factory is LuaDebugger itself.
      /// </remarks>
      public ILuaDebugFileFactory FileFactory
      {
         get { return m_FileFactory; }
         set { m_FileFactory = value; }
      }
      private ILuaDebugFileFactory m_FileFactory;

      /// <summary>
      /// Gets and sets the LuaDebugBreakpoint factory.
      /// </summary>
      /// <remarks>
      /// The default breakpoint factory is LuaDebugger itself.
      /// </remarks>
      public ILuaDebugBreakpointFactory BreakpointFactory
      {
         get { return m_BreakpointFactory; }
         set { m_BreakpointFactory = value; }
      }
      private ILuaDebugBreakpointFactory m_BreakpointFactory;

      /// <summary>
      /// Gets the container with the files.
      /// </summary>
      public LuaDebugFileContainer Files
      {
         get { return m_Files; }
      }
      private LuaDebugFileContainer m_Files = new LuaDebugFileContainer();

      /// <summary>
      /// Gets or sets if the full tryce feature is enabled.
      /// </summary>
      /// <remarks>
      /// The FullTrace property controls if the FullTraceData event is called.
      /// </remarks>
      public bool FullTrace
      {
         get { return m_FullTrace; }
         set { m_FullTrace = value; }
      }
      private bool m_FullTrace = false;

      /// <summary>
      /// true if in debug hook funtion (simple locking mechanism)
      /// </summary>
      private bool m_InDebugHook = false;

      /// <summary>
      /// current action of the debugger
      /// </summary>
      private DebuggerActions m_Action = DebuggerActions.Run;

      /// <summary>
      /// current stack level (absolute value is not important)
      /// </summary>
      private int m_StackLevel = 0;

      /// <summary>
      /// Stack level to use for stepping (step over and step out)
      /// </summary> 
      private int m_StepStackLevel = 0;

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="lua">Lua interpreter to use.</param>
      public LuaDebugger(Lua lua)
      {
         m_Lua = lua;
         m_FileFactory = this as ILuaDebugFileFactory;
         m_BreakpointFactory = this as ILuaDebugBreakpointFactory;
      }

      /// <summary>
      /// Gets and sets if the debugger is enabled.
      /// </summary>
      /// <remarks>
      /// If the debugger is disabled, the lua debug hook funktion is removed.
      /// By this you can avoid any perfomance issues while no debugging is needed.
      /// Enabled sets the State property to Running (true) or Disabled (false).
      /// You can not disable the debugger while it is stopped.
      /// </remarks>
      public bool Enabled
      {
         get { return m_State != LuaDebuggerState.Disabled; }
         set
         {
            if (value)
            {
               State = LuaDebuggerState.Running;
            }
            else
            {
               State = LuaDebuggerState.Disabled;
            }
         }
      }

      /// <summary>
      /// Lua debug hook event handler.
      /// </summary>
      /// <param name="sender">Sender of the event</param>
      /// <param name="e">Event args</param>
      /// <remarks>
      /// This is the main debug handler.
      /// This event calls the FullTraceData event (if enabled),
      /// Stoping event and the WaitingForAction event (when execution is stoped).
      /// </remarks>
      private void Lua_DebugHook(object sender, DebugHookEventArgs e)
      {
         if (!m_InDebugHook)
         {
            m_InDebugHook = true;
            try
            {
               if (m_FullTrace)
               {
                  OnFullTraceData(e);
               }

               if (m_State != LuaDebuggerState.Disabled)
               {
                  if (e.LuaDebug.eventCode == EventCodes.LUA_HOOKCALL)
                  {
                     ++m_StackLevel;
                  }
                  else if (e.LuaDebug.eventCode == EventCodes.LUA_HOOKRET
                     || e.LuaDebug.eventCode == EventCodes.LUA_HOOKTAILRET)
                  {
                     --m_StackLevel;
                  }

                  if (e.LuaDebug.eventCode == EventCodes.LUA_HOOKCALL || e.LuaDebug.eventCode == EventCodes.LUA_HOOKLINE)
                  {
                     LuaDebug luaDebug = e.LuaDebug;
                     m_Lua.GetInfo("nS", ref luaDebug);
                     int line = luaDebug.eventCode == EventCodes.LUA_HOOKCALL ? luaDebug.linedefined : luaDebug.currentline;
                     if (luaDebug.source.Length > 0 && luaDebug.source[0] == '@')
                     {
                        LuaDebugBreakpoint breakpoint;
                        switch (m_Action)
                        {
                           case DebuggerActions.Run:
                              // checked for breakpoints
                              breakpoint = GetBreakpoint(luaDebug.shortsrc, line);
                              if (breakpoint != null && breakpoint.Enabled)
                              {
                                 StopExecution(luaDebug, m_Action, breakpoint);
                              }
                              break;

                           case DebuggerActions.Stop:
                           case DebuggerActions.StepInto:
                              StopExecution(luaDebug, m_Action, null);
                              break;

                           case DebuggerActions.StepOver:
                           case DebuggerActions.StepOut:
                              // checked for breakpoints or if stack level is ready for stopping
                              if (m_StackLevel <= m_StepStackLevel)
                              {
                                 StopExecution(luaDebug, m_Action, null);
                              }
                              else
                              {
                                 breakpoint = GetBreakpoint(luaDebug.shortsrc, line);
                                 if (breakpoint != null && breakpoint.Enabled)
                                 {
                                    StopExecution(luaDebug, m_Action, breakpoint);
                                 }
                              }
                              break;

                           default:
                              break;
                        }
                     }
                  }
               }
            }
            finally
            {
               m_InDebugHook = false;
            }
         }
      }

      /// <summary>
      /// Stops execution.
      /// </summary>
      /// <param name="luaDebug">LuaDebug from debug hook.</param>
      /// <param name="action">Current Debugger Action.</param>
      /// <param name="breakpoint">Brekpoint. Can be null.</param>
      /// <remarks>
      /// The WaitingForAction event is called as long as State == Stoped.
      /// </remarks>
      private void StopExecution(LuaDebug luaDebug, DebuggerActions action, LuaDebugBreakpoint breakpoint)
      {
         m_State = LuaDebuggerState.Stoped;
         try
         {
            OnStopping(new StopingEventArgs(
               luaDebug, luaDebug.shortsrc, 
               luaDebug.eventCode == EventCodes.LUA_HOOKCALL ? luaDebug.linedefined : luaDebug.currentline,
               action,
               breakpoint));
            do
            {
               OnWaitingForAction(new EventArgs());
            }
            while (m_State == LuaDebuggerState.Stoped);
         }
         finally
         {
            m_State = LuaDebuggerState.Running;
         }
      }

      /// <summary>
      /// Event that is raised when the debugger has stoped and a action is required.
      /// </summary>
      /// <remarks>
      /// The event gets called as long as the State == Stoped.
      /// To change the state call one of the following methods:
      /// Run, StepInto, StepOver, StepOut
      /// </remarks>
      public event EventHandler<EventArgs> WaitingForAction;
      private void OnWaitingForAction(EventArgs e)
      {
         EventHandler<EventArgs> temp = WaitingForAction;
         if (temp != null)
         {
            temp(this, e);
         }
      }

      /// <summary>
      /// Event gest called when the debugger stops execution.
      /// </summary>
      public event EventHandler<StopingEventArgs> Stoping;
      private void OnStopping(StopingEventArgs e)
      {
         EventHandler<StopingEventArgs> temp = Stoping;
         if (temp != null)
         {
            temp(this, e);
         }
      }

      /// <summary>
      /// Event gest called on every debug hook call.
      /// </summary>
      /// <remarks>
      /// This event can be enabled or disabeld by the FullTrace property.
      /// By default it's disabled.
      /// </remarks>
      public event EventHandler<DebugHookEventArgs> FullTraceData;
      private void OnFullTraceData(DebugHookEventArgs e)
      {
         EventHandler<DebugHookEventArgs> temp = FullTraceData;
         if (temp != null)
         {
            temp(this, e);
         }
      }

      /// <summary>
      /// Gets or sets the state of the debugger.
      /// </summary>
      public LuaDebuggerState State
      {
         get { return m_State; }
         set
         {
            switch (value)
            {
               case LuaDebuggerState.Disabled:
                  m_StackLevel = 0;
                  if (m_State == LuaDebuggerState.Stoped)
                  {
                     Run();
                  }
                  if(m_State != LuaDebuggerState.Disabled)
                  {
                     m_Lua.RemoveDebugHook();
                     m_Lua.DebugHook -= Lua_DebugHook;
                  }
                  m_State = LuaDebuggerState.Disabled;
                  break;
               
               case LuaDebuggerState.Running:
                  if (m_State == LuaDebuggerState.Disabled)
                  {
                     m_Lua.DebugHook += Lua_DebugHook;
                     m_Lua.SetDebugHook(EventMasks.LUA_MASKALL, 0);
                     m_State = LuaDebuggerState.Running;
                  }
                  else if (m_State == LuaDebuggerState.Stoped)
                  {
                     Run();
                  }
                  break;
               
               case LuaDebuggerState.Stoped:
                  throw new InvalidOperationException("Cant set state to Stoped");

               default:
                  throw new ArgumentException("Unknown value for state");
            }
         }
      }
      private LuaDebuggerState m_State = LuaDebuggerState.Disabled;

      /// <summary>
      /// Continues execution until the next breakpoint.
      /// </summary>
      public void Run()
      {
         if (m_State == LuaDebuggerState.Stoped)
         {
            m_Action = DebuggerActions.Run;
            m_State = LuaDebuggerState.Running;
         }
      }

      /// <summary>
      /// Stops execution on the next executed line of code.
      /// </summary>
      public void Stop()
      {
         m_Action = DebuggerActions.Stop;
      }

      /// <summary>
      /// Steps into the function if possible.
      /// </summary>
      public void StepInto()
      {
         if (m_State == LuaDebuggerState.Stoped)
         {
            m_Action = DebuggerActions.StepInto;
            m_State = LuaDebuggerState.Running;
         }
      }

      /// <summary>
      /// Executes the next line of code.
      /// </summary>
      public void StepOver()
      {
         if (m_State == LuaDebuggerState.Stoped)
         {
            m_Action = DebuggerActions.StepOver;
            m_StepStackLevel = m_StackLevel;
            m_State = LuaDebuggerState.Running;
         }
      }

      /// <summary>
      /// Steps out of the current function.
      /// </summary>
      public void StepOut()
      {
         if (m_State == LuaDebuggerState.Stoped)
         {
            m_Action = DebuggerActions.StepOut;
            m_StepStackLevel = m_StackLevel - 1;
            m_State = LuaDebuggerState.Running;
         }
      }

      /// <summary>
      /// Gets the current callstack.
      /// </summary>
      /// <returns>
      /// Returns the current callstack.
      /// If the debugger is not stoped then a empty array is returned.
      /// </returns>
      public CallStackEntry[] GetCallStack()
      {
         List<CallStackEntry> entries = new List<CallStackEntry>();

         if (m_State == LuaDebuggerState.Stoped)
         {
            int level = 0;
            LuaDebug luaDebug;
            while (m_Lua.GetStack(level, out luaDebug))
            {
               m_Lua.GetInfo("nSl", ref luaDebug);
               entries.Add(new CallStackEntry(luaDebug));
               ++level;
            }
         }

         return entries.ToArray();
      }

      /// <summary>
      /// Gets the local variables and their values.
      /// </summary>
      /// <param name="luaDebug">Current LuaDebug structure.</param>
      /// <returns>
      /// Returns a list with all local variables and their values.
      /// If the debuggger is not stoped, a empty array is returned.
      /// </returns>
      public LuaVar[] GetLocalVars(LuaDebug luaDebug)
      {
         List<LuaVar> vars = new List<LuaVar>();

         if (m_State == LuaDebuggerState.Stoped)
         {
            int index = 1;
            string name = m_Lua.GetLocal(luaDebug, index);
            while (name != null)
            {
               vars.Add(new LuaVar(index, name, m_Lua.Pop()));

               ++index;
               name = m_Lua.GetLocal(luaDebug, index);
            }
         }
         return vars.ToArray();
      }

      /// <summary>
      /// Sets a new value for a local variable.
      /// </summary>
      /// <param name="luaDebug">Current LuaDebug structure.</param>
      /// <param name="var">Variable that was returned by GetLocalVars.</param>
      /// <param name="newValue">New value. The type don't have to match.</param>
      public void SetLocalVar(LuaDebug luaDebug, ref LuaVar var, object newValue)
      {
         if (m_State == LuaDebuggerState.Stoped)
         {
            var.Value = newValue;
            m_Lua.Push(newValue);
            m_Lua.SetLocal(luaDebug, var.Index);
         }
      }

      /// <summary>
      /// Sets a new value for a local variable.
      /// </summary>
      /// <param name="luaDebug">Current LuaDebug structure.</param>
      /// <param name="varName">Name of the variable.</param>
      /// <param name="newValue">New value. The type don't have to match.</param>
      public bool SetLocalVar(LuaDebug luaDebug, string varName, object newValue)
      {
         if (m_State == LuaDebuggerState.Stoped)
         {
            LuaVar[] vars = GetLocalVars(luaDebug);
            for (int n = 0; n < vars.Length; ++n) // no foreach bc var is used as ref parameter!
            {
               var var = vars[n];
               if (String.Compare(varName, var.Name, StringComparison.OrdinalIgnoreCase) == 0)
               {
                  SetLocalVar(luaDebug, ref var, newValue);
                  return true;
               }
            }
         }
         return false;
      }

      /// <summary>
      /// Gets all upvalues.
      /// </summary>
      /// <param name="functionIndex">?</param>
      /// <returns>Array with all up values. Array can be empty.</returns>
      /// <remarks>
      /// Not tested.
      /// </remarks>
      public LuaVar[] GetUpValues(int functionIndex)
      {
         List<LuaVar> values = new List<LuaVar>();

         if (m_State == LuaDebuggerState.Stoped)
         {
            int index = 1;
            string name = m_Lua.GetUpValue(functionIndex, index);
            while (name != null)
            {
               values.Add(new LuaVar(index, name, m_Lua.Pop()));

               ++index;
               name = m_Lua.GetUpValue(functionIndex, index);
            }
         }
         return values.ToArray();
      }

      /// <summary>
      /// Sets a new value for a upvalue.
      /// </summary>
      /// <param name="functionIndex">?</param>
      /// <param name="var">Variable that was returned by GetUpValues.</param>
      /// <param name="newValue">New value. The type don't have to match.</param>
      /// <remarks>
      /// Not tested.
      /// </remarks>
      public void SetUpValue(int functionIndex, ref LuaVar var, object newValue)
      {
         if (m_State == LuaDebuggerState.Stoped)
         {
            var.Value = newValue;
            m_Lua.Push(newValue);
            m_Lua.SetUpValue(functionIndex, var.Index);
         }
      }

      /// <summary>
      /// Sets a new value for a upvalue.
      /// </summary>
      /// <param name="functionIndex">?</param>
      /// <param name="varName">Name of the value.</param>
      /// <param name="newValue">New value. The type don't have to match.</param>
      /// <remarks>
      /// Not tested.
      /// </remarks>
      public bool SetUpValue(int functionIndex, string varName, object newValue)
      {
         if (m_State == LuaDebuggerState.Stoped)
         {
            LuaVar[] values = GetUpValues(functionIndex);
            for (int n = 0; n < values.Length; ++n) // no foreach bc value is used as ref parameter!
            {
               var value = values[n];
               if (String.Compare(varName, value.Name, StringComparison.OrdinalIgnoreCase) == 0)
               {
                  SetUpValue(functionIndex, ref value, newValue);
                  return true;
               }
            }
         }
         return false;
      }

      /// <summary>
      /// Adds a file.
      /// </summary>
      /// <param name="fileName">Filename</param>
      /// <returns>
      /// Returns the added file. If a file with the given name already exists,
      /// the existing file is returned.
      /// </returns>
      public LuaDebugFile AddFile(string fileName)
      {
         LuaDebugFile file = GetFile(fileName);
         if (file == null)
         {
            file = m_FileFactory.CreateFile(this, fileName);
            m_Files.Add(file);
         }
         return file;
      }

      /// <summary>
      /// Gets a file
      /// </summary>
      /// <param name="fileName">Filename</param>
      /// <returns>
      /// Returns the file with the given name.
      /// If the file does not exist, null is returned.
      /// </returns>
      public LuaDebugFile GetFile(string fileName)
      {
         foreach (var file in m_Files)
         {
            if (String.Compare(fileName, file.FileName, StringComparison.OrdinalIgnoreCase) == 0)
            {
               return file;
            }
         }
         return null;
      }

      /// <summary>
      /// Gets a breakpoint.
      /// </summary>
      /// <param name="fileName">Filename</param>
      /// <param name="line">Line</param>
      /// <returns>
      /// Returns the breakpoint of a file at a given line.
      /// If the file or breakpoint des not exist, then null is returned.
      /// </returns>
      public LuaDebugBreakpoint GetBreakpoint(string fileName, int line)
      {
         LuaDebugFile file = GetFile(fileName);
         if (file != null)
         {
            return file.GetBreakpoint(line);
         }
         return null;
      }

      /// <summary>
      /// Adds a new breakpoint.
      /// </summary>
      /// <param name="fileName">Filename</param>
      /// <param name="line">Line</param>
      /// <returns>
      /// Returns the new breakpoint.
      /// If the breakpoint already exists, then the existing breakpoint is returned.
      /// </returns>
      public LuaDebugBreakpoint AddBreakpoint(string fileName, int line)
      {
         LuaDebugFile file = GetFile(fileName);
         if (file == null)
         {
            file = AddFile(fileName);
         }
         LuaDebugBreakpoint breakpoint = file.GetBreakpoint(line);
         if (breakpoint == null)
         {
            breakpoint = file.AddBreakpoint(line);
         }
         return breakpoint;
      }

      /// <summary>
      /// Removes a breakpoint.
      /// </summary>
      /// <param name="fileName">Filename</param>
      /// <param name="line">Line</param>
      /// <remarks>
      /// If no breakpoint exists in the file at the given line, then nothing happens.
      /// </remarks>
      public void RemoveBreakpoint(string fileName, int line)
      {
         LuaDebugFile file = GetFile(fileName);
         if (file != null)
         {
            file.RemoveBreakpoint(line);
         }
      }

      #region ILuaDebugFileFactory Members

      /// <summary>
      /// Creates the default implementation of the LuaDebugFile.
      /// </summary>
      /// <param name="debugger">Debugger.</param>
      /// <param name="fileName">Filename.</param>
      /// <returns>Returns the new LuaDebugFile.</returns>
      LuaDebugFile ILuaDebugFileFactory.CreateFile(LuaDebugger debugger, string fileName)
      {
         return new LuaDebugFile(debugger, fileName);
      }

      #endregion

      #region ILuaDebugBreakpointFactory Members

      /// <summary>
      /// Creates the default implementation of the LuaDebugBreakpoint.
      /// </summary>
      /// <param name="file">File</param>
      /// <param name="line">Line</param>
      /// <returns>Returns the new LuaDebugBreakpoint.</returns>
      LuaDebugBreakpoint ILuaDebugBreakpointFactory.CreateBreakpoint(LuaDebugFile file, int line)
      {
         return new LuaDebugBreakpoint(file, line);
      }

      #endregion
   }

   /// <summary>
   /// Structure that represents a Lua variable and it's value.
   /// </summary>
   public struct LuaVar
   {
      /// <summary>
      /// Get the index of the variable. (Mainly for internal use)
      /// </summary>
      /// <remarks>Index can be for example the index on the stack.</remarks>
      public int Index
      {
         get { return m_Index; }
      }
      private int m_Index;

      /// <summary>
      /// Gets the name of the variable.
      /// </summary>
      public string Name
      {
         get { return m_Name; }
      }
      private string m_Name;

      /// <summary>
      /// Gets the value of the variable.
      /// </summary>
      public object Value
      {
         get { return m_Value; }
         internal set { m_Value = value; }
      }
      private object m_Value;

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="index">Index</param>
      /// <param name="name">Name</param>
      /// <param name="value">Value</param>
      internal LuaVar(int index, string name, object value)
      {
         m_Index = index;
         m_Name = name;
         m_Value = value;
      }
   }

   /// <summary>
   /// Structure representing a callstack entry (level).
   /// </summary>
   public struct CallStackEntry
   {
      /// <summary>
      /// Gets the LuaDebug structure for the entry.
      /// </summary>
      public LuaDebug LuaDebug
      {
         get { return m_LuaDebug; }
      }
      private LuaDebug m_LuaDebug;

      /// <summary>
      /// Gets the function name.
      /// </summary>
      public string FunctionName
      {
         get { return m_LuaDebug.name; }
      }

      /// <summary>
      /// Gets the filename.
      /// </summary>
      public string FileName
      {
         get { return m_LuaDebug.shortsrc; }
      }

      /// <summary>
      /// Gets the line number.
      /// </summary>
      public int Line
      {
         get { return m_LuaDebug.currentline; }
      }

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="luaDebug">LuaDebug structure for the entry.</param>
      internal CallStackEntry(LuaDebug luaDebug)
      {
         m_LuaDebug = luaDebug;
      }
   }

   /// <summary>
   /// Event args for the Stoping event.
   /// </summary>
   public class StopingEventArgs : EventArgs
   {
      /// <summary>
      /// Gets the LuaDebug structure.
      /// </summary>
      public LuaDebug LuaDebug { get; private set; }

      /// <summary>
      /// Gets the filename.
      /// </summary>
      public string FileName { get; private set; }

      /// <summary>
      /// Gets the line number.
      /// </summary>
      public int Line { get; private set; }

      /// <summary>
      /// Gets the breakpoint (can be null).
      /// </summary>
      public LuaDebugBreakpoint Breakpoint { get; private set; }

      /// <summary>
      /// Gets the debug action which is responible for Stopping.
      /// </summary>
      /// <remarks>
      /// If Breakpoint property is set, then the breakpoint is responsible for stoping.
      /// </remarks>
      public DebuggerActions Action { get; private set; }

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="luaDebug">Lua debug structure.</param>
      /// <param name="fileName">Filename</param>
      /// <param name="line">Line number</param>
      /// <param name="action">Action</param>
      /// <param name="breakpoint">Brewakpoint</param>
      public StopingEventArgs(LuaDebug luaDebug, string fileName, int line, DebuggerActions action, LuaDebugBreakpoint breakpoint)
      {
         LuaDebug = luaDebug;
         FileName = fileName;
         Line = line;
         Action = action;
         Breakpoint = breakpoint;
      }
   }

   /// <summary>
   /// Lua debugger states.
   /// </summary>
   public enum LuaDebuggerState
   {
      /// <summary>
      /// Debugger is disabled.
      /// </summary>
      Disabled,

      /// <summary>
      /// Debugger is running.
      /// </summary>
      Running,

      /// <summary>
      /// Debugger is stoped.
      /// </summary>
      Stoped
   }

   /// <summary>
   /// Actions for debugger.
   /// </summary>
   public enum DebuggerActions
   {
      /// <summary>
      /// Run. Continues execution.
      /// </summary>
      Run,

      /// <summary>
      /// Stop execution .
      /// </summary>
      Stop,

      /// <summary>
      /// Step into function.
      /// </summary>
      StepInto,

      /// <summary>
      /// Step over function.
      /// </summary>
      StepOver,

      /// <summary>
      /// Step out of function.
      /// </summary>
      StepOut
   }
}
