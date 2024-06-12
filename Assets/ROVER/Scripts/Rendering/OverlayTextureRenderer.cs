using System.Collections;
using UnityEngine;
using Valve.VR;

namespace ROVER.Overlay
{
    /// <summary>
    /// Renders a texture as an overlay in VR.
    /// </summary>
    public class OverlayTextureRenderer : MonoBehaviour
    {
        public enum Eye
        {
            Both,
            Left,
            Right
        }

        public enum RenderMode
        {
            Update,
            OnEnable,
            OnDemand,
        }

        [Header("Overlay Settings")]
        public string id;  // Unique identifier for the overlay
        public Texture texture;  // The primary texture to display
        public Texture backface;  // The texture to display on the back face
        public Color color = Color.white;  // Color tint of the overlay
        public RenderMode renderMode = RenderMode.Update;  // Determines when the overlay is rendered
        public Camera renderCamera;  // The camera to use for rendering the texture
        public float width = 1f;  // The width of the overlay in meters
        public Eye eye;  // Which eye(s) the overlay should be shown to
        public bool flipToHMD = true;  // Whether to flip the overlay to face the HMD
        public float curvature; // The curvature of the overlay
        public uint order;  // The rendering order of the overlay

        private ulong handle = OpenVR.k_ulOverlayHandleInvalid;  // Handle to the overlay
        private Texture lastTexture;  // Last texture that was set to the overlay
        private bool isFacingHmd = true;  // Whether the overlay is facing the HMD
        private bool queuedRendering = false;  // Whether rendering is queued

        /// <summary>
        /// Gets the overlay key.
        /// </summary>
        public string key => OverlayUtilities.GetKey("image", id);

        /// <summary>
        /// Adds this overlay texture renderer to the overlay manager and updates the overlay.
        /// </summary>
        void Update()
        {
            // Ensure this overlay is registered with the OverlayManager
            if (OverlayManager.instance != null && !OverlayManager.instance.overlayTextures.Contains(this))
            {
                OverlayManager.instance.overlayTextures.Add(this);
            }

            // Get a reference to the VR overlay system
            var overlayLayer = OpenVR.Overlay;
            if (overlayLayer == null) return;

            // Create the overlay if it doesn't already exist
            if (handle == OpenVR.k_ulOverlayHandleInvalid)
            {
                OverlayUtilities.CreateOverlay(key, gameObject.name, ref handle);
            }

            // Get a helper for interacting with the overlay
            var overlayReference = new OverlayUtilities.OverlayHelper(handle);
            if (texture != null && overlayReference.Valid)
            {
                overlayReference.Show();  // Ensure the overlay is shown

                overlayReference.SetOrder(order);  // Set the rendering order
                overlayReference.SetColorWithAlpha(color);  // Set the overlay color and alpha
                overlayReference.SetWidthInMeters(width);  // Set the overlay width

                if (curvature != 0)
                {
                    overlayReference.SetOverlayCurvature(curvature);  // Set the overlay curvature if specified
                }

                overlayReference.SetInputMethod(VROverlayInputMethod.None);  // Disable input for the overlay
                overlayReference.SetMouseScale(1, 1);  // Set mouse scale for the overlay

                // Check if the overlay is facing the HMD
                var isFacing = OverlayUtilities.IsFacingHmd(transform);
                var wasFlipped = isFacingHmd != isFacing;
                isFacingHmd = isFacing;

                // Get the current transform of the overlay
                var offset = new SteamVR_Utils.RigidTransform(transform);

                // Render the texture if needed
                if (renderMode == RenderMode.Update || wasFlipped)
                {
                    RenderTexture();
                }

                // Flip the overlay if it's not facing the HMD
                if (!isFacingHmd && flipToHMD)
                {
                    offset.rot = offset.rot * Quaternion.AngleAxis(180, Vector3.up);
                }

                // Set the transform based on the selected eye
                switch (eye)
                {
                    case Eye.Both:
                        overlayReference.SetTransformAbsolute(ETrackingUniverseOrigin.TrackingUniverseStanding, offset);
                        break;
                    case Eye.Left:
                        overlayReference.SetOverlayTransformProjectionLeft(ETrackingUniverseOrigin.TrackingUniverseStanding, offset);
                        break;
                    case Eye.Right:
                        overlayReference.SetOverlayTransformProjectionRight(ETrackingUniverseOrigin.TrackingUniverseStanding, offset);
                        break;
                }
            }
        }

