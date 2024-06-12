using System;
using System.Collections;
using AOT;
using ManagedRender;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using Valve.VR;



public class OverlaySceneRenderer : MonoBehaviour
{
    //This way of projecting a 3D overlay over a scene application is based on the way Vermillion (https://store.steampowered.com/app/1608400/Vermillion__VR_Painting/) and Aardvark (https://github.com/aardvarkxr/aardvark/tree/master) do things
    //Similar projects (sometimes based on the recent OpenVR API addition IVROverlay::SetOverlayTransformProjection) include https://barshiftgames.itch.io/portable-emulator, https://store.steampowered.com/app/1434890/Portable_Farm/, https://github.com/cnlohr/openvr_overlay_model (overlay with different render image for each eye), and https://store.steampowered.com/app/755540/LIV/ (builds a cube around the user)

    [SerializeField]
    protected Camera leftCamera;

    [SerializeField]
    protected Camera rightCamera;

    [SerializeField]
    private Shader shader;

    private RenderTexture overlayRT;

    private Material overlayPanoramaMaterial;

    private RenderTexture leftRT;

    private RenderTexture rightRT;

    private Coroutine updateOverlayRoutine;

    private WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

    private Matrix4x4 hmdMatrix;

    public float frameDurationAdjust = 1f;

    private ulong lastFrameCount;

    private static OverlaySceneRenderer instance;

    private static ulong handle = 0uL;

    private static ulong inputCaptureDisabledDashboardOverlay = 0uL;

    private static Texture_t overlayTexture;

    private static TrackedDevicePose_t[] poses = new TrackedDevicePose_t[64];

    private static float timeSinceLastOverlayUpdate;

    private static DateTime timeOfLastFrame;

    private static int EyeLeftShaderId = Shader.PropertyToID("_EyeLeft");

    private static int EyeRightShaderId = Shader.PropertyToID("_EyeRight");

    private static int LookRotationShaderId = Shader.PropertyToID("_LookRotation");

    private static int HalfFovInRadiansShaderId = Shader.PropertyToID("_HalfFOVInRadians");

    public bool debug;

    public bool includeCameraAspect = true;
    public float[] bounds = { 0f, 1f, 1f, 0f };


    private void Start()
    {
        //The texture can be rendered by one or by two
        if (rightCamera != null)
        {
            CreateEyeTextures();
            //Automatically set headset FOV on both cameras
            float foV = SteamVR.instance.fieldOfView;
            rightCamera.fieldOfView = foV;
            leftCamera.fieldOfView = foV;
        }

        //Initialize the Error Code reference to be passed into the function below
        EVRSettingsError peError = EVRSettingsError.None;
        //Enable global Action Set Priority, so Overlay Applications can take complete control of input and block scene application input
        OpenVR.Settings.SetBool("steamvr", "globalActionSetPriority", bValue: true, ref peError);
        //Output HMD-specific screen refresh rate
        Debug.Log(string.Format("[{0}] Vsync to photons: {1} Hz: {2}", "Overlay Scene Renderer", SteamVR.instance.hmd_SecondsFromVsyncToPhotons, SteamVR.instance.hmd_DisplayFrequency));
    }
    public float overlayWidth = 3f;
    public VROverlayFlags flags = VROverlayFlags.StereoPanorama;
    public float[] overlayMatrix = { 1f, 0f, 0f, 0f, 0f, 1f, 0f, 0f, 0f, 0f, 1f, -1f };

