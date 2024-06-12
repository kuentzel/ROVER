using UnityEngine;

namespace ROVER.Avatar
{
    public class BlinkController : MonoBehaviour
    {
        [Header("Blink Timing Settings")]
        public float minBlinkWaitTimeInS = 1.5f;
        public float maxBlinkWaitTimeInS = 3.5f;
        public float minBlinkTimeInS = 0.1f;
        public float maxBlinkTimeInS = 0.3f;

        [Header("Face Settings")]
        public SkinnedMeshRenderer faceSMR;
        [HideInInspector]
        public string blendshapeName = "eyesClosed";

        private int blendshapeId;
        private float clock;
        private bool wait;
        private bool started = false;
        private float currentY;
        private float blinkTime;

        /// <summary>
        /// Sets the blendshape ID and starts the blinking process.
        /// </summary>
        /// <param name="v">Blendshape ID.</param>
        internal void SetBlendShapeId(int v)
        {
            blendshapeId = v;
            started = true;
            GenerateNewBlinkWait();
        }

        /// <summary>
        /// Generates a new random wait time before the next blink.
        /// </summary>
        private void GenerateNewBlinkWait()
        {
            clock = Random.Range(minBlinkWaitTimeInS, maxBlinkWaitTimeInS);
            faceSMR.SetBlendShapeWeight(blendshapeId, 0); // Reset blendshape weight to 0 (eyes open)
            wait = true;
        }

        /// <summary>
        /// Generates a new random blink time and initiates the blink.
        /// </summary>
        private void GenerateNewBlink()
        {
            clock = Random.Range(minBlinkTimeInS, maxBlinkTimeInS);
            blinkTime = clock;
            faceSMR.SetBlendShapeWeight(blendshapeId, 80); // Set blendshape weight to simulate eyes closed
            wait = false;
        }

        /// <summary>
        /// Updates the blink state each frame.
        /// </summary>
        private void Update()
        {
            if (!started)
                return;

            clock -= Time.deltaTime;

            if (wait)
            {
                // Waiting for the next blink
                currentY = 0;
                if (clock < 0)
                    GenerateNewBlink();
            }
            else
            {
                // In the process of blinking
                currentY = (0.5f - Mathf.Abs((clock / blinkTime) - 0.5f)) * 200;
                if (clock < 0)
                    GenerateNewBlinkWait();
            }
        }

        /// <summary>
        /// Enters the blendshape values for the blink animation.
        /// </summary>
        /// <param name="values">Array of blendshape values.</param>
        internal void EnterBlendshapeValues(float[] values)
        {
            values[blendshapeId] = currentY;
        }
    }
}
