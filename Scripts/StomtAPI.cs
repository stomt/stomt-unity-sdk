using System;
using System.Collections;
using System.Net;
using System.Text;
using System.IO;
using UnityEngine;
using System.Security.Cryptography.X509Certificates;
using System.Net.Security;


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
        public string restServerURL;
        public bool NetworkError { get; set; }

		/// <summary>
		/// References a method to be called when the asynchronous feed download completes.
		/// </summary>
		/// <param name="feed">The list of stomt items from the requested feed.</param>
		public delegate void FeedCallback(StomtItem[] feed);

        public StomtConfig config;

		#region Inspector Variables
		[SerializeField]
		[Tooltip("The application ID for your game. Create one on https://www.stomt.com/dev/my-apps/.")]
		string _appId = "";
		[SerializeField]
		[Tooltip("The ID of the target page for your game on https://www.stomt.com/.")]
		string _targetId = "";
		#endregion

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
		public string TargetId
		{
			get { return _targetId; }
		}
		/// <summary>
		/// The name of your target page.
		/// </summary>
		public string TargetName { get; set; }
        public string TargetImageURL { get; set; }
		/// <summary>
		/// Requests the asynchronous feed download from your game's target.
		/// </summary>
		/// <param name="callback">The <see cref="FeedCallback"/> delegate.</param>
		/// <param name="offset">The offset from feed begin.</param>
		/// <param name="limit">The maximum amount of stomts to load.</param>
		public void LoadFeed(FeedCallback callback, int offset = 0, int limit = 15)
		{
			LoadFeed(_targetId, callback, offset, limit);
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
            track.target_id = this.TargetId;

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

            StartCoroutine(SendTrack(jsonTrack.ToString()));
        }

        IEnumerator SendTrack(string json)
        {
            var data = Encoding.UTF8.GetBytes(json);

            HttpWebRequest request = WebRequest("POST", string.Format("{0}/tracks", restServerURL));
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
            }
            catch (WebException ex)
            {
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
        }

        public void SendSubscription(string email)
        {
            var jsonSubscription = new StringBuilder();
            var writerSubscription = new LitJson.JsonWriter(jsonSubscription);

            writerSubscription.WriteObjectStart();

            writerSubscription.WritePropertyName("email");
            writerSubscription.Write(email);

            writerSubscription.WriteObjectEnd();

            StartCoroutine(SendSubscriptionAsJson(jsonSubscription.ToString()));
        }

        IEnumerator SendSubscriptionAsJson(string json)
        {
            var data = Encoding.UTF8.GetBytes(json);

            HttpWebRequest request = WebRequest("POST", string.Format("{0}/authentication/subscribe", restServerURL));
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
            }
            catch (WebException ex)
            {
                Debug.Log("EMail not correct: " + ex.ToString() );
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

            this.config.SetSubscribed(true);
            this.SendTrack(this.CreateTrack("auth", "subscribed"));
        }

        void Awake()
        {
            this.config = new StomtConfig();
            this.config.Load();
            StartCoroutine(LoadTarget(_targetId));
            NetworkError = false;
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

			// TODO: Workaround to accept the stomt SSL certificate. This should be replaced with a proper solution.
			ServicePointManager.ServerCertificateValidationCallback += delegate { return true; };

			// Load target information
			TargetName = _targetId;
			StartCoroutine(LoadTarget(_targetId));
		}

		HttpWebRequest WebRequest(string method, string url)
		{
			var request = (HttpWebRequest)System.Net.WebRequest.Create(url);
			request.Method = method;
			request.Accept = request.ContentType = "application/json";
			request.UserAgent = string.Format("Unity/{0} ({1})", Application.unityVersion, Application.platform);
			request.Headers["appid"] = _appId;

			if (!string.IsNullOrEmpty(this.config.GetAccessToken()))
			{
				request.Headers["accesstoken"] = this.config.GetAccessToken();
			}

			return request;
		}
		
		IEnumerator LoadTarget(string target)
		{
			HttpWebRequest request = WebRequest("GET", string.Format("{0}/targets/{1}", restServerURL, target));

            // Workaround for certificate problem
            ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;

            //////////////////////////////////////////////////////////////////
            // Send request and wait for response
            //////////////////////////////////////////////////////////////////
			
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
                this.NetworkError = false;
			}
			catch (WebException ex)
			{
                this.NetworkError = true;
                Debug.LogException(ex);
                Debug.Log("Maybe wrong target id");
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

            // Read Data
			responseData = responseData["data"];

			TargetName = (string)responseData["displayname"];
            TargetImageURL = (string)responseData["images"]["profile"][0];



		}
		
		IEnumerator LoadFeedAsync(string target, FeedCallback callback, int offset, int limit)
		{
			HttpWebRequest request = WebRequest("GET", string.Format("{0}/targets/{1}/stomts/received?offset={2}&limit={3}", restServerURL, target, offset, limit));

            // Workaround for certificate problem
            ServicePointManager.ServerCertificateValidationCallback = RemoteCertificateValidationCallback;


            //////////////////////////////////////////////////////////////////
            // Send request and wait for response
            //////////////////////////////////////////////////////////////////

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


        public WWW LoadTargetImage()
        {
            
            // Start download
            if(TargetImageURL != null)
            { 
                var www = new WWW(TargetImageURL);
                while (!www.isDone)
                {
                    // wait until the download is done
                }

                return www;
            }
            else
            {
                return null;
            }
        }

        public bool RemoteCertificateValidationCallback(System.Object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
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
	}
}
