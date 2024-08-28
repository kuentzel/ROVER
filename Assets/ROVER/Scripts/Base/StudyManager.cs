using ROVER.Sensors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using UnityEngine;

namespace ROVER
{
    /// <summary>
    /// Provides a thread-safe random number generator.
    /// </summary>
    public static class ThreadSafeRandom
    {
        [ThreadStatic] private static System.Random Local;

        /// <summary>
        /// Gets the random number generator for the current thread.
        /// </summary>
        public static System.Random ThisThreadsRandom
        {
            get
            {
                // Initialize the random number generator for this thread if it doesn't exist
                return Local ?? (Local = new System.Random(unchecked(Environment.TickCount * 31 + Thread.CurrentThread.ManagedThreadId)));
            }
        }
    }

    /// <summary>
    /// Extension methods for IList.
    /// </summary>
    static class MyExtensions
    {
        /// <summary>
        /// Shuffles the elements in the list randomly.
        /// </summary>
        public static void Shuffle<T>(this IList<T> list)
        {
            int n = list.Count;
            while (n > 1)
            {
                n--;
                int k = ThreadSafeRandom.ThisThreadsRandom.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }
    }

    public enum StudyEventType
    {
        Start,
        Step,
        End,
        SupervisorInteraction
    }

    /// <summary>
    /// Event arguments for study events.
    /// </summary>
    public class StudyEventArgs : EventArgs
    {
        public StudyEventArgs(StudyEventType eventType, object element, string message, DateTime timestamp)
        {
            this.eventType = eventType;
            this.element = element;
            this.message = message;
            this.Timestamp = timestamp;
        }

        public StudyEventType eventType;

        public object element;

        public string message;
        public DateTime Timestamp { get; set; }
    }

    public delegate void StudyEventHandler(object sender, StudyEventArgs e);

    /// <summary>
    /// Manages the study activities, including logging, tutorial, and study progression.
    /// </summary>
    public class StudyManager : MonoBehaviour
    {


        [Header("Pointers")]
        public VirtualPointer rightPointer;
        public VirtualPointer leftPointer;

        [Header("Managers")]
        public StudyJsonImportExportManager importExportManager;
        public StyleManager styleManager;
        public TutorialManager tutorialManager;
        public ResultManager resultManager;
        public ControlPanel controlPanel;
        public ActivityLogger activityLogger;
        public SensorLogger sensorLogger;
        public ProgressBarManager progressbarManager;
        public BreadcrumbManager breadManager;

        [Header("Study Components")]
        public TMP_InputField vpnInput;
        public bool displayBreadcrumb = false;
        public bool displayProgressbar;
        public int optionShowingDelay = 2;
        public bool debug = false;
        public DisplayInstruction instructionDisplay;

        public DisplayChoiceItem[] singleChoiceItemVariants = new DisplayChoiceItem[3];

        public Transform displayContainer;
        public TextMeshProUGUI lastSelection;
        public TextMeshProUGUI statustext;
        public GameObject navToggle;
        public GameObject backContainer;
        public TextMeshProUGUI backLabel;
        public GameObject hintPanel;
        public CanvasRenderer flashHighlight;
        public float fade = 0f;
        public bool hidePointerInGame = false;

        [Header("Interaction Debugging")]
        public bool proximityTouch = false;
        public bool timedTouch = false;
        public bool sphereTouch = false;
        public bool triggerTouch = false;

        //private fields

        private Study activeStudy;
        private List<object> studySequence = new List<object>();
        private object currentElement;
        private int nextElementIndex;
        private int currentElementIndex = 1;
        private float[] tePrebackups = new float[3];
        private string _sessionStartTimeString;

        public Study ActiveStudy { get => activeStudy; set => activeStudy = value; }
        public int CurrentElementIndex { get => nextElementIndex; set => nextElementIndex = value; }
        public object CurrentElement { get => currentElement; set => currentElement = value; }
        public string SessionStartTimeString { get => _sessionStartTimeString; set => _sessionStartTimeString = value; }

        public event StudyEventHandler StudyProgressEvent;

