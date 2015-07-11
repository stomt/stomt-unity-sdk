using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(StomtAPI))]
public class StomtCreation : MonoBehaviour {

    public GameObject btn_wish, btn_like, toggle;
    public Text message, counter, target;

    StomtAPI api;
    Canvas wish, like;

    void Awake()
    {
        api = GetComponent<StomtAPI>();
    }


	void Start()
    {
        target.text = api.TargetName;
        wish = btn_wish.GetComponent<Canvas>();
        like = btn_like.GetComponent<Canvas>();
    }
	

	void Update()
    {
	
	}

    public void togglePressed()
    {
        if(wish.sortingOrder == 1)
        {
            like.sortingOrder = 1;
            wish.sortingOrder = 2;
        }
        else
        {
            like.sortingOrder = 2;
            wish.sortingOrder = 1;
        }
    }

    public void postPressed()
    {
        if (message.text.Length <= 10)
            return;

        api.CreateStomt(wish.sortingOrder == 2, message.text);
    }

    public void stomtMessageChanged()
    {
        string s = message.text;
        int length = 139 - s.Length;
        counter.text = length.ToString();

    }

}
