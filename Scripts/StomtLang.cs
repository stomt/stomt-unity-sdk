using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Stomt
{
    public class StomtLang
    {
        private LitJsonStomt.JsonData languages;
        private string Currentlanguage;

        public StomtLang(StomtAPI api, string language)
        {
            this.Currentlanguage = language;

            if (api.languageFile != null)
            {
                languages = LitJsonStomt.JsonMapper.ToObject(api.languageFile.ToString());
            }
            else
            {
                Debug.Log("languageFile not found! Please set the language file in StomtAPI Inspector.");
            }
        }

        public void setLanguage(string language)
        {
            this.Currentlanguage = language;
        }

        public string getString(string stringDefinition)
        {
            if(!this.languages["data"].Keys.Contains(Currentlanguage))
            {
                Debug.LogWarning(string.Format("Language {0} not supported (does not exist in language file)", Currentlanguage));
                return "";
            }

            if (!this.languages["data"][Currentlanguage].Keys.Contains(stringDefinition))
            {
                Debug.LogWarning(string.Format("StringDefinition {0} not found in {1}", stringDefinition, Currentlanguage));
                return "";
            }

            //Debug.Log((string)languages["data"][Currentlanguage][stringDefinition]);
            return (string)languages["data"][Currentlanguage][stringDefinition];
        }

    }
}
