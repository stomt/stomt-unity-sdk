using System;
using System.Text;
using System.Net;
using LitJsonStomt;

namespace Stomt
{
	/// <summary>
	/// A single stomt-notification.
	/// </summary>
	public class StomtNotification
	{
		private StomtAPI api;
		public string id { get; set; }
		public string target_id { get; set; }
		public string title { get; set; }
		public string text { get; set; }
		public string fullText { get; set; }
		public string url { get; set; }
		// public obj goal
		// public obj trigger
		public string created_at { get; set; }
		public bool seen { get; set; }
		public bool clicked { get; set; }


		public StomtNotification(StomtAPI api)
		{
			this.api = api;
		}

		public StomtNotification(StomtAPI api, JsonData json)
		{
			this.api = api;

			this.id = (string)json["id"];
			this.target_id = (string)json["target_id"];
			this.title = (string)json["title"];
			this.text = (string)json["text"];
			this.fullText = (string)json["fullText"];
			this.url = (string)json["url"];
			// public obj goal
			// public obj trigger
			this.created_at = (string)json["created_at"];
			this.seen = (bool)json["seen"];
			this.clicked = (bool)json["clicked"];
		}

		public void OpenLink()
		{
			this.api.MarkNotificationAsClicked(this);
			this.api.OpenStomtUrl(this.url, "notification");
		}
	}
}
