using UnityEngine;
using UnityEditor;

namespace StoneAge {
	[CustomEditor(typeof(StoneAging))]
	public class StoneAgingInspector : Editor {

		SerializedProperty loggingLevel;
		SerializedProperty setupShader;
		SerializedProperty erosionShader;
		SerializedProperty albedoMap;
		SerializedProperty heightMap;
		SerializedProperty agingYears;
		SerializedProperty seed;
		SerializedProperty timeStep;
		SerializedProperty rainScale;

		private void OnEnable() {
			loggingLevel = serializedObject.FindProperty("loggingLevel");
			setupShader = serializedObject.FindProperty("setupShader");
			erosionShader = serializedObject.FindProperty("erosionShader");
			albedoMap = serializedObject.FindProperty("albedoMap");
			heightMap = serializedObject.FindProperty("heightMap");
			agingYears = serializedObject.FindProperty("agingYears");
			seed = serializedObject.FindProperty("seed");
			timeStep = serializedObject.FindProperty("timeStep");
			rainScale = serializedObject.FindProperty("rainScale");
			
		}

		public override void OnInspectorGUI() {
			EditorGUILayout.LabelField("Settings:");
			EditorGUILayout.PropertyField(loggingLevel);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Shader programs:");
			EditorGUILayout.PropertyField(setupShader);
			EditorGUILayout.PropertyField(erosionShader);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Input textures:");
			EditorGUILayout.PropertyField(albedoMap);
			EditorGUILayout.PropertyField(heightMap);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Simulation parameters:");
			EditorGUILayout.PropertyField(agingYears);
			EditorGUILayout.PropertyField(seed);
			EditorGUILayout.PropertyField(timeStep);
			EditorGUILayout.PropertyField(rainScale);

			serializedObject.ApplyModifiedProperties();

			EditorGUILayout.Space();
			StoneAging scriptInstance = (StoneAging) target;
			if (GUILayout.Button("Simulate")) {
				scriptInstance.PerformAging();
			}
		}
	}
}