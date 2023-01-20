using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Magaa
{
    public class AnimationListener : MonoBehaviour
    {
        public void AnimationSendMessageUpwards(string methodName)
        {
            SendMessageUpwards(methodName, null, SendMessageOptions.DontRequireReceiver);
        }
    }
}
