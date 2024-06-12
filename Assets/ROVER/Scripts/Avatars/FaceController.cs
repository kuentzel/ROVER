using UnityEngine;

namespace ROVER.Avatar
{
    public class FaceController : MonoBehaviour
    {
        [Header("Microphone Settings")]
        public float updateStep = 0.1f;
        public int sampleDataLength = 1024;

        [Header("Skinned Mesh Renderers")]
        public SkinnedMeshRenderer headsmr;
        public SkinnedMeshRenderer teethsmr;

        [HideInInspector]
        public bool speaking = false;
        public string currentMic;

        private AudioClip micClip;
        private float currentUpdateTime = 0f;
        private float clipLoudness;
        private float[] clipSampleData;

        /// <summary>
        /// Initializes the FaceController.
        /// </summary>
        void Start()
        {
            clipSampleData = new float[sampleDataLength];

            if (Microphone.devices.Length < 1)
                return;

            Debug.Log("Mic " + Microphone.devices[0]);
            string micName = Microphone.devices[0];
            currentMic = micName;

            if (speaking)
            {
                StartMicrophone(micName);
            }
        }

        /// <summary>
        /// Toggles the microphone on or off.
        /// </summary>
        public void ToggleMicrophone()
        {
            speaking = !speaking;

            if (speaking)
            {
                StartMicrophone(Microphone.devices[0]);
            }
            else
            {
                Microphone.End(currentMic);
            }
        }

        /// <summary>
        /// Toggles the microphone based on the given boolean value.
        /// </summary>
        /// <param name="state">True to start the microphone, false to stop it.</param>
        public void ToggleMicrophone(bool state)
        {
            speaking = state;

            if (speaking)
            {
                StartMicrophone(Microphone.devices[0]);
            }
            else
            {
                Microphone.End(currentMic);
            }
        }

        /// <summary>
        /// Gets the loudness from the microphone.
        /// </summary>
        /// <returns>The loudness value.</returns>
        public float GetLoudnessFromMicrophone()
        {
            return GetLoudnessFromAudioClip(Microphone.GetPosition(Microphone.devices[0]), micClip);
        }

        /// <summary>
        /// Gets the loudness from an audio clip.
        /// </summary>
        /// <param name="clipPosition">The position in the clip.</param>
        /// <param name="clip">The audio clip.</param>
        /// <returns>The loudness value.</returns>
        public float GetLoudnessFromAudioClip(int clipPosition, AudioClip clip)
        {
            int startPosition = clipPosition - sampleDataLength;
            if (startPosition < 0)
            {
                return 0f;
            }

            float[] waveData = new float[sampleDataLength];
            clip.GetData(waveData, startPosition);

            float totalLoudness = 0;
            for (int i = 0; i < sampleDataLength; i++)
            {
                totalLoudness += Mathf.Abs(waveData[i]);
            }

            return totalLoudness / sampleDataLength;
        }

        /// <summary>
        /// Updates the blend shapes based on microphone input.
        /// </summary>
        void Update()
        {
            currentUpdateTime += Time.deltaTime;
            if (currentUpdateTime >= updateStep)
            {
                currentUpdateTime = 0f;

                if (speaking && Microphone.devices.Length > 0 && Microphone.devices[0] != null)
                {
                    clipLoudness = GetLoudnessFromMicrophone();
                    UpdateBlendShapes(clipLoudness);
                }
                else
                {
                    ResetBlendShapes();
                }
            }
        }

        /// <summary>
        /// Starts the microphone recording.
        /// </summary>
        /// <param name="micName">The name of the microphone.</param>
        private void StartMicrophone(string micName)
        {
            clipSampleData = new float[sampleDataLength];
            Debug.Log("Mic " + micName);
            currentMic = micName;
            micClip = Microphone.Start(micName, true, 20, AudioSettings.outputSampleRate);
        }

        /// <summary>
        /// Updates the blend shapes based on loudness.
        /// </summary>
        /// <param name="loudness">The loudness value.</param>
        private void UpdateBlendShapes(float loudness)
        {
            if (loudness > 0.001f)
            {
                headsmr.SetBlendShapeWeight(0, Mathf.Clamp(loudness * 32, 0f, 1f));
                teethsmr.SetBlendShapeWeight(0, Mathf.Clamp(loudness * 22, 0f, 1f));
            }
        }

        /// <summary>
        /// Resets the blend shapes to default values.
        /// </summary>
        private void ResetBlendShapes()
        {
            headsmr.SetBlendShapeWeight(0, 0);
            teethsmr.SetBlendShapeWeight(0, 0);
        }
    }
}
