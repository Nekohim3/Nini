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
using System.Security;
using System.Globalization;
using System.Security.Permissions;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;


namespace Nini.Ini
{
	
#if (NET_COMPACT_1_0)
#else
	[Serializable]
#endif
	public class IniException : SystemException /*, ISerializable */
	{
		#region Private variables
		IniReader iniReader = null;
		string message = "";
		#endregion

		#region Public properties
		
		public int LinePosition
		{
			get	{
				return (iniReader == null) ? 0 : iniReader.LinePosition;
			}
		}
		
		
		public int LineNumber
		{
			get {
				return (iniReader == null) ? 0 : iniReader.LineNumber;
			}
		}
		
		
		public override string Message
		{
			get {
				if (iniReader == null) {
					return base.Message;
				}

				return String.Format (CultureInfo.InvariantCulture, "{0} - Line: {1}, Position: {2}.",
										message, this.LineNumber, this.LinePosition);
			}
		}
		#endregion

		#region Constructors
		
		public IniException ()
			: base ()
		{
			this.message  = "An error has occurred";
		}
		
		
		public IniException (string message, Exception exception)
			: base (message, exception)
		{
		}

		
		public IniException (string message)
			: base (message)
		{
			this.message  = message;
		}
		
		
		internal IniException (IniReader reader, string message)
			: this (message)
		{
			iniReader = reader;
			this.message = message;
		}

#if (NET_COMPACT_1_0)
#else
		
		protected IniException (SerializationInfo info, StreamingContext context)
			: base (info, context)
		{
		}
#endif
		#endregion
		
		#region Public methods
#if (NET_COMPACT_1_0)
#else
		
		[SecurityPermissionAttribute(SecurityAction.Demand,SerializationFormatter=true)]
		public override void GetObjectData (SerializationInfo info, 
											StreamingContext context)
		{
			base.GetObjectData (info, context);
			if (iniReader != null) {
				info.AddValue ("lineNumber", iniReader.LineNumber);

				info.AddValue ("linePosition", iniReader.LinePosition);
			}
		}
#endif
		#endregion
	}
}