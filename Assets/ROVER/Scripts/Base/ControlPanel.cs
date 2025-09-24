using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// The namespace that contains classes related to the ROVER system.
/// </summary>
namespace ROVER
{
    public class ControlPanel : MonoBehaviour
    {
        // Private fields        
        private float fade = 0f;

        // Public fields for UI elements
        [Header("Undo Elements")]
        public Image bgUndo;
        public Image iconUndo;
        
        [Header("Last Action Elements")]
        public Image bgLast;
        public TextMeshProUGUI captionLast;
        public TextMeshProUGUI contentLast;
        
        [Header("Navigation Elements")]
        public Image bgNav;
        public Image iconNav;
        
        [Header("Display Elements")]
        public Image bgDisplay;
        public CanvasRenderer flashHighlight;

        [Header("Button Elements")]
        public GameObject buttonContainer;
        public TextMeshProUGUI buttonLabel;

        [Header("Options Containers")]
        public GameObject optionsContainer;
        public GameObject optionButtonPrefab;
        public GameObject optionsFullLabelContainer;
        public GameObject optionFullLabelButtonPrefab;
        public GameObject polarLabelContainer;
        public TextMeshProUGUI labelLeft;
        public TextMeshProUGUI labelRight;
        public GameObject mcConfirmButton;

        // Public properties
        public bool textAutoSizing = false;
        public bool debug;
        public StudyManager studyManager;
        public StyleManager styleManager;


        // Start is called before the first frame update
        void Awake()
        {
            if (styleManager == null)
            {
                styleManager = Transform.FindObjectOfType<StyleManager>();
            }
        }
        void Start()
        {
            flashHighlight.gameObject.SetActive(true);
            flashHighlight?.SetAlpha(0f);
        }

        /// <summary>
        /// Displays a button with the specified text and adjusts the UI elements accordingly.
        /// </summary>
        /// <param name="buttonText">The text to display on the button.</param>
        public void ShowButton(string buttonText)
        {
            // Set the button's material to the secondary active material
            buttonLabel.transform.parent.GetComponent<Image>().material = styleManager.buttonSecondaryActiveMaterial;

            // Deactivate various UI containers
            optionsContainer.SetActive(false);
            optionsFullLabelContainer.SetActive(false);
            polarLabelContainer.SetActive(false);

            // Set the button label text
            buttonLabel.text = buttonText;

            // If the button text is not null and has a length greater than 0, activate the button container
            if (buttonText != null && buttonText.Length > 0)
            {
                buttonContainer.SetActive(true);

                // Cancel any ongoing flash highlight animations
                CancelInvoke("FlashHighlightRise");
                CancelInvoke("FlashHighlightFall");

                // Set the initial fade value and update the highlight alpha
                fade = 0.5f;
                flashHighlight?.SetAlpha(fade);

                // Start the flash highlight rise animation
                InvokeRepeating("FlashHighlightRise", 0f, 0.025f);
            }
            else
            {
                // If the button text is null or empty, deactivate the button container
                buttonContainer.SetActive(false);
            }
        }


        public void ButtonAction()
        {
            studyManager.DisplayNext();
        }

        public void FlashHighlightRise()
        {
            fade += 0.05f;
            flashHighlight?.SetAlpha(fade);

            if (fade >= 0.85f)
            {
                CancelInvoke("FlashHighlightRise");
                InvokeRepeating("FlashHighlightFall", 0.025f, 0.025f);
            }

        }
        public void FlashHighlightFall()
        {
            fade -= 0.15f;
            flashHighlight?.SetAlpha(fade);

            if (fade <= 0.14f)
            {
                fade = 0f;
                flashHighlight?.SetAlpha(fade);
                CancelInvoke("FlashHighlightFall");
            }

        }

        public void ResizePolarLabels()
        {
            if (textAutoSizing)
            {
                labelLeft.enableAutoSizing = false;
                labelRight.enableAutoSizing = false;

                if (labelLeft.fontSize > labelRight.fontSize)
                    labelLeft.fontSize = labelRight.fontSize;
                else
                    labelRight.fontSize = labelLeft.fontSize;
            }

            labelLeft.GetComponent<CanvasRenderer>().SetAlpha(1);
            labelRight.GetComponent<CanvasRenderer>().SetAlpha(1);
        }

        public void HideOptions()
        {
            buttonContainer.SetActive(false);
            optionsFullLabelContainer.SetActive(false);
            optionsContainer.SetActive(false);
            polarLabelContainer.SetActive(false);
        }

