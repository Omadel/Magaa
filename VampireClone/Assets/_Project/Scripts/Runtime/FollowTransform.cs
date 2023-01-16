using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Magaa
{
    public class FollowTransform : MonoBehaviour
    {
        [SerializeField] Transform targetTransform;

        void LateUpdate()
        {
            transform.position = targetTransform.position;
        }
    }
}
