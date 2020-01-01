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
using System.Collections;

namespace Nini.Ini
{
	#region IniReadState enumeration
	
	public enum IniReadState : int
	{
		
		Closed,
		
		EndOfFile,
		
		Error,
		
		Initial,
		
		Interactive
	};
	#endregion

	#region IniType enumeration
	
	public enum IniType : int
	{
		
		Section,
		
		Key,
		
		Empty
	}
	#endregion

	
	public class IniReader : IDisposable
	{
		#region Private variables
		int lineNumber = 1;
		int column = 1;
		IniType iniType = IniType.Empty;
		TextReader textReader = null;
		bool ignoreComments = false;
		StringBuilder name = new StringBuilder ();
		StringBuilder value = new StringBuilder ();
		StringBuilder comment = new StringBuilder ();
		IniReadState readState = IniReadState.Initial;
		bool hasComment = false;
		bool disposed = false;
		bool lineContinuation = false;
		bool acceptCommentAfterKey = true;
		bool acceptNoAssignmentOperator = false;
		bool consumeAllKeyText = false;
		char[] commentDelimiters = new char[] { ';' };
		char[] assignDelimiters = new char[] { '=' };
		#endregion

		#region Public properties
		
		public string Name
		{
			get { return this.name.ToString (); }
		}
		
		
		public string Value
		{
			get { return this.value.ToString (); }
		}
		
		
		public IniType Type
		{
			get { return iniType; }
		}
		
		
		public string Comment
		{
			get { return (hasComment) ? this.comment.ToString () : null; }
		}
		
		
		public int LineNumber
		{
			get { return lineNumber; }
		}
		
		
		public int LinePosition
		{
			get { return column; }
		}
		
		
		public bool IgnoreComments
		{
			get { return ignoreComments; }
			set { ignoreComments = value; }
		}
		
		
		public IniReadState ReadState
		{
			get { return readState; }
		}
		
		
		public bool LineContinuation
		{
			get { return lineContinuation; }
			set { lineContinuation = value; }
		}
		
		
		public bool AcceptCommentAfterKey
		{
			get { return acceptCommentAfterKey; }
			set { acceptCommentAfterKey = value; }
		}

		
		public bool AcceptNoAssignmentOperator
		{
			get { return acceptNoAssignmentOperator; }
			set { acceptNoAssignmentOperator = value; }
		}

		
		public bool ConsumeAllKeyText
		{
			get { return consumeAllKeyText; }
			set { consumeAllKeyText = value; }
		}
		#endregion
		
		#region Constructors
		
		public IniReader (string filePath)
		{
			textReader = new StreamReader (filePath);
		}
		
		
		public IniReader (TextReader reader)
		{
			textReader = reader;
		}
		
		
		public IniReader (Stream stream)
			: this (new StreamReader (stream))
		{
		}
		#endregion
		
		#region Public methods
		
		public bool Read ()
		{
			bool result = false;
			
			if (readState != IniReadState.EndOfFile 
				|| readState != IniReadState.Closed) {
				readState = IniReadState.Interactive;
				result = ReadNext ();
			}
			
			return result;
		}
		
		
		public bool MoveToNextSection ()
		{
			bool result = false;
			
			while (true)
			{
				result = Read ();

				if (iniType == IniType.Section || !result) {
					break;
				}
			}
			
			return result;
		}
		
		
		public bool MoveToNextKey ()
		{
			bool result = false;
			
			while (true)
			{
				result = Read ();

				if (iniType == IniType.Section) {
					result = false;
					break;
				}
				if (iniType == IniType.Key || !result) {
					break;
				}
			}
			
			return result;
		}
		
		
		public void Close ()
		{
			Reset ();
			readState = IniReadState.Closed;
			
			if (textReader != null) {
				textReader.Close ();
			}
		}

		
		public void Dispose ()
		{
			Dispose (true);
		}
		
		
		public char[] GetCommentDelimiters ()
		{
			char[] result = new char[commentDelimiters.Length];
			Array.Copy (commentDelimiters, 0, result, 0, commentDelimiters.Length);

			return result;
		}
		
		
		public void SetCommentDelimiters (char[] delimiters)
		{
			if (delimiters.Length < 1) {
				throw new ArgumentException ("Must supply at least one delimiter");
			}
			
			commentDelimiters = delimiters;
		}
		
		
		public char[] GetAssignDelimiters ()
		{
			char[] result = new char[assignDelimiters.Length];
			Array.Copy (assignDelimiters, 0, result, 0, assignDelimiters.Length);

			return result;
		}
		
		
		public void SetAssignDelimiters (char[] delimiters)
		{
			if (delimiters.Length < 1) {
				throw new ArgumentException ("Must supply at least one delimiter");
			}
			
			assignDelimiters = delimiters;
		}
		#endregion
		
