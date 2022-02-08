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
        private Shader finalizationShader;
        [SerializeField]
        Texture2D albedoMap = null;
        [SerializeField]
        Texture2D heightMap = null;
        [SerializeField]
        [Range(0.0f, 1.0f)]
        private float timeScale = 0.1f;
        [SerializeField]
        [Range(0.0f, 2.0f)]
        private float rainScale = 0.5f;
        [SerializeField]
        private float pipeArea = 0.5f;
        [SerializeField]
        private float pipeLength = 0.5f;
        [SerializeField]
        private float cellSize = 5.0f;
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
        private float evaporationModifier = 1.0f;

        private Material setupMaterial;
        private Material erosionMaterial;
        private Material finalizationMaterial;

        private RenderTexture terrainTexture;
        private RenderTexture fluxTexture;
        private RenderTexture velocityTexture;

        private int size;
        private int numSteps = 0;

        private void OnEnable() {
            Initialize();
        }

        private void OnDisable() {
            FinalizeTextures();
        }

		private void OnRenderImage(RenderTexture source, RenderTexture destination) {
            finalizationMaterial.SetTexture("_SecondTex", fluxTexture);
            finalizationMaterial.SetTexture("_ThirdTex", velocityTexture);
            Graphics.Blit(terrainTexture, destination, finalizationMaterial, 2);
		}

		private void FixedUpdate() {
            SimulationStep();
        }

        private void Initialize() {
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

            Debug.Log("Initializing.");
            System.DateTime initializationStart = System.DateTime.Now;

            size = heightMap.width;

            setupMaterial = new Material(setupShader);
            erosionMaterial = new Material(erosionShader);
            finalizationMaterial = new Material(finalizationShader);

            RenderTexture previousRT = RenderTexture.active;
            RenderTexture tempOutput = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            terrainTexture = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            fluxTexture = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            velocityTexture = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Graphics.Blit(heightMap, terrainTexture, setupMaterial, 0);
            Graphics.Blit(velocityTexture, tempOutput, setupMaterial, 1);
            Graphics.Blit(tempOutput, velocityTexture);
            RenderTexture.active = previousRT;

            erosionMaterial.SetFloat("_PipeArea", pipeArea);
            erosionMaterial.SetFloat("_CellSize", cellSize);
            erosionMaterial.SetFloat("_PipeLength", pipeLength);
            erosionMaterial.SetFloat("_TimeStep", timeScale * Time.fixedDeltaTime);
            erosionMaterial.SetFloat("_RainScale", rainScale);
            erosionMaterial.SetFloat("_Gravity", gravity);
            erosionMaterial.SetFloat("_MinTilt", minTiltAngle);
            erosionMaterial.SetFloat("_CapacityConst", capacityModifier);
            erosionMaterial.SetFloat("_DissolvingConst", dissolvingModifier);
            erosionMaterial.SetFloat("_DepositionConst", depositionModifier);
            erosionMaterial.SetFloat("_EvaporationConst", evaporationModifier);

            LogTime("Initialization done", initializationStart);
        }

        private void SimulationStep() {
            RenderTexture previousRT = RenderTexture.active;
            RenderTexture tempOutput = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            RenderTexture tempTerrainTexture1 = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            RenderTexture tempTerrainTexture2 = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            RenderTexture tempTerrainTexture3 = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

            // Perform hydraulic erosion.

            // Step 1
            // Pass 0
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

            // Clean up.
            RenderTexture.active = previousRT;
            RenderTexture.ReleaseTemporary(tempOutput);
            RenderTexture.ReleaseTemporary(tempTerrainTexture1);
            RenderTexture.ReleaseTemporary(tempTerrainTexture2);
            RenderTexture.ReleaseTemporary(tempTerrainTexture3);

            numSteps++;
        }

        private void FinalizeTextures() {
            Debug.Log("Finalizing.");
            System.DateTime finalizationStart = System.DateTime.Now;
            RenderTexture previousRT = RenderTexture.active;

            string savePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures) + "/StoneAgeGPU/";
            System.IO.Directory.CreateDirectory(savePath);

            RenderTexture tempOutput = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

            // Save texture (debug).
            Textures.SaveTextureAsPNG(Textures.GetRTPixels(terrainTexture), savePath + "TestTerrain.png");
            Textures.SaveTextureAsPNG(Textures.GetRTPixels(fluxTexture), savePath + "TestFlux.png");
            Textures.SaveTextureAsPNG(Textures.GetRTPixels(velocityTexture), savePath + "TestVelocity.png");

            // Save heightmap.
            Graphics.Blit(terrainTexture, tempOutput, finalizationMaterial, 0);
            Texture2D newHeightMap = Textures.GetRTPixels(tempOutput);
            Textures.SaveTextureAsPNG(newHeightMap, savePath + "Height.png");

            // Save sediment map.
            finalizationMaterial.SetTexture("_SecondTex", heightMap);
            Graphics.Blit(newHeightMap, tempOutput, finalizationMaterial, 1);
            Textures.SaveTextureAsPNG(Textures.GetRTPixels(tempOutput), savePath + "Sediment.png");

            // Save erosion map.
            finalizationMaterial.SetTexture("_SecondTex", newHeightMap);
            Graphics.Blit(heightMap, tempOutput, finalizationMaterial, 1);
            Textures.SaveTextureAsPNG(Textures.GetRTPixels(tempOutput), savePath + "Erosion.png");

            // TODO CREATE COLOR MAP

            RenderTexture.ReleaseTemporary(terrainTexture);
            RenderTexture.ReleaseTemporary(fluxTexture);
            RenderTexture.ReleaseTemporary(velocityTexture);
            RenderTexture.ReleaseTemporary(tempOutput);
            RenderTexture.active = previousRT;

            LogTime("Finalization done", finalizationStart);
            Debug.Log("Simulation steps: " + numSteps);
        }

        private void LogTime(string text, System.DateTime startTime) {
            if (loggingLevel >= LoggingLevel.Timing) {
                System.TimeSpan timeDifference = System.DateTime.Now - startTime;
                Debug.Log(text + " (" + (timeDifference.Hours * 3600 + timeDifference.Minutes * 60 + timeDifference.Seconds + timeDifference.Milliseconds * 0.001) + " s).");
            }
        }
    }
}
