using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Utility;

namespace StoneAge {

    public enum DebugTextures {
        ErosionBuffer = 1,
        SedimentBuffer = 2,
        VisitBuffer = 4,
        LichenBuffer = 8,
        LichenHeightBuffer = 16
    }

    public class StoneAging : EditorWindow {
        [SerializeField]
        private Texture2D albedoMap = null;
        [SerializeField]
        private Texture2D heightMap = null;
        [SerializeField]
        private Texture2D roughnessMap = null;

        [SerializeField]
        private int iterations = 30000;
        [SerializeField]
        private int effectiveMaxIterations = 300000;
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
        private string savePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);
        [SerializeField]
        private bool animate = false;
        [SerializeField]
        private int iterationsPerFrame = 10;

        private static int additionalTextureFlags = 0;
        private static readonly string[] additionalTextureOptions = new string[] { "Erosion buffer", "Sediment buffer", "Visit buffer", "Lichen buffer", "Lichen height buffer" };

        [SerializeField]
        private bool simulateSequence = false;
        private string currentSettingName;

        private Vector2 scrollPos;

        private SerializedObject serializedObject;
        private SerializedProperty propAlbedoMap;
        private SerializedProperty propHeightMap;
        private SerializedProperty propRoughnessMap;
        private SerializedProperty propIterations;
        private SerializedProperty propEffectiveMaxIterations;
        private SerializedProperty propSeed;
        private SerializedProperty propRainRate;
        private SerializedProperty propColorationParameters;
        private SerializedProperty propErosionParameters;
        private SerializedProperty propLichenParameters;
        private SerializedProperty propRoughnessParameters;
        private SerializedProperty propSavePath;
        private SerializedProperty propAnimate;
        private SerializedProperty propIterationsPerFrame;
        private SerializedProperty propSimulateSequence;

        [MenuItem("Tools/Stone aging")]
        public static void OpenTool() => GetWindow<StoneAging>("Stone aging");

        private void OnEnable() {
            serializedObject = new SerializedObject(this);
            propAlbedoMap = serializedObject.FindProperty("albedoMap");
            propHeightMap = serializedObject.FindProperty("heightMap");
            propRoughnessMap = serializedObject.FindProperty("roughnessMap");
            propIterations = serializedObject.FindProperty("iterations");
            propEffectiveMaxIterations = serializedObject.FindProperty("effectiveMaxIterations");
            propSeed = serializedObject.FindProperty("seed");
            propRainRate = serializedObject.FindProperty("rainRate");
            propColorationParameters = serializedObject.FindProperty("colorationParameters");
            propErosionParameters = serializedObject.FindProperty("erosionParameters");
            propLichenParameters = serializedObject.FindProperty("lichenParameters");
            propRoughnessParameters = serializedObject.FindProperty("roughnessParameters");
            propSavePath = serializedObject.FindProperty("savePath");
            propAnimate = serializedObject.FindProperty("animate");
            propIterationsPerFrame = serializedObject.FindProperty("iterationsPerFrame");
            propSimulateSequence = serializedObject.FindProperty("simulateSequence");
            DeserializeAndLoadTemp();
        }

        private void OnDisable() {
            SerializeAndStoreTemp();
        }

