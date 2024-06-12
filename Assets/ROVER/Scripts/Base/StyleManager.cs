using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;

namespace ROVER
{
    /// <summary>
    /// Manages the styling of UI elements, including colors and materials.
    /// </summary>
    public class StyleManager : MonoBehaviour
    {
        [Header("Managers")]
        public ConfigurationManager configurationManager;

        [Header("Colors")]
        public Color32 displayFrameColor;
        public Color32 backgroundDisplayColor;
        public Color32 backgroundPrimaryColor;
        public Color32 backgroundSecondaryColor;
        public Color32 buttonSelectedColor;
        public Color32 buttonPressedColor;
        public Color32 buttonHoverColor;
        public Color32 buttonIconActiveColor;
        public Color32 buttonIconInactiveColor;
        public Color32 buttonPrimaryActiveColor;
        public Color32 buttonPrimaryInactiveColor;
        public Color32 buttonSecondaryActiveColor;
        public Color32 buttonSecondaryInactiveColor;
        public Color32 foregroundPrimaryColor;
        public Color32 foregroundSecondaryColor;
        public Color32 iconColor;
        public Color32 highlightFlashColor;
        public Color32 textActiveColor;
        public Color32 buttonTextPrimaryColor;
        public Color32 buttonTextSecondaryColor;
        public Color32 defaultOutlineColor;
        public Color32 overlayDefaultColor;

        [Header("Materials")]
        public Material displayFrameMaterial;
        public Material backgroundDisplayMaterial;
        public Material backgroundPrimaryMaterial;
        public Material backgroundSecondaryMaterial;
        public Material buttonSelectedMaterial;
        public Material buttonPressedMaterial;
        public Material buttonHoverMaterial;
        public Material buttonIconActiveMaterial;
        public Material buttonIconInactiveMaterial;
        public Material buttonPrimaryActiveMaterial;
        public Material buttonPrimaryInactiveMaterial;
        public Material buttonSecondaryActiveMaterial;
        public Material buttonSecondaryInactiveMaterial;
        public Material foregroundPrimaryMaterial;
        public Material foregroundSecondaryMaterial;
        public Material iconColorMaterial;
        public Material highlightFlashMaterial;
        public Material textActiveMaterial;
        public Material buttonTextPrimaryMaterial;
        public Material buttonTextSecondaryMaterial;
        public Material defaultOutlineMaterial;
        public Material overlayDefaultMaterial;

        private bool listed = false;
        private Dictionary<string, Color32> colors;

        /// <summary>
        /// Start is called before the first frame update.
        /// Lists material colors and saves default color styling if in debug mode.
        /// </summary>
        void Start()
        {
            ListMaterialColors();
            if (configurationManager.debug && !File.Exists(Path.Combine(Application.streamingAssetsPath, "Configuration/DefaultColorStyling.json")))
            {
                using (StreamWriter writer = new StreamWriter(Path.Combine(Application.streamingAssetsPath, "Configuration/DefaultColorStyling.json")))
                {
                    writer.Write(JsonConvert.SerializeObject(colors, Formatting.Indented));
                }
            }
        }

        /// <summary>
        /// Lists the colors of all materials.
        /// </summary>
        public void ListMaterialColors()
        {
            if (listed) return;
            listed = true;

            colors = new Dictionary<string, Color32>();

            // Add colors to dictionary and set the colors for materials
            AddColor("displayFrameColor", displayFrameMaterial, ref displayFrameColor);
            AddColor("backgroundDisplayColor", backgroundDisplayMaterial, ref backgroundDisplayColor);
            AddColor("backgroundPrimaryColor", backgroundPrimaryMaterial, ref backgroundPrimaryColor);
            AddColor("backgroundSecondaryColor", backgroundSecondaryMaterial, ref backgroundSecondaryColor);
            AddColor("buttonSelectedColor", buttonSelectedMaterial, ref buttonSelectedColor);
            AddColor("buttonPressedColor", buttonPressedMaterial, ref buttonPressedColor);
            AddColor("buttonHoverColor", buttonHoverMaterial, ref buttonHoverColor);
            AddColor("buttonIconActiveColor", buttonIconActiveMaterial, ref buttonIconActiveColor);
            AddColor("buttonIconInactiveColor", buttonIconInactiveMaterial, ref buttonIconInactiveColor);
            AddColor("buttonPrimaryActiveColor", buttonPrimaryActiveMaterial, ref buttonPrimaryActiveColor);
            AddColor("buttonPrimaryInactiveColor", buttonPrimaryInactiveMaterial, ref buttonPrimaryInactiveColor);
            AddColor("buttonSecondaryActiveColor", buttonSecondaryActiveMaterial, ref buttonSecondaryActiveColor);
            AddColor("buttonSecondaryInactiveColor", buttonSecondaryInactiveMaterial, ref buttonSecondaryInactiveColor);
            AddColor("foregroundPrimaryColor", foregroundPrimaryMaterial, ref foregroundPrimaryColor);
            AddColor("foregroundSecondaryColor", foregroundSecondaryMaterial, ref foregroundSecondaryColor);
            AddColor("iconColor", iconColorMaterial, ref iconColor);
            AddColor("highlightFlashColor", highlightFlashMaterial, ref highlightFlashColor);
            AddColor("textActiveColor", textActiveMaterial, ref textActiveColor, ShaderUtilities.ID_FaceColor);
            AddColor("buttonTextPrimaryColor", buttonTextPrimaryMaterial, ref buttonTextPrimaryColor, ShaderUtilities.ID_FaceColor);
            AddColor("buttonTextSecondaryColor", buttonTextSecondaryMaterial, ref buttonTextSecondaryColor, ShaderUtilities.ID_FaceColor);
            AddColor("defaultOutlineColor", defaultOutlineMaterial, ref defaultOutlineColor);
            AddColor("overlayDefaultColor", overlayDefaultMaterial, ref overlayDefaultColor);
        }

