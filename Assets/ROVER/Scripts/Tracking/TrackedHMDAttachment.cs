using ROVER.Overlay;
using UnityEngine;

namespace ROVER
{
    public class TrackedHMDAttachment : MonoBehaviour
    {
        public static TrackedHMDAttachment _instance;
        public static TrackedHMDAttachment instance
        {
            get
            {
                return OverlayUtilities.Singleton(ref _instance, "[HMD]");
            }
        }

        public static Transform Transform
        {
            get
            {
                return instance.transform;
            }
        }
    }
}