    private void OnEnable()
    {
        instance = this;
        //Check if SteamVR Overlay API is running
        CVROverlay overlay = OpenVR.Overlay;
        if (overlay == null)
        {
            Debug.LogError("Can't access overlays. Is an OpenVR runtime active?");
            return;
        }
        //Try to create scene overlay everytime the component is enabled, handle gets assigned by API
        EVROverlayError eVROverlayError = overlay.CreateOverlay(SteamVR_Overlay.key, "ROVER Scene Overlay", ref handle);
        if (eVROverlayError != 0)
        {
            Debug.LogError("Couldn't create overlay: " + overlay.GetOverlayErrorNameFromEnum(eVROverlayError));
            base.enabled = false;
            return;
        }
        //Create material on first enable
        if (!overlayPanoramaMaterial)
        {
            if (shader == null)
                shader = Shader.Find("VR/OpenVROverlayPanorama");
            overlayPanoramaMaterial = new Material(shader);
            //Should always have both cameras enabled and thus enable the separate cameras keyword
        }

        //Initialize matrix full of 0, HmdMatrix34_t represents a row-major rigid transform (rotation plus translation)
        //// the first 3 columns represent the rotation vectors around X,Y,Z axis and the 4th colum is the translation vector
        // 0 0 0 0
        // 0 0 0 0
        // 0 0 0 0
        HmdMatrix34_t hmdMatrix34_t = default;
        //Configure Transformation
        hmdMatrix34_t.m0 = 1;
        hmdMatrix34_t.m1 = 0;
        hmdMatrix34_t.m2 = 0;
        hmdMatrix34_t.m3 = 0;
        hmdMatrix34_t.m4 = 0;
        hmdMatrix34_t.m5 = 1;
        hmdMatrix34_t.m6 = 0;
        hmdMatrix34_t.m7 = 0;
        hmdMatrix34_t.m8 = 0;
        hmdMatrix34_t.m9 = 0;
        hmdMatrix34_t.m10 = 1;
        hmdMatrix34_t.m11 = -1;
        //Apply Transformation (Identity matrix for Rotation, so no rotation, but translated by 1 unit in front of hmd, as Unity uses +Z forward and SteamVR/OpenVR uses -Z forward
        // 1 0 0 0
        // 0 1 0 0
        // 0 0 1 -1
        HmdMatrix34_t refMatTrackedDeviceToOverlayTransform = hmdMatrix34_t;
        

        //Attach overlay to HMD (device ID 0)
        overlay.SetOverlayTransformTrackedDeviceRelative(handle, 0u, ref refMatTrackedDeviceToOverlayTransform);
        //Turn overlay into a screen spanning StereoPanorama (maximum resolution)
        overlay.SetOverlayFlag(handle, VROverlayFlags.StereoPanorama, bEnabled: true);
        overlay.SetOverlayWidthInMeters(handle, overlayWidth);

        CreatePanoramaTexture();
        //Set default texture bounds to use full texture. UV Min is the upper left corner and UV Max is the lower right corner. By default overlays use the entire texture.
        //Based on https://github.com/cnlohr/openvr_overlay_model/blob/master/overlay_model_test.c and https://gist.github.com/Rectus/68dea55184412bf31959d941e1bd12bc "to control the way the texture is mapped" but inversed cause Unity
        
        VRTextureBounds_t pOverlayTextureBounds = default;
        pOverlayTextureBounds.uMin = 0f;
        pOverlayTextureBounds.uMax = 1f;
        pOverlayTextureBounds.vMin = 1f;
        pOverlayTextureBounds.vMax = 0f;
        eVROverlayError = OpenVR.Overlay.SetOverlayTextureBounds(handle, ref pOverlayTextureBounds);
        if (eVROverlayError != 0)
        {
            Debug.LogError("Couldn't set texture bounds: " + OpenVR.Overlay.GetOverlayErrorNameFromEnum(eVROverlayError));
        }        

        
        
        //Start Coroutine and subscribe to Render Event
        updateOverlayRoutine = StartCoroutine(RunUpdateOverlay());
        Application.onBeforeRender += Application_onBeforeRender;
    }

    
    private void OnDisable()
    {
        //Clear instance, stop coroutine and unsubscribe from Render Event
        instance = null;
        Application.onBeforeRender -= Application_onBeforeRender;
        if (updateOverlayRoutine != null)
        {
            StopCoroutine(updateOverlayRoutine);
        }
        //if overlay was created and handle assigned, destroy it now and reset handle
        if (handle != 0L)
        {
            if (OpenVR.Overlay != null)
            {
                OpenVR.Overlay.DestroyOverlay(handle);
            }
            handle = 0uL;
        }
        //can be deleted
        if (inputCaptureDisabledDashboardOverlay != 0L)
        {
            if (OpenVR.Overlay != null)
            {
                OpenVR.Overlay.DestroyOverlay(inputCaptureDisabledDashboardOverlay);
            }
            inputCaptureDisabledDashboardOverlay = 0uL;
        }
        //Destroy overlay renderTexture
        Destroy(overlayRT);
    }

    //When the GameObject is destroyed, destroy even eye RTs and Material
    private void OnDestroy()
    {
        Destroy(overlayPanoramaMaterial);
        Destroy(leftRT);
        Destroy(rightRT);
    }

