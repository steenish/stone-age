using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace StoneAge {
    public class StoneAging : MonoBehaviour {

        [SerializeField]
        private GameObject agingObject;
        [SerializeField]
        Texture2D albedoMap = null;
        [SerializeField]
        Texture2D heightMap = null;
        [SerializeField]
        Texture2D occlusionMap = null;
        [SerializeField]
        private int agingYears;
        [SerializeField]
        private bool saveToDisk = false;

        public void PerformAging() {
            Debug.Log("Initializing.");
            System.DateTime initializationStart = System.DateTime.Now;

            // Create buffers from the extracted textures.
            DoubleColor[,] albedoBuffer = null;
            double[,] heightBuffer = null;
            float[,] occlusionBuffer = null;

            if (albedoMap != null) {
                albedoBuffer = Conversion.CreateColorBuffer(albedoMap);
            } else {
                Debug.LogError("No albedo map supplied.");
			}

            if (heightMap != null) {
                heightBuffer = Conversion.CreateDoubleBuffer(heightMap);
			} else {
                heightBuffer = Height.HeightFromAlbedo(albedoMap); // TODO INVESTIGATE MODIFYING CONTRAST
            }

            if (occlusionMap != null) {
                occlusionBuffer = Conversion.CreateFloatBuffer(occlusionMap);
            } else {
                // Default occlusion buffer to all 1.
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

            for (int year = 0; year < agingYears; ++year) {
                Debug.Log("Aged " + (year + 1) + " years.");
            }

            if (saveToDisk) {
                string savePath = "C:/Users/Anders Steen/Documents/"; // TODO FIND A BETTER LOCATION
                // TODO NULL CHECKS
				TextureDebug.SaveTextureAsPNG(Conversion.CreateTexture(albedoMap.width, albedoMap.height, albedoBuffer), savePath + "Albedo_Aged.png");
				TextureDebug.SaveTextureAsPNG(Conversion.CreateTexture(albedoMap.width, albedoMap.height, heightBuffer), savePath + "Height_Aged.png");
                int occlusionWidth = occlusionMap == null ? albedoMap.width : occlusionMap.width;
                int occlusionHeight = occlusionMap == null ? albedoMap.height : occlusionMap.height;
				TextureDebug.SaveTextureAsPNG(Conversion.CreateTexture(occlusionWidth, occlusionHeight, occlusionBuffer), savePath + "Occlusion_Aged.png");
			}
        }
    }
}
