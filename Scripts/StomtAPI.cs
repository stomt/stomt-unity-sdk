using System;
using System.Collections;
using System.Net;
using System.Text;
using UnityEngine;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using LitJsonStomt;
using System.Collections.Generic;

namespace Stomt
{
	/// <summary>
	/// Low-level stomt API component.
	/// </summary>
	public class StomtAPI : MonoBehaviour
	{
		#region Inspector Variables
		[SerializeField]
		[Tooltip("The application ID for your game. Create one on https://www.stomt.com/dev/my-apps/.")]
		string _appId = "";
		[SerializeField]
		[Tooltip("The language file: languages.json")]
		public TextAsset languageFile;
		#endregion

		// The ID of the target page for your game on https://www.stomt.com/.
		string _targetId = "";
		private string restServerURL = "https://rest.stomt.com";

		[HideInInspector]
		public string stomtURL = "https://www.stomt.com";

		[HideInInspector]
		public StomtLang lang;
		public string defaultLanguage;
		public bool ForceDefaultLanguage;
		public bool DisableDefaultLabels;
		public bool DebugDisableConfigFile = false;
		private StomtConfig _config = null;
		public StomtConfig config {
			get {
				if (this._config == null)
				{
					this._config = new StomtConfig();
				}
				return this._config;
			}
		}

		public enum SubscriptionType { EMail, Phone };

		/// <summary>
		/// The targets amount of received stomts.
		/// </summary>
		public int amountStomtsReceived {
			get; set;
		}

		/// <summary>
		/// The users amount of created stomts.
		/// </summary>
		public int amountStomtsCreated {
			get; set;
		}

		/// <summary>
		/// The stomt username.
		/// </summary>
		public string UserDisplayname {
			get; set;
		}

		/// <summary>
		/// The stomt user ID.
		/// </summary>
		public string UserID {
			get; set;
		}

		/// <summary>
		/// Flag if client is offline.
		/// </summary>
		public bool NetworkError {
			get; set;
		}

		/// <summary>
		/// The application ID for your game.
		/// </summary>
		public string AppId
		{
			get { return _appId; }
		}

		/// <summary>
		/// The target page ID for your game.
		/// </summary>
		public string TargetID
		{
			get { return _targetId; }
		}

		/// <summary>
		/// The name of your target page.
		/// </summary>
		public string TargetDisplayname {
			get; set;
		}

		/// <summary>
		/// The image url of your target page.
		/// </summary>
		public string TargetImageURL {
			get; set;
		}

		/// <summary>
		/// labels attached to the stomt.
		/// </summary>
		public string[] Labels;

		/// <summary>
		/// AdditionalData attached to the stomt.
		/// </summary>
		private List<List<string> > CustomKeyValuePairs;

		// Called once at initialization
		public StomtAPI()
		{
			CustomKeyValuePairs = new List<List<string> >();
		}

		// Called once when script is enabled
		void Awake()
		{
			// Debug/Testing on the test-server
			if (this.AppId.Equals("Copy_your_AppID_here"))
			{
				this._appId = "r7BZ0Lz4phqYB0Rl7xPGcHLLR";
				this.restServerURL = "https://test.rest.stomt.com";
				this.stomtURL = "https://test.stomt.com";
			}
			else
			{
				this.restServerURL = "https://rest.stomt.com";
				this.stomtURL = "https://stomt.com";
				this.DebugDisableConfigFile = false;
			}

			NetworkError = false;

			this.lang = new StomtLang(this, defaultLanguage);

			// TODO: Workaround to accept the stomt SSL certificate. This should be replaced with a proper solution.
			// ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
			ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;
		}

		void Start()
		{
			if (DebugDisableConfigFile)
			{
				this.config.SetLoggedin(false);
				this.config.SetSubscribed(false);
			}
			else
			{
				this.config.Load();
			}

			if (string.IsNullOrEmpty(_appId))
			{
				throw new ArgumentException("The stomt application ID variable cannot be empty.");
			}

			// TargetDisplayname = _targetId;
			TargetDisplayname = "Loading";
		}


		// Track Handling
		public StomtTrack initStomtTrack()
		{
			StomtTrack stomtTrack = new StomtTrack(this);

			stomtTrack.device_platform = Application.platform.ToString();
			stomtTrack.device_id = SystemInfo.deviceUniqueIdentifier;
			stomtTrack.sdk_type = "Unity" + Application.unityVersion;
			stomtTrack.sdk_version = "Beta - 2.0";
			stomtTrack.sdk_integration = Application.productName;
			stomtTrack.target_id = this.TargetID;

			return stomtTrack;
		}