        /// <summary>
        /// Unity Awake method called when the script instance is being loaded.
        /// </summary>
        private void Awake()
        {
            // Set the session start time to the current UTC time formatted as a string
            _sessionStartTimeString = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss");
        }

        /// <summary>
        /// Unity Start method called before the first frame update.
        /// </summary>
        void Start()
        {
            // Initialize and reset all relevant fields and components
            ResetAll();
            flashHighlight.gameObject.SetActive(true);
            flashHighlight?.SetAlpha(0f);
            fade = 0;

            // Backup font size settings for text elements in choice item variants
            foreach (DisplayChoiceItem dsci in singleChoiceItemVariants)
            {
                if (dsci.textElementPrefab != null)
                {
                    TextMeshProUGUI tePre = dsci.textElementPrefab.GetComponentInChildren<TextMeshProUGUI>();
                    tePrebackups[0] = tePre.fontSizeMax;
                    tePrebackups[1] = tePre.fontSizeMin;
                    tePrebackups[2] = tePre.fontSize;
                }
            }

            // Start the tutorial
            tutorialManager.StartTutorial();
        }

        /// <summary>
        /// Toggles the timed touch setting.
        /// </summary>
        public void ToggleTimedTouch()
        {
            timedTouch = !timedTouch;
        }

        /// <summary>
        /// Toggles the proximity touch setting.
        /// </summary>
        public void ToggleProximityTouch()
        {
            proximityTouch = !proximityTouch;
        }

        /// <summary>
        /// Starts the study by importing study data and initializing components.
        /// </summary>
        public void StartStudy()
        {
#if UNITY_EDITOR
            if (debug)
#endif
                activityLogger.StopLogging();
#if UNITY_EDITOR
            if (debug)
#endif
                sensorLogger.StopLogging();
#if UNITY_EDITOR
            if (debug)
#endif
                resultManager.StopCollectingResults();

            // Clear previous results
            resultManager.Results?.Clear();
            activeStudy = null;

            // Import study data
            activeStudy = importExportManager.ImportStudy();
            if (activeStudy == null)
            {
                ResetAll();
                tutorialManager.StartTutorial();
                return;
            }

            // Set session start time
            _sessionStartTimeString = DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss");

            // Initialize display settings
            displayProgressbar = activeStudy.ShowProgressBar;
            displayBreadcrumb = false;
            displayContainer.gameObject.SetActive(true);
            controlPanel.gameObject.SetActive(true);
            FillStudySequence();
            nextElementIndex = 0;

#if UNITY_EDITOR
            if (debug)
#endif
                resultManager.StartCollectingResults(vpnInput.text);
#if UNITY_EDITOR
            if (debug)
#endif
                activityLogger.StartLogging(vpnInput.text);
#if UNITY_EDITOR
            if (debug)
#endif
                sensorLogger.StartLogging(vpnInput.text);

            // Reset tutorial if it is not in its initial state
            if (tutorialManager.TutorialState != 0)
            {
                tutorialManager.ResetTutorialParts();
                tutorialManager.TutorialState = 0;
            }

            // Trigger the study start event
            StudyProgressEvent?.Invoke(this, new StudyEventArgs(StudyEventType.Start, null, "Study Start", DateTime.UtcNow));
            // Display the first element of the study
            DisplayNext();
        }

        /// <summary>
        /// Registers a result for a specific item in the current element.
        /// </summary>
        /// <param name="i">The result index.</param>
        public void TestForResult(int i)
        {
            if (currentElement is Item)
            {
                List<int> list = new List<int> { i };
                RegisterResults((Item)currentElement, list);
            }
        }

        /// <summary>
        /// Moves one step back in the study sequence.
        /// </summary>
        public void BackOneStep()
        {
            if (tutorialManager.TutorialState > 1)
            {
                tutorialManager.TutorialState -= 2;
                tutorialManager.ProgressTutorial();
                return;
            }

            nextElementIndex = Math.Max(0, currentElementIndex - 1);
            DisplayNext();
        }

