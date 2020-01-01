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
	
	public class IniSectionCollection : ICollection, IEnumerable
	{
		#region Private variables
		OrderedList list = new OrderedList ();
		#endregion

		#region Public properties	
		
		public IniSection this[int index]
		{
			get { return (IniSection)list[index]; }
		}
		
		
		public IniSection this[string configName]
		{
			get { return (IniSection)list[configName]; }
		}

		
		public int Count
		{
			get { return list.Count; }
		}
		
		
		public object SyncRoot
		{
			get { return list.SyncRoot; }
		}
		
		
		public bool IsSynchronized
		{
			get { return list.IsSynchronized; }
		}
		#endregion

		#region Public methods
		
		public void Add (IniSection section)
		{
			if (list.Contains (section)) {
				throw new ArgumentException ("IniSection already exists");
			}
			
			list.Add (section.Name, section);
		}
		
		
		public void Remove (string config)
		{
			list.Remove (config);
		}
		
		
		public void CopyTo (Array array, int index) 
		{
			list.CopyTo (array, index);
		}
		
		
		public void CopyTo (IniSection[] array, int index)
		{
			((ICollection)list).CopyTo (array, index);
		}

		
		public IEnumerator GetEnumerator () 
		{
			return list.GetEnumerator ();
		}
		#endregion
		
		#region Private methods
		#endregion
	}
}