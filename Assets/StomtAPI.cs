using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

/// <summary>
/// Stomt data structure laid out after the JSON representation
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
/// Low-level stomt API component
/// </summary>
public class StomtAPI : MonoBehaviour
{
	[SerializeField]
	[Tooltip("The specific ID for your game. Contact Philipp (philipp.zentner@stomt.com) to request your own ID.")]
	string _AppId = "";
	[SerializeField]
	[Tooltip("The name of the target you created for your game on stomt.")]
	string _TargetName = "";

	public string AppId
	{
		get { return _AppId; }
	}
	public string TargetName
	{
		get { return _TargetName; }
	}

	void Start()
	{
		// TODO: Workaround to accept the stomt SSL certificate. This should be replaced with a proper solution.
		ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
	}

	public void RequestFeed(ref List<Stomt> outlist)
	{
		RequestFeed(_TargetName, ref outlist);
	}
	public void RequestFeed(string target, ref List<Stomt> outlist)
	{
		StartCoroutine(RequestFeedAsync(target, outlist));
	}
	IEnumerator RequestFeedAsync(string target, List<Stomt> outlist)
	{
		const int limit = 15;

		HttpWebRequest request = HttpWebRequest.Create(string.Format("https://rest.stomt.com/targets/{0}/stomts/received?limit={1}", target, limit)) as HttpWebRequest;
		request.Method = "GET";
		request.ContentType = "application/json";
		request.Headers["appid"] = _AppId;

		var async1 = request.BeginGetResponse(null, null);

		yield return async1;

		HttpWebResponse response = null;
		Stream responseStream = null;

		try
		{
			response = request.EndGetResponse(async1) as HttpWebResponse;
			responseStream = response.GetResponseStream();
		}
		catch (WebException ex)
		{
			Debug.LogException(ex);
			yield break;
		}

		string dataText = string.Empty;
		byte[] dataBuffer = new byte[2048];

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
		else
		{
			data = data["data"];
		}

		for (int i = 0; i < data.Count; i++)
		{
			outlist.Add(LitJson.JsonMapper.ToObject<Stomt>(data[i].ToJson()));
		}
	}

	public void CreateStomt(bool negative, string text)
	{
		CreateStomt(negative, _TargetName, text);
	}
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
	IEnumerator CreateStomtAsync(string json)
	{
		byte[] data = Encoding.UTF8.GetBytes(json);

		HttpWebRequest request = HttpWebRequest.Create("https://rest.stomt.com/stomts") as HttpWebRequest;
		request.Method = "POST";
		request.ContentType = "application/json";
		request.ContentLength = data.Length;
		request.Headers["appid"] = _AppId;

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
			yield break;
		}
	}
}
