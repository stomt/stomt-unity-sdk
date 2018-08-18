using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Stomt
{
	public class StomtNotificationCounter : MonoBehaviour
	{
		[SerializeField]
		[HideInInspector]
		protected GameObject UI;
		[SerializeField]
		[HideInInspector]
		protected Text NotificationCount;

		// Use this for initialization
		void Start()
		{
			this.NotificationCount.text = 0.ToString();
			this.UI.SetActive(false);
		}

		// Update is called once per frame
		void Update()
		{
			var count = StomtConfig.UserAmountNotifications;
			NotificationCount.text = count.ToString();
			this.UI.SetActive(count > 0);
		}
	}
}