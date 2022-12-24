using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BardicBytes.BardicPlatformerSamples
{
    public class JumpHeightChecker : MonoBehaviour
    {
        public float maxHeight = 0f;
        private void Update()
        {
            if (transform.position.y > maxHeight) maxHeight = transform.position.y;
        }
    }
}
