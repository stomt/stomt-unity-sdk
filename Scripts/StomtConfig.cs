using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

namespace Stomt
{
	public class StomtConfig
	{
		public delegate void StomtConfigAction();
		public static event StomtConfigAction OnStomtConfigUpdate;
		private string StomtAccesstokenKey = "StomtAccesstoken";
		private string StomtSubscribedKey = "StomtSubscribed";
		private string StomtLoggedinKey = "StomtLoggedin";
		private string Accesstoken;
		private bool Subscribed;
		private bool Loggedin;

		public void Load()
		{
			this.Accesstoken = this.GetAccessToken();
			this.Subscribed = this.GetSubscribed();
			this.Loggedin = this.GetLoggedin();

			if (OnStomtConfigUpdate != null)
			{
				OnStomtConfigUpdate();
			}
		}

		public void Delete()
		{
			PlayerPrefs.DeleteKey(this.StomtAccesstokenKey);
			PlayerPrefs.DeleteKey(this.StomtLoggedinKey);
			PlayerPrefs.DeleteKey(this.StomtSubscribedKey);
			PlayerPrefs.Save();

			if (OnStomtConfigUpdate != null)
			{
				OnStomtConfigUpdate();
			}
		}

		public void SetAccessToken(string accesstoken)
		{
			if (PlayerPrefs.HasKey(this.StomtAccesstokenKey))
			{
				if (PlayerPrefs.GetString(this.StomtAccesstokenKey).Equals(accesstoken))
				{
					return;
				}
			}

			if (accesstoken != null)
			{
				this.Accesstoken = accesstoken;
				PlayerPrefs.SetString(this.StomtAccesstokenKey, this.Accesstoken);
				PlayerPrefs.Save();

				if (OnStomtConfigUpdate != null)
				{
					OnStomtConfigUpdate();
				}
			}
		}

		public void SetSubscribed(bool subscribed)
		{
			if (PlayerPrefs.HasKey(this.StomtSubscribedKey))
			{
				if (PlayerPrefs.GetString(this.StomtSubscribedKey).Equals(subscribed.ToString()) )
				{
					return;
				}
			}

			if (!string.IsNullOrEmpty(subscribed.ToString()))
			{
				this.Subscribed = subscribed;
				PlayerPrefs.SetString(this.StomtSubscribedKey, this.Subscribed.ToString());
				PlayerPrefs.Save();

				if (OnStomtConfigUpdate != null)
				{
					OnStomtConfigUpdate();
				}
			}
		}

		public void SetLoggedin(bool loggedin)
		{
			if (PlayerPrefs.HasKey(this.StomtLoggedinKey))
			{
				if (PlayerPrefs.GetString(this.StomtLoggedinKey).Equals(loggedin.ToString()))
				{
					return;
				}
			}

			if (!string.IsNullOrEmpty(loggedin.ToString()))
			{
				this.Loggedin = loggedin;
				PlayerPrefs.SetString(this.StomtLoggedinKey, this.Loggedin.ToString());
				PlayerPrefs.Save();

				if (OnStomtConfigUpdate != null)
				{
					OnStomtConfigUpdate();
				}
			}
		}

		public string GetAccessToken()
		{
			if (PlayerPrefs.HasKey(this.StomtAccesstokenKey))
			{
				if (PlayerPrefs.GetString(this.StomtAccesstokenKey).Equals(this.Accesstoken))
				{
					if (!string.IsNullOrEmpty(this.Accesstoken))
					{
						return this.Accesstoken;
					}
					else
					{
						//Debug.Log("Accesstoken was NullOrEmpty and Key was set");
						return "";
					}
				}
				else
				{
					return PlayerPrefs.GetString(this.StomtAccesstokenKey);
				}
			}

			if (!string.IsNullOrEmpty(this.Accesstoken))
			{
				return this.Accesstoken;
			}
			else
			{
				return "";
			}
		}

		public bool GetSubscribed()
		{
			if (PlayerPrefs.HasKey(this.StomtSubscribedKey))
			{
				if (PlayerPrefs.GetString(this.StomtSubscribedKey).Equals(this.Subscribed.ToString()))
				{
					return this.Subscribed;
				}
				else
				{
					return Convert.ToBoolean(PlayerPrefs.GetString(this.StomtSubscribedKey));
				}
			}

			return false;
		}

		public bool GetLoggedin()
		{
			if (PlayerPrefs.HasKey(this.StomtLoggedinKey))
			{
				if (PlayerPrefs.GetString(this.StomtLoggedinKey).Equals(this.Loggedin.ToString()))
				{
					return this.Loggedin;
				}
				else
				{
					return Convert.ToBoolean(PlayerPrefs.GetString(this.StomtLoggedinKey));
				}
			}

			return false;
		}
	}
}
