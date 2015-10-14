using Stomt;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(StomtAPI))]
public class StomtExample : MonoBehaviour
{
	public Canvas _like, _wish;
	public InputField _message;
	public Text _wouldBecauseText, _characterLimit, _targetText;

	StomtAPI _api;

	void Awake()
	{
		_api = GetComponent<StomtAPI>();
		_targetText.text = _api.TargetName;
	}
	void Start()
	{
		OnMessageChanged();
	}

	public void OnToggleButtonPressed()
	{
		if (_wish.sortingOrder == 1)
		{
			_like.sortingOrder = 1;
			_wish.sortingOrder = 2;
			_wouldBecauseText.text = "would";
		}
		else
		{
			_like.sortingOrder = 2;
			_wish.sortingOrder = 1;
			_wouldBecauseText.text = "because";
		}

		OnMessageChanged();
	}
	public void OnMessageChanged()
	{
		int limit = 101 - _wouldBecauseText.text.Length;
		int reverselength = limit - _message.text.Length;

		if (reverselength <= 0)
		{
			reverselength = 0;
			_message.text = _message.text.Substring(0, limit);
		}

		_characterLimit.text = reverselength.ToString();
	}
	public void OnPostButtonPressed()
	{
		if (_message.text.Length == 0)
		{
			return;
		}

		_api.CreateStomt(_like.sortingOrder == 2, _wouldBecauseText.text + " " + _message.text);
	}
}
