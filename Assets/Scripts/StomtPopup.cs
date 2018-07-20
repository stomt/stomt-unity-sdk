using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Stomt
{
	[RequireComponent(typeof(StomtAPI))]
	public class StomtPopup : MonoBehaviour
	{
		#region Inspector Variables
		[SerializeField]
		[HideInInspector]
		public GameObject _typeObj;
		[SerializeField]
		[HideInInspector]
		public GameObject _targetObj;
		[SerializeField]
		[HideInInspector]
		public GameObject _messageObj;
		[SerializeField]
		[HideInInspector]
		public GameObject _errorMessage;
		[HideInInspector]
		public GameObject _closeButton;
		[HideInInspector]
		public GameObject _postButton;
		[SerializeField]
		[HideInInspector]
		public GameObject _postButtonSubscription;
		[HideInInspector]
		public GameObject _LayerNetworkError;
		[HideInInspector]
		public GameObject _LayerSuccessfulSent;
		[HideInInspector]
		public GameObject _LayerInput;
		[HideInInspector]
		public GameObject _LayerSubscription;
        [HideInInspector]
        public GameObject _LayerLogin;
        [HideInInspector]
        public GameObject _LayerLoginMessage;
        [HideInInspector]
		public Text _TargetURL;
		[HideInInspector]
		public GameObject _ErrorMessageObject;
		[HideInInspector]
		public Text _ErrorMessageText;
		[SerializeField]
		[HideInInspector]
		public InputField _EmailInput;
		[SerializeField]
		[HideInInspector]
		public Text _STOMTS;
		[SerializeField]
		[HideInInspector]
		public Text _STOMTS_Number;
		[SerializeField]
		[HideInInspector]
		public Text _YOURS;
		[SerializeField]
		[HideInInspector]
		public Text _YOURS_Number;
		[SerializeField]
		[HideInInspector]
		public Text toggleItemEMail;
		[SerializeField]
		[HideInInspector]
		public Text toggleItemSMS;
		[SerializeField]
		[HideInInspector]
		public Text SubscribtionInfoText;
		[SerializeField]
		[HideInInspector]
		public GameObject ArrowFindStomt;
		[SerializeField]
		[HideInInspector]
		public GameObject CustomPlaceholderText;

        public Text MessagePlaceholder;

        // Login Layer
        [SerializeField]
        [HideInInspector]
        public InputField LoginUser;
        [SerializeField]
        [HideInInspector]
        public InputField LoginPassword;
        [SerializeField]
        [HideInInspector]
        public Text LoginMessage;
        [SerializeField]
        [HideInInspector]
        public Text PasswordPlaceholder;
        [SerializeField]
        [HideInInspector]
        public Text LoginButtonText;

        // Subscription Layer
        [SerializeField]
		[HideInInspector]
		public Text SkipButton;
		[SerializeField]
		[HideInInspector]
		public Text GetNotifiedText;

		// Success Layer
		[SerializeField]
		[HideInInspector]
		public Text CreateButtonText;
		[SerializeField]
		[HideInInspector]
		public Text ThankYouText;
		[SerializeField]
		[HideInInspector]
		public Text ArrowText;
		[SerializeField]
		[HideInInspector]
		public Text SentLayerMessage;

		// Error Layer
		[SerializeField]
		[HideInInspector]
		public Text ReconnectText;
		[SerializeField]
		[HideInInspector]
		public Text ErrorHeaderText;
		[SerializeField]
		[HideInInspector]
		public Text ErrorMessageText;

		[SerializeField]
		[HideInInspector]
		GameObject _ui;
		[SerializeField]
		[HideInInspector]
		Canvas _like;
		[SerializeField]
		[HideInInspector]
		Canvas _wish;
		[SerializeField]
		[HideInInspector]
		InputField _message;
		[SerializeField]
		[HideInInspector]
		Text _wouldBecauseText;
		[SerializeField]
		[HideInInspector]
		Text _characterLimit;
		[SerializeField]
		[HideInInspector]
		Text _targetText;
		[SerializeField]
		[HideInInspector]
		Toggle _screenshotToggle;
		[SerializeField]
		[HideInInspector]
		public GameObject placeholderText;
		[SerializeField]
		[HideInInspector]
		public GameObject messageText;
		[SerializeField]
		[HideInInspector]
		public Image TargetIcon;
		#endregion

		// editable in GUI
		public KeyCode _toggleKey = KeyCode.F1;
		public string DisplayGameName;
		public Texture2D ProfileImageTexture;
		public bool UploadLogFile = true;
		public bool PrefetchTarget = false;
		public bool ShowCloseButton = true;
		public bool ShowDefaultText = true; // Activates the would/because text
		public bool ShowWidgetOnStart = false;

		// callbacks
		public delegate void StomtAction();
		public static event StomtAction OnStomtSend;
		public static event StomtAction OnWidgetClosed;
		public static event StomtAction OnWidgetOpen;

		// internal
		private StomtAPI _api;
		private Texture2D _screenshot;
		private StomtLog _log;
		private WWW ImageDownload;
		private int TargetNameCharLimit = 20;
		private int ErrorMessageCharLimit = 35;
		private bool TargetImageApplied = false;
		private bool StartedTyping = false;
		private bool IsErrorState = false;
		private int CharLimit = 120;
		private bool useEmailOnSubscribe = true;
		private bool onMobile = false;
		private string wouldText;
		private string becauseText;
		private Vector3 targetLocalStartPostion;
		private Vector3 placeholderLocalStartPosition;
		enum UILayer { Input, Subscription, Success, Error, Login, LoginMessage };
		private UILayer CurrentLayer;
        private string langPlaceholderText;
        private string langPlaceholderOffset = " ";

		//////////////////////////////////////////////////////////////////
		// General (used for all layers)
		//////////////////////////////////////////////////////////////////

		// Called once at initialization
		void Awake()
		{
			_ui.SetActive(false);

			// Set local start positions
			targetLocalStartPostion = _targetObj.GetComponent<RectTransform>().localPosition;
			placeholderLocalStartPosition = placeholderText.transform.localPosition;
			placeholderLocalStartPosition.x -= 130; // offset
		}

		// Called once when script is enabled
		void Start()
		{
			LoginPassword.contentType = InputField.ContentType.Password;
			LoginPassword.inputType = InputField.InputType.Password;
			if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
			{
				this.onMobile = true;
			}

			_api = GetComponent<StomtAPI>();
			_screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

			CurrentLayer = UILayer.Input;
			StartedTyping = false;

			if (PrefetchTarget)
			{
				RequestTargetAndUser();
			}

			if (ShowWidgetOnStart)
			{
				this.ShowWidget();
			}
		}

		// is called every frame
		void Update()
		{
			if (_ui.activeSelf)
			{
				if (!_LayerNetworkError.activeSelf && _api.NetworkError)
				{
					ShowNetworkErrorLayer();
				}
				else if (_LayerNetworkError.activeSelf && !_api.NetworkError)
				{
					HideNetworkErrorLayer();
				}
			}
		}

		// OnGUI is called for rendering and handling GUI events.
		void OnGUI()
		{
			if (this._toggleKey != KeyCode.None && Event.current.Equals(Event.KeyboardEvent(this._toggleKey.ToString())))
			{
				if (_ui.activeSelf)
				{
					this.HideWidget();
				}
				else
				{
					this.ShowWidget();
				}
			}
		}

		// Enables the Widget/Popup when hidden
		public void ShowWidget()
		{
			print(_ui.activeSelf);
			if (!_ui.activeSelf)
			{
			    print("in method ShowWidget");
				StartCoroutine(Show());
			}
		}

        public void OpenWidget()
        {
            this.Show();
        }

        public void OpenWidget(string CustomEventLabel)
        {
            var track = _api.initStomtTrack();
            track.event_category = CustomEventLabel;
            track.event_action = "open";
            track.save();

            this.OpenWidget();
        }

        // Actually shows the widget
        private IEnumerator Show()
		{
			yield return new WaitForEndOfFrame();

			// update UI elements
			ApplyLanguage();
			ApplyProfileImageTextureIfAvailable();
			SetTargetName();
			RequestTargetAndUser();

			_api.NetworkError = false;

			var track = _api.initStomtTrack();
			track.event_category = "form";
			track.event_action = "open";
			track.save ();

			// Capture screenshot
			_screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);

			// Get Logs
			if (this.UploadLogFile)
			{
				if (this._log != null)
				{
					this._log.stopThread();
				}
				this._log = new StomtLog(this._api);
			}

			if (_api.NetworkError)
			{
				ShowNetworkErrorLayer();
			}
			else
			{
				// Show UI
				ResetUILayers();
			}

			if (this.IsMessageLengthCorrect())
			{
				_postButton.GetComponent<Button>().interactable = true;
			}
			else
			{
				_postButton.GetComponent<Button>().interactable = false;
			}

			useEmailOnSubscribe = true;

			// Call Event
			if (OnWidgetOpen != null)
			{
				OnWidgetOpen();
			}

			Canvas.ForceUpdateCanvases();

			// Move Target (Fit with Toggle)
			MoveTargetBasedOnToggle(_targetObj.GetComponent<RectTransform>().rect);
			//this.MovePlaceholderBasedOnMessage();
		}

		// FIXME: Make this private and use special function on success layer create new stomt
		public void ResetUILayers()
		{
			ResetInputForm();

			_ui.SetActive(true);
			_closeButton.SetActive(ShowCloseButton);

			this._LayerInput.SetActive(true);
			this._LayerSuccessfulSent.SetActive(false);

			// Reset Subscription Layer
			this._LayerSubscription.SetActive(false);
			_EmailInput.text = "";

            // Reset Login Layer

            this._LayerLogin.SetActive(false);

            // Reset Login Message Layer

            this._LayerLoginMessage.SetActive(false);

			// Handle Animations
			_characterLimit.GetComponent<Animator>().SetBool("Active", false);
			_like.GetComponent<Animator>().SetBool("OnTop", false);
			_wish.GetComponent<Animator>().SetBool("OnTop", true);

			CurrentLayer = UILayer.Input;
		}
			
		// Disables the Widget/Popup when active
		public void HideWidget()
		{
			if (_ui.activeSelf)
			{
				Hide();
			}
		}

		// Actually hides the widget
		private void Hide()
		{
			// Hide UI
			_ui.SetActive(false);

			if (OnWidgetClosed != null)
			{
				OnWidgetClosed();
			}
		}

		// Setup multi-language strings
		private void ApplyLanguage()
		{
			// Input Layer
			this.wouldText = this._api.lang.getString("SDK_STOMT_DEFAULT_TEXT_WISH") + " ";
			this.becauseText = this._api.lang.getString("SDK_STOMT_DEFAULT_TEXT_LIKE") + " ";
			this._wish.GetComponentsInChildren<Text>()[0].text = this._api.lang.getString("SDK_STOMT_WISH_BUBBLE");
			this._like.GetComponentsInChildren<Text>()[0].text = this._api.lang.getString("SDK_STOMT_LIKE_BUBBLE");
			_wouldBecauseText.text = _api.lang.getString("SDK_STOMT_DEFAULT_TEXT_WISH");
            langPlaceholderText = this._api.lang.getString("SDK_STOMT_PLACEHOLDER");
            // FIXME: add translation for SDK_STOMT_SCREENSHOT

            // Header
            this._STOMTS.GetComponent<Text>().text = this._api.lang.getString("SDK_HEADER_TARGET_STOMTS");
			this._YOURS.GetComponent<Text>().text = this._api.lang.getString("SDK_HEADER_YOUR_STOMTS");
            
            if(this._api.config.GetLoggedin())
            {
                this.LoginButtonText.text = this._api.lang.getString("SDK_LOGIN_LOGOUT");
            }
            else
            {
                this.LoginButtonText.text = this._api.lang.getString("SDK_LOGIN");
            }

			// Subscription Layer
			// FIXME: add translation for SDK_SUBSCRIBE_TOGGLE_EMAIL
			// FIXME: add translation for SDK_SUBSCRIBE_TOGGLE_PHONE
			this.GetNotifiedText.text = this._api.lang.getString("SDK_SUBSCRIBE_GET_NOTIFIED");
			this.PlayShowAnimation(SubscribtionInfoText.GetComponent<Animator>(), 0.4f, this.SubscribtionInfoText, this._api.lang.getString("SDK_SUBSCRIBE_EMAIL_QUESTION"));
			this._EmailInput.placeholder.GetComponent<Text>().text = this._api.lang.getString("SDK_SUBSCRIBE_EMAIL_PLACEHOLDER");
			this.SkipButton.text = this._api.lang.getString("SDK_SUBSCRIBE_SKIP");

			// Success Layer
			this.ThankYouText.text = this._api.lang.getString("SDK_SUCCESS_THANK_YOU");
			this.ArrowText.text = this._api.lang.getString("SDK_SUCCESS_FIND_YOUR_STOMTS");
			this.SentLayerMessage.text = this._api.lang.getString("SDK_SUCCESS_FIND_ALL_STOMTS");
			this.CreateButtonText.text = this._api.lang.getString("SDK_SUCCESS_CREATE_NEW_WISH");

			// Error Layer
			this.ReconnectText.text = this._api.lang.getString("SDK_NETWORK_RECONNECT");
			this.ErrorHeaderText.text = this._api.lang.getString("SDK_NETWORK_NOT_CONNECTED");
			this.ErrorMessageText.text = this._api.lang.getString("SDK_NETWORK_NO_INTERNET");

            //Login Layer
            this.PasswordPlaceholder.text = this._api.lang.getString("SDK_LOGIN_PASSWORD");
            
        }

        //////////////////////////////////////////////////////////////////
        // HEADER
        //////////////////////////////////////////////////////////////////

        public void OpenTargetURL()
		{
			string url = this._api.stomtURL + "/" + _api.TargetID;
			this.OpenStomtUrl(url, "link-stomts");
		}

		public void OpenUserProfileURL()
		{
			string url = this._api.stomtURL + "/";

			if (!string.IsNullOrEmpty(_api.UserID)) {
				url = url + _api.UserID; // link to user profile
			}
			else
			{
				url = url + _api.TargetID; // fallback to target
			}

			this.OpenStomtUrl(url, "link-yours");
		}

		private void OpenStomtUrl(string url)
		{
			this.OpenStomtUrl(url, null);
		}

        private void OpenStomtUrl(string url, string utm_content)
        {
            url += string.Format("?utm_source={0}", "stomt");
            url += string.Format("&utm_medium={0}", "sdk");
            url += string.Format("&utm_campaign={0}", "unity");
            url += string.Format("&utm_term={0}", Application.productName);

            if (!string.IsNullOrEmpty(utm_content))
            {
                url += string.Format("&utm_content={0}", utm_content);
            }

            if (!string.IsNullOrEmpty(this._api.config.GetAccessToken()))
            {
                url += string.Format("&access_token={0}", this._api.config.GetAccessToken());
            }

            Application.OpenURL(url);
        }

        public void OpenTermsUrl()
        {
        	this.OpenStomtUrl("https://www.stomt.com/terms", "link-terms");
        }
    
        public void OpenPrivacyUrl()
        {
        	this.OpenStomtUrl("https://www.stomt.com/privacy", "link-privacy");
        }

        private void RequestTargetAndUser(bool force = false)
		{
			// only request them once
			if (!force && !string.IsNullOrEmpty(_api.TargetID) && !string.IsNullOrEmpty(_api.TargetDisplayname))
			{
				return;
			}

			_api.RequestTargetAndUser((response) => {
				SetStomtNumbers();
				_TargetURL.text = "stomt.com/" + _api.TargetID;
				SetTargetName();
				if (!TargetImageApplied)
				{
					StartCoroutine(RefreshTargetIcon());
				}
			}, null);
		}

		private void SetStomtNumbers()
		{
			_STOMTS_Number.text = _api.amountStomtsReceived.ToString ();
			_YOURS_Number.text = _api.amountStomtsCreated.ToString ();
		}

		private void SetTargetName()
		{
			if (string.IsNullOrEmpty(DisplayGameName))
			{
				if (_api.TargetDisplayname != null)
				{
					if (_api.TargetDisplayname.Length > TargetNameCharLimit)
					{
						_targetText.text = _api.TargetDisplayname.Substring (0, TargetNameCharLimit);
					}
					else
					{
						_targetText.text = _api.TargetDisplayname;
					}
				}
				else
				{
					_targetText.text = _api.TargetID;
				}
			}
			else
			{
				_targetText.text = DisplayGameName;
			}
		}


		//////////////////////////////////////////////////////////////////
		// Input Layer
		//////////////////////////////////////////////////////////////////

        public void OpenInputLayer()
        {
            DisableCurrentLayer();

            this._LayerInput.SetActive(true);

            // Handle Animations
            //_characterLimit.GetComponent<Animator>().SetBool("Active", false);
            _like.GetComponent<Animator>().SetBool("OnTop", false);
            _wish.GetComponent<Animator>().SetBool("OnTop", true);

            CurrentLayer = UILayer.Input;
        }

		public void OnToggleButtonPressed()
		{
			if (!StartedTyping)
			{
				this.StartedTyping = true;
				this.RefreshStartText();
				_message.ActivateInputField();
				_message.Select();
			}
			else
			{
				_message.ActivateInputField();
				_message.Select();

				this.StartedTyping = true;
				StartCoroutine(MoveMessageCaretToEnd());
			}

			var likeAnimator = _like.GetComponent<Animator>();
			var wishAnimator = _wish.GetComponent<Animator>();

			if (likeAnimator.isInitialized && wishAnimator.isInitialized)
			{
				bool tmp = likeAnimator.GetBool("OnTop");
				likeAnimator.SetBool("OnTop", wishAnimator.GetBool("OnTop"));
				wishAnimator.SetBool("OnTop", tmp);
			}

			if (_like.sortingOrder == 2)
			{
				// I wish
				_like.sortingOrder = 1;
				_wish.sortingOrder = 2;
				_wouldBecauseText.text = _api.lang.getString("SDK_STOMT_DEFAULT_TEXT_WISH");

				if (!this.IsMessageLengthCorrect() && ShowDefaultText)
				{
					_message.text = this.wouldText;
				}
			}
			else
			{
				// I like
				_like.sortingOrder = 2;
				_wish.sortingOrder = 1;
				_wouldBecauseText.text = _api.lang.getString("SDK_STOMT_DEFAULT_TEXT_LIKE");

				if (!this.IsMessageLengthCorrect() && ShowDefaultText)
				{
					_message.text = this.becauseText;
				}
			}

			OnMessageChanged();
		}

		public void OnMessageChanged()
		{
            


            int limit = CharLimit;
			int reverselength = limit - _message.text.Length;

			if (reverselength <= 0)
			{
				reverselength = 0;
				_message.text = _message.text.Substring(0, limit);
			}

			_characterLimit.text = reverselength.ToString();


			// Change Text
			if ( (!placeholderText.GetComponent<Text>().IsActive()) && _ui.activeSelf)
			{
				this.RefreshStartText();
			}

			if (IsMessageLengthCorrect())
			{
				_postButton.GetComponent<Button>().interactable = true;
			}
			else
			{
				_postButton.GetComponent<Button>().interactable = false;
			}

			if (StartedTyping && _message.text.Length < 6)
			{
				this.ShowErrorMessage(_api.lang.getString("SDK_STOMT_ERROR_MORE_TEXT"));
			}

			if (_characterLimit.GetComponent<Animator>().isInitialized)
			{
				if (_message.text.Length > 15)
				{
					_characterLimit.GetComponent<Animator>().SetBool("Active", true);
				}
				else
				{
					_characterLimit.GetComponent<Animator>().SetBool("Active", false);
				}
			}

			if (StartedTyping)
			{
				if (_message.text.Equals(this.wouldText) || _message.text.Equals(this.becauseText))
				{
                    MessagePlaceholder.text = _wouldBecauseText.text + langPlaceholderOffset + langPlaceholderText;
				}
				else
				{
                    MessagePlaceholder.text = "";
				}
			}
		}

		public void OnPointerEnterMessage()
		{
			this.RefreshStartText();

			if (!IsErrorState)
			{
				if (_characterLimit.GetComponent<Animator>().isInitialized)
				{
					_screenshotToggle.GetComponent<Animator>().SetBool("Show", true);
				}
			}

			_message.ActivateInputField();
			_message.Select();

			this.StartedTyping = true;
			StartCoroutine(MoveMessageCaretToEnd());
		}

		public void OnPointerEnterToggle()
		{
			var likeAnimator = _like.GetComponent<Animator>();
			var wishAnimator = _wish.GetComponent<Animator>();

			wishAnimator.SetBool("Hover", true);
			likeAnimator.SetBool("Hover", true);
		}

		public void OnPointerExitToggle()
		{
			var likeAnimator = _like.GetComponent<Animator>();
			var wishAnimator = _wish.GetComponent<Animator>();

			wishAnimator.SetBool("Hover", false);
			likeAnimator.SetBool("Hover", false);
		}

		public void OnPostButtonPressed()
		{
			if (IsErrorState)
			{
				this.HideErrorMessage();
				return;
			}

			if (!IsMessageLengthCorrect())
			{
				this.ShowErrorMessage(_api.lang.getString("SDK_STOMT_ERROR_MORE_TEXT"));
				return;
			}

			// Switch UI Layer
			_LayerInput.SetActive(false);
			if (this._api.config.GetSubscribed())
			{
				_LayerSuccessfulSent.SetActive(true);
				CurrentLayer = UILayer.Success;
				OnSwitchToSuccessLayer();
			}
			else
			{
				_LayerSubscription.SetActive(true);
				CurrentLayer = UILayer.Subscription;
				_EmailInput.ActivateInputField();
				_EmailInput.Select();
				SubscribtionInfoText.GetComponent<Animator>().SetBool("Show", true);
			}

			// Submit
			this.handleStomtSending();
		}
			
		// internal

		private void MoveTargetBasedOnToggle(Rect toggleRect)
		{
			_targetObj.GetComponent<RectTransform>().localPosition = new Vector3(targetLocalStartPostion.x + CalculateTargetXOffset(320), targetLocalStartPostion.y, 0);
		}

		private float CalculateTargetXOffset(float ReferenceValue)
		{
			float CurrentMaxWidth = 0.0f;

			if (_like.GetComponent<RectTransform>().rect.width > _wish.GetComponent<RectTransform>().rect.width)
			{
				CurrentMaxWidth = _like.GetComponent<RectTransform>().rect.width;
			}
			else
			{
				CurrentMaxWidth = _wish.GetComponent<RectTransform>().rect.width;
			}

			if (CurrentMaxWidth > ReferenceValue)
			{
				return CurrentMaxWidth - ReferenceValue;
			}

			return 0.0f;
		}

		private void MovePlaceholderBasedOnMessage()
		{
			CustomPlaceholderText.transform.localPosition = new Vector3( placeholderLocalStartPosition.x + CalculateLengthOfMessage(messageText.GetComponent<Text>()), CustomPlaceholderText.transform.localPosition.y, placeholderLocalStartPosition.z);
		}
			
		private int CalculateLengthOfMessage(Text textComponent)
		{
			int totalLength = 0;
			Font font = textComponent.font;
			CharacterInfo characterInfo = new CharacterInfo();

			char[] arr = textComponent.text.ToCharArray();

			foreach (char c in arr)
			{
				font.GetCharacterInfo(c, out characterInfo, textComponent.fontSize);

				totalLength += characterInfo.advance;
			}

			return totalLength;
		}
			
		private IEnumerator RefreshTargetIcon()
		{
			yield return 0;
			// check wether download needed
			if (ImageDownload == null && ProfileImageTexture == null)
			{
				// Start download
				if (this._api.TargetImageURL != null)
				{
					WWW www = new WWW(this._api.TargetImageURL);
					while (!www.isDone)
					{
						// wait until the download is done
					}

					ImageDownload = www;
				}
			}

			// check wether download finished
			if (ImageDownload != null && !TargetImageApplied && ProfileImageTexture == null)
			{
				if (ImageDownload.texture != null) // scale now and apply
				{
					if (ImageDownload.texture.width != 128 || ImageDownload.texture.height != 128)
					{
						ProfileImageTexture = TextureScaler.scaled(ImageDownload.texture, 128, 128, FilterMode.Trilinear);
					}

					TargetIcon.sprite.texture.LoadImage(ProfileImageTexture.EncodeToPNG());
					this.TargetImageApplied = true;
				}
			}
			else
			{
				// already loaded, apply now
				ApplyProfileImageTextureIfAvailable();
			}
		}

		private void ApplyProfileImageTextureIfAvailable()
		{
			if (!TargetImageApplied && ProfileImageTexture != null) 
			{
				TargetIcon.sprite.texture.LoadImage(ProfileImageTexture.EncodeToPNG(), false);
				TargetImageApplied = true;
			}
		}
			
		private IEnumerator MoveMessageCaretToEnd()
		{
			yield return 0; // Skip the first frame
			_message.MoveTextEnd(false);
		}
			
		private void RefreshStartText()
		{
			if (!ShowDefaultText || onMobile)
			{
				return;
			}

			if (this.StartedTyping)
			{
				if (_like.sortingOrder == 1)
				{
					// I wish
					if (_message.text.Equals(this.becauseText) || !StartedTyping)
					{
						_message.text = this.wouldText;
					}
				}
				else
				{
					// I like
					if (_message.text.Equals(this.wouldText) || !StartedTyping)
					{
						_message.text = this.becauseText;
					}
				}

				_message.GetComponent<InputField>().MoveTextEnd(true);
			}
			else
			{
				if (_like.sortingOrder == 1)
				{
					// I wish
					if (_message.text.Equals(this.becauseText) || !StartedTyping)
					{
						_message.text = this.wouldText;
					}
				}
			}
		}

		private bool IsMessageLengthCorrect()
		{
			if (_message.text.Length <= 9)
			{
				return false;
			}
			else
			{
				HideErrorMessage();
				return true;
			}
		}
			
		private void handleStomtSending()
		{
			// send StomtCreation
			StomtCreation stomtCreation = _api.initStomtCreation();

			stomtCreation.text = this._message.text;
			stomtCreation.positive = _like.sortingOrder == 2;

			// attach screenshot
			if (this._screenshotToggle.isOn)
			{
				stomtCreation.attachScreenshot(this._screenshot);
			}

			// attach logs
			if (this.UploadLogFile)
			{
				stomtCreation.attachLogs(this._log);
			}

			stomtCreation.save((response) => {
				SetStomtNumbers();
			}, (response) => {
				if (response == null)
				{
					return;
				}

				if (response.StatusCode.ToString().Equals("409"))
				{
					Debug.Log("Duplicate");
					// TODO return to form
					//					ShowErrorMessage("You already posted this stomt.");
					//					_LayerInput.SetActive(true);
					//					_LayerSuccessfulSent.SetActive(false);
					//					_LayerSubscription.SetActive(false);
				}
			});

			ResetInputForm();

			// Tigger Callback
			if (OnStomtSend != null)
			{
				OnStomtSend();
			}
		}
			
		private void ShowErrorMessage(string message)
		{
			_postButton.GetComponent<Button>().interactable = false;
			IsErrorState = true;

			if (message.Length > ErrorMessageCharLimit)
			{
				_ErrorMessageText.text = message.Substring(0, ErrorMessageCharLimit);
			}
			else
			{
				_ErrorMessageText.text = message;
			}

			_ErrorMessageObject.SetActive(true);

			if ((_screenshotToggle.GetComponent<Animator>().isInitialized))
			{
				_screenshotToggle.GetComponent<Animator>().SetBool("Show", false);
			}

			if ((_postButton.GetComponent<Animator>().isInitialized))
			{
				_postButton.GetComponent<Animator>().SetBool("Left", true);
			}

			if ((_ErrorMessageObject.GetComponent<Animator>().isInitialized))
			{
				_ErrorMessageObject.GetComponent<Animator>().SetBool("Appear", true);
			}
		}

		private void HideErrorMessage()
		{
			if (IsErrorState)
			{
				IsErrorState = false;

				_postButton.GetComponent<Button>().interactable = false;

				_postButton.GetComponent<Animator>().SetBool("Left", false);
				_ErrorMessageObject.GetComponent<Animator>().SetBool("Appear", false);
				_screenshotToggle.GetComponent<Animator>().SetBool("Show", true);
			}
		}
			
		private void ResetInputForm()
		{
			_message.text = "";
            MessagePlaceholder.text = _wouldBecauseText.text + langPlaceholderOffset + langPlaceholderText;
            this.StartedTyping = false;
			_screenshotToggle.isOn = Application.internetReachability == NetworkReachability.ReachableViaLocalAreaNetwork;

			if (_like.sortingOrder == 2)
			{
				OnToggleButtonPressed();
			}
			else
			{
				OnMessageChanged();
			}

			RefreshStartText();
		}


		//////////////////////////////////////////////////////////////////
		// Subscription Layer
		//////////////////////////////////////////////////////////////////

		// Email Toggle
		public void OnSubscribeTogglePressed()
		{
			SubscribtionInfoText.GetComponent<Animator>().SetBool("Show", false);
			useEmailOnSubscribe = !useEmailOnSubscribe;

			if (useEmailOnSubscribe)
			{
				toggleItemEMail.color = Color.black;
				toggleItemSMS.color = Color.gray;

				PlayShowAnimation(SubscribtionInfoText.GetComponent<Animator>(), 0.4f, SubscribtionInfoText, _api.lang.getString("SDK_SUBSCRIBE_EMAIL_QUESTION"));
				_EmailInput.placeholder.GetComponent<Text>().text = _api.lang.getString("SDK_SUBSCRIBE_EMAIL_PLACEHOLDER");
			}
			else
			{
				toggleItemEMail.color = Color.gray;
				toggleItemSMS.color = Color.black;

				PlayShowAnimation(SubscribtionInfoText.GetComponent<Animator>(), 0.4f, SubscribtionInfoText, _api.lang.getString("SDK_SUBSCRIBE_PHONE_QUESTION"));
				_EmailInput.placeholder.GetComponent<Text>().text = _api.lang.getString("SDK_SUBSCRIBE_PHONE_PLACEHOLDER");
			}

			_EmailInput.ActivateInputField();
			_EmailInput.Select();
		}

		// FIXME: What is this for?
		public void OnSubscriptionPointerEnter()
		{
			_EmailInput.text = "";
		}

		public void OnMobileInput()
		{
			if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
			{
				if (!useEmailOnSubscribe)
				{
					TouchScreenKeyboard.Open("", TouchScreenKeyboardType.NamePhonePad, false, false, false, true);
				}
				else
				{
					TouchScreenKeyboard.Open("", TouchScreenKeyboardType.EmailAddress, false, false, true, true);
				}
			}
		}

		public void OnSubscriptionInputChanged()
		{
            Regex regex = new Regex(@"@[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)*(\.[a-zA-Z]{2,15})$");
            Match match = regex.Match(_EmailInput.text);

            if ( match.Success )
			{
                if ( !SubscribtionInfoText.text.Equals(_api.lang.getString("SDK_SUBSCRIBE_VALID_EMAIL")) )
                {
                    _postButtonSubscription.GetComponent<Button>().interactable = true;
                    PlayShowAnimation(SubscribtionInfoText.GetComponent<Animator>(), 0.4f, new Color(0.0F, 0.9F, 0.265F, 1.0F), SubscribtionInfoText, _api.lang.getString("SDK_SUBSCRIBE_VALID_EMAIL"));
                }
            }
            else if(_EmailInput.text.Length > 3)
			{
                if (!SubscribtionInfoText.text.Equals(_api.lang.getString("SDK_SUBSCRIBE_NO_VALID_EMAIL")))
                {
                    _postButtonSubscription.GetComponent<Button>().interactable = false;
                    PlayShowAnimation(SubscribtionInfoText.GetComponent<Animator>(), 0.4f, new Color(1.0F, 0.089F, 0.089F, 1.0F), SubscribtionInfoText, _api.lang.getString("SDK_SUBSCRIBE_NO_VALID_EMAIL"));
                }
            }
		}

		public void SubmitSubscriptionLayer()
		{
			SubmitSubscription();
			SkipSubscriptionLayer();
		}

		public void SkipSubscriptionLayer()
		{
			this._LayerSuccessfulSent.SetActive(true);
			this._LayerSubscription.SetActive(false);
			OnSwitchToSuccessLayer();
		}

		private void SubmitSubscription()
		{
			if (!string.IsNullOrEmpty(_EmailInput.text))
			{
				if (useEmailOnSubscribe)
				{
					this._api.SendSubscription(_EmailInput.text, StomtAPI.SubscriptionType.EMail, null, null);
				}
				else
				{
					this._api.SendSubscription(_EmailInput.text, StomtAPI.SubscriptionType.Phone, null, null);
				}
			}

            SubmitLogin();
		}

        //////////////////////////////////////////////////////////////////
        // Login Layer
        //////////////////////////////////////////////////////////////////

        public void OpenSignupURL()
        {
            this.OpenStomtUrl("https://www.stomt.com/signup", "link-signup");
        }

        public void OpenForgotPasswordURL()
        {
            this.OpenStomtUrl("https://www.stomt.com/password/request", "link-forgot-password");
        }

        public void OpenLoginLayer()
        {
            DisableCurrentLayer();

            this._LayerLogin.SetActive(true);

            this.CurrentLayer = UILayer.Login;
        }

        public void SubmitLogin()
        {
            string userName = LoginUser.text;
            string password = LoginPassword.text;

            if (!string.IsNullOrEmpty(LoginUser.text) && !string.IsNullOrEmpty(LoginPassword.text))
            {
                this._api.disableContentLog = true;
                this._api.SendLoginRequest(userName, password, (response) =>
                {
                    LoginMessage.text = this._api.lang.getString("SDK_LOGIN_SUCCESS");

                    this.LoginButtonText.text = this._api.lang.getString("SDK_LOGIN_LOGOUT");

                    password = "";
                    LoginPassword.text = "";

                    this._api.disableContentLog = false;

                    this._LayerLogin.SetActive(false);
                    OpenLoginMessageLayer();

                }, (response) =>
                {
                    Debug.Log("Stomt Login failed: " + response.StatusCode);

                    if (response.StatusCode.Equals(System.Net.HttpStatusCode.NotFound))
                    {
                        LoginMessage.text = this._api.lang.getString("SDK_LOGIN_ACCOUNT_WRONG");
                        this.LoginUser.text = "";

                    }
                    else if(response.StatusCode.Equals(System.Net.HttpStatusCode.Forbidden))
                    {
                        LoginMessage.text = this._api.lang.getString("SDK_LOGIN_PASSWORD_WRONG");
                    }
                    else
                    {
                        LoginMessage.text = this._api.lang.getString("SDK_LOGIN_WENT_WRONG");

                        Debug.Log("Status Code: " + response.StatusCode);
                    }

                    LoginPassword.text = "";
                    password = "";

                    this._api.disableContentLog = false;

                    this._LayerLogin.SetActive(false);
                    OpenLoginMessageLayer();
                });
            }
        }

        public void OpenLoginMessageLayer()
        {
            _LayerLoginMessage.SetActive(true);

            this.CurrentLayer = UILayer.LoginMessage;
        }

        public void SubmitLoginMessageLayer()
        {
            _LayerLoginMessage.SetActive(false);
            this.OpenInputLayer();
        }

        public void LeaveLoginLayer()
        {
            this._LayerLogin.SetActive(false);
            this.OpenInputLayer();
        }

        public void OnLoginButtonPressed()
        {
            if( this._api.config.GetLoggedin() )
            {
                this.LoginButtonText.text = this._api.lang.getString("SDK_LOGIN");

                // Logout User
                this._api.config.SetAccessToken("");
                this._api.config.SetLoggedin(false);
            }
            else
            {
                this.OpenLoginLayer();
            }
        }


        //////////////////////////////////////////////////////////////////
        // Successful Sent Layer
        //////////////////////////////////////////////////////////////////

        public void OnSwitchToSuccessLayer()
		{
			PlayShowAnimation(ArrowFindStomt.GetComponent<Animator>(), 0.5f);
			CurrentLayer = UILayer.Success;
		}

		public void SubmitSuccessLayer()
		{
			this.HideWidget();
		}


		//////////////////////////////////////////////////////////////////
		// Network Error Layer
		//////////////////////////////////////////////////////////////////

        public void DisableCurrentLayer()
        {
            switch (CurrentLayer)
            {
                case UILayer.Input:
                    this._LayerInput.SetActive(false);
                    break;

                case UILayer.Subscription:
                    this._LayerSubscription.SetActive(false);
                    break;

                case UILayer.Login:
                    this._LayerLogin.SetActive(false);
                    break;

                case UILayer.Error:
                    this._LayerNetworkError.SetActive(false);
                    break;

                case UILayer.LoginMessage:
                    this._LayerLoginMessage.SetActive(false);
                    break;

                case UILayer.Success:
                    this._LayerSuccessfulSent.SetActive(false);
                    break;
            }
        }

		public void Reconnect()
		{
			switch (CurrentLayer)
			{
				case UILayer.Input:
					this.HideWidget();
					this.ShowWidget();
					break;

				case UILayer.Subscription:
					handleStomtSending();
					break;

				case UILayer.Success:
					if (this._api.config.GetSubscribed())
					{
						handleStomtSending();
					}
					else
					{
						SubmitSubscription();
					}
				    break;
			}

			this.RequestTargetAndUser();
		}

		private void ShowNetworkErrorLayer()
		{
			_LayerNetworkError.SetActive(true);

			switch (CurrentLayer)
			{
				case UILayer.Input:
					_LayerInput.SetActive(false);
					break;

				case UILayer.Subscription:
					_LayerSubscription.SetActive(false);
					break;

				case UILayer.Success:
					_LayerSuccessfulSent.SetActive(false);
					break;
			}
		}

		private void HideNetworkErrorLayer()
		{
			_LayerNetworkError.SetActive(false);

			switch (CurrentLayer)
			{
				case UILayer.Input:
					_LayerInput.SetActive(true);
					break;

				case UILayer.Subscription:
					_LayerSubscription.SetActive(true);
					break;

				case UILayer.Success:
					_LayerSuccessfulSent.SetActive(true);
					break;
			}
		}


		//////////////////////////////////////////////////////////////////
		// Helpers
		//////////////////////////////////////////////////////////////////

		private void PlayShowAnimation(Animator animator, float delayTime, Text TextToChange = null, string NewText = null)
		{
			StartCoroutine(PlayShowAnimationAsync(animator, delayTime, TextToChange, NewText, new Color(0.5625F, 0.07F, 0.95F, 1.0F)));
        }

        private void PlayShowAnimation(Animator animator, float delayTime, Color textColor, Text TextToChange = null, string NewText = null)
        {
            StartCoroutine(PlayShowAnimationAsync(animator, delayTime, TextToChange, NewText, textColor));
        }

        private IEnumerator PlayShowAnimationAsync(Animator animator, float delayTime, Text TextToChange, string NewText, Color textColor)
		{
            if (animator.isInitialized)
            {
                animator.SetBool("Show", false);
            }

            yield return new WaitForSeconds(delayTime);

            /*
            if (textColor == null)
            {
                textColor = new Color(0.5625F, 0.07F, 0.95F, 1.0F);
            }
            */

            if(TextToChange != null)
            {
                TextToChange.color = textColor;
            }

            if (animator.isInitialized)
			{
				animator.SetBool("Show", true);
			}

			if (TextToChange != null && NewText != null)
			{
				TextToChange.text = NewText;
			}
		}
	}
}
