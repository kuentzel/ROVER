using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ROVER
{
    /// <summary>
    /// Manages the display and content of instructions in the UI.
    /// </summary>
    public class DisplayInstruction : MonoBehaviour
    {
        // Private fields
        private StyleManager styleManager;
        private float boxColliderOriginalX;
        private float boxColliderOriginalY;
        private float boxColliderOriginalZ;
        private bool isInitialized;

        // Public fields for managing the study
        [Header("Managers")]
        public StudyManager studyManager;

        // Public fields for text elements
        [Header("Text Elements")]
        public TextMeshProUGUI textTitle;
        public TextMeshProUGUI textContent;
        public TextMeshProUGUI textButton;

        // Public fields for image elements
        [Header("Image Elements")]
        public Image imageTitle;
        public Image imageContent;
        public Image imageButton;

        // Public fields for collider elements
        [Header("Collider Elements")]
        public BoxCollider buttonBoxCollider;
        public float boxColliderOffsetX;
        public float boxColliderOffsetY;
        public float boxColliderOffsetZ;

        // Public fields for other settings
        [Header("Settings")]
        public bool debug;

        // Public properties
        public StyleManager StyleManager
        {
            get => styleManager;
            set => styleManager = value;
        }

        /// <summary>
        /// Start is called before the first frame update.
        /// Initializes the original size of the box collider and sets the style manager.
        /// </summary>
        void Start()
        {
            // Store the original size of the box collider
            boxColliderOriginalX = buttonBoxCollider.size.x;
            boxColliderOriginalY = buttonBoxCollider.size.y;
            boxColliderOriginalZ = buttonBoxCollider.size.z;

            // Find and set the style manager if not already set
            if (StyleManager == null)
            {
                StyleManager = FindObjectOfType<StyleManager>();
            }
        }

        /// <summary>
        /// Triggers the button action to display the next instruction.
        /// </summary>
        public void ButtonAction()
        {
            studyManager.DisplayNext();
        }

        /// <summary>
        /// Initializes the display instruction component.
        /// </summary>
        private void Initialize()
        {
            // Resize the content bounds box collider and set initialization status
            if (ResizeContentBoundsBoxCollider())
            {
                isInitialized = true;
            }
        }

        /// <summary>
        /// Sets the content of the instruction display.
        /// </summary>
        /// <param name="instruction">The instruction to display.</param>
        public void SetContent(Instruction instruction)
        {
            // Set the title, content, and button text
            textTitle.text = instruction.Title;
            textContent.text = instruction.Paragraphs[0];
            textButton.text = instruction.ButtonText;
        }

        /// <summary>
        /// Resizes the content bounds box collider based on the offsets.
        /// </summary>
        /// <returns>True if the resize was successful, false otherwise.</returns>
        private bool ResizeContentBoundsBoxCollider()
        {
            RectTransform trans = (RectTransform)buttonBoxCollider.transform;

            // Adjust the size of the box collider based on the offsets
            buttonBoxCollider.size = new Vector3(boxColliderOriginalX + boxColliderOffsetX, boxColliderOriginalY + boxColliderOffsetY, boxColliderOriginalZ + boxColliderOffsetZ);

            // Check if the resized collider has valid dimensions
            return trans.rect.width != 0 && trans.rect.height != 0;
        }

        /// <summary>
        /// Update is called once per frame.
        /// Ensures the component is initialized and handles debug mode.
        /// </summary>
        void Update()
        {
            // Initialize the component if not already initialized
            if (!isInitialized)
                Initialize();

            // Re-initialize the component if debug mode is enabled
            if (debug)
            {
                Initialize();
            }
        }
    }
}
