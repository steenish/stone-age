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
        [SerializeField]
        private string folderName = "StoneAge";

        public void PerformAging() {
            if (albedoMap == null) {
                Debug.LogError("No albedo map supplied.");
                return;
            }

            if (heightMap == null) {
                Debug.LogError("No height map supplied.");
                return;
            }

            Debug.Log("Initializing...");
            System.DateTime initializationStart = System.DateTime.Now;

            // Create buffers from the input textures.
            DoubleColor[,] albedoBuffer = null;
            albedoBuffer = Conversion.CreateColorBuffer(albedoMap);

            double[,,] layers = new double[heightMap.width, heightMap.height, 2];
            Conversion.FillDoubleBufferLayer(heightMap, ref layers, (int) Erosion.LayerName.ROCK);

            // Sediment buffer initialized to same size as height buffer, with all zeros.
            double[,] sedimentBuffer = new double[layers.GetLength(1), layers.GetLength(0)];

            LogTime("Initialization done", initializationStart);

            // Perform the aging.
            Debug.Log("Aging...");

            System.DateTime simulationStart = System.DateTime.Now;
            int rainDays = Mathf.FloorToInt(365.25f * rainRate);

            for (int year = 0; year < agingYears; ++year) {
                System.DateTime yearStart = System.DateTime.Now;

                // Perform rain erosion.
                for (int rainDay = 0; rainDay < rainDays; ++rainDay) {
                    Erosion.ErosionEvent(ref layers);
				}

                LogTime("Aged " + (year + 1) + " year" + ((year + 1 == 1) ? "" : "s"), yearStart);
            }


            LogTime("Aging done", simulationStart);

            Debug.Log("Finalizing...");
            System.DateTime finalizationStart = System.DateTime.Now;

            double[,] heightBuffer = Height.FinalizeHeight(ref layers);
            // TODO make color map.

            LogTime("Finalization done", finalizationStart);

            if (saveToDisk) {
                Debug.Log("Saving...");
                System.DateTime savingStart = System.DateTime.Now;

                string savePath = System.Environment.GetFolderPath(saveLocation) + "/" + folderName + "/";
                System.IO.Directory.CreateDirectory(savePath);
				Textures.SaveTextureAsPNG(Conversion.CreateTexture(albedoMap.width, albedoMap.height, albedoBuffer), savePath + "Albedo_Aged_" + agingYears + ".png");
				Textures.SaveTextureAsPNG(Conversion.CreateTexture(heightMap.width, heightMap.height, heightBuffer), savePath + "Height_Aged_" + agingYears + ".png");

                LogTime("Saving done", savingStart);
			}

            LogTime("All done", initializationStart);
        }

        private static void LogTime(string text, System.DateTime startTime) {
            System.TimeSpan timeDifference = System.DateTime.Now - startTime;
            Debug.Log(text + " (" + (timeDifference.Minutes * 60 + timeDifference.Seconds + timeDifference.Milliseconds * 0.001) + " s).");
        }
    }
}