        private void OnGUI() {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            EditorGUILayout.LabelField("Parameter import/export", EditorStyles.boldLabel);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Import parameter settings")) {
                DeserializeAndImport();
            }
            if (GUILayout.Button("Export parameter settings")) {
                SerializeAndExport();
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Input textures", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(propAlbedoMap);
            EditorGUILayout.PropertyField(propHeightMap);
            EditorGUILayout.PropertyField(propRoughnessMap);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("General parameters", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(propIterations);
            EditorGUILayout.PropertyField(propEffectiveMaxIterations);
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

            EditorGUILayout.LabelField($"Save path: {savePath}/");
            if (GUILayout.Button("Select save path")) {
                SelectSavePath();
            }
            
            additionalTextureFlags = EditorGUILayout.MaskField("Save additional textures", additionalTextureFlags, additionalTextureOptions);

            GUILayout.BeginHorizontal();
            EditorGUILayout.PropertyField(propAnimate);
            if (propAnimate.boolValue) {
                EditorGUILayout.PropertyField(propIterationsPerFrame);
            }
            GUILayout.EndHorizontal();

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            EditorGUILayout.PropertyField(propSimulateSequence);
            if (propSimulateSequence.boolValue) {
                if (GUILayout.Button("Simulate sequence")) {
                    AgeSequence();
                }
            } else {
                if (GUILayout.Button("Simulate")) {
                    StartAging();
                }
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

        private void AgeSequence() {
            string sequencePath = EditorUtility.OpenFilePanel("Select setting sequence file", Application.dataPath, "txt");
            if (sequencePath.Length > 0) {
                string[] settingNames = System.IO.File.ReadAllLines(sequencePath);
                bool keepGoing = true;
                try {
                    for (int i = 0; i < settingNames.Length && keepGoing; ++i) {
                        string path = $"{Application.dataPath}/ParameterSettings/{settingNames[i]}.json";
                        DeserializeAndImport(path);
                        currentSettingName = settingNames[i];
                        simulateSequence = true;
                        keepGoing = PerformAging();
                    }
                } catch (System.Exception e) {
                    CleanUp();
                    Debug.LogError(e);
                }
            }
        }

        private bool PerformAging() {
            if (albedoMap == null) {
                Debug.LogError("No albedo map supplied.");
                return false;
            }

            if (heightMap == null) {
                Debug.LogError("No height map supplied.");
                return false;
            }

            bool roughnessMapNull = roughnessMap == null;

            if (albedoMap.width != albedoMap.height || albedoMap.width != heightMap.width || albedoMap.height != heightMap.height || (!roughnessMapNull && (roughnessMap.height != roughnessMap.width || roughnessMap.width != albedoMap.width))) {
                Debug.LogError("Maps are not the same size or not square.");
                return false;
            }

            float totalWork = iterations + 3 + (animate ? 0 : 1);
            float completeWork = 0;
            Utility.Logger logger = new Utility.Logger();

            logger.Log("Initializing...");
            if (EditorUtility.DisplayCancelableProgressBar(SelectProgressBarTitle(), "Initializing", completeWork++ / totalWork)) {
                CleanUp();
                return false;
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
            float[,] roughnessBufferDead = Conversion.CopyBuffer(roughnessBuffer);

            List<LichenGrowth.Cluster> lichenClusters = new List<LichenGrowth.Cluster>();
            for (int i = 0; i < lichenParameters.initialSeeds; ++i) {
                LichenGrowth.SpawnCluster(ref lichenClusters, size, lichenParameters);
            }

            Shader utilityShader = Shader.Find("Hidden/Utility");

            System.IO.Directory.CreateDirectory($"{savePath}/{iterations}");
            string alivePath = $"{savePath}/{iterations}/Alive/";
            string deadPath = $"{savePath}/{iterations}/Dead/";
            if (animate) {
                System.IO.Directory.CreateDirectory(alivePath);
                System.IO.Directory.CreateDirectory($"{alivePath}Albedo/");
                System.IO.Directory.CreateDirectory($"{alivePath}Height/");
                System.IO.Directory.CreateDirectory($"{alivePath}Roughness/");
                System.IO.Directory.CreateDirectory(deadPath);
                System.IO.Directory.CreateDirectory($"{deadPath}Albedo/");
                System.IO.Directory.CreateDirectory($"{deadPath}Height/");
                System.IO.Directory.CreateDirectory($"{deadPath}Roughness/");
            }

            float[,] ironGrains = Textures.PerlinNoise(size, colorationParameters.ironGranularity);
            float[,] efflorescenceGrains = Textures.PerlinNoise(size, colorationParameters.efflorescenceGranularity);

            logger.LogTime("Initialization done", initializationStart);

            // Perform the aging.
            logger.Log("Aging...");
            if (EditorUtility.DisplayCancelableProgressBar(SelectProgressBarTitle(), "Aging", completeWork++ / totalWork)) {
                CleanUp();
                return false;
            }
            
            System.DateTime simulationStart = System.DateTime.Now;
            System.TimeSpan totalErosionTime = System.TimeSpan.Zero;
            System.TimeSpan totalLichenTime = System.TimeSpan.Zero;

            float[,] visits = new float[size, size];

            for (int iteration = 0, frame = 0; iteration < iterations; ++iteration) {
                // Perform hydraulic (rain) erosion.
                System.DateTime erosionStart = System.DateTime.Now;
                if (Random.value < rainRate) {
                    Erosion.ErosionEvent(ref layers, erosionParameters, ref visits);
                }
                totalErosionTime += System.DateTime.Now - erosionStart;

                // Perform lichen growth.
                System.DateTime lichenStart = System.DateTime.Now;
                List<LichenGrowth.Cluster> dailyClusters = lichenClusters.OrderBy(x => Random.value).Take(lichenParameters.maxClustersPerDay).ToList();
                for (int i = 0; i < dailyClusters.Count; ++i) {
                    LichenGrowth.LichenGrowthEvent(dailyClusters[i], size, lichenParameters, layers);
                }

                // Spawn new lichen seeds.
                if (lichenClusters.Count < lichenParameters.maxTotalClusters && Random.value < lichenParameters.newSeedProbability) {
                    LichenGrowth.SpawnCluster(ref lichenClusters, size, lichenParameters);
                }
                totalLichenTime += System.DateTime.Now - lichenStart;

                if (EditorUtility.DisplayCancelableProgressBar(SelectProgressBarTitle(), $"Aged iteration {iteration + 1} / {iterations}", completeWork++ / totalWork)) {
                    CleanUp();
                    return false;
                }

                if (animate && iteration % iterationsPerFrame == 0) {
                    float[,] erosionBuffer = Conversion.DifferenceMap(originalRockHeight, Conversion.ExtractBufferLayer(layers, (int) Erosion.LayerName.Rock));
                    erosionBuffer = Height.Normalize(erosionBuffer);

                    float[,] heightBufferDead = Height.FinalizeHeight(layers);

                    Color[,] tempAlbedo = Coloration.SurfaceSolutionDiscoloration(albedoBuffer, ironNoise, ironGrains, colorationParameters.ironColor, colorationParameters.ironOpacityModifier, iteration, effectiveMaxIterations, true);

                    tempAlbedo = Coloration.SurfaceSolutionDiscoloration(tempAlbedo, efflorescenceNoise, efflorescenceGrains, colorationParameters.efflorescenceColor, colorationParameters.efflorescenceOpacityModifier, iteration, effectiveMaxIterations);

                    tempAlbedo = Coloration.ColorErodedAreas(tempAlbedo, Textures.GaussianBlur(erosionBuffer, colorationParameters.blurRadius), colorationParameters.erosionDarkening, iteration, effectiveMaxIterations);

                    float[,] sedimentBuffer = Conversion.ExtractBufferLayer(layers, (int) Erosion.LayerName.Sediment);
                    sedimentBuffer = Height.Normalize(sedimentBuffer);
                    tempAlbedo = Coloration.OverlaySediment(tempAlbedo, sedimentBuffer, colorationParameters.sedimentColor, colorationParameters.sedimentOpacityModifier, sedimentNoise);

                    Texture2D albedoResult = Conversion.CreateTexture(size, tempAlbedo);
                    Texture2D[] lichenResults = LichenGrowth.CreateLichenTexture(lichenClusters, size, albedoResult, utilityShader, lichenParameters);
                    float[,] lichenHeight = Conversion.CreateFloatBuffer(lichenResults[1]);
                    lichenHeight = Conversion.ScalarMultMap(lichenHeight, lichenParameters.lichenHeightScale);
                    float[,] heightBuffer = Conversion.SumMap(heightBufferDead, lichenHeight);

                    (float[,] newRoughness, float[,] newRoughnessDead) = Height.GenerateRoughness(roughnessBuffer, roughnessBufferDead, erosionBuffer, sedimentBuffer, lichenHeight, roughnessParameters);
                    Textures.SaveTextureAsPNG(lichenResults[0], $"{alivePath}Albedo/{frame}.png");
                    Textures.SaveTextureAsPNG(Conversion.CreateTexture(size, heightBuffer), $"{alivePath}Height/{frame}.png");
                    Textures.SaveTextureAsPNG(Conversion.CreateTexture(size, newRoughness), $"{alivePath}Roughness/{frame}.png");

                    Textures.SaveTextureAsPNG(albedoResult, $"{deadPath}Albedo/{frame}.png");
                    Textures.SaveTextureAsPNG(Conversion.CreateTexture(size, heightBufferDead), $"{deadPath}Height/{frame}.png");
                    Textures.SaveTextureAsPNG(Conversion.CreateTexture(size, newRoughnessDead), $"{deadPath}Roughness/{frame}.png");
                    frame++;
                    logger.Log($"Saved frame {frame}.");
                }
            }

            logger.LogTime("Aging done", simulationStart);
            logger.LogTime("Total erosion time", totalErosionTime);
            logger.LogTime("Total lichen time", totalLichenTime);

            int numLichenParticles = 0;
            for (int i = 0; i < lichenClusters.Count; ++i) {
                numLichenParticles += lichenClusters[i].particles.Count;
            }

            logger.Log($"Number of lichen clusters: {lichenClusters.Count}");
            logger.Log($"Number of lichen particles: {numLichenParticles}");

            if (!animate) {
                logger.Log("Finalizing...");
                if (EditorUtility.DisplayCancelableProgressBar(SelectProgressBarTitle(), "Finalizing", completeWork++ / totalWork)) {
                    CleanUp();
                    return false;
                }
                System.DateTime finalizationStart = System.DateTime.Now;

                float[,] erosionBuffer = Conversion.DifferenceMap(originalRockHeight, Conversion.ExtractBufferLayer(layers, (int) Erosion.LayerName.Rock));
                erosionBuffer = Height.Normalize(erosionBuffer);

                float[,] heightBufferDead = Height.FinalizeHeight(layers);

                albedoBuffer = Coloration.SurfaceSolutionDiscoloration(albedoBuffer, ironNoise, ironGrains, colorationParameters.ironColor, colorationParameters.ironOpacityModifier, iterations, effectiveMaxIterations, true);

                albedoBuffer = Coloration.SurfaceSolutionDiscoloration(albedoBuffer, efflorescenceNoise, efflorescenceGrains, colorationParameters.efflorescenceColor, colorationParameters.efflorescenceOpacityModifier, iterations, effectiveMaxIterations);

                albedoBuffer = Coloration.ColorErodedAreas(albedoBuffer, Textures.GaussianBlur(erosionBuffer, colorationParameters.blurRadius), colorationParameters.erosionDarkening, iterations, effectiveMaxIterations);

                float[,] sedimentBuffer = Conversion.ExtractBufferLayer(layers, (int) Erosion.LayerName.Sediment);
                sedimentBuffer = Height.Normalize(sedimentBuffer);
                albedoBuffer = Coloration.OverlaySediment(albedoBuffer, sedimentBuffer, colorationParameters.sedimentColor, colorationParameters.sedimentOpacityModifier, sedimentNoise);

                visits = Height.Normalize(visits);

                Texture2D albedoResult = Conversion.CreateTexture(size, albedoBuffer);
                Texture2D[] lichenResults = LichenGrowth.CreateLichenTexture(lichenClusters, size, albedoResult, utilityShader, lichenParameters);
                float[,] lichenHeight = Conversion.CreateFloatBuffer(lichenResults[1]);
                lichenHeight = Conversion.ScalarMultMap(lichenHeight, lichenParameters.lichenHeightScale);
                float[,] heightBuffer = Conversion.SumMap(heightBufferDead, lichenHeight);

                (float[,] newRoughness, float[,] newRoughnessDead) = Height.GenerateRoughness(roughnessBuffer, roughnessBufferDead, erosionBuffer, sedimentBuffer, lichenHeight, roughnessParameters);

                logger.LogTime("Finalization done", finalizationStart);

                logger.Log("Saving...");
                if (EditorUtility.DisplayCancelableProgressBar(SelectProgressBarTitle(), "Saving", completeWork++ / totalWork)) {
                    CleanUp();
                    return false;
                }
                System.DateTime savingStart = System.DateTime.Now;

                Textures.SaveTextureAsPNG(lichenResults[0], $"{savePath}/{iterations}/Albedo.png");
                Textures.SaveTextureAsPNG(Conversion.CreateTexture(size, heightBuffer), $"{savePath}/{iterations}/Height.png");
                Textures.SaveTextureAsPNG(Conversion.CreateTexture(size, newRoughness), $"{savePath}/{iterations}/Roughness.png");

                Textures.SaveTextureAsPNG(albedoResult, $"{savePath}/{iterations}/Dead_Albedo.png");
                Textures.SaveTextureAsPNG(Conversion.CreateTexture(size, heightBufferDead), $"{savePath}/{iterations}/Dead_Height.png");
                Textures.SaveTextureAsPNG(Conversion.CreateTexture(size, newRoughnessDead), $"{savePath}/{iterations}/Dead_Roughness.png");

                if (additionalTextureFlags != 0) {
                    string debugPath = $"{savePath}/{iterations}/Debug/";
                    System.IO.Directory.CreateDirectory(debugPath);

                    if ((additionalTextureFlags & (int) DebugTextures.ErosionBuffer) > 0) {
                        Textures.SaveTextureAsPNG(Conversion.CreateTexture(size, erosionBuffer), $"{debugPath}Erosion_Buffer.png");
                    }

                    if ((additionalTextureFlags & (int) DebugTextures.SedimentBuffer) > 0) {
                        Textures.SaveTextureAsPNG(Conversion.CreateTexture(size, sedimentBuffer), $"{debugPath}Sediment_Buffer.png");
                    }

                    if ((additionalTextureFlags & (int) DebugTextures.VisitBuffer) > 0) {
                        Textures.SaveTextureAsPNG(Conversion.CreateTexture(size, visits), $"{debugPath}Visit_Buffer.png");
                    }

                    if ((additionalTextureFlags & (int) DebugTextures.LichenBuffer) > 0) {
                        Textures.SaveTextureAsPNG(lichenResults[2], $"{debugPath}Lichen_Buffer.png");
                    }

                    if ((additionalTextureFlags & (int) DebugTextures.LichenHeightBuffer) > 0) {
                        Textures.SaveTextureAsPNG(lichenResults[1], $"{debugPath}Lichen_Height.png");
                    }
                }

                logger.LogTime("Saving done", savingStart);
            }
            
            CleanUp();
            logger.LogTime("All done", initializationStart);
            logger.WriteToFile($"{savePath}/{iterations}/Log.txt");
            return true;
        }

        private void CleanUp() {
            EditorUtility.ClearProgressBar();
        }

        private void SerializeAndExport() {
            string savePath = EditorUtility.SaveFilePanel("Export stone aging parameters", Application.dataPath, "StoneAging.json", "json");
            if (savePath.Length > 0) {
                string json = EditorJsonUtility.ToJson(this);
                System.IO.File.WriteAllText(savePath, json);
            }
        }

        private void SerializeAndStoreTemp() {
            EditorPrefs.SetString("tempParams", EditorJsonUtility.ToJson(this));
        }

        private void DeserializeAndImport() {
            string loadPath = EditorUtility.OpenFilePanel("Import stone aging parameters", Application.dataPath, "json");
            if (loadPath.Length > 0) {
                string json = System.IO.File.ReadAllText(loadPath);
                EditorJsonUtility.FromJsonOverwrite(json, this);
                EditorUtility.RequestScriptReload();
            }
        }

        private void DeserializeAndImport(string loadPath) {
            string json = System.IO.File.ReadAllText(loadPath);
            EditorJsonUtility.FromJsonOverwrite(json, this);
        }

        private void DeserializeAndLoadTemp() {
            string json = EditorPrefs.GetString("tempParams");
            EditorJsonUtility.FromJsonOverwrite(json, this);
            EditorUtility.RequestScriptReload();
        }

        private void SelectSavePath() {
            string savePath = EditorUtility.OpenFolderPanel("Select save location path", "", "");
            if (savePath.Length > 0) {
                this.savePath = savePath;
            }
        }

        private string SelectProgressBarTitle() {
            return "Aging" + (simulateSequence ? $" using {currentSettingName}" : "");
        }
    }
}
