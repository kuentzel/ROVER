using Newtonsoft.Json;
using ROVER.Overlay;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using Valve.VR;

namespace ROVER
{
    /// <summary>
    /// Represents the configuration of a controller with position and rotation coordinates.
    /// </summary>
    class ControllerConfiguration
    {
        [JsonProperty]
        public float x;
        [JsonProperty]
        public float y;
        [JsonProperty]
        public float z;
        [JsonProperty]
        public float rotX;
        [JsonProperty]
        public float rotY;
        [JsonProperty]
        public float rotZ;

        /// <summary>
        /// Initializes a new instance of the ControllerConfiguration class.
        /// </summary>
        /// <param name="x">X position.</param>
        /// <param name="y">Y position.</param>
        /// <param name="z">Z position.</param>
        /// <param name="rotX">Rotation around X axis.</param>
        /// <param name="rotY">Rotation around Y axis.</param>
        /// <param name="rotZ">Rotation around Z axis.</param>
        public ControllerConfiguration(float x, float y, float z, float rotX, float rotY, float rotZ)
        {
            this.x = x;
            this.y = y;
            this.z = z;
            this.rotX = rotX;
            this.rotY = rotY;
            this.rotZ = rotZ;
        }
    }

    public enum TouchInteractionMode
    {
        Trigger,
        ColliderTime,
        RaycastTime,
        Proximity,
        ProximitySphere
    }

    /// <summary>
    /// Provides data for the touch button events.
    /// </summary>
    public class TouchButtonEventArgs : EventArgs
    {
        public TouchButtonEventArgs(TouchInteractionMode mode, TouchButton button, DateTime timestamp, SteamVR_Input_Sources handType, Vector3 buttonWorldPos, Vector3 buttonWorldRot, Vector3 pointerBaseWorldPos, Vector3 pointerBaseWorldRot)
        {
            this.mode = mode;
            this.button = button;
            Timestamp = timestamp;
            this.handType = handType;
            this.buttonWorldPos = buttonWorldPos;
            this.buttonWorldRot = buttonWorldRot;
            this.pointerBaseWorldPos = pointerBaseWorldPos;
            this.pointerBaseWorldRot = pointerBaseWorldRot;
        }

        public TouchInteractionMode mode;

        public TouchButton button { get; set; }
        public DateTime Timestamp { get; set; }

        public SteamVR_Input_Sources handType;

        public Vector3 buttonWorldPos;
        public Vector3 buttonWorldRot;

        public Vector3 pointerBaseWorldPos;
        public Vector3 pointerBaseWorldRot;
    }

    public delegate void TouchButtonEventHandler(object sender, TouchButtonEventArgs e);

    public class VirtualPointer : MonoBehaviour
    {
        #region Unity Inspector Fields

        [Header("Overlay and Camera Settings")]
        public OverlayManager overlayManager;
        public Camera headset;

        [Header("Pointer Settings")]
        public Transform pointerVisualizationContainer;
        public Transform orbOrigin;
        public Transform stylusContainer;
        public Transform pointerTip;
        public Transform raycastTip;
        [SerializeField] protected float defaultLength = 0.15f;

        [Header("VR Actions")]
        public SteamVR_Input_Sources handType;
        public SteamVR_Action_Vibration hapticAction;
        public SteamVR_Action_Boolean inputAction;

        [Header("Line Renderer Settings")]
        public LineRenderer lineRenderer = null;
        public LineRenderer outlineRenderer = null;

        [Header("Hover Settings")]
        public float hoverDelay = 0.25f;
        public float confirmDelay = 0.15f;
        public float highlightingSpeed = 90f;
        public float highlightingStartPercentage = 20f;
        public ushort shortPulse = 400;
        public ushort longPulse = 1200;
        public ushort hoverPulse = 800;
        public float hoveringSuspension = 0.6f;
        public float hoveringDelay = 0.5f;

