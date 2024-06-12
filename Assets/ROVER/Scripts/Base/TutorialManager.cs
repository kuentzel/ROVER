using System;
using UnityEngine;
using TMPro;

namespace ROVER
{
    /// <summary>
    /// Event arguments for tutorial events.
    /// </summary>
    public class TutorialEventArgs : EventArgs
    {
        public TutorialEventArgs(string message, DateTime timestamp)
        {
            this.message = message;
            Timestamp = timestamp;
        }

        public string message;
        public DateTime Timestamp { get; set; }
    }

    public delegate void TutorialEventHandler(object sender, TutorialEventArgs e);

    /// <summary>
    /// Manages the tutorial sequence and handles tutorial-related events.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        public event TutorialEventHandler TutorialProgressEvent;

        [Header("Managers")]
        public StudyManager studyManager;
        public LocalizationManager locale;

        [Header("Tutorial Panels")]
        public GameObject tutorial1;
        public GameObject tutorial2;
        public GameObject tutorialpanel2;
        public GameObject tutorial3;
        public GameObject tutorialpanel3;
        public GameObject tutorial4;
        public GameObject tutorialpanel4;
        public GameObject tutorial5;
        public GameObject tutorialpanel5;
        public GameObject tutorialhint;

        [Header("Animations")]
        public Animator anim1;
        public Animator anim2;
        public Animator anim3;
        public Animator anim4;
        public GameObject animContainer;

        [Header("Touch Button")]
        public TouchButton triggerButton;

        private int tutorialState;
        private bool initialized;

        public int TutorialState { get => tutorialState; set => tutorialState = value; }

        /// <summary>
        /// Starts the tutorial sequence.
        /// </summary>
        public void StartTutorial()
        {
            ResetTutorialParts();
            TutorialProgressEvent?.Invoke(this, new TutorialEventArgs("Start", DateTime.UtcNow));
            TutorialProgressEvent?.Invoke(this, new TutorialEventArgs("Display Setup", DateTime.UtcNow));
            tutorial1.SetActive(true);
            tutorialState = 1;
        }

        /// <summary>
        /// Progresses the tutorial to the next step.
        /// </summary>
        public void ProgressTutorial()
        {
            studyManager.InvokeRepeating("FlashHighlightRise", 0f, 0.025f);
            ResetTutorialParts();
            tutorialState++;

            switch (tutorialState)
            {
                case 1:
                    TutorialProgressEvent?.Invoke(this, new TutorialEventArgs("Display Setup", DateTime.UtcNow));
                    tutorial1.SetActive(true);
                    break;
                case 2:
                    TutorialProgressEvent?.Invoke(this, new TutorialEventArgs("Console Setup", DateTime.UtcNow));
                    tutorial2.SetActive(true);
                    tutorialpanel2.SetActive(true);
                    break;
                case 3:
                    TutorialProgressEvent?.Invoke(this, new TutorialEventArgs("Touch Interaction", DateTime.UtcNow));
                    SetupTutorial3();
                    break;
                case 4:
                    TutorialProgressEvent?.Invoke(this, new TutorialEventArgs("Option Selection", DateTime.UtcNow));
                    SetupTutorial4();
                    break;
                case 5:
                    SetupTutorial5();
                    break;
                case 6:
                    EndTutorial();
                    break;
            }
        }

        /// <summary>
        /// Ends the tutorial sequence.
        /// </summary>
        public void EndTutorial()
        {
            TutorialProgressEvent?.Invoke(this, new TutorialEventArgs("End", DateTime.UtcNow));
            tutorialState = 0;
            ResetTutorialParts();
            studyManager.StartStudy();
        }

        /// <summary>
        /// Resets all tutorial parts, deactivating them.
        /// </summary>
        public void ResetTutorialParts()
        {
            tutorial1.SetActive(false);
            tutorial2.SetActive(false);
            tutorial3.SetActive(false);
            tutorial4.SetActive(false);
            tutorial5.SetActive(false);
            tutorialpanel2.SetActive(false);
            tutorialpanel3.SetActive(false);
            tutorialpanel4.SetActive(false);
            tutorialpanel5.SetActive(false);
            animContainer.SetActive(false);
            anim1.gameObject.SetActive(false);
            anim2.gameObject.SetActive(false);
            anim3.gameObject.SetActive(false);
            anim4.gameObject.SetActive(false);
            tutorialhint.SetActive(false);
            studyManager.controlPanel.polarLabelContainer.SetActive(false);
            studyManager.hintPanel.transform.parent.parent.parent.parent.parent.parent.gameObject.SetActive(false);
        }

