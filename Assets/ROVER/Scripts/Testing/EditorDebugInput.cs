using ROVER;
using UnityEngine;
using Valve.VR;

public class EditorDebugInput : MonoBehaviour
{
    public SteamVR_Action_Boolean nextSlideAction;
    public SteamVR_Action_Boolean previousSlideAction;

    public StudyManager studyManager;

#if UNITY_EDITOR
    // Update is called once per frame

    private void Start()
    {
        nextSlideAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("EditorDebugNextSlide");
        previousSlideAction = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("EditorDebugPreviousSlide");
}
    void Update()
    {
        if (nextSlideAction.GetStateDown(SteamVR_Input_Sources.LeftHand) || nextSlideAction.GetStateDown(SteamVR_Input_Sources.RightHand))
            studyManager.DisplayNext();
        else if (previousSlideAction.GetStateDown(SteamVR_Input_Sources.LeftHand) || previousSlideAction.GetStateDown(SteamVR_Input_Sources.RightHand))
            studyManager.BackOneStep();
    }
#endif
}
