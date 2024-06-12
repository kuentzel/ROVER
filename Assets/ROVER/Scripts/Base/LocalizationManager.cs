using System.Collections.Generic;
using System.IO;
using UnityEngine;
using TMPro;

namespace ROVER
{
    /// <summary>
    /// Manages the localization of text strings in the application.
    /// </summary>
    public class LocalizationManager : MonoBehaviour
    {
        // Public fields for localization settings
        [Header("Localization Settings")]
        public string locale = "en_US";

        // Public fields for UI elements
        [Header("Hint Panel Animations")]
        public TextMeshPro hintPanelAnimation1;
        public TextMeshPro hintPanelAnimation2;
        public TextMeshPro hintPanelAnimation3;
        public TextMeshPro hintPanelAnimation4;
        public TextMeshPro hintPanelAnimation5;

        [Header("Hint Panel Text")]
        public TextMeshProUGUI hintPanelText;

        [Header("Control Panel Animations")]
        public TextMeshProUGUI controlPanelAnim1;
        public TextMeshProUGUI controlPanelAnim2;
        public TextMeshProUGUI controlPanelAnim3;
        public TextMeshProUGUI controlPanelAnim4;

        [Header("Tutorial Text")]
        public TextMeshProUGUI tut1Cap;
        public TextMeshProUGUI tut11;
        public TextMeshProUGUI tut12;
        public TextMeshProUGUI tut2Cap;
        public TextMeshProUGUI tut21;
        public TextMeshProUGUI tut22;
        public TextMeshProUGUI tut2panel;
        public TextMeshProUGUI tut3Cap;
        public TextMeshProUGUI tut31;
        public TextMeshProUGUI tut32;
        public TextMeshProUGUI tut4Cap;
        public TextMeshProUGUI tut41;
        public TextMeshProUGUI tut42;
        public TextMeshProUGUI tut4option1;
        public TextMeshProUGUI tut4option2;
        public TextMeshProUGUI tut4option3;
        public TextMeshProUGUI tut4option4;
        public TextMeshProUGUI tut5Cap;
        public TextMeshProUGUI tut51;
        public TextMeshProUGUI tut52;
        public TextMeshProUGUI tut5button;
        public TextMeshProUGUI tutButton;

        [Header("Polar Text")]
        public TextMeshProUGUI polarLeft;
        public TextMeshProUGUI polarRight;

        [Header("Hint Titles")]
        public TextMeshProUGUI hintTitle;
        public TextMeshProUGUI buttonTitle;

        [Header("Feedback Text")]
        public TextMeshProUGUI posFB1;
        public TextMeshProUGUI posFB2;
        public TextMeshProUGUI posFB3;
        public TextMeshProUGUI neutFB;

        // Private fields
        private List<string> strings = new List<string>();

        /// <summary>
        /// Gets the localized string at the specified index.
        /// </summary>
        /// <param name="i">The index of the string.</param>
        /// <returns>The localized string.</returns>
        public string GetLocaleString(int i)
        {
            if (i < strings.Count)
                return strings[i];
            else
                return "";
        }

        /// <summary>
        /// Sets the localized strings to the UI elements.
        /// </summary>
        private void SetStrings()
        {
            hintPanelAnimation1.text = $"1. {strings[1]}\n2. {strings[2]}\n3. {strings[3]}\n\n    {strings[4]} !";
            hintPanelAnimation2.text = $"{strings[0]}1. {strings[1]}</color></b>\n2. {strings[2]}\n3. {strings[3]}\n\n    {strings[4]} !";
            hintPanelAnimation3.text = $"1. {strings[1]}\n{strings[0]}2. {strings[2]}</color></b>\n3. {strings[3]}\n\n    {strings[4]} !";
            hintPanelAnimation4.text = $"1. {strings[1]}\n2. {strings[2]}\n{strings[0]}3. {strings[3]}</color></b>\n\n    {strings[4]} !";
            hintPanelAnimation5.text = $"1. {strings[1]}\n2. {strings[2]}\n3. {strings[3]}\n\n    {strings[0]}{strings[4]} !</color></b>";
            int i = 1;
            controlPanelAnim1.text = strings[i++];
            controlPanelAnim2.text = strings[i++];
            controlPanelAnim3.text = strings[i++];
            controlPanelAnim4.text = strings[i++];
            hintPanelText.text = strings[i++].Replace("\\n", "\n");
            tut1Cap.text = strings[i++];
            tut11.text = strings[i++].Replace("\\n", "\n");
            tut12.text = strings[i++].Replace("\\n", "\n");
            tut2Cap.text = strings[i++];
            tut21.text = strings[i++].Replace("\\n", "\n");
            tut22.text = strings[i++].Replace("\\n", "\n");
            tut2panel.text = strings[i++];
            tut3Cap.text = strings[i++];
            tut31.text = strings[i++].Replace("\\n", "\n");
            tut32.text = strings[i++].Replace("\\n", "\n");
            tut4Cap.text = strings[i++];
            tut41.text = strings[i++].Replace("\\n", "\n");
            tut42.text = strings[i++].Replace("\\n", "\n");
            tut4option1.text = strings[i++];
            tut4option2.text = strings[i++];
            tut4option3.text = strings[i++];
            tut4option4.text = strings[i++];
            tut5Cap.text = strings[i++];
            tut51.text = strings[i++].Replace("\\n", "\n");
            tut52.text = strings[i++].Replace("\\n", "\n");
            tut5button.text = strings[i++];
            tutButton.text = strings[i++];
            polarLeft.text = strings[i++];
            polarRight.text = strings[i++];
            hintTitle.text = strings[i++];
            buttonTitle.text = strings[i++];
            posFB1.text = strings[i++].Replace("\\n", "\n");
            posFB2.text = strings[i++].Replace("\\n", "\n");
            posFB3.text = strings[i++].Replace("\\n", "\n");
            neutFB.text = strings[i++].Replace("\\n", "\n");
        }

        /// <summary>
        /// Loads localization strings from a file for the specified locale.
        /// </summary>
        /// <param name="locale">The locale to load.</param>
        public void LoadLocalization(string locale)
        {
            if (locale.Contains("_locale"))
            {
                locale = locale.Replace("_locale", "");
            }

            string importPath = Path.Combine(Application.streamingAssetsPath, "Configuration/Localization", $"{locale}_locale.csv");

            if (!File.Exists(importPath))
            {
                Debug.Log("File does not exist.");
                return;
            }

            using (StreamReader reader = new StreamReader(importPath))
            {
                while (!reader.EndOfStream)
                {
                    string s = reader.ReadLine().Replace("\\n", "\n");
                    strings.Add(s);
                }
            }

            SetStrings();
        }
    }
}
