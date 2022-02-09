using System.Collections.Generic;
using UnityEngine;
using Utility;
using System.Linq;
using UnityEditor;

namespace StoneAge {
    public enum LoggingLevel {
        None = 0,
        Timing,
        Debug,
    }

    public class StoneAging : MonoBehaviour {
        [Header("Settings")]
        [SerializeField]
        private LoggingLevel loggingLevel = LoggingLevel.Timing;

        [Header("Input textures")]
        [SerializeField]
        Texture2D albedoMap = null;
        [SerializeField]
        Texture2D heightMap = null;

        [Header("General parameters")]
        [SerializeField]
        private int agingYears;
        [SerializeField]
        private int effectiveMaxAge = 1000;
        [SerializeField]
        private int seed;

        [Header("Erosion parameters")]
        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float rainRate = 1.0f;
        [SerializeField]
        [Range(1, 4)]
        private int blurRadius = 2;
        [SerializeField]
        private Gradient sedimentColor;
        [SerializeField]
        private float sedimentOpacityModifier = 1.0f;
        [SerializeField]
        private bool customErosionParameters = true;
        [SerializeField]
        private Erosion.ErosionParameters erosionParameters;

        [Header("Export settings")]
        [SerializeField]
        private bool saveToDisk = false;
        [SerializeField]
        private System.Environment.SpecialFolder saveLocation;
        [SerializeField]
        private string folderName = "StoneAge";
        [SerializeField]
        private bool saveDebugTextures = false;

        public void PerformAging() {
            // TODO CHECK MAPS EQUAL SIZE, AND SQUARE

            if (albedoMap == null) {
                Debug.LogError("No albedo map supplied.");
                return;
            }

            if (heightMap == null) {
                Debug.LogError("No height map supplied.");
                return;
            }

            if (albedoMap.width != albedoMap.height || albedoMap.width != heightMap.width || albedoMap.height != heightMap.height) {
                Debug.LogError("Maps are not the same size or not square.");
                return;
            }

            float totalWork = agingYears + 3 + (saveToDisk ? 1 : 0);
            float completeWork = 0;

            Debug.Log("Initializing...");
            EditorUtility.DisplayProgressBar("Aging", "Initializing", completeWork++ / totalWork);
            System.DateTime initializationStart = System.DateTime.Now;

            Random.InitState(seed);

            if (!customErosionParameters) {
                erosionParameters = new Erosion.ErosionParameters();
            }

            // Create buffers from the input textures.
            Color[,] albedoBuffer = null;
            albedoBuffer = Conversion.CreateColorBuffer(albedoMap);

            float[,,] layers = new float[heightMap.width, heightMap.height, 2];
            float[,] originalRockHeight = Conversion.CreateFloatBuffer(heightMap);
            Conversion.FillBufferLayer(originalRockHeight, ref layers, (int) Erosion.LayerName.Rock);

            LogTime("Initialization done", initializationStart);

            // Perform the aging.
            Debug.Log("Aging...");
            EditorUtility.DisplayProgressBar("Aging", "Aging", completeWork++ / totalWork);

            System.DateTime simulationStart = System.DateTime.Now;
            int rainDays = Mathf.FloorToInt(365.25f * rainRate);

            List<int> numSteps = new List<int>();

            for (int year = 0; year < agingYears; ++year) {
                System.DateTime yearStart = System.DateTime.Now;

                // Perform rain erosion.
                for (int rainDay = 0; rainDay < rainDays; ++rainDay) {
                    if (customErosionParameters) {
                        numSteps.Add(Erosion.ErosionEvent(ref layers, erosionParameters));
                    } else {
                        numSteps.Add(Erosion.ErosionEvent(ref layers));
                    }
                }

                EditorUtility.DisplayProgressBar("Aging", "Aged year " + (year + 1) + " / " + agingYears, completeWork++ / totalWork);

                LogTime("Aged " + (year + 1) + " year" + ((year + 1 == 1) ? "" : "s"), yearStart);
            }

            LogTime("Aging done", simulationStart);

            if (loggingLevel >= LoggingLevel.Debug) {
                int totalSteps = numSteps.Sum();
                int averageSteps = totalSteps / numSteps.Count;
                int numMaxSteps = numSteps.FindAll(e => e >= erosionParameters.maxPath-1).Count;
                int maxStep = numSteps.Max();
                int minStep = numSteps.Min();
                Debug.Log("min steps: " + minStep + ", max steps: " + maxStep + ", average steps: " + averageSteps + ", times maxStep parameter reached: " + numMaxSteps + " / " + numSteps.Count + " = " + ( numMaxSteps / numSteps.Count));
            }

            Debug.Log("Finalizing...");
            EditorUtility.DisplayProgressBar("Aging", "Finalizing", completeWork++ / totalWork);
            System.DateTime finalizationStart = System.DateTime.Now;

            float[,] rockErosion = Conversion.DifferenceMap(originalRockHeight, Conversion.ExtractBufferLayer(layers, (int) Erosion.LayerName.Rock));
            Height.NormalizeHeight(ref rockErosion);

            float[,] heightBuffer = Height.FinalizeHeight(ref layers);

            Textures.ColorErodedAreas(ref albedoBuffer, Textures.GaussianBlur(rockErosion, blurRadius), agingYears, effectiveMaxAge);

            float[,] sedimentBuffer = Conversion.ExtractBufferLayer(layers, (int) Erosion.LayerName.Sediment);
            Height.NormalizeHeight(ref sedimentBuffer);
            albedoBuffer = Textures.OverlaySediment(albedoBuffer, sedimentBuffer, sedimentColor, sedimentOpacityModifier);

            LogTime("Finalization done", finalizationStart);

            if (saveToDisk) {
                Debug.Log("Saving...");
                EditorUtility.DisplayProgressBar("Aging", "Saving", completeWork++ / totalWork);
                System.DateTime savingStart = System.DateTime.Now;

                string savePath = System.Environment.GetFolderPath(saveLocation) + "/" + folderName + "/";
                System.IO.Directory.CreateDirectory(savePath);
                Textures.SaveTextureAsPNG(Conversion.CreateTexture(albedoMap.width, albedoMap.height, albedoBuffer), savePath + "Albedo_Aged_" + agingYears + ".png");
                Textures.SaveTextureAsPNG(Conversion.CreateTexture(heightMap.width, heightMap.height, heightBuffer), savePath + "Height_Aged_" + agingYears + ".png");

                if (saveDebugTextures) {
                    Textures.SaveTextureAsPNG(Conversion.CreateTexture(heightMap.width, heightMap.height, rockErosion), savePath + "Rock_Erosion_" + agingYears + ".png");
                    Textures.SaveTextureAsPNG(Conversion.CreateTexture(heightMap.width, heightMap.height, sedimentBuffer), savePath + "Sediment_Buffer_" + agingYears + ".png");
                }

                LogTime("Saving done", savingStart);
            }
            EditorUtility.ClearProgressBar();
            LogTime("All done", initializationStart);
        }

        private void LogTime(string text, System.DateTime startTime) {
            if (loggingLevel >= LoggingLevel.Timing) {
                System.TimeSpan timeDifference = System.DateTime.Now - startTime;
                Debug.Log(text + " (" + (timeDifference.Hours * 3600 + timeDifference.Minutes * 60 + timeDifference.Seconds + timeDifference.Milliseconds * 0.001) + " s).");
            }
        }
    }
}
