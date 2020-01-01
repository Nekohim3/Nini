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
using System.Collections;
using Nini.Util;

namespace Nini.Ini
{
	
	public class IniSection
	{
		#region Private variables
		OrderedList configList = new OrderedList ();
		string name = "";
		string comment = null;
		int commentCount = 0;
		#endregion

		#region Constructors
		
		public IniSection (string name, string comment)
		{
			this.name = name;
			this.comment = comment;
		}
		
		
		public IniSection (string name)
			: this (name, null)
		{
		}
		#endregion
		
		#region Public properties
		
		public string Name
		{
			get { return name; }
		}
		
		
		public string Comment
		{
			get { return comment; }
		}
		
		
		public int ItemCount
		{
			get { return configList.Count; }
		}
		#endregion

		#region Public methods
		
		
		public string GetValue (string key)
		{
			string result = null;

			if (Contains (key)) {
				IniItem item = (IniItem)configList[key];
				result = item.Value;
			}

			return result;
		}
		
		
		public IniItem GetItem (int index)
		{
			return (IniItem)configList[index];
		}
		
		
		public string[] GetKeys ()
		{
			ArrayList list = new ArrayList ();
			IniItem item = null;
			
			for (int i = 0; i < configList.Count; i++)
			{
				item = (IniItem)configList[i]; 
				if (item.Type == IniType.Key) {
					list.Add (item.Name);
				}
			}
			string[] result = new string[list.Count];
			list.CopyTo (result, 0);
			
			return result;
		}
		
		
		public bool Contains (string key)
		{
			return (configList[key] != null); 
		}
		
		
		public void Set (string key, string value, string comment)
		{
			IniItem item = null;

			if (Contains (key)) {
				item = (IniItem)configList[key];
				item.Value = value;
				item.Comment = comment;
			} else {
				item = new IniItem (key, value, IniType.Key, comment);
				configList.Add (key, item);
			}
		}

		
		public void Set (string key, string value)
		{
			Set (key, value, null);
		}
		
		
		public void Set (string comment)
		{
			string name = "#comment" + commentCount;
			IniItem item = new IniItem (name, null, 
										IniType.Empty, comment);
			configList.Add (name, item);
			
			commentCount++;
		}
		
		
		public void Set ()
		{
			Set (null);
		}
		
		
		public void Remove (string key)
		{
			if (Contains (key)) {
				configList.Remove (key);
			}
		}
		#endregion
	}
}