using System;
using System.Collections.Generic;
using System.Text;

namespace LuaInterface.Debugger
{
   /// <summary>
   /// Interface for LuaDebugFile factories.
   /// </summary>
   public interface ILuaDebugFileFactory
   {
      /// <summary>
      /// Creates a new LuaDebugFile.
      /// </summary>
      /// <param name="debugger">Debugger.</param>
      /// <param name="fileName">Filename.</param>
      /// <returns>Returns a new file.</returns>
      LuaDebugFile CreateFile(LuaDebugger debugger, string fileName);
   }

   /// <summary>
   /// Default implementation for files.
   /// </summary>
   public class LuaDebugFile
   {
      /// <summary>
      /// Gets the debugger.
      /// </summary>
      public LuaDebugger Debugger { get; private set; }

      /// <summary>
      /// Gets the filename of the file.
      /// </summary>
      public string FileName { get; private set; }

      /// <summary>
      /// Gets the breakpoints of this file.
      /// </summary>
      public LuaDebugBreakpointContainer Breakpoints
      {
         get { return m_Breakpoints; }
      }
      private LuaDebugBreakpointContainer m_Breakpoints = new LuaDebugBreakpointContainer();

      /// <summary>
      /// Constructor
      /// </summary>
      /// <param name="debugger">Debugger</param>
      /// <param name="fileName">Filename</param>
      public LuaDebugFile(LuaDebugger debugger, string fileName)
      {
         Debugger = debugger;
         FileName = fileName;
      }

      /// <summary>
      /// Adds a new breapoint.
      /// </summary>
      /// <param name="line">Line number.</param>
      /// <returns>
      /// Returns the new breakpoints.
      /// If the breakpoint at that line already exists, then the existing breakpoint is returned.
      /// </returns>
      public LuaDebugBreakpoint AddBreakpoint(int line)
      {
         LuaDebugBreakpoint breakpoint = GetBreakpoint(line);
         if (breakpoint != null)
         {
            breakpoint.Enabled = true;
         }
         else
         {
            breakpoint = Debugger.BreakpointFactory.CreateBreakpoint(this, line);
            m_Breakpoints.Add(breakpoint);
         }
         return breakpoint;
      }

      /// <summary>
      /// Remves a breakpoint.
      /// </summary>
      /// <param name="line">Line number</param>
      /// <remarks>
      /// If no breakpoint exists at the given line, then nothing happens.
      /// </remarks>
      public void RemoveBreakpoint(int line)
      {
         RemoveBreakpoint(GetBreakpoint(line));
      }

      /// <summary>
      /// Removes a breakpoint.
      /// </summary>
      /// <param name="breakpoint">Breakpoint</param>
      public void RemoveBreakpoint(LuaDebugBreakpoint breakpoint)
      {
         m_Breakpoints.Remove(breakpoint);
      }

      /// <summary>
      /// Gets a breakpoint at a given line.
      /// </summary>
      /// <param name="line">Line</param>
      /// <returns>
      /// Returns the breakpoint at the given line.
      /// If no breakpoint exists at the line, then null is returned.
      /// </returns>
      public LuaDebugBreakpoint GetBreakpoint(int line)
      {
         foreach (var breakpoint in m_Breakpoints)
         {
            if (breakpoint.Line == line)
            {
               return breakpoint;
            }
         }
         return null;
      }

      /// <summary>
      /// Toggels the breakpoint at a given line.
      /// </summary>
      /// <param name="line">Line number.</param>
      /// <returns>
      /// If the breakpoint was created, then the breakpoint is returned.
      /// If the breakpoint was removen, then null is returned.
      /// </returns>
      public LuaDebugBreakpoint ToggleBreakpoint(int line)
      {
         LuaDebugBreakpoint breakpoint = GetBreakpoint(line);
         if (breakpoint != null)
         {
            RemoveBreakpoint(breakpoint);
            return null;
         }
         else
         {
            return AddBreakpoint(line);
         }
      }
   }

   /// <summary>
   /// Container for LuaDebugFiles.
   /// </summary>
   public class LuaDebugFileContainer : IEnumerable<LuaDebugFile>
   {
      /// <summary>
      /// List with all breakpoints.
      /// </summary>
      private List<LuaDebugFile> m_Items = new List<LuaDebugFile>();

      /// <summary>
      /// Constructor.
      /// </summary>
      internal LuaDebugFileContainer()
      {

      }

      /// <summary>
      /// Adds a file.
      /// </summary>
      /// <param name="item">File.</param>
      internal void Add(LuaDebugFile item)
      {
         m_Items.Add(item);
      }

      /// <summary>
      /// Removes a file.
      /// </summary>
      /// <param name="item">File</param>
      internal void Remove(LuaDebugFile item)
      {
         m_Items.Remove(item);
      }

      /// <summary>
      /// Removes a file at a given index.
      /// </summary>
      /// <param name="index">Index.</param>
      internal void RemoveAt(int index)
      {
         m_Items.RemoveAt(index);
      }

      /// <summary>
      /// Removes all files.
      /// </summary>
      internal void Clear()
      {
         m_Items.Clear();
      }

      /// <summary>
      /// Gets the number of files.
      /// </summary>
      public int Count
      {
         get { return m_Items.Count; }
      }

      /// <summary>
      /// Gets file by index
      /// </summary>
      /// <param name="index">Index</param>
      /// <returns>Returns a file.</returns>
      public LuaDebugFile this[int index]
      {
         get { return m_Items[index]; }
      }

      #region IEnumerable<LuaDebugFile> Members

      /// <summary>
      /// Returns the enumerator for the files.
      /// </summary>
      /// <returns>Returns the enumerator for the files.</returns>
      public IEnumerator<LuaDebugFile> GetEnumerator()
      {
         return m_Items.GetEnumerator();
      }

      #endregion

      #region IEnumerable Members

      /// <summary>
      /// Returns the enumerator for the files.
      /// </summary>
      /// <returns>Returns the enumerator for the files.</returns>
      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
         return m_Items.GetEnumerator();
      }

      #endregion
   }
}
