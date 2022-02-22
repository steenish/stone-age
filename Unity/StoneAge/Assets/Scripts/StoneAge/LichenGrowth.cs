using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace StoneAge {
    public class LichenGrowth {

        public class Cluster {
            public LichenParticle source { get; private set; }
            public List<LichenParticle> particles { get; private set; }
            public Bounds bounds { get; private set; }
            public Species species { get; private set; }

            public Cluster(Vector2 sourcePosition, LichenParameters parameters) {
                source = new LichenParticle(sourcePosition, parameters.particleRadius);
                particles = new List<LichenParticle> {
                    source
                };
                bounds = new Bounds(source.position, Vector3.one * source.radius);
                species = parameters.species[Random.Range(0, parameters.species.Length)];
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
            public Species[] species;
            public AnimationCurve directLightSensitivity;
            public AnimationCurve indirectLightSensitivity;
            public AnimationCurve moistureSensitivity;
        }

        public class LichenParticle {
            public Vector2 position { get; set; }
            public float radius { get; private set; }
            public int age { get; set; } = 0;

            public LichenParticle(Vector2 position, float radius) {
                this.position = position;
                this.radius = radius;
            }
        }

        [System.Serializable]
        public class Species {
            public Gradient colorGradient;

            public Species(Gradient colorGradient) {
                this.colorGradient = colorGradient;
            }
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

        public static Color[,] CreateLichenBuffer(List<Cluster> clusters, int size, Shader voronoiShader, LichenParameters parameters) {
            Material voronoiMaterial = new Material(voronoiShader);

            RenderTexture result = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            RenderTexture dummy = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);

            for (int i = 0; i < clusters.Count; ++i) {
                List<LichenParticle> particles = clusters[i].particles;
                Vector4[] particlePositions = new Vector4[particles.Count];
                for (int j = 0; j < particles.Count; ++j) {
                    particlePositions[j] = particles[j].position;
                }
                Color clusterColor = clusters[i].species.colorGradient.Evaluate(0);

                voronoiMaterial.SetInt("_ParticleCount", particles.Count);
                voronoiMaterial.SetInt("_Size", size);
                voronoiMaterial.SetFloat("_Scale", parameters.scale);
                voronoiMaterial.SetVectorArray("_Particles", particlePositions);
                voronoiMaterial.SetFloat("_MaxDistance", 2.0f * parameters.particleRadius);
                voronoiMaterial.SetColor("_ClusterColor", clusterColor);

                Graphics.Blit(result, dummy, voronoiMaterial);
                Graphics.Blit(dummy, result);
            }

            return Conversion.CreateColorBuffer(Textures.GetRTPixels(result));

            //for (int i = 0; i < clusters.Count; ++i) {
            //    List<LichenParticle> particles = clusters[i].particles;
            //    Color clusterColor = clusters[i].species.colorGradient.Evaluate(0);
            //    for (int y = 0; y < size; ++y) {
            //        for (int x = 0; x < size; ++x) {
            //            float minDistance = float.PositiveInfinity;
            //            for (int j = 0; j < particles.Count; ++j) {
            //                float distance = Vector2.Distance(new Vector2(x, y), particles[j].position);
            //                if (distance < minDistance) {
            //                    minDistance = distance;
            //                }
            //            }
            //            if (minDistance < 2.0f * parameters.particleRadius) {
            //                result[x, y] = clusterColor;
            //            }
            //        }
            //    }
            //}

            //return result;
        }

        private static int FindNeighbors(List<LichenParticle> particles, LichenParticle particle, float radius) {
            int numNeighbors = 0;

            for (int i = 0; i < particles.Count; ++i) {
                LichenParticle otherParticle = particles[i];
                if (particle != otherParticle && Vector2.Distance(particle.position, otherParticle.position) <= radius) {
                    numNeighbors++;
                }
            }

            return numNeighbors;
        }

        private static Vector2 FindUntiledPosition(Vector2 tiledPoint, Vector2 targetPoint, float bound) {
            bool targetAboveMiddle = targetPoint.y > bound * 0.5f;
            bool targetRightOfMiddle = targetPoint.x > bound * 0.5f;

            bool tiledAboveMiddle = tiledPoint.y > bound * 0.5f;
            bool tiledRightOfMiddle = tiledPoint.x > bound * 0.5f;

            Vector2 untiledPoint = Vector2.zero;

            if (targetAboveMiddle && targetRightOfMiddle) {
                // Target is in upper right quadrant.
                if (tiledAboveMiddle && !tiledRightOfMiddle) {
                    // Tiled is in upper left quadrant.
                    untiledPoint = new Vector2(tiledPoint.x + bound, tiledPoint.y);
                } else if (!tiledAboveMiddle && !tiledRightOfMiddle) {
                    // Tiled is in lower left quadrant.
                    untiledPoint = new Vector2(tiledPoint.x + bound, tiledPoint.y + bound);
                } else if (!tiledAboveMiddle && tiledRightOfMiddle) {
                    // Tiled is in lower right quadrant.
                    untiledPoint = new Vector2(tiledPoint.x, tiledPoint.y + bound);
                }
            } else if (targetAboveMiddle && !targetRightOfMiddle) {
                // Target is in upper left quadrant.
                if (tiledAboveMiddle && tiledRightOfMiddle) {
                    // Tiled is in upper right quadrant.
                    untiledPoint = new Vector2(tiledPoint.x - bound, tiledPoint.y);
                } else if (!tiledAboveMiddle && !tiledRightOfMiddle) {
                    // Tiled is in lower left quadrant.
                    untiledPoint = new Vector2(tiledPoint.x, tiledPoint.y + bound);
                } else if (!tiledAboveMiddle && tiledRightOfMiddle) {
                    // Tiled is in lower right quadrant.
                    untiledPoint = new Vector2(tiledPoint.x - bound, tiledPoint.y + bound);
                }
            } else if (!targetAboveMiddle && !targetRightOfMiddle) {
                // Target is in lower left quadrant.
                if (tiledAboveMiddle && tiledRightOfMiddle) {
                    // Tiled is in upper right quadrant.
                    untiledPoint = new Vector2(tiledPoint.x - bound, tiledPoint.y - bound);
                } else if (tiledAboveMiddle && !tiledRightOfMiddle) {
                    // Tiled is in upper left quadrant.
                    untiledPoint = new Vector2(tiledPoint.x, tiledPoint.y - bound);
                } else if (!tiledAboveMiddle && tiledRightOfMiddle) {
                    // Tiled is in lower right quadrant.
                    untiledPoint = new Vector2(tiledPoint.x - bound, tiledPoint.y);
                }
            } else if (!targetAboveMiddle && targetRightOfMiddle) {
                // Target is in lower right quadrant.
                if (tiledAboveMiddle && tiledRightOfMiddle) {
                    // Tiled is in upper right quadrant.
                    untiledPoint = new Vector2(tiledPoint.x, tiledPoint.y - bound);
                } else if (tiledAboveMiddle && !tiledRightOfMiddle) {
                    // Tiled is in upper left quadrant.
                    untiledPoint = new Vector2(tiledPoint.x + bound, tiledPoint.y - bound);
                } else if (!tiledAboveMiddle && !tiledRightOfMiddle) {
                    // Tiled is in lower left quadrant.
                    untiledPoint = new Vector2(tiledPoint.x + bound, tiledPoint.y);
                }
            }

            return untiledPoint;
        }

        public static void LichenGrowthEvent(List<Cluster> clusters, int clusterIndex, int size, LichenParameters parameters, float[,,] layers) {
            size = (int) (size / parameters.scale);
            Cluster sourceCluster = clusters[clusterIndex];
            int numParticles = sourceCluster.particles.Count;
            LichenParticle randomParticle = sourceCluster.particles[Random.Range(0, numParticles)];
            Vector2 direction = randomParticle == sourceCluster.source ? Random.insideUnitCircle.normalized : (randomParticle.position - sourceCluster.source.position).normalized;
            Vector2 newPosition = randomParticle.position + direction * (2 * parameters.particleRadius + parameters.spawnEpsilon);
            newPosition = Height.TilePosition(newPosition, size);
            LichenParticle particle = new LichenParticle(newPosition, parameters.particleRadius);

            // Update ages.
            for (int i = 0; i < sourceCluster.particles.Count; ++i) {
                sourceCluster.particles[i].age += 1;
            }

            bool resolved = false;

            for (int step = 0; step < parameters.maxPath && !resolved; ++step) {
                // Check if particle in its tiled or untiled position are inside the radius.
                Vector2 untiledPosition = FindUntiledPosition(particle.position, randomParticle.position, size);
                bool tiledInRange = Vector2.Distance(randomParticle.position, particle.position) < parameters.deathRadius;
                bool untiledInRange = Vector2.Distance(randomParticle.position, untiledPosition) < parameters.deathRadius;

                // If either are true, particle is inside the radius.
                if (tiledInRange || untiledInRange) {
                    // Check collision with the current cluster.
                    // Get the tiled position first, then the untiled position if necessary.
                    Vector2 comparisonPosition = tiledInRange ? particle.position : untiledPosition;

                    for (int i = 0; i < sourceCluster.particles.Count; ++i) {
                        LichenParticle otherParticle = sourceCluster.particles[i];
                        if (otherParticle != particle && CheckCollision(comparisonPosition, otherParticle.position, parameters.particleRadius)) {
                            int numNeighbors = FindNeighbors(sourceCluster.particles, particle, parameters.rho);
                            float theoreticalAggregation = parameters.alpha + (1 - parameters.alpha) * Mathf.Exp(-parameters.sigma * (numNeighbors - parameters.tau));
                            float height = Height.GetAggregatedValue((int) (particle.position.x * parameters.scale), (int) (particle.position.y * parameters.scale), layers);
                            float environmentInfluence = CalculateEnvironmentalInfluence(parameters, height);
                            float aggregationProbability = environmentInfluence * theoreticalAggregation;
                            if (Random.value < aggregationProbability) {
                                Vector2 offset = 2.0f * parameters.particleRadius * (particle.position - otherParticle.position).normalized;
                                particle.position = Height.TilePosition(otherParticle.position + offset, size);
                                sourceCluster.particles.Add(particle);
                                Vector2 encapsulationOffset = (particle.position - sourceCluster.source.position).normalized * parameters.particleRadius;
                                sourceCluster.bounds.Encapsulate(particle.position + encapsulationOffset);
                            }
                            resolved = true;
                            break;
                        }
                    }
                }

                // Check collision with other clusters.
                if (!resolved) {
                    for (int i = 0; i < clusters.Count && !resolved; ++i) {
                        Cluster cluster = clusters[i];
                        if (cluster != sourceCluster && cluster.bounds.Contains(particle.position)) {
                            List<LichenParticle> particles = cluster.particles;
                            for (int j = 0; j < particles.Count; ++j) {
                                if (CheckCollision(particle.position, particles[j].position, parameters.particleRadius)) {
                                    resolved = true;
                                    break;
                                }
                            }
                        }
                    }
                }

                // Move particle, making sure to tile the new position.
                if (!resolved) {
                    particle.position += Random.insideUnitCircle.normalized * parameters.stepDistance;
                    particle.position = Height.TilePosition(particle.position, size);
                }
            }
        }

        public static void SpawnCluster(ref List<Cluster> clusters, int size, LichenParameters parameters) {
            size = (int) (size / parameters.scale);
            bool spawned = false;
            
            for (int i = 0; i < 100 && !spawned; ++i) {
                Vector2 position = new Vector2(Random.Range(0.0f, size), Random.Range(0.0f, size));
                bool collided = false;

                for (int j = 0; j < clusters.Count && !collided; ++j) {
                    Cluster cluster = clusters[j];
                    if (cluster.bounds.Contains(position)) {
                        List<LichenParticle> particles = cluster.particles;
                        for (int k = 0; k < particles.Count; ++k) {
                            if (CheckCollision(position, particles[k].position, parameters.particleRadius)) {
                                collided = true;
                                break;
                            }
                        }
                    }
                }

                if (!collided) {
                    clusters.Add(new Cluster(position, parameters));
                    break;
                }
            }
        }
    }
}