		public void SendTrack(StomtTrack track, Action<LitJsonStomt.JsonData> callbackSuccess, Action<HttpWebResponse> callbackError)
		{
			var url = string.Format("{0}/tracks", restServerURL);
			GetPOSTResponse (url, track.ToString(), callbackSuccess, callbackError);
		}


		// Target / Session Handling
		public void RequestTargetAndUser(Action<LitJsonStomt.JsonData> callbackSuccess, Action<HttpWebResponse> callbackError)
		{
			RequestTarget (_targetId, callbackSuccess, callbackError);

			if (!string.IsNullOrEmpty(this.config.GetAccessToken()))
			{
				RequestSession (callbackSuccess, callbackError);
			}
		}

		public void RequestTarget(string target, Action<LitJsonStomt.JsonData> callbackSuccess, Action<HttpWebResponse> callbackError)
		{
			var url = string.Format ("{0}/targets/", restServerURL, target);
			GetGETResponse (url, (response) => {
				this._targetId = (string)response["id"];
				TargetDisplayname = (string)response["displayname"];
				TargetImageURL = (string)response["images"]["profile"]["url"];
				amountStomtsReceived = (int)response["stats"]["amountStomtsReceived"];

				if (callbackSuccess != null)
				{
					callbackSuccess(response);
				}
			}, (response) => {
				if (response == null)
				{
					return;
				}
				if (response.StatusCode.ToString().Equals("419"))
				{
					RequestTarget(target, callbackSuccess, callbackError);
				}
				if (callbackError != null)
				{
					callbackError(response);
				}
			});
		}

		public void RequestSession(Action<LitJsonStomt.JsonData> callbackSuccess, Action<HttpWebResponse> callbackError)
		{
			var url = string.Format ("{0}/authentication/session", restServerURL);
			GetGETResponse (url, (response) => {
				amountStomtsCreated = (int)response["user"]["stats"][4];
				UserDisplayname = (string)response["user"]["displayname"];
				UserID = (string)response["user"]["id"];

				if (callbackSuccess != null)
				{
					callbackSuccess(response);
				}
			}, callbackError);
		}

		public void SendSubscription(string addressOrNumber, SubscriptionType type, Action<LitJsonStomt.JsonData> callbackSuccess, Action<HttpWebResponse> callbackError)
		{
			var jsonSubscription = new StringBuilder();
			var writerSubscription = new LitJsonStomt.JsonWriter(jsonSubscription);
			writerSubscription.WriteObjectStart();

			switch (type)
			{
				case SubscriptionType.EMail:
					writerSubscription.WritePropertyName("email");
					break;

				case SubscriptionType.Phone:
					writerSubscription.WritePropertyName("phone");
					break;
			}

			writerSubscription.Write(addressOrNumber);
			writerSubscription.WriteObjectEnd();

			var url = string.Format ("{0}/authentication/subscribe", restServerURL);
			GetPOSTResponse (url, jsonSubscription.ToString(), (response) => {
				this.config.SetSubscribed(true);

				var track = initStomtTrack();
				track.event_category = "auth";
				track.event_action = "subscribed";
				track.event_label = type.ToString();
				track.save ();

				if (callbackSuccess != null)
				{
					callbackSuccess(response);
				}
			}, callbackError);
		}

		// Stomt Handling
		public StomtCreation initStomtCreation()
		{
			StomtCreation stomtCreation = new StomtCreation(this);

			stomtCreation.DisableDefaultLabels = DisableDefaultLabels;

			stomtCreation.target_id = this.TargetID;
			stomtCreation.lang = "en";
			stomtCreation.anonym = false;
			stomtCreation.labels = Labels;
			stomtCreation.CustomKeyValuePairs = this.CustomKeyValuePairs;

			return stomtCreation;
		}

