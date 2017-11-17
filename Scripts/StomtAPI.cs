using System;
using System.Collections;
using System.Net;
using System.Text;
using System.IO;
using UnityEngine;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;
using System.Threading; 


namespace Stomt
{


    /// <summary>
    /// A single stomt-track.
    /// </summary>
    public struct StomtTrack
    {
        public string device_platform { get; set; }
        public string device_id { get; set; }
        public string sdk_type { get; set; }
        public string sdk_version { get; set; }
        public string sdk_integration { get; set; }
        public string target_id { get; set; }
        public string stomt_id { get; set; }
        public string event_category { get; set; }
        public string event_action { get; set; }
        public string event_label { get; set; }
    }

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
		[Tooltip("The ID of the target page for your game on https://www.stomt.com/.")]
		string _targetId = "";
		#endregion

		public string restServerURL;

		public StomtConfig config;

        
		/// <summary>
		/// The targets amount of received stomts.
		/// </summary>
		public int amountStomtsReceived { get; set; }

        /// <summary>
        /// The users amount of created stomts.
        /// </summary>
        public int amountStomtsCreated { get; set; }

        /// <summary>
        /// The stomt username.
        /// </summary>
        public string UserDisplayname { get; set; }

        /// <summary>
        /// The stomt user ID.
        /// </summary>
        public string UserID { get; set; }

		/// <summary>
		/// Flag if client is offline.
		/// </summary>
		public bool NetworkError { get; set; }

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
		public string TargetDisplayname { get; set; }

		/// <summary>
		/// The image url of your target page.
		/// </summary>
        public string TargetImageURL { get; set; }



