using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using Valve.VR;

namespace ROVER.Overlay
{
    /// <summary>
    /// Enum representing the state of the overlay.
    /// </summary>
    public enum OverlayState
    {
        None,
        Light,
        Scene
    }

    /// <summary>
    /// Manages VR overlay operations, including state changes and input filtering.
    /// </summary>
    public class OverlayManager : MonoBehaviour
    {
        public static OverlayManager instance;

        [Header("Overlay Settings")]
        public int targetFrameRate = 60;
        public OverlayState state;

        [Header("Renderers")]
        [SerializeField]
        private OverlaySceneRenderer overlaySceneRenderer;
        public List<OverlayTextureRenderer> overlayTextures = new List<OverlayTextureRenderer>();

        // Private fields
        private OverlayState prevState;
        private bool inputFiltering = true;
        private bool filteringInput;
        private bool shouldFilterInput;
        private bool dashboardVisible;
        private bool dashboardVisibleLastUpdate;
        private OverlayState preDashboardState;

        /// <summary>
        /// Start is called before the first frame update.
        /// Initializes the overlay manager and sets the frame rate.
        /// </summary>
        private void Awake()
        {
            //StartCoroutine(ChangeFramerate(0, targetFrameRate));
            instance = this;
            
        }

        private void Start()
        {
            prevState = state;
        }

        /// <summary>
        /// Sets the input filtering state.
        /// </summary>
        /// <param name="b">True to enable input filtering, false to disable.</param>
        public void SetInputFiltering(bool b)
        {
            inputFiltering = b;
            if (inputFiltering)
            {
                EVRSettingsError peError = EVRSettingsError.None;
                if (OpenVR.Settings != null)
                {
                    OpenVR.Settings.SetBool("steamvr", "globalActionSetPriority", true, ref peError);
                }
            }
            else
            {
                SteamVR_Actions.pointer.Activate(SteamVR_Input_Sources.Any);
            }
        }

        /// <summary>
        /// Filters the input when the overlay is active.
        /// </summary>
        public void FilterInput()
        {
            if (OpenVR.Overlay != null && inputFiltering && !dashboardVisible && !filteringInput)
            {
                SteamVR_Actions.pointer.Activate(SteamVR_Input_Sources.Any, 33554430);
                filteringInput = true;
            }
        }

        /// <summary>
        /// Releases the input when the overlay is not active.
        /// </summary>
        public void ReleaseInput()
        {
            if (inputFiltering && filteringInput)
            {
                SteamVR_Actions.pointer.Deactivate();
                filteringInput = false;
            }
        }

        /// <summary>
        /// Updates the overlay state and manages input filtering based on the dashboard visibility.
        /// </summary>
        private void LateUpdate()
        {
            dashboardVisible = OpenVR.Overlay != null && OpenVR.Overlay.IsDashboardVisible();

            if (dashboardVisible)
            {
                ReleaseInput();
            }

            if (!dashboardVisibleLastUpdate && dashboardVisible)
            {
                preDashboardState = state;
                state = OverlayState.None;
                dashboardVisibleLastUpdate = true;
            }
            else if (dashboardVisibleLastUpdate && !dashboardVisible)
            {
                state = preDashboardState;
                dashboardVisibleLastUpdate = false;
            }

            if (state == prevState) return;

            switch (state)
            {
                case OverlayState.None:
                    SetOverlayState(false, false);
                    //StartCoroutine(ChangeFramerate(0, targetFrameRate));
                    break;
                case OverlayState.Light:
                    SetOverlayState(true, false);
                    //StartCoroutine(ChangeFramerate(0, targetFrameRate));
                    break;
                case OverlayState.Scene:
                    SetOverlayState(false, true);
                    //StartCoroutine(ChangeFramerate(0, -1));
                    break;
            }

            prevState = state;
        }

        /// <summary>
        /// Sets the enabled state of the overlay texture renderers and the scene renderer.
        /// </summary>
        /// <param name="overlayTexturesEnabled">Whether the overlay textures should be enabled.</param>
        /// <param name="overlaySceneRendererEnabled">Whether the overlay scene renderer should be enabled.</param>
        private void SetOverlayState(bool overlayTexturesEnabled, bool overlaySceneRendererEnabled)
        {
            foreach (var textureRenderer in overlayTextures)
            {
                textureRenderer.enabled = overlayTexturesEnabled;
            }
            overlaySceneRenderer.enabled = overlaySceneRendererEnabled;
        }

        /// <summary>
        /// Coroutine to change the frame rate after a delay.
        /// </summary>
        /// <param name="v">VSync count.</param>
        /// <param name="fr">Target frame rate.</param>
        /// <returns>IEnumerator for coroutine.</returns>
        private IEnumerator ChangeFramerate(int v, int fr)
        {
            yield return new WaitForSeconds(1);
            Debug.Log("Max Framerate: " + fr);
            QualitySettings.vSyncCount = v;
            Application.targetFrameRate = fr;
        }


