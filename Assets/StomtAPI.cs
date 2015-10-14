using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using UnityEngine;

/// <summary>
/// A single stomt item, laid out after the JSON representation.
/// </summary>
public struct Stomt
{
	public class Target
	{
		public string id { get; set; }
		public string displayname { get; set; }
	}

	public string id { get; set; }
	public bool positive { get; set; }
	public string text { get; set; }
	public string lang { get; set; }
	public string created_at { get; set; }
	public bool anonym { get; set; }
	public Target creator { get; set; }
}

/// <summary>
/// Low-level stomt API component.
/// </summary>
public class StomtAPI : MonoBehaviour
{
	[SerializeField]
	[Tooltip("The application identifier for your game. Tell us what you want to do (api@stomt.com) to request your own.")]
	string _appId = "";
	[SerializeField]
	[Tooltip("The name of your game's target on stomt.")]
	string _targetName = "";

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

	void Start()
	{
		// TODO: Workaround to accept the stomt SSL certificate. This should be replaced with a proper solution.
		ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
	}

	/// <summary>
	/// Requests the asynchronous feed download from your game's target.
	/// </summary>
	/// <param name="outlist">Reference to a list receiving the feed once downloaded.</param>
	public void LoadFeed(ref List<Stomt> outlist)
	{
		LoadFeed(_targetName, ref outlist);
	}
	/// <summary>
	/// Requests the asynchronous feed download from the specified target.
	/// </summary>
	/// <param name="target">The target to download the feed from.</param>
	/// <param name="outlist">Reference to a list receiving the feed once downloaded.</param>
	public void LoadFeed(string target, ref List<Stomt> outlist)
	{
		StartCoroutine(LoadFeedAsync(target, outlist));
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

	HttpWebRequest WebRequest(string method, string url)
	{
		var request = (HttpWebRequest)System.Net.WebRequest.Create(url);
		request.Method = method;
		request.Accept = request.ContentType = "application/json";
		request.UserAgent = string.Format("Unity/{0} ({1})", Application.unityVersion, Application.platform);
		request.Headers["appid"] = _appId;

		return request;
	}
	IEnumerator LoadFeedAsync(string target, ICollection<Stomt> outlist)
	{
		HttpWebRequest request = WebRequest("GET", string.Format("https://rest.stomt.com/targets/{0}/stomts/received?limit={1}", target, 15));

		// Send request and wait for response
		var async1 = request.BeginGetResponse(null, null);

		yield return async1;

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
		using (Stream responseStream = response.GetResponseStream())
		{
			if (responseStream == null)
			{
				yield break;
			}

			var buffer = new byte[2048];

			while (true)
			{
				var async2 = responseStream.BeginRead(buffer, 0, buffer.Length, null, null);

				yield return async2;

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

		for (int i = 0; i < responseData.Count; i++)
		{
			outlist.Add(LitJson.JsonMapper.ToObject<Stomt>(responseData[i].ToJson()));
		}
	}
	IEnumerator CreateStomtAsync(string json)
	{
		var data = Encoding.UTF8.GetBytes(json);

		HttpWebRequest request = WebRequest("POST", "https://test.rest.stomt.com/stomts");
		request.ContentLength = data.Length;

		// Send request
		var async1 = request.BeginGetRequestStream(null, null);

		yield return async1;

		try
		{
			using (Stream requestStream = request.EndGetRequestStream(async1))
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

		HttpWebRequest request = WebRequest("POST", "https://test.rest.stomt.com/images");
		request.ContentLength = data.Length;

		// Send request
		var async1 = request.BeginGetRequestStream(null, null);

		yield return async1;

		try
		{
			using (Stream requestStream = request.EndGetRequestStream(async1))
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

		yield return async2;

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
		using (Stream responseStream = response.GetResponseStream())
		{
			if (responseStream == null)
			{
				yield break;
			}

			var buffer = new byte[2048];

			while (true)
			{
				var async3 = responseStream.BeginRead(buffer, 0, buffer.Length, null, null);

				yield return async3;

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
