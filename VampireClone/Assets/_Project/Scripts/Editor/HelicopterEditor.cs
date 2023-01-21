using UnityEditor;
using UnityEngine;

namespace Magaa
{
    [CustomEditor(typeof(Helicopter))]
    public class HelicopterEditor : Editor
    {
        private SerializedProperty startPosition;
        private SerializedProperty endPosition;
        private SerializedProperty rope;
        private SerializedProperty ropeMaxSize;

        private void OnEnable()
        {
            startPosition = serializedObject.FindProperty(nameof(startPosition));
            endPosition = serializedObject.FindProperty(nameof(endPosition));
            rope = serializedObject.FindProperty(nameof(rope));
            ropeMaxSize = serializedObject.FindProperty(nameof(ropeMaxSize));
        }
        private void OnSceneGUI()
        {
            if (Application.isPlaying) return;
            EditorGUI.BeginChangeCheck();
            Helicopter target = this.target as Helicopter;
            using (new Handles.DrawingScope(target.transform.localToWorldMatrix))
            {
                Handles.DrawAAPolyLine(startPosition.vector3Value, Vector3.zero, endPosition.vector3Value);
                startPosition.vector3Value = Handles.PositionHandle(startPosition.vector3Value, Quaternion.identity);
                endPosition.vector3Value = Handles.PositionHandle(endPosition.vector3Value, Quaternion.identity);
            }
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }

            Transform ropeTransform = ((Transform)rope.objectReferenceValue);
            if (ropeTransform == null) return;
            using (new Handles.DrawingScope(ropeTransform.localToWorldMatrix))
            {
                Handles.DrawAAPolyLine(Vector3.zero, Vector3.zero + (Vector3.down * ropeMaxSize.floatValue));
            }
        }
    }
}
