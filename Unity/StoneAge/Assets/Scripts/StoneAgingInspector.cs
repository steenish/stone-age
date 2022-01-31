using UnityEngine;
using UnityEditor;

namespace StoneAge {
	[CustomEditor(typeof(StoneAging))]
	public class StoneAgingInspector : Editor {

		SerializedProperty loggingLevel;
		SerializedProperty albedoMap;
		SerializedProperty heightMap;
		SerializedProperty agingYears;
		SerializedProperty seed;
		SerializedProperty rainRate;
		SerializedProperty sedimentColor;
		SerializedProperty sedimentOpacityModifier;
		SerializedProperty customErosionParameters;
		SerializedProperty inertia;
		SerializedProperty capacity;
		SerializedProperty deposition;
		SerializedProperty erosion;
		SerializedProperty evaporation;
		SerializedProperty radius;
		SerializedProperty minSlope;
		SerializedProperty maxPath;
		SerializedProperty gravity;
		SerializedProperty rockErosionFactor;
		SerializedProperty sedimentErosionFactor;
		SerializedProperty saveToDisk;
		SerializedProperty saveLocation;
		SerializedProperty folderName;
		SerializedProperty saveDebugTextures;

		private void OnEnable() {
			loggingLevel = serializedObject.FindProperty("loggingLevel");
			albedoMap = serializedObject.FindProperty("albedoMap");
			heightMap = serializedObject.FindProperty("heightMap");
			agingYears = serializedObject.FindProperty("agingYears");
			seed = serializedObject.FindProperty("seed");
			rainRate = serializedObject.FindProperty("rainRate");
			sedimentColor = serializedObject.FindProperty("sedimentColor");
			sedimentOpacityModifier = serializedObject.FindProperty("sedimentOpacityModifier");
			customErosionParameters = serializedObject.FindProperty("customErosionParameters");
			inertia = serializedObject.FindProperty("inertia");
			capacity = serializedObject.FindProperty("capacity");
			deposition = serializedObject.FindProperty("deposition");
			erosion = serializedObject.FindProperty("erosion");
			evaporation = serializedObject.FindProperty("evaporation");
			radius = serializedObject.FindProperty("radius");
			minSlope = serializedObject.FindProperty("minSlope");
			maxPath = serializedObject.FindProperty("maxPath");
			gravity = serializedObject.FindProperty("gravity");
			rockErosionFactor = serializedObject.FindProperty("rockErosionFactor");
			sedimentErosionFactor = serializedObject.FindProperty("sedimentErosionFactor");
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
			EditorGUILayout.PropertyField(seed);

			EditorGUILayout.Space();
			EditorGUILayout.LabelField("Erosion parameters:");
			EditorGUILayout.PropertyField(rainRate);
			EditorGUILayout.PropertyField(sedimentColor);
			EditorGUILayout.PropertyField(sedimentOpacityModifier);
			EditorGUILayout.PropertyField(customErosionParameters);

			if (customErosionParameters.boolValue) {
				EditorGUILayout.PropertyField(inertia);
				EditorGUILayout.PropertyField(capacity);
				EditorGUILayout.PropertyField(deposition);
				EditorGUILayout.PropertyField(erosion);
				EditorGUILayout.PropertyField(evaporation);
				EditorGUILayout.PropertyField(radius);
				EditorGUILayout.PropertyField(minSlope);
				EditorGUILayout.PropertyField(maxPath);
				EditorGUILayout.PropertyField(gravity);
				EditorGUILayout.PropertyField(rockErosionFactor);
				EditorGUILayout.PropertyField(sedimentErosionFactor);
			}

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