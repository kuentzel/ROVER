using ROVER.Overlay;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ROVER
{
    /// <summary>
    /// Manages button toast notifications, displaying temporary feedback when buttons are selected.
    /// </summary>
    public class ButtonToastManager : MonoBehaviour
    {
        [Header("Pointers")]
        public VirtualPointer leftPointer;
        public VirtualPointer rightPointer;

        [Header("Overlay")]
        public OverlayTextureRenderer overlayTextureRenderer;

        [Header("Managers")]
        public StudyManager studyManager;

        [Header("Materials")]
        public Material mat;

        [Header("Toast Settings")]
        public CanvasRenderer toastCanvas;

        private Material oMat;
        private List<GameObject> toasts = new List<GameObject>();
        private TouchButton currentToastedButton;
        private float alpha;
        private bool init;

        /// <summary>
        /// Initializes the component if not already initialized and updates the toast position and alpha.
        /// </summary>
        void FixedUpdate()
        {
            if (!init)
                Init();

            if (init && currentToastedButton != null)
            {
                transform.position = new Vector3(transform.position.x, transform.position.y + 0.0005f, transform.position.z);
                alpha -= 0.02f;
            }
        }

        /// <summary>
        /// Initializes the button toast manager by setting up event listeners.
        /// </summary>
        void Init()
        {
            if (leftPointer != null && rightPointer != null)
            {
                init = true;
                leftPointer.TouchButtonSelected += TouchButtonSelected;
                rightPointer.TouchButtonSelected += TouchButtonSelected;
            }
        }

        /// <summary>
        /// Handles the event when a touch button is selected, displaying a toast notification.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The touch button event arguments.</param>
        private void TouchButtonSelected(object sender, TouchButtonEventArgs e)
        {
            TextMeshProUGUI obut = e.button.GetComponentInChildren<TextMeshProUGUI>();
            Collider c = e.button.GetComponent<Collider>();
            TouchButton tb = e.button.GetComponentInChildren<TouchButton>();

            if (tb != null && !tb.spawnToast)
                return;

            if (tb != null)
                tb.enabled = false;
            if (c != null)
                c.enabled = false;

            bool setFont = false;
            if (obut != null)
            {
                if (obut.enableAutoSizing)
                {
                    float fs = obut.fontSize;
                    obut.enableAutoSizing = false;
                    obut.fontSize = fs;
                    setFont = true;
                }
            }

            e.button.enabled = false;
            Image i = e.button.GetComponent<Image>();
            i.enabled = false;

            if (toasts.Count > 0)
            {
                CancelInvoke("DestroyToastedButton");
                DestroyToastedButton();
            }

            currentToastedButton = Instantiate(e.button.gameObject, toastCanvas.transform).GetComponent<TouchButton>();
            DisplaySingleChoiceOption but = currentToastedButton?.GetComponent<DisplaySingleChoiceOption>();
            

            if (but != null)
            {
                but.textButton.enableAutoSizing = false;
                but.textButton.overflowMode = TextOverflowModes.Overflow;
            }

            if (but != null && but.tb != null)
            {
                but.tb.enabled = false;
            }

            if (c != null)
                c.enabled = true;
            if (obut != null && setFont)
            {
                obut.enableAutoSizing = true;
            }

            if (tb != null)
                tb.enabled = true;

            i.enabled = true;
            e.button.enabled = true;
            currentToastedButton.gameObject.layer = LayerMask.NameToLayer("ROVERToast");

            if (currentToastedButton.laserPoint != null)
                currentToastedButton.laserPoint.gameObject.SetActive(false);
            currentToastedButton.laserPoint = null;

            var children = currentToastedButton.gameObject.GetComponentsInChildren<Transform>(includeInactive: true);
            foreach (var child in children)
            {
                child.gameObject.layer = LayerMask.NameToLayer("ROVERToast");
            }

            toasts.Add(currentToastedButton.gameObject);

            transform.position = e.button.transform.position;
            transform.rotation = e.button.transform.rotation;

            Invoke("DisplayToastedButton", 0.02f);
            Invoke("DestroyToastedButton", 1f);
        }

        /// <summary>
        /// Destroys the current toasted button and removes it from the list.
        /// </summary>
        void DestroyToastedButton()
        {
            if (toasts.Count > 0 && toasts[0] != null)
            {
                toasts[0].SetActive(false);
                Destroy(toasts[0].gameObject);
                toasts.RemoveAt(0);
            }
            if (toasts.Count < 1)
            {
                currentToastedButton = null;
            }
            overlayTextureRenderer.RenderOnDemand();
        }

        /// <summary>
        /// Destroys the current toasted button and removes it from the list.
        /// </summary>
        void DisplayToastedButton()
        {
            overlayTextureRenderer.RenderOnDemand();
        }

        /// <summary>
        /// Cleans up resources when the object is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (leftPointer != null)
                leftPointer.TouchButtonSelected -= TouchButtonSelected;
            if (rightPointer != null)
                rightPointer.TouchButtonSelected -= TouchButtonSelected;
        }
    }
}
