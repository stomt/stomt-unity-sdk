using System;
using System.Text;
using System.Net;
using UnityEngine;
using LitJsonStomt;
using System.Collections.Generic;

namespace Stomt
{
	[Serializable]
	public class StomtCreation
	{
		public string screenshot { get; set; }
		public string logs { get; set; }
		public string target_id { get; set; }
		public bool positive { get; set; }
		public string text { get; set; }
		public string lang { get; set; }
		public bool anonym { get; set; }
		public string img_name { get; set; }
		public string file_uid { get; set; }
		public string[] labels;
		public List<List<string> > CustomKeyValuePairs;
		public bool DisableDefaultLabels;

		public StomtCreation()
		{
			CustomKeyValuePairs = new List<List<string>>();
		}

		public override string ToString()
		{
			var jsonStomt = new StringBuilder();
			var writerStomt = new LitJsonStomt.JsonWriter(jsonStomt);

			writerStomt.WriteObjectStart();
			//writerStomt.WritePropertyName("anonym");
			//writerStomt.Write(this.anonym);
			writerStomt.WritePropertyName("positive");
			writerStomt.Write(this.positive);
			writerStomt.WritePropertyName("text");
			writerStomt.Write(this.text);
			writerStomt.WritePropertyName("lang");
			writerStomt.Write(this.lang);

			if (!string.IsNullOrEmpty(this.target_id)) {
				writerStomt.WritePropertyName("target_id");
				writerStomt.Write(this.target_id);
			}

			// Add labels
			writerStomt.WritePropertyName("extradata");
			writerStomt.WriteObjectStart();

			writerStomt.WritePropertyName("labels");
			writerStomt.WriteArrayStart();

			if (labels.Length > 0)
			{
				foreach (string label in labels)
				{
					if (label != "AddLabelHere" && label != "add-label-here")
					{
						writerStomt.Write(label);
					}
				}
			}

			// Add default labels
			if (!DisableDefaultLabels)
			{
				writerStomt.Write(Application.platform.ToString());
				writerStomt.Write(Screen.currentResolution.ToString());
			}

			writerStomt.WriteArrayEnd();

			// Add CustomKeyValuePairs
			if (CustomKeyValuePairs.Count > 0)
			{
				foreach (List<string> PairList in CustomKeyValuePairs)
				{
					if (PairList.Count > 1)
					{
						writerStomt.WritePropertyName(PairList[0]);
						writerStomt.Write(PairList[1]);
					}
				}
			}

			writerStomt.WriteObjectEnd();


			if (!string.IsNullOrEmpty(this.img_name))
			{
				writerStomt.WritePropertyName("img_name");
				writerStomt.Write(this.img_name);
			}

			if (!string.IsNullOrEmpty(this.file_uid))
			{
				writerStomt.WritePropertyName("files");
				writerStomt.WriteObjectStart();
				writerStomt.WritePropertyName("stomt");
				writerStomt.WriteObjectStart();
				writerStomt.WritePropertyName("file_uid");
				writerStomt.Write(this.file_uid);
				writerStomt.WriteObjectEnd();
				writerStomt.WriteObjectEnd();
			}

			writerStomt.WriteObjectEnd();
			return jsonStomt.ToString();
		}

		public void AddCustomKeyValuePair(string key, string value)
		{
			List<string> pair = new List<string>();
			pair.Add(key);
			pair.Add(value);

			CustomKeyValuePairs.Add(pair);
		}

		public void attachScreenshot(Texture2D image)
		{
			// Convert to Base64
			byte[] imageBytes = image.EncodeToPNG();
			this.screenshot = Convert.ToBase64String(imageBytes);
		}

		public void attachLogs(string logs)
		{
			this.logs = logs;
		}

		public void attachLogs(StomtLog log)
		{
			this.logs = log.getFileConent ();
		}
	}
}
