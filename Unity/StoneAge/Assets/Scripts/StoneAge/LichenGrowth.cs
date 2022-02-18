using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace StoneAge {
    public class LichenGrowth {

        public class LichenParameters {
            public int initialSeeds;
            public float alpha;
            public float sigma;
            public float tau;
            public float rho;
            public int maxPath;
            public float particleRadius;
            public float spawnEpsilon;
            public float stepDistance;
            public float deathRadius;
            public float aggregationProbabilityThreshold;

            public LichenParameters(int initialSeeds = 5, float alpha = 1e-4f, float sigma = 1.0f, float tau = 3.0f, float rho = 2.5f, int maxPath = 100, float particleRadius = 1.0f, float stepDistance = 1.0f, float deathRadius = 5.0f, float aggregationProbability = 0.5f) {
                this.initialSeeds = initialSeeds;
                this.alpha = alpha;
                this.sigma = sigma;
                this.tau = tau;
                this.rho = rho;
                this.maxPath = maxPath;
                this.particleRadius = particleRadius;
                spawnEpsilon = particleRadius * 0.5f;
                this.stepDistance = stepDistance;
                this.deathRadius = deathRadius;
                this.aggregationProbabilityThreshold = aggregationProbability;
            }
        }

        public class Cluster {
            public LichenParticle source { get; private set; }
            public List<LichenParticle> particles { get; private set; }

            public Cluster(Vector2 sourcePosition, LichenParameters parameters) {
                source = new LichenParticle(sourcePosition, parameters.particleRadius);
                particles = new List<LichenParticle> {
                    source
                };
            }
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

        public static void LichenGrowthEvent(List<Cluster> clusters, int clusterIndex, int size, LichenParameters parameters) {
            Cluster sourceCluster = clusters[clusterIndex];
            int numParticles = sourceCluster.particles.Count;
            LichenParticle randomParticle = sourceCluster.particles[Random.Range(0, numParticles)];
            Vector2 direction = (randomParticle.position - sourceCluster.source.position).normalized;
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
                    Vector2 comparisonPosition = tiledInRange ? particle.position : untiledPosition;
                    
                    for (int i = 0; i < sourceCluster.particles.Count; ++i) {
                        LichenParticle otherParticle = sourceCluster.particles[i];
                        if (otherParticle != particle && CheckCollision(comparisonPosition, otherParticle.position, parameters.particleRadius)) {
                            // TODO CHECK AGGREGATION
                            int numNeighbors = FindNeighbors(sourceCluster.particles, particle, parameters.particleRadius);
                            float theoreticalAggregation = parameters.alpha + (1 - parameters.alpha) * Mathf.Exp(-parameters.sigma * (numNeighbors - parameters.tau));
                            float environmentInfluence = 0.1f; // TODO

                            resolved = true;
                            break;
                        }
                    }
                }

                // Check collision with other clusters.
                if (!resolved) {
                    for (int i = 0; i < clusters.Count && !resolved; ++i) {
                        Cluster cluster = clusters[i];
                        if (cluster != sourceCluster) {
                            List<LichenParticle> particles = cluster.particles;
                            for (int j = 0; j < particles.Count; ++j) {
                                if (CheckCollision(particle.position, particles[i].position, parameters.particleRadius)) {
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

        private static bool CheckCollision(Vector2 position1, Vector2 position2, float radius) {
            return Vector2.SqrMagnitude(position1 - position2) <= radius * radius;
        }

        private static int FindNeighbors(List<LichenParticle> particles, LichenParticle particle, float radius) {
            return 1; // TODO
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
    }
}