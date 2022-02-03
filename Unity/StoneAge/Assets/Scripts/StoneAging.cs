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
        private int agingYears;
        [SerializeField]
        private int seed;
        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float timeStep = 0.1f;
        [SerializeField]
        [Range(0.0f, 2.0f)]
        private float rainScale = 0.5f;

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

            int width = heightMap.width;
            int height = heightMap.height;

            Material setupMaterial = new Material(setupShader);
            Material erosionMaterial = new Material(erosionShader);

            System.DateTime simulationStart = System.DateTime.Now;

            RenderTexture previousRT = RenderTexture.active;

            RenderTexture terrainTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            RenderTexture fluxTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            RenderTexture velocityTexture = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Graphics.Blit(heightMap, terrainTexture, setupMaterial);

            Texture2D noiseTexture = Textures.PerlinNoiseTexture((int) (width * 0.25f), (int) (height * 0.25f));

            RenderTexture tempOutput = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

            erosionMaterial.SetFloat("_TimeStep", timeStep);
            erosionMaterial.SetFloat("_RainScale", rainScale);

            LogTime("Initialization done", initializationStart);

            // Perform the aging.
            Debug.Log("Aging...");

            for (int year = 0; year < agingYears; ++year) {
				System.DateTime yearStart = System.DateTime.Now;

                if (year % 10 == 0) {
                    noiseTexture = Textures.PerlinNoiseTexture((int) (width * 0.25f), (int) (height * 0.25f));
                }

                // Perform hydraulic erosion.

                // Step 1
                // Pass 0
                erosionMaterial.SetTexture("_NoiseTex", noiseTexture);
                Graphics.Blit(terrainTexture, tempOutput, erosionMaterial, 0); // TODO MIGHT NEED A TEMPWATER TEXTURE FOR d_1 IN PAPER
                Graphics.Blit(tempOutput, terrainTexture);

                // Step 2
				// Pass 1
				erosionMaterial.SetTexture("_TerrainTex", terrainTexture);
				Graphics.Blit(fluxTexture, tempOutput, erosionMaterial, 1);
                Graphics.Blit(tempOutput, fluxTexture);

                // Pass 2
                erosionMaterial.SetTexture("_FluxTex", fluxTexture);
                Graphics.Blit(terrainTexture, tempOutput, erosionMaterial, 2);
                Graphics.Blit(tempOutput, terrainTexture);

                // Pass 3
                erosionMaterial.SetTexture("_TerrainTex", terrainTexture);
                erosionMaterial.SetTexture("_FluxTex", fluxTexture);
                Graphics.Blit(velocityTexture, tempOutput, erosionMaterial, 3);
                Graphics.Blit(tempOutput, velocityTexture);

                // Step 3
                // Pass 4
                erosionMaterial.SetTexture("_VelocityTex", velocityTexture);
                Graphics.Blit(terrainTexture, tempOutput, erosionMaterial, 4);
                Graphics.Blit(tempOutput, terrainTexture);

                // Step 4
                // Pass 5
                erosionMaterial.SetTexture("_VelocityTex", velocityTexture);
                Graphics.Blit(terrainTexture, tempOutput, erosionMaterial, 5);
                Graphics.Blit(tempOutput, terrainTexture);

                // Step 5
                // Pass 6
                Graphics.Blit(terrainTexture, tempOutput, erosionMaterial, 6);
                Graphics.Blit(tempOutput, terrainTexture);

                LogTime("Aged " + (year + 1) + " year" + ((year + 1 == 1) ? "" : "s"), yearStart);
			}

            // Save texture (debug).
            string savePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures) + "/StoneAgeGPU/";
            System.IO.Directory.CreateDirectory(savePath);
			Textures.SaveTextureAsPNG(Textures.GetRTPixels(terrainTexture), savePath + "Test.png");

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
