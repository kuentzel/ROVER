using ROVER.Sensors;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Valve.VR;

namespace ROVER
{
    /// <summary>
    /// Manages the Desktop User Interface for ROVER.
    /// </summary>
    public class DesktopUserInterfaceManager : MonoBehaviour
    {
        #region Public Fields

        [Header("Control Panel Settings")]
        public TextMeshProUGUI labelControlPanelHeight;
        public Slider sliderControlPanelHeight;
        public Transform transformControlPanel;

        [Header("Display Panel Settings")]
        public TextMeshProUGUI labelDisplayPanelHeight;
        public Slider sliderDisplayPanelHeight;
        public TextMeshProUGUI labelDisplayPanelRadius;
        public Slider sliderDisplayPanelRadius;
        public Transform transformDisplayPanel;

        [Header("Survey Station Settings")]
        public TextMeshProUGUI labelSurveyStationRotation;
        public Slider sliderSurveyStationRotation;
        public Transform transformSurveyStation;
        public GameObject surveyStationBase;

        [Header("Study Manager")]
        public StudyManager studyManager;
        public Image startButtonImage;

        [Header("Selection Settings")]
        public TextMeshProUGUI labelSelectionSpeed;
        public Slider sliderSelectionSpeed;
        public TextMeshProUGUI labelTimeDelay;
        public Slider sliderTimeDelay;

        [Header("Toast Manager")]
        public ButtonToastManager toast;

        [Header("Interaction Tutorial")]
        public Transform interactionTut;
        public Transform animation1;

        [Header("Touch Buttons")]
        public Image TimeTouchButton;
        public Image ProxyTouchButton;
        public Image TriggerTouchButton;
        public Image ProxySphereTouchButton;

        [Header("Overlay Settings")]
        public GameObject rightArrowOverlays;
        public GameObject leftArrowOverlays;
        public GameObject ArrowOverlayContainer;
        public GameObject NotificationOverlayContainer;
        public GameObject DisplayContentContainer;
        public GameObject PositiveFeedback;
        public Image PositiveFeedbackButton;
        public GameObject NeutralFeedback;
        public Image NeutralFeedbackButton;

        [Header("Heart Rate Settings")]
        public SensorLogger sLogger;
        public TextMeshProUGUI heartRateText;
        public TextMeshProUGUI heartRateConnection;

        [Header("Display Mirrors")]
        public RawImage displayMirror;
        public RawImage navigatorMirror;

        [Header("Arrows")]
        public GameObject[] arrows;
        public Image[] arrowImages;
        public Image[] rotationImages;
        public Image arrowDirectionImage;
        public bool arrowDirection = false;
        public GameObject hintPanel;
        public GameObject avatarFallback;

        [Header("SteamVR")]
        public SteamVR_TrackedObject tracker;
        public SteamVR_Behaviour_Pose trackerBehaviour;

        #endregion

        #region Private Fields

        private float originalHeightControlPanel;
        private float originalHeightDisplayPanel;
        private float originalRadiusDisplayPanel;
        private float originalYRotationSurveyStation;
        private bool rightOn;
        private bool leftOn;
        private bool positiveOn;
        private bool neutralOn;
        private bool doubleClickProtected = false;
        private bool hrOverride;
        private int currentArrowGroup;
        private int currentRotation = 1;
        private float stationRotation;
        private int overrideHR = 77;

        #endregion

        #region Unity Methods

        /// <summary>
        /// Initialize the component.
        /// </summary>
        private void Awake()
        {
            ArrowOverlayContainer.SetActive(true);
        }

        /// <summary>
        /// Start is called before the first frame update.
        /// </summary>
        private void Start()
        {
            Invoke("Init", 1f);
            if (hrOverride)
            {
                InvokeRepeating("HeartRateOverride", 0.5f, 3f);
                heartRateText.text = "75";
            }
        }

        /// <summary>
        /// Cleanup on destroy.
        /// </summary>
        private void OnDestroy()
        {
            if (sLogger != null && sLogger.enabled && !hrOverride)
                sLogger.HREvent -= UpdateHeartRateText;
        }

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes the UI and sets up the logger event handlers.
        /// </summary>
        private void Init()
        {
            if (sLogger != null && sLogger.enabled && !hrOverride)
                sLogger.HREvent += UpdateHeartRateText;

            originalYRotationSurveyStation = transformSurveyStation.localRotation.eulerAngles.y;
            originalHeightControlPanel = transformControlPanel.localPosition.y;
            originalHeightDisplayPanel = transformDisplayPanel.localPosition.y;
            originalRadiusDisplayPanel = transformDisplayPanel.localPosition.z;
        }

