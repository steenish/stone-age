using System.Collections.Generic;
using UnityEngine;
using Utility;
using System.Linq;

namespace StoneAge {
    public enum LoggingLevel {
        None = 0,
        Timing,
        Debug,
	}

    public class StoneAging : MonoBehaviour {
        [SerializeField]
        private LoggingLevel loggingLevel = LoggingLevel.Timing;
        [SerializeField]
        Texture2D albedoMap = null;
        [SerializeField]
        Texture2D heightMap = null;
        [SerializeField]
        private int agingYears;
        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float rainRate = 1.0f;
        [SerializeField]
        private Gradient sedimentColor;
        // TODO add erosion parameters
        [SerializeField]
        private bool saveToDisk = false;
        [SerializeField]
        private System.Environment.SpecialFolder saveLocation;
        [SerializeField]
        private string folderName = "StoneAge";
        [SerializeField]
        private bool saveDebugTextures = false;

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
            Color[,] albedoBuffer = null;
            albedoBuffer = Conversion.CreateColorBuffer(albedoMap);

            double[,,] layers = new double[heightMap.width, heightMap.height, 2];
            Conversion.FillDoubleBufferLayer(heightMap, ref layers, (int) Erosion.LayerName.Rock);

            LogTime("Initialization done", initializationStart);

            // Perform the aging.
            Debug.Log("Aging...");

            System.DateTime simulationStart = System.DateTime.Now;
            int rainDays = Mathf.FloorToInt(365.25f * rainRate);

            List<int> numSteps = new List<int>(); // TEMP

            for (int year = 0; year < agingYears; ++year) {
                System.DateTime yearStart = System.DateTime.Now;

                // Perform rain erosion.
                for (int rainDay = 0; rainDay < rainDays; ++rainDay) {
                    numSteps.Add(Erosion.ErosionEvent(ref layers)); // TEMP
				}

                LogTime("Aged " + (year + 1) + " year" + ((year + 1 == 1) ? "" : "s"), yearStart);
            }

            LogTime("Aging done", simulationStart);

            if (loggingLevel >= LoggingLevel.Debug) {
                int totalSteps = numSteps.Sum();
                int averageSteps = totalSteps / numSteps.Count;
                int numMaxSteps = numSteps.FindAll(e => e >= 999).Count;
                int maxStep = numSteps.Max();
                int minStep = numSteps.Min();
                Debug.Log("min steps: " + minStep + ", max steps: " + maxStep + ", average steps: " + averageSteps + ", times maxStep parameter reached: " + numMaxSteps + " / " + numSteps.Count + " = " + ((float) numMaxSteps / numSteps.Count));
			}

            Debug.Log("Finalizing...");
            System.DateTime finalizationStart = System.DateTime.Now;

            double[,] heightBuffer = Height.FinalizeHeight(ref layers);

            double[,] sedimentBuffer = Conversion.ExtractBufferLayer(layers, (int) Erosion.LayerName.Sediment);
            Height.NormalizeHeight(ref sedimentBuffer);
            albedoBuffer = Textures.OverlaySediment(albedoBuffer, sedimentBuffer, sedimentColor);

            LogTime("Finalization done", finalizationStart);

            if (saveToDisk) {
                Debug.Log("Saving...");
                System.DateTime savingStart = System.DateTime.Now;

                string savePath = System.Environment.GetFolderPath(saveLocation) + "/" + folderName + "/";
                System.IO.Directory.CreateDirectory(savePath);
				Textures.SaveTextureAsPNG(Conversion.CreateTexture(albedoMap.width, albedoMap.height, albedoBuffer), savePath + "Albedo_Aged_" + agingYears + ".png");
				Textures.SaveTextureAsPNG(Conversion.CreateTexture(heightMap.width, heightMap.height, heightBuffer), savePath + "Height_Aged_" + agingYears + ".png");

                if (saveDebugTextures) {
                    Textures.SaveTextureAsPNG(Conversion.CreateTexture(heightMap.width, heightMap.height, sedimentBuffer), savePath + "Sediment_Buffer_" + agingYears + ".png");
                }

                LogTime("Saving done", savingStart);
			}

            LogTime("All done", initializationStart);
        }

        private void LogTime(string text, System.DateTime startTime) {
            if (loggingLevel >= LoggingLevel.Timing) {
                System.TimeSpan timeDifference = System.DateTime.Now - startTime;
                Debug.Log(text + " (" + (timeDifference.Minutes * 60 + timeDifference.Seconds + timeDifference.Milliseconds * 0.001) + " s).");
			}
        }
    }
}
