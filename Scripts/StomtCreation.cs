using System;
using System.Text;
using UnityEngine;

namespace Stomt
{
	public class StomtCreation
	{
		private StomtAPI _api;

		public Texture2D screenshot { get; set; }
		public string logs { get; set; }

		public string target_id  { get; set; }
		public bool positive  { get; set; }
		public string text  { get; set; }
		public string lang  { get; set; }
		public bool anonym  { get; set; }
		public string img_name  { get; set; }
		public string file_uid  { get; set; }

		public StomtCreation(StomtAPI api) {
			this._api = api;
		}

		public override string ToString() {
			var jsonStomt = new StringBuilder();
			var writerStomt = new LitJson.JsonWriter(jsonStomt);

			writerStomt.WriteObjectStart();
			writerStomt.WritePropertyName("anonym");
			writerStomt.Write(this.anonym);
			writerStomt.WritePropertyName("positive");
			writerStomt.Write(this.positive);
			writerStomt.WritePropertyName("target_id");
			writerStomt.Write(this.target_id);
			writerStomt.WritePropertyName("text");
			writerStomt.Write(this.text);

			if(!string.IsNullOrEmpty(this.img_name)) {
				writerStomt.WritePropertyName("img_name");
				writerStomt.Write(this.img_name);
			}

			if(!string.IsNullOrEmpty(this.file_uid)) {
				writerStomt.WritePropertyName("file_uid");
				writerStomt.Write(this.file_uid);
			}

			writerStomt.WriteObjectEnd();

			return jsonStomt.ToString();
		}

		public void attachScreenshot(Texture2D screenshot) {
			this.screenshot = screenshot;
		}

		public void attachLogs(string logs) {
			this.logs = logs;
		}

		public void save() {
			this._api.SendStomt(this, null, null);
		}
	}
}
