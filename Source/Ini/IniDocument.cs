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
using System.Collections;
using Nini.Util;

namespace Nini.Ini
{
	#region IniFileType enumeration
	
	public enum IniFileType
	{
		
		Standard,
		
		PythonStyle,
		
		SambaStyle,
		
		MysqlStyle,
		
		WindowsStyle
	}
	#endregion

	
	public class IniDocument
	{
		#region Private variables
		IniSectionCollection sections = new IniSectionCollection ();
		ArrayList initialComment = new ArrayList ();
		IniFileType fileType = IniFileType.Standard;
		#endregion
		
		#region Public properties
		
		public IniFileType FileType
		{
			get { return fileType; }
			set { fileType = value; }
		}
		#endregion

		#region Constructors
		
		public IniDocument (string filePath)
		{
			fileType = IniFileType.Standard;
			Load (filePath);
		}

		
		public IniDocument (string filePath, IniFileType type)
		{
			fileType = type;
			Load (filePath);
		}

		
		public IniDocument (TextReader reader)
		{
			fileType = IniFileType.Standard;
			Load (reader);
		}

		
		public IniDocument (TextReader reader, IniFileType type)
		{
			fileType = type;
			Load (reader);
		}
		
		
		public IniDocument (Stream stream)
		{
			fileType = IniFileType.Standard;
			Load (stream);
		}

		
		public IniDocument (Stream stream, IniFileType type)
		{
			fileType = type;
			Load (stream);
		}

		
		public IniDocument (IniReader reader)
		{
			fileType = IniFileType.Standard;
			Load (reader);
		}

		
		public IniDocument ()
		{
		}
		#endregion
		
		#region Public methods
		
		public void Load (string filePath)
		{
			Load (new StreamReader (filePath));
		}

		
		public void Load (TextReader reader)
		{
			Load (GetIniReader (reader, fileType));
		}

		
		public void Load (Stream stream)
		{
			Load (new StreamReader (stream));
		}

		
		public void Load (IniReader reader)
		{
			LoadReader (reader);
		}

		
		public IniSectionCollection Sections
		{
			get { return sections; }
		}

		
		public void Save (TextWriter textWriter)
		{
			IniWriter writer = GetIniWriter (textWriter, fileType);
			IniItem item = null;
			IniSection section = null;
			
			foreach (string comment in initialComment)
			{
				writer.WriteEmpty  (comment);
			}

			for (int j = 0; j < sections.Count; j++)
			{
				section = sections[j];
				writer.WriteSection (section.Name, section.Comment);
				for (int i = 0; i < section.ItemCount; i++)
				{
					item = section.GetItem (i);
					switch (item.Type)
					{
					case IniType.Key:
						writer.WriteKey (item.Name, item.Value, item.Comment);
						break;
					case IniType.Empty:
						writer.WriteEmpty (item.Comment);
						break;
					}
				}
			}
		}
		
		
		public void Save (string filePath)
		{
			StreamWriter writer = new StreamWriter (filePath);
			Save (writer);
		    writer.Flush ();
			writer.Close ();
		}

		
		public void Save (Stream stream)
		{
			Save (new StreamWriter (stream));
		}
		#endregion
		
		#region Private methods
		/// <summary>
		/// Loads the file not saving comments.
		/// </summary>
		private void LoadReader (IniReader reader)
		{
			reader.IgnoreComments = false;
			bool sectionFound = false;
			IniSection section = null;
			
			try {
				while (reader.Read ())
				{
					switch (reader.Type)
					{
					case IniType.Empty:
						if (!sectionFound) {
							initialComment.Add (reader.Comment);
						} else {
							section.Set (reader.Comment);
						}

						break;
					case IniType.Section:
						sectionFound = true;
						// If section already exists then overwrite it
						if (sections[reader.Name] != null) {
							sections.Remove (reader.Name);
						}
						section = new IniSection (reader.Name, reader.Comment);
						sections.Add (section);

						break;
					case IniType.Key:
						if (section.GetValue (reader.Name) == null) { 
							section.Set (reader.Name, reader.Value, reader.Comment); 
						} 
						break;
					}
				}
			} catch (Exception ex) {
				throw ex;
			} finally {
				// Always close the file
				reader.Close ();
			}
		}

		/// <summary>
		/// Returns a proper INI reader depending upon the type parameter.
		/// </summary>
		private IniReader GetIniReader (TextReader reader, IniFileType type)
		{
			IniReader result = new IniReader (reader);

			switch (type)
			{
			case IniFileType.Standard:
				// do nothing
				break;
			case IniFileType.PythonStyle:
				result.AcceptCommentAfterKey = false;
				result.SetCommentDelimiters (new char[] { ';', '#' });
				result.SetAssignDelimiters (new char[] { ':' });
				break;
			case IniFileType.SambaStyle:
				result.AcceptCommentAfterKey = false;
				result.SetCommentDelimiters (new char[] { ';', '#' });
				result.LineContinuation = true;
				break;
			case IniFileType.MysqlStyle:
				result.AcceptCommentAfterKey = false;
				result.AcceptNoAssignmentOperator = true;
				result.SetCommentDelimiters (new char[] { '#' });
				result.SetAssignDelimiters (new char[] { ':', '=' });
				break;
			case IniFileType.WindowsStyle:
				result.ConsumeAllKeyText = true;
				break;
			}

			return result;
		}

		/// <summary>
		/// Returns a proper IniWriter depending upon the type parameter.
		/// </summary>
		private IniWriter GetIniWriter (TextWriter reader, IniFileType type)
		{
			IniWriter result = new IniWriter (reader);

			switch (type)
			{
			case IniFileType.Standard:
			case IniFileType.WindowsStyle:
				// do nothing
				break;
			case IniFileType.PythonStyle:
				result.AssignDelimiter = ':';
				result.CommentDelimiter = '#';
				break;
			case IniFileType.SambaStyle:
			case IniFileType.MysqlStyle:
				result.AssignDelimiter = '=';
				result.CommentDelimiter = '#';
				break;
			}

			return result;
		}
		#endregion
	}
}