        /// <summary>
        /// Adds color to the dictionary and sets the color for the material.
        /// </summary>
        private void AddColor(string key, Material material, ref Color32 color, int propertyID = 0)
        {
            color = propertyID == 0 ? material.color : material.GetColor(propertyID);
            colors.Add(key, color);
        }

        /// <summary>
        /// Update is called once per frame.
        /// Loads material configuration if in debug mode and 'M' key is pressed.
        /// </summary>
        void Update()
        {
            if (configurationManager.debug && Input.GetKeyDown(KeyCode.M))
            {
                LoadMaterialConfiguration("DefaultColorStyling");
            }
        }

        /// <summary>
        /// Loads material configuration from the specified JSON file.
        /// </summary>
        /// <param name="importPath">The path of the JSON file to import.</param>
        public void LoadMaterialConfiguration(string importPath)
        {
            string path = Path.Combine(Application.streamingAssetsPath, "Configuration", $"{importPath}.json");

            using (StreamReader reader = new StreamReader(path))
            {
                string jsonString = reader.ReadToEnd().Trim();
                Dictionary<string, Color32> config = JsonConvert.DeserializeObject<Dictionary<string, Color32>>(jsonString);

                foreach (var key in config.Keys)
                {
                    colors[key] = config[key];
                }
            }

            UpdateMaterials();
        }

        /// <summary>
        /// Updates all materials with the colors from the dictionary.
        /// </summary>
        void UpdateMaterials()
        {
            UpdateMaterialColor(displayFrameMaterial, displayFrameColor);
            UpdateMaterialColor(backgroundDisplayMaterial, backgroundDisplayColor);
            UpdateMaterialColor(backgroundPrimaryMaterial, backgroundPrimaryColor);
            UpdateMaterialColor(backgroundSecondaryMaterial, backgroundSecondaryColor);
            UpdateMaterialColor(buttonSelectedMaterial, buttonSelectedColor);
            UpdateMaterialColor(buttonPressedMaterial, buttonPressedColor);
            UpdateMaterialColor(buttonHoverMaterial, buttonHoverColor);
            UpdateMaterialColor(buttonIconActiveMaterial, buttonIconActiveColor);
            UpdateMaterialColor(buttonIconInactiveMaterial, buttonIconInactiveColor);
            UpdateMaterialColor(buttonPrimaryActiveMaterial, buttonPrimaryActiveColor);
            UpdateMaterialColor(buttonPrimaryInactiveMaterial, buttonPrimaryInactiveColor);
            UpdateMaterialColor(buttonSecondaryActiveMaterial, buttonSecondaryActiveColor);
            UpdateMaterialColor(buttonSecondaryInactiveMaterial, buttonSecondaryInactiveColor);
            UpdateMaterialColor(foregroundPrimaryMaterial, foregroundPrimaryColor);
            UpdateMaterialColor(foregroundSecondaryMaterial, foregroundSecondaryColor);
            UpdateMaterialColor(iconColorMaterial, iconColor);
            UpdateMaterialColor(highlightFlashMaterial, highlightFlashColor);
            UpdateMaterialColor(textActiveMaterial, textActiveColor, ShaderUtilities.ID_FaceColor);
            UpdateMaterialColor(buttonTextPrimaryMaterial, buttonTextPrimaryColor, ShaderUtilities.ID_FaceColor);
            UpdateMaterialColor(buttonTextSecondaryMaterial, buttonTextSecondaryColor, ShaderUtilities.ID_FaceColor);
            UpdateMaterialColor(defaultOutlineMaterial, defaultOutlineColor);
            UpdateMaterialColor(overlayDefaultMaterial, overlayDefaultColor);
        }

        /// <summary>
        /// Updates the color of a material.
        /// </summary>
        private void UpdateMaterialColor(Material material, Color32 color, int propertyID = 0)
        {
            if (propertyID == 0)
            {
                material.color = color;
            }
            else
            {
                material.SetColor(propertyID, color);
            }
        }
    }
}
