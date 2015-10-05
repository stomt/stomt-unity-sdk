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
	public struct Target
	{
		public string id { get; set; }
		public string displayname { get; set; }
	}

	public string id { get; set; }
	public bool negative { get; set; }
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
	[Tooltip("The application identifier for your game. Contact Philipp (philipp.zentner@stomt.com) to request your own.")]
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
	public void RequestFeed(ref List<Stomt> outlist)
	{
		RequestFeed(_targetName, ref outlist);
	}
	/// <summary>
	/// Requests the asynchronous feed download from the specified target.
	/// </summary>
	/// <param name="target">The target to download the feed from.</param>
	/// <param name="outlist">Reference to a list receiving the feed once downloaded.</param>
	public void RequestFeed(string target, ref List<Stomt> outlist)
	{
		StartCoroutine(RequestFeedAsync(target, outlist));
	}

	private IEnumerator RequestFeedAsync(string target, ICollection<Stomt> outlist)
	{
		const int limit = 15;

		var request = (HttpWebRequest)WebRequest.Create(string.Format("https://rest.stomt.com/targets/{0}/stomts/received?limit={1}", target, limit));
		request.Method = "GET";
		request.ContentType = "application/json";
		request.Headers["appid"] = _appId;

		var async1 = request.BeginGetResponse(null, null);

		yield return async1;

		Stream responseStream;

		try
		{
			var response = (HttpWebResponse)request.EndGetResponse(async1);
			responseStream = response.GetResponseStream();
		}
		catch (WebException ex)
		{
			Debug.LogException(ex);
			yield break;
		}

		if (responseStream == null)
		{
			yield break;
		}

		var dataText = string.Empty;
		var dataBuffer = new byte[2048];

		while (true)
		{
			var async2 = responseStream.BeginRead(dataBuffer, 0, dataBuffer.Length, null, null);

			yield return async2;

			int length = responseStream.EndRead(async2);

			if (length <= 0)
			{
				break;
			}

			dataText += Encoding.UTF8.GetString(dataBuffer, 0, length);
		}

		responseStream.Close();

		LitJson.JsonData data = LitJson.JsonMapper.ToObject(dataText);

		if (data.Keys.Contains("error"))
		{
			Debug.LogError((string)data["error"]["msg"]);
			yield break;
		}

		data = data["data"];

		for (int i = 0; i < data.Count; i++)
		{
			outlist.Add(LitJson.JsonMapper.ToObject<Stomt>(data[i].ToJson()));
		}
	}

	/// <summary>
	/// Creates a new anonymous stomt on the game's target.
	/// </summary>
	/// <param name="negative">The stomt type. True for "I wish" and false for "I like".</param>
	/// <param name="text">The stomt message.</param>
	public void CreateStomt(bool negative, string text)
	{
		CreateStomt(negative, _targetName, text);
	}
	/// <summary>
	/// Creates a new anonymous stomt on the specified target.
	/// </summary>
	/// <param name="negative">The stomt type. True for "I wish" and false for "I like".</param>
	/// <param name="target">The target to post the stomt to.</param>
	/// <param name="text">The stomt message.</param>
	public void CreateStomt(bool negative, string target, string text)
	{
		StringBuilder json = new StringBuilder();
		LitJson.JsonWriter writer = new LitJson.JsonWriter(json);

		writer.WriteObjectStart();
		writer.WritePropertyName("anonym");
		writer.Write(true);
		writer.WritePropertyName("negative");
		writer.Write(negative);
		writer.WritePropertyName("target_id");
		writer.Write(target);
		writer.WritePropertyName("text");
		writer.Write(text);
		writer.WriteObjectEnd();

		StartCoroutine(CreateStomtAsync(json.ToString()));
	}

	private IEnumerator CreateStomtAsync(string json)
	{
		var data = Encoding.UTF8.GetBytes(json);

		var request = (HttpWebRequest)WebRequest.Create("https://rest.stomt.com/stomts");
		request.Method = "POST";
		request.ContentType = "application/json";
		request.ContentLength = data.Length;
		request.Headers["appid"] = _appId;

		var async1 = request.BeginGetRequestStream(null, null);

		yield return async1;

		try
		{
			Stream requestStream = request.EndGetRequestStream(async1);
			requestStream.Write(data, 0, data.Length);
			requestStream.Close();
		}
		catch (WebException ex)
		{
			Debug.LogException(ex);
		}
	}
}
