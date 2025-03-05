using System;
using UnityEngine;

namespace ES3Types
{
	[UnityEngine.Scripting.Preserve]
	[ES3PropertiesAttribute("gravity", "SpawnMinDonationAmount", "SpawnMinSubscriptionMonth", "SpawnMinSpeed", "SpawnMaxSpeed", "dragPower", "stabilityPower", "physicMaxVelocity", "peepoConfig", "donationConfig", "chatBubbleSize", "thumbnailsCacheDic")]
	public class ES3UserType_GameManager : ES3ComponentType
	{
		public static ES3Type Instance = null;

		public ES3UserType_GameManager() : base(typeof(GameManager)){ Instance = this; priority = 1;}


		protected override void WriteComponent(object obj, ES3Writer writer)
		{
			var instance = (GameManager)obj;
			
			writer.WriteProperty("gravity", instance.gravity, ES3Type_float.Instance);
			writer.WriteProperty("SpawnMinDonationAmount", instance.SpawnMinDonationAmount, ES3Type_int.Instance);
			writer.WriteProperty("SpawnMinSubscriptionMonth", instance.SpawnMinSubscriptionMonth, ES3Type_int.Instance);
			writer.WriteProperty("SpawnMinSpeed", instance.SpawnMinSpeed, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(Unity.Mathematics.float2)));
			writer.WriteProperty("SpawnMaxSpeed", instance.SpawnMaxSpeed, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(Unity.Mathematics.float2)));
			writer.WriteProperty("dragPower", instance.dragPower, ES3Type_float.Instance);
			writer.WriteProperty("stabilityPower", instance.stabilityPower, ES3Type_float.Instance);
			writer.WriteProperty("physicMaxVelocity", instance.physicMaxVelocity, ES3Type_float.Instance);
			writer.WriteProperty("peepoConfig", instance.peepoConfig, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(GameManager.PeepoConfig)));
			writer.WriteProperty("donationConfig", instance.donationConfig, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(GameManager.DonationConfig)));
			writer.WriteProperty("chatBubbleSize", instance.chatBubbleSize, ES3Type_float.Instance);
			writer.WriteProperty("thumbnailsCacheDic", instance.thumbnailsCacheDic, ES3Internal.ES3TypeMgr.GetOrCreateES3Type(typeof(System.Collections.Generic.Dictionary<System.Int32, UnityEngine.Texture2D>)));
		}

		protected override void ReadComponent<T>(ES3Reader reader, object obj)
		{
			var instance = (GameManager)obj;
			foreach(string propertyName in reader.Properties)
			{
				switch(propertyName)
				{
					
					case "gravity":
						instance.gravity = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "SpawnMinDonationAmount":
						instance.SpawnMinDonationAmount = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "SpawnMinSubscriptionMonth":
						instance.SpawnMinSubscriptionMonth = reader.Read<System.Int32>(ES3Type_int.Instance);
						break;
					case "SpawnMinSpeed":
						instance.SpawnMinSpeed = reader.Read<Unity.Mathematics.float2>();
						break;
					case "SpawnMaxSpeed":
						instance.SpawnMaxSpeed = reader.Read<Unity.Mathematics.float2>();
						break;
					case "dragPower":
						instance.dragPower = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "stabilityPower":
						instance.stabilityPower = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "physicMaxVelocity":
						instance.physicMaxVelocity = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "peepoConfig":
						instance.peepoConfig = reader.Read<GameManager.PeepoConfig>();
						break;
					case "donationConfig":
						instance.donationConfig = reader.Read<GameManager.DonationConfig>();
						break;
					case "chatBubbleSize":
						instance.chatBubbleSize = reader.Read<System.Single>(ES3Type_float.Instance);
						break;
					case "thumbnailsCacheDic":
						instance.thumbnailsCacheDic = reader.Read<System.Collections.Generic.Dictionary<System.Int32, UnityEngine.Texture2D>>();
						break;
					default:
						reader.Skip();
						break;
				}
			}
		}
	}


	public class ES3UserType_GameManagerArray : ES3ArrayType
	{
		public static ES3Type Instance;

		public ES3UserType_GameManagerArray() : base(typeof(GameManager[]), ES3UserType_GameManager.Instance)
		{
			Instance = this;
		}
	}
}