# ROVER (Rating Overlay for Virtual Environments in Research)

ROVER is a standalone, non-exclusive SteamVR Overlay Application designed for use with SteamVR as the OpenVR runtime or in OpenXR compatibility mode. Currently, ROVER is tested and supported on Windows 10 only.

IMPORTANT: For input filter functionality to work "Experimental overlay input overrides" must be enabled in the developer section of SteamVR settings.


## Load Study Configuration

You can load a study configuration via a JSON or XML file from the following path: `Application Folder -> ROVER_Data/StreamingAssets/Import`. Enter the filename (not case sensitive) WITHOUT the file extension (.json/.xml) into the ROVER Desktop UI under "Study Filename".

This demo includes:
- A native JSON sample: `rover_sample.json`
- A LimeSurvey queXML sample: `rover_limesurvey_sample.xml`
- A converted JSON sample generated from the LimeSurvey file: `rover_conversion_sample.json`

Review these files to learn how to set up your own configuration. The LimeSurvey sample explains how to use the LimeSurvey UI for ROVER surveys and its limitations. The JSON sample serves as a structure to build your own survey using a text editor or in conjunction with the C# class `StudyTestDataGenerator`.

## Data Export

Before starting the ROVER study, ensure you enter the "Participant ID" in ROVER. The study automatically begins if the tutorial is completed and a valid configuration is loaded.

Make sure your Bluetooth Heart Rate Sensor (BLE GATT HR Profile) is connected before starting ROVER.

Note that the Activity and Sensor Log start before the Participant ID can be entered, so you will need to manually copy the ID from the Study Results file to the Logs. All logs and results of a session are stored together in a folder named using the participant ID and a timestamp.

A sample export is provided in `ROVER_Data/Export`.

## Input

### Virtual Pointers

- The virtual pointers are automatically hidden if the headset is not facing the rating station.
- ROVER pauses and receives no input while the SteamVR Dashboard is open.
- In `ROVER_Data/StreamingAssets/Configuration/MainConfiguration.json`, you can specify whether ROVER should filter input to the main scene application when the virtual pointers are showing.

IMPORTANT: "Experimental overlay input overrides" must be enabled in the developer section of SteamVR settings.

If the virtual pointers are not attached to the controllers or are stuck in the floor, follow these steps:

1. Ensure the action set of ROVER is bound to your controller inputs. Override `ROVER_Data/StreamingAssets/SteamVR/action.json` and the binding file for your controller with the appropriate files from the Backup folder to restore the selection of official default bindings.
2. In SteamVR, go to Settings -> Controllers -> Manage Controller Bindings / Show Binding UI.
3. Select ROVER / ROVER [Testing].
4. Select your controller type and wait for the binding to load.
5. In the "Manage Controller Bindings" Pop-Up, select custom and "Edit this Binding". Alternatively, in the "Show Binding UI Menu", select "Edit" on the Current Binding or "Create a New Binding".
6. Ensure "Mirror Mode" is selected.
7. Assign "haptics" to "Left/Right Hand Haptics" under "Haptics".
8. Assign "pose" to "Left/Right Hand Raw" under "Poses" if possible. You may need to try different configurations for your controller.
9. Assign the Boolean "InteractUI" Action to a controller button, e.g., the Trigger (as Button Click or Trigger Click).
10. Save the Personal Binding.

To ensure your configuration persists, you may need to be logged into a Steam User Account. When creating a new configuration in SteamVR for your controller type, note that the menu does not allow setting haptic and pose targets on creation. Create your configuration and then edit it to set haptics and pose.

### Virtual Pointer Offset

Edit the default virtual pointer offset for your controller in the `ROVER_Data/StreamingAssets/Configuration` folder and specify the configuration file in `MainConfiguration.json`. ROVER needs to restart to load a new configuration.

## Instructor Features

### Avatar Tracker

1. In SteamVR, go to Settings -> Controllers -> Manage Trackers.
2. Identify your tracker.
3. Assign the tracker role "Keyboard".

If you cannot or do not want to use a Tracker, but a controller or a tracker without the Keyboard role, you can use the "Tracker OVERRIDE" buttons in the UI to cycle through the tracked devices (all devices except HMD).

### Avatar Lipsyncing

Lipsyncing binds to the default microphone on application start. On Windows, go to the Sound Control Panel, select the "Recording" tab, and right-click to set your desired microphone as the Default Device.

### Heart Rate

If you set a target heart rate above 50 in the UI, ROVER will show a small wrist-attached widget to the user, prompting them to reach the target heart rate and guiding them to stay within -5 to +10 of the target heart rate. Ensure you have a Polar or Garmin BLE Bluetooth sensor connected to your PC to use this feature.

## Other Configuration

Using `MainConfiguration.json` in `ROVER_Data/StreamingAssets/Configuration`, you can configure additional features. The file includes commentary explaining the various settings.

- Set the import mode depending on your import file. In LimeSurvey import mode, ROVER will automatically generate a JSON conversion of your LimeSurvey file.
- Set the export mode. In default mode, ROVER logs user selections. In Mode 1, you can export a lexicographically sorted list of variable IDs and answers manually using the ROVER UI or automatically after the end of the survey.
- Specify which styling file to use. The default styling file exists in the folder, where you can set colors for each component of the rating station.
- Specify which localization file to use. German (de) and English (en_US) localizations exist in the "Localization" folder. Most localizations refer to the tutorial and the title of the hint panel. Other texts depend on your survey file.
- Specify which offset files to use for the left and right controllers.
- Specify whether ROVER should filter input for the main scene application.
- Specify whether the virtual pointer should be colored upon hovering over targets.
- Specify the link for the Ready Player Me avatars created for the instructor avatar feature at rover.readyplayer.me. Create an avatar and copy the link (no need to create an account or log in). Place the link into "link" and separate multiple links with commas, except for the last entry in the list. Follow the structure in the default configuration.
- Specify whether to have the avatar blinking and lipsyncing.
- Specify whether to use the heart rate sensor feature.
- Set height and rotation offsets for the whole rating station.