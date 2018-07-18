using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Model;
namespace UI
{
	/// <summary>
	/// This class will handle the buttons on heroes in heroselection panel,
	/// in order to show information of selected hero.
	/// </summary>
	public class HeroSelectorButtonHandler : MonoBehaviour {

		// [SerializeField]
		public GameObject heroDetailsPanel;
		private Button heroSelector;
		private HeroProperties selectedHero = new HeroProperties();

		private List<HeroProperties> userHeroes = new List<HeroProperties>();

		private List<GameObject> heroPrefab = new List<GameObject>();

		// private List<Button> prefabButton = new List<Button>();
		// private Button prefabButton;

		private Button button;
        public void ReadFromCSV()
		{
			userHeroes.Add(new HeroProperties());
			userHeroes = selectedHero.Parse();
			
			
		}
		
		public void HeroOnClickHandler(Button prefabButton,int index)
		{
			// for (int i = 0; i < userHeroes.Count; i++)
			// {
				// heroPrefab[i] = GameObject.Find("Hero_" + i);
				// prefabButton[i] = heroPrefab[i].GetComponent<Button>();
				/// <summary>
				/// this is lambda expresion we call actions in the way that we
				/// want with this syntax.
				/// </summary>
				// prefabButton.onClick.RemoveAllListeners();
				prefabButton.onClick.AddListener( () => ShowHeroInfo(index));
			// }			
		}

		public void ShowHeroInfo(int index)
		{
			print("button clicked");
			print(heroDetailsPanel==null);
			heroDetailsPanel.SetActive(true);




		}

		private void Awake() {

			print(heroDetailsPanel == null);
			heroDetailsPanel.gameObject.SetActive(false);
			// heroPrefab.Add(new GameObject());
			// prefabButton.Add(button);
			ReadFromCSV();
			
			
		}

		// Use this for initialization1S
		void Start () {

			// print("Sdsdsd");
			// print(heroDetailsPanel.name);

			
		}

		// Update is called once per frame
		void Update () {

			// HeroOnClickHandler();
			
		}
	}
}
