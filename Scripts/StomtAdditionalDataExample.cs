using UnityEngine;
using System.Collections;
using Stomt;

public class StomtAdditionalDataExample : MonoBehaviour 
{
	private StomtAPI StomtAPI;

	// Use this for initialization
	void Start () 
	{
		StomtAPI = GameObject.Find("StomtPopup").GetComponent<StomtAPI>();

		StomtAPI.AddCustomKeyValuePair("customkey", "blasasasasTEXT");

		Debug.Log("TEST");
	}
}
