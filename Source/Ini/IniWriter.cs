#region Copyright
//
// Nini Configuration Project.
// Copyright (C) 2006 Brent R. Matzelle.  All rights reserved.
//
// This software is published under the terms of the MIT X11 license, a copy of 
// which has been included with this distribution in the LICENSE.txt file.
// 
#endregion

using System;
using System.IO;
using System.Text;

namespace Nini.Ini
{
	#region IniWriteState enumeration
	
	public enum IniWriteState : int
	{
		
		Start,
		
		BeforeFirstSection,
		
		Section,
		
		Closed
	};
	#endregion

	
	public class IniWriter : IDisposable
	{
		#region Private variables
		int indentation = 0;
		bool useValueQuotes = false;
		IniWriteState writeState = IniWriteState.Start;
		char commentDelimiter = ';';
		char assignDelimiter = '=';
		TextWriter textWriter = null;
		string eol = "\r\n";
		StringBuilder indentationBuffer = new StringBuilder ();
		Stream baseStream = null;
		bool disposed = false;
		#endregion
		
		#region Public properties
		
		public int Indentation
		{
			get { return indentation; }
			set
			{
				if (value < 0)
					throw new ArgumentException ("Negative values are illegal");
				
				indentation = value;
				indentationBuffer.Remove(0, indentationBuffer.Length);
				for (int i = 0; i < value; i++)
					indentationBuffer.Append (' ');
			}
		}

		
		public bool UseValueQuotes
		{
			get { return useValueQuotes; }
			set { useValueQuotes = value; }
		}

		
		public IniWriteState WriteState
		{
			get { return writeState; }
		}

		
		public char CommentDelimiter
		{
			get { return commentDelimiter; }
			set { commentDelimiter = value; }
		}
		
		
		public char AssignDelimiter
		{
			get { return assignDelimiter; }
			set { assignDelimiter = value; }
		}
		
		
		public Stream BaseStream
		{
			get { return baseStream; }
		}
		#endregion
		
		#region Constructors
		
		public IniWriter(string filePath)
			: this (new FileStream (filePath, FileMode.Create, FileAccess.Write, FileShare.None))
		{
		}
		
		
		public IniWriter (TextWriter writer)
		{
			textWriter = writer;
			StreamWriter streamWriter = writer as StreamWriter;
			if (streamWriter != null) {
				baseStream = streamWriter.BaseStream;
			}
		}
		
		
		public IniWriter (Stream stream)
			: this (new StreamWriter (stream))
		{
		}
		#endregion
		
		#region Public methods
		
		public void Close ()
		{
		    textWriter.Flush ();
            textWriter.Close ();
			writeState = IniWriteState.Closed;
		}
		
		
		public void Flush ()
		{
			textWriter.Flush ();
		}
		
		
		public override string ToString ()
		{
			return textWriter.ToString ();
		}
		
		
		public void WriteSection (string section)
		{
			ValidateState ();
			writeState = IniWriteState.Section;
			WriteLine ("[" + section + "]");
		}
		
		
		public void WriteSection (string section, string comment)
		{
			ValidateState ();
			writeState = IniWriteState.Section;
			WriteLine ("[" + section + "]" + Comment(comment));
		}
		
		
		public void WriteKey (string key, string value)
		{
			ValidateStateKey ();
			WriteLine (key + " " + assignDelimiter + " " + GetKeyValue (value));
		}
		
		
		public void WriteKey (string key, string value, string comment)
		{
			ValidateStateKey ();
			WriteLine (key + " " + assignDelimiter + " " + GetKeyValue (value) + Comment (comment));
		}
	
		
		public void WriteEmpty ()
		{
			ValidateState ();
			if (writeState == IniWriteState.Start) {
				writeState = IniWriteState.BeforeFirstSection;
			}
			WriteLine ("");
		}

		
		public void WriteEmpty (string comment)
		{
			ValidateState ();
			if (writeState == IniWriteState.Start) {
				writeState = IniWriteState.BeforeFirstSection;
			}
			if (comment == null) {
				WriteLine ("");
			} else {
				WriteLine (commentDelimiter + " " + comment);
			}
		}
		
		
		public void Dispose ()
		{
			Dispose (true);
		}
		#endregion
		
		#region Protected methods
		
		protected virtual void Dispose (bool disposing)
		{
			if (!disposed) 
			{

			    try
			    {
			        textWriter.Flush();
			    }
                catch(Exception ex) { }

			    try
			    {
			        textWriter.Close();
                }
			    catch (Exception ex) { }
			    try
			    {
			        baseStream.Flush();
                }
			    catch (Exception ex) { }
			    try
			    {
			        baseStream.Close();
                }
                catch (Exception ex) { }
			    
				disposed = true;

				if (disposing) 
				{
					GC.SuppressFinalize (this);
				}
			}
		}
		#endregion
		
		#region Private methods
		/// <summary>
		/// Destructor.
		/// </summary>
		~IniWriter ()
		{
			Dispose (false);
		}

		/// <summary>
		/// Returns the value of a key.
		/// </summary>
		private string GetKeyValue (string text)
		{
			string result;

			if (useValueQuotes) {
				result = MassageValue ('"' + text + '"');
			} else {
				result = MassageValue (text);
			}
			
			return result;
		}
		
		/// <summary>
		/// Validates whether a key can be written.
		/// </summary>
		private void ValidateStateKey ()
		{
			ValidateState ();

			switch (writeState)
			{
			case IniWriteState.BeforeFirstSection:
			case IniWriteState.Start:
				throw  new InvalidOperationException ("The WriteState is not Section");
			case IniWriteState.Closed:
				throw  new InvalidOperationException ("The writer is closed");
			}
		}
		
		/// <summary>
		/// Validates the state to determine if the item can be written.
		/// </summary>
		private void ValidateState ()
		{
			if (writeState == IniWriteState.Closed) {
				throw  new InvalidOperationException ("The writer is closed");
			}
		}
		
		/// <summary>
		/// Returns a formatted comment.
		/// </summary>
		private string Comment (string text)
		{
			return (text == null) ? "" : (" " + commentDelimiter + " " + text);
		}
		
		/// <summary>
		/// Writes data to the writer.
		/// </summary>
		private void Write (string value)
		{
			textWriter.Write (indentationBuffer.ToString () + value);
		}
		
		/// <summary>
		/// Writes a full line to the writer.
		/// </summary>
		private void WriteLine (string value)
		{
			Write (value + eol);
		}

		/// <summary>
		/// Fixes the incoming value to prevent illegal characters from 
		/// hurting the integrity of the INI file.
		/// </summary>
		private string MassageValue (string text)
		{
			return text.Replace ("\n", "");
		}
		#endregion
	}
}