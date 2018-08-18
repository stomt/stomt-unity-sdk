using UnityEngine;
using UnityEngine.UI;
using System.Collections;

namespace Stomt
{
	public class StomtNotificationBox : MonoBehaviour
	{
		[SerializeField]
		[HideInInspector]
		protected GameObject UI;
		[SerializeField]
		[HideInInspector]
		protected Text NotificationText;
		[SerializeField]
		[HideInInspector]
		protected Text NotificationInfo;
		[SerializeField]
		[HideInInspector]
		protected GameObject CounterUI;
		[SerializeField]
		[HideInInspector]
		protected Text NotificationCount;

		private StomtAPI api;
		private StomtNotification notification;

		// Use this for initialization
		void Start()
		{
			this.api = StomtAPI.Instance;
			this.NotificationCount.text = 0.ToString();
			this.UI.SetActive(false);
		}

		// Update is called once per frame
		void Update()
		{
			var count = StomtConfig.UserAmountNotifications;
			NotificationCount.text = "+" + count.ToString();
			this.CounterUI.SetActive(count > 0);

			if (this.notification == null) {
				// check for new notification
				var newNotification = api.GetFirstNotification(true);
				if (newNotification != null) {
					this.notification = newNotification;
					NotificationText.text = newNotification.fullText;
					this.UI.SetActive(true);
				}
			}
		}

		public void OpenUrl()
		{
			if (this.notification != null)
			{
				this.notification.OpenLink();
				this.notification = null;
				this.UI.SetActive(false);
			}
		}
	}
}
