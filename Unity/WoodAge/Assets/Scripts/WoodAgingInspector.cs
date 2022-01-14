using UnityEngine;
using UnityEditor;

namespace WoodAge {
	[CustomEditor(typeof(WoodAging))]
	public class WoodAgingInspector : Editor {

		public override void OnInspectorGUI() {
			DrawDefaultInspector();

			WoodAging scriptInstance = (WoodAging) target;
			if (GUILayout.Button("Simulate")) {
				scriptInstance.PerformAging();
			}
		}
	}
}