using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StomtCreation : MonoBehaviour {

    public GameObject btn_wish, btn_like, toggle;
    public Text message;
    


	void Start()
    {

	}
	

	void Update()
    {
	
	}

    public void togglePressed()
    {
        Canvas wish = btn_wish.GetComponent<Canvas>();
        Canvas like = btn_like.GetComponent<Canvas>();

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

}
