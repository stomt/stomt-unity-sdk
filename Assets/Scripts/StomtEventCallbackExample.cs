using UnityEngine;
using System.Collections;
using Stomt;

public class StomtEventCallbackExample : MonoBehaviour
{
	// Use this for initialization
	void Start ()
	{
		StomtPopup.OnStomtSend += YourFunction;
		StomtPopup.OnWidgetClosed += YourFunction;
		StomtPopup.OnWidgetOpen += YourFunction;
	}

	// Your Function
	void YourFunction()
	{
		// React
	}
}
