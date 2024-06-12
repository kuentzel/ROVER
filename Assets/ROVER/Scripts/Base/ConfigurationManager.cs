using ROVER.Avatar;
using ROVER.Overlay;
using ROVER.Sensors;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ROVER
{
    /// <summary>
    /// Structure to hold ROVER configuration settings.
    /// </summary>
    struct ROVERSettings
    {
        public int ImportMode;
        public int ExportMode;
        public string ColorStylingFile;
        public string LocalizationFile;
        public string LeftPointerOffsetFile;
        public string RightPointerOffsetFile;
        public int InputFiltering;
        public int LeftPointerColorizeOnHover;
        public int RightPointerColorizeOnHover;
        public string[] RPM_URLs;
        public int RPM_EyeAnimationEnabled; // 0 = None, 1 = RPM, 2 = Custom
        public int RPM_VoiceAnimationEnabled; // 0 = None, 1 = RPM, 2 = Custom
        public int HeartRateSensor;
        public float HeightOffset;
        public float RotationOffsetX;
        public float RotationOffsetY;
        public float RotationOffsetZ;
    }

    /// <summary>
    /// Manages the configuration settings for the ROVER application.
    /// </summary>
    public class ConfigurationManager : MonoBehaviour
    {
        public static ConfigurationManager instance;
        public static string mainConfigurationFilePath = Application.streamingAssetsPath + "/Configuration/MainConfiguration.json";

        public bool debug = false;

        public StudyManager studyManager;
        public StyleManager styleManager;
        public LocalizationManager localizationManager;
        public VirtualPointer leftPointer;
        public VirtualPointer rightPointer;
        public AvatarManager avatarManager;
        public ResultManager resultManager;
        public OverlayManager overlayManager;
        public SensorLogger sLogger;
        public HeartRateController hrController;
        public GameObject hrContainer;
        public Transform station;

        private ROVERSettings configuration;

        /// <summary>
        /// Initializes the ConfigurationManager and loads the configuration settings.
        /// </summary>
        void Start()
        {
            instance = this;
            LoadConfiguration();
        }

        /// <summary>
        /// Loads the configuration from the main configuration file.
        /// </summary>
        public void LoadConfiguration()
        {
            string importPath = mainConfigurationFilePath;

            using (StreamReader reader = new StreamReader(importPath))
            {
                string jsonString = reader.ReadToEnd().Trim();
                configuration = JsonUtility.FromJson<ROVERSettings>(jsonString);
            }

            ApplyConfiguration();
        }

        /// <summary>
        /// Applies the configuration settings to the various components of the system.
        /// </summary>
        public void ApplyConfiguration()
        {
            // Apply import mode setting to study manager
            if (configuration.ImportMode >= 0 && configuration.ImportMode < Enum.GetNames(typeof(ImportFileType)).Length)
                studyManager.importExportManager.fileType = (ImportFileType)configuration.ImportMode;

            // Apply export mode setting to result manager
            if (configuration.ExportMode >= 0 && configuration.ExportMode < Enum.GetNames(typeof(ExportMode)).Length)
                resultManager.exportMode = (ExportMode)configuration.ExportMode;
            else
                resultManager.exportMode = ExportMode.Default;

            // Load color styling file if specified
            if (!string.IsNullOrEmpty(configuration.ColorStylingFile))
                styleManager.LoadMaterialConfiguration(configuration.ColorStylingFile);

            // Load localization file if specified
            if (!string.IsNullOrEmpty(configuration.LocalizationFile))
                localizationManager.LoadLocalization(configuration.LocalizationFile);

            // Load left pointer offset file if specified
            if (!string.IsNullOrEmpty(configuration.LeftPointerOffsetFile))
                leftPointer.LoadControllerConfiguration(configuration.LeftPointerOffsetFile);

            // Load right pointer offset file if specified
            if (!string.IsNullOrEmpty(configuration.RightPointerOffsetFile))
                rightPointer.LoadControllerConfiguration(configuration.RightPointerOffsetFile);

            // Set eye animation mode for avatar manager
            avatarManager.eyeAnimation = (EyeHandling)configuration.RPM_EyeAnimationEnabled;

            // Set voice animation mode for avatar manager
            avatarManager.voiceAnimation = (VoiceHandling)configuration.RPM_VoiceAnimationEnabled;

            // Load avatars if RPM URLs are specified
            if (configuration.RPM_URLs != null && configuration.RPM_URLs.Length > 0)
                avatarManager.StartCoroutine(avatarManager.LoadAvatars(new HashSet<string>(configuration.RPM_URLs)));

            // Set colorize on hover for left pointer
            leftPointer.colorizeOnHover = configuration.LeftPointerColorizeOnHover == 1;

            // Set colorize on hover for right pointer
            rightPointer.colorizeOnHover = configuration.RightPointerColorizeOnHover == 1;

            // Enable heart rate sensor if specified
            if (configuration.HeartRateSensor == 1)
            {
                sLogger.enabled = true;
                hrController.enabled = true;
                hrContainer.SetActive(true);
            }

            // Set height offset for station
            if (configuration.HeightOffset != 0)
                station.localPosition = new Vector3(0, configuration.HeightOffset, 0);

            // Set rotation offset for station
            if (configuration.RotationOffsetX != 0 || configuration.RotationOffsetY != 0 || configuration.RotationOffsetZ != 0)
                station.localRotation = Quaternion.Euler(configuration.RotationOffsetX, configuration.RotationOffsetY, configuration.RotationOffsetZ);

            // Set input filtering for overlay manager
            overlayManager.SetInputFiltering(configuration.InputFiltering == 1);
        }

        /// <summary>
        /// Handles keyboard shortcuts for debugging purposes.
        /// </summary>
        void Update()
        {
            if (debug && Input.GetKeyDown(KeyCode.L))
            {
                LoadConfiguration();
            }
        }
    }
}
