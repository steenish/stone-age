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
        private int numSteps;
        [SerializeField]
        private int effectiveMaxSteps = 1000;
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

            float totalWork = this.numSteps + 3 + (saveToDisk ? 1 : 0);
            float completeWork = 0;
            int size = heightMap.width;

            Debug.Log("Initializing...");
            EditorUtility.DisplayProgressBar("Aging", "Initializing", completeWork++ / totalWork);
            System.DateTime initializationStart = System.DateTime.Now;

            Random.InitState(seed);

            if (!customErosionParameters) {
                erosionParameters = new Erosion.ErosionParameters();
            }

            // Create buffers from the input textures.
            Color[,] albedoBuffer = Conversion.CreateColorBuffer(albedoMap);
            float[,] heightBuffer = Conversion.CreateFloatBuffer(heightMap);

            LogTime("Initialization done", initializationStart);

            // Perform the aging.
            Debug.Log("Aging...");
            EditorUtility.DisplayProgressBar("Aging", "Aging", completeWork++ / totalWork);

            System.DateTime simulationStart = System.DateTime.Now;
            int rainDays = Mathf.FloorToInt(365.25f * rainRate);

            List<int> numSteps = new List<int>();

            for (int step = 0; step < this.numSteps; ++step) {
                System.DateTime stepStart = System.DateTime.Now;

                // Perform rain erosion.
                for (int rainDay = 0; rainDay < rainDays; ++rainDay) {
                    if (customErosionParameters) {
                        numSteps.Add(Erosion.ErosionEvent(ref heightBuffer, erosionParameters));
                    } else {
                        numSteps.Add(Erosion.ErosionEvent(ref heightBuffer));
                    }
                }

                EditorUtility.DisplayProgressBar("Aging", "Aged step " + (step + 1) + " / " + this.numSteps, completeWork++ / totalWork);

                LogTime("Aged " + (step + 1) + " step" + ((step + 1 == 1) ? "" : "s"), stepStart);
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

            // TODO do this with shaders instead probably
            //float[,] rockErosion = Conversion.DifferenceMap(originalRockHeight, Conversion.ExtractBufferLayer(heightBuffer, (int) Erosion.LayerName.Rock));
            Height.NormalizeHeight(ref heightBuffer); // TODO this could be done with a shader once the max and min heights have been found

            // TODO do this with shaders instead probably
            //Textures.ColorErodedAreas(ref albedoBuffer, Textures.GaussianBlur(rockErosion, blurRadius), this.numSteps, effectiveMaxSteps);

            // TODO do this with shaders instead probably
            //float[,] sedimentBuffer = Conversion.ExtractBufferLayer(heightBuffer, (int) Erosion.LayerName.Sediment);
            //Height.NormalizeHeight(ref sedimentBuffer);
            //albedoBuffer = Textures.OverlaySediment(albedoBuffer, sedimentBuffer, sedimentColor, sedimentOpacityModifier);

            LogTime("Finalization done", finalizationStart);

            if (saveToDisk) {
                Debug.Log("Saving...");
                EditorUtility.DisplayProgressBar("Aging", "Saving", completeWork++ / totalWork);
                System.DateTime savingStart = System.DateTime.Now;

                string savePath = System.Environment.GetFolderPath(saveLocation) + "/" + folderName + "/";
                System.IO.Directory.CreateDirectory(savePath);
                Textures.SaveTextureAsPNG(Conversion.CreateTexture(albedoMap.width, albedoBuffer), savePath + "Albedo_Aged_" + this.numSteps + ".png");
                Textures.SaveTextureAsPNG(Conversion.CreateTexture(heightMap.width, heightBuffer), savePath + "Height_Aged_" + this.numSteps + ".png");

                // TODO re-implement once the buffers are calculated again
                //if (saveDebugTextures) {
                //    Textures.SaveTextureAsPNG(Conversion.CreateTexture(heightMap.width, rockErosion), savePath + "Rock_Erosion_" + this.numSteps + ".png");
                //    Textures.SaveTextureAsPNG(Conversion.CreateTexture(heightMap.width, sedimentBuffer), savePath + "Sediment_Buffer_" + this.numSteps + ".png");
                //}

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
