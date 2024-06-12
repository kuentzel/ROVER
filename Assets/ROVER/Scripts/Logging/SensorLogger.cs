using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using static HrData;

namespace ROVER.Sensors
{
    /// <summary>
    /// Event arguments for heart rate events.
    /// </summary>
    public class HREventArgs : EventArgs
    {
        public HREventArgs(ConnectionStatus status, int hr, DateTime timestamp)
        {
            this.status = status;
            this.hr = hr;
            Timestamp = timestamp;
        }

        public ConnectionStatus status;
        public int hr;
        public DateTime Timestamp { get; set; }
    }

    /// <summary>
    /// Delegate for heart rate event handlers.
    /// </summary>
    public delegate void HREventHandler(object sender, HREventArgs e);

    /// <summary>
    /// Logs heart rate sensor data and raises events for heart rate changes.
    /// </summary>
    public class SensorLogger : MonoBehaviour
    {
        [Header("Settings")]
        public bool debug = false;
        public bool isPolling = true;

        [Header("Managers")]
        public StudyManager studyManager;
        public HeartRateController hrController;


        private StreamWriter activityWriter;
        private int previousHR;
        private bool pollNow = true;
        private string vpn;
        private bool isLogging;
        private int currentHR = -1;
        private ConnectionStatus currentStatus = ConnectionStatus.Disconnected;

        public event HREventHandler HREvent;
        public HrData hrData { get; private set; }

        #region Native calls 

        [DllImport("HeartRatePlugin", CallingConvention = CallingConvention.Cdecl)]
        private static extern IntPtr GetData();

        [DllImport("HeartRatePlugin", CallingConvention = CallingConvention.Cdecl)]
        private static extern void Disconnect();

        #endregion

        /// <summary>
        /// Cleans up resources when the object is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            Disconnect();
            if (activityWriter != null)
            {
                activityWriter.Flush();
                activityWriter.Close();
            }
        }

        /// <summary>
        /// Updates the heart rate data and logs changes.
        /// </summary>
        private void Update()
        {
            if (hrData == null)
            {
                hrData = hrController.HrData;
            }
            else
            {
                string s = DateTime.UtcNow.ToString("HH:mm:ss:fff") + ";HeartRate";
                currentStatus = hrController.HrData.CurrentConnection;
                previousHR = currentHR;

                switch (currentStatus)
                {
                    case ConnectionStatus.Connected:
                        currentHR = 0;
                        s += ";Connected;?";
                        break;
                    case ConnectionStatus.Disconnected:
                        currentHR = -1;
                        s += ";Disconnected;-";
                        break;
                    case ConnectionStatus.DataAvailable:
                        currentHR = hrController.HrData.HeartRate;
                        s += ";Available;" + currentHR;
                        break;
                }

                if (currentHR != previousHR || (isPolling && pollNow))
                {
                    pollNow = false;
                    HREvent?.Invoke(this, new HREventArgs(currentStatus, currentHR, DateTime.UtcNow));

                    if (isLogging && activityWriter != null)
                    {
                        activityWriter.WriteLine(s);
                        activityWriter.Flush();
                    }

                    CancelInvoke("NextPolling");
                    Invoke("NextPolling", 0.25f);
                }
            }
        }

        /// <summary>
        /// Enables the next polling cycle.
        /// </summary>
        void NextPolling()
        {
            pollNow = true;
        }

        /// <summary>
        /// Generates the file path for the sensor log.
        /// </summary>
        /// <returns>The generated file path.</returns>
        private string GenerateFilePath()
        {
            string directory = $"{Application.dataPath}/Export/{vpn}_{studyManager.SessionStartTimeString}";

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            return $"{directory}/{vpn}_{studyManager.SessionStartTimeString}_SensorLog.csv";
        }

        /// <summary>
        /// Starts logging heart rate data.
        /// </summary>
        /// <param name="vpn">The VPN identifier.</param>
        public void StartLogging(string vpn)
        {
            if (isLogging || !gameObject.activeInHierarchy)
                return;

            this.vpn = vpn;
            isLogging = true;
            activityWriter = new StreamWriter(GenerateFilePath());

            activityWriter.WriteLine($"VPN:;{vpn};Session:;{studyManager.SessionStartTimeString}");
            activityWriter.WriteLine();
            activityWriter.WriteLine("Time;System;Type;Detail");
            activityWriter.Flush();

            Debug.Log("Started Sensor Log");
        }

        /// <summary>
        /// Stops logging heart rate data.
        /// </summary>
        public void StopLogging()
        {
            if (!isLogging)
                return;

            isLogging = false;

            if (activityWriter != null)
            {
                activityWriter.Flush();
                activityWriter.Close();
            }
        }
    }
}