        /// <summary>
        /// Toggles the display container's active state.
        /// </summary>
        public void ToggleDisplay()
        {
            displayContainer.gameObject.SetActive(!displayContainer.gameObject.activeInHierarchy);
            flashHighlight?.SetAlpha(0);
            fade = 0;
        }

        /// <summary>
        /// Called when the MonoBehaviour becomes disabled or inactive.
        /// </summary>
        public void OnDisable()
        {
            flashHighlight?.SetAlpha(0);
            fade = 0;
            CancelInvoke("FlashHighlightFall");
            CancelInvoke("FlashHighlightRise");
        }

        /// <summary>
        /// Called when the MonoBehaviour will be destroyed.
        /// </summary>
        public void OnDestroy()
        {
            flashHighlight?.SetAlpha(0);
            fade = 0;
            CancelInvoke("FlashHighlightFall");
            CancelInvoke("FlashHighlightRise");

            bool tePreChanged = false;
            foreach (DisplayChoiceItem dsci in singleChoiceItemVariants)
            {
                if (!tePreChanged && dsci.textElementPrefab != null)
                {
                    TextMeshProUGUI tePre = dsci.textElementPrefab.GetComponentInChildren<TextMeshProUGUI>();
                    tePre.fontSizeMax = tePrebackups[0];
                    tePre.fontSizeMin = tePrebackups[1];
                    tePre.fontSize = tePrebackups[2];
                    tePreChanged = true;
                }
            }
        }

        private List<int> mc_results = new List<int>();
        private Item resultItem;

        /// <summary>
        /// Appends a multiple choice result for a given item.
        /// </summary>
        /// <param name="item">The item to append the result to.</param>
        /// <param name="mc_result">The result index.</param>
        public bool ToggleMCAnswer(Item item, int mc_result)
        {
            resultItem = item;
            if (!mc_results.Contains(mc_result))
                mc_results.Add(mc_result);
            else
                {
                    mc_results.Remove(mc_result);
                    return false;
                }
            return true;            
        }

        /// <summary>
        /// Sends the collected multiple choice results.
        /// </summary>
        public void SendMCResults()
        {
            if (resultItem == null || !(resultItem is ChoiceItem && ((ChoiceItem)resultItem).IsMultipleChoice))
                return; 
            ChoiceItem choiceItem = (ChoiceItem)resultItem;
            if (mc_results.Count < choiceItem.MinSelection || mc_results.Count > choiceItem.MaxSelection)
                return;
            RegisterResults(resultItem, mc_results);
            resultItem = null;
            mc_results = new List<int>();
            DisplayNext();
        }

        /// <summary>
        /// Registers the results for a given item.
        /// </summary>
        /// <param name="item">The item to register the results for.</param>
        /// <param name="result">The list of result indices.</param>
        public void RegisterResults(Item item, List<int> result)
        {
            resultManager.WriteItemAnswer(item, result);
        }

