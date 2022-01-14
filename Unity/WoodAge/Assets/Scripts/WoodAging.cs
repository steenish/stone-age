using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace WoodAge {
    public class WoodAging : MonoBehaviour {

        [SerializeField]
        private GameObject agingObject;
        [SerializeField]
        private Texture2D woodMap;
        [SerializeField]
        Texture2D albedoMap = null;
        [SerializeField]
        Texture2D roughnessMap = null;
        [SerializeField]
        Texture2D occlusionMap = null;
        [SerializeField]
        private int agingDays;
        [SerializeField]
        private bool saveToDisk = false;
        [SerializeField]
        private float latitude;
        [SerializeField]
        private int yearStartDay;

        public void PerformAging() {
            Debug.Log("Initializing.");
            System.DateTime initializationStart = System.DateTime.Now;

            // Create wood buffer, from supplied map.
            bool[,] woodBuffer = new bool[woodMap.width, woodMap.height];
            for (int x = 0; x < woodMap.width; ++x) {
                for (int y = 0; y < woodMap.height; ++y) {
                    woodBuffer[x, y] = woodMap.GetPixel(x, y).r >= 0.5f;
                }
            }

            // Create buffers from the extracted textures.
            DoubleColor[,] albedoBuffer = null;
            double[,] roughnessBuffer = null;
            double[,] heightBuffer = null;
            float[,] occlusionBuffer = null;

            if (albedoMap != null) {
                albedoBuffer = Conversion.CreateColorBuffer(albedoMap);
            } else {
                Debug.LogError("No albedo map supplied.");
			}

            if (roughnessMap != null) {
                roughnessBuffer = Conversion.CreateDoubleBuffer(roughnessMap);
            }

            if (albedoMap != null) {
                heightBuffer = Height.HeightFromNormals(albedoMap); // TODO INVESTIGATE MODIFYING CONTRAST
            }

            if (occlusionMap != null) {
                occlusionBuffer = Conversion.CreateFloatBuffer(occlusionMap);
            } else {
                occlusionBuffer = new float[albedoMap.width, albedoMap.height];
                for (int y = 0; y < albedoMap.height; ++y) {
                    for (int x = 0; x < albedoMap.height; ++x) {
                        occlusionBuffer[x, y] = 1.0f;
					}
				}
			}

            System.TimeSpan timeDifference = System.DateTime.Now - initializationStart;
            Debug.Log("Initialization done (" + (timeDifference.Seconds + timeDifference.Milliseconds * 0.001) + " s)");

            // Perform the aging.
            Debug.Log("Aging " + agingObject.name + ".");

            for (int day = 0; day < agingDays; ++day) {
                int yearDay = (yearStartDay + day) % 365;

                Debug.Log("Aged " + (day + 1) + " days.");
            }

            if (saveToDisk) {
                string savePath = "C:/Users/Anders Steen/Documents/"; // TODO FIND A BETTER LOCATION
                // TODO NULL CHECKS
				TextureDebug.SaveTextureAsPNG(Conversion.CreateTexture(albedoMap.width, albedoMap.height, albedoBuffer), savePath + "Albedo_Aged.png");
				TextureDebug.SaveTextureAsPNG(Conversion.CreateTexture(roughnessMap.width, roughnessMap.height, roughnessBuffer), savePath + "Roughness_Aged.png");
				TextureDebug.SaveTextureAsPNG(Conversion.CreateTexture(albedoMap.width, albedoMap.height, heightBuffer), savePath + "Height_Aged.png");
                int occlusionWidth = occlusionMap == null ? albedoMap.width : occlusionMap.width;
                int occlusionHeight = occlusionMap == null ? albedoMap.height : occlusionMap.height;
				TextureDebug.SaveTextureAsPNG(Conversion.CreateTexture(occlusionWidth, occlusionHeight, occlusionBuffer), savePath + "Occlusion_Aged.png");
			}
        }
    }
}
