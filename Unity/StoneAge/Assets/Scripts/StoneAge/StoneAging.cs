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
        private Texture2D albedoMap = null;
        [SerializeField]
        private Texture2D heightMap = null;

        [Header("General parameters")]
        [SerializeField]
        private int agingYears = 100;
        [SerializeField]
        private int effectiveMaxAge = 1000;
        [SerializeField]
        private int seed;

        [Header("Erosion settings")]
        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float rainRate = 1.0f;
        [SerializeField]
        private Coloration.ColorationParameters colorationParameters;
        [SerializeField]
        private Erosion.ErosionParameters erosionParameters;

        [Header("Lichen settings")]
        [SerializeField]
        private LichenGrowth.LichenParameters lichenParameters;

        [Header("Export settings")]
        [SerializeField]
        private bool saveToDisk = false;
        [SerializeField]
        private System.Environment.SpecialFolder saveLocation;
        [SerializeField]
        private string folderName = "StoneAge";
        [SerializeField]
        private bool saveDebugTextures = false;

        public void StartAging() {
            try {
                PerformAging();
            } catch (System.Exception e) {
                CleanUp();
                Debug.LogError(e);
            }
        }

        private void PerformAging() {
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
            if (EditorUtility.DisplayCancelableProgressBar("Aging", "Initializing", completeWork++ / totalWork)) {
                CleanUp();
                return;
            }
            System.DateTime initializationStart = System.DateTime.Now;

            Random.InitState(seed);

            int size = albedoMap.width;
            float[,] sedimentNoise = Textures.PerlinNoise(size, colorationParameters.noiseScale);
            float[,] ironNoise = Textures.PerlinNoise(size, colorationParameters.noiseScale);
            float[,] efflorescenceNoise = Textures.PerlinNoise(size, colorationParameters.noiseScale);

            // Create buffers from the input textures.
            Color[,] albedoBuffer = Conversion.CreateColorBuffer(albedoMap);

            float[,,] layers = new float[size, size, 2];
            float[,] originalRockHeight = Conversion.CreateFloatBuffer(heightMap);
            Conversion.FillBufferLayer(originalRockHeight, ref layers, (int) Erosion.LayerName.Rock);

            List<LichenGrowth.Cluster> lichenClusters = new List<LichenGrowth.Cluster>();
            for (int i = 0; i < lichenParameters.initialSeeds; ++i) {
                LichenGrowth.SpawnCluster(ref lichenClusters, size, lichenParameters);
            }

            LogTime("Initialization done", initializationStart);

            // Perform the aging.
            Debug.Log("Aging...");
            if (EditorUtility.DisplayCancelableProgressBar("Aging", "Aging", completeWork++ / totalWork)) {
                CleanUp();
                return;
            }
            
            System.DateTime simulationStart = System.DateTime.Now;

            List<int> numSteps = new List<int>();
            float[,] visits = new float[size, size];

            for (int year = 0; year < agingYears; ++year) {
                System.DateTime yearStart = System.DateTime.Now;

                // Simulate each day.
                for (int day = 0; day < 365; ++day) {

                    // Perform hydraulic (rain) erosion.
                    if (Random.value < rainRate) {
                        numSteps.Add(Erosion.ErosionEvent(ref layers, erosionParameters, ref visits));
                    }

                    // Perform lichen growth.
                    for (int i = 0; i < lichenClusters.Count; ++i) {
                        LichenGrowth.LichenGrowthEvent(lichenClusters, i, size, lichenParameters, layers);
                    }

                    // Spawn new lichen seeds.
                    if (Random.value < lichenParameters.newSeedProbability) {
                        LichenGrowth.SpawnCluster(ref lichenClusters, size, lichenParameters);
                    }
                }

                if (EditorUtility.DisplayCancelableProgressBar("Aging", "Aged year " + (year + 1) + " / " + agingYears, completeWork++ / totalWork)) {
                    CleanUp();
                    return;
                }

                LogTime("Aged " + (year + 1) + " year" + ((year + 1 == 1) ? "" : "s"), yearStart);
            }

            LogTime("Aging done", simulationStart);

            if (loggingLevel >= LoggingLevel.Debug && numSteps.Count > 0) {
                int totalSteps = numSteps.Sum();
                int averageSteps = totalSteps / numSteps.Count;
                int numMaxSteps = numSteps.FindAll(e => e >= erosionParameters.maxPath-1).Count;
                int maxStep = numSteps.Max();
                int minStep = numSteps.Min();
                Debug.Log("min steps: " + minStep + ", max steps: " + maxStep + ", average steps: " + averageSteps + ", times maxStep parameter reached: " + numMaxSteps + " / " + numSteps.Count + " = " + ( numMaxSteps / numSteps.Count));
            }

            Debug.Log("Finalizing...");
            if (EditorUtility.DisplayCancelableProgressBar("Aging", "Finalizing", completeWork++ / totalWork)) {
                CleanUp();
                return;
            }
            System.DateTime finalizationStart = System.DateTime.Now;

            float[,] erosionBuffer = Conversion.DifferenceMap(originalRockHeight, Conversion.ExtractBufferLayer(layers, (int) Erosion.LayerName.Rock));
            Height.NormalizeHeight(ref erosionBuffer);

            float[,] heightBuffer = Height.FinalizeHeight(ref layers);

            Coloration.SurfaceSolutionDiscoloration(ref albedoBuffer, ironNoise, colorationParameters.ironGranularity, colorationParameters.ironColor, colorationParameters.ironOpacityModifier, agingYears, effectiveMaxAge, true);

            Coloration.SurfaceSolutionDiscoloration(ref albedoBuffer, efflorescenceNoise, colorationParameters.efflorescenceGranularity, colorationParameters.efflorescenceColor, colorationParameters.efflorescenceOpacityModifier, agingYears, effectiveMaxAge);

            Coloration.ColorErodedAreas(ref albedoBuffer, Textures.GaussianBlur(erosionBuffer, colorationParameters.blurRadius), colorationParameters.erosionDarkening, agingYears, effectiveMaxAge);

            float[,] sedimentBuffer = Conversion.ExtractBufferLayer(layers, (int) Erosion.LayerName.Sediment);
            Height.NormalizeHeight(ref sedimentBuffer);
            Coloration.OverlaySediment(ref albedoBuffer, sedimentBuffer, colorationParameters.sedimentColor, colorationParameters.sedimentOpacityModifier, sedimentNoise);

            Height.NormalizeHeight(ref visits);

            Color[,] lichenBuffer = LichenGrowth.CreateLichenBuffer(lichenClusters, size);
            Coloration.OverlayLichens(ref albedoBuffer, lichenBuffer);

            LogTime("Finalization done", finalizationStart);

            if (saveToDisk) {
                Debug.Log("Saving...");
                if (EditorUtility.DisplayCancelableProgressBar("Aging", "Saving", completeWork++ / totalWork)) {
                    CleanUp();
                    return;
                }
                System.DateTime savingStart = System.DateTime.Now;

                string savePath = System.Environment.GetFolderPath(saveLocation) + "/" + folderName + "/";
                System.IO.Directory.CreateDirectory(savePath);
                Textures.SaveTextureAsPNG(Conversion.CreateTexture(size, albedoBuffer), savePath + "Albedo_Aged_" + agingYears + ".png");
                Textures.SaveTextureAsPNG(Conversion.CreateTexture(size, heightBuffer), savePath + "Height_Aged_" + agingYears + ".png");

                if (saveDebugTextures) {
                    Textures.SaveTextureAsPNG(Conversion.CreateTexture(size, erosionBuffer), savePath + "Erosion_Buffer_" + agingYears + ".png");
                    Textures.SaveTextureAsPNG(Conversion.CreateTexture(size, sedimentBuffer), savePath + "Sediment_Buffer_" + agingYears + ".png");
                    Textures.SaveTextureAsPNG(Conversion.CreateTexture(size, visits), savePath + "Visit_Buffer_" + agingYears + ".png");
                    Textures.SaveTextureAsPNG(Conversion.CreateTexture(size, lichenBuffer), savePath + "Lichen_Buffer_" + agingYears + ".png");
                }

                LogTime("Saving done", savingStart);
            }
            CleanUp();
            LogTime("All done", initializationStart);
        }

        private void LogTime(string text, System.DateTime startTime) {
            if (loggingLevel >= LoggingLevel.Timing) {
                System.TimeSpan timeDifference = System.DateTime.Now - startTime;
                Debug.Log(text + " (" + (timeDifference.Hours * 3600 + timeDifference.Minutes * 60 + timeDifference.Seconds + timeDifference.Milliseconds * 0.001) + " s).");
            }
        }

        private void CleanUp() {
            EditorUtility.ClearProgressBar();
        }
    }
}
