using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace ROVER.Sensors
{
    /// <summary>
    /// Enum representing the target state of the heart rate.
    /// </summary>
    public enum HR_TargetState
    {
        Unknown,
        ApproachFromBottom,
        ApproachFromTop,
        Hold,
        BackFromTop,
        BackFromBottom,
        Danger
    }

    /// <summary>
    /// Event that triggers when the heart rate widget changes.
    /// </summary>
    [Serializable]
    public class WidgetChangedEvent : UnityEvent<HeartRateWidgetDemo> { }

    /// <summary>
    /// Class representing the heart rate widget demo.
    /// </summary>
    public class HeartRateWidgetDemo : MonoBehaviour
    {
        [Header("UI Elements")]
        public Image[] indicators;
        public Image heartRateIcon;
        public TextMeshProUGUI targetHRdisplayTop;
        public TextMeshProUGUI targetHRdisplayBot;
        public TextMeshProUGUI hrText;
        public TMP_InputField target_field;

        [Header("Heart Rate Settings")]
        public int targetHR;
        public bool hrOverride = false;
        public int overrideHR = 77;

        [Header("Miscellaneous")]
        public bool debug = false;
        public Transform watchContainer;
        public SensorLogger sLogger;
        public WidgetChangedEvent OnDisplayChanged;

        private bool targetReached;
        private HR_TargetState currenState;
        private HR_TargetState newState;
        private int sensorHR;

        /// <summary>
        /// Initializes the heart rate widget demo.
        /// </summary>
        void Start()
        {
            Init();
        }

        /// <summary>
        /// Initializes the heart rate widget demo.
        /// </summary>
        void Init()
        {
            if (!hrOverride)
            {
                sLogger.HREvent += UpdateHeartRate;
            }
        }

        /// <summary>
        /// Cleans up resources when the object is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (!hrOverride)
            {
                sLogger.HREvent -= UpdateHeartRate;
            }
        }

        /// <summary>
        /// Updates the heart rate widget based on the heart rate event data.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The heart rate event arguments.</param>
        void UpdateHeartRate(object sender, HREventArgs e)
        {
            if (hrOverride)
            {
                return;
            }

            string statusMessage = GetStatusMessage(e.status);
            bool dataAvailable = e.status == HrData.ConnectionStatus.DataAvailable;
            int heartRate = dataAvailable ? e.hr : 0;

            ChangeDisplay(heartRate, dataAvailable);

            if (debug)
            {
                Debug.Log($"HR Status: {statusMessage}, HR: {heartRate}");
            }
        }

        /// <summary>
        /// Gets the status message based on the heart rate connection status.
        /// </summary>
        /// <param name="status">The heart rate connection status.</param>
        /// <returns>The status message.</returns>
        private string GetStatusMessage(HrData.ConnectionStatus status)
        {
            switch (status)
            {
                case HrData.ConnectionStatus.Connected:
                    return "Connected";
                case HrData.ConnectionStatus.Disconnected:
                    return "Disconnected";
                case HrData.ConnectionStatus.DataAvailable:
                    return "Receiving Data";
                default:
                    return "Unknown Status";
            }
        }

        /// <summary>
        /// Changes the display based on the heart rate and data availability.
        /// </summary>
        /// <param name="heartRate">The heart rate.</param>
        /// <param name="dataAvailable">Whether data is available.</param>
        private void ChangeDisplay(int heartRate, bool dataAvailable)
        {
            sensorHR = heartRate;
            CheckTargetState(dataAvailable);

            if (newState != currenState)
            {
                currenState = newState;
                UpdateDisplayBasedOnState(currenState);
            }

            hrText.text = $"{sensorHR}";
            OnDisplayChanged.Invoke(this);

            if (debug)
            {
                Debug.Log($"Displayed HR: {sensorHR} BPM");
            }
        }

        /// <summary>
        /// Updates the display based on the current heart rate target state.
        /// </summary>
        /// <param name="state">The current heart rate target state.</param>
        private void UpdateDisplayBasedOnState(HR_TargetState state)
        {
            ResetIndicators();
            Color32 color;

            switch (state)
            {
                case HR_TargetState.Unknown:
                case HR_TargetState.Hold:
                    color = new Color32(255, 255, 225, 255);
                    break;
                case HR_TargetState.ApproachFromBottom:
                case HR_TargetState.ApproachFromTop:
                    color = new Color32(0, 255, 0, 255);
                    break;
                case HR_TargetState.BackFromBottom:
                case HR_TargetState.BackFromTop:
                    color = new Color32(255, 255, 0, 255);
                    break;
                case HR_TargetState.Danger:
                    color = new Color32(255, 0, 0, 255);
                    break;
                default:
                    color = new Color32(255, 255, 225, 255);
                    break;
            }

            heartRateIcon.color = color;

            switch (state)
            {
                case HR_TargetState.ApproachFromBottom:
                    indicators[0].gameObject.SetActive(true);
                    indicators[0].color = color;
                    break;
                case HR_TargetState.ApproachFromTop:
                    indicators[2].gameObject.SetActive(true);
                    indicators[2].color = color;
                    break;
                case HR_TargetState.Hold:
                    indicators[1].gameObject.SetActive(true);
                    indicators[1].color = color;
                    break;
                case HR_TargetState.BackFromBottom:
                    indicators[0].gameObject.SetActive(true);
                    indicators[0].color = color;
                    break;
                case HR_TargetState.BackFromTop:
                    indicators[2].gameObject.SetActive(true);
                    indicators[2].color = color;
                    break;
                case HR_TargetState.Danger:
                    indicators[2].gameObject.SetActive(true);
                    indicators[2].color = color;
                    break;
                default:
                    break;
            }

            ShowTargetHRDisplay(state);
        }

        /// <summary>
        /// Shows or hides the target heart rate display based on the current state.
        /// </summary>
        /// <param name="state">The current heart rate target state.</param>
        private void ShowTargetHRDisplay(HR_TargetState state)
        {
            bool showTop = state == HR_TargetState.BackFromBottom || state == HR_TargetState.ApproachFromBottom;
            bool showBottom = !showTop && state != HR_TargetState.Unknown && state != HR_TargetState.Hold;

            targetHRdisplayBot.gameObject.SetActive(showBottom);
            targetHRdisplayTop.gameObject.SetActive(showTop);
        }

        /// <summary>
        /// Resets all indicator colors and disables their game objects.
        /// </summary>
        private void ResetIndicators()
        {
            foreach (var indicator in indicators)
            {
                indicator.color = new Color32(255, 255, 225, 100);
                indicator.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Checks and updates the current heart rate target state based on availability.
        /// </summary>
        /// <param name="availability">Whether data is available.</param>
        private void CheckTargetState(bool availability)
        {
            if (!availability)
            {
                newState = HR_TargetState.Unknown;
                return;
            }

            int HRdiff = sensorHR - targetHR;
            targetReached = Mathf.Abs(HRdiff) <= 5;

            if (targetReached)
            {
                if (HRdiff < -5)
                {
                    newState = HR_TargetState.BackFromBottom;
                }
                else if (HRdiff >= 20)
                {
                    newState = HR_TargetState.Danger;
                }
                else if (HRdiff > 10)
                {
                    newState = HR_TargetState.BackFromTop;
                }
                else
                {
                    newState = HR_TargetState.Hold;
                }
            }
            else if (HRdiff >= 20)
            {
                newState = HR_TargetState.Danger;
            }
            else if (HRdiff > 5)
            {
                newState = HR_TargetState.ApproachFromTop;
            }
            else
            {
                newState = HR_TargetState.ApproachFromBottom;
            }

            if (debug)
            {
                Debug.Log($"HR Target State: {newState}");
            }
        }

        /// <summary>
        /// Sets the target heart rate based on the input field value.
        /// </summary>
        public void SetTargetHR()
        {
            if (int.TryParse(target_field.text, out int value) && value >= 50 && value <= 250)
            {
                targetHR = value;
                targetHRdisplayBot.text = targetHR.ToString();
                targetHRdisplayTop.text = targetHR.ToString();
                watchContainer.gameObject.SetActive(true);
                ResetTarget();
                ChangeDisplay(sensorHR, true);

                if (debug)
                {
                    Debug.Log($"Set Target HR: {value}");
                }
            }
            else
            {
                watchContainer.gameObject.SetActive(false);

                if (debug)
                {
                    Debug.Log("Invalid target HR value");
                }
            }
        }

        /// <summary>
        /// Resets the target state.
        /// </summary>
        public void ResetTarget()
        {
            targetReached = false;
            currenState = HR_TargetState.Unknown;

            if (debug)
            {
                Debug.Log("Reset Target State");
            }
        }
    }
}
