using System;
using UnityEngine;
using Valve.VR;

namespace ROVER.Overlay
{
    /// <summary>
    /// Utility class for OpenVR-related functions.
    /// </summary>
    public static class OpenVR_Utility
    {
        /// <summary>
        /// Gets the texture type based on the graphics device type.
        /// </summary>
        public static ETextureType textureType
        {
            get
            {
                switch (SystemInfo.graphicsDeviceType)
                {
                    case UnityEngine.Rendering.GraphicsDeviceType.OpenGLCore:
                    case UnityEngine.Rendering.GraphicsDeviceType.OpenGLES2:
                    case UnityEngine.Rendering.GraphicsDeviceType.OpenGLES3:
                        return ETextureType.OpenGL;
                    case UnityEngine.Rendering.GraphicsDeviceType.Vulkan:
                        return ETextureType.Vulkan;
                    default:
                        return ETextureType.DirectX;
                }
            }
        }
    }

    /// <summary>
    /// Utility class for various overlay-related functions.
    /// </summary>
    public static class OverlayUtilities
    {
        /// <summary>
        /// Simplifies the boilerplate for creating singleton controller objects.
        /// </summary>
        public static T Singleton<T>(ref T _instance, string name, bool create = true) where T : MonoBehaviour
        {
            if (_instance == null)
            {
                _instance = UnityEngine.Object.FindObjectOfType<T>();

                if (_instance == null)
                {
                    if (create)
                    {
                        var obj = new GameObject(name);
                        _instance = obj.AddComponent<T>();
                    }
                    else
                    {
                        Debug.LogErrorFormat("Instance for {0} ({1}) does not exist in the scene", name, typeof(T).Name);
                        return null;
                    }
                }
            }

            return _instance;
        }

        /// <summary>
        /// Generates a key for the OpenVR overlay using application details.
        /// </summary>
        public static string GetKey(params string[] keys)
        {
            return "unity:" + Application.companyName + "." + Application.productName + "." + string.Join(".", keys);
        }

        /// <summary>
        /// Safely creates an OpenVR overlay.
        /// </summary>
        public static bool CreateOverlay(string key, string name, ref ulong handle)
        {
            var overlay = OpenVR.Overlay;
            if (overlay == null)
            {
                Debug.LogWarning("Overlay system not available");
                return false;
            }

            return ReportError(overlay.CreateOverlay(key, name, ref handle));
        }

