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
	}
	public void OnMessageChanged()
	{
		_CharacterLimit.text = (129 - _Message.text.Length).ToString();
	}
	public void OnPostButtonPressed()
	{
		if (_Message.text.Length <= 10)
		{
			return;
		}

		_API.CreateStomt(_Wish.sortingOrder == 2, _WouldBecauseText.text + " " + _Message.text);
	}
}
