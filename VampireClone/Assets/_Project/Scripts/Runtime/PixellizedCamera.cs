using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace Magaa
{
    [RequireComponent(typeof(Camera))]
    [ExecuteAlways]
    public class PixellizedCamera : MonoBehaviour
    {
        [SerializeField, Tooltip("Render Scale at 1080p")] private float pixelSize = .3f;
        private new Camera camera;
        private readonly Vector2Int defaultResolution = new Vector2Int(1920, 1080);
        private Vector2Int currentResolution;

        private void Start()
        {
            camera = GetComponent<Camera>();
            SetRenderScale(pixelSize * GetRenderScaleRatio(GetEditorGameViewResolution()));
        }

        private void Update()
        {
            Vector2Int oldResolution = currentResolution;
            currentResolution = GetEditorGameViewResolution();
            if (oldResolution == currentResolution) return;
            SetRenderScale(pixelSize * GetRenderScaleRatio(GetEditorGameViewResolution()));
        }

        private void SetRenderScale(float renderScale)
        {
            // Get the URP asset from the active render pipeline
            UniversalRenderPipelineAsset renderPipelineAsset = UniversalRenderPipeline.asset;
            // Change the render scale
            renderPipelineAsset.renderScale = renderScale;
        }

        private float GetRenderScaleRatio(Vector2Int resolution)
        {
            Vector2 scaledResolution = resolution.DividedBy(defaultResolution);
            return 1 / scaledResolution.Max();
        }

        public static Vector2Int GetEditorGameViewResolution()
        {
#if UNITY_EDITOR
            // Get the System.Type for the UnityEditor.GameView class using reflection
            System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
            // Get the GetSizeOfMainGameView method from the UnityEditor.GameView class using reflection
            // This method is not publicly exposed, so we need to use binding flags to get access to it
            System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            // Invoke the GetSizeOfMainGameView method to get the size of the main GameView window
            System.Object Res = GetSizeOfMainGameView.Invoke(null, null);
            // Cast the returned object to a Vector2 and return it as a Vector2Int rounded
            return Vector2Int.RoundToInt((Vector2)Res);
#else
            // If this code is not running in the Unity editor, return the screen resolution
            return new Vector2Int(Screen.width, Screen.height);
#endif
        }

        private void Reset()
        {
            UniversalAdditionalCameraData datas = GetComponent<Camera>().GetUniversalAdditionalCameraData();
            // Remove anti aliasing from the camera
            datas.antialiasing = AntialiasingMode.None;
            // Get the URP asset from the active render pipeline
            UniversalRenderPipelineAsset renderPipelineAsset = UniversalRenderPipeline.asset;
            // Remove anti aliasing from the render asset
            renderPipelineAsset.msaaSampleCount = 1;
            // Set the upsaling filter to point
            renderPipelineAsset.upscalingFilter = UpscalingFilterSelection.Point;
            // Set the render scale
            SetRenderScale(pixelSize * GetRenderScaleRatio(GetEditorGameViewResolution()));
        }

    }

    public static class Vector2Extentions
    {
        public static Vector2Int ToVectior2Int(this Resolution resolution) => new Vector2Int(resolution.width, resolution.height);
        public static Vector2 DividedBy(this Vector2Int dividend, Vector2Int divisor)
        {
            Vector2 quotient = new Vector2
            {
                x = dividend.x / (float)divisor.x,
                y = dividend.y / (float)divisor.y
            };
            return quotient;
        }
        public static float Max(this Vector2 vector)
        {
            return Mathf.Max(vector.x, vector.y);
        }
    }
}
