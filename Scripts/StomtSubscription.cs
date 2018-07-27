using System;
using System.Text;
using System.Net;
using LitJsonStomt;

namespace Stomt
{
	/// <summary>
	/// A single stomt-track.
	/// </summary>
	[Serializable]
	public class StomtSubscription
	{
		public string email { get; set; }
		public string phone { get; set; }
		public string message { get; set; }

		public StomtSubscription() {}

		public override string ToString() {
			var jsonSubscription = new StringBuilder();
			var writer = new LitJsonStomt.JsonWriter(jsonSubscription);

			writer.WriteObjectStart();

			if (!string.IsNullOrEmpty(this.email))
			{
				writer.WritePropertyName("email");
				writer.Write(this.email);
			}

			if (!string.IsNullOrEmpty(this.phone))
			{
				writer.WritePropertyName("phone");
				writer.Write(this.phone);
			}
			
			writer.WritePropertyName("message");
			writer.Write(this.message);

			writer.WriteObjectEnd();

			return jsonSubscription.ToString();
		}
	}
}
