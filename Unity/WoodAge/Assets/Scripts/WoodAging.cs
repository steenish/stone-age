using System.Collections.Generic;
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

        Debug.Log("Aging " + agingObject.name + ".");

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
        // TODO

        // Perform the aging.
        for (int day = 0; day < agingDays; ++day) {
            int yearDay = (yearStartDay + day) % 365;

            Debug.Log("Aged " + (day + 1) + " days.");
		}

        if (applyToMaterialAfter) {
            MaterialPropertyBlock mpb = new MaterialPropertyBlock();
            if (albedoMap != null) {
                mpb.SetTexture("_MainTex", CreateTexture(albedoBuffer));
			}

            if (roughnessMap != null) {
                mpb.SetTexture("_SpecGlossMap", CreateTexture(roughnessBuffer));
            }

            if (normalMap != null) {
                mpb.SetTexture("_BumpMap", NormalsFromHeight(heightBuffer));
			}

            agingObject.GetComponent<Renderer>().SetPropertyBlock(mpb, applyMaterialInSlotIndex);
        }
	}

    private Texture2D CreateTexture(DoubleColor[,] colorBuffer) {
        // TODO
        throw new System.NotImplementedException();
	}

    private Texture2D CreateTexture(double[,] doubleBuffer) {
        // TODO
        throw new System.NotImplementedException();
    }

    private float[,] HeightFromNormals(Texture2D normalMap) {
        // TODO
        throw new System.NotImplementedException();
    }

    private Texture2D NormalsFromHeight(double[,] height) {
        // TODO
        throw new System.NotImplementedException();
	}
}
