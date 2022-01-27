using UnityEngine;
using UnityEditor;

namespace StoneAge {
	[CustomEditor(typeof(StoneAging))]
	public class StoneAgingInspector : Editor {

		SerializedProperty loggingLevel;
		SerializedProperty albedoMap;
		SerializedProperty heightMap;
		SerializedProperty agingYears;
		SerializedProperty rainRate;
		SerializedProperty saveToDisk;
		SerializedProperty saveLocation;
		SerializedProperty folderName;
		SerializedProperty saveDebugTextures;

		private void OnEnable() {
			loggingLevel = serializedObject.FindProperty("loggingLevel");
			albedoMap = serializedObject.FindProperty("albedoMap");
			heightMap = serializedObject.FindProperty("heightMap");
			agingYears = serializedObject.FindProperty("agingYears");
			rainRate = serializedObject.FindProperty("rainRate");
			saveToDisk = serializedObject.FindProperty("saveToDisk");
			saveLocation = serializedObject.FindProperty("saveLocation");
			folderName = serializedObject.FindProperty("folderName");
			saveDebugTextures = serializedObject.FindProperty("saveDebugTextures");
		}

		public override void OnInspectorGUI() {
			EditorGUILayout.LabelField("Settings:");
			EditorGUILayout.PropertyField(loggingLevel);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Input textures:");
			EditorGUILayout.PropertyField(albedoMap);
			EditorGUILayout.PropertyField(heightMap);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("General parameters:");
			EditorGUILayout.PropertyField(agingYears);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Erosion parameters:");
			EditorGUILayout.PropertyField(rainRate);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Export settings:");
			EditorGUILayout.PropertyField(saveToDisk);

			if (saveToDisk.boolValue) {
				EditorGUILayout.PropertyField(saveLocation);
				EditorGUILayout.PropertyField(folderName);
				EditorGUILayout.PropertyField(saveDebugTextures);
			}

			serializedObject.ApplyModifiedProperties();

			EditorGUILayout.Space();
			StoneAging scriptInstance = (StoneAging) target;
			if (GUILayout.Button("Simulate")) {
				scriptInstance.PerformAging();
			}
		}
	}
}