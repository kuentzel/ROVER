using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ROVER.Overlay;
using ReadyPlayerMe.Core;

namespace ROVER.Avatar
{
    public enum VoiceHandling
    {
        None,
        RPM,
        Custom
    }

    public enum EyeHandling
    {
        None,
        RPM,
        Custom
    }

    public class AvatarManager : MonoBehaviour
    {
        // Public fields
        public StudyManager studyManager;
        public Transform avatarTracker;
        public OverlayTextureRenderer avatarOverlay;
        public Transform headset;
        public bool following;
        public GameObject[] avatars;
        public UnityEngine.UI.Image ToggleButton;
        public UnityEngine.UI.Image MicButton;
        public UnityEngine.UI.Image cover;
        public Transform positionFallback;
        public Transform avatarAnchor;
        public List<GameObject> avatarList;
        public EyeHandling eyeAnimation;
        public VoiceHandling voiceAnimation;

        // Private fields
        private bool toggledAvatar;
        private int currentAvatar;
        private bool doubleClickProtected;
        private bool avatarsLoaded;
        private int avatarIndex;
        private int avatarCount = 0;
        private bool voiceActive = true;
        private bool loadLocked = true;

        /// <summary>
        /// Cleans up when the AvatarManager is destroyed.
        /// </summary>
        private void OnDestroy()
        {
            StopAllCoroutines();
            if (avatarList != null)
            {
                foreach (GameObject avatar in avatarList)
                {
                    Destroy(avatar);
                }
                avatarList.Clear();
                avatarList = null;
            }
        }

        /// <summary>
        /// Coroutine to load avatars from a set of URLs.
        /// </summary>
        /// <param name="urlSet">A set of URLs to load avatars from.</param>
        /// <returns>IEnumerator for coroutine.</returns>
        public IEnumerator LoadAvatars(HashSet<string> urlSet)
        {
            avatarCount = urlSet.Count;
            loadLocked = true;

            // Deactivate and destroy existing avatars
            foreach (GameObject go in avatarList)
            {
                go.SetActive(false);
                Destroy(go);
            }
            avatarList = new List<GameObject>();

            bool loading = false;

            foreach (var url in urlSet)
            {
                loading = true;
                var loader = new AvatarObjectLoader(true);

                // Setup the loader's OnCompleted event
                loader.OnCompleted += (sender, args) =>
                {
                    loading = false;
                    AvatarAnimatorHelper.SetupAnimator(args.Metadata.BodyType, args.Avatar);
                    OnAvatarLoaded(args.Avatar);
                };
                loader.LoadAvatar(url);

                yield return new WaitUntil(() => !loading);
            }
        }

        /// <summary>
        /// Handles the event when an avatar is loaded.
        /// </summary>
        /// <param name="avatar">The loaded avatar GameObject.</param>
        private void OnAvatarLoaded(GameObject avatar)
        {
            if (avatarList != null)
            {
                // Handle eye animations
                if (eyeAnimation == EyeHandling.RPM)
                    avatar.AddComponent<EyeAnimationHandler>();
                else if (eyeAnimation == EyeHandling.None && avatar.GetComponent<EyeAnimationHandler>() != null)
                    Destroy(avatar.GetComponent<EyeAnimationHandler>());

                // Handle voice animations
                if (voiceAnimation == VoiceHandling.RPM)
                    avatar.AddComponent<VoiceHandler>();
                else if (voiceAnimation == VoiceHandling.None && avatar.GetComponent<VoiceHandler>() != null)
                    Destroy(avatar.GetComponent<VoiceHandler>());
                else if (voiceAnimation == VoiceHandling.Custom)
                {
                    FaceController fc = avatar.AddComponent<FaceController>();
                    fc.speaking = voiceActive;

                    // Setup face controller blend shapes
                    foreach (SkinnedMeshRenderer smr in avatar.GetComponentsInChildren<SkinnedMeshRenderer>())
                    {
                        if (smr.gameObject.name == "Renderer_Head")
                        {
                            fc.headsmr = smr;
                            smr.SetBlendShapeWeight(1, 0.1f);
                        }
                        if (smr.gameObject.name == "Renderer_Teeth")
                        {
                            fc.teethsmr = smr;
                            smr.SetBlendShapeWeight(1, 0.1f);
                        }
                    }
                }

                avatarList.Add(avatar);
                avatar.transform.parent = avatarAnchor;
                avatar.transform.localPosition = Vector3.zero;
                avatar.transform.localRotation = Quaternion.identity;
                avatar.GetComponent<Animator>().enabled = false;

                if (avatarList[0] != avatar)
                {
                    avatar.SetActive(false);
                }

                if (avatarList.Count == avatarCount)
                {
                    loadLocked = false;
                    avatarsLoaded = true;
                }

                // Set avatar layers
                var children = avatar.GetComponentsInChildren<Transform>(includeInactive: true);
                foreach (var child in children)
                {
                    child.gameObject.layer = LayerMask.NameToLayer("ROVERAvatar");
                }

                if (avatar.GetComponent<AvatarData>().AvatarMetadata.OutfitGender == OutfitGender.Feminine)
                    avatar.transform.localPosition = new Vector3(0f, 0.1f, 0f);

                avatar.layer = LayerMask.NameToLayer("ROVERAvatar");
            }
            else
            {
                DestroyImmediate(avatar);
            }
        }

