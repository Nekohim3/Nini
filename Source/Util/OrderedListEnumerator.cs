using System;
using System.Collections;

namespace Nini.Util
{
	
	public class OrderedListEnumerator : IDictionaryEnumerator
	{
		#region Private variables
		int index = -1;
		ArrayList list;
		#endregion

		#region Constructors
		/// <summary>
		/// Instantiates an ordered list enumerator with an ArrayList.
		/// </summary>
		internal OrderedListEnumerator (ArrayList arrayList)
		{
			list = arrayList;
		}
		#endregion

		#region Public properties
		
		object IEnumerator.Current 
		{
			get 
			{
				if (index < 0 || index >= list.Count)
					throw new InvalidOperationException ();

				return list[index];
			}
		}
		
		
		public DictionaryEntry Current 
		{
			get 
			{
				if (index < 0 || index >= list.Count)
					throw new InvalidOperationException ();

				return (DictionaryEntry)list[index];
			}
		}

		
		public DictionaryEntry Entry 
		{
			get { return (DictionaryEntry) Current; }
		}

		
		public object Key 
		{
			get { return Entry.Key; }
		}

		
		public object Value 
		{
			get { return Entry.Value; }
		}
		#endregion

		#region Public methods
		
		public bool MoveNext ()
		{
			index++;
			if (index >= list.Count)
				return false;

			return true;
		}

		
		public void Reset ()
		{
			index = -1;
		}
		#endregion
	}
}