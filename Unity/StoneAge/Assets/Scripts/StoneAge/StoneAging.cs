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

        [Header("Erosion parameters")]
        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float rainRate = 1.0f;
        [SerializeField]
        private ColorationParameters colorationParameters;
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

        [System.Serializable]
        public class ColorationParameters {
            [Range(1, 4)]
            public int blurRadius;
            [Range(0.0f, 2.0f)]
            public float erosionDarkening;
            [Range(1.0f, 20.0f)]
            public float noiseScale;
            public Gradient sedimentColor;
            [Range(0.0f, 2.0f)]
            public float sedimentOpacityModifier;
            public Color ironColor;
            [Range(0.0f, 2.0f)]
            public float ironOpacityModifier;
            [Range(50, 300)]
            public float ironGranularity;
            public Color efflorescenceColor;
            [Range(0.0f, 2.0f)]
            public float efflorescenceOpacityModifier;
            [Range(50, 300)]
            public float efflorescenceGranularity;
            public ColorationParameters(Gradient sedimentColor, Color ironColor, Color efflorescenceColor, int blurRadius = 2, float erosionDarkening = 0.4f, float noiseScale = 5.0f, float sedimentOpacityModifier = 2.0f, float ironOpacityModifier = 0.6f, float ironGranularity = 200, float efflorescenceOpacityModifier = 0.6f, float efflorescenceGranularity = 200) {
                this.blurRadius = blurRadius;
                this.erosionDarkening = erosionDarkening;
                this.noiseScale = noiseScale;
                if (sedimentColor == null) {
                    sedimentColor = new Gradient();
                    GradientColorKey[] keys = new GradientColorKey[] {
                        new GradientColorKey(new Color(0.5607843f, 0.5764706f, 0.5803922f), 0.0f),
                        new GradientColorKey(new Color(0.2745098f, 0.2941177f, 0.2901961f), 1.0f)
                    };
                    sedimentColor.colorKeys = keys;
                } else {
                    this.sedimentColor = sedimentColor;
                }
                this.sedimentOpacityModifier = sedimentOpacityModifier;
                if (ironColor == null) {
                    this.ironColor = new Color(1.0f, 0.5309945f, 0.0f);
                } else {
                    this.ironColor = ironColor;
                }
                this.ironOpacityModifier = ironOpacityModifier;
                this.ironGranularity = ironGranularity;
                if (efflorescenceColor == null) {
                    this.efflorescenceColor = Color.white;
                } else {
                    this.efflorescenceColor = efflorescenceColor;
                }
                this.efflorescenceOpacityModifier = efflorescenceOpacityModifier;
                this.efflorescenceGranularity = efflorescenceGranularity;
            }
        }

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

            // Create buffers from the input textures.
            Color[,] albedoBuffer = null;
            albedoBuffer = Conversion.CreateColorBuffer(albedoMap);

            float[,,] layers = new float[size, size, 2];
            float[,] originalRockHeight = Conversion.CreateFloatBuffer(heightMap);
            Conversion.FillBufferLayer(originalRockHeight, ref layers, (int) Erosion.LayerName.Rock);

            LogTime("Initialization done", initializationStart);

            // Perform the aging.
            Debug.Log("Aging...");
            if (EditorUtility.DisplayCancelableProgressBar("Aging", "Aging", completeWork++ / totalWork)) {
                CleanUp();
                return;
            }
            
            System.DateTime simulationStart = System.DateTime.Now;
            int rainDays = Mathf.FloorToInt(365.25f * rainRate);

            List<int> numSteps = new List<int>();
            float[,] visits = new float[size, size];

            for (int year = 0; year < agingYears; ++year) {
                System.DateTime yearStart = System.DateTime.Now;

                // Perform rain erosion.
                for (int rainDay = 0; rainDay < rainDays; ++rainDay) {
                    numSteps.Add(Erosion.ErosionEvent(ref layers, erosionParameters, ref visits));
                }

                if (EditorUtility.DisplayCancelableProgressBar("Aging", "Aged year " + (year + 1) + " / " + agingYears, completeWork++ / totalWork)) {
                    CleanUp();
                    return;
                }

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
            if (EditorUtility.DisplayCancelableProgressBar("Aging", "Finalizing", completeWork++ / totalWork)) {
                CleanUp();
                return;
            }
            System.DateTime finalizationStart = System.DateTime.Now;

            float[,] erosionBuffer = Conversion.DifferenceMap(originalRockHeight, Conversion.ExtractBufferLayer(layers, (int) Erosion.LayerName.Rock));
            Height.NormalizeHeight(ref erosionBuffer);

            float[,] heightBuffer = Height.FinalizeHeight(ref layers);

            float[,] ironNoise = Textures.PerlinNoise(size, colorationParameters.noiseScale);
            Coloration.SurfaceSolutionDiscoloration(ref albedoBuffer, ironNoise, colorationParameters.ironGranularity, colorationParameters.ironColor, colorationParameters.ironOpacityModifier, agingYears, effectiveMaxAge, true);

            float[,] efflorescenceNoise = Textures.PerlinNoise(size, colorationParameters.noiseScale);
            Coloration.SurfaceSolutionDiscoloration(ref albedoBuffer, efflorescenceNoise, colorationParameters.efflorescenceGranularity, colorationParameters.efflorescenceColor, colorationParameters.efflorescenceOpacityModifier, agingYears, effectiveMaxAge);

            Coloration.ColorErodedAreas(ref albedoBuffer, Textures.GaussianBlur(erosionBuffer, colorationParameters.blurRadius), colorationParameters.erosionDarkening, agingYears, effectiveMaxAge);

            float[,] sedimentBuffer = Conversion.ExtractBufferLayer(layers, (int) Erosion.LayerName.Sediment);
            Height.NormalizeHeight(ref sedimentBuffer);
            float[,] sedimentNoise = Textures.PerlinNoise(size, colorationParameters.noiseScale);
            Coloration.OverlaySediment(ref albedoBuffer, sedimentBuffer, colorationParameters.sedimentColor, colorationParameters.sedimentOpacityModifier, sedimentNoise);


            Height.NormalizeHeight(ref visits);

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
