using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using System.Linq;

namespace StoneAge {
    public class LichenGrowth {

        public class Cluster {
            public Vector2 source { get; private set; }
            public List<Vector2> particles { get; private set; }
            public Bounds bounds { get; private set; }
            public Species species { get; private set; }

            public Cluster(Vector2 sourcePosition, LichenParameters parameters) {
                source = sourcePosition;
                particles = new List<Vector2> {
                    source
                };
                bounds = new Bounds(source, Vector3.one * parameters.particleRadius);
                species = FindSpecies(parameters.species);
            }
        }

        [System.Serializable]
        public class LichenParameters {
            [Range(0.1f, 10.0f)]
            public float scale = 1.0f;
            [Range(0, 20)]
            public int initialSeeds = 5;
            [Range(0.00001f, 0.1f)]
            public float alpha = 1e-4f;
            [Range(0.1f, 10.0f)]
            public float sigma = 1.0f;
            [Range(1.0f, 10.0f)]
            public float tau = 3.0f;
            [Range(1.0f, 5.0f)]
            public float rho = 2.5f;
            [Range(10, 200)]
            public int maxPath = 100;
            [Range(0.1f, 5.0f)]
            public float particleRadius = 1.0f;
            [Range(0.1f, 5.0f)]
            public float spawnEpsilon = 1.0f;
            [Range(1.0f, 10.0f)]
            public float stepDistance = 5.0f;
            [Range(1.0f, 30.0f)]
            public float deathRadius = 10.0f;
            [Range(0.0f, 0.1f)]
            public float newSeedProbability = 0.001f;
            [Range(50.0f, 300.0f)]
            public float noiseScale = 100.0f;
            [Range(0.01f, 2.0f)]
            public float lichenHeightScale = 0.2f;
            [Range(1, 100)]
            public int maxClustersPerDay = 10;
            public Species[] species;
            public AnimationCurve directLightSensitivity;
            public AnimationCurve indirectLightSensitivity;
            public AnimationCurve moistureSensitivity;
        }

        [System.Serializable]
        public class Species {
            public Color color;
            public int weight;
        }

        private static float CalculateEnvironmentalInfluence(LichenParameters parameters, float height) {
            float direct = parameters.directLightSensitivity.Evaluate(Mathf.Clamp01(2 * height * height));
            float indirect = parameters.indirectLightSensitivity.Evaluate(Mathf.Clamp01(height));
            float moisture = parameters.moistureSensitivity.Evaluate(Mathf.Clamp01(1 - height));
            return Mathf.Min(direct, Mathf.Min(indirect, moisture));
        }

        private static bool CheckCollision(Vector2 position1, Vector2 position2, float radius) {
            return Vector2.SqrMagnitude(position1 - position2) <= radius * radius;
        }

        public static Texture2D[] CreateLichenTexture(List<Cluster> clusters, int size, Texture2D albedoTexture, Shader utilityShader, LichenParameters parameters) {
            RenderTexture previous = RenderTexture.active;
            Material utilityMaterial = new Material(utilityShader);

            RenderTexture colorResult = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            RenderTexture heightResult = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            RenderTexture dummy = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

            Texture2D noiseTexture = Textures.PerlinNoiseTexture(size, parameters.noiseScale);
            utilityMaterial.SetTexture("_NoiseTex", noiseTexture);

            // Sort to avoid capping position array size.
            clusters.Sort((c1, c2) => c2.particles.Count.CompareTo(c1.particles.Count));

            int arrayCap = 1023;
            for (int i = 0; i < clusters.Count; ++i) {
                List<Vector2> particles = clusters[i].particles;
                int numArrays = 1 + particles.Count / arrayCap;
                Vector4[][] particlePositions = new Vector4[numArrays][];
                for (int j = 0; j < numArrays; ++j) {
                    if (j == numArrays - 1) {
                        particlePositions[j] = new Vector4[particles.Count - (numArrays - 1) * arrayCap];
                    } else {
                        particlePositions[j] = new Vector4[arrayCap];
                    }

                    for (int k = 0; k < particlePositions[j].Length; ++k) {
                        particlePositions[j][k] = Height.TilePosition(particles[j * arrayCap + k] * parameters.scale, size);
                    }
                }
                
                Color clusterColor = clusters[i].species.color;
                utilityMaterial.SetInt("_Size", size);
                utilityMaterial.SetFloat("_MaxDistance", 2.0f * parameters.particleRadius * parameters.scale);

                for (int j = 0; j < numArrays; ++j) {
                    if (particlePositions[j].Length == 0) {
                        continue;
                    }

                    utilityMaterial.SetColor("_ClusterColor", clusterColor);
                    utilityMaterial.SetInt("_ParticleCount", particlePositions[j].Length);
                    utilityMaterial.SetVectorArray("_Particles", particlePositions[j]);
                    Graphics.Blit(colorResult, dummy, utilityMaterial, 0); // Color pass.
                    Graphics.Blit(dummy, colorResult);

                    utilityMaterial.SetColor("_ClusterColor", Color.white);
                    Graphics.Blit(heightResult, dummy, utilityMaterial, 0); // Height pass.
                    Graphics.Blit(dummy, heightResult);
                }
            }

            Texture2D[] finalResults = new Texture2D[3];
            finalResults[2] = Textures.GetRTPixels(colorResult);

            utilityMaterial.SetTexture("_AlbedoTex", albedoTexture);
            Graphics.Blit(colorResult, dummy, utilityMaterial, 1); // Blend with albedo.
            finalResults[0] = Textures.GetRTPixels(dummy);

            finalResults[1] = Textures.GetRTPixels(heightResult);

            RenderTexture.active = previous;
            return finalResults;
        }