        // Start is called before the first frame update
        private void Start()
        {
            Invoke("Init", 0.05f);
        }

        /// <summary>
        /// Initializes the tutorial manager and sets up event listeners.
        /// </summary>
        public void Init()
        {
            if (studyManager == null || studyManager.leftPointer == null)
                return;

            initialized = true;

            // Register event handlers for the left pointer
            studyManager.leftPointer.TouchButtonAimed += TouchButtonAimed;
            studyManager.leftPointer.TouchButtonHovered += TouchButtonHovered;
            studyManager.leftPointer.TouchButtonCharging += TouchButtonCharging;
            studyManager.leftPointer.TouchButtonLost += TouchButtonLost;
            studyManager.leftPointer.TouchButtonSelected += TouchButtonSelected;
            studyManager.leftPointer.TouchButtonCommitted += TouchButtonCommitted;

            // Register event handlers for the right pointer
            studyManager.rightPointer.TouchButtonAimed += TouchButtonAimed;
            studyManager.rightPointer.TouchButtonHovered += TouchButtonHovered;
            studyManager.rightPointer.TouchButtonCharging += TouchButtonCharging;
            studyManager.rightPointer.TouchButtonLost += TouchButtonLost;
            studyManager.rightPointer.TouchButtonSelected += TouchButtonSelected;
            studyManager.rightPointer.TouchButtonCommitted += TouchButtonCommitted;
        }

        /// <summary>
        /// Event handler for the TouchButtonCommitted event.
        /// </summary>
        private void TouchButtonCommitted(object sender, TouchButtonEventArgs e)
        {
            anim2.gameObject.SetActive(false);
            anim3.gameObject.SetActive(false);
            anim4.gameObject.SetActive(true);
            if (anim4.gameObject.activeInHierarchy)
                anim4.Play(0, -1, 0);
        }

        private void OnDestroy()
        {
            // Unregister event handlers for the left pointer
            studyManager.leftPointer.TouchButtonAimed -= TouchButtonAimed;
            studyManager.leftPointer.TouchButtonHovered -= TouchButtonHovered;
            studyManager.leftPointer.TouchButtonCharging -= TouchButtonCharging;
            studyManager.leftPointer.TouchButtonLost -= TouchButtonLost;
            studyManager.leftPointer.TouchButtonSelected -= TouchButtonSelected;
            studyManager.leftPointer.TouchButtonCommitted -= TouchButtonCommitted;

            // Unregister event handlers for the right pointer
            studyManager.rightPointer.TouchButtonAimed -= TouchButtonAimed;
            studyManager.rightPointer.TouchButtonHovered -= TouchButtonHovered;
            studyManager.rightPointer.TouchButtonCharging -= TouchButtonCharging;
            studyManager.rightPointer.TouchButtonLost -= TouchButtonLost;
            studyManager.rightPointer.TouchButtonSelected -= TouchButtonSelected;
            studyManager.rightPointer.TouchButtonCommitted -= TouchButtonCommitted;
        }

        private void TouchButtonAimed(object sender, TouchButtonEventArgs e)
        {
            anim1.gameObject.SetActive(false);
            anim2.gameObject.SetActive(true);
            if (anim2.gameObject.activeInHierarchy)
                anim2.Play(0, -1, 0);
        }

        private void TouchButtonHovered(object sender, TouchButtonEventArgs e)
        {
            // Add functionality if needed
        }

        private void TouchButtonCharging(object sender, TouchButtonEventArgs e)
        {
            if (studyManager.triggerTouch)
                return;

            anim2.gameObject.SetActive(false);
            anim3.gameObject.SetActive(true);
            if (anim3.gameObject.activeInHierarchy)
                anim3.Play(0, -1, 0);
        }

