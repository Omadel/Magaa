using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VampireClone
{
    public class AnimatorListener : MonoBehaviour
    {
       public void Attack()
        {
            GetComponentInParent<IAttacker>().Attack();
        }
    }
}
