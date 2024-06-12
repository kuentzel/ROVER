using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ROVER
{
    /// <summary>
    /// Manages touch interactions with buttons within the ROVER environment.
    /// </summary>
    public class TouchButton : MonoBehaviour
    {
        // Public Fields with Headers
        [Header("Managers")]
        public StudyManager studyManager;
        private StyleManager styleManager;

        [Header("Button Settings")]
        public UnityEvent OnSelect;
        public bool isToggle;
        public bool toggled;
        public Material selectedMaterial;
        public Material defaultMaterial;

        [Header("UI Elements")]
        public Image bgButton;
        public RectTransform highlightImage;
        public RectTransform hoverImage;
        public CanvasRenderer hoverCanvas;
        public Image laserPoint;

        [Header("Touch Settings")]
        public bool innerHoverHighlight = false;
        public bool spawnToast = false;
        public int overrideSpeed = 0;
        public bool timedTouch = false;
        public float timedTouchSpeed = 90;
        public float highlightStartSizePercentage = 20f;
        public bool proximityTouch = false;
        public bool triggerTouch = false;
        public bool adjustCollider = true;
        public bool adjustColliderKeepHeight = true;
        public bool debug = false;

        // Private Fields
        private VirtualPointer mainPointer;
        private Material hoverStartMaterial;
        private int buttonState;
        private bool hovered;
        private bool selected;
        private RectTransform trans;
        private bool proximityHovering = false;
        private VirtualPointer proximityCandidate = null;
        private Vector3 hitPoint;
        private bool timeProximity = false;
        private float startHeight;
        private BoxCollider coll;

        // Initialization
        private void Awake()
        {
            // Find and assign the StudyManager and StyleManager components
            studyManager = studyManager ?? Transform.FindObjectOfType<StudyManager>();
            styleManager = styleManager ?? studyManager.styleManager;

            // Set the touch settings based on the StudyManager
            timedTouch = studyManager.timedTouch;
            proximityTouch = studyManager.proximityTouch;

            // Assign default materials if they are not set
            defaultMaterial = bgButton != null ? bgButton.material : styleManager.foregroundPrimaryMaterial;
            selectedMaterial = selectedMaterial ?? styleManager.buttonSelectedMaterial;

            // Set the button material if toggled
            if (toggled) bgButton.material = selectedMaterial;

            // Initialize the BoxCollider component and store its initial height
            coll = GetComponent<BoxCollider>();
            startHeight = coll != null ? coll.size.y : 0;

            // Keep collider height if the parent object is "OptionContent"
            adjustColliderKeepHeight = transform.parent != null && transform.parent.gameObject.name == "OptionContent";

            // Assign the RectTransform component
            trans = GetComponent<RectTransform>();
        }

        private void Start()
        {
            // Reassign touch settings in Start method to ensure they are up to date
            timedTouch = studyManager.timedTouch;
            proximityTouch = studyManager.proximityTouch;

            // Assign the CanvasRenderer component if it is not already assigned
            if (hoverCanvas == null && hoverImage != null)                
                hoverCanvas = GetComponent<CanvasRenderer>();
        }

        // Update is called once per frame
        private void Update()
        {
            // Update touch settings
            timedTouch = studyManager.timedTouch;
            proximityTouch = studyManager.proximityTouch;
            triggerTouch = studyManager.triggerTouch;

            // Reset proximityHovering flag
            proximityHovering = false;

            // Adjust the collider size based on the parent object's size
            if (adjustCollider)
            {
                coll.size = adjustColliderKeepHeight
                    ? new Vector3(((RectTransform)transform.parent).rect.width, coll.size.y, 10f)
                    : new Vector3(trans.rect.width, trans.rect.height, 10f);
            }
        }

        /// <summary>
        /// Handles proximity hover events.
        /// </summary>
        public void ProximityHover(bool b, VirtualPointer pointer, Vector3 hitPoint)
        {
            if (proximityHovering) return;

            // Assign the proximity candidate and hit point
            proximityCandidate = pointer;
            this.hitPoint = hitPoint;
            proximityHovering = true;
        }

        // LateUpdate is called after all Update functions have been called
        private void LateUpdate()
        {
            HandleProximityTouch();
            HandleHoverProximity();
            HandleLaserPointer();
        }


        /// <summary>
        /// Handles the hover state for buttons when proximity touch is enabled.
        /// </summary>
        private void HandleHoverProximity()
        {
            // Check if proximity touch is enabled and the button is currently hovered
            if (proximityTouch && hovered)
            {
                // Calculate the proximity value based on the distance between the pointer and the button
                float proximity = CalculateProximity();

                // Handle the hover state differently based on whether timed touch is enabled
                if (timedTouch)
                {
                    // Set the hover canvas transparency based on the proximity value
                    hoverCanvas.SetAlpha(Math.Min(proximity + 0.25f, 1));

                    // Determine if the proximity is close enough to trigger a timed touch event
                    timeProximity = proximity >= 0.75f;
                }
                else
                {
                    // Set the hover canvas to fully visible
                    hoverCanvas.SetAlpha(1f);

                    // Check if trigger touch is enabled and the main pointer is triggered
                    if (triggerTouch && mainPointer.Triggered)
                    {
                        // Mark the button as selected and invoke the click action
                        selected = true;
                        mainPointer?.Click(this);
                        Invoke("Select", 0.25f);

                        // Unhover the button after selection
                        UnhoverButton();
                    }
                    else if (!triggerTouch)
                    {
                        // Adjust the size of the highlight image based on the proximity value
                        highlightImage.sizeDelta = new Vector2(proximity * trans.rect.width, proximity * trans.rect.height);

                        // Charge the touch if the proximity is within 70% of the button size
                        if (proximity >= 0.7f)
                        {
                            mainPointer.ChargeTouch(this);
                        }

                        // Commit the touch if the proximity is within 90% of the button size
                        if (proximity >= 0.9f)
                        {
                            mainPointer.CommitTouch(this);
                        }

                        // Mark the button as selected and invoke the click action if the proximity is 100%
                        if (proximity >= 1f)
                        {
                            selected = true;
                            mainPointer?.Click(this);
                            Invoke("Select", 0.25f);

                            // Unhover the button after selection
                            UnhoverButton();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the proximity value based on the distance between the pointer and the button.
        /// </summary>
        /// <returns>A float representing the proximity value.</returns>
        private float CalculateProximity()
        {
            // Determine the proximity value based on the pointer type (raycast or sphere)
            if (!mainPointer.SphereTouch)
            {
                // For raycast pointers, calculate proximity based on the raycast tip position
                return 1.1f - ((Vector3.Distance(hitPoint, mainPointer.raycastTip.position) - mainPointer.proximityOffset) / mainPointer.maxProximityDistance);
            }
            else
            {
                // For sphere pointers, calculate proximity based on the orb origin position
                return 1.1f - ((Vector3.Distance(hitPoint, mainPointer.orbOrigin.position) + mainPointer.sphereProximityOffset) / mainPointer.maxProximityDistance);
            }
        }


        /// <summary>
        /// Handles the proximity touch interactions.
        /// </summary>
        private void HandleProximityTouch()
        {
            if (proximityTouch && proximityHovering && mainPointer == null)
            {
                // Ensure mainPointer is not already assigned
                if (mainPointer != null && proximityCandidate != mainPointer && mainPointer.hoveredButtons.Count > 0) return;

                // Set hover image size based on the innerHoverHighlight flag
                SetHoverImageSize();

                // Assign the proximity candidate as the main pointer
                mainPointer = proximityCandidate;

                // Add this button to the main pointer's hovered buttons list
                if (!mainPointer.hoveredButtons.Contains(this)) mainPointer.hoveredButtons.Add(this);

                // If the main pointer is hovering over this button, handle the hover
                if (mainPointer.hoveredButtons.Contains(this) && (proximityCandidate.hoveredButtons.Count == 1 || mainPointer.SphereTouch))
                {
                    HoverButton();
                }
            }
            else if (proximityTouch && !proximityHovering)
            {
                // Reset hover image size and unhover the button if no longer hovering
                hoverImage.sizeDelta = Vector2.zero;
                selected = false;
                UnhoverButton();
                if (mainPointer != null && mainPointer.hoveredButtons.Contains(this)) mainPointer.hoveredButtons.Remove(this);
                mainPointer = null;
            }
        }

        /// <summary>
        /// Handles the laser pointer behavior when proximity hovering is active.
        /// </summary>
        private void HandleLaserPointer()
        {
            // Check if the laser pointer exists and if proximity hovering is active
            if (laserPoint != null && proximityHovering)
            {
                // Check if either trigger touch or timed touch is enabled, timeProximity is not active,
                // hitPoint is defined, and mainPointer is not null
                if ((triggerTouch || timedTouch) && !timeProximity && hitPoint != null && mainPointer != null)
                {
                    // Enable the laser pointer game object
                    laserPoint.gameObject.SetActive(true);

                    // Set the position of the laser pointer to the hit point of the proximity hover
                    laserPoint.transform.position = new Vector3(hitPoint.x, hitPoint.y, hitPoint.z);

                    // Calculate the scale of the laser pointer based on the distance to the pointer tip,
                    // ensuring it remains within a defined range
                    laserPoint.transform.localScale = Vector3.one * 1.5f * Mathf.Clamp(
                        (1 - Mathf.Clamp(Vector3.Distance(hitPoint, mainPointer.pointerTip.transform.position), 0.1f, 1f)),
                        0.5f, 1f
                    );
                }
                else
                {
                    // Disable the laser pointer game object if the conditions are not met
                    laserPoint.gameObject.SetActive(false);
                }
            }
            else if (laserPoint != null)
            {
                // Ensure the laser pointer is disabled if proximity hovering is not active
                laserPoint.gameObject.SetActive(false);
            }
        }


        /// <summary>
        /// Sets the hover image size based on the innerHoverHighlight flag.
        /// </summary>
        private void SetHoverImageSize()
        {
            if (innerHoverHighlight)
                hoverImage.sizeDelta = new Vector2(trans.rect.width, trans.rect.height);
            else
                hoverImage.sizeDelta = new Vector2(1.16f * trans.rect.width, 1.16f * trans.rect.height);
        }

        /// <summary>
        /// Handles the selection of the button.
        /// </summary>
        private void Select()
        {
            if (!isToggle)
                bgButton.material = defaultMaterial;
            else
                bgButton.material = selectedMaterial;

            selected = true;
            CancelInvoke("Select");
            OnSelect.Invoke();
        }

        // FixedUpdate is called at a fixed interval
        private void FixedUpdate()
        {
            HandleTimedTouch();
        }

        /// <summary>
        /// Handles the timed touch interactions.
        /// </summary>
        private void HandleTimedTouch()
        {
            // Ensure that timedTouch is enabled, proximityTouch is either disabled or timeProximity is true,
            // the highlightImage is not null, the button is hovered, and either debugging is enabled or the mainPointer is not null.
            if (timedTouch && (!proximityTouch || timeProximity) && highlightImage != null && hovered && (debug || mainPointer != null))
            {
                // Adjust touch speed based on overrideSpeed or mainPointer's highlighting speed.
                timedTouchSpeed = overrideSpeed != 0 ? overrideSpeed : mainPointer.highlightingSpeed;

                // Adjust the start size percentage based on mainPointer's setting.
                highlightStartSizePercentage = mainPointer.highlightingStartPercentage;

                // Calculate the new width for the highlight image, ensuring it doesn't exceed the button's width.
                float width = Math.Min(highlightImage.rect.width + ((1f / timedTouchSpeed) * trans.rect.width), trans.rect.width);
                // Ensure the width is at least the starting size percentage.
                width = Math.Max(width, highlightStartSizePercentage * 0.01f * trans.rect.width);

                // Calculate the new height for the highlight image, ensuring it doesn't exceed the button's height.
                float height = Math.Min(highlightImage.rect.height + ((1f / timedTouchSpeed) * trans.rect.height), trans.rect.height);
                // Ensure the height is at least the starting size percentage.
                height = Math.Max(height, highlightStartSizePercentage * 0.01f * trans.rect.height);

                // Set the new size for the highlight image.
                highlightImage.sizeDelta = new Vector2(width, height);

                // Handle hover and touch events based on the highlight size.
                if (!timeProximity && width >= mainPointer.hoveringDelay * trans.rect.width && height >= mainPointer.hoveringDelay * trans.rect.height)
                {
                    // Trigger hover and charge touch events if the highlight image is sufficiently large.
                    mainPointer.Hover(this);
                    mainPointer.ChargeTouch(this);
                }
                else if (timeProximity)
                {
                    // If timeProximity is true, trigger hover and charge touch events regardless of size.
                    mainPointer.Hover(this);
                    mainPointer.ChargeTouch(this);
                }

                // If the highlight image size reaches 99% of the button size, select the button.
                if (width >= 0.99f * trans.rect.width && height >= 0.99f * trans.rect.height)
                {
                    selected = true;
                    mainPointer?.Click(this);
                    Invoke("Select", 0.25f);
                    UnhoverButton();
                }
                // If the highlight image size reaches 90% of the button size, commit the touch.
                else if (width >= 0.9f * trans.rect.width && height >= 0.9f * trans.rect.height)
                {
                    mainPointer.CommitTouch(this);
                }
                // If the highlight image size reaches 50% of the button size, charge the touch.
                else if (width >= 0.5f * trans.rect.width && height >= 0.9f * trans.rect.height)
                {
                    mainPointer.ChargeTouch(this);
                }
            }
            // If the button is not hovered, reduce the highlight image size gradually.
            else if (highlightImage != null && !hovered && highlightImage.rect.width > 0 && highlightImage.rect.height > 0)
            {
                // Determine the reduction factor based on whether proximityTouch is enabled.
                float factor = proximityTouch ? 0.25f : 0.5f;

                // Reduce the highlight image width gradually.
                float width = Math.Max(highlightImage.rect.width - ((1f / (factor * timedTouchSpeed)) * trans.rect.width), 0);
                // Reduce the highlight image height gradually.
                float height = Math.Max(highlightImage.rect.height - ((1f / (factor * timedTouchSpeed)) * trans.rect.height), 0);

                // Set the new size for the highlight image.
                highlightImage.sizeDelta = new Vector2(width, height);
            }
        }


        // Handles trigger enter events for proximity touch
        private void OnTriggerEnter(Collider other)
        {
            if (proximityTouch) return;

            if (other.gameObject.tag == "Pointer" && mainPointer == null)
            {
                VirtualPointer pointer = other.gameObject.GetComponent<VirtualPointer>();

                if (pointer != null)
                {
                    mainPointer = pointer;
                    if (!pointer.hoveredButtons.Contains(this))
                        pointer.hoveredButtons.Add(this);
                    if (pointer.hoveredButtons.Contains(this) && pointer.hoveredButtons.Count == 1)
                    {
                        HoverButton();
                    }
                }
            }
        }

        // Handles trigger exit events for proximity touch
        private void OnTriggerExit(Collider other)
        {
            if (proximityTouch) return;

            if (other.gameObject.tag == "Pointer")
            {
                VirtualPointer pointer = other.gameObject.GetComponent<VirtualPointer>();

                if (pointer != null && pointer == mainPointer)
                {
                    selected = false;
                    UnhoverButton();
                    if (mainPointer != null && mainPointer.hoveredButtons.Contains(this))
                        mainPointer.hoveredButtons.Remove(this);
                    mainPointer = null;
                }
            }
        }

        // Handles the behavior when the button is disabled
        private void OnDisable()
        {
            if (highlightImage != null)
                highlightImage.sizeDelta = new Vector2(0, 0);
            if (hoverImage != null)
                hoverImage.sizeDelta = new Vector2(0, 0);
            selected = false;
            UnhoverButton();
            if (mainPointer != null && mainPointer.hoveredButtons.Contains(this))
                mainPointer.hoveredButtons.Remove(this);
            mainPointer = null;
        }

        /// <summary>
        /// Handles the hover state of the button.
        /// </summary>
        private void HoverButton()
        {
            if (!hovered && buttonState != 4 && mainPointer != null)
            {
                hovered = true;
                buttonState = 1;
                hoverStartMaterial = bgButton.material;
                if (!timedTouch && !proximityTouch)
                {
                    InvokeRepeating("UpdateButtonState", mainPointer.hoverDelay, mainPointer.hoverDelay);
                    hoverStartMaterial = bgButton.material;
                    bgButton.material = styleManager.buttonHoverMaterial;
                }
                mainPointer?.PlayShortClick(this);
            }
        }

        /// <summary>
        /// Handles the unhover state of the button.
        /// </summary>
        private void UnhoverButton()
        {
            // If the button is not currently hovered, exit the method early.
            if (!hovered) return;

            // If both timedTouch and proximityTouch are enabled
            if (timedTouch && proximityTouch)
            {
                // Reset the hover state
                ResetHoverState();

                // Handle the toggle state if the button is a toggle button
                if (isToggle)
                {
                    // If the button is selected, toggle its state and update the background material
                    if (selected)
                    {
                        toggled = !toggled;
                        highlightImage.sizeDelta = new Vector2(0, 0);
                        bgButton.material = toggled ? selectedMaterial : defaultMaterial;
                    }
                }
                else
                {
                    // If the button is not a toggle button and is selected, set its material to the pressed state
                    if (selected)
                    {
                        highlightImage.sizeDelta = new Vector2(0, 0);
                        bgButton.material = styleManager.buttonPressedMaterial;
                    }
                }
            }
            // If timedTouch is enabled but proximityTouch is not
            else if (timedTouch && !proximityTouch)
            {
                // Reset the hover state
                ResetHoverState();

                // Handle the toggle state if the button is a toggle button
                if (isToggle)
                {
                    // If the button is selected, toggle its state and update the background material
                    if (selected)
                    {
                        toggled = !toggled;
                        bgButton.material = toggled ? selectedMaterial : defaultMaterial;
                    }
                }
                else
                {
                    // If the button is not a toggle button and is selected, set its material to the pressed state
                    if (selected)
                    {
                        highlightImage.sizeDelta = new Vector2(0, 0);
                        bgButton.material = styleManager.buttonPressedMaterial;
                    }
                }
            }
            // If proximityTouch is enabled but timedTouch is not
            else if (proximityTouch && !timedTouch)
            {
                // Reset the hover image size
                hoverImage.sizeDelta = new Vector2(0, 0);

                // Handle the toggle state if the button is a toggle button
                if (isToggle)
                {
                    // If the button is selected, toggle its state and update the background material
                    if (selected)
                    {
                        toggled = !toggled;
                        highlightImage.sizeDelta = new Vector2(0, 0);
                        bgButton.material = toggled ? selectedMaterial : defaultMaterial;
                    }
                }
                else
                {
                    // If the button is not a toggle button and is selected, set its material to the pressed state
                    if (selected)
                    {
                        highlightImage.sizeDelta = new Vector2(0, 0);
                        bgButton.material = styleManager.buttonPressedMaterial;
                    }
                }
            }
            // If neither timedTouch nor proximityTouch are enabled
            else if (!timedTouch && !proximityTouch)
            {
                // Cancel any ongoing button state updates
                CancelInvoke("UpdateButtonState");

                // Handle the toggle state if the button is a toggle button
                if (isToggle)
                {
                    // If the button is selected, toggle its state and update the background material
                    if (selected)
                    {
                        toggled = !toggled;
                        bgButton.material = toggled ? selectedMaterial : defaultMaterial;
                    }
                    else
                    {
                        // If the button is not selected, revert to the initial hover material
                        bgButton.material = hoverStartMaterial;
                    }
                }
                else
                {
                    // If the button is not a toggle button
                    if (selected)
                    {
                        // If the button is selected, revert to the default material
                        bgButton.material = defaultMaterial;
                    }
                    else
                    {
                        // If the button is not selected, revert to the initial hover material
                        bgButton.material = hoverStartMaterial;
                    }
                }
            }

            // Reset the button state and hover status
            buttonState = 0;
            hovered = false;
            selected = false;

            // Remove the button from the main pointer's hovered buttons list if it is in there
            if (mainPointer != null && mainPointer.hoveredButtons.Contains(this))
                mainPointer.hoveredButtons.Remove(this);

            // Clear the reference to the main pointer
            mainPointer = null;
        }


        /// <summary>
        ///Resets the hover state of the button.
        ///</summary>
        private void ResetHoverState()
        {
            mainPointer?.StopHover();
            hoverImage.sizeDelta = new Vector2(0, 0);
        }

        /// <summary>
        ///Updates the button state during hover.
        ///</summary>
        private void UpdateButtonState()
        {
            if (buttonState == 1)
            {
                bgButton.material = styleManager.buttonPressedMaterial;
                buttonState++;
            }
            else if (buttonState == 2)
            {
                bgButton.material = selectedMaterial;
                buttonState++;
                CancelInvoke("UpdateButtonState");
                Invoke("UpdateButtonState", mainPointer.confirmDelay);
            }
            else if (buttonState == 3)
            {
                buttonState++;
                selected = true;
                OnSelect.Invoke();
                mainPointer?.Click(this);
                CancelInvoke("UpdateButtonState");
            }
        }
    }
}