    private void Update()
    {
        if (OpenVR.Overlay == null)
        {
            return;
        }
        leftCamera.clearFlags = CameraClearFlags.SolidColor;

        //Check if the SteamVR dashboard has become visible since the last frame and disable cameras to stop rendering the overlay
        bool dashboardVisible = OpenVR.Overlay.IsDashboardVisible();
        rightCamera.enabled = !dashboardVisible;
        leftCamera.enabled = !dashboardVisible;

        //Hide overlay when the SteamVR Dashboard is enabled to avoid occlusion and duplication (of controller models)
        if (OpenVR.Overlay.IsDashboardVisible())
        {
            OpenVR.Overlay.HideOverlay(handle);
        }

        if (debug)
        {
            CVROverlay overlay = OpenVR.Overlay;

            HmdMatrix34_t hmdMatrix34_t = default;
            //Configure Transformation
            hmdMatrix34_t.m0 = overlayMatrix[0];
            hmdMatrix34_t.m1 = overlayMatrix[1];
            hmdMatrix34_t.m2 = overlayMatrix[2];
            hmdMatrix34_t.m3 = overlayMatrix[3];
            hmdMatrix34_t.m4 = overlayMatrix[4];
            hmdMatrix34_t.m5 = overlayMatrix[5];
            hmdMatrix34_t.m6 = overlayMatrix[6];
            hmdMatrix34_t.m7 = overlayMatrix[7];
            hmdMatrix34_t.m8 = overlayMatrix[8];
            hmdMatrix34_t.m9 = overlayMatrix[9];
            hmdMatrix34_t.m10 = overlayMatrix[10];
            hmdMatrix34_t.m11 = overlayMatrix[11];
            //Apply Transformation (Identity matrix for Rotation, so no rotation, but translated by 1 unit in front of hmd, as Unity uses +Z forward and SteamVR/OpenVR uses -Z forward
            // 1 0 0 0
            // 0 1 0 0
            // 0 0 1 -1
            HmdMatrix34_t refMatTrackedDeviceToOverlayTransform = hmdMatrix34_t;


            //Attach overlay to HMD (device ID 0)
            overlay.SetOverlayTransformTrackedDeviceRelative(handle, 0u, ref refMatTrackedDeviceToOverlayTransform);
            //Turn overlay into a screen spanning StereoPanorama (maximum resolution)
            overlay.SetOverlayFlag(handle, flags, bEnabled: true);
            overlay.SetOverlayWidthInMeters(handle, overlayWidth);

            //Set default texture bounds to use full texture. UV Min is the upper left corner and UV Max is the lower right corner. By default overlays use the entire texture.
            //Based on https://github.com/cnlohr/openvr_overlay_model/blob/master/overlay_model_test.c and https://gist.github.com/Rectus/68dea55184412bf31959d941e1bd12bc "to control the way the texture is mapped" but inversed cause Unity

            VRTextureBounds_t pOverlayTextureBounds = default;
            pOverlayTextureBounds.uMin = bounds[0];
            pOverlayTextureBounds.uMax = bounds[1];
            pOverlayTextureBounds.vMin = bounds[2];
            pOverlayTextureBounds.vMax = bounds[3];
            EVROverlayError eVROverlayError = OpenVR.Overlay.SetOverlayTextureBounds(handle, ref pOverlayTextureBounds);
        }

        //Check if below framerate
        if (timeSinceLastOverlayUpdate > 1.05f / SteamVR.instance.hmd_DisplayFrequency)
        {
            Debug.Log("Overlay update too slow");
        }
    }

