using System;
using System.Collections;
using System.Net;
using System.Text;
using UnityEngine;

namespace Stomt
{
	/// <summary>
	/// A single stomt item.
	/// </summary>
	public struct StomtItem
	{
		public string Id { get; set; }
		public bool Positive { get; set; }
		public string Text { get; set; }
		public string Language { get; set; }
		public DateTime CreationDate { get; set; }
		public bool Anonym { get; set; }
		public string CreatorId { get; set; }
		public string CreatorName { get; set; }
	}

	/// <summary>
	/// Low-level stomt API component.
	/// </summary>
	public class StomtAPI : MonoBehaviour
	{
		/// <summary>
		/// References a method to be called when the asynchronous feed download completes.
		/// </summary>
		/// <param name="feed">The list of stomt items from the requested feed.</param>
		public delegate void FeedCallback(StomtItem[] feed);

		#region Inspector Variables
		[SerializeField]
		[Tooltip("The application identifier for your game. Tell us what you want to do (api@stomt.com) to request your own.")]
		string _appId = "";
		[SerializeField]
		[Tooltip("The name of your game's target on stomt.")]
		string _targetName = "";
		#endregion

		/// <summary>
		/// The application identifier for your game.
		/// </summary>
		public string AppId
		{
			get { return _appId; }
		}
		/// <summary>
		/// The name of your game's target on stomt.
		/// </summary>
		public string TargetName
		{
			get { return _targetName; }
		}

		/// <summary>
		/// Requests the asynchronous feed download from your game's target.
		/// </summary>
		/// <param name="callback">The <see cref="FeedCallback"/> delegate.</param>
		/// <param name="offset">The offset from feed begin.</param>
		/// <param name="limit">The maximum amount of stomts to load.</param>
		public void LoadFeed(FeedCallback callback, int offset = 0, int limit = 15)
		{
			LoadFeed(_targetName, callback, offset, limit);
		}
		/// <summary>
		/// Requests the asynchronous feed download from the specified target.
		/// </summary>
		/// <param name="target">The target to download the feed from.</param>
		/// <param name="callback">The <see cref="FeedCallback"/> delegate.</param>
		/// <param name="offset">The offset from feed begin.</param>
		/// <param name="limit">The maximum amount of stomts to load.</param>
		public void LoadFeed(string target, FeedCallback callback, int offset = 0, int limit = 15)
		{
			StartCoroutine(LoadFeedAsync(target, callback, offset, limit));
		}
		/// <summary>
		/// Creates a new anonymous stomt on the game's target.
		/// </summary>
		/// <param name="positive">The stomt type. True for "I like" and false for "I wish".</param>
		/// <param name="text">The stomt message.</param>
		public void CreateStomt(bool positive, string text)
		{
			CreateStomt(positive, _targetName, text);
		}
		/// <summary>
		/// Creates a new anonymous stomt on the specified target.
		/// </summary>
		/// <param name="positive">The stomt type. True for "I like" and false for "I wish".</param>
		/// <param name="target">The target to post the stomt to.</param>
		/// <param name="text">The stomt message.</param>
		public void CreateStomt(bool positive, string target, string text)
		{
			var json = new StringBuilder();
			var writer = new LitJson.JsonWriter(json);

			writer.WriteObjectStart();
			writer.WritePropertyName("anonym");
			writer.Write(true);
			writer.WritePropertyName("positive");
			writer.Write(positive);
			writer.WritePropertyName("target_id");
			writer.Write(target);
			writer.WritePropertyName("text");
			writer.Write(text);
			writer.WriteObjectEnd();

			StartCoroutine(CreateStomtAsync(json.ToString()));
		}
		/// <summary>
		/// Creates a new anonymous stomt on the game's target with an image attached to it.
		/// </summary>
		/// <param name="positive">The stomt type. True for "I like" and false for "I wish".</param>
		/// <param name="text">The stomt message.</param>
		/// <param name="image">The image texture to upload and attach to the stomt.</param>
		public void CreateStomtWithImage(bool positive, string text, Texture2D image)
		{
			CreateStomtWithImage(positive, _targetName, text, image);
		}
		/// <summary>
		/// Creates a new anonymous stomt on the specified target with an image attached to it.
		/// </summary>
		/// <param name="positive">The stomt type. True for "I like" and false for "I wish".</param>
		/// <param name="target">The target to post the stomt to.</param>
		/// <param name="text">The stomt message.</param>
		/// <param name="image">The image texture to upload and attach to the stomt.</param>
		public void CreateStomtWithImage(bool positive, string target, string text, Texture2D image)
		{
			if (image == null)
			{
				CreateStomt(positive, target, text);
				return;
			}

			byte[] imageBytes = image.EncodeToPNG();

			if (imageBytes == null)
			{
				return;
			}

			var jsonImage = new StringBuilder();
			var writerImage = new LitJson.JsonWriter(jsonImage);

			writerImage.WriteObjectStart();
			writerImage.WritePropertyName("images");
			writerImage.WriteObjectStart();
			writerImage.WritePropertyName("stomt");
			writerImage.WriteArrayStart();
			writerImage.WriteObjectStart();
			writerImage.WritePropertyName("data");
			writerImage.Write(System.Convert.ToBase64String(imageBytes));
			writerImage.WriteObjectEnd();
			writerImage.WriteArrayEnd();
			writerImage.WriteObjectEnd();
			writerImage.WriteObjectEnd();

			var jsonStomt = new StringBuilder();
			var writerStomt = new LitJson.JsonWriter(jsonStomt);

			writerStomt.WriteObjectStart();
			writerStomt.WritePropertyName("anonym");
			writerStomt.Write(true);
			writerStomt.WritePropertyName("positive");
			writerStomt.Write(positive);
			writerStomt.WritePropertyName("target_id");
			writerStomt.Write(target);
			writerStomt.WritePropertyName("text");
			writerStomt.Write(text);
			writerStomt.WritePropertyName("img_name");
			writerStomt.Write("{img_name}");
			writerStomt.WriteObjectEnd();

			StartCoroutine(CreateStomtWithImageAsync(jsonImage.ToString(), jsonStomt.ToString()));
		}