        /// <summary>
        /// Displays the next element in the study sequence.
        /// </summary>
        public void DisplayNext()
        {
            // If the tutorial is still in progress, proceed to the next tutorial step and exit this method.
            if (tutorialManager.TutorialState != 0)
            {
                tutorialManager.ProgressTutorial();
                return;
            }

            // Reset all display elements and relevant variables.
            ResetAll();

            // Check if we are at the last element in the study sequence.
            if (nextElementIndex + 1 == studySequence.Count)
            {
                // Trigger the study end event.
                StudyProgressEvent?.Invoke(this, new StudyEventArgs(StudyEventType.End, null, "Study End", DateTime.UtcNow));
#if UNITY_EDITOR
                if (debug)
#endif
                    // Export the results if not in default export mode.
                    if (resultManager.exportMode != ExportMode.Default)
                        resultManager.ExportAnswers(true);
            }

            // If the next element index is out of bounds, handle the end of the study sequence.
            if (nextElementIndex >= studySequence.Count || nextElementIndex < 0)
            {
                if (nextElementIndex == studySequence.Count)
                {
                    nextElementIndex++;
                    controlPanel.gameObject.SetActive(false);
                    hintPanel.transform.parent.parent.parent.parent.parent.parent.gameObject.SetActive(false);
                    // Trigger the station shutdown event.
                    StudyProgressEvent?.Invoke(this, new StudyEventArgs(StudyEventType.End, null, "Station Shutdown", DateTime.UtcNow));
                }
                return;
            }

            // Make sure the display container is active.
            displayContainer.gameObject.SetActive(true);

            // Get the current element from the study sequence and increment the index.
            currentElement = studySequence[nextElementIndex++];

            // If the current element is an Item, handle conditional checks.
            if (currentElement is Item)
            {
                Item currentItem = (Item)currentElement;
                // Check if the Item Set has a ConditionItem and if the condition is met.
                if (currentItem.ItemSet.ConditionItem != null)
                {
                    if (resultManager.Results.ContainsKey(currentItem.ItemSet.ConditionItem))
                    {
                        List<int> remains = resultManager.Results[currentItem.ItemSet.ConditionItem].Except(currentItem.ItemSet.ConditionalAnswers).ToList();
                        if (!(remains.Count < resultManager.Results[currentItem.ItemSet.ConditionItem].Count))
                        {
                            currentElementIndex = studySequence.IndexOf(currentItem.ItemSet.ConditionItem) + 1;
                            DisplayNext();
                            return;
                        }
                    }
                    else
                    {
                        currentElementIndex = studySequence.IndexOf(currentItem.ItemSet.ConditionItem) + 1;
                        DisplayNext();
                        return;
                    }
                }
                // Check if the Item itself has a ConditionItem and if the condition is met.
                if (currentItem.ConditionItem != null)
                {
                    Debug.Log("Has Condition Item");
                    if (resultManager.Results.ContainsKey(currentItem.ConditionItem))
                    {
                        List<int> remains = resultManager.Results[currentItem.ConditionItem].Except(currentItem.ConditionalAnswers).ToList();
                        if (!(remains.Count < resultManager.Results[currentItem.ConditionItem].Count))
                        {
                            Debug.Log("Condition not met");
                            currentElementIndex = studySequence.IndexOf(currentItem.ConditionItem) + 1;
                            DisplayNext();
                            return;
                        }
                        Debug.Log("Condition met");
                    }
                    else
                    {
                        currentElementIndex = studySequence.IndexOf(currentItem.ConditionItem) + 1;
                        Debug.Log("Condition not answered");
                        DisplayNext();
                        return;
                    }
                }
            }

            // If the current element is an Instruction, handle conditional checks.
            if (currentElement is Instruction)
            {
                Instruction currentInstruction = (Instruction)currentElement;
                if (currentInstruction.ConditionItem != null)
                {
                    if (resultManager.Results.ContainsKey(currentInstruction.ConditionItem))
                    {
                        List<int> remains = resultManager.Results[currentInstruction.ConditionItem].Except(currentInstruction.ConditionalAnswers).ToList();
                        if (!(remains.Count < resultManager.Results[currentInstruction.ConditionItem].Count))
                        {
                            currentElementIndex = studySequence.IndexOf(currentInstruction.ConditionItem) + 1;
                            DisplayNext();
                            return;
                        }
                    }
                    else
                    {
                        currentElementIndex = studySequence.IndexOf(currentInstruction.ConditionItem) + 1;
                        DisplayNext();
                        return;
                    }
                }
            }

            // Trigger the study step event.
            StudyProgressEvent?.Invoke(this, new StudyEventArgs(StudyEventType.Step, currentElement, "Study Step", DateTime.UtcNow));

            // Hide the hint panel.
            ToggleHintPanel(false);

            // Update the progress bar display.
            progressbarManager.gameObject.SetActive(displayProgressbar);
            if (displayProgressbar)
                progressbarManager.SetProgress((int)((float)CurrentElementIndex / studySequence.Count * 100));

            // Determine if the back button should be visible.
            if (nextElementIndex > 1 && !(studySequence[nextElementIndex - 2] is Item && !((Item)studySequence[nextElementIndex - 2]).ItemSet.AllowBacksteps))
            {
                backContainer.SetActive(true);
            }
            else
            {
                backContainer.SetActive(false);
            }

            // Handle displaying instructions.
            if (currentElement is Instruction)
            {
                lastSelection.text = "";
                instructionDisplay.gameObject.SetActive(true);
                instructionDisplay.SetContent((Instruction)currentElement);
                breadManager.gameObject.SetActive(displayBreadcrumb);
                if (displayBreadcrumb)
                    breadManager.SetContent("", "" + ((Instruction)currentElement).Index, "" + ((Instruction)currentElement).Section.Index);

                CancelInvoke("DelayShowingButton");
                if (optionShowingDelay > 0)
                {
                    controlPanel.HideOptions();
                    Invoke("DelayShowingButton", optionShowingDelay);
                }
                else
                {
                    controlPanel.ShowButton(((Instruction)currentElement).ButtonText);
                }

                Instruction i = (Instruction)currentElement;
                DisplayProgressOnScreen("", "" + i.Index, "" + i.Section.Index, "");
            }
            // Handle displaying items.
            else if (currentElement is Item)
            {
                Item item = (Item)currentElement;
                backContainer.SetActive(activeStudy.AllowBacksteps && item.ItemSet.AllowBacksteps);

                DisplayItem(item);
                breadManager.gameObject.SetActive(displayBreadcrumb);
                if (displayBreadcrumb)
                    breadManager.SetContent("" + item.Index, "" + item.ItemSet.Index, "" + item.ItemSet.Section.Index);

                DisplayProgressOnScreen("" + item.Index, "" + item.ItemSet.Index, "" + item.ItemSet.Section.Index, item.Title);
                ToggleHintPanel(true);
            }

            // Update the current element index.
            currentElementIndex = nextElementIndex - 1;
            if (currentElementIndex > 0 && studySequence[currentElementIndex - 1] is Instruction)
                backContainer.SetActive(true);

            // Start the highlight flash effect.
            fade = 0;
            InvokeRepeating("FlashHighlightRise", 0f, 0.025f);
        }


