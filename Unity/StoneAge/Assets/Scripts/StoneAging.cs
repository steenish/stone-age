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
        private Shader setupShader;
        [SerializeField]
        private Shader erosionShader;
        [SerializeField]
        Texture2D albedoMap = null;
        [SerializeField]
        Texture2D heightMap = null;
        [SerializeField]
        private int numSteps;
        [SerializeField]
        private int seed;
        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float timeStep = 0.1f;
        [SerializeField]
        [Range(0.0f, 2.0f)]
        private float rainScale = 0.5f;
        [SerializeField]
        private float pipeRadius = 0.5f;
        [SerializeField]
        private float realWorldSize = 5.0f;
        [SerializeField]
        private float gravity = 9.82f;
        [SerializeField]
        [Range(0.0f, 90.0f)]
        private float minTiltAngle = 1.0f;
        [SerializeField]
        private float capacityModifier = 1.0f;
        [SerializeField]
        private float dissolvingModifier = 1.0f;
        [SerializeField]
        private float depositionModifier = 1.0f;
        [SerializeField]
        private float advectionModifier = 1.0f;
        [SerializeField]
        private float evaporationModifier = 1.0f;

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

            Debug.Log("Initializing...");
            System.DateTime initializationStart = System.DateTime.Now;

			Random.InitState(seed);

            int size = heightMap.width;

            Material setupMaterial = new Material(setupShader);
            Material erosionMaterial = new Material(erosionShader);

            System.DateTime simulationStart = System.DateTime.Now;

            RenderTexture previousRT = RenderTexture.active;

            RenderTexture tempOutput = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

            RenderTexture terrainTexture = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            RenderTexture fluxTexture = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            RenderTexture velocityTexture = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Graphics.Blit(heightMap, terrainTexture, setupMaterial, 0);
            Graphics.Blit(velocityTexture, tempOutput, setupMaterial, 1);
            Graphics.Blit(tempOutput, velocityTexture);

            RenderTexture tempTerrainTexture1 = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            RenderTexture tempTerrainTexture2 = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            RenderTexture tempTerrainTexture3 = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

            Texture2D noiseTexture = Textures.PerlinNoiseTexture(size, size);

            float pipeArea = Mathf.PI * pipeRadius * pipeRadius;
            float scaledWorldSize = realWorldSize * 1000; // Real world size must be scaled up to achieve simulation stability.
            float gridPointDistance = scaledWorldSize / size;
            erosionMaterial.SetFloat("_PipeArea", pipeArea);
            erosionMaterial.SetFloat("_GridPointDistance", gridPointDistance);
            erosionMaterial.SetFloat("_TimeStep", timeStep);
            erosionMaterial.SetFloat("_RainScale", rainScale);
            erosionMaterial.SetFloat("_Gravity", gravity);
            erosionMaterial.SetFloat("_MinTilt", minTiltAngle);
            erosionMaterial.SetFloat("_CapacityConst", capacityModifier);
            erosionMaterial.SetFloat("_DissolvingConst", dissolvingModifier);
            erosionMaterial.SetFloat("_DepositionConst", depositionModifier);
            erosionMaterial.SetFloat("_AdvectionConst", advectionModifier);
            erosionMaterial.SetFloat("_EvaporationConst", evaporationModifier);

            LogTime("Initialization done", initializationStart);

            string savePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures) + "/StoneAgeGPU/";
            System.IO.Directory.CreateDirectory(savePath);

            // Perform the aging.
            Debug.Log("Aging...");

            for (int step = 0; step < numSteps; ++step) {
				System.DateTime stepStart = System.DateTime.Now;

                if (step % 10 == 0) {
                    noiseTexture = Textures.PerlinNoiseTexture(size, size);
                }

                // Perform hydraulic erosion. TODO CHECK BOUNDARY CONDITIONS (NO SLIP/TILING?)

                // Step 1
                // Pass 0
                erosionMaterial.SetTexture("_NoiseTex", noiseTexture);
                Graphics.Blit(terrainTexture, tempTerrainTexture1, erosionMaterial, 0);

                // Step 2
				// Pass 1
				erosionMaterial.SetTexture("_TerrainTex", tempTerrainTexture1);
				Graphics.Blit(fluxTexture, tempOutput, erosionMaterial, 1);
                Graphics.Blit(tempOutput, fluxTexture);

                // Pass 2
                erosionMaterial.SetTexture("_FluxTex", fluxTexture);
                Graphics.Blit(tempTerrainTexture1, tempTerrainTexture2, erosionMaterial, 2);

                // Pass 3
                erosionMaterial.SetTexture("_TerrainTex", tempTerrainTexture1);
                erosionMaterial.SetTexture("_TempTerrainTex", tempTerrainTexture2);
                erosionMaterial.SetTexture("_FluxTex", fluxTexture);
                Graphics.Blit(velocityTexture, tempOutput, erosionMaterial, 3);
                Graphics.Blit(tempOutput, velocityTexture);

                // Step 3
                // Pass 4
                erosionMaterial.SetTexture("_VelocityTex", velocityTexture);
                Graphics.Blit(tempTerrainTexture2, tempTerrainTexture3, erosionMaterial, 4);

                // Step 4
                // Pass 5
                erosionMaterial.SetTexture("_VelocityTex", velocityTexture);
                Graphics.Blit(tempTerrainTexture3, tempOutput, erosionMaterial, 5);
                Graphics.Blit(tempOutput, tempTerrainTexture3);

                // Step 5
                // Pass 6
                Graphics.Blit(tempTerrainTexture3, terrainTexture, erosionMaterial, 6);

                LogTime("Aged " + (step + 1) + " step" + ((step + 1 == 1) ? "" : "s"), stepStart);
			}

			// Save texture (debug).
			Textures.SaveTextureAsPNG(Textures.GetRTPixels(terrainTexture), savePath + "Test1.png");
			Textures.SaveTextureAsPNG(Textures.GetRTPixels(fluxTexture), savePath + "Test2.png");
			Textures.SaveTextureAsPNG(Textures.GetRTPixels(velocityTexture), savePath + "Test3.png");

            // TODO FINALIZE HEIGHT MAP
            // TODO CREATE COLOR MAP

            // Clean up. TODO RELEASE TEMPORARY RTs
            RenderTexture.active = previousRT;

            LogTime("Aging done", simulationStart);

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