        private void TouchButtonSelected(object sender, TouchButtonEventArgs e)
        {
            // Add functionality if needed
        }

        private void TouchButtonLost(object sender, TouchButtonEventArgs e)
        {
            anim2.gameObject.SetActive(false);
            anim3.gameObject.SetActive(false);
            anim4.gameObject.SetActive(false);
            anim1.gameObject.SetActive(true);
            if (anim1.gameObject.activeInHierarchy)
                anim1.Play(0, -1, 0);
        }

        /// <summary>
        /// Sets up the third tutorial step.
        /// </summary>
        private void SetupTutorial3()
        {
            tutorial3.SetActive(true);
            tutorialpanel3.SetActive(true);
            studyManager.hintPanel.transform.parent.parent.parent.parent.parent.parent.gameObject.SetActive(true);
            animContainer.SetActive(true);
            anim1.gameObject.SetActive(true);

            if (!studyManager.triggerTouch)
            {
                tutorialhint.SetActive(true);
                studyManager.hintPanel.GetComponentInChildren<TextMeshProUGUI>().text = "";
            }
            else
            {
                studyManager.hintPanel.GetComponentInChildren<TextMeshProUGUI>().text = locale != null
                    ? locale.GetLocaleString(5).Replace("\\n", "\n")
                    : "To make a selection, aim the <b>white <color=#68B3DE>VIRTUAL POINTER</color></b> at the <b><color=#68B3DE>BUTTON</color></b>,  so that you see a <b>blue frame</b> and then press the <b><color=#68B3DE>CONTROLLER INDEX FINGER BUTTON (TRIGGER)</color>.</b>";
            }
        }

        /// <summary>
        /// Sets up the fourth tutorial step.
        /// </summary>
        private void SetupTutorial4()
        {
            studyManager.backContainer.SetActive(true);
            tutorial4.SetActive(true);
            tutorialpanel4.SetActive(true);
            studyManager.hintPanel.transform.parent.parent.parent.parent.parent.parent.gameObject.SetActive(true);
            animContainer.SetActive(true);
            anim1.gameObject.SetActive(true);

            if (!studyManager.triggerTouch)
            {
                studyManager.hintPanel.GetComponentInChildren<TextMeshProUGUI>().text = "";
                tutorialhint.SetActive(true);
            }
            else
            {
                studyManager.hintPanel.GetComponentInChildren<TextMeshProUGUI>().text = locale != null
                    ? locale.GetLocaleString(5).Replace("\\n", "\n")
                    : "To make a selection, aim the <b>white <color=#68B3DE>VIRTUAL POINTER</color></b> at the <b><color=#68B3DE>BUTTON</color></b>,  so that you see a <b>blue frame</b> and then press the <b><color=#68B3DE>CONTROLLER INDEX FINGER BUTTON (TRIGGER)</color>.</b>";
            }

            studyManager.controlPanel.polarLabelContainer.SetActive(true);
        }

        /// <summary>
        /// Sets up the fifth tutorial step.
        /// </summary>
        private void SetupTutorial5()
        {
            studyManager.backContainer.SetActive(true);
            tutorial5.SetActive(true);
            tutorialpanel5.SetActive(true);
            studyManager.hintPanel.transform.parent.parent.parent.parent.parent.parent.gameObject.SetActive(true);
            animContainer.SetActive(true);
            anim1.gameObject.SetActive(true);

            if (!studyManager.triggerTouch)
            {
                studyManager.hintPanel.GetComponentInChildren<TextMeshProUGUI>().text = "";
                tutorialhint.SetActive(true);
            }
            else
            {
                studyManager.hintPanel.GetComponentInChildren<TextMeshProUGUI>().text = locale != null
                    ? locale.GetLocaleString(5).Replace("\\n", "\n")
                    : "To make a selection, aim the <b>white <color=#68B3DE>VIRTUAL POINTER</color></b> at the <b><color=#68B3DE>BUTTON</color></b>,  so that you see a <b>blue frame</b> and then press the <b><color=#68B3DE>CONTROLLER INDEX FINGER BUTTON (TRIGGER)</color>.</b>";
            }
        }
    }
}
