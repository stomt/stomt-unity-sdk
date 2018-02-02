using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using System.ComponentModel;
using System.Collections.Generic;


namespace Stomt
{
	[RequireComponent(typeof(StomtAPI))]
	public class StomtPopup : MonoBehaviour
	{
		#region Inspector Variables
		[SerializeField]
		KeyCode _toggleKey = KeyCode.F1;
        public string DisplayGameName;

		[SerializeField]
		[HideInInspector]
		public GameObject _typeObj;
		[SerializeField]
		[HideInInspector]
		public GameObject _targetNameObj;
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
		public Text _STOMTS_Number;
		[SerializeField]
		[HideInInspector]
		public Text _YOURS_Number;
		[SerializeField]
        [HideInInspector]
        public Text SentLayerMessage;
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
		#endregion
		StomtAPI _api;
		Texture2D _screenshot;
		StomtLog _log;
		[SerializeField]
		[HideInInspector]
		public GameObject placeholderText;
		[SerializeField]
		[HideInInspector]
		public GameObject messageText;
		[SerializeField]
		[HideInInspector]
		public Image TargetIcon;

		private WWW ImageDownload;
		public Texture2D ProfileImageTexture;
		private bool TargetImageApplied = false;
		private bool StartedTyping;
		private bool IsErrorState;

		public bool LogFileUpload = true;
		public bool ShowCloseButton = true;
		public bool WouldBecauseText = true; // activates the would/because text
		public int TargetNameCharLimit = 11;
		public int ErrorMessageCharLimit = 20;
		public bool ShowWidgetOnStart = false;
		private int CharLimit = 120;
        private bool useEmailOnSubscribe = true;
        private bool onMobile = false;
        private string wouldText = "would ";
        private string becauseText = "because ";

		public delegate void StomtAction();
		public static event StomtAction OnStomtSend;
		public static event StomtAction OnWidgetClosed;
		public static event StomtAction OnWidgetOpen;

        enum UILayer { Input, Subscription, Success, Error };

        UILayer CurrentLayer;

        void Awake()
		{
			_ui.SetActive(false);
        }

		void Start()
		{
            if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer)
                this.onMobile = true;

            if (placeholderText == null)
            {
                Debug.Log("PlaceholderText not found: Find(\"/Message/PlaceholderText\")");
            }

            _api = GetComponent<StomtAPI>();
            _screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);


            CurrentLayer = UILayer.Input;
            StartedTyping = false;

			_api.RequestTargetAndUser((response) => {
				SetStomtNumbers();
				_TargetURL.text = "stomt.com/" + _api.TargetID;
				setTargetName ();
				StartCoroutine(refreshTargetIcon());
			}, null);

