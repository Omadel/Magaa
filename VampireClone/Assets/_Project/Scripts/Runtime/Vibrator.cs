using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Magaa
{

    public static class Vibration
    {

#if UNITY_ANDROID && !UNITY_EDITOR
    public static AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
    public static AndroidJavaObject currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
    public static AndroidJavaObject vibrator = currentActivity.Call<AndroidJavaObject>("getSystemService", "vibrator");
#else
        public static AndroidJavaClass unityPlayer;
        public static AndroidJavaObject currentActivity;
        public static AndroidJavaObject vibrator;
#endif

        internal class GamePadVibrator : MonoBehaviour
        {
            private static GamePadVibrator Instance
            {
                get
                {
                    if (instance != null) return instance;
                    GameObject go = new GameObject("GamePadVibrator Manager");
                    instance = go.AddComponent<GamePadVibrator>();
                    return instance;
                }
            }

            private static GamePadVibrator instance;
            private Coroutine stopRoutine;

            public static void Vibrate(float lowFrequency, float highFrequency, float duration)
            {
                Gamepad.current?.SetMotorSpeeds(lowFrequency, highFrequency);
                Instance.Vibrate(duration);
            }

            private void Vibrate(float duration)
            {
                if (stopRoutine != null) StopCoroutine(stopRoutine);
                stopRoutine = StartCoroutine(VibrationRoutine(duration));
            }

            private IEnumerator VibrationRoutine(float duration)
            {
                yield return new WaitForSeconds(duration);
                Gamepad.current?.SetMotorSpeeds(0, 0);
            }

            private void OnDestroy()
            {
                Gamepad.current?.SetMotorSpeeds(0, 0);
            }
        }

        public static void Vibrate()
        {
#if !UNITY_WEBGL && !UNITY_STANDALONE_WIN
            if (isAndroid())
                vibrator.Call("vibrate");
            else
                Handheld.Vibrate();
#endif
        }


        public static void Vibrate(long milliseconds)
        {
#if !UNITY_WEBGL&& !UNITY_STANDALONE_WIN
            if (isAndroid())
                vibrator.Call("vibrate", milliseconds);
            else
                Handheld.Vibrate();
#endif
            GamePadVibrator.Vibrate(400, 100, milliseconds * .001f);
        }

        public static void Vibrate(long[] pattern, int repeat)
        {
#if !UNITY_WEBGL && !UNITY_STANDALONE_WIN
            if (isAndroid())
                vibrator.Call("vibrate", pattern, repeat);
            else
                Handheld.Vibrate();
#endif 
        }

        public static bool HasVibrator()
        {
            return isAndroid();
        }

        public static void Cancel()
        {
            if (isAndroid())
                vibrator.Call("cancel");
        }

        private static bool isAndroid()
        {
#if UNITY_ANDROID && !UNITY_EDITOR
	return true;
#else
            return false;
#endif
        }
    }
}
