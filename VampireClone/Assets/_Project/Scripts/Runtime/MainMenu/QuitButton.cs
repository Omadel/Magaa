using UnityEngine;
using UnityEngine.UI;

namespace Magaa
{
    [RequireComponent(typeof(Button))]
    public class QuitButton : MonoBehaviour
    {
        private void Start()
        {
            GetComponent<Button>().onClick.AddListener(QuitGame);
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
            Application.Quit();
#endif
        }
    }
}