        [Header("Renderer Settings")]
        [SerializeField] private OverlayTextureRenderer pointerRenderer1;
        [SerializeField] private OverlayTextureRenderer pointerRenderer2;

        [Header("Interaction Distances")]
        public float doubleClickDelay = 0.5f;
        public float maxProximityDistance = 0.2f;
        public float raycastDistance = 0.6f;
        public float minProximityDistance = 0.05f;
        public float proximityOffset;
        public float sphereProximityOffset = 0f;
        public float sphereSafeguardOffset = 0.025f;

        [Header("Audio Settings")]
        public AudioClip shortClick;
        public AudioClip longClick;
        public AudioClip hoverSound;
        public AudioSource audioSource;
        public AudioSource longClickaudioSource;

        [Header("Debug Settings")]
        public bool debug = false;

        [Header("Interaction Settings")]
        public bool colorizeOnHover;
        public Color hoverColor;
        public Color defaultColor;

        #endregion

        public VirtualPointer otherPointer;
        public List<TouchButton> hoveredButtons = new List<TouchButton>();
        public Transform stationBase;
        public StudyManager studyManager;
        public GameObject controlPanel;

        private bool timedTouch = false;
        private bool proximityTouch = false;
        private bool sphereTouch = false;
        private bool triggerTouch = false;
        public bool overrideSphere = false;

        private TouchButton prevHit;
        private bool aiming = false;
        private bool committed = false;
        private bool charging = false;

        public Collider coll;
        private bool hasClicked = false;
        private TouchButton lastClicked;
        private float clickDistance;
        private Vector3 clickForward;
        private Vector3 clickOrigin;
        private Vector3 clickHitPoint;
        private bool triggered;

        private bool playingHover = false;
        private TouchInteractionMode currentMode;

        public bool Triggered { get => triggered; set => triggered = value; }
        public bool SphereTouch { get => sphereTouch; set => sphereTouch = value; }

        public event TouchButtonEventHandler TouchButtonAimed;
        public event TouchButtonEventHandler TouchButtonLost;
        public event TouchButtonEventHandler TouchButtonHovered;
        public event TouchButtonEventHandler TouchButtonCharging;
        public event TouchButtonEventHandler TouchButtonCommitted;
        public event TouchButtonEventHandler TouchButtonSelected;
        public event TouchButtonEventHandler TouchButtonMissed;

        /// <summary>
        /// Unity Awake method called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            // Ensure the line renderer is assigned
            if (lineRenderer == null)
                lineRenderer = GetComponent<LineRenderer>();
        }

        /// <summary>
        /// Unity Start method called before the first frame update.
        /// </summary>
        private void Start()
        {
            //If manually initializing SteamVR_Input, SteamVR_Action_Pose.SetTrackingUniverseOrigin(SteamVR_Settings.instance.trackingSpace) -> OpenVR.Compositor.SetTrackingSpace(newOrigin) will cause the Compositor projection to break for OpenXR standalone apps
            //SteamVR_Input.Initialize();
            //OpenVR.Compositor.SetTrackingSpace(...) is only called in the SteamVR Plugin in SteamVR_Action_Pose.SetTrackingUniverseOrigin(...) and in SteamVR_Render.RenderLoop(). When setting up actions manually, you still need to set their Tracking Universe Origin, but you can do it without setting Compositor Tracking Space. When using SteamVR_Behaviour Components, it will set up a SteamVR_Render component automatically, which will set the Compositor Tracking Space each render loop iteration. For our project, we added a check "if (SteamVR.isStandalone)" before the two occurences of OpenVR.Compositor.SetTrackingSpace(...) in the SteamVR Plugin, which works for our purposes.


            // Initialize VR actions
            inputAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("interactui");
            hapticAction = SteamVR_Input.GetAction<SteamVR_Action_Vibration>("haptic");

            // Retrieve touch settings from the study manager
            timedTouch = studyManager.timedTouch;
            proximityTouch = studyManager.proximityTouch;
            sphereTouch = studyManager.sphereTouch;
            triggerTouch = studyManager.triggerTouch;

            // Disable collider if proximity touch is enabled without timed touch
            if (proximityTouch && !timedTouch)
                coll.enabled = false;
        }

