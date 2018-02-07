using UnityEngine;
using System.Collections;
using Stomt;

public class StomtAdditionalDataExample : MonoBehaviour
{
	// Use this for initialization
	void Start ()
	{
		StomtAPI api = GameObject.Find("StomtPopup").GetComponent<StomtAPI>();

		api.AddCustomKeyValuePair("CustomKey", "Value/Data");
	}
}
