using System;
using System.Text;
using System.Net;
using LitJsonStomt;

namespace Stomt
{

	/// <summary>
	/// A single stomt-track.
	/// </summary>
	public class StomtTrack
	{
		private StomtAPI _api;
		public string device_platform {
			get; set;
		}
		public string device_id {
			get; set;
		}
		public string sdk_type {
			get; set;
		}
		public string sdk_version {
			get; set;
		}
		public string sdk_integration {
			get; set;
		}
		public string target_id {
			get; set;
		}
		public string stomt_id {
			get; set;
		}
		public string event_category {
			get; set;
		}
		public string event_action {
			get; set;
		}
		public string event_label {
			get; set;
		}

		public StomtTrack(StomtAPI api) {
			this._api = api;
		}

		public override string ToString() {
			var jsonTrack = new StringBuilder();
			var writerTrack = new LitJsonStomt.JsonWriter(jsonTrack);

			writerTrack.WriteObjectStart();

			writerTrack.WritePropertyName("device_platform");
			writerTrack.Write(this.device_platform);

			writerTrack.WritePropertyName("device_id");
			writerTrack.Write(this.device_id);

			writerTrack.WritePropertyName("sdk_type");
			writerTrack.Write(this.sdk_type);

			writerTrack.WritePropertyName("sdk_version");
			writerTrack.Write(this.sdk_version);

			writerTrack.WritePropertyName("sdk_integration");
			writerTrack.Write(this.sdk_integration);

			writerTrack.WritePropertyName("target_id");
			writerTrack.Write(this.target_id);

			writerTrack.WritePropertyName("stomt_id");
			writerTrack.Write(this.stomt_id);

			writerTrack.WritePropertyName("event_category");
			writerTrack.Write(this.event_category);

			writerTrack.WritePropertyName("event_action");
			writerTrack.Write(this.event_action);

			if (!String.IsNullOrEmpty(this.event_label)) {
				writerTrack.WritePropertyName("event_label");
				writerTrack.Write(this.event_label);
			}

			writerTrack.WriteObjectEnd();

			return jsonTrack.ToString();
		}

		public void save() {
			this.save(null, null);
		}

		public void save(Action<LitJsonStomt.JsonData> callbackSuccess, Action<HttpWebResponse> callbackError) {
			this._api.SendTrack(this, callbackSuccess, callbackError);
		}
	}
}