        /// <summary>
        /// Loads the controller configuration from a JSON file.
        /// </summary>
        /// <param name="importPath">The path to the JSON file.</param>
        public void LoadControllerConfiguration(string importPath)
        {
            // Construct the full path to the configuration file
            string path = Application.streamingAssetsPath + "/Configuration/" + importPath + ".json";
            if (!File.Exists(path))
            {
                Debug.Log("File does not exist.");
                return;
            }

            // Read the JSON file
            StreamReader reader = new StreamReader(path);
            String jsonString = reader.ReadToEnd().Trim();
            reader.Close();

            // Deserialize the JSON string to a ControllerConfiguration object
            ControllerConfiguration config = JsonUtility.FromJson<ControllerConfiguration>(jsonString);

            if (debug)
                Debug.Log("ControllerConfiguration = " + config.x + "," + config.y + "," + config.z + "," + config.rotX + "," + config.rotY + "," + config.rotZ);

            // Set the local position and rotation based on the configuration
            transform.parent.localPosition = new Vector3(config.x, config.y, config.z);
            transform.parent.localRotation = Quaternion.Euler(config.rotX, config.rotY, config.rotZ);
        }

        /// <summary>
        /// Toggles the override sphere flag.
        /// </summary>
        public void ToggleOverrideSphere()
        {
            overrideSphere = !overrideSphere;
        }

        private TouchButton missCandidate;
        private float timeout = 0.3f;

        private void ResetMissCandidate()
        {
            missCandidate = null;
        }


        /// <summary>
        /// Unity Update method called once per frame.
        /// </summary>
        private void Update()
        {
            // Update the length of the pointer
            UpdateLength();

            if(missCandidate != null && inputAction.GetStateDown(handType))
            {
                TouchButtonMissed?.Invoke(this, new TouchButtonEventArgs(currentMode, missCandidate, DateTime.UtcNow, handType, missCandidate.transform.position, missCandidate.transform.rotation.eulerAngles, transform.position, transform.rotation.eulerAngles));
                missCandidate = null;
            }

            // Determine if the control panel should be visible based on the headset position and orientation
            Vector3 lookDirection = new Vector3(headset.transform.forward.x, 0, headset.transform.forward.z);
            if (controlPanel != null && headset != null && controlPanel.activeInHierarchy && (isVisibleToHeadset(headset, controlPanel) || Vector3.Angle(lookDirection, stationBase.transform.forward) <= 60))
            {
                pointerVisualizationContainer.gameObject.SetActive(true);
                overlayManager.FilterInput();
            }
            else
            {
                overlayManager.ReleaseInput();
                pointerVisualizationContainer.gameObject.SetActive(false);
            }

            // Hide the pointer if configured to do so
            if (studyManager.hidePointerInGame)
                pointerVisualizationContainer.gameObject.SetActive(false);

            // Debugging: Empty the list of hovered buttons when key 0 is pressed
            if (debug && Input.GetKeyDown(KeyCode.Alpha0))
            {
                EmptyList();
            }

            // Update touch interaction settings based on study manager configurations
            timedTouch = studyManager.timedTouch;
            proximityTouch = studyManager.proximityTouch;
            if (overrideSphere)
                sphereTouch = true;
            else
                sphereTouch = studyManager.sphereTouch;
            triggerTouch = studyManager.triggerTouch;

            // Determine the current touch interaction mode
            if (proximityTouch)
            {
                if (timedTouch)
                    currentMode = TouchInteractionMode.RaycastTime;
                else if (sphereTouch)
                    currentMode = TouchInteractionMode.ProximitySphere;
                else if (triggerTouch)
                    currentMode = TouchInteractionMode.Trigger;
                else
                    currentMode = TouchInteractionMode.Proximity;
            }
            else
                currentMode = TouchInteractionMode.ColliderTime;

            // Toggle visibility of orb origin and stylus container based on sphere touch setting
            orbOrigin.gameObject.SetActive(sphereTouch);
            stylusContainer.gameObject.SetActive(!sphereTouch);

            // Enable or disable the collider based on the proximity touch settings
            if (proximityTouch && otherPointer.hoveredButtons.Count > 0)
            {
                coll.enabled = false;
            }
            else if (proximityTouch)
            {
                coll.enabled = true;
            }
            if (sphereTouch)
                coll.enabled = false;
            else
                coll.enabled = true;
        }

