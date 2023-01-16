using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Magaa
{
    [RequireComponent(typeof(Button))]
    public class StartButton : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(StartGame);
        }

        private void StartGame()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(1);
        }
    }
}
