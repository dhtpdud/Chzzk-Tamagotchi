using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("isOn")]
	public class ES3UserType_Toggle : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_Toggle() : base(typeof(UnityEngine.UI.Toggle)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (UnityEngine.UI.Toggle)obj;
			
			writer.WriteProperty("isOn", instance.isOn, ES3Type_bool.Instance);
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (UnityEngine.UI.Toggle)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "isOn":
						instance.isOn = reader.Read<System.Boolean>(ES3Type_bool.Instance);
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_ToggleArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_ToggleArray() : base(typeof(UnityEngine.UI.Toggle[]), ES3UserType_Toggle.Instance)
		{
			Instance = this;
		}
	}
}