        public bool sphereCast;

        /// <summary>
        /// Unity LateUpdate method called after all Update functions have been called.
        /// </summary>
        private void LateUpdate()
        {
            // Update the color of the line renderer based on hover state
            if (colorizeOnHover)
            {
                // Set the start and end color of the line renderer to the default color
                lineRenderer.startColor = defaultColor;
                lineRenderer.endColor = defaultColor;
            }

            // Exit the method early if proximity touch is not enabled
            if (!proximityTouch)
                return;

            // Initialize variables for touch button detection
            TouchButton tb = null; // The touch button that might be hit
            RaycastHit hit; // Information about the raycast hit
            Vector3 hitPoint = Vector3.zero; // The point where the ray hits
            Ray ray; // The ray used for raycasting
            bool somethingHit = false; // Flag indicating if something was hit

            // Determine if a touch button is hit using sphere casting or raycasting
            if (sphereTouch || sphereCast)
            {
                Collider[] colliders = null; // Array to hold colliders detected by the sphere cast
                Transform castOrigin = null; // The origin point of the cast

                // Set the origin of the cast based on the type of touch
                if (sphereTouch)
                    castOrigin = orbOrigin;
                else if (sphereCast)
                    castOrigin = pointerTip;

                // Perform a sphere cast to detect colliders within a certain radius
                if (sphereTouch)
                    colliders = Physics.OverlapSphere(castOrigin.position, 0.2f);
                else if (sphereCast)
                    colliders = Physics.OverlapSphere(castOrigin.position, 1f);

                // If colliders were detected
                if (colliders != null && colliders.Length > 0)
                {
                    // Create a list of transforms from the detected colliders
                    List<Transform> transforms = new List<Transform>();
                    foreach (Collider col in colliders)
                    {
                        // Only add colliders that have a TouchButton component
                        if (col.GetComponent<TouchButton>() != null)
                            transforms.Add(col.transform);
                    }

                    // If there are any valid transforms
                    if (transforms.Count > 0)
                    {
                        // Order the transforms by distance to the cast origin
                        transforms = transforms.OrderBy(x => (castOrigin.position - x.position).sqrMagnitude).ToList();
                        // Perform a linecast to check if there is an unobstructed path to the closest transform
                        somethingHit = Physics.Linecast(castOrigin.position, transforms[0].position, out hit);

                        // Check if the hit point is within a certain distance from the headset and not above the cast origin
                        if (Vector3.Distance(hit.point, headset.transform.position) < Vector3.Distance(castOrigin.position, headset.transform.position) - sphereSafeguardOffset || hit.point.y > castOrigin.position.y + sphereSafeguardOffset)
                            somethingHit = false;

                        // If the linecast hit something, set the touch button and hit point
                        if (somethingHit)
                        {
                            tb = transforms[0].GetComponent<TouchButton>();
                            hitPoint = hit.point;
                        }
                    }
                }
            }
            else
            {
                // Perform a raycast to detect colliders along the ray
                ray = new Ray(raycastTip.position, raycastTip.forward);
                somethingHit = Physics.Raycast(ray, out hit, raycastDistance);
                // If the raycast hit something, set the touch button and hit point
                if (somethingHit)
                {
                    tb = hit.collider.GetComponent<TouchButton>();
                    hitPoint = hit.point;
                }
            }

            // Handle interactions with the detected touch button
            if (tb != null)
            {
                // Debugging: Log visibility check result
                if (debug)
                    Debug.Log(isVisibleToHeadset(headset, gameObject));
                // If the touch button being aimed at has changed
                if (aiming && tb != prevHit)
                {
                    aiming = false;
                    // Invoke the TouchButtonLost event
                    if (prevHit != null)
                        TouchButtonLost?.Invoke(this, new TouchButtonEventArgs(currentMode, prevHit, DateTime.UtcNow, handType, prevHit.transform.position, prevHit.transform.rotation.eulerAngles, transform.position, transform.rotation.eulerAngles));
                    prevHit = null;
                }
                // If the touch button is still valid and either trigger touch is enabled or the button allows proximity touch
                if (tb != null && (triggerTouch || (tb.proximityTouch && (isVisibleToHeadset(headset, gameObject) || charging))))
                {
                    // If the touch button being aimed at has changed
                    if (prevHit != tb)
                    {
                        committed = false;
                        charging = false;
                        // Invoke the TouchButtonAimed event
                        TouchButtonAimed?.Invoke(this, new TouchButtonEventArgs(currentMode, tb, DateTime.UtcNow, handType, tb.transform.position, tb.transform.rotation.eulerAngles, transform.position, transform.rotation.eulerAngles));
                        missCandidate = null;
                    }

                    prevHit = tb;
                    aiming = true;
                    float distance = 0;
                    // Calculate the distance to the hit point based on the type of touch
                    if (sphereTouch)
                        distance = Vector3.Distance(orbOrigin.position, hitPoint);
                    else
                        distance = Vector3.Distance(raycastTip.position, hitPoint);

                    // Debugging: Log the distance to the hit point
                    if (debug)
                        Debug.Log("Ray hit tb at " + distance);

                    // Handle resetting the click state based on various conditions
                    if (hasClicked)
                    {
                        if (triggerTouch && !inputAction.GetState(handType))
                            ResetClick();
                        else if (!timedTouch && distance >= minProximityDistance)
                            ResetClick();
                        else if (timedTouch && (distance >= clickDistance + minProximityDistance || (tb != lastClicked && Vector3.Distance(hitPoint, clickHitPoint) >= 0.05)))
                            ResetClick();
                    }

                    // Debugging: Log the number of hovered buttons on the other pointer
                    if (debug)
                        Debug.Log("OtherHand " + handType + " hovers " + otherPointer.hoveredButtons.Count);

                    // Handle hovering over the touch button if not already clicked and the other pointer is not hovering over any buttons
                    if (!hasClicked && otherPointer.hoveredButtons.Count < 1)
                    {
                        // Check distance conditions based on the type of touch
                        if (sphereTouch && !((distance >= minProximityDistance || hoveredButtons.Contains(tb)) && distance <= maxProximityDistance))
                            return;
                        if (!sphereTouch && !triggerTouch && !((distance >= minProximityDistance + proximityOffset || hoveredButtons.Contains(tb)) && distance <= maxProximityDistance))
                            return;

                        // Invoke the TouchButtonHovered event if the touch button is not already in the list of hovered buttons
                        if (!hoveredButtons.Contains(tb))
                            TouchButtonHovered?.Invoke(this, new TouchButtonEventArgs(currentMode, tb, DateTime.UtcNow, handType, tb.transform.position, tb.transform.rotation.eulerAngles, transform.position, transform.rotation.eulerAngles));
                        
                        // Indicate the button is being hovered
                        tb.ProximityHover(true, this, hitPoint);

                        // Add the touch button to the list of hovered buttons if it's not already there
                        if (!hoveredButtons.Contains(tb))
                            hoveredButtons.Add(tb);

                        // Update the color of the line renderer to the hover color if colorizeOnHover is enabled
                        if (colorizeOnHover)
                        {
                            lineRenderer.startColor = hoverColor;
                            lineRenderer.endColor = hoverColor;
                        }

                        // Check if the input action (e.g., trigger press) is initiated
                        if (inputAction.GetStateDown(handType))
                        {
                            triggered = true;
                            // Commit the touch action on the touch button
                            CommitTouch(tb);
                        }
                        else
                            triggered = false;

                        // Store the current distance, forward direction, origin, and hit point for later comparison
                        clickDistance = distance;
                        clickForward = gameObject.transform.forward;
                        clickOrigin = gameObject.transform.position;
                        clickHitPoint = hitPoint;
                    }
                }
            }
            else if (aiming && (tb == null || tb != prevHit))
            {
                // If no touch button is hit or the touch button has changed, stop aiming
                aiming = false;
                // Invoke the TouchButtonLost event
                if (prevHit != null)
                    TouchButtonLost?.Invoke(this, new TouchButtonEventArgs(currentMode, prevHit, DateTime.UtcNow, handType, prevHit.transform.position, prevHit.transform.rotation.eulerAngles, transform.position, transform.rotation.eulerAngles));
                missCandidate = prevHit;
                Invoke(nameof(ResetMissCandidate), timeout);
                prevHit = null;
            }

            // Reset click state if certain conditions are met
            if (timedTouch && hasClicked && Vector3.Angle(clickForward, transform.forward) >= 10)
                ResetClick();

            // Render the pointer on demand if colorizeOnHover is enabled
            if (colorizeOnHover)
            {
                pointerRenderer1?.RenderOnDemand();
                pointerRenderer2?.RenderOnDemand();
            }
        }

