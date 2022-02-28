using System.Collections.Generic;
using UnityEngine;
using Utility;
using System.Linq;
using UnityEditor;

namespace StoneAge {
    public enum LoggingLevel {
        None = 0,
        Timing,
    }

    public enum DebugTextures {
        ErosionBuffer = 1,
        SedimentBuffer = 2,
        VisitBuffer = 4,
        LichenBuffer = 8,
        LichenHeightBuffer = 16
    }

    public class StoneAging : EditorWindow {
        [SerializeField]
        private LoggingLevel loggingLevel = LoggingLevel.Timing;

        [SerializeField]
        private Texture2D albedoMap = null;
        [SerializeField]
        private Texture2D heightMap = null;
        [SerializeField]
        private Texture2D roughnessMap = null;

        [SerializeField]
        private int agingYears = 100;
        [SerializeField]
        private int effectiveMaxAge = 1000;
        [SerializeField]
        private int seed;

        [Range(0.0f, 1.0f)]
        [SerializeField]
        private float rainRate = 1.0f;
        [SerializeField]
        private Coloration.ColorationParameters colorationParameters;
        [SerializeField]
        private Erosion.ErosionParameters erosionParameters;

        [SerializeField]
        private LichenGrowth.LichenParameters lichenParameters;

        [SerializeField]
        private Height.RoughnessParameters roughnessParameters;

        [SerializeField]
        private bool saveToDisk = false;
        [SerializeField]
        private System.Environment.SpecialFolder saveLocation;
        [SerializeField]
        private string folderName = "StoneAge";

        private static int additionalTextureFlags = 0;
        private static readonly string[] additionalTextureOptions = new string[] { "Erosion buffer", "Sediment buffer", "Visit buffer", "Lichen buffer", "Lichen height buffer" };

        private Vector2 scrollPos;

        private SerializedObject serializedObject;
        private SerializedProperty propLoggingLevel;
        private SerializedProperty propAlbedoMap;
        private SerializedProperty propHeightMap;
        private SerializedProperty propRoughnessMap;
        private SerializedProperty propAgingYears;
        private SerializedProperty propEffectiveMaxAge;
        private SerializedProperty propSeed;
        private SerializedProperty propRainRate;
        private SerializedProperty propColorationParameters;
        private SerializedProperty propErosionParameters;
        private SerializedProperty propLichenParameters;
        private SerializedProperty propRoughnessParameters;
        private SerializedProperty propSaveToDisk;
        private SerializedProperty propSaveLocation;
        private SerializedProperty propFolderName;

        [MenuItem("Tools/Stone aging")]
        public static void OpenTool() => GetWindow<StoneAging>("Stone aging");

        private void OnEnable() {
            serializedObject = new SerializedObject(this);
            propLoggingLevel = serializedObject.FindProperty("loggingLevel");
            propAlbedoMap = serializedObject.FindProperty("albedoMap");
            propHeightMap = serializedObject.FindProperty("heightMap");
            propRoughnessMap = serializedObject.FindProperty("roughnessMap");
            propAgingYears = serializedObject.FindProperty("agingYears");
            propEffectiveMaxAge = serializedObject.FindProperty("effectiveMaxAge");
            propSeed = serializedObject.FindProperty("seed");
            propRainRate = serializedObject.FindProperty("rainRate");
            propColorationParameters = serializedObject.FindProperty("colorationParameters");
            propErosionParameters = serializedObject.FindProperty("erosionParameters");
            propLichenParameters = serializedObject.FindProperty("lichenParameters");
            propRoughnessParameters = serializedObject.FindProperty("roughnessParameters");
            propSaveToDisk = serializedObject.FindProperty("saveToDisk");
            propSaveLocation = serializedObject.FindProperty("saveLocation");
            propFolderName = serializedObject.FindProperty("folderName");
        }

