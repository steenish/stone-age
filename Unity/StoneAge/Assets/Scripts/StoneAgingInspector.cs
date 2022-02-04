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
		SerializedProperty numSteps;
		SerializedProperty seed;
		SerializedProperty timeStep;
		SerializedProperty rainScale;
		SerializedProperty pipeRadius;
		SerializedProperty realWorldSize;
		SerializedProperty gravity;
		SerializedProperty minTiltAngle;
		SerializedProperty capacityModifier;
		SerializedProperty dissolvingModifier;
		SerializedProperty depositionModifier;

		private void OnEnable() {
			loggingLevel = serializedObject.FindProperty("loggingLevel");
			setupShader = serializedObject.FindProperty("setupShader");
			erosionShader = serializedObject.FindProperty("erosionShader");
			albedoMap = serializedObject.FindProperty("albedoMap");
			heightMap = serializedObject.FindProperty("heightMap");
			numSteps = serializedObject.FindProperty("numSteps");
			seed = serializedObject.FindProperty("seed");
			timeStep = serializedObject.FindProperty("timeStep");
			rainScale = serializedObject.FindProperty("rainScale");
			pipeRadius = serializedObject.FindProperty("pipeRadius");
			realWorldSize = serializedObject.FindProperty("realWorldSize");
			gravity = serializedObject.FindProperty("gravity");
			minTiltAngle = serializedObject.FindProperty("minTiltAngle");
			capacityModifier = serializedObject.FindProperty("capacityModifier");
			dissolvingModifier = serializedObject.FindProperty("dissolvingModifier");
			depositionModifier = serializedObject.FindProperty("depositionModifier");
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
			EditorGUILayout.PropertyField(seed);
			EditorGUILayout.PropertyField(numSteps);
			EditorGUILayout.PropertyField(timeStep);
			EditorGUILayout.PropertyField(rainScale);
			EditorGUILayout.PropertyField(pipeRadius);
			EditorGUILayout.PropertyField(realWorldSize);
			EditorGUILayout.PropertyField(gravity);
			EditorGUILayout.PropertyField(minTiltAngle);
			EditorGUILayout.PropertyField(capacityModifier);
			EditorGUILayout.PropertyField(dissolvingModifier);
			EditorGUILayout.PropertyField(depositionModifier);

			serializedObject.ApplyModifiedProperties();

			EditorGUILayout.Space();
			StoneAging scriptInstance = (StoneAging) target;
			if (GUILayout.Button("Simulate")) {
				scriptInstance.PerformAging();
			}
		}
	}
}