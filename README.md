# Collect Feedback In-Game | STOMT-SDK for Unity 3D
#### Version: Beta
#### Implementation Time: ~20 Minutes (incl. Triggers)

<p align="center">
  <img alt="STOMT Unity Feedback Integration" src="https://schukies.io/images/stomt/stomtunity9.png" />
</p>

We have created a custom feedback solution at [www.stomt.com](https://www.stomt.com/), which easily allows you to collect feedback from your users. This Unity plug-in allows you to integrate our solution in your app or game. You can download and install or download from the [Unity Asset Store](https://www.assetstore.unity3d.com/en/#!/content/64669). Our web platform helps you to manage all incoming feedback and build a community uppon it.

## Use-Cases

_What will the result look like?_ 

Example Games that use our integrations:

* [Empires of the Undergrowth](https://www.stomt.com/empires-of-the-undergrowth)      
* [All Walls Must Fall](https://www.stomt.com/AWMF)


## Installation

1. Download this repository and [or get it from the AssetStore](https://www.assetstore.unity3d.com/en/#!/content/64669) and copy the assets into your project.

2. Add the ```StomtPopup``` prefab to your main UI canvas.


## Configuration

1. Register on [www.stomt.com](https://www.stomt.com/signup/game) 

2. And create an [App Id](https://www.stomt.com/integrate) for your project.

3. Enter all necessary data into the ```StomtAPI``` component on the prefab.     

* Enter the `App Id` you obtained in the second step
* Optional: Enter `Labels` that will be attached to every stomt

<img alt="Configure STOMT Unity plugin" src="http://schukies.io/images/stomt/StomtUnitySettings.PNG" />

Finished! *Regularly communicate your page on social channels and checkout our [Website-Widget](https://www.stomt.com/dev/js-sdk) for your websites to collect feedback from anywhere.*    


## Usage

The Widget can be enabled by using a toggle key or calling the API Methods.

Add Labels that will be attached to every stomt the user sends via script `_api.Labels` or in the inspector.

StomtPopup Class
* Enable:	ShowWidget()
* Disable:	HideWidget()


<img alt="STOMT Unity Feedback Integration" src="https://i.imgur.com/p8bdLL3.gif" />



## Labels

Labels will help you track down user issues.
Append labels, as for example your game-version or the player position.
<p align="center">
<img alt="Events" src="https://i.imgur.com/sS8T8Fy.png" />
</p>

Unity Inspector:

<img alt="Events" src="https://i.imgur.com/qS6IdZL.png" />


From Code:
```
using UnityEngine;
using System.Collections;
using Stomt;

public class StomtLabelExample : MonoBehaviour 
{
	private StomtAPI StomtAPI;

	// Use this for initialization
	void Start () 
	{
		// Find StomtPopup
		StomtAPI = GameObject.Find("StomtPopup").GetComponent<StomtAPI>();

		// Add your labels
		StomtAPI.Labels = new string[] { "label1", "label2" };
	}
}

```


## Event Callbacks


The STOMT Widget supports a variety of callback events.

This shows how you can access them.

```
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
```


## Common Issues

* Error (401) Unauthorized: Is your application ID right? ```test.stomt.com``` and ```stomt.com``` use different ID's.
* Error (500) Internal Server Error: [Report] (https://www.stomt.com/dev/unity-sdk) us the problem.
* Stomts are not sent out on Android: change the API Compatibility Level from `.NET 2.0 Subset` to `.NET 2.0` in the Player Settings.


## Contribution

We would love to see you contributing to this project. Feel free to fork it and send in your pull requests! Visit the [project on STOMT](https://www.stomt.com/stomt-unity) to support with your ideas, wishes and feedback.


## Authors

[Daniel Schukies](https://github.com/daniel-schukies) | [Follow Daniel Schukies on STOMT](https://www.stomt.com/danielschukies)    
[Patrick Mours](https://github.com/crosire) | [Follow Patrick Mours on STOMT](https://www.stomt.com/crosire)


## More about STOMT

* On the web [www.stomt.com](https://www.stomt.com)
* [STOMT for iOS](http://stomt.co/ios)
* [STOMT for Android](http://stomt.co/android)
* [STOMT for Unreal](http://stomt.co/unreal)
* [STOMT for Websites](http://stomt.co/web)
* [STOMT for Wordpress](http://stomt.co/wordpress)
* [STOMT for Drupal](http://stomt.co/drupal)
