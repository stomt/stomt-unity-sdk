# Collect Feedback In-Game | STOMT-SDK for Unity 3D
#### Implementation Time: ~20 Minutes (incl. Triggers)

<p align="center">
  <img alt="STOMT Unity Feedback Integration" width="441px" height="282px" src="https://i.imgur.com/IfnTfpr.gif" />
</p>

We have created a custom feedback solution at [www.stomt.com](https://www.stomt.com/), which easily allows you to collect feedback from your users. This Unity plug-in allows you to integrate our solution in your app or game. You can download and install or download from the [Unity Asset Store](https://www.assetstore.unity3d.com/en/#!/content/64669). Our web platform helps you to manage all incoming feedback and build a community uppon it.

## Examples / Use-Cases / Demo

Example Games that use our integrations:

* [Empires of the Undergrowth](https://www.stomt.com/empires-of-the-undergrowth)      
* [All Walls Must Fall](https://www.stomt.com/AWMF)
* [Pantropy](https://www.stomt.com/pantropy)

## Installation

1. Download this repository and [or get it from the AssetStore](https://www.assetstore.unity3d.com/en/#!/content/64669) and copy the assets into your project.

2. Add the ```StomtPopup``` prefab to your main UI canvas.


## Configuration

1. Create a page for your game on [www.stomt.com](https://www.stomt.com/signup/game).

2. Create an [App Id](https://www.stomt.com/integrate) for Unity.

3. Enter all necessary data into the ```StomtAPI``` component on the prefab.   

* Enter the `App Id` you obtained in the second step

<p align="center">
<img alt="Configure STOMT Unity plugin" src="http://schukies.io/images/stomt/StomtUnitySettings.PNG" /> 
</p>

4. Finished!

Scaling: You can scale the widget by using the StomtPopup's Rect Transform Scale property in the inspector.

## Optional Configuration

#### Stomt API (Script)

| Property | Type | Description |
| :--- | :--- | :--- |
| **App Id** _(required)_ | String | The [App Id](https://www.stomt.com/integrate) of your game on STOMT |
| **Language File** | File | A JSON-File containing the translations ([Learn more](#translations)). |
| **Default Language** | String | Show the UI in this language if the users language is not available. |
| **Force Default Language** | Bool | Show every user the UI in the Default Language. |
| **Send Default Labels** | Bool | Automatically attach platform and resolution to stomts. |
| **Debug Mode** | Bool | Only used in development to access all layers. |
| **Labels** | List | Enter Labels that will be attached to every stomt ([See also Section "In-Game Labeling"](https://github.com/stomt/stomt-unity-sdk#in-game-labeling)). |

#### Stomt Popup (Script)

| Property | Type | Description |
| :--- | :--- | :--- |
| **Toggle Key** | Key | Keyboard Key to toggle the creation form. |
| **Display Game Name** | String | Force to show this name instead of loading it from the server. |
| **Profile Image Texture** | Texture | Force to use this image instead of loading it from the server. |
| **Upload Log File** | Bool | Upload the log file with every stomt. |
| **Prefetch Target** | Bool | Load the target (name, image, stats) from the server when initializing StomtApi. |
| **Show Close Button** | Bool | Show the close button in the top right of the widget. |
| **Show Default Text** | Bool | Enter "would" / "because" (or the translation) in the textarea, so users understand that they have to finish the sentence. |
| **Show Widget On Start** | Bool | Automatically show the widget on startup. |


## Form Triggers

The widget can be opened and closed whenever you want by using our trigger functions.     
    
That allows you to:    
* Put a button into the main menu [(Example)](https://imgur.com/5SoQzfj)
* Put a button into the HUD [(Example)](https://imgur.com/t9wPpJj)
* Only show the button to certain players (e.g. power users)
* Trigger the form after certain events

**StomtPopup Class**
* Enable:	```ShowWidget()```
* Disable:	```HideWidget()```

## In-Game Labeling

Labels will help you track down user issues.
Append labels, as for example your game-version or the player position/level. You can either hardcode them in the Unity Inspector or use a script to add them in a flexible way based on the information you have.

If you want different labels for every stomt then you have to clear the label array and set new labels.

<p align="center">
<img alt="Events" width="600" height="auto" src="https://i.imgur.com/sS8T8Fy.png" />
</p>

**Hardcoded via Unity Inspector:**
<p align="center">
<img alt="Events" src="https://i.imgur.com/qS6IdZL.png" />
</p>

**Flexible via Script:**
```C#
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


## Append Additional Data

If you need more data but you do not want to flood the labels, you can add additional data to a stomt by adding custom key value pairs.

```C#
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
```

## Event Callbacks


The STOMT Widget supports a variety of callback events.

This shows how you can access them.

```C#
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


## Translations

The UI of the SDK supports multiple languages. At the moment we provide support for English and German (but you can add [more languages](#extend-the-translations)). We automatically search for translations for the users [`Application.SystemLanguage`](https://docs.unity3d.com/ScriptReference/Application-systemLanguage.html). If you use another way to let the user configure his language you can pass this language to the STOMT SDK as well. Please pass the [ISO 639-1 Code](https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes) of the language to `StomtAPI.SetUserLanguage`.

```C#
using UnityEngine;
using System.Collections;
using Stomt;

public class StomtLanguageExample : MonoBehaviour
{
	private StomtAPI StomtAPI;

	// Use this for initialization
	void Start()
	{
		StomtAPI = GameObject.Find("StomtPopup").GetComponent<StomtAPI>();

		StomtAPI.SetUserLanguage("ru");
	}
}
```

### Extend the Translations

To extend the translation simply add your languages in the `Language/languages.json` file. Use the [ISO 639-1 Code](https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes) of the language as new key and copy the translations object from one of the other languages as value. Then replace the strings with your translated ones. You should review the translations in the UI to ensure that everything fits in the available space.

If you have added a languages, please [share them](https://github.com/stomt/stomt-unity-sdk/issues) with us. 


## Common Issues

* Error (401) Unauthorized: Is your application ID right? ```test.stomt.com``` and ```stomt.com``` use different ID's.
* Error (500) Internal Server Error: [Report](https://www.stomt.com/dev/unity-sdk) us the problem.
* Stomts are not sent out on Android: change the API Compatibility Level from `.NET 2.0 Subset` to `.NET 2.0` in the Player Settings.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/stomt/stomt-unity-sdk/tags). 

## Contribution

We would love to see you contributing to this project. Please read [CONTRIBUTING.md](https://github.com/stomt/stomt-unity-sdk/blob/master/CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests to us.    
    
Visit the [project on STOMT](https://www.stomt.com/stomt-unity) to support with your ideas, wishes and feedback.

## Authors

[Daniel Schukies](https://github.com/daniel-schukies) | [Follow Daniel Schukies on STOMT](https://www.stomt.com/danielschukies)

See also the list of [contributors](https://github.com/stomt/stomt-unity-sdk/contributors) who participated in this project.

## More about STOMT

*Regularly communicate your page on social channels and checkout our [Website-Widget](https://www.stomt.com/dev/js-sdk) for your websites to collect feedback from anywhere.*   

* On the web [www.stomt.com](https://www.stomt.com)
* [STOMT for iOS](http://stomt.co/ios)
* [STOMT for Android](http://stomt.co/android)
* [STOMT for Unreal](http://stomt.co/unreal)
* [STOMT for Websites](http://stomt.co/web)
* [STOMT for Wordpress](http://stomt.co/wordpress)
* [STOMT for Drupal](http://stomt.co/drupal)