        /// <summary>
        /// Called when the script is enabled.
        /// Initializes the overlay manager and sets up event listeners.
        /// </summary>
        private void OnEnable()
        {
            Initialize();
            SteamVR_Events.System(EVREventType.VREvent_Quit).Listen(OnQuit);
        }

        /// <summary>
        /// Called when the script is disabled.
        /// Shuts down the overlay manager and removes event listeners.
        /// </summary>
        private void OnDisable()
        {
            //SteamVR_Events.Initialized.Send(false);
            OpenVR.Shutdown();
            SteamVR_Events.System(EVREventType.VREvent_Quit).Remove(OnQuit);
        }

        /// <summary>
        /// Handles the VR quit event.
        /// </summary>
        /// <param name="ev">The VR event.</param>
        private void OnQuit(VREvent_t ev)
        {
            enabled = false;
            Debug.Log("OpenVR Quit event received, quitting");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// Initializes the overlay manager by connecting to the VR runtime.
        /// </summary>
        /// <returns>True if initialization was successful, false otherwise.</returns>
        private bool Initialize()
        {
            if (!ConnectToVRRuntime())
            {
                enabled = false;
                return false;
            }
            return true;
        }


        /// <summary>
        /// Connects to the VR runtime.
        /// </summary>
        /// <returns>True if the connection was successful, false otherwise.</returns>
        private bool ConnectToVRRuntime()
        {
            //If manually initializing SteamVR_Input, SteamVR_Action_Pose.SetTrackingUniverseOrigin(SteamVR_Settings.instance.trackingSpace) -> OpenVR.Compositor.SetTrackingSpace(newOrigin) will cause the Compositor projection to break for OpenXR standalone apps
            //SteamVR_Input.Initialize();
            //OpenVR.Compositor.SetTrackingSpace(...) is only called in the SteamVR Plugin in SteamVR_Action_Pose.SetTrackingUniverseOrigin(...) and in SteamVR_Render.RenderLoop(). When setting up actions manually, you still need to set their Tracking Universe Origin, but you can do it without setting Compositor Tracking Space. When using SteamVR_Behaviour Components, it will set up a SteamVR_Render component automatically, which will set the Compositor Tracking Space each render loop iteration. For our project, we added a check "if (SteamVR.isStandalone)" before the two occurences of OpenVR.Compositor.SetTrackingSpace(...) in the SteamVR Plugin, which works for our purposes.
            //Using SteamLink and a Quest headset, I have noticed that in OpenXR standalone apps like up-to-date BeatSaber, when opening the SteamVR Dashboard, the Tracking Universe gets flipped, meaning the game gets turned around 180 degrees around height axis, while the Dashboard appears in front. Could be related.

            //Cannot use regular SteamVR.InitializeStandalone(EVRApplicationType.VRApplication_Overlay) because the SteamVR Object and Render Components will be initialized and run their RenderLoop triggering OpenVR.Compositor.SetTrackingSpace(...) before SteamVR.isStandalone is set to true, which will cause the Compositor projection to break for OpenXR standalone apps
            //SteamVR.InitializeStandalone(EVRApplicationType.VRApplication_Overlay);
            // if (OpenVR.Overlay == null)
            // {
            //     Debug.Log("Overlay not found");
            //     return false;
            // }
            //SteamVR.Initialize(true);

            EVRInitError initError = EVRInitError.Unknown;
            if (OpenVR.Overlay == null)
            {   
                OpenVR.Init(ref initError, EVRApplicationType.VRApplication_Overlay);            
                if (OpenVR.Overlay == null)
                {
                    Debug.Log("Overlay not found");
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Gets a tracked device property as a string.
        /// </summary>
        /// <param name="prop">The device property.</param>
        /// <param name="deviceId">The device ID (default is HMD).</param>
        /// <returns>The property value as a string.</returns>
        public string GetStringTrackedDeviceProperty(ETrackedDeviceProperty prop, uint deviceId = OpenVR.k_unTrackedDeviceIndex_Hmd)
        {
            var error = ETrackedPropertyError.TrackedProp_Success;
            var result = new StringBuilder((int)OpenVR.k_unMaxPropertyStringSize);
            var capacity = OpenVR.System.GetStringTrackedDeviceProperty(deviceId, prop, result, OpenVR.k_unMaxPropertyStringSize, ref error);

            if (error == ETrackedPropertyError.TrackedProp_Success)
            {
                return result.ToString(0, (int)capacity - 1);
            }
            else
            {
                return "Error Getting String: " + error;
            }
        }
    }
}
