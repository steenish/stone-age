using UnityEngine;
using UnityEditor;

namespace Utility {
    [CustomEditor(typeof(FunctionTester))]
    public class FunctionTesterInspector : Editor {

        public override void OnInspectorGUI() {
            DrawDefaultInspector();
            FunctionTester scriptInstance = (FunctionTester) target;
            if (GUILayout.Button("Test")) {
                scriptInstance.PerformTest();
            }
        }
    }
}