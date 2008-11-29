using System;
using System.Collections.Generic;
using System.Text;

namespace LuaInterface.Debugger
{
   /// <summary>
   /// Interface for LuaDebugBreakpoint factories.
   /// </summary>
   public interface ILuaDebugBreakpointFactory
   {
      /// <summary>
      /// Creates a new LuaDebugBreakpoint.
      /// </summary>
      /// <param name="file">File</param>
      /// <param name="line">Line number</param>
      /// <returns>Returns the new breakpoint.</returns>
      LuaDebugBreakpoint CreateBreakpoint(LuaDebugFile file, int line);
   }

   /// <summary>
   /// Default implementation for breakpoints.
   /// </summary>
   public class LuaDebugBreakpoint
   {
      /// <summary>
      /// Gets the file which containes the breakpoint.
      /// </summary>
      public LuaDebugFile File { get; private set; }

      /// <summary>
      /// Gets or sets the Line.
      /// </summary>
      public int Line { get; set; }

      /// <summary>
      /// Gets or sets if the breakpoint is enabled.
      /// </summary>
      public bool Enabled { get; set; }

      /// <summary>
      /// Constructor.
      /// </summary>
      /// <param name="file">File</param>
      /// <param name="line">Line</param>
      public LuaDebugBreakpoint(LuaDebugFile file, int line)
      {
         File = file;
         Line = line;
         Enabled = true;
      }
   }

   /// <summary>
   /// Container for LuaDebugBreakpoints.
   /// </summary>
   public class LuaDebugBreakpointContainer : IEnumerable<LuaDebugBreakpoint>
   {
      /// <summary>
      /// List with all breakpoints.
      /// </summary>
      private List<LuaDebugBreakpoint> m_Items = new List<LuaDebugBreakpoint>();

      /// <summary>
      /// Constructor
      /// </summary>
      internal LuaDebugBreakpointContainer()
      {

      }

      /// <summary>
      /// Adds a breakpoint.
      /// </summary>
      /// <param name="item">Breakpoint.</param>
      internal void Add(LuaDebugBreakpoint item)
      {
         m_Items.Add(item);
      }

      /// <summary>
      /// Removes a breakpoint.
      /// </summary>
      /// <param name="item">Breakpoint.</param>
      public void Remove(LuaDebugBreakpoint item)
      {
         m_Items.Remove(item);
      }

      /// <summary>
      /// Removes breakpoint at a given index.
      /// </summary>
      /// <param name="index">Index</param>
      internal void RemoveAt(int index)
      {
         m_Items.RemoveAt(index);
      }

      /// <summary>
      /// Removes all breakpoints.
      /// </summary>
      public void Clear()
      {
         m_Items.Clear();
      }

      /// <summary>
      /// Gets the number of breakpoints.
      /// </summary>
      public int Count
      {
         get { return m_Items.Count; }
      }

      /// <summary>
      /// Gets breakpoints by index.
      /// </summary>
      /// <param name="index">Index</param>
      /// <returns>Returns a breakpoint.</returns>
      public LuaDebugBreakpoint this[int index]
      {
         get { return m_Items[index]; }
      }

      #region IEnumerable<LuaDebugBreakpoint> Members

      /// <summary>
      /// Returns the enumerator for the breakpoints.
      /// </summary>
      /// <returns>Returns the enumerator for the breakpoints.</returns>
      public IEnumerator<LuaDebugBreakpoint> GetEnumerator()
      {
         return m_Items.GetEnumerator();
      }

      #endregion

      #region IEnumerable Members

      /// <summary>
      /// Returns the enumerator for the breakpoints.
      /// </summary>
      /// <returns>Returns the enumerator for the breakpoints.</returns>
      System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
      {
         return m_Items.GetEnumerator();
      }

      #endregion
   }
}