			if(ShowWidgetOnStart)
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
                else if(_LayerNetworkError.activeSelf && !_api.NetworkError)
                {
                    HideNetworkErrorLayer();
                }
            }
		}
			

		/**
		 *   Enables the Widget/Popup when hidden
		 */
		public void ShowWidget()
		{
			if (!_ui.activeSelf)
			{
				StartCoroutine(Show());
			}
		}


		/**
		 *  Disables the Widget/Popup when active
		 */
		public void HideWidget()
		{
			if (_ui.activeSelf)
			{
				Hide();
			}
		}

		void OnGUI()
		{
			if (Event.current.Equals(Event.KeyboardEvent(_toggleKey.ToString())) && _toggleKey != KeyCode.None)
			{
				if (_ui.activeSelf)
				{
					Hide();
				}
				else
				{
					StartCoroutine(Show());
				}
			}
		}

		IEnumerator Show()
		{
			yield return new WaitForEndOfFrame();

            _api.NetworkError = false;

            var track = _api.initStomtTrack();
			track.event_category = "form";
			track.event_action = "open";
			track.save ();

			// Capture screenshot
			_screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);

			// Get Logs
			if (this.LogFileUpload) {
				if (this._log != null) {
					this._log.stopThread();
				}
				this._log = new StomtLog(this._api);
			}
            if(_api.NetworkError)
            {
                ShowNetworkErrorLayer();
            }
            else
            {
                // Show UI
                ResetUILayer();
            }
            
			if(this.IsMessageLengthCorrect())
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
		}

		private void SetStomtNumbers()
		{
			_STOMTS_Number.text = _api.amountStomtsReceived.ToString ();
			_YOURS_Number.text = _api.amountStomtsCreated.ToString ();
		}

        public void Reconnect()
        {
            if(CurrentLayer == UILayer.Success)
            {
                if(this._api.config.GetSubscribed())
                {
                    handleStomtSending();
                }
                else
                {
                    SubmitSubscription();
                }
            }

            if(CurrentLayer == UILayer.Subscription)
            {
                handleStomtSending();
            }

            if (CurrentLayer == UILayer.Input)
            {
                this.HideWidget();
                this.ShowWidget();
            }
        }

        void HideNetworkErrorLayer()
        {
            _LayerNetworkError.SetActive(false);

            if (CurrentLayer == UILayer.Input)
            {
                _LayerInput.SetActive(true);
            }

            if (CurrentLayer == UILayer.Subscription)
            {
                _LayerSubscription.SetActive(true);
            }

            if (CurrentLayer == UILayer.Success)
            {
                _LayerSuccessfulSent.SetActive(true);
            }
        }


		void ShowNetworkErrorLayer()
		{
            _LayerNetworkError.SetActive(true);

            if(CurrentLayer == UILayer.Input)
            {
                _LayerInput.SetActive(false);
            }
                
            if(CurrentLayer == UILayer.Subscription)
            {
                _LayerSubscription.SetActive(false);
            }

            if (CurrentLayer == UILayer.Success)
            {
                _LayerSuccessfulSent.SetActive(false);
            }
        }

		public void Hide()
		{
			// Hide UI
			_ui.SetActive(false);

			if (OnWidgetClosed != null)
			{
				OnWidgetClosed();
			}
		}

		void Reset()
		{
            this.CustomPlaceholderText.SetActive(true);
            this.StartedTyping = false;
			_screenshotToggle.isOn = true;

			if (_like.sortingOrder == 2)
			{
				OnToggleButtonPressed();
			}
			else
			{
				OnMessageChanged();
			}

			RefreshStartText ();
		}

		public void OnToggleButtonPressed()
		{
			if(!StartedTyping)
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
				_wouldBecauseText.text = "would";

                if (!this.IsMessageLengthCorrect() && WouldBecauseText)
				{
					_message.text = "would ";
				}
			}
			else
			{
				// I like
				_like.sortingOrder = 2;
				_wish.sortingOrder = 1;
				_wouldBecauseText.text = "because";
			   
				if(!this.IsMessageLengthCorrect() && WouldBecauseText)
				{
					_message.text = "because ";
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


			/** Change Text **/
			if ( (!placeholderText.GetComponent<Text>().IsActive()) && _ui.activeSelf )
			{
				this.RefreshStartText();
			}

			if(IsMessageLengthCorrect())
			{
				_postButton.GetComponent<Button>().interactable = true;
			}
			else
			{
				_postButton.GetComponent<Button>().interactable = false;
			}

			if(StartedTyping && _message.text.Length < 6)
			{
				this.ShowErrorMessage("Please write a bit more.");
			}

			if(_characterLimit.GetComponent<Animator>().isInitialized)
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

            if(StartedTyping)
            {
                if(_message.text.Equals(this.wouldText) || _message.text.Equals(this.becauseText))
                {
                    this.CustomPlaceholderText.SetActive(true);
                }
                else
                {
                    this.CustomPlaceholderText.SetActive(false);
                }
            }
		}

		public void RefreshStartText()
		{
            if (!WouldBecauseText || onMobile)
                return;

			if (this.StartedTyping)
			{
				if (_like.sortingOrder == 1)
				{
					// I wish
					if (_message.text.Equals("because ") || !StartedTyping)
					{
						_message.text = "would ";
					}
				}
				else
				{
					// I like
					if (_message.text.Equals("would ") || !StartedTyping)
					{
						_message.text = "because ";
					}
				}

				_message.GetComponent<InputField>().MoveTextEnd(true);
			}
			else
			{
				if (_like.sortingOrder == 1)
				{
					// I wish
					if (_message.text.Equals("because ") || !StartedTyping)
					{
						_message.text = "would ";
					}
				}
			}
		}


		private bool IsMessageLengthCorrect()
		{
			if (_message.text.Length == 0 || _message.text.Length <= 9)
			{
				return false;
			}
			else
			{
				HideErrorMessage();
				return true;
			}
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
				this.ShowErrorMessage("Please write a bit more");
				Debug.Log("_message to short!");
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

            SentLayerMessage.text = "Amazing, find more wishes to " + _api.TargetDisplayname + " on";
        }

		private void handleStomtSending()
		{
			// send StomtCreation
			StomtCreation stomtCreation = _api.initStomtCreation();

			stomtCreation.text = this._message.text;
			stomtCreation.positive = _like.sortingOrder == 2;

			// attach screenshot
			if (this._screenshotToggle.isOn) {
				stomtCreation.attachScreenshot (this._screenshot);
			}

			// attach logs
			if (this.LogFileUpload) {
				stomtCreation.attachLogs(this._log);
			}

			stomtCreation.save((response) => {
				SetStomtNumbers();
			}, (response) => {
				if (response == null) {
					return;
				}
				if (response.StatusCode.ToString().Equals("409")) {
					Debug.Log("Duplicate");
					// TODO return to form
//					ShowErrorMessage("You already posted this stomt.");
//					_LayerInput.SetActive(true);
//					_LayerSuccessfulSent.SetActive(false);
//					_LayerSubscription.SetActive(false);
				}
			});

			_message.text = "";

			if (OnStomtSend != null) {
				OnStomtSend();
			}
		}
	
		private void setTargetName()
		{
            if(string.IsNullOrEmpty(DisplayGameName))
            {
			    if (_api.TargetDisplayname != null)
                {
				    if (_api.TargetDisplayname.Length > TargetNameCharLimit)
                    {
					    _targetText.text = _api.TargetDisplayname.Substring (0, TargetNameCharLimit);
				    } else
                    {
					    _targetText.text = _api.TargetDisplayname;
				    }
			    } else
                {
				    _targetText.text = _api.TargetID;
			    }
            }
            else
            {
                _targetText.text = DisplayGameName;
            }

		}

		private IEnumerator refreshTargetIcon()
		{
			yield return 0;
			// check wether download needed
			if (ImageDownload == null )
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
			if (ImageDownload != null && !TargetImageApplied)
			{

				if (ProfileImageTexture != null) // already loaded, apply now
				{
					TargetIcon.sprite.texture.LoadImage(ProfileImageTexture.EncodeToPNG(), false);
				}
				else if (ImageDownload.texture != null) // scale now and apply
				{
					ProfileImageTexture = TextureScaler.scaled(ImageDownload.texture, 128, 128, FilterMode.Trilinear);

					TargetIcon.sprite.texture.LoadImage(ProfileImageTexture.EncodeToPNG());
					this.TargetImageApplied = true;
				}
			}
		}

		IEnumerator MoveMessageCaretToEnd()
		{
			yield return 0; // Skip the first frame
			_message.MoveTextEnd(false);
        }

		public void OnPointerEnterMessage()
		{
			this.RefreshStartText();

			if(!IsErrorState)
			{
				if(_characterLimit.GetComponent<Animator>().isInitialized)
					_screenshotToggle.GetComponent<Animator>().SetBool("Show", true);
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

		public void ResetUILayer()
		{
            Reset();

            _ui.SetActive(true);
            _closeButton.SetActive(ShowCloseButton);

            this._LayerInput.SetActive(true);
			this._LayerSuccessfulSent.SetActive(false);

            // Reset Error Layer
            // TODO

			// Reset Subscription Layer
			this._LayerSubscription.SetActive(false);
			_EmailInput.text = "";

            // Handle Animations
            _characterLimit.GetComponent<Animator>().SetBool("Active", false);
            _like.GetComponent<Animator>().SetBool("OnTop", false);
            _wish.GetComponent<Animator>().SetBool("OnTop", true);

            CurrentLayer = UILayer.Input;
        }

		public void OpenTargetURL()
		{
            string url = this._api.stomtURL + "/" + _api.TargetID;

            if (!string.IsNullOrEmpty(this._api.config.GetAccessToken()))
            {
                url += string.Format("?access_token={0}", this._api.config.GetAccessToken());
            }

            Application.OpenURL(url);
        }

		public void OpenUserProfileURL()
		{
            string url = this._api.stomtURL + "/" + _api.UserID;

            if (!string.IsNullOrEmpty(this._api.config.GetAccessToken()))
            {
                url += string.Format("?access_token={0}", this._api.config.GetAccessToken());
            }

            Application.OpenURL(url);
        }

		public void ShowErrorMessage(string message)
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

            if((_screenshotToggle.GetComponent<Animator>().isInitialized))
			    _screenshotToggle.GetComponent<Animator>().SetBool("Show", false);

            if ((_postButton.GetComponent<Animator>().isInitialized))
                _postButton.GetComponent<Animator>().SetBool("Left", true);

            if ((_ErrorMessageObject.GetComponent<Animator>().isInitialized))
                _ErrorMessageObject.GetComponent<Animator>().SetBool("Appear", true);
		}

		public void HideErrorMessage()
		{
			if(IsErrorState)
			{
				IsErrorState = false;

				_postButton.GetComponent<Button>().interactable = false;

				_postButton.GetComponent<Animator>().SetBool("Left", false);
				_ErrorMessageObject.GetComponent<Animator>().SetBool("Appear", false);
				_screenshotToggle.GetComponent<Animator>().SetBool("Show", true);
			}
		}

        public void SubmitSubscription()
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
        }

		public void SubmitSubscriptionLayer()
		{
            SubmitSubscription();

            this._LayerSuccessfulSent.SetActive(true);
			this._LayerSubscription.SetActive(false);
            OnSwitchToSuccessLayer();

        }

		public void OnSubscriptionInputChanged()
		{
			if(_EmailInput.text.Length > 8 )
			{
				_postButtonSubscription.GetComponent<Button>().interactable = true;
			}
			else
			{
				_postButtonSubscription.GetComponent<Button>().interactable = false;
			}
		}

		public void OnSubscriptionPointerEnter()
		{
			_EmailInput.text = "";
		}

		public void SubmitSuccessLayer()
		{
			this.HideWidget();
		}

        public void OnSwitchToSuccessLayer()
        {
            PlayShowAnimation(ArrowFindStomt.GetComponent<Animator>(), 0.5f);
            CurrentLayer = UILayer.Success;
        }


        // Email Toggle
        public void OnSubscribeTogglePressed()
        {
            string finalInfoText = "";
            string defaultText = "What's your ";
            string phoneTextEnding = "phone number?";
            string emailTextEnding = "email address?";

            SubscribtionInfoText.GetComponent<Animator>().SetBool("Show", false);
            useEmailOnSubscribe = !useEmailOnSubscribe;
            //Debug.Log("useEmailOnSubscribe " + useEmailOnSubscribe.ToString());

            if (useEmailOnSubscribe)
            {
                toggleItemEMail.color = Color.black;
                toggleItemSMS.color = Color.gray;

                finalInfoText = defaultText + emailTextEnding;

            }
            else
            {
                toggleItemEMail.color = Color.gray;
                toggleItemSMS.color = Color.black;

                finalInfoText = defaultText + phoneTextEnding;
            }

            _EmailInput.ActivateInputField();
            _EmailInput.Select();


            PlayShowAnimation(SubscribtionInfoText.GetComponent<Animator>(), 0.4f, SubscribtionInfoText, finalInfoText);
            //PlayShowAnimation(SubscribtionInfoText.GetComponent<Animator>(), 0.6f);
        }

        void PlayShowAnimation(Animator animator, float delayTime)
        {
            StartCoroutine(PlayShowAnimationAsync(animator, delayTime, null, null));
        }

        void PlayShowAnimation(Animator animator, float delayTime, Text TextToChange, string NewText)
        {
            StartCoroutine(PlayShowAnimationAsync(animator, delayTime, TextToChange, NewText));
        }

        IEnumerator PlayShowAnimationAsync(Animator animator, float delayTime, Text TextToChange, string NewText)
        {
            yield return new WaitForSeconds(delayTime);

            if (animator.isInitialized)
            {
                animator.SetBool("Show", true);
            }
                
            if(TextToChange != null && NewText != null)
            {
                TextToChange.text = NewText;
            }
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
                    TouchScreenKeyboard.Open("", TouchScreenKeyboardType.EmailAddress, false, false, false, true);
                }
            }
        }
    }
}