        /// <summary>
        /// Updates the AvatarManager each frame.
        /// </summary>
        private void Update()
        {
            // Update the position of the AvatarManager based on the avatarTracker
            if (!avatarTracker.gameObject.activeInHierarchy || avatarTracker.position == Vector3.zero)
            {
                transform.position = positionFallback.position;
            }
            else
            {
                transform.position = new Vector3(avatarTracker.position.x, 1.65f, avatarTracker.position.z);
            }

            // Make the AvatarManager look at the headset if following is enabled
            if (following)
                transform.LookAt(headset.position);
        }

        /// <summary>
        /// Cycles through the available avatars.
        /// </summary>
        public void CycleAvatars()
        {
            if (avatarsLoaded)
            {
                // Deactivate all avatars
                foreach (GameObject avatar in avatarList)
                {
                    avatar.SetActive(false);
                }

                // Activate the next avatar in the list
                avatarIndex = (avatarIndex + 1) % avatarList.Count;
                avatarList[avatarIndex].SetActive(true);

                // Handle voice animation for the selected avatar
                if (voiceAnimation == VoiceHandling.RPM)
                    avatarList[avatarIndex].GetComponent<VoiceHandler>().enabled = voiceActive;
                else if (voiceAnimation == VoiceHandling.Custom)
                {
                    FaceController fc = avatarList[avatarIndex].GetComponent<FaceController>();
                    fc.ToggleMicrophone(voiceActive);
                    fc.headsmr.SetBlendShapeWeight(0, 0f);
                }
            }
            else
            {
                // Cycle through legacy avatars if new avatars are not loaded
                if (avatars[currentAvatar] != null)
                {
                    FaceController fc = avatars[currentAvatar].GetComponentInParent<FaceController>();
                    if (fc != null)
                        fc.ToggleMicrophone();
                    avatars[currentAvatar].SetActive(false);
                }

                currentAvatar = (currentAvatar + 1) % avatars.Length;

                if (avatars[currentAvatar] != null)
                {
                    avatars[currentAvatar].SetActive(true);
                    FaceController fc = avatars[currentAvatar].GetComponentInParent<FaceController>();
                    if (fc != null)
                        fc.ToggleMicrophone();
                }
            }
        }

        /// <summary>
        /// Toggles the lip sync functionality.
        /// </summary>
        public void ToggleLips()
        {
            if (loadLocked || DoubleClickProtection())
                return;

            voiceActive = !voiceActive;

            // Toggle voice animation for the current avatar
            if (voiceAnimation == VoiceHandling.RPM)
                avatarList[avatarIndex].GetComponent<VoiceHandler>().enabled = voiceActive;
            else if (voiceAnimation == VoiceHandling.Custom)
            {
                FaceController fc = avatarsLoaded
                    ? avatarList[avatarIndex].GetComponent<FaceController>()
                    : avatars[currentAvatar].GetComponentInParent<FaceController>();
                fc?.ToggleMicrophone();
            }

            // Update the microphone button color
            MicButton.color = voiceActive ? studyManager.styleManager.buttonSelectedColor : Color.white;
        }

        /// <summary>
        /// Protects against double clicks.
        /// </summary>
        /// <returns>True if double click protection is active, otherwise false.</returns>
        private bool DoubleClickProtection()
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
        /// Resets double click protection.
        /// </summary>
        private void DelayDoubleClick()
        {
            doubleClickProtected = false;
        }

        /// <summary>
        /// Toggles the avatar display on and off.
        /// </summary>
        public void ToggleAvatar()
        {
            if (loadLocked || DoubleClickProtection())
                return;

            toggledAvatar = !toggledAvatar;

            // Update the toggle button color and overlay visibility
            ToggleButton.color = toggledAvatar ? studyManager.styleManager.buttonSelectedColor : Color.white;
            cover.gameObject.SetActive(!toggledAvatar);
            avatarTracker.gameObject.SetActive(toggledAvatar);
            avatarOverlay.gameObject.SetActive(toggledAvatar);
        }
    }
}

