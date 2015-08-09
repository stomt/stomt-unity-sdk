using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(StomtAPI))]
public class StomtCreation : MonoBehaviour
{
	public Canvas _Like, _Wish;
	public InputField _Message;
	public Text _WouldBecauseText, _CharacterLimit, _TargetText;

	StomtAPI _API;

	void Awake()
	{
		_API = GetComponent<StomtAPI>();
	}
	void Start()
	{
		_TargetText.text = _API.TargetName;

		OnMessageChanged();
	}

	public void OnToggleButtonPressed()
	{
		if (_Wish.sortingOrder == 1)
		{
			_Like.sortingOrder = 1;
			_Wish.sortingOrder = 2;
			_WouldBecauseText.text = "would";
		}
		else
		{
			_Like.sortingOrder = 2;
			_Wish.sortingOrder = 1;
			_WouldBecauseText.text = "because";
		}

		OnMessageChanged();
	}
	public void OnMessageChanged()
	{
		int limit = 101 - _WouldBecauseText.text.Length;
		int reverselength = limit - _Message.text.Length;

		if (reverselength <= 0)
		{
			reverselength = 0;
			_Message.text = _Message.text.Substring(0, limit);
		}

		_CharacterLimit.text = reverselength.ToString();
	}
	public void OnPostButtonPressed()
	{
		if (_Message.text.Length == 0)
		{
			return;
		}

		_API.CreateStomt(_Wish.sortingOrder == 2, _WouldBecauseText.text + " " + _Message.text);
	}
}
