using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace StoneAge {
    public class Erosion {

        public enum LayerName {
            Sediment = 0,
            Rock
		}

        private struct DropParticle {
            public Vector2 position;
            public Vector2 direction;
            public float speed;
            public float water;
            public float sediment;

			public DropParticle(Vector2 position, Vector2 direction) {
                this.position = position;
                this.direction = direction;
                speed = 0.0f;
                water = 1.0f;
                sediment = 0.0f;
			}
		}

        public struct ErosionParameters {
            public float inertia;
            public float capacity;
            public float deposition;
            public float erosion;
            public float evaporation;
            public float radius;
            public float minSlope;
            public int maxPath;
            public float gravity;
            public float[] erosionFactors;

            public ErosionParameters(float inertia = 0.1f, float capacity = 8.0f, float deposition = 0.1f, float erosion = 0.9f, float evaporation = 0.05f, float radius = 4.0f, float minSlope = 0.01f, int maxPath = 1000, float gravity = 1.0f, float sedimentErosionFactor = 0.9f, float rockErosionFactor = 0.5f) {
                this.inertia = inertia;
                this.capacity = capacity;
                this.deposition = deposition;
                this.erosion = erosion;
                this.evaporation = evaporation;
                this.radius = radius;
                this.minSlope = minSlope;
                this.maxPath = maxPath;
                this.gravity = gravity;
                erosionFactors = new float[] { sedimentErosionFactor, rockErosionFactor};
			}
		}

        private static void DepositSediment(Vector2 position, ref float[,,] layers, float amount) {
            int width = layers.GetLength(1);
            int height = layers.GetLength(0);

            // Get whole and fractional parts of the coordinates.
            int flooredX = Mathf.FloorToInt(position.x);
            int flooredY = Mathf.FloorToInt(position.y);
            float u = position.x - flooredX;
            float v = position.y - flooredY;

            // Weights are reverse-linear interpolated based on u and v.
            float[,] weights = new float[2, 2];
            weights[0, 0] = (1 - v) * (1 - u);
            weights[1, 0] = (1 - v) * u;
            weights[0, 1] = v * (1 - u);
            weights[1, 1] = v * u;

            for (int y = flooredY; y <= flooredY + 1; ++y) {
                for (int x = flooredX; x <= flooredX + 1; ++x) {
                    if (IsIndexInBounds(x, y, width, height)) {
                        layers[x, y, (int) LayerName.Sediment] += weights[x - flooredX, y - flooredY] * amount;
					}
				}
			}
        }

        public static int ErosionEvent(ref float[,,] layers) {
            return ErosionEvent(ref layers, new ErosionParameters());
		}

        public static int ErosionEvent(ref float[,,] layers, ErosionParameters parameters) {
            int width = layers.GetLength(1);
            int height = layers.GetLength(0);

            Vector2 startPos = new Vector2(Random.Range(0, width), Random.Range(0, height));
            Vector2 startDir = Height.GetInterpolatedGradient(startPos, layers);

            DropParticle drop = new DropParticle(startPos, startDir);

            int numSteps = 0;

            for (int step = 0; step < parameters.maxPath; ++step) {
                // Check breaking conditions.
                if (drop.water <= 0 || IsDropOutOfBounds(drop, width, height)) {
                    numSteps = step; // TEMP
                    break;
				}

                if (step == parameters.maxPath - 1) {
                    numSteps = step; // TEMP
				}

                // Update position and direction.
                Vector2 gradient = Height.GetInterpolatedGradient(drop.position, layers);
                Vector2 newDir = drop.direction * parameters.inertia - gradient * (1 - parameters.inertia);

                if (newDir == Vector2.zero) {
                    newDir = Random.insideUnitCircle;
				}

                drop.direction = newDir.normalized;

                float oldHeight = Height.GetInterpolatedHeight(drop.position, layers);
                Vector2 oldPosition = drop.position;
                drop.position = oldPosition + drop.direction;
                float newHeight = Height.GetInterpolatedHeight(drop.position, layers);
                float heightDifference = newHeight - oldHeight;

                // Update sediment.
                if (heightDifference > 0.0f) { // New height higher than old height, drop ran uphill.
                    // Deposit sediment in the pit the drop just left.
                    float amount = Mathf.Max( heightDifference, drop.sediment);
                    drop.sediment -=  amount;
                    DepositSediment(oldPosition, ref layers, amount);
				} else { // Drop ran downhill.
                    float newCapacity = Mathf.Max( -heightDifference, parameters.minSlope) * drop.speed * drop.water * parameters.capacity;

                    if (drop.sediment > newCapacity) {
                        // Deposit the right amount of sediment to obey the new capacity.
                        float amount = (drop.sediment - newCapacity) * parameters.deposition;
                        drop.sediment -=  amount;
                        DepositSediment(oldPosition, ref layers, amount);
					} else if (drop.sediment < newCapacity) {
                        // Pick up as much sediment as possible with new capacity.
                        float amount = Mathf.Min( (newCapacity - drop.sediment) * parameters.erosion,  -heightDifference);
                        drop.sediment +=  PickUpSediment(oldPosition, ref layers, parameters.radius, amount, parameters.erosionFactors);
					}
				}

                // Update speed and water.
                drop.speed = Mathf.Sqrt(drop.speed * drop.speed +  heightDifference * parameters.gravity);
                drop.water *= (1 - parameters.evaporation);
			}

            return numSteps;
        }

        private static bool IsDropOutOfBounds(DropParticle drop, int width, int height) {
            return drop.position.x < 0 || drop.position.x > width || drop.position.y < 0 || drop.position.y > height;
		}

        private static bool IsIndexInBounds(int x, int y, int width, int height) {
            return x >= 0 && x < width && y >= 0 && y < height;
		}

        private static float PickUpSediment(Vector2 position, ref float[,,] layers, float radius, float amount, float[] erosionFactors) {
            int width = layers.GetLength(1);
            int height = layers.GetLength(0);

            int flooredX = Mathf.FloorToInt(position.x);
            int flooredY = Mathf.FloorToInt(position.y);
            int flooredRadius = Mathf.FloorToInt(radius);

            float[,] rawWeights = new float[2 * (flooredRadius + 1), 2 * (flooredRadius + 1)];
            float weightSum = 0.0f;

            // First pass calculates raw weights and weight sum.
            for (int y = flooredY - flooredRadius; y <= flooredY + flooredRadius + 1; ++y) {
                for (int x = flooredX - flooredRadius; x <= flooredX + flooredRadius + 1; ++x) {
                    float weight = Mathf.Max(0.0f, radius - Vector2.Distance(new Vector2(x, y), position));
                    rawWeights[x - flooredX + flooredRadius, y - flooredY + flooredRadius] = weight;
                    weightSum += weight;
				}
			}

            float normalizationFactor = 1 / weightSum;

            float totalRemoved = 0.0f;

            // Second pass removes sediment.
            System.Array layerNames = System.Enum.GetValues(typeof(LayerName));
            for (int y = flooredY - flooredRadius; y <= flooredY + flooredRadius + 1; ++y) {
                for (int x = flooredX - flooredRadius; x <= flooredX + flooredRadius + 1; ++x) {
                    float removedHeight = Mathf.Min(0.0f,  rawWeights[x - flooredX + flooredRadius, y - flooredY + flooredRadius] * normalizationFactor * amount);
                    if (IsIndexInBounds(x, y, width, height)) {
                        for (int i = 0; i < layerNames.Length; ++i) {
                            int layerIndex = (int) layerNames.GetValue(i);
                            float removedHeightLayer = removedHeight * erosionFactors[i];
                            float heightDifference = layers[x, y, layerIndex] - removedHeightLayer;

                            if (heightDifference >= 0.0) { // All or some of this layer was removed, no remainder to remove.
                                layers[x, y, layerIndex] -= removedHeightLayer;
                                totalRemoved += removedHeightLayer;
							} else { // All of this layer was removed, with remainder for next layer.
                                totalRemoved += layers[x, y, layerIndex];
                                layers[x, y, layerIndex] = 0.0f;
                                removedHeight = -heightDifference;
                            }
                        }
					}
                }
            }

            return totalRemoved;
        }
    }
}
