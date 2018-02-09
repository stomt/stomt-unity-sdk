using UnityEngine;
using System.Collections;
using Stomt;

public class StomtLabelExample : MonoBehaviour
{
	private StomtAPI StomtAPI;

	// Use this for initialization
	void Start ()
	{
		StomtAPI = GameObject.Find("StomtPopup").GetComponent<StomtAPI>();

		StomtAPI.Labels = new string[] { "label1", "label2" };
	}
}