        private static Species FindSpecies(Species[] speciesList) {
            int numWeights = speciesList.Length;
            int[] cumulativeWeightSum = new int[numWeights + 1];
            cumulativeWeightSum[0] = 0;

            int currentSum = 0;
            for (int i = 1; i < numWeights + 1; ++i) {
                currentSum += speciesList[i - 1].weight;
                cumulativeWeightSum[i] = currentSum;
            }

            Species result = null;
            int target = Random.Range(0, cumulativeWeightSum[numWeights]);
            for (int i = 0; i < numWeights; ++i) {
                if (cumulativeWeightSum[i] <= target && target < cumulativeWeightSum[i + 1]) {
                    result = speciesList[i];
                    break;
                }
            }            
            return result;
        }

        private static int FindNeighbors(List<Vector2> particles, Vector2 particle, float radius) {
            int numNeighbors = 0;

            for (int i = 0; i < particles.Count; ++i) {
                Vector2 otherParticle = particles[i];
                if (particle != otherParticle && Vector2.Distance(particle, otherParticle) <= radius) {
                    numNeighbors++;
                }
            }

            return numNeighbors;
        }

        public static void LichenGrowthEvent(Cluster sourceCluster, int size, LichenParameters parameters, float[,,] layers) {
            size = (int) (size / parameters.scale);
            int numParticles = sourceCluster.particles.Count;
            Vector2 randomParticle = sourceCluster.particles.ElementAt(Random.Range(0, numParticles));
            Vector2 direction = randomParticle == sourceCluster.source ? Random.insideUnitCircle.normalized : (randomParticle - sourceCluster.source).normalized;
            Vector2 particle = randomParticle + direction * (2 * parameters.particleRadius + parameters.spawnEpsilon);

            bool resolved = false;

            for (int step = 0; step < parameters.maxPath && !resolved; ++step) {
                Vector2 tiledHeightPosition = Height.TilePosition(particle, size) * parameters.scale;
                float height = Height.GetAggregatedValue((int) tiledHeightPosition.x, (int) tiledHeightPosition.y, layers);
                float environmentInfluence = CalculateEnvironmentalInfluence(parameters, height);

                // Check if particle is within the death radius.
                if (Vector2.Distance(randomParticle, particle) < parameters.deathRadius) {
                    // Check collision with the current cluster.
                    for (int i = 0; i < sourceCluster.particles.Count; ++i) {
                        Vector2 otherParticle = sourceCluster.particles[i];
                        if (otherParticle != particle && CheckCollision(particle, otherParticle, parameters.particleRadius)) {
                            int numNeighbors = FindNeighbors(sourceCluster.particles, particle, parameters.rho);
                            float theoreticalAggregation = parameters.alpha + (1 - parameters.alpha) * Mathf.Exp(-parameters.sigma * (numNeighbors - parameters.tau));
                            float aggregationProbability = environmentInfluence * theoreticalAggregation;
                            if (Random.value < aggregationProbability) {
                                Vector2 offset = 2.0f * parameters.particleRadius * (particle - otherParticle).normalized;
                                particle = otherParticle + offset;
                                sourceCluster.particles.Add(particle);
                                Vector2 encapsulationOffset = (particle - sourceCluster.source).normalized * parameters.particleRadius;
                                sourceCluster.bounds.Encapsulate(particle + encapsulationOffset);
                            }
                            resolved = true;
                            break;
                        }
                    }
                }

                // Move particle.
                if (!resolved) {
                    particle += Random.insideUnitCircle.normalized * parameters.stepDistance;
                }
            }
        }

        public static void SpawnCluster(ref List<Cluster> clusters, int size, LichenParameters parameters) {
            size = (int) (size / parameters.scale);
            Vector2 position = new Vector2(Random.Range(0.0f, size), Random.Range(0.0f, size));
            clusters.Add(new Cluster(position, parameters));
        }
    }
}