		void Start()
		{
			// TODO: Workaround to accept the stomt SSL certificate. This should be replaced with a proper solution.
			ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
		}

		HttpWebRequest WebRequest(string method, string url)
		{
			var request = (HttpWebRequest)System.Net.WebRequest.Create(url);
			request.Method = method;
			request.Accept = request.ContentType = "application/json";
			request.UserAgent = string.Format("Unity/{0} ({1})", Application.unityVersion, Application.platform);
			request.Headers["appid"] = _appId;

			return request;
		}
		IEnumerator LoadFeedAsync(string target, FeedCallback callback, int offset, int limit)
		{
			HttpWebRequest request = WebRequest("GET", string.Format("https://rest.stomt.com/targets/{0}/stomts/received?offset={1}&limit={2}", target, offset, limit));

			// Send request and wait for response
			var async1 = request.BeginGetResponse(null, null);

			while (!async1.IsCompleted)
			{
				yield return null;
			}

			HttpWebResponse response;
			var responseDataText = string.Empty;

			try
			{
				response = (HttpWebResponse)request.EndGetResponse(async1);
			}
			catch (WebException ex)
			{
				Debug.LogException(ex);
				yield break;
			}

			// Read response stream
			using (var responseStream = response.GetResponseStream())
			{
				if (responseStream == null)
				{
					yield break;
				}

				var buffer = new byte[2048];

				while (true)
				{
					var async2 = responseStream.BeginRead(buffer, 0, buffer.Length, null, null);

					while (!async2.IsCompleted)
					{
						yield return null;
					}

					int length = responseStream.EndRead(async2);

					if (length <= 0)
					{
						break;
					}

					responseDataText += Encoding.UTF8.GetString(buffer, 0, length);
				}
			}

			// Analyze JSON data
			LitJson.JsonData responseData = LitJson.JsonMapper.ToObject(responseDataText);

			if (responseData.Keys.Contains("error"))
			{
				Debug.LogError((string)responseData["error"]["msg"]);
				yield break;
			}

			responseData = responseData["data"];

			var feed = new StomtItem[responseData.Count];

			for (int i = 0; i < responseData.Count; i++)
			{
				var item = responseData[i];

				feed[i] = new StomtItem {
					Id = (string)item["id"],
					Positive = (bool)item["positive"],
					Text = (string)item["text"],
					Language = (string)item["lang"],
					CreationDate = DateTime.Parse((string)item["created_at"]),
					Anonym = (bool)item["anonym"]
				};

				if (feed[i].Anonym)
				{
					continue;
				}

				feed[i].CreatorId = (string)item["creator"]["id"];
				feed[i].CreatorName = (string)item["creator"]["displayname"];
			}

			callback(feed);
		}
		IEnumerator CreateStomtAsync(string json)
		{
			var data = Encoding.UTF8.GetBytes(json);

			HttpWebRequest request = WebRequest("POST", "https://rest.stomt.com/stomts");
			request.ContentLength = data.Length;

			// Send request
			var async1 = request.BeginGetRequestStream(null, null);

			while (!async1.IsCompleted)
			{
				yield return null;
			}

			try
			{
				using (var requestStream = request.EndGetRequestStream(async1))
				{
					requestStream.Write(data, 0, data.Length);
				}
			}
			catch (WebException ex)
			{
				Debug.LogException(ex);
			}
		}
		IEnumerator CreateStomtWithImageAsync(string jsonImage, string jsonStomt)
		{
			var data = Encoding.UTF8.GetBytes(jsonImage);

			HttpWebRequest request = WebRequest("POST", "https://rest.stomt.com/images");
			request.ContentLength = data.Length;

			// Send request
			var async1 = request.BeginGetRequestStream(null, null);

			while (!async1.IsCompleted)
			{
				yield return null;
			}

			try
			{
				using (var requestStream = request.EndGetRequestStream(async1))
				{
					requestStream.Write(data, 0, data.Length);
				}
			}
			catch (WebException ex)
			{
				Debug.LogException(ex);
				yield break;
			}

			// Wait for response
			var async2 = request.BeginGetResponse(null, null);

			while (!async2.IsCompleted)
			{
				yield return null;
			}

			HttpWebResponse response;
			var responseDataText = string.Empty;

			try
			{
				response = (HttpWebResponse)request.EndGetResponse(async2);
			}
			catch (WebException ex)
			{
				Debug.LogException(ex);
				yield break;
			}

			// Read response stream
			using (var responseStream = response.GetResponseStream())
			{
				if (responseStream == null)
				{
					yield break;
				}

				var buffer = new byte[2048];

				while (true)
				{
					var async3 = responseStream.BeginRead(buffer, 0, buffer.Length, null, null);

					while (!async3.IsCompleted)
					{
						yield return null;
					}

					int length = responseStream.EndRead(async3);

					if (length <= 0)
					{
						break;
					}

					responseDataText += Encoding.UTF8.GetString(buffer, 0, length);
				}
			}

			// Analyze JSON data
			LitJson.JsonData responseData = LitJson.JsonMapper.ToObject(responseDataText);

			if (responseData.Keys.Contains("error"))
			{
				Debug.LogError((string)responseData["error"]["msg"]);
				yield break;
			}

			var imagename = (string)responseData["data"]["images"]["stomt"]["name"];

			yield return StartCoroutine(CreateStomtAsync(jsonStomt.Replace("{img_name}", imagename)));
		}
	}
}
