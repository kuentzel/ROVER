using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

namespace ROVER
{
public enum OVRTrackedDeviceType
{
    HMD,
    Controller,
    Tracker,
}

public enum OVRTrackedDeviceRole
{
    LeftHand,
    RightHand,
}

public class OVRTrackedDevice : MonoBehaviour
{
    public OVRTrackedDeviceType type;
    public OVRTrackedDeviceRole role;

    // Update is called once per frame
    void Update()
    {
        if (OpenVR.System == null)
        {
            return;
        }

        TrackedDevicePose_t[] poses = new TrackedDevicePose_t[OpenVR.k_unMaxTrackedDeviceCount];

        uint deviceIndex = OpenVR.k_unTrackedDeviceIndexInvalid;

        //Set the device index based on the type
        if (type == OVRTrackedDeviceType.HMD)
        {
            deviceIndex = OpenVR.k_unTrackedDeviceIndex_Hmd;
        }
        else if (type == OVRTrackedDeviceType.Controller)
        {
        ETrackedControllerRole trackedRole = (role == OVRTrackedDeviceRole.LeftHand) ? ETrackedControllerRole.LeftHand : ETrackedControllerRole.RightHand;

        deviceIndex = OpenVR.System.GetTrackedDeviceIndexForControllerRole(trackedRole);
        }
        else if (type == OVRTrackedDeviceType.Tracker)
        {
        // Find the first device that is not a base station, controller, or HMD
        for (uint i = 0; i < OpenVR.k_unMaxTrackedDeviceCount; i++)
        {
            ETrackedDeviceClass deviceClass = OpenVR.System.GetTrackedDeviceClass(i);
            if (deviceClass != ETrackedDeviceClass.Controller &&
                deviceClass != ETrackedDeviceClass.HMD &&
                deviceClass != ETrackedDeviceClass.TrackingReference &&
                deviceClass == ETrackedDeviceClass.GenericTracker)
            {
                deviceIndex = i;
                break;
            }
        }
        }
        //Get the pose for the device
        if (deviceIndex != OpenVR.k_unTrackedDeviceIndexInvalid)
        {            
            OpenVR.System.GetDeviceToAbsoluteTrackingPose(ETrackingUniverseOrigin.TrackingUniverseStanding, 0, poses);

            TrackedDevicePose_t pose = poses[deviceIndex];
            if (pose.bDeviceIsConnected && pose.bPoseIsValid)
            {
                // Use Valve's RigidTransform conversion for OpenVR/SteamVR
                SteamVR_Utils.RigidTransform rigidTransform = new SteamVR_Utils.RigidTransform(pose.mDeviceToAbsoluteTracking);

                transform.position = rigidTransform.pos;
                transform.rotation = rigidTransform.rot;
            }
        }

    }
}
}