		/// <summary>
		/// Creates a new anonymous stomt on the game's target.
		/// </summary>
		/// <param name="positive">The stomt type. True for "I like" and false for "I wish".</param>
		/// <param name="text">The stomt message.</param>
		public void CreateStomt(bool positive, string text)
		{
			CreateStomt(positive, _targetId, text);
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

        public void CreateStomtWidthFile(bool positive, string text, string file, string fileTag)
        {
            CreateStomtWidthFile(positive, this.TargetID, text, file, fileTag);
        }

        public void CreateStomtWidthFile(bool positive, string target, string text, string file, string fileTag)
        {
            StartCoroutine(CreateStomtWidthFileAsync(positive, target, text, file, fileTag));
        }

        IEnumerator CreateStomtWidthFileAsync(bool positive, string target, string text, string file, string fileTag)
        {
            if (string.IsNullOrEmpty(file))
            {
                CreateStomt(positive, target, text);
                yield break;
            }

            string jsonFileUpload = ConstructFileUploadAsJson(file, fileTag);

            string jsonStomt = ConstructStomtWidthFileAsJson(positive, target, text, "{file_uid}");

            StartCoroutine(CreateStomtWithFileAsync(jsonFileUpload, jsonStomt));
        }

        public void CreateStomtWidthImageAndFile(bool positive, string text, Texture2D image, string file, string fileTag)
        {
            this.CreateStomtWidthImageAndFile(positive, this.TargetID, text, image, file, fileTag);
        }

        public void CreateStomtWidthImageAndFile(bool positive, string target, string text, Texture2D image, string file, string fileTag)
        {
            StartCoroutine(CreateStomtWidthImageAndFileAsync(positive, target, text, image, file, fileTag));
        }

        IEnumerator CreateStomtWidthImageAndFileAsync(bool positive, string target, string text, Texture2D image, string file, string fileTag)
        {
            if (image == null && string.IsNullOrEmpty(file) )
            {
                CreateStomt(positive, target, text);
                yield break;
            }

            string jsonImageUpload = ConstructImageUploadAsJson(image);
            string jsonFileUpload = ConstructFileUploadAsJson(file, fileTag);

            string jsonStomt = ConstructStomtWithImageAndFileAsJson(positive, target, text, "{img_name}", "{file_uid}");


            StartCoroutine(CreateStomtWithImageAndFileAsync(jsonImageUpload, jsonFileUpload, jsonStomt));
        }

		/// <summary>
		/// Creates a new anonymous stomt on the game's target with an image attached to it.
		/// </summary>
		/// <param name="positive">The stomt type. True for "I like" and false for "I wish".</param>
		/// <param name="text">The stomt message.</param>
		/// <param name="image">The image texture to upload and attach to the stomt.</param>
		public void CreateStomtWithImage(bool positive, string text, Texture2D image)
		{
			CreateStomtWithImage(positive, _targetId, text, image);
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
			writerImage.Write(Convert.ToBase64String(imageBytes));
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

        public string ConstructFileUploadAsJson(string file, string fileNameOrTag)
        {
            if (string.IsNullOrEmpty(file))
            {
                Debug.Log(" file IsNullOrEmpty");
                return "";
            }

            // Convert to Base64
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(file);
            file = System.Convert.ToBase64String(plainTextBytes);

            var jsonFileUpload = new StringBuilder();
            var writerImage = new LitJson.JsonWriter(jsonFileUpload);

            writerImage.WriteObjectStart();
            writerImage.WritePropertyName("files");
            writerImage.WriteObjectStart();
            writerImage.WritePropertyName("stomt");
            writerImage.WriteArrayStart();
            writerImage.WriteObjectStart();
            writerImage.WritePropertyName("data");
            writerImage.Write(file);
            writerImage.WritePropertyName("filename");
            writerImage.Write(fileNameOrTag);
            writerImage.WriteObjectEnd();
            writerImage.WriteArrayEnd();
            writerImage.WriteObjectEnd();
            writerImage.WriteObjectEnd();

            return jsonFileUpload.ToString();
        }

        public string ConstructImageUploadAsJson(Texture2D image)
        {
            byte[] imageBytes = image.EncodeToPNG();

            if (imageBytes == null)
            {
                return "";
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
            writerImage.Write(Convert.ToBase64String(imageBytes));
            writerImage.WriteObjectEnd();
            writerImage.WriteArrayEnd();
            writerImage.WriteObjectEnd();
            writerImage.WriteObjectEnd();

            return jsonImage.ToString();
        }

        public string ConstructStomtWithImageAsJson(bool positive, string target, string text, string img_name)
        {
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

            return jsonStomt.ToString();
        }

        private string ConstructStomtWidthOptFileImageAsJson(bool positive, string target, string text, string img_name, string file_uid)
        {
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

            if(!string.IsNullOrEmpty(img_name))
            {
                writerStomt.WritePropertyName("img_name");
                writerStomt.Write("{img_name}");
            }

            if(!string.IsNullOrEmpty(file_uid))
            {
                writerStomt.WritePropertyName("files");
                writerStomt.WriteObjectStart();

                writerStomt.WritePropertyName("stomt");
                writerStomt.WriteObjectStart();

                writerStomt.WritePropertyName("file_uid");
                writerStomt.Write(file_uid);

                writerStomt.WriteObjectEnd();
                writerStomt.WriteObjectEnd();
            }

            writerStomt.WriteObjectEnd();

            return jsonStomt.ToString();
        }

        public string ConstructStomtWidthFileAsJson(bool positive, string target, string text, string file_uid)
        {
            return ConstructStomtWidthOptFileImageAsJson(positive, target, text, "", file_uid);
        }

        public string ConstructStomtWithImageAndFileAsJson(bool positive, string target, string text, string img_name, string file_uid)
        {
            return ConstructStomtWidthOptFileImageAsJson(positive, target, text, img_name, file_uid);
        }

        public string ConstructStomtAsJson(bool positive, string target, string text)
        {
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
            writerStomt.WriteObjectEnd();

            return jsonStomt.ToString();
        }
			
		IEnumerator CreateStomtAsync(string json)
		{
			var data = Encoding.UTF8.GetBytes(json);

			HttpWebRequest request = WebRequest("POST", string.Format("{0}/stomts", restServerURL));
			request.ContentLength = data.Length;

			// Workaround for certificate problem
			ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;

			//////////////////////////////////////////////////////////////////
			// Send request
			//////////////////////////////////////////////////////////////////

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

			// Workaround for certificate problem
			ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;

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
				this.NetworkError = false;
			}
			catch (WebException ex)
			{
				this.NetworkError = true;
				Debug.LogWarning("Could not send stomt: " + ex.ToString());
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

			LitJson.JsonData responseData = LitJson.JsonMapper.ToObject(responseDataText);

			if (responseData.Keys.Contains("error"))
			{
				Debug.LogError((string)responseData["error"]["msg"]);
				yield break;
			}

			// Store access token
			if (responseData.Keys.Contains("meta"))
			{
				string accesstoken = (string)responseData["meta"]["accesstoken"];
				this.config.SetAccessToken(accesstoken);
			}

			string stomt_id = (string)responseData["data"]["id"];

			this.SendTrack(this.CreateTrack("stomt", "submit", stomt_id));

		}

		IEnumerator CreateStomtWithImageAsync(string jsonImage, string jsonStomt)
		{
			var data = Encoding.UTF8.GetBytes(jsonImage);

			HttpWebRequest request = WebRequest("POST", string.Format("{0}/images", restServerURL));
			request.ContentLength = data.Length;

			// Workaround for certificate problem
			ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;

			//////////////////////////////////////////////////////////////////
			// Send request
			//////////////////////////////////////////////////////////////////

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

			// Workaround for certificate problem
			ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;

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
				this.NetworkError = false;
			}
			catch (WebException ex)
			{ 
				this.NetworkError = true;
				Debug.LogException(ex);
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

			LitJson.JsonData responseData = LitJson.JsonMapper.ToObject(responseDataText);

			if (responseData.Keys.Contains("error"))
			{
				Debug.LogError((string)responseData["error"]["msg"]);
				yield break;
			}

			// Store access token
			if(responseData.Keys.Contains("meta"))
			{
				string accesstoken = (string)responseData["meta"]["accesstoken"];
				this.config.SetAccessToken(accesstoken);
			}

			var imagename = (string)responseData["data"]["images"]["stomt"]["name"];

			yield return StartCoroutine(CreateStomtAsync(jsonStomt.Replace("{img_name}", imagename)));
		}

		IEnumerator CreateStomtWithFileAsync(string jsonFile, string jsonStomt)
		{
			var data = Encoding.UTF8.GetBytes(jsonFile);

			HttpWebRequest request = WebRequest("POST", string.Format("{0}/files", restServerURL));
			request.ContentLength = data.Length;

			// Workaround for certificate problem
			ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;

			//////////////////////////////////////////////////////////////////
			// Send request
			//////////////////////////////////////////////////////////////////

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

			// Workaround for certificate problem
			ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;

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
				this.NetworkError = false;
			}
			catch (WebException ex)
			{
				this.NetworkError = true;
				Debug.LogException(ex);
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

			LitJson.JsonData responseData = LitJson.JsonMapper.ToObject(responseDataText);

			if (responseData.Keys.Contains("error"))
			{
				Debug.LogError((string)responseData["error"]["msg"]);
				yield break;
			}

			// Store access token
			if (responseData.Keys.Contains("meta"))
			{
				string accesstoken = (string)responseData["meta"]["accesstoken"];
				this.config.SetAccessToken(accesstoken);
			}

			var filename = (string)responseData["data"]["files"]["stomt"]["file_uid"];

			yield return StartCoroutine(CreateStomtAsync(jsonStomt.Replace("{file_uid}", filename)));
		}

		IEnumerator CreateStomtWithImageAndFileAsync(string jsonImage, string jsonFile, string jsonStomt)
		{
			var data = Encoding.UTF8.GetBytes(jsonFile);

			HttpWebRequest request = WebRequest("POST", string.Format("{0}/files", restServerURL));
			request.ContentLength = data.Length;

			// Workaround for certificate problem
			ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;

			//////////////////////////////////////////////////////////////////
			// Send request
			//////////////////////////////////////////////////////////////////

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

			// Workaround for certificate problem
			ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;

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
				this.NetworkError = false;
			}
			catch (WebException ex)
			{
				this.NetworkError = true;
				Debug.LogException(ex);
				StartCoroutine(CreateStomtAsync(jsonStomt));
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

			LitJson.JsonData responseData = LitJson.JsonMapper.ToObject(responseDataText);

			if (responseData.Keys.Contains("error"))
			{
				Debug.LogError((string)responseData["error"]["msg"]);
				yield break;
			}

			// Store access token
			if (responseData.Keys.Contains("meta"))
			{
				string accesstoken = (string)responseData["meta"]["accesstoken"];
				this.config.SetAccessToken(accesstoken);
			}

			var filename = (string)responseData["data"]["files"]["stomt"]["file_uid"];

			yield return StartCoroutine(CreateStomtWithImageAsync(jsonImage, jsonStomt.Replace("{file_uid}", filename)));
		}

	

        void Awake()
        {
            this.config = new StomtConfig();
            this.config.Load();

            NetworkError = false;

			// TODO: Workaround to accept the stomt SSL certificate. This should be replaced with a proper solution.
			ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };
			//ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;
        }

		void Start()
		{
			if (string.IsNullOrEmpty(_appId))
			{
				throw new ArgumentException("The stomt application ID variable cannot be empty.");
			}
			if (string.IsNullOrEmpty(_targetId))
			{
				throw new ArgumentException("The stomt target ID variable cannot be empty.");
			}

			TargetDisplayname = _targetId;
		}


		// Tracks
		public StomtTrack CreateTrack(string event_category, string event_action)
		{
			return CreateTrack(event_category, event_action, "", "");
		}

		public StomtTrack CreateTrack(string event_category, string event_action, string stomt_id)
		{
			return CreateTrack(event_category, event_action, "", stomt_id);
		}

		public StomtTrack CreateTrack(string event_category, string event_action, string event_label, string stomt_id)
		{
			StomtTrack track = new StomtTrack();

			track.event_category = event_category;
			track.event_action = event_action;
			track.event_label = event_label;
			track.stomt_id = stomt_id;

			track.device_platform = Application.platform.ToString();
			track.device_id = SystemInfo.deviceUniqueIdentifier;
			track.sdk_type = "Unity" + Application.unityVersion;
			track.sdk_version = "Beta - 2.0";
			track.sdk_integration = Application.productName;
			track.target_id = this.TargetID;

			return track;
		}

		public void SendTrack(StomtTrack track)
		{
			var jsonTrack = new StringBuilder();
			var writerTrack = new LitJson.JsonWriter(jsonTrack);

			writerTrack.WriteObjectStart();

			writerTrack.WritePropertyName("device_platform");
			writerTrack.Write(track.device_platform);

			writerTrack.WritePropertyName("device_id");
			writerTrack.Write(track.device_id);

			writerTrack.WritePropertyName("sdk_type");
			writerTrack.Write(track.sdk_type);

			writerTrack.WritePropertyName("sdk_version");
			writerTrack.Write(track.sdk_version);

			writerTrack.WritePropertyName("sdk_integration");
			writerTrack.Write(track.sdk_integration);

			writerTrack.WritePropertyName("target_id");
			writerTrack.Write(track.target_id);

			writerTrack.WritePropertyName("stomt_id");
			writerTrack.Write(track.stomt_id);

			writerTrack.WritePropertyName("event_category");
			writerTrack.Write(track.event_category);

			writerTrack.WritePropertyName("event_action");
			writerTrack.Write(track.event_action);

			if (!String.IsNullOrEmpty(track.event_label))
			{
				writerTrack.WritePropertyName("event_label");
				writerTrack.Write(track.event_label);
			}

			writerTrack.WriteObjectEnd();

			SendTrack (jsonTrack.ToString());
		}

		public void SendTrack(string json) {
			var url = string.Format("{0}/tracks", restServerURL);
			GetPOSTResponse (url, json, null, null);
		}


		// Public Request Methodes
		public void RequestTargetAndUser(Action<LitJson.JsonData> callback)
        {
			RequestTarget (_targetId, callback);

            if (!string.IsNullOrEmpty(this.config.GetAccessToken()))
            {
				RequestSession (callback);
            }
        }

		public void RequestTarget(string target, Action<LitJson.JsonData> callback)
		{
			var url = string.Format ("{0}/targets/{1}", restServerURL, target);
			GetGETResponse (url, (response) => {
				TargetDisplayname = (string)response["displayname"];
				TargetImageURL = (string)response["images"]["profile"]["url"];
				amountStomtsReceived = (int)response["stats"]["amountStomtsReceived"];

				if (callback != null) {
					callback(response);
				}
			}, (response) => {
				if (response == null) {
					return;
				}
				if (response.StatusCode.ToString().Equals("419")) {
					Debug.Log("RequestAgain");
					RequestTarget(target, callback);
				}
			});
		}

		public void RequestSession(Action<LitJson.JsonData> callback)
		{
			var url = string.Format ("{0}/authentication/session", restServerURL);
			GetGETResponse (url, (response) => {
				amountStomtsCreated = (int)response["user"]["stats"][4];
				UserDisplayname = (string)response["user"]["displayname"];
				UserID = (string)response["user"]["id"];

				if (callback != null) {
					callback(response);
				}
			}, null);
		}

		public void SendSubscription(string email)
		{
			var jsonSubscription = new StringBuilder();
			var writerSubscription = new LitJson.JsonWriter(jsonSubscription);
			writerSubscription.WriteObjectStart();
			writerSubscription.WritePropertyName("email");
			writerSubscription.Write(email);
			writerSubscription.WriteObjectEnd();

			var url = string.Format ("{0}/authentication/subscribe", restServerURL);
			GetPOSTResponse (url, jsonSubscription.ToString(), (response) => {
				this.config.SetSubscribed(true);
				this.SendTrack(this.CreateTrack("auth", "subscribed"));
//				if (callback != null) {
//					callback(response);
//				}
			}, null);
		}

		public void SendStomt(StomtCreation stomtCreation, Action<LitJson.JsonData> callbackSuccess, Action<HttpWebResponse> callbackError)
		{
			// Upload file if pressent (and call function again)
			if (stomtCreation.screenshot != null) {
				SendImage(stomtCreation.screenshot, (response) => {
					var img_name = (string)response["images"]["stomt"]["name"];
					stomtCreation.img_name = img_name;
					stomtCreation.screenshot = null;
					SendStomt(stomtCreation, callbackSuccess, callbackError);
				},  (response) => {
					// upload even when scrennshot upload failed
					stomtCreation.screenshot = null;
					SendStomt(stomtCreation, callbackSuccess, callbackError);
				});
				return;
			}

			// Upload image if pressent (and call function again)
			if (stomtCreation.logs != null) {
				SendFile(stomtCreation.logs, (response) => {
					var file_uid = (string)response["files"]["stomt"]["file_uid"];
					stomtCreation.file_uid = file_uid;
					stomtCreation.logs = null;
					SendStomt(stomtCreation, callbackSuccess, callbackError);
				},  (response) => {
					// upload even when logs upload failed
					stomtCreation.logs = null;
					SendStomt(stomtCreation, callbackSuccess, callbackError);
				});
				return;
			}

			// Submit stomt
			var url = string.Format ("{0}/stomts", restServerURL);
			GetPOSTResponse (url, stomtCreation.ToString(), (response) => {
				string stomt_id = (string)response["id"];
				this.SendTrack(this.CreateTrack("stomt", "submit", stomt_id));

				if (callbackSuccess != null) {
					callbackSuccess(response);
				}
			}, callbackError);
		}

		private void SendFile(string fileContent, Action<LitJson.JsonData> callbackSuccess, Action<HttpWebResponse> callbackError) {
			// Convert to Base64
			var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(fileContent);
			var file = System.Convert.ToBase64String(plainTextBytes);

			// Build Body
			var jsonFileUpload = new StringBuilder();
			var writerImage = new LitJson.JsonWriter(jsonFileUpload);

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

		private void SendImage(Texture2D image, Action<LitJson.JsonData> callbackSuccess, Action<HttpWebResponse> callbackError) {
			// Convert to Base64
			byte[] imageBytes = image.EncodeToPNG();
			var imageContent = Convert.ToBase64String (imageBytes);

			// Build Body
			var jsonImage = new StringBuilder();
			var writerImage = new LitJson.JsonWriter(jsonImage);

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

		private void GetGETResponse(string uri, Action<LitJson.JsonData> callbackSuccess, Action<HttpWebResponse> callbackError)
		{
			Debug.Log ("GetGETResponse " + uri);
			HttpWebRequest request = WebRequest ("GET", uri);

			this.StartCoroutine(ExecuteRequest(request, uri, null, callbackSuccess, callbackError));
		}

		private void GetPOSTResponse(string uri, string data, Action<LitJson.JsonData> callbackSuccess, Action<HttpWebResponse> callbackError)
		{
			Debug.Log ("GetPOSTResponse " + uri);
			HttpWebRequest request = WebRequest ("POST", uri);

			this.StartCoroutine(ExecuteRequest(request, uri, data, callbackSuccess, callbackError));
		}

		private IEnumerator ExecuteRequest(HttpWebRequest request, string uri, string data, Action<LitJson.JsonData> callbackSuccess, Action<HttpWebResponse> callbackError) {

			//////////////////////////////////////////////////////////////////
			// Add data
			//////////////////////////////////////////////////////////////////
			if (data != null) {
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
					Debug.LogException (ex);
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
				if (errorResponse != null) {
					statusCode = errorResponse.StatusCode.ToString();
				}

				Debug.LogException (ex);
				Debug.Log ("ExecuteRequest exception " + statusCode);

				// Handle invalid Session
				if (statusCode.Equals ("419")) {
					this.config.SetAccessToken("");
				}

				// Handle Offline
				if (errorResponse == null) {
					this.NetworkError = true;
				}

				if (callbackError != null) {
					callbackError (errorResponse);
				}

				yield break;

//					using (var responseStream = ex.Response.GetResponseStream())
//					{
//						if (responseStream == null)
//						{
//							yield break;
//						}
//
//						var buffer = new byte[2048];
//						int length;
//
//						while ((length = responseStream.Read(buffer, 0, buffer.Length)) > 0)
//						{
//							responseDataText += Encoding.UTF8.GetString(buffer, 0, length);
//						}
//					}
//
//					LitJson.JsonData ExceptionResponseData = LitJson.JsonMapper.ToObject(responseDataText);
//
//					if (ExceptionResponseData.Keys.Contains("error"))
//					{
//						Debug.LogError((string)ExceptionResponseData["error"]);
//						yield break;
//					}
//
//					this.NetworkError = true;
//					Debug.LogException(ex);
//					Debug.Log("Maybe wrong target id or accesstoken");
//
//					yield break;

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

			LitJson.JsonData responseData = LitJson.JsonMapper.ToObject(responseDataText);

			if (responseData.Keys.Contains("error"))
			{
				Debug.LogError((string)responseData["error"]["msg"]);
				Debug.Log ("ExecuteRequest error msg " + responseData["error"]["msg"]);
				callbackError(response);
				yield break;
			}

			// Store access token
			if (responseData.Keys.Contains("meta") && responseData["meta"].Keys.Contains("accesstoken"))
			{
				string accesstoken = (string)responseData["meta"]["accesstoken"];
				this.config.SetAccessToken(accesstoken);
			}

			Debug.Log ("ExecuteRequest response " + uri);
			if (callbackSuccess != null) {
				callbackSuccess(responseData["data"]);
			}
		
		}
		       

		// Logs
        public string GetLogFilePath()
        {
            string logFilePath = "";
            //////////////////////////////////////////////////////////////////
            // Windows Paths
            //////////////////////////////////////////////////////////////////

            if(Application.platform == RuntimePlatform.WindowsEditor)
            {
                logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\Unity\\Editor\\Editor.log";
            }
            
            if(Application.platform == RuntimePlatform.WindowsPlayer)
            {
                logFilePath = "_EXECNAME_Data_\\output_log.txt";
            }

            //////////////////////////////////////////////////////////////////
            // OSX Paths
            //////////////////////////////////////////////////////////////////

            if(Application.platform == RuntimePlatform.OSXEditor)
            {
                logFilePath = "~/Library/Logs/Unity/Editor.log";
            }

            if(Application.platform == RuntimePlatform.OSXPlayer)
            {
                logFilePath = "~/Library/Logs/Unity/Player.log";
            }

            //////////////////////////////////////////////////////////////////
            // Linux Paths
            //////////////////////////////////////////////////////////////////

            if(Application.platform == RuntimePlatform.LinuxEditor)
            {
                logFilePath = "~/.config/unity3d/CompanyName/ProductName/Editor.log";
            }

            if(Application.platform == RuntimePlatform.LinuxPlayer)
            {
                logFilePath = "~/.config/unity3d/CompanyName/ProductName/Player.log";
            }

            if(!string.IsNullOrEmpty(logFilePath))
            {
                if (File.Exists(logFilePath))
                {
                    return logFilePath;
                }
            }

            return "";
        }

        public string ReadFile(string FilePath)
        {
			if (string.IsNullOrEmpty(FilePath)) {
				Debug.LogWarning("No FilePath specified");
				return null;
			}

            var fileInfo = new System.IO.FileInfo(FilePath);

            if (fileInfo.Length > 30000000) 
            {
                Debug.LogWarning("Log file too big. Size: " + fileInfo.Length + "Bytes. Path: " + FilePath);
                this.SendTrack(this.CreateTrack("log", "tooBig"));
                return null; 
            }

            string FileCopyPath = FilePath + ".tmp.copy";

            // Copy File for reading an already opened file
            File.Copy(FilePath, FileCopyPath, true);

            // Read File
            StreamReader reader = new StreamReader(FileCopyPath);
            string content = reader.ReadToEnd();

            // Close stream and delete file copy
            reader.Close();
            File.Delete(FilePath + ".tmp.copy");

            return content;
        }
	

		// StomtHandling
		public StomtCreation initStomtCreation()
		{
			StomtCreation stomtCreation = new StomtCreation(this);

			stomtCreation.target_id = this.TargetID;
			stomtCreation.lang = "en";
			stomtCreation.anonym = false;

			return stomtCreation;
		}
	}
}