        /// <summary>
        /// Displays the current progress on the screen.
        /// </summary>
        /// <param name="item">Current item index.</param>
        /// <param name="itemSet">Current item set index.</param>
        /// <param name="section">Current section index.</param>
        /// <param name="itemtitle">Current item title.</param>
        public void DisplayProgressOnScreen(string item, string itemSet, string section, string itemtitle)
        {
            statustext.text = "Progress: " + ((int)((float)CurrentElementIndex / studySequence.Count * 100)) + "%";
            statustext.text += "\nSection: " + section;
            if (!string.IsNullOrEmpty(item))
                statustext.text += "\nItemSet: " + itemSet + "\nItem: " + item + "\n" + itemtitle;
            else
                statustext.text += "\nElement: " + itemSet;
        }

        /// <summary>
        /// Gradually increases the highlight flash.
        /// </summary>
        public void FlashHighlightRise()
        {
            fade += 0.15f;
            flashHighlight?.SetAlpha(fade);

            if (fade >= 0.85f)
            {
                CancelInvoke("FlashHighlightRise");
                InvokeRepeating("FlashHighlightFall", 0.025f, 0.025f);
            }
        }

        /// <summary>
        /// Gradually decreases the highlight flash.
        /// </summary>
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

        public LocalizationManager locale;

        /// <summary>
        /// Toggles the hint panel visibility.
        /// </summary>
        /// <param name="enable">Whether to enable the hint panel.</param>
        public void ToggleHintPanel(bool enable)
        {
            hintPanel.transform.parent.parent.parent.parent.parent.parent.gameObject.SetActive(enable);

            if (enable && currentElement is Item)
            {
                Item item = (Item)currentElement;
                if (locale != null)
                    hintPanel.transform.parent.GetComponent<TextMeshProUGUI>().text = locale.GetLocaleString(30);
                else
                    hintPanel.transform.parent.GetComponent<TextMeshProUGUI>().text = "Hint";
                hintPanel.GetComponentInChildren<TextMeshProUGUI>(true).text = item.ItemSet.Hint;
            }
        }