		#region Protected methods
		
		protected virtual void Dispose (bool disposing)
		{
			if (!disposed) {
				textReader.Close ();
				disposed = true;

				if (disposing) {
					GC.SuppressFinalize (this);
				}
			}
		}
		#endregion
		
		#region Private methods
		/// <summary>
		/// Destructor.
		/// </summary>
		~IniReader ()
		{
			Dispose (false);
		}

		/// <summary>
		/// Resets all of the current INI line data.
		/// </summary>
		private void Reset ()
		{
			this.name.Remove (0, this.name.Length);
			this.value.Remove (0, this.value.Length);
			this.comment.Remove (0, this.comment.Length);
			iniType = IniType.Empty;
			hasComment = false;
		}
		
		/// <summary>
		/// Reads the next INI line item.
		/// </summary>
		private bool ReadNext ()
		{
			bool result = true;
			int ch = PeekChar ();
			Reset ();
			
			if (IsComment (ch)) {
				iniType = IniType.Empty;
				ReadChar (); // consume comment character
				ReadComment ();

				return result;
			}

			switch (ch)
			{
				case ' ':
				case '\t':
				case '\r':
					SkipWhitespace ();
					ReadNext ();
					break;
				case '\n':
					ReadChar ();
					break;
				case '[':
					ReadSection ();
					break;
				case -1:
					readState = IniReadState.EndOfFile;
					result = false;
					break;
				default:
					ReadKey ();
					break;
			}
			
			return result;
		}
		
		/// <summary>
		/// Reads a comment. Must start after the comment delimiter.
		/// </summary>
		private void ReadComment  ()
		{
			int ch = -1;
			SkipWhitespace ();
			hasComment = true;

			do
			{
				ch = ReadChar ();
				this.comment.Append ((char)ch);
			} while (!EndOfLine (ch));
			
			RemoveTrailingWhitespace (this.comment);
		}
		
		/// <summary>
		/// Removes trailing whitespace from a StringBuilder.
		/// </summary>
		private void RemoveTrailingWhitespace (StringBuilder builder)
		{
			string temp = builder.ToString ();
		
			builder.Remove (0, builder.Length);
			builder.Append (temp.TrimEnd (null));
		}
		
		/// <summary>
		/// Reads a key.
		/// </summary>
		private void ReadKey ()
		{
			int ch = -1;
			iniType = IniType.Key;
			
			while (true)
			{
				ch = PeekChar ();

				if (IsAssign (ch)) {
					ReadChar ();
					break;
				}
				
				if (EndOfLine (ch)) {
					if (acceptNoAssignmentOperator) {
						break;
					}
					throw new IniException (this, 
						String.Format ("Expected assignment operator ({0})", 
										assignDelimiters[0]));
				}

				this.name.Append ((char)ReadChar ());
			}
			
			ReadKeyValue ();
			SearchForComment ();
			RemoveTrailingWhitespace (this.name);
		}
		
