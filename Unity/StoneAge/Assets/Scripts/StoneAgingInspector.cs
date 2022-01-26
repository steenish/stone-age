using UnityEngine;
using UnityEditor;

namespace StoneAge {
	[CustomEditor(typeof(StoneAging))]
	public class StoneAgingInspector : Editor {

		SerializedProperty albedoMap;
		SerializedProperty heightMap;
		SerializedProperty agingYears;
		SerializedProperty rainRate;
		SerializedProperty saveToDisk;
		SerializedProperty saveLocation;
		SerializedProperty folderName;

		private void OnEnable() {
			albedoMap = serializedObject.FindProperty("albedoMap");
			heightMap = serializedObject.FindProperty("heightMap");
			agingYears = serializedObject.FindProperty("agingYears");
			rainRate = serializedObject.FindProperty("rainRate");
			saveToDisk = serializedObject.FindProperty("saveToDisk");
			saveLocation = serializedObject.FindProperty("saveLocation");
			folderName = serializedObject.FindProperty("folderName");
		}

		public override void OnInspectorGUI() {
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