        /// <summary>
        /// Resets all the display components.
        /// </summary>
        public void ResetAll()
        {
            mc_results = new List<int>();
            instructionDisplay.gameObject.SetActive(false);
            foreach (var variant in singleChoiceItemVariants)
            {
                variant.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Displays the given item.
        /// </summary>
        /// <param name="item">The item to display.</param>
        private void DisplayItem(Item item)
        {
            if (item is ChoiceItem)
            {
                ChoiceItem ci = (ChoiceItem)item;
                singleChoiceItemVariants[(int)ci.LayoutVariant].gameObject.SetActive(true);
                singleChoiceItemVariants[(int)ci.LayoutVariant].SetContent(this, item);

                CancelInvoke("DelayShowingOptions");
                if (optionShowingDelay > 0)
                {
                    controlPanel.HideOptions();
                    Invoke("DelayShowingOptions", optionShowingDelay);
                }
                else
                {
                    controlPanel.ShowOptions(item, ci.LayoutVariant, false);
                }
            }
        }

        /// <summary>
        /// Delays showing the options for choice items.
        /// </summary>
        private void DelayShowingOptions()
        {
            try
            {
                if (currentElement is ChoiceItem)
                    controlPanel.ShowOptions((ChoiceItem)studySequence[currentElementIndex], ((ChoiceItem)studySequence[currentElementIndex]).LayoutVariant, false);
            }
            catch (InvalidCastException)
            {
                // Handle exception
            }
        }

        /// <summary>
        /// Delays showing the button for instructions.
        /// </summary>
        private void DelayShowingButton()
        {
            try
            {
                if (studySequence[currentElementIndex] is Instruction)
                    controlPanel.ShowButton(((Instruction)studySequence[currentElementIndex]).ButtonText);
            }
            catch (InvalidCastException)
            {
                // Handle exception
            }
        }

        private bool generateRandomResults = false;

        /// <summary>
        /// Fills the study sequence with items and elements from the active study.
        /// </summary>
        /// <summary>
/// Fills the study sequence with items and elements from the active study.
/// </summary>
private void FillStudySequence()
{
    // Clear the existing study sequence to start fresh.
    studySequence.Clear();

    // Iterate over each section in the active study.
    foreach (StudySection section in activeStudy.Sections)
    {
        // Iterate over each element within the current section.
        foreach (StudyElement element in section.Elements)
        {
            // Check if the current element is an ItemSet.
            if (element.Type == TypeOfStudyElement.ItemSet)
            {
                ItemSet itemSet = (ItemSet)element; // Cast the element to an ItemSet.
                List<Item> randomOrder = new List<Item>(); // List to hold items if they need to be randomized.

                // Iterate over each item in the ItemSet.
                foreach (Item item in itemSet.Items)
                {
                    // If the item does not have an ItemSet, assign it the current ItemSet.
                    if (item.ItemSet == null)
                        item.ItemSet = itemSet;

                    // If the ItemSet is set to randomize items, add the item to the randomOrder list.
                    if (itemSet.RandomizeItems)
                        randomOrder.Add(item);
                    else
                        // Otherwise, add the item directly to the study sequence.
                        studySequence.Add(item);

                    // If in debug mode and random results generation is enabled, generate random results for the item.
                    if (debug && generateRandomResults)
                    {
                        System.Random random = new System.Random(); // Create a new random number generator.
                        List<int> result = new List<int> { random.Next(1, 4) }; // Generate a random result.
                        resultManager.Results.Add(item, result); // Add the result to the result manager.
                    }
                }

                // If items need to be randomized and there are items in the randomOrder list.
                if (itemSet.RandomizeItems && randomOrder.Count > 0)
                {
                    randomOrder.Shuffle(); // Shuffle the items in the randomOrder list.
                    studySequence.AddRange(randomOrder); // Add the shuffled items to the study sequence.
                }
            }
            else
            {
                // If the element is not an ItemSet, add it directly to the study sequence.
                studySequence.Add(element);
            }
        }
    }
    // Log the number of elements in the filled study sequence for debugging purposes.
    Debug.Log("StudySequence filled:" + studySequence.Count);
}

    }
}
