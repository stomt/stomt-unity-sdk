using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Stomt
{
	[RequireComponent(typeof(StomtAPI))]
	public class StomtPopup : MonoBehaviour
	{
		#region Inspector Variables
		[SerializeField]
		KeyCode _toggleKey = KeyCode.F1;

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
        [HideInInspector]
        public GameObject _LayerSuccessfulSent;
        [HideInInspector]
        public GameObject _LayerInput;
        [HideInInspector]
        public Text _TargetURL;
        [HideInInspector]
        public GameObject _ErrorMessageObject;
        [HideInInspector]
        public Text _ErrorMessageText;

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
        private Texture2D ProfileImageTexture;
        private bool TargetImageApplied;
        private bool StartedTyping;
        private bool IsErrorState;

        public bool ShowCloseButton = true;
        public bool WouldBecauseText = true; // activates the would/because text
        public bool AutoImageDownload = true; // will automatically download the targetImage after %DelayTime Seconds;
        public float AutoImageDownloadDelay = 5; // DelayTime in seconds
        public int TargetNameCharLimit = 11;
        public int ErrorMessageCharLimit = 20;
        private int CharLimit = 120;

        public delegate void StomtAction();
        public static event StomtAction OnStomtSend;
        public static event StomtAction OnWidgetClosed;
        public static event StomtAction OnWidgetOpen;

		void Awake()
		{
            TargetImageApplied = false;

            if(placeholderText == null)
            {
                Debug.Log("PlaceholderText not found: Find(\"/Message/PlaceholderText\")");
            }

			_api = GetComponent<StomtAPI>();
			_screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

			Reset();
            StartCoroutine(this.refreshTargetIcon(AutoImageDownloadDelay));
		}

		void Start()
		{
            _TargetURL.text = "stomt.com/" + _api.TargetId;
            StartedTyping = false;
			Hide();
		}

		void Update()
		{
            if( (_ui.activeSelf && _api.NetworkError) && !_errorMessage.activeSelf)
            {
                ShowError();
            }
		}

        /**
         *   Enables the Widget/Popup when hidden
         */
        public void ShowWidget()
        {
            _api.SendTrack(_api.CreateTrack("form", "open"));

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

			// Capture screenshot
			_screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);

			// Show UI
			Reset();
            _ui.SetActive(true);
            _closeButton.SetActive(ShowCloseButton);

            if(this.IsMessageLengthCorrect())
            {
                _postButton.GetComponent<Button>().interactable = true;
            }
            else
            {
                _postButton.GetComponent<Button>().interactable = false;
            }

            ResetUILayer();

            ShowError();

            // Handle Animations
            _characterLimit.GetComponent<Animator>().SetBool("Active", false);
            _like.GetComponent<Animator>().SetBool("OnTop", false);
            _wish.GetComponent<Animator>().SetBool("OnTop", true);

            // Call Event
            if (OnWidgetOpen != null)
            {
                OnWidgetOpen();
            }
		}

        void ShowError()
        {
            if (_api.NetworkError)
            {
                // Diable GUI
                _messageObj.SetActive(false);
                _typeObj.SetActive(false);
                _targetNameObj.SetActive(false);
                // Enable Error MSG
                _errorMessage.SetActive(true);
                TargetIcon.enabled = false;

            }
            else
            {
                // Diable GUI
                _messageObj.SetActive(true);
                _typeObj.SetActive(true);
                _targetNameObj.SetActive(true);
                // Enable Error MSG
                _errorMessage.SetActive(false);
                TargetIcon.enabled = true;
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
            if(_api.TargetName != null)
            {
                if (_api.TargetName.Length > TargetNameCharLimit)
                {
                    _targetText.text = _api.TargetName.Substring(0, TargetNameCharLimit);
                }
                else
                {
                    _targetText.text = _api.TargetName;
                }
            }
			
            if( !TargetImageApplied )
            {
                refreshTargetIcon();
            }
                        
			_screenshotToggle.isOn = true;

			if (_like.sortingOrder == 2)
			{
				OnToggleButtonPressed();
			}
			else
			{
				OnMessageChanged();
			}
		}

		public void OnToggleButtonPressed()
		{
            if(!StartedTyping)
            {
                this.StartedTyping = true;
                this.RefreshStartText();
            }

            var likeAnimator = _like.GetComponent<Animator>();
            var wishAnimator = _wish.GetComponent<Animator>();

            bool tmp = likeAnimator.GetBool("OnTop");
            likeAnimator.SetBool("OnTop", wishAnimator.GetBool("OnTop"));
            wishAnimator.SetBool("OnTop", tmp);

			if (_like.sortingOrder == 2)
			{
				// I wish
				_like.sortingOrder = 1;
				_wish.sortingOrder = 2;
				_wouldBecauseText.text = "would";

                if (!this.IsMessageLengthCorrect())
                {
                    _message.text = "because ";
                }
			}
			else
			{
				// I like
				_like.sortingOrder = 2;
				_wish.sortingOrder = 1;
				_wouldBecauseText.text = "because";
               
                if(!this.IsMessageLengthCorrect())
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

            if(_message.text.Length > 15)
            {
                _characterLimit.GetComponent<Animator>().SetBool("Active", true);
            }
            else
            {
                if(_characterLimit.GetComponent<Animator>().isInitialized)
                {
                    _characterLimit.GetComponent<Animator>().SetBool("Active", false);
                }
            }
		}

        public void RefreshStartText()
        {
            if (this.StartedTyping && WouldBecauseText)
            {
                if (_like.sortingOrder == 1)
                {

                    // I wish
                    if (_message.text.Equals("") || _message.text.Equals("because "))
                    {
                        _message.text = "would ";
                    }
                }
                else
                {
                    // I like
                    if (_message.text.Equals("") || _message.text.Equals("would ") )
                    {
                        _message.text = "because ";
                    }
                }

                _message.GetComponent<InputField>().MoveTextEnd(true);
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

			if (_screenshotToggle.isOn)
			{
				_api.CreateStomtWithImage(_like.sortingOrder == 2, _message.text, _screenshot);
			}
			else
			{
				_api.CreateStomt(_like.sortingOrder == 2,  _message.text);
			}

            if ( OnStomtSend != null)
            {
                OnStomtSend();
            }

            _message.text = "";

            //Switch UI Layer
            _LayerSuccessfulSent.SetActive(true);
            _LayerInput.SetActive(false);

		}

        private void refreshTargetIcon()
        {
            StartCoroutine(refreshTargetIcon(0));
        }

        private IEnumerator refreshTargetIcon(float DelayTime)
        {
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

            yield return new WaitForSeconds(DelayTime);

            if(DelayTime > 0)
            {
                this.refreshTargetIcon();
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

        public void OnPointerEnterMessage()
        {
            this.StartedTyping = true;
            this.RefreshStartText();

            if(!IsErrorState)
            {
                _screenshotToggle.GetComponent<Animator>().SetBool("Show", true);
            }
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
            this._LayerInput.SetActive(true);
            this._LayerSuccessfulSent.SetActive(false);
        }

        public void OpenTargetURL()
        {
            Application.OpenURL("https://www.stomt.com/" + _api.TargetId);
        }

        public void ShowErrorMessage(string message)
        {
            _postButton.GetComponent<Button>().interactable = true;
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
            _screenshotToggle.GetComponent<Animator>().SetBool("Show", false);
            _postButton.GetComponent<Animator>().SetBool("Left", true);
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
	}
}
