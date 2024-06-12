using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ROVER
{
    /// <summary>
    /// Manages the display and interaction of single choice options in the UI.
    /// </summary>
    public class DisplaySingleChoiceOption : MonoBehaviour
    {
        // Public fields for choice item display
        [Header("Choice Item Display")]
        public DisplayChoiceItem singleChoiceItemDisplay;

        // Public fields for layout settings
        [Header("Layout Settings")]
        public bool isVerticalLayout;
        public bool isWithText;
        public bool isSelfAdjusting;
        public bool endScaleOnly;

        // Public fields for style management
        [Header("Style Management")]
        public StyleManager styleManager;

        // Public fields for UI elements
        [Header("UI Elements")]
        public BoxCollider contentBoundsBoxCollider;
        public Image imageButton;
        public TextMeshProUGUI textButton;
        public TextMeshProUGUI textLabel;

        // Public fields for other settings
        [Header("Settings")]
        public bool debug;
        public StudyManager studyManager;

        // Public fields for referenced item and option
        [Header("Referenced Item and Option")]
        public Item referencedSCitem;
        public int optionNumber;

        // Private fields
        private bool isInitialized;
        private int buttonState;
        private float fade;
        private CanvasRenderer c1, c2, c3;
        public TouchButton tb;

        /// <summary>
        /// Gets or sets the button state.
        /// </summary>
        public int ButtonState
        {
            get => buttonState;
            set => buttonState = value;
        }

        /// <summary>
        /// Gets or sets the StyleManager.
        /// </summary>
        public StyleManager StyleManager
        {
            get => styleManager;
            set => styleManager = value;
        }

        /// <summary>
        /// Start is called before the first frame update.
        /// Initializes the style manager.
        /// </summary>
        void Start()
        {
            if (StyleManager == null)
            {
                StyleManager = FindObjectOfType<StyleManager>();
            }
        }

        /// <summary>
        /// Initializes the display option.
        /// </summary>
        private void Initialize()
        {
            if (ResizeContentBoundsBoxCollider())
            {
                isInitialized = true;
            }
        }

        /// <summary>
        /// Fades in the button elements.
        /// </summary>
        public void FadeIn()
        {
            fade += 0.25f;
            c1?.SetAlpha(fade);
            c2?.SetAlpha(fade);
            c3?.SetAlpha(fade);

            if (fade >= 0.9f)
            {
                CancelInvoke("FadeIn");
            }
        }

        /// <summary>
        /// Sets the content of the display option.
        /// </summary>
        /// <param name="manager">The study manager.</param>
        /// <param name="item">The item to display.</param>
        /// <param name="optionNumber">The option number.</param>
        public void SetContent(StudyManager manager, Item item, int optionNumber)
        {
            studyManager = manager;
            textButton.text = ((ChoiceItem)item).OptionScale[optionNumber];
            this.optionNumber = optionNumber;
            referencedSCitem = item;

            if (isWithText && !endScaleOnly)
            {
                textLabel.text = ((ChoiceItem)item).OptionLabels[optionNumber];
            }

            if (((ChoiceItem)referencedSCitem).IsMultipleChoice && tb != null)
            {
                tb.isToggle = true;
            }

            c1 = textButton?.GetComponent<CanvasRenderer>();
            c2 = textLabel?.GetComponent<CanvasRenderer>();
            c3 = imageButton?.GetComponent<CanvasRenderer>();

            c1?.SetAlpha(0f);
            c2?.SetAlpha(0f);
            c3?.SetAlpha(0f);
            fade = 0f;
            InvokeRepeating("FadeIn", 0.1f, 0.1f);
        }

        /// <summary>
        /// Resets the button state to its default.
        /// </summary>
        private void ResetButtonState()
        {
            //TODO: Reset visual feedback
            //imageButton.material.color = StyleManager.buttonTextPrimaryColor;
            buttonState = 0;
        }

        /// <summary>
        /// Resizes the content bounds box collider based on layout settings.
        /// </summary>
        /// <returns>True if resizing was successful, false otherwise.</returns>
        private bool ResizeContentBoundsBoxCollider()
        {
            RectTransform trans = (RectTransform)contentBoundsBoxCollider.transform;

            // Adjust the size of the box collider based on layout orientation
            contentBoundsBoxCollider.size = isVerticalLayout
                ? new Vector3(trans.rect.width, trans.rect.height, contentBoundsBoxCollider.size.z)
                : new Vector3(trans.rect.width, trans.rect.height, contentBoundsBoxCollider.size.z);

            // Check if the resized collider has valid dimensions
            return trans.rect.width != 0 && trans.rect.height != 0;
        }

        /// <summary>
        /// Handles the button action when clicked.
        /// </summary>
        public void ButtonAction()
        {
            if (((ChoiceItem)referencedSCitem).IsMultipleChoice)
            {
                if (studyManager.ToggleMCAnswer(referencedSCitem, optionNumber) == false)
                    ResetButtonState();
            }
            else
            {
                studyManager.RegisterResults(referencedSCitem, new System.Collections.Generic.List<int> { optionNumber });
                studyManager.DisplayNext();
            }
        }

        /// <summary>
        /// Update is called once per frame.
        /// Ensures the component is initialized and handles debug mode.
        /// </summary>
        void Update()
        {
            // Initialize the component if not already initialized
            if (!isInitialized)
            {
                Initialize();
            }

            // Re-initialize the component if debug mode is enabled
            if (debug)
            {
                Initialize();
            }

            // Force the layout to rebuild immediately
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)transform);
        }
    }
}
