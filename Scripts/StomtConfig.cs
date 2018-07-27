using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Stomt
{
	public static class StomtConfig
	{
		public delegate void StomtConfigAction();
		public static event StomtConfigAction OnStomtConfigUpdate;

		private static string StomtAccesstokenKey = "StomtAccesstoken";
		private static string StomtSubscribedKey = "StomtSubscribed";
		private static string StomtLoggedinKey = "StomtLoggedin";
		private static string StomtTargetIDKey = "StomtTargetID";
		private static string StomtTargetDisplaynameKey = "StomtTargetDisplayname";
		private static string StomtTargetImageUrlKey = "StomtTargetImageUrl";
		private static string StomtTargetAmountStomtsKey = "StomtTargetAmountStomts";
		private static string StomtUserIDKey = "StomtUserID";
		private static string StomtUserDisplaynameKey = "StomtUserDisplayname";
		private static string StomtUserImageUrlKey = "StomtUserImageUrl";
		private static string StomtUserAmountStomtsKey = "StomtUserAmountStomts";

		public static string AccessToken
		{
			get {
				return PlayerPrefs.GetString(StomtConfig.StomtAccesstokenKey, "");
			}
			set {
				StomtConfig.SetString(StomtConfig.StomtAccesstokenKey, value);
			}
		}

		public static bool Subscribed
		{
			get
			{
				return Convert.ToBoolean(PlayerPrefs.GetString(StomtConfig.StomtSubscribedKey, "False"));
			}
			set
			{
				StomtConfig.SetString(StomtConfig.StomtSubscribedKey, value.ToString());
			}
		}

		public static bool LoggedIn
		{
			get
			{
				return Convert.ToBoolean(PlayerPrefs.GetString(StomtConfig.StomtLoggedinKey, "False"));
			}
			set
			{
				StomtConfig.SetString(StomtConfig.StomtLoggedinKey, value.ToString());
			}
		}

		public static string TargetID
		{
			get
			{
				return PlayerPrefs.GetString(StomtConfig.StomtTargetIDKey, "");
			}
			set
			{
				StomtConfig.SetString(StomtConfig.StomtTargetIDKey, value);
			}
		}

		public static string TargetDisplayname
		{
			get
			{
				return PlayerPrefs.GetString(StomtConfig.StomtTargetDisplaynameKey, "this game");
			}
			set
			{
				StomtConfig.SetString(StomtConfig.StomtTargetDisplaynameKey, value);
			}
		}

		public static string TargetImageUrl
		{
			get
			{
				return PlayerPrefs.GetString(StomtConfig.StomtTargetImageUrlKey, "");
			}
			set
			{
				StomtConfig.SetString(StomtConfig.StomtTargetImageUrlKey, value);
			}
		}

		public static int TargetAmountStomts
		{
			get
			{
				return PlayerPrefs.GetInt(StomtConfig.StomtTargetAmountStomtsKey, 0);
			}
			set
			{
				StomtConfig.SetInt(StomtConfig.StomtTargetAmountStomtsKey, value);
			}
		}

		public static string UserID
		{
			get
			{
				return PlayerPrefs.GetString(StomtConfig.StomtUserIDKey, "");
			}
			set
			{
				StomtConfig.SetString(StomtConfig.StomtUserIDKey, value);
			}
		}

		public static string UserDisplayname
		{
			get
			{
				return PlayerPrefs.GetString(StomtConfig.StomtUserDisplaynameKey, "you");
			}
			set
			{
				StomtConfig.SetString(StomtConfig.StomtUserDisplaynameKey, value);
			}
		}

		public static string UserImageUrl
		{
			get
			{
				return PlayerPrefs.GetString(StomtConfig.StomtUserImageUrlKey, "");
			}
			set
			{
				StomtConfig.SetString(StomtConfig.StomtUserImageUrlKey, value);
			}
		}

		public static int UserAmountStomts
		{
			get
			{
				return PlayerPrefs.GetInt(StomtConfig.StomtUserAmountStomtsKey, 0);
			}
			set
			{
				StomtConfig.SetInt(StomtConfig.StomtUserAmountStomtsKey, value);
			}
		}

		//// Helpers

		public static void Delete()
		{
			PlayerPrefs.DeleteKey(StomtConfig.StomtAccesstokenKey);
			PlayerPrefs.DeleteKey(StomtConfig.StomtSubscribedKey);
			PlayerPrefs.DeleteKey(StomtConfig.StomtLoggedinKey);

			PlayerPrefs.DeleteKey(StomtConfig.StomtTargetIDKey);
			PlayerPrefs.DeleteKey(StomtConfig.StomtTargetDisplaynameKey);
			PlayerPrefs.DeleteKey(StomtConfig.StomtTargetImageUrlKey);
			PlayerPrefs.DeleteKey(StomtConfig.StomtTargetAmountStomtsKey);

			PlayerPrefs.DeleteKey(StomtConfig.StomtUserIDKey);
			PlayerPrefs.DeleteKey(StomtConfig.StomtUserDisplaynameKey);
			PlayerPrefs.DeleteKey(StomtConfig.StomtUserImageUrlKey);
			PlayerPrefs.DeleteKey(StomtConfig.StomtUserAmountStomtsKey);

			PlayerPrefs.Save();

			if (OnStomtConfigUpdate != null)
			{
				OnStomtConfigUpdate();
			}
		}

		public static void DeleteUser()
		{
			PlayerPrefs.DeleteKey(StomtConfig.StomtAccesstokenKey);
			PlayerPrefs.DeleteKey(StomtConfig.StomtSubscribedKey);
			PlayerPrefs.DeleteKey(StomtConfig.StomtLoggedinKey);

			PlayerPrefs.DeleteKey(StomtConfig.StomtUserIDKey);
			PlayerPrefs.DeleteKey(StomtConfig.StomtUserDisplaynameKey);
			PlayerPrefs.DeleteKey(StomtConfig.StomtUserImageUrlKey);
			PlayerPrefs.DeleteKey(StomtConfig.StomtUserAmountStomtsKey);
			PlayerPrefs.Save();

			if (OnStomtConfigUpdate != null)
			{
				OnStomtConfigUpdate();
			}
		}

		private static bool SetString(string key, string value)
		{
			if (PlayerPrefs.HasKey(key) && PlayerPrefs.GetString(key).Equals(value))
			{
				return false;
			}

			PlayerPrefs.SetString(key, value);
			PlayerPrefs.Save();

			if (OnStomtConfigUpdate != null)
			{
				OnStomtConfigUpdate();
			}

			return true;
		}

		private static bool SetInt(string key, int value)
		{
			if (PlayerPrefs.HasKey(key) && PlayerPrefs.GetInt(key) == value)
			{
				return false;
			}

			PlayerPrefs.SetInt(key, value);
			PlayerPrefs.Save();

			if (OnStomtConfigUpdate != null)
			{
				OnStomtConfigUpdate();
			}

			return true;
		}
	}
}
