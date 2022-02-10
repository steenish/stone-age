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

        [Header("Shaders")]
        [SerializeField]
        private ComputeShader erosion;

        [Header("General parameters")]
        [SerializeField]
        private int seed;

        [Header("Erosion parameters")]
        [SerializeField]
        private int erosionSteps = 100;
        [SerializeField]
        private int effectiveMaxSteps = 1000;
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

        struct Int2 {
            readonly int x;
            readonly int y;

            public Int2(int x, int y) {
                this.x = x;
                this.y = y;
            }
        }

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

            float totalWork = 5 + (saveToDisk ? 1 : 0);
            float completeWork = 0;

            Debug.Log("Initializing.");
            if (EditorUtility.DisplayCancelableProgressBar("Aging", "Initializing", completeWork++ / totalWork)) {
                EditorUtility.ClearProgressBar();
                return;
			}
            System.DateTime initializationStart = System.DateTime.Now;

            int size = heightMap.width;
            Random.InitState(seed);

            if (!customErosionParameters) {
                erosionParameters = new Erosion.ErosionParameters();
            }

            // Create buffers from the input textures.
            Color[,] albedoBuffer = Conversion.Create2DColorBuffer(albedoMap);
            float[] heightBuffer = Conversion.CreateFloatBuffer(heightMap);

			// Set up erosion compute shader parameters.
			ComputeBuffer heightComputeBuffer = new ComputeBuffer(size * size, sizeof(float));
			heightComputeBuffer.SetData(heightBuffer);
			erosion.SetBuffer(0, "heightBuffer", heightComputeBuffer);

			// Calculate and set weights and offsets for erosion.
			int radius = erosionParameters.radius;
			List<Int2> erosionIndexOffsets = new List<Int2>();
			List<float> weights = new List<float>();
			float weightSum = 0;
			for (int y = -radius; y <= radius; y++) {
				for (int x = -radius; x <= radius; x++) {
					float sqrDst = x * x + y * y;
					if (sqrDst < radius * radius) {
						erosionIndexOffsets.Add(new Int2(x, y));
						float weight = 1 - Mathf.Sqrt(sqrDst) / radius;
						weightSum += weight;
						weights.Add(weight);
					}
				}
			}
			for (int i = 0; i < weights.Count; i++) {
				weights[i] /= weightSum;
			}

			ComputeBuffer indexOffsetBuffer = new ComputeBuffer(erosionIndexOffsets.Count, sizeof(int) * 2);
			ComputeBuffer weightBuffer = new ComputeBuffer(weights.Count, sizeof(float));
			indexOffsetBuffer.SetData(erosionIndexOffsets);
			weightBuffer.SetData(weights);
			erosion.SetBuffer(0, "erosionOffsets", indexOffsetBuffer);
			erosion.SetBuffer(0, "erosionWeights", weightBuffer);

			// Generate random positions (indices) for raindrop placement.
			int[] randomIndices = new int[erosionSteps];
			for (int i = 0; i < erosionSteps; i++) {
				int randomX = Random.Range(0, size);
				int randomY = Random.Range(0, size);
				randomIndices[i] = randomX + randomY * size;
			}

			ComputeBuffer rainPositionIndexBuffer = new ComputeBuffer(randomIndices.Length, sizeof(int));
			rainPositionIndexBuffer.SetData(randomIndices);
			erosion.SetBuffer(0, "rainPositionIndices", rainPositionIndexBuffer);

			erosion.SetFloat("inertia", erosionParameters.inertia);
			erosion.SetFloat("capacity", erosionParameters.capacity);
			erosion.SetFloat("deposition", erosionParameters.deposition);
			erosion.SetFloat("erosion", erosionParameters.erosion);
			erosion.SetFloat("evaporation", erosionParameters.evaporation);
			erosion.SetFloat("radius", erosionParameters.radius);
			erosion.SetFloat("minSlope", erosionParameters.minSlope);
			erosion.SetInt("maxPath", erosionParameters.maxPath);
			erosion.SetFloat("gravity", erosionParameters.gravity);
			erosion.SetFloat("startSpeed", erosionParameters.startSpeed);
			erosion.SetFloat("startWater", erosionParameters.startWater);

			erosion.SetInt("size", size);
			erosion.SetInt("numWeights", weights.Count);

			LogTime("Initialization done", initializationStart);

			// Perform the aging.
			Debug.Log("Aging.");
			if (EditorUtility.DisplayCancelableProgressBar("Aging", "Aging", completeWork++ / totalWork)) {
				EditorUtility.ClearProgressBar();
				return;
			}
			System.DateTime simulationStart = System.DateTime.Now;

			Debug.Log("Eroding.");
			if (EditorUtility.DisplayCancelableProgressBar("Aging", "Eroding", completeWork++ / totalWork)) {
				EditorUtility.ClearProgressBar();
				return;
			}
			System.DateTime erosionStart = System.DateTime.Now;

			// Perform hydraulic erosion.
			erosion.Dispatch(0, erosionSteps / 1024, 1, 1);
			heightComputeBuffer.GetData(heightBuffer);

			LogTime("Erosion done", erosionStart);

			//for (int step = 0; step < this.erosionSteps; ++step) {
			//    System.DateTime stepStart = System.DateTime.Now;

			//    EditorUtility.DisplayProgressBar("Aging", "Aged step " + (step + 1) + " / " + this.erosionSteps, completeWork++ / totalWork);

			//    LogTime("Aged " + (step + 1) + " step" + ((step + 1 == 1) ? "" : "s"), stepStart);
			//}

			LogTime("Aging done", simulationStart);

			Debug.Log("Finalizing.");
			if (EditorUtility.DisplayCancelableProgressBar("Aging", "Finalizing", completeWork++ / totalWork)) {
				EditorUtility.ClearProgressBar();
				return;
			}
			System.DateTime finalizationStart = System.DateTime.Now;

			heightComputeBuffer.Release();
			indexOffsetBuffer.Release();
			weightBuffer.Release();
			rainPositionIndexBuffer.Release();

			// TODO do this with shaders instead probably
			//float[,] rockErosion = Conversion.DifferenceMap(originalRockHeight, Conversion.ExtractBufferLayer(heightBuffer, (int) Erosion.LayerName.Rock));
			Height.NormalizeHeight(ref heightBuffer); // TODO this could be done with a shader once the max and min heights have been found

			// TODO do this with shaders instead probably
			//Textures.ColorErodedAreas(ref albedoBuffer, Textures.GaussianBlur(rockErosion, blurRadius), this.numSteps, effectiveMaxSteps);

			// TODO do this with shaders instead probably
			//float[,] sedimentBuffer = Conversion.ExtractBufferLayer(heightBuffer, (int) Erosion.LayerName.Sediment);
			//Height.NormalizeHeight(ref sedimentBuffer);
			//albedoBuffer = Textures.OverlaySediment(albedoBuffer, sedimentBuffer, sedimentColor, sedimentOpacityModifier);

			//LogTime("Finalization done", finalizationStart);

            if (saveToDisk) {
                Debug.Log("Saving.");
                if (EditorUtility.DisplayCancelableProgressBar("Aging", "Saving", completeWork++ / totalWork)) {
                    EditorUtility.ClearProgressBar();
                    return;
                }
                System.DateTime savingStart = System.DateTime.Now;

                string savePath = System.Environment.GetFolderPath(saveLocation) + "/" + folderName + "/";
                System.IO.Directory.CreateDirectory(savePath);
                Textures.SaveTextureAsPNG(Conversion.CreateTexture(albedoMap.width, albedoBuffer), savePath + "Albedo_Aged_" + this.erosionSteps + ".png");
                Textures.SaveTextureAsPNG(Conversion.CreateTexture(heightMap.width, heightBuffer), savePath + "Height_Aged_" + this.erosionSteps + ".png");

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
