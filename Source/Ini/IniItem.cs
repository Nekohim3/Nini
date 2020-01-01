using System;

namespace Nini.Ini
{
		
		public class IniItem
		{
			#region Private variables
			IniType iniType = IniType.Empty;
			string iniName = "";
			string iniValue = "";
			string iniComment = null;
			#endregion
			
			#region Public properties
			
			public IniType Type
			{
				get { return iniType; }
				set { iniType = value; }
			}
			
			
			public string Value
			{
				get { return iniValue; }
				set { iniValue = value; }
			}
			
			
			public string Name
			{
				get { return iniName; }
			}
			
			
			public string Comment
			{
				get { return iniComment; }
				set { iniComment = value; }
			}
			#endregion
			
			
			protected internal IniItem (string name, string value, IniType type, string comment)
			{
				iniName = name;
				iniValue = value;
				iniType = type;
				iniComment = comment;
			}
		}
}

