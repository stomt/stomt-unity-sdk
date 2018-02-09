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
		private StomtAPI Api;

		public StomtLang(StomtAPI api, string language = null)
		{
			this.Api = api;
			this.ForceDefaultLanguage = api.ForceDefaultLanguage;
			this.LoadLanguageFile();

			// Select Language
			if (this.ForceDefaultLanguage)
			{
				this.setLanguage(language);
			}
			else
			{
				this.setLanguage(this.getLanguageCode(Application.systemLanguage));
			}
		}

		private void LoadLanguageFile()
		{
			if (this.Api.languageFile != null)
			{
				languages = LitJsonStomt.JsonMapper.ToObject(this.Api.languageFile.ToString());
			}
			else
			{
				Debug.LogWarning("languageFile not found! Please set the language file in StomtAPI Inspector.");
			}
		}

		public void setLanguage(string languageCode)
		{
			this.Currentlanguage = languageCode;

			// Check Language
			if (!this.languages["data"].Keys.Contains(this.Currentlanguage))
			{
				Debug.Log(string.Format("Language {0} not supported (does not exist in language file) falling back to english.", this.Currentlanguage));
				this.Currentlanguage = "en";
			}
		}

		public string getLanguage()
		{
			return this.Currentlanguage;
		}

		public string getString(string stringDefinition)
		{
			if (!this.languages["data"].Keys.Contains(this.Currentlanguage))
			{
				Debug.Log(string.Format("Language {0} not supported (does not exist in language file) falling back to english.", this.Currentlanguage));
				this.Currentlanguage = "en";
			}

			if (!this.languages["data"][this.Currentlanguage].Keys.Contains(stringDefinition))
			{
				Debug.LogWarning(string.Format("Translation for '{0}' not found in language: '{1}'", stringDefinition, this.Currentlanguage));

				// try in english
				if (this.languages["data"].Keys.Contains("en") && this.languages["data"]["en"].Keys.Contains(stringDefinition))
				{
					return (string)languages["data"]["en"][stringDefinition];
				}
				else
				{
					return "";
				}
			}

			return (string)languages["data"][Currentlanguage][stringDefinition];
		}

		private string getLanguageCode(SystemLanguage languageName)
		{
			switch (languageName)
			{
				case SystemLanguage.Afrikaans: return "af";
				case SystemLanguage.Arabic: return "ar";
				case SystemLanguage.Basque: return "eu";
				case SystemLanguage.Belarusian: return "be";
				case SystemLanguage.Bulgarian: return "bg";
				case SystemLanguage.Catalan: return "es";
				case SystemLanguage.Chinese: return "zh";
				case SystemLanguage.Czech: return "cs";
				case SystemLanguage.Danish: return "da";
				case SystemLanguage.Dutch: return "nl";
				case SystemLanguage.English: return "en";
				case SystemLanguage.Estonian: return "et";
				case SystemLanguage.Faroese: return "fo";
				case SystemLanguage.Finnish: return "fi";
				case SystemLanguage.French: return "fr";
				case SystemLanguage.German: return "de";
				case SystemLanguage.Greek: return "el";
				case SystemLanguage.Hebrew: return "he";
				case SystemLanguage.Hungarian: return "hu";
				case SystemLanguage.Icelandic: return "is";
				case SystemLanguage.Indonesian: return "id";
				case SystemLanguage.Italian: return "it";
				case SystemLanguage.Japanese: return "jp";
				case SystemLanguage.Korean: return "ko";
				case SystemLanguage.Latvian: return "lv";
				case SystemLanguage.Lithuanian: return "lt";
				case SystemLanguage.Norwegian: return "no";
				case SystemLanguage.Polish: return "pl";
				case SystemLanguage.Portuguese: return "pt";
				case SystemLanguage.Romanian: return "ro";
				case SystemLanguage.Russian: return "ru";
				case SystemLanguage.SerboCroatian: return "sr";
				case SystemLanguage.Slovak: return "sk";
				case SystemLanguage.Slovenian: return "sl";
				case SystemLanguage.Spanish: return "es";
				case SystemLanguage.Swedish: return "sv";
				case SystemLanguage.Thai: return "th";
				case SystemLanguage.Turkish: return "tr";
				case SystemLanguage.Ukrainian: return "uk";
				case SystemLanguage.Vietnamese: return "vi";
				case SystemLanguage.ChineseSimplified: return "zh"; // "zh-si"
				case SystemLanguage.ChineseTraditional: return "zh"; // "zh-tr"
				case SystemLanguage.Unknown: return "en";
				default: return "en";
			}
		}
	}
}
