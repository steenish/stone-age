using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace StoneAge {
    public class StoneAging : MonoBehaviour {
        [SerializeField]
        Texture2D albedoMap = null;
        [SerializeField]
        Texture2D heightMap = null;
        [SerializeField]
        private int agingYears;
        [SerializeField]
        private float rainRate = 1.0f;
        [SerializeField]
        private bool saveToDisk = false;
        [SerializeField]
        private System.Environment.SpecialFolder saveLocation;

        public void PerformAging() {
            Debug.Log("Initializing...");
            System.DateTime initializationStart = System.DateTime.Now;

            // Create buffers from the extracted textures.
            DoubleColor[,] albedoBuffer = null;
            double[,] heightBuffer = null;

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

            // Sediment buffer initialized to same size as height buffer, with all zeros.
            double[,] sedimentBuffer = new double[heightBuffer.GetLength(1), heightBuffer.GetLength(0)];

            LogTime("Initialization done", initializationStart);

            // Perform the aging.
            Debug.Log("Aging...");

            System.DateTime simulationStart = System.DateTime.Now;

            for (int year = 0; year < agingYears; ++year) {
                System.DateTime yearStart = System.DateTime.Now;

                // Perform rain erosion.
                for (int rainDay = 0; rainDay < Mathf.FloorToInt(365.25f * rainRate); ++rainDay) {
                    Erosion.ErosionEvent(ref heightBuffer);
				}

                LogTime("Aged " + (year + 1) + " year" + ((year + 1 == 1) ? "" : "s"), yearStart);
            }

            Height.NormalizeHeight(ref heightBuffer);

            LogTime("Aging done", simulationStart);

            if (saveToDisk) {
                string savePath = System.Environment.GetFolderPath(saveLocation) + "/StoneAge/";
                System.IO.Directory.CreateDirectory(savePath);
				Textures.SaveTextureAsPNG(Conversion.CreateTexture(albedoMap.width, albedoMap.height, albedoBuffer), savePath + "Albedo_Aged_" + agingYears + ".png");
				Textures.SaveTextureAsPNG(Conversion.CreateTexture(albedoMap.width, albedoMap.height, heightBuffer), savePath + "Height_Aged_" + agingYears + ".png");
			}

            LogTime("All done", initializationStart);
        }

        private static void LogTime(string text, System.DateTime startTime) {
            System.TimeSpan timeDifference = System.DateTime.Now - startTime;
            Debug.Log(text + " (" + (timeDifference.Seconds + timeDifference.Milliseconds * 0.001) + " s).");
        }
    }
}
