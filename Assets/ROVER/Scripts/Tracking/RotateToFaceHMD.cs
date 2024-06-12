using UnityEngine;
using ROVER.Overlay;

namespace ROVER
{
    public class RotateToFaceHMD : MonoBehaviour
    {
        public bool debug = true;
        public Transform headset;
        public bool yOnly = false;
        public bool activated = true;
        // Update is called once per frame
        void Update()
        {
            if (!activated)
                return;
            if (debug)
                return;

            if (!yOnly)
                OverlayUtilities.TurnToHmd(transform);
            else
            {
                transform.LookAt(headset);
                transform.localRotation = Quaternion.Euler(0f, transform.localRotation.eulerAngles.y, 0f);
            }
        }

        public void ToggleActivate()
        {
            activated = !activated;
        }

    }
}

