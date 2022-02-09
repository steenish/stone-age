using UnityEngine;
using UnityEditor;

namespace StoneAge {
    [CustomEditor(typeof(StoneAging))]
    public class StoneAgingInspector : Editor {

        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            StoneAging scriptInstance = (StoneAging) target;
            if (GUILayout.Button("Simulate")) {
                scriptInstance.PerformAging();
            }
        }
    }
}