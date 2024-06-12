using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ROVER
{
    public class DisplayChoiceItem : MonoBehaviour
    {
        private StyleManager styleManager;

        [Header("UI Elements")]
        public Image imageTitleBar;
        public TextMeshProUGUI textTitle;
        public Image imageContent;
        public TextMeshProUGUI textContent;
        public RectTransform textContainer;
        public GameObject textElementPrefab;
        public TextMeshProUGUI textEndpointLegendLeft;
        public TextMeshProUGUI textEndpointLegendRight;
        public GameObject optionsPrefab;
        public RectTransform optionsContainer;

        [Header("Settings")]
        public bool hasParagraphs;
        public bool noOptionsOnDisplay = false;
        public bool hasEndpointLegend;
        public bool debug;

        public ChoiceItemLayoutVariant variant;
        public StudyManager studyManager;
        public DisplaySingleChoiceOption[] singleChoiceOptionDisplays;

        public StyleManager StyleManager
        {
            get => styleManager;
            set => styleManager = value;
        }

        // Start is called before the first frame update
        private void Start()
        {
            // Find and assign the StyleManager instance if not already set
            if (StyleManager == null)
            {
                StyleManager = FindObjectOfType<StyleManager>();
            }
        }

        // Update is called once per frame
        private void Update()
        {
            if (debug)
            {
                // Force the layout to rebuild immediately for the options container
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)optionsContainer);

                // Force the layout to rebuild immediately for the text container if paragraphs are enabled
                if (hasParagraphs)
                {
                    LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)textContainer);
                }
            }
        }

        /// <summary>
        /// Sets the content of the display choice item.
        /// </summary>
        /// <param name="studyManager">The study manager instance.</param>
        /// <param name="item">The item to display.</param>
        public void SetContent(StudyManager studyManager, Item item)
        {
            // Cast the item to ChoiceItem type
            ChoiceItem sci = (ChoiceItem)item;

            // Set the title text
            textTitle.text = sci.Title;

            if (hasParagraphs)
            {
                // Clear any existing text elements in the text container
                ClearChildren(textContainer);

                // Create and set text for each paragraph
                foreach (string paragraph in sci.Paragraphs)
                {
                    TextMeshProUGUI textBit = Instantiate(textElementPrefab, textContainer).GetComponentInChildren<TextMeshProUGUI>();
                    textBit.text = paragraph;
                }
            }
            else
            {
                // Set the content text to the first paragraph if available, otherwise empty
                textContent.text = sci.Paragraphs != null && sci.Paragraphs.Length > 0 ? sci.Paragraphs[0] : "";
            }

            // Assign the study manager
            this.studyManager = studyManager;

            if (hasEndpointLegend)
            {
                // Set the endpoint legend texts
                textEndpointLegendLeft.text = sci.OptionLabels[0];
                textEndpointLegendRight.text = sci.OptionLabels[sci.OptionLabels.Length - 1];
            }

            if (!noOptionsOnDisplay)
            {
                // Clear any existing option elements in the options container
                ClearChildren(optionsContainer);

                // Create and set content for each option
                for (int i = 0; i < sci.OptionScale.Length; i++)
                {
                    DisplaySingleChoiceOption option = Instantiate(optionsPrefab, optionsContainer).GetComponent<DisplaySingleChoiceOption>();
                    option.SetContent(studyManager, item, i);
                }
            }
        }

        /// <summary>
        /// Clears all children of the specified RectTransform.
        /// </summary>
        /// <param name="parent">The parent RectTransform to clear.</param>
        private void ClearChildren(RectTransform parent)
        {
            // Destroy all child game objects of the specified parent
            foreach (Transform child in parent.GetComponentInChildren<Transform>(true))
            {
                Destroy(child.gameObject);
            }
        }
    }
}
