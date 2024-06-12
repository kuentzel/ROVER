using ROVER;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ROVER
{
    /// <summary>
    /// Logs activity events such as study progress and touch button interactions.
    /// </summary>
    public class ActivityLogger : MonoBehaviour
    {
        [Header("Settings")]
        public bool debug = false;

        [Header("Managers")]
        public StudyManager studyManager;

        private bool initialized = false;
        private StreamWriter activityWriter;
        private bool isLogging = false;
        private string vpn;

        /// <summary>
        /// Initializes the activity logger.
        /// </summary>
        void Start()
        {
            Invoke("Init", 0.05f);
        }

        /// <summary>
        /// Sets up event listeners and initializes logging.
        /// </summary>
        public void Init()
        {
            if (studyManager == null || studyManager.leftPointer == null || studyManager.rightPointer == null)
                return;

            initialized = true;
            studyManager.StudyProgressEvent += StudyManager_StudyProgressEvent;
            studyManager.tutorialManager.TutorialProgressEvent += TutorialManager_TutorialProgressEvent;

            studyManager.leftPointer.TouchButtonAimed += TouchButtonAimed;
            studyManager.leftPointer.TouchButtonHovered += TouchButtonHovered;
            studyManager.leftPointer.TouchButtonCharging += TouchButtonCharging;
            studyManager.leftPointer.TouchButtonCommitted += TouchButtonCommitted;
            studyManager.leftPointer.TouchButtonLost += TouchButtonLost;
            studyManager.leftPointer.TouchButtonSelected += TouchButtonSelected;

            studyManager.rightPointer.TouchButtonAimed += TouchButtonAimed;
            studyManager.rightPointer.TouchButtonHovered += TouchButtonHovered;
            studyManager.rightPointer.TouchButtonCharging += TouchButtonCharging;
            studyManager.rightPointer.TouchButtonCommitted += TouchButtonCommitted;
            studyManager.rightPointer.TouchButtonLost += TouchButtonLost;
            studyManager.rightPointer.TouchButtonSelected += TouchButtonSelected;
        }

        /// <summary>
        /// Cleans up resources when the object is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            if (activityWriter != null)
            {
                activityWriter.Flush();
                activityWriter.Close();
            }

            studyManager.StudyProgressEvent -= StudyManager_StudyProgressEvent;
            studyManager.tutorialManager.TutorialProgressEvent -= TutorialManager_TutorialProgressEvent;

            studyManager.leftPointer.TouchButtonAimed -= TouchButtonAimed;
            studyManager.leftPointer.TouchButtonHovered -= TouchButtonHovered;
            studyManager.leftPointer.TouchButtonCharging -= TouchButtonCharging;
            studyManager.leftPointer.TouchButtonCommitted -= TouchButtonCommitted;
            studyManager.leftPointer.TouchButtonLost -= TouchButtonLost;
            studyManager.leftPointer.TouchButtonSelected -= TouchButtonSelected;

            studyManager.rightPointer.TouchButtonAimed -= TouchButtonAimed;
            studyManager.rightPointer.TouchButtonHovered -= TouchButtonHovered;
            studyManager.rightPointer.TouchButtonCharging -= TouchButtonCharging;
            studyManager.rightPointer.TouchButtonCommitted -= TouchButtonCommitted;
            studyManager.rightPointer.TouchButtonLost -= TouchButtonLost;
            studyManager.rightPointer.TouchButtonSelected -= TouchButtonSelected;
        }

        /// <summary>
        /// Handles study progress events and logs them.
        /// </summary>
        private void StudyManager_StudyProgressEvent(object sender, StudyEventArgs e)
        {
            if (activityWriter == null)
                return;

            string log = $"{e.Timestamp:HH:mm:ss:fff};Study";

            switch (e.eventType)
            {
                case StudyEventType.Start:
                    log += ";Start";
                    break;
                case StudyEventType.End:
                    log += ";End";
                    break;
                case StudyEventType.Step:
                    log += ";Step";
                    if (e.element is Instruction instruction)
                    {
                        log += $";S{instruction.Section.Index}-E{instruction.Index}";
                    }
                    else if (e.element is Item item)
                    {
                        log += $";S{item.ItemSet.Section.Index}-E{item.ItemSet.Index}-I{item.Index}";
                    }
                    break;
            }

            if (debug)
                Debug.Log(log);

            activityWriter.WriteLine(log);
            activityWriter.Flush();
        }

        /// <summary>
        /// Handles tutorial progress events and logs them.
        /// </summary>
        private void TutorialManager_TutorialProgressEvent(object sender, TutorialEventArgs e)
        {
            if (activityWriter == null)
                return;

            string log = $"{e.Timestamp:HH:mm:ss:fff};Tutorial;{e.message}";

            if (debug)
                Debug.Log(log);

            activityWriter.WriteLine(log);
            activityWriter.Flush();
        }

        /// <summary>
        /// Logs touch button events.
        /// </summary>
        private void LogTouchButtonEvent(TouchButtonEventArgs e, string type)
        {
            if (activityWriter == null || e.button == null)
                return;

            string log = $"{e.Timestamp:HH:mm:ss:fff};Interaction;{type};{e.button.name} - ";

            if (e.button.name == "OptionScaleButton")
            {
                DisplaySingleChoiceOption dsco = e.button.GetComponentInParent<DisplaySingleChoiceOption>();
                if (dsco != null)
                {
                    log += $"Option {dsco.optionNumber} of Item S{dsco.referencedSCitem.ItemSet.Section.Index}-E{dsco.referencedSCitem.ItemSet.Index}-I{dsco.referencedSCitem.Index} - ";
                }
            }

            log += e.handType switch
            {
                Valve.VR.SteamVR_Input_Sources.LeftHand => "Left Hand - ",
                Valve.VR.SteamVR_Input_Sources.RightHand => "Right Hand - ",
                _ => string.Empty
            };

            log += e.mode switch
            {
                TouchInteractionMode.Trigger => "Trigger",
                TouchInteractionMode.ColliderTime => "Time-Based w Collider",
                TouchInteractionMode.RaycastTime => "Time-Based w Raycast",
                TouchInteractionMode.Proximity => "Proximity-Based w Pointer",
                TouchInteractionMode.ProximitySphere => "Proximity-Based w Sphere",
                _ => string.Empty
            };

            log += $";{e.buttonWorldPos};{e.buttonWorldRot};{e.pointerBaseWorldPos};{e.pointerBaseWorldRot}";

            if (debug)
                Debug.Log(log);

            activityWriter.WriteLine(log);
            activityWriter.Flush();
        }

        private void TouchButtonAimed(object sender, TouchButtonEventArgs e) => LogTouchButtonEvent(e, "Aimed");
        private void TouchButtonHovered(object sender, TouchButtonEventArgs e) => LogTouchButtonEvent(e, "Hovered");
        private void TouchButtonCharging(object sender, TouchButtonEventArgs e) => LogTouchButtonEvent(e, "Charging");
        private void TouchButtonCommitted(object sender, TouchButtonEventArgs e) => LogTouchButtonEvent(e, "Committed");
        private void TouchButtonSelected(object sender, TouchButtonEventArgs e) => LogTouchButtonEvent(e, "Selected");
        private void TouchButtonLost(object sender, TouchButtonEventArgs e) => LogTouchButtonEvent(e, "Lost");

        /// <summary>
        /// Generates the file path for the activity log.
        /// </summary>
        /// <returns>The generated file path.</returns>
        private string GenerateFilePath()
        {
            string directory = Application.dataPath + "/Export/" + vpn + "_" + studyManager.SessionStartTimeString;

            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            return $"{directory}/{vpn}_{studyManager.SessionStartTimeString}_ActivityLog.csv";
        }

        /// <summary>
        /// Starts logging activity events.
        /// </summary>
        /// <param name="vpn">The VPN identifier.</param>
        public void StartLogging(string vpn)
        {
            if (isLogging || !gameObject.activeInHierarchy)
                return;

            this.vpn = vpn;
            activityWriter = new StreamWriter(GenerateFilePath());

            isLogging = true;

            activityWriter.WriteLine($"VPN:;{vpn};Session:;{studyManager.SessionStartTimeString}");
            activityWriter.WriteLine();
            activityWriter.WriteLine("Time;System;Type;Detail;ButtonWorldPos;ButtonWorldRot;PointerBaseWorldPos;PointerBaseWorldRot");
            activityWriter.Flush();

            Debug.Log("Started Activity Log");
        }

        /// <summary>
        /// Stops logging activity events.
        /// </summary>
        public void StopLogging()
        {
            if (isLogging)
            {
                isLogging = false;
                if (activityWriter != null)
                {
                    activityWriter.Flush();
                    activityWriter.Close();
                }
            }
        }
    }
}
