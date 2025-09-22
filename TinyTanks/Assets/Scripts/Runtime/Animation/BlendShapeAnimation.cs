using JetBrains.Annotations;
using UnityEngine;

namespace Animation
{
    /// <summary>
    /// 
    /// </summary>
    public class BlendShapeAnimation : MonoBehaviour
    {
        public SkinnedMeshRenderer skinnedMeshRenderer;
        public int blendshapeIndex = 0;
        public float speed = 50f; // how many units per second

        void Update()
        {
            // Increase continuously, but wrap around at 100
            float weight = Mathf.Repeat(Time.time * speed, 100f);
            skinnedMeshRenderer.SetBlendShapeWeight(blendshapeIndex, weight);
        }
    }
}