        /// <summary>
        /// Logs overlay errors if they are not None.
        /// </summary>
        public static bool ReportError(EVROverlayError err)
        {
            if (err != EVROverlayError.None)
            {
                Debug.LogWarning(OpenVR.Overlay.GetOverlayErrorNameFromEnum(err));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if an overlay is facing the HMD.
        /// </summary>
        public static bool IsFacingHmd(Transform overlayTransform)
        {
            var hmd = TrackedHMDAttachment.Transform;

            // Overlays face -z, so .forward is actually the back of the overlay
            var dot = Vector3.Dot(overlayTransform.forward, (hmd.position - overlayTransform.position).normalized);
            // Positive numbers mean the overlay is not facing the HMD
            return dot <= 0f;
        }

        /// <summary>
        /// Rotates the overlay to face the HMD.
        /// </summary>
        public static void TurnToHmd(Transform overlayTransform)
        {
            var hmd = TrackedHMDAttachment.Transform;
            overlayTransform.LookAt(hmd);
            overlayTransform.Rotate(new Vector3(0, 180, 0));
        }

        /// <summary>
        /// Helper class for interacting with OpenVR Overlay methods.
        /// </summary>
        public class OverlayHelper
        {
            private ulong handle = OpenVR.k_ulOverlayHandleInvalid;
            private CVROverlay overlay;

            /// <summary>
            /// Indicates if the overlay helper is valid.
            /// </summary>
            public bool Valid
            {
                get
                {
                    return overlay != null && handle != OpenVR.k_ulOverlayHandleInvalid;
                }
            }

            /// <summary>
            /// Initializes a new instance of the OverlayHelper class.
            /// </summary>
            public OverlayHelper(bool warn = true)
            {
                overlay = OpenVR.Overlay;
                if (overlay == null && warn)
                {
                    Debug.LogError("Overlay system not available");
                }
            }

            /// <summary>
            /// Initializes a new instance of the OverlayHelper class with a handle.
            /// </summary>
            public OverlayHelper(ulong handle, bool warn = true) : this(warn)
            {
                this.handle = handle;
            }

            /// <summary>
            /// Destroys the overlay.
            /// </summary>
            public void Destroy()
            {
                ReportError(overlay.DestroyOverlay(handle));
            }

            /// <summary>
            /// Shows the overlay.
            /// </summary>
            public void Show()
            {
                ReportError(overlay.ShowOverlay(handle));
            }

            /// <summary>
            /// Hides the overlay.
            /// </summary>
            public void Hide()
            {
                ReportError(overlay.HideOverlay(handle));
            }

            /// <summary>
            /// Sets the sort order of the overlay.
            /// </summary>
            public void SetOrder(uint order)
            {
                ReportError(overlay.SetOverlaySortOrder(handle, order));
            }

            /// <summary>
            /// Sets the color of the overlay.
            /// </summary>
            public void SetColor(float red, float green, float blue)
            {
                ReportError(overlay.SetOverlayColor(handle, red, green, blue));
            }

            /// <summary>
            /// Sets the color of the overlay without affecting alpha.
            /// </summary>
            public void SetColorWithoutAlpha(Color color)
            {
                SetColor(color.r, color.g, color.b);
            }

            /// <summary>
            /// Sets the alpha value of the overlay.
            /// </summary>
            public void SetAlpha(float alpha)
            {
                ReportError(overlay.SetOverlayAlpha(handle, alpha));
            }

            /// <summary>
            /// Sets both the color and alpha of the overlay.
            /// </summary>
            public void SetColorWithAlpha(Color color)
            {
                SetColorWithoutAlpha(color);
                SetAlpha(color.a);
            }

            /// <summary>
            /// Sets the width of the overlay in meters.
            /// </summary>
            public void SetWidthInMeters(float width)
            {
                ReportError(overlay.SetOverlayWidthInMeters(handle, width));
            }

            /// <summary>
            /// Sets the curvature of the overlay.
            /// </summary>
            public void SetOverlayCurvature(float curvature)
            {
                ReportError(overlay.SetOverlayCurvature(handle, curvature));
            }

            /// <summary>
            /// Sets the overlay texture to a Unity texture.
            /// </summary>
            public void SetTexture(Texture texture)
            {
                var tex = new Texture_t
                {
                    handle = texture.GetNativeTexturePtr(),
                    eType = OpenVR_Utility.textureType,
                    eColorSpace = EColorSpace.Auto
                };
                ReportError(overlay.SetOverlayTexture(handle, ref tex));
            }

            /// <summary>
            /// Sets the texture bounds of the overlay.
            /// </summary>
            public void SetTextureBounds(float xMin, float yMin, float xMax, float yMax)
            {
                // u=horizontal/x, v=vertical/y
                // OpenVR's vMin is the top of the overlay, and vMax is the bottom
                var textureBounds = new VRTextureBounds_t
                {
                    uMin = xMin,
                    vMin = yMax,
                    uMax = xMax,
                    vMax = yMin
                };
                ReportError(overlay.SetOverlayTextureBounds(handle, ref textureBounds));
            }

            /// <summary>
            /// Resets the texture bounds so the whole texture fills the overlay.
            /// </summary>
            public void FillTextureBounds()
            {
                SetTextureBounds(0, 0, 1, 1);
            }

            /// <summary>
            /// Sets the overlay to be filled with a Unity texture.
            /// </summary>
            public void SetFullTexture(Texture texture)
            {
                SetTexture(texture);
                FillTextureBounds();
            }

            /// <summary>
            /// Sets the input method of the overlay.
            /// </summary>
            public void SetInputMethod(VROverlayInputMethod inputMethod)
            {
                ReportError(overlay.SetOverlayInputMethod(handle, inputMethod));
            }

            /// <summary>
            /// Sets the mouse scale of the overlay.
            /// </summary>
            public void SetMouseScale(float v0, float v1)
            {
                var vecMouseScale = new HmdVector2_t
                {
                    v0 = v0,
                    v1 = v1
                };
                ReportError(overlay.SetOverlayMouseScale(handle, ref vecMouseScale));
            }

            /// <summary>
            /// Sets an absolute transform for the overlay using an OpenVR HmdMatrix.
            /// </summary>
            public void SetTransformAbsolute(ETrackingUniverseOrigin trackingUniverseOrigin, ref HmdMatrix34_t t)
            {
                ReportError(overlay.SetOverlayTransformAbsolute(handle, trackingUniverseOrigin, ref t));
            }

            /// <summary>
            /// Sets the overlay transform projection for the left eye.
            /// </summary>
            public void SetOverlayTransformProjectionLeft(ETrackingUniverseOrigin trackingUniverseOrigin, SteamVR_Utils.RigidTransform rigidTransform)
            {
                var t = rigidTransform.ToHmdMatrix34();
                var p = new VROverlayProjection_t { fLeft = 1 };
                ReportError(overlay.SetOverlayTransformProjection(handle, trackingUniverseOrigin, ref t, ref p, EVREye.Eye_Left));
            }

            /// <summary>
            /// Sets the overlay transform projection for the right eye.
            /// </summary>
            public void SetOverlayTransformProjectionRight(ETrackingUniverseOrigin trackingUniverseOrigin, SteamVR_Utils.RigidTransform rigidTransform)
            {
                var t = rigidTransform.ToHmdMatrix34();
                var p = new VROverlayProjection_t { fRight = 1 };
                ReportError(overlay.SetOverlayTransformProjection(handle, trackingUniverseOrigin, ref t, ref p, EVREye.Eye_Right));
            }

            /// <summary>
            /// Sets an absolute transform for the overlay using a RigidTransform.
            /// </summary>
            public void SetTransformAbsolute(ETrackingUniverseOrigin trackingUniverseOrigin, SteamVR_Utils.RigidTransform rigidTransform)
            {
                var t = rigidTransform.ToHmdMatrix34();
                SetTransformAbsolute(trackingUniverseOrigin, ref t);
            }

            /// <summary>
            /// Sets an absolute transform for the overlay using a Unity Transform.
            /// </summary>
            public void SetTransformAbsolute(ETrackingUniverseOrigin trackingUniverseOrigin, Transform transform)
            {
                SetTransformAbsolute(trackingUniverseOrigin, new SteamVR_Utils.RigidTransform(transform));
            }
        }
    }
}
