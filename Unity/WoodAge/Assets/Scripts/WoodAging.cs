using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public class WoodAging : MonoBehaviour {

    [SerializeField]
    private GameObject agingObject;
    [SerializeField]
    private Material agingMaterial;
    [SerializeField]
    private Texture2D woodMap;
    [SerializeField]
    private int agingDays;
    [SerializeField]
    private bool applyToMaterialAfter = false;
    [SerializeField]
    private int applyMaterialInSlotIndex = 0;
    [SerializeField]
    private float latitude;
    [SerializeField]
    private int yearStartDay;

    public void PerformAging() {
        if (agingMaterial.shader != Shader.Find("Autodesk Interactive")) {
            Debug.LogError("Parameter Aging Material is not an instance of the Autodesk Interactive shader.");
            return;
		}

        Debug.Log("Initializing.");
        System.DateTime initializationStart = System.DateTime.Now;

        // Extract textures from material.
        Texture2D albedoMap = null;
        Texture2D roughnessMap = null;
        Texture2D normalMap = null;
        Texture2D occlusionMap = null;

        List<string> texturePropertyNames = new List<string>();
        agingMaterial.GetTexturePropertyNames(texturePropertyNames);
        for (int i = 0; i < texturePropertyNames.Count; ++i) {
            switch (texturePropertyNames[i]) {
                case "_MainTex":
                    albedoMap = (Texture2D) agingMaterial.GetTexture("_MainTex");
                    break;
                case "_SpecGlossMap":
                    roughnessMap = (Texture2D) agingMaterial.GetTexture("_SpecGlossMap");
                    break;
                case "_BumpMap":
                    normalMap = (Texture2D) agingMaterial.GetTexture("_BumpMap");
                    break;
                case "_OcclusionMap":
                    occlusionMap = (Texture2D) agingMaterial.GetTexture("_OcclusionMap");
                    break;
			}
		}

        // Create buffers from the extracted textures.
        DoubleColor[,] albedoBuffer = null;
        double[,] roughnessBuffer = null;
        double[,] heightBuffer = null;
        float[,] occlusionBuffer = null;
        

        if (albedoMap != null) {
            albedoBuffer = Utility.CreateColorBuffer(albedoMap);
            //TextureDebug.DrawTexture(albedoMap.width, albedoMap.height, albedoBuffer);
        }

        if (roughnessMap != null) {
            roughnessBuffer = Utility.CreateDoubleBuffer(roughnessMap);
            //TextureDebug.DrawTexture(roughnessMap.width, roughnessMap.height, roughnessBuffer);
        }

        if (normalMap != null) {
			heightBuffer = Utility.HeightFromNormals(normalMap);
			TextureDebug.DrawTexture(normalMap.width, normalMap.height, heightBuffer);
		}

		if (occlusionMap != null) {
            occlusionBuffer = Utility.CreateFloatBuffer(occlusionMap);
            //TextureDebug.DrawTexture(occlusionMap.width, occlusionMap.height, occlusionBuffer);
        }

        System.TimeSpan timeDifference = System.DateTime.Now - initializationStart;
        Debug.Log("Initialization done (" + (timeDifference.Seconds + timeDifference.Milliseconds * 0.001) + " s)"); // TODO

        // Perform the aging.
        Debug.Log("Aging " + agingObject.name + ".");

        for (int day = 0; day < agingDays; ++day) {
            int yearDay = (yearStartDay + day) % 365;

            Debug.Log("Aged " + (day + 1) + " days.");
		}

        if (applyToMaterialAfter) {
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            if (albedoMap != null) {
                mpb.SetTexture("_MainTex", Utility.CreateTexture(albedoMap.width, albedoMap.height, albedoBuffer));
			}

            if (roughnessMap != null) {
                mpb.SetTexture("_SpecGlossMap", Utility.CreateTexture(roughnessMap.width, roughnessMap.height, roughnessBuffer));
            }

            if (normalMap != null) {
                mpb.SetTexture("_BumpMap", Utility.NormalsFromHeight(heightBuffer));
			}

            agingObject.GetComponent<Renderer>().SetPropertyBlock(mpb, applyMaterialInSlotIndex);
        }
	}

    
}
