using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stomt
{
	public class StomtLang
	{
		private LitJsonStomt.JsonData languages;
		private string Currentlanguage;
		private bool ForceDefaultLanguage;

		public StomtLang(StomtAPI api, string language)
		{
			this.Currentlanguage = language;
			this.ForceDefaultLanguage = api.ForceDefaultLanguage;

			if (api.languageFile != null) {
				languages = LitJsonStomt.JsonMapper.ToObject(api.languageFile.ToString());
			} else {
				Debug.Log("languageFile not found! Please set the language file in StomtAPI Inspector.");
			}

			if (!this.ForceDefaultLanguage) {
				if (Application.systemLanguage == SystemLanguage.English) {
					this.Currentlanguage = "en";
				}

				if (Application.systemLanguage == SystemLanguage.German) {
					this.Currentlanguage = "de";
				}
			}
		}

		public void setLanguage(string language)
		{
			this.Currentlanguage = language;
		}

		public string getString(string stringDefinition)
		{
			if (!this.languages["data"].Keys.Contains(Currentlanguage)) {
				Debug.LogWarning(string.Format("Language {0} not supported (does not exist in language file)", Currentlanguage));
				this.Currentlanguage = "en";
			}

			if (!this.languages["data"][Currentlanguage].Keys.Contains(stringDefinition)) {
				Debug.LogWarning(string.Format("StringDefinition {0} not found in {1}", stringDefinition, Currentlanguage));
				return "";
			}

			return (string)languages["data"][Currentlanguage][stringDefinition];
		}

	}
}