        #endregion

        #region UI Methods

        /// <summary>
        /// Prevents double-clicks by introducing a short delay.
        /// </summary>
        /// <returns>True if double-click protection is active, otherwise false.</returns>
        bool DoubleClickProtection()
        {
            if (!doubleClickProtected)
            {
                Invoke("DelayDoubleClick", 0.25f);
                doubleClickProtected = true;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Resets the double-click protection flag.
        /// </summary>
        void DelayDoubleClick()
        {
            doubleClickProtected = false;
        }

        /// <summary>
        /// Toggles the interaction tutorial visibility.
        /// </summary>
        public void ToggleButtonTutorial()
        {
            if (DoubleClickProtection()) return;
            interactionTut.gameObject.SetActive(!interactionTut.gameObject.activeInHierarchy);
            animation1.gameObject.SetActive(true);
        }

        /// <summary>
        /// Starts the study.
        /// </summary>
        public void StartStudy()
        {
            if (DoubleClickProtection()) return;
            startButtonImage.color = studyManager.styleManager.buttonSelectedColor;
            studyManager.StartStudy();
        }

        /// <summary>
        /// Advances to the next step in the study.
        /// </summary>
        public void StudyNextStep()
        {
            if (DoubleClickProtection()) return;
            studyManager.DisplayNext();
        }

        /// <summary>
        /// Goes back one step in the study.
        /// </summary>
        public void StudyBackStep()
        {
            if (DoubleClickProtection()) return;
            studyManager.BackOneStep();
        }

        /// <summary>
        /// Toggles the display mirror.
        /// </summary>
        public void ToggleMirror()
        {
            if (DoubleClickProtection()) return;
            displayMirror.gameObject.GetComponent<RawImage>().enabled = !displayMirror.gameObject.activeSelf;
        }

        /// <summary>
        /// Updates the control panel height slider.
        /// </summary>
        public void SliderPanelHeightUpdate()
        {
            // Update the label with the new slider value.
            labelControlPanelHeight.text = sliderControlPanelHeight.value + "cm";
            // Update the position of the control panel based on the slider value.
            transformControlPanel.localPosition = new Vector3(transformControlPanel.localPosition.x, originalHeightControlPanel + sliderControlPanelHeight.value / 100, transformControlPanel.localPosition.z);
        }

        /// <summary>
        /// Updates the display panel height slider.
        /// </summary>
        public void SliderDisplayHeightUpdate()
        {
            // Update the label with the new slider value.
            labelDisplayPanelHeight.text = sliderDisplayPanelHeight.value + "cm";
            // Update the position of the display panel based on the slider value.
            transformDisplayPanel.localPosition = new Vector3(transformDisplayPanel.localPosition.x, originalHeightDisplayPanel + sliderDisplayPanelHeight.value / 100, transformDisplayPanel.localPosition.z);
        }

        /// <summary>
        /// Updates the display panel radius slider.
        /// </summary>
        public void SliderDisplayRadiusUpdate()
        {
            // Update the label with the new slider value.
            labelDisplayPanelRadius.text = sliderDisplayPanelRadius.value + "cm";
            // Update the position of the display panel based on the slider value.
            transformDisplayPanel.localPosition = new Vector3(transformDisplayPanel.localPosition.x, transformDisplayPanel.localPosition.y, originalRadiusDisplayPanel + sliderDisplayPanelRadius.value / 100);
        }

        /// <summary>
        /// Updates the survey station rotation slider.
        /// </summary>
        public void SliderSurveyStationRotationUpdate()
        {
            // Update the rotation of the survey station based on the slider value.
            transformSurveyStation.localRotation = Quaternion.Euler(transformSurveyStation.localRotation.eulerAngles.x, originalYRotationSurveyStation + stationRotation, transformSurveyStation.localRotation.eulerAngles.z);
        }

        /// <summary>
        /// Sets the survey station to front position.
        /// </summary>
        public void SliderFront()
        {
            if (DoubleClickProtection()) return;
            sliderSurveyStationRotation.value = 0;
        }

        /// <summary>
        /// Resets the rotation images to their default state.
        /// </summary>
        public void ResetRotationImages()
        {
            // Reset the color of each rotation image to white.
            foreach (Image i in rotationImages)
            {
                i.color = Color.white;
            }
        }

        /// <summary>
        /// Sets the hint panel side based on the direction.
        /// </summary>
        public void SetHintPanelSide(bool direction)
        {
            if (direction)
            {
                // Set the hint panel and avatar fallback to the right side.
                hintPanel.transform.localPosition = new Vector3(1.3f, 0.1f, -0.3f);
                hintPanel.transform.localRotation = Quaternion.Euler(0, 30, 0);
                avatarFallback.transform.localPosition = new Vector3(-2.2f, 0f, -1.3f);
            }
            else
            {
                // Set the hint panel and avatar fallback to the left side.
                hintPanel.transform.localPosition = new Vector3(-1.3f, 0.1f, -0.3f);
                hintPanel.transform.localRotation = Quaternion.Euler(0, -30, 0);
                avatarFallback.transform.localPosition = new Vector3(2.2f, 0f, -1.3f);
            }
        }

        /// <summary>
        /// Rotates the survey station.
        /// </summary>
        public void SurveyStationRotation(float angle)
        {
            stationRotation = angle;
            SliderSurveyStationRotationUpdate();
            ResetRotationImages();
            surveyStationBase.SetActive(false);

            if (angle == 0)
            {
                if (currentRotation == 1)
                {
                    currentRotation = 0;
                    return;
                }
                currentRotation = 1;
                SetHintPanelSide(false);
                rotationImages[0].color = studyManager.styleManager.buttonSelectedColor;
            }
            else if (angle == 90)
            {
                if (currentRotation == 2)
                {
                    currentRotation = 0;
                    return;
                }
                currentRotation = 2;
                SetHintPanelSide(true);
                rotationImages[1].color = studyManager.styleManager.buttonSelectedColor;
            }
            else if (angle == 180)
            {
                if (currentRotation == 3)
                {
                    currentRotation = 0;
                    return;
                }
                currentRotation = 3;
                SetHintPanelSide(false);
                rotationImages[2].color = studyManager.styleManager.buttonSelectedColor;
            }
            else if (angle == -90)
            {
                if (currentRotation == 4)
                {
                    currentRotation = 0;
                    return;
                }
                currentRotation = 4;
                SetHintPanelSide(false);
                rotationImages[3].color = studyManager.styleManager.buttonSelectedColor;
            }
            surveyStationBase.SetActive(true);
            studyManager.flashHighlight?.SetAlpha(0f);
            studyManager.controlPanel.flashHighlight?.SetAlpha(0f);
        }

        /// <summary>
        /// Resets the color of all arrow images to white.
        /// </summary>
        public void ResetArrowImages()
        {
            // Reset the color of each arrow image to white.
            foreach (Image i in arrowImages)
            {
                i.color = Color.white;
            }
        }

        /// <summary>
        /// Sets the slider for rear position.
        /// </summary>
        public void SliderRear()
        {
            if (DoubleClickProtection()) return;
            sliderSurveyStationRotation.value = 180;
        }

        /// <summary>
        /// Updates the selection speed slider.
        /// </summary>
        public void SliderSelectionSpeed()
        {
            if (DoubleClickProtection()) return;
            labelSelectionSpeed.text = "" + (180 + sliderSelectionSpeed.value);
            studyManager.rightPointer.highlightingSpeed = -1f * sliderSelectionSpeed.value;
            studyManager.leftPointer.highlightingSpeed = -1f * sliderSelectionSpeed.value;
        }

        /// <summary>
        /// Resets the color of all touch button images to white.
        /// </summary>
        public void ResetTouchButtonColors()
        {
            TimeTouchButton.color = Color.white;
            TriggerTouchButton.color = Color.white;
            ProxySphereTouchButton.color = Color.white;
        }

        /// <summary>
        /// Toggles the timed touch mode.
        /// </summary>
        public void ToggleTimedTouch()
        {
            ResetTouchButtonColors();
            studyManager.timedTouch = true;
            studyManager.proximityTouch = true;
            studyManager.sphereTouch = false;
            studyManager.triggerTouch = false;
            TimeTouchButton.color = studyManager.styleManager.buttonSelectedColor;
        }

        /// <summary>
        /// Toggles the proximity touch mode.
        /// </summary>
        public void ToggleProximityTouch()
        {
            ResetTouchButtonColors();
            studyManager.timedTouch = false;
            studyManager.proximityTouch = true;
            studyManager.sphereTouch = false;
            studyManager.triggerTouch = false;
            ProxyTouchButton.color = studyManager.styleManager.buttonSelectedColor;
        }

        /// <summary>
        /// Toggles the trigger touch mode.
        /// </summary>
        public void ToggleTriggerTouch()
        {
            ResetTouchButtonColors();
            studyManager.timedTouch = false;
            studyManager.proximityTouch = true;
            studyManager.sphereTouch = false;
            studyManager.triggerTouch = true;
            TriggerTouchButton.color = studyManager.styleManager.buttonSelectedColor;
        }

        /// <summary>
        /// Toggles the proximity touch mode with sphere.
        /// </summary>
        public void ToggleProximityTouchWithSphere()
        {
            ResetTouchButtonColors();
            studyManager.timedTouch = false;
            studyManager.proximityTouch = true;
            studyManager.sphereTouch = true;
            studyManager.triggerTouch = false;
            ProxySphereTouchButton.color = studyManager.styleManager.buttonSelectedColor;
        }

        /// <summary>
        /// Toggles the visibility of the pointer.
        /// </summary>
        public void ToggleHidePointer()
        {
            if (DoubleClickProtection()) return;
            studyManager.hidePointerInGame = !studyManager.hidePointerInGame;
        }

        /// <summary>
        /// Updates the selection speed slider.
        /// </summary>
        public void SliderSelectionSpeed(int i)
        {
            labelSelectionSpeed.text = "" + (180 + sliderSelectionSpeed.value);
            studyManager.rightPointer.highlightingSpeed = -1f * sliderSelectionSpeed.value;
            studyManager.leftPointer.highlightingSpeed = -1f * sliderSelectionSpeed.value;
        }

        /// <summary>
        /// Updates the time delay slider.
        /// </summary>
        public void SliderTimeDelay()
        {
            labelTimeDelay.text = sliderTimeDelay.value + "s";
            studyManager.optionShowingDelay = (int)sliderTimeDelay.value;
        }

        /// <summary>
        /// Toggles the visibility of the toast notifications.
        /// </summary>
        public void ToggleToast()
        {
            if (DoubleClickProtection()) return;
            toast.gameObject.SetActive(!toast.gameObject.activeInHierarchy);
        }

        /// <summary>
        /// Reloads the current scene.
        /// </summary>
        public void ReloadScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(scene.name);
        }

        /// <summary>
        /// Toggles the arrow direction.
        /// </summary>
        public void ToggleArrowDirection()
        {
            arrowDirection = !arrowDirection;
            if (!arrowDirection)
                arrowDirectionImage.transform.localRotation = Quaternion.Euler(arrowDirectionImage.transform.localRotation.eulerAngles.x, arrowDirectionImage.transform.localRotation.eulerAngles.y, 90f);
            else
                arrowDirectionImage.transform.localRotation = Quaternion.Euler(arrowDirectionImage.transform.localRotation.eulerAngles.x, arrowDirectionImage.transform.localRotation.eulerAngles.y, -90f);
            foreach (GameObject arrow in arrows)
            {
                if (arrowDirection)
                    arrow.transform.localRotation = Quaternion.Euler(arrow.transform.localRotation.eulerAngles.x, arrow.transform.localRotation.eulerAngles.y, 0f);
                else
                    arrow.transform.localRotation = Quaternion.Euler(arrow.transform.localRotation.eulerAngles.x, arrow.transform.localRotation.eulerAngles.y, 180f);
            }
        }

        /// <summary>
        /// Sets the arrow direction based on the given direction.
        /// </summary>
        public void SetArrowDirection(bool direction)
        {
            foreach (GameObject arrow in arrows)
            {
                if (direction)
                    arrow.transform.localRotation = Quaternion.Euler(arrow.transform.localRotation.eulerAngles.x, arrow.transform.localRotation.eulerAngles.y, 0f);
                else
                    arrow.transform.localRotation = Quaternion.Euler(arrow.transform.localRotation.eulerAngles.x, arrow.transform.localRotation.eulerAngles.y, 180f);
            }
        }

        /// <summary>
        /// Toggles the visibility of the specified arrow group.
        /// </summary>
        public void ToggleArrowGroup(int i)
        {
            foreach (GameObject arrow in arrows)
                arrow.SetActive(false);

            ResetArrowImages();

            switch (i)
            {
                case 11:
                    if (currentArrowGroup == 11)
                    {
                        currentArrowGroup = 0;
                        return;
                    }
                    currentArrowGroup = 11;
                    arrowImages[0].color = studyManager.styleManager.buttonSelectedColor;
                    arrows[0].SetActive(true);
                    arrows[1].SetActive(true);
                    arrows[7].SetActive(true);
                    SetArrowDirection(false);
                    break;
                case 12:
                    if (currentArrowGroup == 12)
                    {
                        currentArrowGroup = 0;
                        return;
                    }
                    currentArrowGroup = 12;
                    arrowImages[1].color = studyManager.styleManager.buttonSelectedColor;
                    arrows[0].SetActive(true);
                    arrows[1].SetActive(true);
                    arrows[7].SetActive(true);
                    SetArrowDirection(true);
                    break;
                case 21:
                    if (currentArrowGroup == 21)
                    {
                        currentArrowGroup = 0;
                        return;
                    }
                    currentArrowGroup = 21;
                    arrowImages[2].color = studyManager.styleManager.buttonSelectedColor;
                    arrows[1].SetActive(true);
                    arrows[2].SetActive(true);
                    arrows[3].SetActive(true);
                    SetArrowDirection(false);
                    break;
                case 22:
                    if (currentArrowGroup == 22)
                    {
                        currentArrowGroup = 0;
                        return;
                    }
                    currentArrowGroup = 22;
                    arrowImages[3].color = studyManager.styleManager.buttonSelectedColor;
                    arrows[1].SetActive(true);
                    arrows[2].SetActive(true);
                    arrows[3].SetActive(true);
                    SetArrowDirection(true);
                    break;
                case 31:
                    if (currentArrowGroup == 31)
                    {
                        currentArrowGroup = 0;
                        return;
                    }
                    currentArrowGroup = 31;
                    arrowImages[4].color = studyManager.styleManager.buttonSelectedColor;
                    arrows[3].SetActive(true);
                    arrows[4].SetActive(true);
                    arrows[5].SetActive(true);
                    SetArrowDirection(false);
                    break;
                case 32:
                    if (currentArrowGroup == 32)
                    {
                        currentArrowGroup = 0;
                        return;
                    }
                    currentArrowGroup = 32;
                    arrowImages[5].color = studyManager.styleManager.buttonSelectedColor;
                    arrows[3].SetActive(true);
                    arrows[4].SetActive(true);
                    arrows[5].SetActive(true);
                    SetArrowDirection(true);
                    break;
                case 41:
                    if (currentArrowGroup == 41)
                    {
                        currentArrowGroup = 0;
                        return;
                    }
                    currentArrowGroup = 41;
                    arrowImages[6].color = studyManager.styleManager.buttonSelectedColor;
                    arrows[5].SetActive(true);
                    arrows[6].SetActive(true);
                    arrows[7].SetActive(true);
                    SetArrowDirection(false);
                    break;
                case 42:
                    if (currentArrowGroup == 42)
                    {
                        currentArrowGroup = 0;
                        return;
                    }
                    currentArrowGroup = 42;
                    arrowImages[7].color = studyManager.styleManager.buttonSelectedColor;
                    arrows[5].SetActive(true);
                    arrows[6].SetActive(true);
                    arrows[7].SetActive(true);
                    SetArrowDirection(true);
                    break;
            }
        }

        /// <summary>
        /// Toggles the visibility of the right arrow overlays.
        /// </summary>
        public void ToggleRightArrows()
        {
            if (DoubleClickProtection()) return;
            if (!leftOn)
            {
                rightOn = !rightOn;
                rightArrowOverlays.SetActive(rightOn);
            }
        }

        /// <summary>
        /// Toggles the visibility of the left arrow overlays.
        /// </summary>
        public void ToggleLeftArrows()
        {
            if (DoubleClickProtection()) return;
            if (!rightOn)
            {
                leftOn = !leftOn;
                leftArrowOverlays.SetActive(leftOn);
            }
        }

        /// <summary>
        /// Toggles the visibility of the positive feedback overlay.
        /// </summary>
        public void TogglePositiveFeedback()
        {
            if (DoubleClickProtection()) return;
            if (!neutralOn)
            {
                positiveOn = !positiveOn;
                NotificationOverlayContainer.SetActive(positiveOn);
                DisplayContentContainer.SetActive(!positiveOn);
                PositiveFeedback.SetActive(positiveOn);
                if (!positiveOn)
                {
                    studyManager.CancelInvoke("FlashHighlightRise");
                    studyManager.CancelInvoke("FlashHighlightFall");
                    studyManager.fade = 0;
                    studyManager.InvokeRepeating("FlashHighlightRise", 0f, 0.025f);
                }
                else
                {
                    studyManager.CancelInvoke("FlashHighlightRise");
                    studyManager.CancelInvoke("FlashHighlightFall");
                    studyManager.fade = 0;
                }
                PositiveFeedbackButton.color = positiveOn ? studyManager.styleManager.buttonSelectedColor : Color.white;
            }
        }

        /// <summary>
        /// Toggles the visibility of the neutral feedback overlay.
        /// </summary>
        public void ToggleNeutralFeedback()
        {
            if (DoubleClickProtection()) return;
            if (!positiveOn)
            {
                neutralOn = !neutralOn;
                NotificationOverlayContainer.SetActive(neutralOn);
                NeutralFeedback.SetActive(neutralOn);
                DisplayContentContainer.SetActive(!neutralOn);
                if (!neutralOn)
                {
                    studyManager.CancelInvoke("FlashHighlightRise");
                    studyManager.CancelInvoke("FlashHighlightFall");
                    studyManager.fade = 0;
                    studyManager.InvokeRepeating("FlashHighlightRise", 0f, 0.025f);
                }
                else
                {
                    studyManager.CancelInvoke("FlashHighlightRise");
                    studyManager.CancelInvoke("FlashHighlightFall");
                    studyManager.fade = 0;
                }
                NeutralFeedbackButton.color = neutralOn ? studyManager.styleManager.buttonSelectedColor : Color.white;
            }
        }

        /// <summary>
        /// Simulates heart rate changes for testing purposes.
        /// </summary>
        private void HeartRateOverride()
        {
            float rand = Random.value;

            if (rand <= 0.33) return;
            if (rand > 0.33)
            {
                if (rand <= 0.66)
                    overrideHR = Mathf.Max(69, overrideHR - (int)Random.Range(1f, 4f));
                else
                    overrideHR = Mathf.Min(96, overrideHR + (int)Random.Range(1f, 4f));
            }
            heartRateText.text = "" + overrideHR;
        }

        /// <summary>
        /// Updates the heart rate text based on the HREventArgs.
        /// </summary>
        public void UpdateHeartRateText(object sender, HREventArgs e)
        {
            if (hrOverride) return;
            string s = "";
            string h = "";

            if (e.status == HrData.ConnectionStatus.Connected)
            {
                s = "Connected";
                h = "?";
            }
            else if (e.status == HrData.ConnectionStatus.Disconnected)
            {
                s = "Disconnected";
                heartRateText.text = "-";
            }
            else if (e.status == HrData.ConnectionStatus.DataAvailable)
            {
                s = "Receiving Data";
                h = e.hr + "";
            }

            heartRateText.text = h;
        }

        /// <summary>
        /// Increases the tracker channel.
        /// </summary>
        public void TrackerChannelUp()
        {
            if (tracker == null)
                tracker = trackerBehaviour.gameObject.AddComponent<SteamVR_TrackedObject>();
            Destroy(trackerBehaviour);
            tracker.enabled = true;
            if (trackerBehaviour != null)
            {
                Destroy(trackerBehaviour);
                trackerBehaviour.enabled = false;
                trackerBehaviour = null;
            }
            if (tracker != null)
                tracker.index = (SteamVR_TrackedObject.EIndex)Mathf.Max(1, (((int)tracker.index) + 1) % 17);
        }

        /// <summary>
        /// Decreases the tracker channel.
        /// </summary>
        public void TrackerChannelDown()
        {
            if (tracker == null)
                tracker = trackerBehaviour.gameObject.AddComponent<SteamVR_TrackedObject>();
            tracker.enabled = true;
            if (trackerBehaviour != null)
            {
                Destroy(trackerBehaviour);
                trackerBehaviour.enabled = false;
                trackerBehaviour = null;
            }
            if (tracker != null)
                tracker.index = (SteamVR_TrackedObject.EIndex)Mathf.Max(1, (((int)tracker.index) - 1) % 17);
        }

        #endregion
    }
}