		public void SendStomt(StomtCreation stomtCreation, Action<LitJsonStomt.JsonData> callbackSuccess, Action<HttpWebResponse> callbackError)
		{
			// Upload file if pressent (and call function again)
			if (stomtCreation.screenshot != null)
			{
				SendImage(stomtCreation.screenshot, (response) => {
					var img_name = (string)response["images"]["stomt"]["name"];
					stomtCreation.img_name = img_name;
					stomtCreation.screenshot = null;
					SendStomt(stomtCreation, callbackSuccess, callbackError);
				}, (response) => {
					// upload even when scrennshot upload failed
					stomtCreation.screenshot = null;
					SendStomt(stomtCreation, callbackSuccess, callbackError);
				});
				return;
			}

			// Upload image if pressent (and call function again)
			if (stomtCreation.logs != null)
			{
				SendFile(stomtCreation.logs, (response) => {
					var file_uid = (string)response["files"]["stomt"]["file_uid"];
					stomtCreation.file_uid = file_uid;
					stomtCreation.logs = null;
					SendStomt(stomtCreation, callbackSuccess, callbackError);
				}, (response) => {
					// upload even when logs upload failed
					stomtCreation.logs = null;
					SendStomt(stomtCreation, callbackSuccess, callbackError);
				});
				return;
			}

			// Submit stomt
			var url = string.Format ("{0}/stomts", restServerURL);

			// Send Stomt
			GetPOSTResponse (url, stomtCreation.ToString(), (response) => {
				amountStomtsCreated += 1;
				string stomt_id = (string)response["id"];

				var track = initStomtTrack();
				track.event_category = "stomt";
				track.event_action = "submit";
				track.stomt_id = stomt_id;
				track.save ();

				if (callbackSuccess != null)
				{
					callbackSuccess(response);
				}
			}, callbackError);
		}

		private void SendFile(string fileContent, Action<LitJsonStomt.JsonData> callbackSuccess, Action<HttpWebResponse> callbackError) {
			// Convert to Base64
			var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(fileContent);
			var file = System.Convert.ToBase64String(plainTextBytes);

			// Build Body
			var jsonFileUpload = new StringBuilder();
			var writerImage = new LitJsonStomt.JsonWriter(jsonFileUpload);

			writerImage.WriteObjectStart();
			writerImage.WritePropertyName("files");
			writerImage.WriteObjectStart();
			writerImage.WritePropertyName("stomt");
			writerImage.WriteArrayStart();
			writerImage.WriteObjectStart();
			writerImage.WritePropertyName("data");
			writerImage.Write(file);
			writerImage.WritePropertyName("filename");
			writerImage.Write("UnityLogs");
			writerImage.WriteObjectEnd();
			writerImage.WriteArrayEnd();
			writerImage.WriteObjectEnd();
			writerImage.WriteObjectEnd();

			// Send Request
			var url = string.Format("{0}/files", restServerURL);
			GetPOSTResponse (url, jsonFileUpload.ToString(), callbackSuccess, callbackError);
		}

		private void SendImage(Texture2D image, Action<LitJsonStomt.JsonData> callbackSuccess, Action<HttpWebResponse> callbackError) {
			// Convert to Base64
			byte[] imageBytes = image.EncodeToPNG();
			var imageContent = Convert.ToBase64String (imageBytes);

			// Build Body
			var jsonImage = new StringBuilder();
			var writerImage = new LitJsonStomt.JsonWriter(jsonImage);

			writerImage.WriteObjectStart();
			writerImage.WritePropertyName("images");
			writerImage.WriteObjectStart();
			writerImage.WritePropertyName("stomt");
			writerImage.WriteArrayStart();
			writerImage.WriteObjectStart();
			writerImage.WritePropertyName("data");
			writerImage.Write(imageContent);
			writerImage.WriteObjectEnd();
			writerImage.WriteArrayEnd();
			writerImage.WriteObjectEnd();
			writerImage.WriteObjectEnd();

			// Send Request
			var url = string.Format("{0}/images", restServerURL);
			GetPOSTResponse (url, jsonImage.ToString(), callbackSuccess, callbackError);
		}


		// Private Request Handlers
		private bool RemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
		{
			bool isOk = true;
			// If there are errors in the certificate chain, look at each error to determine the cause.
			if (sslPolicyErrors != SslPolicyErrors.None)
			{
				for (int i = 0; i < chain.ChainStatus.Length; i++)
				{
					if (chain.ChainStatus[i].Status != X509ChainStatusFlags.RevocationStatusUnknown)
					{
						chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;
						chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
						chain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 1, 0);
						chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllFlags;
						bool chainIsValid = chain.Build((X509Certificate2)certificate);
						if (!chainIsValid)
						{
							isOk = false;
						}
					}
				}
			}
			return isOk;
		}

		private HttpWebRequest WebRequest(string method, string url)
		{
			var request = (HttpWebRequest)System.Net.WebRequest.Create(url);
			request.Method = method;
			request.Accept = request.ContentType = "application/json";
			request.UserAgent = string.Format("Unity/{0} ({1})", Application.unityVersion, Application.platform);
			request.Headers["appid"] = _appId;

			if (!string.IsNullOrEmpty(this.config.GetAccessToken()))
			{
				request.Headers ["accesstoken"] = this.config.GetAccessToken();
			}

			return request;
		}

