using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ROVER.Sensors
{
    /// <summary>
    /// Displays heart rate data on a UI Text element.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class HeartRateDisplay : MonoBehaviour
    {
        [Header("Heart Rate Controller")]
        [Tooltip("The controller we read the connection data from.")]
        public HeartRateController m_controller;

        private Text text;

        /// <summary>
        /// Initializes the Text component.
        /// </summary>
        void Start()
        {
            text = GetComponent<Text>();
        }

        /// <summary>
        /// Updates the heart rate display with the latest data from the controller.
        /// </summary>
        void Update()
        {
            // Retrieve the latest heart rate data
            HrData hrData = m_controller.HrData;
            // Build the display string based on the connection status and available data
            string finalString = BuildDisplayString(hrData);
            // Update the text component with the new string
            text.text = finalString;
        }

        /// <summary>
        /// Builds the display string based on the heart rate data.
        /// </summary>
        /// <param name="hrData">The heart rate data.</param>
        /// <returns>The formatted display string.</returns>
        private string BuildDisplayString(HrData hrData)
        {
            string finalString = "Connection status: " + hrData.CurrentConnection + "\n";

            if (hrData.CurrentConnection == HrData.ConnectionStatus.DataAvailable)
            {
                finalString += "Milliseconds passed: " + hrData.MillisecondsSinceFirstReading + "\n";
                finalString += "Sensor contact: " + hrData.CurrentSensorContact + "\n";

                if (hrData.EnergyAvailable)
                {
                    finalString += "Energy: " + hrData.EnergyUsed + "\n";
                }

                finalString += "Heart Rate: " + hrData.HeartRate + "\n";

                if (hrData.RrIntervalAvailable)
                {
                    finalString += "RR Intervals:";

                    foreach (int interval in hrData.RrIntervals)
                    {
                        finalString += " " + interval;
                    }

                    finalString += "\n";
                }
            }

            return finalString;
        }
    }
}