        /// <summary>
        /// Checks if the given game object is visible to the headset.
        /// </summary>
        /// <param name="c">The camera representing the headset.</param>
        /// <param name="go">The game object to check visibility for.</param>
        /// <returns>True if the object is visible, false otherwise.</returns>
        bool isVisibleToHeadset(Camera c, GameObject go)
        {
            if (!go.activeInHierarchy)
                return false;

            var planes = GeometryUtility.CalculateFrustumPlanes(c);
            var point = go.transform.position;

            foreach (var plane in planes)
            {
                if (plane.GetDistanceToPoint(point) < -0.4)
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Commits the touch action on the given touch button.
        /// </summary>
        /// <param name="tb">The touch button being interacted with.</param>
        public void CommitTouch(TouchButton tb)
        {
            if (!committed)
            {
                TouchButtonCommitted?.Invoke(this, new TouchButtonEventArgs(currentMode, tb, DateTime.UtcNow, handType, tb.transform.position, tb.transform.rotation.eulerAngles, transform.position, transform.rotation.eulerAngles));
                committed = true;
            }
        }

        /// <summary>
        /// Charges the touch action on the given touch button.
        /// </summary>
        /// <param name="tb">The touch button being interacted with.</param>
        public void ChargeTouch(TouchButton tb)
        {
            if (!charging)
            {
                TouchButtonCharging?.Invoke(this, new TouchButtonEventArgs(currentMode, tb, DateTime.UtcNow, handType, tb.transform.position, tb.transform.rotation.eulerAngles, transform.position, transform.rotation.eulerAngles));
                charging = true;
            }
        }

        /// <summary>
        /// Clicks the given touch button.
        /// </summary>
        /// <param name="tb">The touch button being interacted with.</param>
        public void Click(TouchButton tb)
        {
            if (sphereTouch)
                triggered = false;

            if (!hasClicked)
            {
                lastClicked = tb;
                PlayLongClick();
                TouchButtonSelected?.Invoke(this, new TouchButtonEventArgs(currentMode, tb, DateTime.UtcNow, handType, tb.transform.position, tb.transform.rotation.eulerAngles, transform.position, transform.rotation.eulerAngles));

                EmptyList();
                if (!proximityTouch)
                    coll.enabled = false;
            }

            if (!proximityTouch || triggerTouch)
                Invoke("ResetClick", doubleClickDelay);
        }

        /// <summary>
        /// Resets the click state.
        /// </summary>
        void ResetClick()
        {
            hasClicked = false;
            lastClicked = null;
            if (!proximityTouch)
                coll.enabled = true;

            if (debug)
                Debug.Log("reset");
        }

        /// <summary>
        /// Plays the long click sound.
        /// </summary>
        public void PlayLongClick()
        {
            if (hasClicked || longClick == null || longClickaudioSource == null)
                return;

            hasClicked = true;

            if (longClickaudioSource.isPlaying)
                longClickaudioSource.Stop();

            CancelInvoke("SuspendHover");
            longClickaudioSource.clip = longClick;
            longClickaudioSource.Play();
            TriggerHapticPulse(longPulse);
        }

        /// <summary>
        /// Plays the hover sound.
        /// </summary>
        /// <param name="tb">The touch button being interacted with.</param>
        public void Hover(TouchButton tb)
        {
            if (hasClicked || hoverSound == null || audioSource == null || playingHover)
                return;

            playingHover = true;

            if (audioSource.isPlaying)
                audioSource.Stop();

            audioSource.clip = hoverSound;
            audioSource.Play();
            TriggerHapticPulse(hoverPulse);
        }

        /// <summary>
        /// Suspends the hover state.
        /// </summary>
        void SuspendHover()
        {
            playingHover = false;
        }

        /// <summary>
        /// Stops the hover sound.
        /// </summary>
        public void StopHover()
        {
            if (hoverSound == null || audioSource == null)
                return;

            playingHover = false;

            if (!hasClicked && audioSource.isPlaying)
                audioSource.Stop();
        }

        /// <summary>
        /// Plays the short click sound.
        /// </summary>
        /// <param name="tb">The touch button being interacted with.</param>
        public void PlayShortClick(TouchButton tb)
        {
            if (hasClicked || shortClick == null || audioSource == null)
                return;

            if (audioSource.isPlaying)
                audioSource.Stop();

            CancelInvoke("SuspendHover");
            audioSource.clip = shortClick;
            audioSource.Play();
            TriggerHapticPulse(shortPulse);
        }

        /// <summary>
        /// Triggers a haptic pulse.
        /// </summary>
        /// <param name="microSecondsDuration">Duration of the pulse in microseconds.</param>
        public void TriggerHapticPulse(ushort microSecondsDuration)
        {
            float seconds = (float)microSecondsDuration / 1000000f;
            hapticAction.Execute(0, seconds, 1f / seconds, 1, handType);
        }

        /// <summary>
        /// Triggers a haptic pulse with specified parameters.
        /// </summary>
        /// <param name="duration">Duration of the pulse.</param>
        /// <param name="frequency">Frequency of the pulse.</param>
        /// <param name="amplitude">Amplitude of the pulse.</param>
        public void TriggerHapticPulse(float duration, float frequency, float amplitude)
        {
            hapticAction.Execute(0, duration, frequency, amplitude, handType);
        }

        /// <summary>
        /// Empties the list of hovered buttons.
        /// </summary>
        public void EmptyList()
        {
            hoveredButtons.Clear();
        }

        /// <summary>
        /// Updates the length of the pointer.
        /// </summary>
        private void UpdateLength()
        {
            lineRenderer.SetPosition(0, transform.localPosition);
            lineRenderer.SetPosition(1, GetEnd());

            if (outlineRenderer == null)
                return;

            outlineRenderer.SetPosition(0, transform.localPosition);
            outlineRenderer.SetPosition(1, GetEnd());
        }

        /// <summary>
        /// Gets the end position of the pointer.
        /// </summary>
        /// <returns>The end position of the pointer.</returns>
        protected virtual Vector3 GetEnd()
        {
            return transform.localPosition + (Vector3.forward * defaultLength);
        }
    }
}