        /// <summary>
        /// Renders the texture on demand or when enabled.
        /// </summary>
        public void RenderOnDemand()
        {
            if (renderMode == RenderMode.OnDemand || renderMode == RenderMode.OnEnable)
            {
                RenderTexture();
            }
        }

        /// <summary>
        /// Renders the texture for the overlay.
        /// </summary>
        private void RenderTexture()
        {
            // Get a helper for interacting with the overlay
            var overlayReference = new OverlayUtilities.OverlayHelper(handle, false);
            if (texture == null || !overlayReference.Valid) return;

            // Render the camera if one is specified
            if (renderCamera)
            {
                renderCamera.Render();
            }

            // Set the texture and bounds based on whether the overlay is facing the HMD
            if (isFacingHmd)
            {
                if (texture is RenderTexture || lastTexture != texture)
                {
                    lastTexture = texture;
                    overlayReference.SetTexture(texture);
                }

                overlayReference.FillTextureBounds();
            }
            else
            {
                if (backface == null)
                {
                    if (texture is RenderTexture || lastTexture != texture)
                    {
                        lastTexture = texture;
                        overlayReference.SetTexture(texture);
                    }
                    overlayReference.SetTextureBounds(1, 0, 0, 1);
                }
                else
                {
                    if (backface is RenderTexture || lastTexture != backface)
                    {
                        lastTexture = backface;
                        overlayReference.SetTexture(backface);
                    }
                    overlayReference.FillTextureBounds();
                }
            }
        }

        /// <summary>
        /// Initializes the overlay and starts delayed rendering.
        /// </summary>
        private void OnEnable()
        {
            RenderTexture renderTexture = texture as RenderTexture;
            if (renderTexture != null && !renderTexture.IsCreated())
            {
                renderTexture.Create();
            }

            if (!queuedRendering)
            {
                queuedRendering = true;
                StartCoroutine(DelayedRender());
            }
        }

        /// <summary>
        /// Cleans up resources when the overlay is disabled.
        /// </summary>
        private void OnDisable()
        {
            StopAllCoroutines();
            var overlayReference = new OverlayUtilities.OverlayHelper(handle, false);
            if (overlayReference.Valid)
            {
                overlayReference.Destroy();
            }

            handle = OpenVR.k_ulOverlayHandleInvalid;
            lastTexture = null;
            isFacingHmd = true;
        }

        /// <summary>
        /// Delays rendering by one frame.
        /// </summary>
        private IEnumerator DelayedRender()
        {
            yield return null;
            yield return new WaitForEndOfFrame();
            queuedRendering = false;
            RenderOnDemand();
        }

        /// <summary>
        /// Draws a gizmo in the editor to visualize the overlay bounds.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (texture == null) return;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.color = Color.blue;

            float hw = width / 2f;
            float hh = ((float)texture.height / texture.width * width) / 2f;
            Vector3 ul = new Vector3(-hw, -hh);
            Vector3 ur = new Vector3(hw, -hh);
            Vector3 ll = new Vector3(-hw, hh);
            Vector3 lr = new Vector3(hw, hh);

            Gizmos.DrawLine(ul, ur);
            Gizmos.DrawLine(ur, lr);
            Gizmos.DrawLine(lr, ll);
            Gizmos.DrawLine(ll, ul);
        }
    }
}