		private void GetGETResponse(string uri, Action<LitJsonStomt.JsonData> callbackSuccess, Action<HttpWebResponse> callbackError)
		{
			HttpWebRequest request = WebRequest ("GET", uri);

			this.StartCoroutine(ExecuteRequest(request, uri, null, callbackSuccess, callbackError));
		}

		private void GetPOSTResponse(string uri, string data, Action<LitJsonStomt.JsonData> callbackSuccess, Action<HttpWebResponse> callbackError)
		{
			HttpWebRequest request = WebRequest ("POST", uri);

			this.StartCoroutine(ExecuteRequest(request, uri, data, callbackSuccess, callbackError));
		}

		private IEnumerator ExecuteRequest(HttpWebRequest request, string uri, string data, Action<LitJsonStomt.JsonData> callbackSuccess, Action<HttpWebResponse> callbackError) {

			//////////////////////////////////////////////////////////////////
			// Add data
			//////////////////////////////////////////////////////////////////
			if (data != null)
			{
				var bytes = Encoding.UTF8.GetBytes(data);
				request.ContentLength = bytes.Length;

				var async1 = request.BeginGetRequestStream (null, null);

				while (!async1.IsCompleted) {
					yield return null;
				}

				try {
					using (var requestStream = request.EndGetRequestStream (async1)) {
						requestStream.Write (bytes, 0, bytes.Length);
					}
				} catch (WebException ex) {
					Debug.Log (ex);
					this.NetworkError = true;
					yield break;
				}
			}

			//////////////////////////////////////////////////////////////////
			// Send request and wait for response
			//////////////////////////////////////////////////////////////////
			var async2 = request.BeginGetResponse (null, null);

			while (!async2.IsCompleted) {
				yield return null;
			}

			HttpWebResponse response;
			var responseDataText = string.Empty;

			try
			{
				response = (HttpWebResponse)request.EndGetResponse(async2);
				this.NetworkError = false;
			}
			catch (WebException ex)
			{
				var errorResponse = (HttpWebResponse)ex.Response;
				var statusCode = "";
				if (errorResponse != null)
				{
					statusCode = errorResponse.StatusCode.ToString();
				}
				else
				{
					this.NetworkError = false;
				}

				Debug.Log (ex);
				Debug.Log ("ExecuteRequest exception " + statusCode);
				Debug.Log("Request: " + data);

				// Handle invalid Session
				if (statusCode.Equals ("419"))
				{
					this.config.SetAccessToken("");
				}

				// Handle Offline
				if (errorResponse == null)
				{
					this.NetworkError = true;
				}

				if (callbackError != null)
				{
					callbackError (errorResponse);
				}

				yield break;
			}

			//////////////////////////////////////////////////////////////////
			// Read response stream
			//////////////////////////////////////////////////////////////////

			using (var responseStream = response.GetResponseStream())
			{
				if (responseStream == null)
				{
					yield break;
				}

				var buffer = new byte[2048];
				int length;

				while ((length = responseStream.Read(buffer, 0, buffer.Length)) > 0)
				{
					responseDataText += Encoding.UTF8.GetString(buffer, 0, length);
				}
			}

			//////////////////////////////////////////////////////////////////
			// Analyze JSON data
			//////////////////////////////////////////////////////////////////

			LitJsonStomt.JsonData responseData = LitJsonStomt.JsonMapper.ToObject(responseDataText);

			if (responseData.Keys.Contains("error"))
			{
				Debug.LogError((string)responseData["error"]["msg"]);
				Debug.Log ("ExecuteRequest error msg " + responseData["error"]["msg"]);
				callbackError(response);
				yield break;
			}

			// Store access token
			if (responseData.Keys.Contains("meta") && !responseData["meta"].IsArray)
			{
				if (responseData["meta"].Keys.Contains("accesstoken"))
				{
					string accesstoken = (string)responseData["meta"]["accesstoken"];
					this.config.SetAccessToken(accesstoken);
				}
			}

			if (callbackSuccess != null)
			{
				callbackSuccess(responseData["data"]);
			}
		}


		/// <summary>
		/// AdditionalData attached to the stomt.
		/// @key data name
		/// @value string has to be escaped!
		/// </summary>
		public void AddCustomKeyValuePair(string key, string value)
		{
			List<string> pair = new List<string>();
			pair.Add(key);
			pair.Add(value);

			CustomKeyValuePairs.Add(pair);
		}
	}
}
