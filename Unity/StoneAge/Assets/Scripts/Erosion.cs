using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace StoneAge {
    public class Erosion {

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

        public class ErosionParameters {
            public float blendFactor = 0.0f;
            public float inertia = 0.1f;
            public float capacity = 8.0f;
            public float deposition = 0.1f;
            public float erosion = 0.9f;
            public float evaporation = 0.05f;
            public float radius = 4.0f;
            public float minSlope = 0.01f;
            public int maxPath = 1000;
            public float gravity = 1.0f;
		}

        public static void DepositSediment(Vector2 position, ref double[,] heightBuffer, double amount) {
            int width = heightBuffer.GetLength(1);
            int height = heightBuffer.GetLength(0);

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
                        heightBuffer[x, y] += weights[x - flooredX, y - flooredY] * amount;
					}
				}
			}
        }

        public static void ErosionEvent(ref double[,] heightBuffer) {
            ErosionEvent(ref heightBuffer, new ErosionParameters());
		}

        public static void ErosionEvent(ref double[,] heightBuffer, ErosionParameters parameters) {
            int width = heightBuffer.GetLength(1);
            int height = heightBuffer.GetLength(0);

            Vector2 startPos = new Vector2(Random.Range(0, width), Random.Range(0, height));
            Vector2 startDir = Height.GetInterpolatedGradient(startPos, heightBuffer);

            DropParticle drop = new DropParticle(startPos, startDir);

            for (int step = 0; step < parameters.maxPath; ++step) {
                // Check breaking conditions.
                if (drop.water <= 0 || IsDropOutOfBounds(drop, width, height)) {
                    break;
				}

                // Update position and direction.
                Vector2 gradient = Height.GetInterpolatedGradient(drop.position, heightBuffer);
                Vector2 newDir = drop.direction * parameters.inertia - gradient * (1 - parameters.inertia);

                if (newDir == Vector2.zero) {
                    newDir = Random.insideUnitCircle;
				}

                drop.direction = newDir.normalized;

                double oldHeight = Height.GetInterpolatedHeight(drop.position, heightBuffer);
                Vector2 oldPosition = drop.position;
                drop.position = oldPosition + drop.direction;
                double newHeight = Height.GetInterpolatedHeight(drop.position, heightBuffer);
                double heightDifference = newHeight - oldHeight;

                // Update sediment.
                if (heightDifference > 0.0f) { // New height higher than old height, drop ran uphill.
                    // Deposit sediment in the pit the drop just left.
                    double amount = Mathf.Max((float) heightDifference, drop.sediment);
                    drop.sediment -= (float) amount;
                    DepositSediment(oldPosition, ref heightBuffer, amount);
				} else { // Drop ran downhill.
                    double newCapacity = Mathf.Max((float) -heightDifference, parameters.minSlope) * drop.speed * drop.water * parameters.capacity;

                    if (drop.sediment > newCapacity) {
                        // Deposit the right amount of sediment.
                        double amount = (drop.sediment - newCapacity) * parameters.deposition;
                        drop.sediment -= (float) amount;
                        DepositSediment(oldPosition, ref heightBuffer, amount);
					} else if (drop.sediment < newCapacity) {
                        // Pick up as much sediment as possible.
                        double amount = Mathf.Min((float) (newCapacity - drop.sediment) * parameters.erosion, (float) -heightDifference);
                        drop.sediment += (float) amount;
                        PickUpSediment(oldPosition, ref heightBuffer, parameters.radius, amount);
					}
				}

                // Update speed and water.
                drop.speed = Mathf.Sqrt(drop.speed * drop.speed + (float) heightDifference * parameters.gravity);
                drop.water = drop.water * (1 - parameters.evaporation);
			}
        }

        private static bool IsDropOutOfBounds(DropParticle drop, int width, int height) {
            return drop.position.x < 0 || drop.position.x > width || drop.position.y < 0 || drop.position.y > height;
		}

        private static bool IsIndexInBounds(int x, int y, int width, int height) {
            return x >= 0 && x < width && y >= 0 && y < height;
		}

        private static void PickUpSediment(Vector2 position, ref double[,] heightBuffer, float radius, double amount) {
            int width = heightBuffer.GetLength(1);
            int height = heightBuffer.GetLength(0);

            int flooredX = Mathf.FloorToInt(position.x);
            int flooredY = Mathf.FloorToInt(position.y);
            int flooredRadius = Mathf.FloorToInt(radius);

            double[,] rawWeights = new double[2 * (flooredRadius + 1), 2 * (flooredRadius + 1)];
            double weightSum = 0.0;

            // First pass calculates raw weights and weight sum.
            for (int y = flooredY - flooredRadius; y <= flooredY + flooredRadius + 1; ++y) {
                for (int x = flooredX - flooredRadius; x <= flooredX + flooredRadius + 1; ++x) {
                    double weight = Mathf.Max(0.0f, radius - Vector2.Distance(new Vector2(x, y), position));
                    rawWeights[x - flooredX + flooredRadius, y - flooredY + flooredRadius] = weight;
                    weightSum += weight;
				}
			}

            double normalizationFactor = 1 / weightSum;

            double totalRemoved = 0.0f;

            // Second pass removes sediment.
            for (int y = flooredY - flooredRadius; y <= flooredY + flooredRadius + 1; ++y) {
                for (int x = flooredX - flooredRadius; x <= flooredX + flooredRadius + 1; ++x) {
                    double removedSediment = Mathf.Min(0.0f, (float) (rawWeights[x - flooredX + flooredRadius, y - flooredY + flooredRadius] * normalizationFactor * amount));
                    totalRemoved += removedSediment;
                    if (IsIndexInBounds(x, y, width, height)) {
                        heightBuffer[x, y] -= removedSediment;
					}
                }
            }
        }
    }
}