        private void OnGUI() {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(propLoggingLevel);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Input textures", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(propAlbedoMap);
            EditorGUILayout.PropertyField(propHeightMap);
            EditorGUILayout.PropertyField(propRoughnessMap);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("General parameters", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(propAgingYears);
            EditorGUILayout.PropertyField(propEffectiveMaxAge);
            EditorGUILayout.PropertyField(propSeed);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Simulation parameters", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(propRainRate);
            EditorGUILayout.PropertyField(propColorationParameters);
            EditorGUILayout.PropertyField(propErosionParameters);
            EditorGUILayout.PropertyField(propLichenParameters);
            EditorGUILayout.PropertyField(propRoughnessParameters);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Export settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(propSaveToDisk);

            if (propSaveToDisk.boolValue) {
                EditorGUILayout.PropertyField(propSaveLocation);
                EditorGUILayout.PropertyField(propFolderName);
                additionalTextureFlags = EditorGUILayout.MaskField("Save additional textures", additionalTextureFlags, additionalTextureOptions);
            }

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            if (GUILayout.Button("Simulate")) {
                StartAging();
            }

            EditorGUILayout.EndScrollView();
        }

        private void StartAging() {
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

            bool roughnessMapNull = roughnessMap == null;

            if (albedoMap.width != albedoMap.height || albedoMap.width != heightMap.width || albedoMap.height != heightMap.height || (!roughnessMapNull && (roughnessMap.height != roughnessMap.width || roughnessMap.width != albedoMap.width))) {
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

            float[,] roughnessBuffer = null;
            if (!roughnessMapNull) {
                roughnessBuffer = Conversion.CreateFloatBuffer(roughnessMap);
            } else {
                roughnessBuffer = new float[size, size];

                for (int y = 0; y < size; ++y) {
                    for (int x = 0; x < size; ++x) {
                        roughnessBuffer[x, y] = 0.5f;
                    }
                }
            }

            List<LichenGrowth.Cluster> lichenClusters = new List<LichenGrowth.Cluster>();
            for (int i = 0; i < lichenParameters.initialSeeds; ++i) {
                LichenGrowth.SpawnCluster(ref lichenClusters, size, lichenParameters);
            }

            Shader utilityShader = Shader.Find("Hidden/Utility");

            LogTime("Initialization done", initializationStart);

            // Perform the aging.
            Debug.Log("Aging...");
            if (EditorUtility.DisplayCancelableProgressBar("Aging", "Aging", completeWork++ / totalWork)) {
                CleanUp();
                return;
            }
            
            System.DateTime simulationStart = System.DateTime.Now;

            float[,] visits = new float[size, size];

            for (int year = 0; year < agingYears; ++year) {
                System.DateTime yearStart = System.DateTime.Now;

                // Simulate each day.
                for (int day = 0; day < 365; ++day) {

                    // Perform hydraulic (rain) erosion.
                    if (Random.value < rainRate) {
                        Erosion.ErosionEvent(ref layers, erosionParameters, ref visits);
                    }

                    // Perform lichen growth.
                    List<LichenGrowth.Cluster> dailyClusters = lichenClusters.OrderBy(x => Random.value).Take(lichenParameters.maxClustersPerDay).ToList();
                    for (int i = 0; i < dailyClusters.Count; ++i) {
                        LichenGrowth.LichenGrowthEvent(dailyClusters[i], size, lichenParameters, layers);
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

            Debug.Log("Finalizing...");
            if (EditorUtility.DisplayCancelableProgressBar("Aging", "Finalizing", completeWork++ / totalWork)) {
                CleanUp();
                return;
            }
            System.DateTime finalizationStart = System.DateTime.Now;

            float[,] erosionBuffer = Conversion.DifferenceMap(originalRockHeight, Conversion.ExtractBufferLayer(layers, (int) Erosion.LayerName.Rock));
            Height.Normalize(ref erosionBuffer);

            float[,] heightBuffer = Height.FinalizeHeight(ref layers);

            Coloration.SurfaceSolutionDiscoloration(ref albedoBuffer, ironNoise, colorationParameters.ironGranularity, colorationParameters.ironColor, colorationParameters.ironOpacityModifier, agingYears, effectiveMaxAge, true);

            Coloration.SurfaceSolutionDiscoloration(ref albedoBuffer, efflorescenceNoise, colorationParameters.efflorescenceGranularity, colorationParameters.efflorescenceColor, colorationParameters.efflorescenceOpacityModifier, agingYears, effectiveMaxAge);

            Coloration.ColorErodedAreas(ref albedoBuffer, Textures.GaussianBlur(erosionBuffer, colorationParameters.blurRadius), colorationParameters.erosionDarkening, agingYears, effectiveMaxAge);

            float[,] sedimentBuffer = Conversion.ExtractBufferLayer(layers, (int) Erosion.LayerName.Sediment);
            Height.Normalize(ref sedimentBuffer);
            Coloration.OverlaySediment(ref albedoBuffer, sedimentBuffer, colorationParameters.sedimentColor, colorationParameters.sedimentOpacityModifier, sedimentNoise);

            Height.Normalize(ref visits);

            Texture2D[] lichenResults = LichenGrowth.CreateLichenTexture(lichenClusters, size, Conversion.CreateTexture(size, albedoBuffer), utilityShader, lichenParameters);
            float[,] lichenHeight = Conversion.CreateFloatBuffer(lichenResults[1]);
            lichenHeight = Conversion.ScalarMultMap(lichenHeight, lichenParameters.lichenHeightScale);
            heightBuffer = Conversion.SumMap(heightBuffer, lichenHeight);
            Height.Normalize(ref heightBuffer);

            Height.GenerateRoughness(ref roughnessBuffer, erosionBuffer, sedimentBuffer, lichenHeight, roughnessParameters);
            Height.Normalize(ref roughnessBuffer);

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
                Textures.SaveTextureAsPNG(lichenResults[0], savePath + "Albedo_Aged_" + agingYears + ".png");
                Textures.SaveTextureAsPNG(Conversion.CreateTexture(size, heightBuffer), savePath + "Height_Aged_" + agingYears + ".png");
                Textures.SaveTextureAsPNG(Conversion.CreateTexture(size, roughnessBuffer), savePath + "Roughness_Aged_" + agingYears + ".png");

                if ((additionalTextureFlags & (int) DebugTextures.ErosionBuffer) > 0) {
                    Textures.SaveTextureAsPNG(Conversion.CreateTexture(size, erosionBuffer), savePath + "Erosion_Buffer_" + agingYears + ".png");
                }

                if ((additionalTextureFlags & (int) DebugTextures.SedimentBuffer) > 0) {
                    Textures.SaveTextureAsPNG(Conversion.CreateTexture(size, sedimentBuffer), savePath + "Sediment_Buffer_" + agingYears + ".png");
                }

                if ((additionalTextureFlags & (int) DebugTextures.VisitBuffer) > 0) {
                    Textures.SaveTextureAsPNG(Conversion.CreateTexture(size, visits), savePath + "Visit_Buffer_" + agingYears + ".png");
                }

                if ((additionalTextureFlags & (int) DebugTextures.LichenBuffer) > 0) {
                    Textures.SaveTextureAsPNG(lichenResults[2], savePath + "Lichen_Buffer_" + agingYears + ".png");
                }

                if ((additionalTextureFlags & (int) DebugTextures.LichenHeightBuffer) > 0) {
                    Textures.SaveTextureAsPNG(lichenResults[1], savePath + "Lichen_Height_Buffer_" + agingYears + ".png");
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
