using Etienne;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Magaa
{
    [RequireComponent(typeof(Slider))]
    public class ExperienceBar : MonoBehaviour
    {
        Slider slider;

        private void Awake()
        {
            GameManager.Instance.SetExperienceBar(this);
            slider = GetComponent<Slider>();
            slider.wholeNumbers = true;
        }

        public void SetMax(float max)
        {
            slider.maxValue = max;
        }

        public void SetValue(int value)
        {
            slider.value = value;
        }
    }
}