    public bool predictPose = true;
    private void Application_onBeforeRender()
    {
        //Check if SteamVR runtime is active
        CVRSystem cVRSystem = SteamVR.instance?.hmd;
        if (cVRSystem == null)
        {
            return;
        }
        //Try to get number of elapsed seconds since the last recorded vsync event, returns false if no vsync times are available
        float secondsSinceLastVsync = 0f;
        ulong frameCounter = 0uL;
        if (!cVRSystem.GetTimeSinceLastVsync(ref secondsSinceLastVsync, ref frameCounter))
        {
            return;
        }
        //in Debug Build show screen refresh sync fails 
        if (debug)
        {
            long num = (long)(frameCounter - (lastFrameCount));
            if (num > 0)
            {
                Debug.LogWarning("skipped " + waitFrameSyncRenderQueue + " frames");
            }
            lastFrameCount = frameCounter;
        }
        // the quotient is the frame duration and the number of seconds from now to when the next photons will come out of the HMD can be computed automatically. This assumes that the rendering pipeline doesn't have any extra frames buffering.
        float predictedSecondsToPhotonsFromNow = frameDurationAdjust / SteamVR.instance.hmd_DisplayFrequency - secondsSinceLastVsync + SteamVR.instance.hmd_SecondsFromVsyncToPhotons;
        if (!predictPose)
            predictedSecondsToPhotonsFromNow = 0;
        //Get the predicted pose for the next screen refresh
        cVRSystem.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, predictedSecondsToPhotonsFromNow, poses);
        //But we only care about the HMDs rotation and translation with regards to the origin of the standing tracking universe
        SteamVR_Utils.RigidTransform rigidTransform = new SteamVR_Utils.RigidTransform(poses[0].mDeviceToAbsoluteTracking);
        //The 4x4 Matrix is Translation, Rotation and Scaling matrix, with a standard identity scale and pos and rot from the HMDs rigid transform
        hmdMatrix = Matrix4x4.TRS(rigidTransform.pos, rigidTransform.rot, Vector3.one);
        //Left eye rigid transform relative to HMD
        Matrix4x4 matrix4x = Matrix4x4.TRS(SteamVR.instance.eyes[0].pos, SteamVR.instance.eyes[0].rot, Vector3.one);
        //Right eye rigid transform relative to HMD
        Matrix4x4 matrix4x2 = Matrix4x4.TRS(SteamVR.instance.eyes[1].pos, SteamVR.instance.eyes[1].rot, Vector3.one);
        //We want the absolute (relative to tracking origin) transform of each eye to apply them to the render cameras
        leftCamera.transform.localPosition = (hmdMatrix * matrix4x).GetPosition();
        leftCamera.transform.localRotation = rigidTransform.rot;
        rightCamera.transform.localPosition = (hmdMatrix * matrix4x2).GetPosition();
        rightCamera.transform.localRotation = rigidTransform.rot;
    }

    public bool setFence = true;
    //This coroutine is based on twitter conversations https://twitter.com/thmsvdberg/status/1621946510744117248 and https://twitter.com/thegiantsox/status/1583562477396103169
    //involving Thomas van den Berge (Vermillion), Charles Lohr (https://github.com/cnlohr), David Goodman (https://davidgoodman.me/) and Rectus (https://github.com/Rectus)
    private IEnumerator RunUpdateOverlay()
    {
        //Only run while the application is playing, but should be stopped on Destroy anyway
        while (Application.isPlaying)
        {
            //Wait with this until after render, so until after onBeforeRender has gone and bc you only want to run it once per frame
            yield return waitForEndOfFrame;
            //The cameras are always rendering to the target texture when enabled, when the left one is enabled, the right one is too if it exists
            if (leftCamera.enabled && (bool)leftRT && (bool)rightRT && (bool)overlayRT)
            {
                //Stitch the overlayRT from the eyeRTs
                UpdatePanoramaMaterial();
                //For maximum synchronization, rendering quality and performance use a Command Buffer Queue after everything else has been rendered - we render this after the end of frame and then pass it to the overlay but with the predicted hmd movement at the end of the frame
                //This is the empty queue
                CommandBuffer commandBuffer = new CommandBuffer();
                //First copy the pixel data from the stitched panorama material onto the overlay RT, no texture needed cause it's in other Material Shader properties (LeftEye and RightEye)
                //The material shader does not have a _MainTex property, so Blit does not use the Texture source (which is null here)
				commandBuffer.Blit(null, overlayRT, overlayPanoramaMaterial);
                //Then Wait until all other GPU Operations are finished, so there is no delay between the predicted overlay content and the hmd position when the photons go brrrrr
                GraphicsFence fence = commandBuffer.CreateGraphicsFence(GraphicsFenceType.AsyncQueueSynchronisation, SynchronisationStageFlags.AllGPUOperations);
                if (setFence)
                commandBuffer.WaitOnAsyncGraphicsFence(fence);
                //Lastly in a round-about way run the method to update the overlay in the compositor
                //the regular CommandBuffer.IssuePluginEventAndData is overwritten in ManagedRenderEvent to safely call managed method (our Mono Callback UpdateOverlay method) from graphics thread
                if (waitFrameSyncRenderQueue)
                    commandBuffer.IssuePluginEventAndData(UpdateSceneOverlay, 111, IntPtr.Zero);
                else
                    commandBuffer.IssuePluginEventAndData(UpdateSceneOverlay, 112, IntPtr.Zero);
                //Release the Queue
                Graphics.ExecuteCommandBuffer(commandBuffer);
            }
        }
    }

    public bool inverseHMDMatrixForShader = true;
    public bool scaleHMDMatrixForShader = true;
    public float[] scaleVector = { 1f, -1f, 1f };

    //This is where the eye RTs are stitched together to the panorama
    private void UpdatePanoramaMaterial()
    {
        //This is based on a twitter conversation https://twitter.com/joeludwig/status/1622023049800466432 between Thomas van der Berge and work of the Aardvark team https://github.com/aardvarkxr/aardvark/tree/master, specifically Joe Ludwig. The method takes from the vrmanager.cpp for getting the look direction  and the varggles.frag shader for the FOV operations
        //Inverse the HMD transform (position and rotation) and mirror the direction using a scale operation (Unity is +Z forward to SteamVR -Z forward) 
        Matrix4x4 inverse = hmdMatrix;
        if (inverseHMDMatrixForShader)
            inverse = hmdMatrix.inverse;
        if (scaleHMDMatrixForShader)
            inverse = Matrix4x4.Scale(new Vector3(scaleVector[0], scaleVector[1], scaleVector[2])) * inverse;
        //Set Look Direction
        overlayPanoramaMaterial.SetMatrix(LookRotationShaderId, inverse);
        //This is important to pass to the shader to get the proper panorama curve for the FOV
        float aspectOverride = 1;
        if (includeCameraAspect)
            aspectOverride = leftCamera.aspect;
        
        overlayPanoramaMaterial.SetFloat(HalfFovInRadiansShaderId, MathF.PI / 180f * (leftCamera.fieldOfView * aspectOverride) * 0.5f);
        
        //pass the texture of each eye RT to the panorama
        if (leftRT!=null && rightRT!=null)
        {
            overlayPanoramaMaterial.SetTexture(EyeLeftShaderId, leftRT);
            overlayPanoramaMaterial.SetTexture(EyeRightShaderId, rightRT);
        }
    }

    public bool waitFrameSyncRenderQueue = true;

    [MonoPInvokeCallback(typeof(RenderPluginDelegate))]
    private static void UpdateSceneOverlay(int eventId, IntPtr data)
    {
        if (OpenVR.Overlay != null && !OpenVR.Overlay.IsDashboardVisible())
        {
            ETrackedPropertyError pError = ETrackedPropertyError.TrackedProp_Success;
            //float num = Mathf.Max(1f, OpenVR.System.GetFloatTrackedDeviceProperty(0u, ETrackedDeviceProperty.Prop_DisplayFrequency_Float, ref pError));
            //WaitFrameSync will block until the top of each frame, and can therefore be used to synchronize with the runtime's update rate. Added with OpenVR SDK 1.23.7, requiring manual updating of the OpenVR Unity XR Plugin (OpenVR Loader) by Valve (1.4.4)
            if (eventId == 111)
                OpenVR.Overlay.WaitFrameSync((uint)(1000f)); // num));
            EVROverlayError eVROverlayError = OpenVR.Overlay.SetOverlayTexture(handle, ref overlayTexture);
            if (eVROverlayError != 0)
                Debug.LogError("Could not set overlay texture: " + OpenVR.Overlay.GetOverlayErrorNameFromEnum(eVROverlayError));

            eVROverlayError = OpenVR.Overlay.ShowOverlay(handle);
            if (eVROverlayError != 0)
                Debug.LogError("Could not show overlay: " + OpenVR.Overlay.GetOverlayErrorNameFromEnum(eVROverlayError));

            timeSinceLastOverlayUpdate = (float)(DateTime.Now - timeOfLastFrame).TotalSeconds;
            timeOfLastFrame = DateTime.Now;
        }
    }

    [SerializeField]
    private int eyeResolution = 2048;
    //Creates and assigns Render Texture for each eye
    private void CreateEyeTextures()
    {
        //Unassign and destroy previous RT
        leftCamera.targetTexture = null;
        rightCamera.targetTexture = null;
        Destroy(leftRT);
        Destroy(rightRT);
        //Scale RT to configured resolution
        uint num = 2048u;
        uint num2 = 2048u;
        //Initialize and Create RT, then assign to camera as target
        leftRT = new RenderTexture((int)num, (int)num2, 24)
        {
            antiAliasing = 4,
            autoGenerateMips = false
        };
        leftRT.Create();
        leftCamera.targetTexture = leftRT;
        rightRT = new RenderTexture((int)num, (int)num2, 24)
        {
            antiAliasing = 4,
            autoGenerateMips = false
        };
        rightRT.Create();
        rightCamera.targetTexture = rightRT;
    }

    [SerializeField]
    private int panoramaResolution = 4096;
    //Create and Scale the Overlay Render Texture
    private void CreatePanoramaTexture()
    {
        //Square Overlay Texture, because it is merged together into a FOV panorama texture via shader from the two square eye textures
        //Destroy previous texture
        Destroy(overlayRT);
        overlayRT = new RenderTexture(panoramaResolution, panoramaResolution, 0);
        overlayRT.Create();
        overlayTexture = default;
        //Link overlay texture to be passed to OpenVR API to the pointer of the RT
        overlayTexture.handle = overlayRT.GetNativeTexturePtr();
        overlayTexture.eType = SteamVR.instance.textureType;
        overlayTexture.eColorSpace = EColorSpace.Auto;
    }

}