		/// <summary>
		/// Reads the value of a key.
		/// </summary>
		private void ReadKeyValue ()
		{
			int ch = -1;
			bool foundQuote = false;
			int characters = 0;
			SkipWhitespace ();

			while (true)
			{
				ch = PeekChar ();

				if (!IsWhitespace (ch)) {
					characters++;
				}
				
				if (!this.ConsumeAllKeyText && ch == '"') {
					ReadChar ();

					if (!foundQuote && characters == 1) {				
						foundQuote = true;
						continue;
					} else {
						break;
					}
				}
				
				if (foundQuote && EndOfLine (ch)) {
					throw new IniException (this, "Expected closing quote (\")");
				}
				
				// Handle line continuation
				if (lineContinuation && ch == '\\') 
				{
					StringBuilder buffer = new StringBuilder ();
					buffer.Append ((char)ReadChar ()); // append '\'
					
					while (PeekChar () != '\n' && IsWhitespace (PeekChar ()))
					{
						if (PeekChar () != '\r') {
							buffer.Append ((char)ReadChar ());
						} else {
							ReadChar (); // consume '\r'
						}
					}
					
					if (PeekChar () == '\n') {
						// continue reading key value on next line
						ReadChar ();
						continue;
					} else {
						// Replace consumed characters
						this.value.Append (buffer.ToString ());
					}
				}

				if (!this.ConsumeAllKeyText) {
					// If accepting comments then don't consume as key value
					if (acceptCommentAfterKey && IsComment (ch) && !foundQuote) {
						break;
					}
				}

				// Always break at end of line
				if (EndOfLine (ch)) {
					break;
				}

				this.value.Append ((char)ReadChar ());
			}
			
			if (!foundQuote) {
				RemoveTrailingWhitespace (this.value);
			}
		}
		
		/// <summary>
		/// Reads an INI section.
		/// </summary>
		private void ReadSection ()
		{
			int ch = -1;
			iniType = IniType.Section;
			ch = ReadChar (); // consume "["

			while (true)
			{
				ch = PeekChar ();
				if (ch == ']') {
					break;
				}
				if (EndOfLine (ch)) {
					throw new IniException (this, "Expected section end (])");
				}

				this.name.Append ((char)ReadChar ());
			}

			ConsumeToEnd (); // all after '[' is garbage			
			RemoveTrailingWhitespace (this.name);
		}
		
		/// <summary>
		/// Looks for a comment.
		/// </summary>
		private void SearchForComment ()
		{
			int ch = ReadChar ();
			
			while (!EndOfLine (ch))
			{
				if (IsComment (ch)) {
					if (ignoreComments) {
						ConsumeToEnd ();
					} else {
						ReadComment ();
					}
					break;
				}
				ch = ReadChar ();
			}
		}

		/// <summary>
		/// Consumes all data until the end of a line. 
		/// </summary>		
		private void ConsumeToEnd ()
		{
			int ch = -1;

			do
			{
				ch = ReadChar ();
			} while (!EndOfLine (ch));
		}
		
		/// <summary>
		/// Returns and consumes the next character from the stream.
		/// </summary>
		private int ReadChar ()
		{
			int result = textReader.Read ();
			
			if (result == '\n') {
				lineNumber++;
				column = 1;
			} else {
				column++;
			}
			
			return result;
		}
		
		/// <summary>
		/// Returns the next upcoming character from the stream.
		/// </summary>
		private int PeekChar ()
		{
			return textReader.Peek ();
		}
		
		/// <summary>
		/// Returns true if a comment character is found.
		/// </summary>
		private bool IsComment (int ch)
		{
			return HasCharacter (commentDelimiters, ch);
		}
		
		/// <summary>
		/// Returns true if character is an assign character.
		/// </summary>
		private bool IsAssign (int ch)
		{
			return HasCharacter (assignDelimiters, ch);
		}

		/// <summary>
		/// Returns true if the character is found in the given array.
		/// </summary>
		private bool HasCharacter (char[] characters, int ch)
		{
			bool result = false;
			
			for (int i = 0; i < characters.Length; i++)
			{
				if (ch == characters[i]) 
				{
					result = true;
					break;
				}
			}
			
			return result;
		}
		
		/// <summary>
		/// Returns true if a value is whitespace.
		/// </summary>
		private bool IsWhitespace (int ch)
		{
			return ch == 0x20 || ch == 0x9 || ch == 0xD || ch == 0xA;
		}
		
		/// <summary>
		/// Skips all whitespace.
		/// </summary>
		private void SkipWhitespace ()
		{
			while (IsWhitespace (PeekChar ()))
			{
				if (EndOfLine (PeekChar ())) {
					break;
				}

				ReadChar ();
			}
		}

		/// <summary>
		/// Returns true if an end of line is found.  End of line
		/// includes both an end of line or end of file.
		/// </summary>
		private bool EndOfLine (int ch)
		{
			return (ch == '\n' || ch == -1);
		}
		#endregion
	}
}