        /// <summary>
        /// Displays the options for the specified item using the given layout variant and label settings.
        /// Adjusts the UI elements accordingly.
        /// </summary>
        /// <param name="item">The item whose options are to be displayed.</param>
        /// <param name="variant">The layout variant for displaying options.</param>
        /// <param name="fullLabels">Indicates whether to display full labels for the options.</param>
        public void ShowOptions(Item item, ChoiceItemLayoutVariant variant, bool fullLabels)
        {
            // Cancel any ongoing flash highlight animations
            CancelInvoke("FlashHighlightRise");
            CancelInvoke("FlashHighlightFall");

            // Set the initial fade value and update the highlight alpha
            fade = 0.5f;
            flashHighlight?.SetAlpha(fade);

            // Start the flash highlight rise animation
            InvokeRepeating("FlashHighlightRise", 0f, 0.025f);

            // Deactivate various UI containers
            buttonContainer.SetActive(false);
            optionsFullLabelContainer.SetActive(false);
            optionsContainer.SetActive(false);
            polarLabelContainer.SetActive(false);

            // Handle the case where the variant is ScaleEndpointsNoDisplay
            if (variant == ChoiceItemLayoutVariant.ScaleEndpointsNoDisplay || variant == ChoiceItemLayoutVariant.ScaleEndpoints)
            {
                // Activate the polar labels container and enable auto-sizing if needed
                polarLabelContainer.SetActive(true);
                if (textAutoSizing)
                {
                    labelLeft.enableAutoSizing = true;
                    labelRight.enableAutoSizing = true;
                }

                // Set the alpha for the left and right labels and update their text
                labelLeft.GetComponent<CanvasRenderer>().SetAlpha(0);
                labelRight.GetComponent<CanvasRenderer>().SetAlpha(0);
                labelLeft.text = ((ChoiceItem)item).OptionLabels[0];
                labelRight.text = ((ChoiceItem)item).OptionLabels[((ChoiceItem)item).OptionLabels.Length - 1];

                if (variant == ChoiceItemLayoutVariant.ScaleEndpoints)
                {
                    labelLeft.text = "";
                    labelRight.text = "";
                }

                // Invoke resizing of polar labels after a short delay
                Invoke("ResizePolarLabels", 0.005f);
            }

            // Handle the case where full labels or panel labels variant is specified
            if (fullLabels || variant == ChoiceItemLayoutVariant.PanelLabels)
            {
                fullLabels = true;
                optionsFullLabelContainer.SetActive(true);

                // Destroy all existing children in the options full label container
                foreach (Transform child in optionsFullLabelContainer.GetComponentInChildren<Transform>(true))
                {
                    Destroy(child.gameObject);
                }
            }
            else
            {
                optionsContainer.SetActive(true);

                // Destroy all existing children in the options container
                foreach (Transform child in optionsContainer.GetComponentInChildren<Transform>(true))
                {
                    Destroy(child.gameObject);
                }
            }

            // If the item is a ChoiceItem, populate the options
            if (item is ChoiceItem)
            {
                for (int i = 0; i < ((ChoiceItem)item).OptionScale.Length; i++)
                {
                    DisplaySingleChoiceOption option = null;

                    // Instantiate the appropriate option prefab based on the fullLabels flag
                    if (fullLabels)
                        option = Instantiate(optionFullLabelButtonPrefab, optionsFullLabelContainer.transform).GetComponent<DisplaySingleChoiceOption>();
                    else
                        option = Instantiate(optionButtonPrefab, optionsContainer.transform).GetComponent<DisplaySingleChoiceOption>();

                    // Set the content for the option
                    option?.SetContent(studyManager, item, i);

                    // Apply selected material if the option is already selected
                    if (studyManager.resultManager.Results.ContainsKey(item) && studyManager.resultManager.Results[item].Contains(option.optionNumber))
                    {
                        if (!((ChoiceItem)item).IsMultipleChoice)
                            option.imageButton.material = styleManager.buttonSelectedMaterial;
                    }
                }

                // Show or hide the multiple-choice confirmation button based on the item type
                if (((ChoiceItem)item).IsMultipleChoice)
                {
                    mcConfirmButton.SetActive(true);
                }
                else
                {
                    mcConfirmButton.SetActive(false);
                }
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// This method is called every frame update when run in the Unity Editor.
        /// If the debug flag is set to true, it forces an immediate rebuild of the layout for the current transform.
        /// </summary>
        private void Update()
        {
            if (debug)
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)this.transform);
        }
#endif

        public void OnDisable()
        {
            flashHighlight?.SetAlpha(0);
            CancelInvoke("FlashHighlightFall");
            CancelInvoke("FlashHighlightRise");
        }

        public void OnDestroy()
        {
            flashHighlight?.SetAlpha(0);
            CancelInvoke("FlashHighlightFall");
            CancelInvoke("FlashHighlightRise");
        }
    }
}


