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

        [System.Serializable]
        public class ErosionParameters {
            public float inertia;
            public float capacity;
            public float deposition;
            public float erosion;
            public float evaporation;
            public float radius;
            public float minSlope;
            public int maxPath;
            public float gravity;

            public ErosionParameters(float inertia = 0.1f, float capacity = 8.0f, float deposition = 0.1f, float erosion = 0.9f, float evaporation = 0.05f, float radius = 4.0f, float minSlope = 0.01f, int maxPath = 1000, float gravity = 1.0f) {
                this.inertia = inertia;
                this.capacity = capacity;
                this.deposition = deposition;
                this.erosion = erosion;
                this.evaporation = evaporation;
                this.radius = radius;
                this.minSlope = minSlope;
                this.maxPath = maxPath;
                this.gravity = gravity;
            }
        }

        private static void DepositSediment(Vector2 position, ref float[,] heightBuffer, float amount) {
            int size = heightBuffer.GetLength(0);

            // Get whole and fractional parts of the coordinates.
            int flooredX = Mathf.FloorToInt(position.x);
            int flooredY = Mathf.FloorToInt(position.y);
            float u = position.x - flooredX;
            float v = position.y - flooredY;

            // Tile the whole coordinates.
            flooredX = Height.TileCoordinate(flooredX, size);
            flooredY = Height.TileCoordinate(flooredY, size);
            int bumpedX = Height.TileCoordinate(flooredX + 1, size);
            int bumpedY = Height.TileCoordinate(flooredY + 1, size);

            // Weights are reverse-linear interpolated based on u and v.
            float[,] weights = new float[2, 2];
            weights[0, 0] = (1 - v) * (1 - u);
            weights[1, 0] = (1 - v) * u;
            weights[0, 1] = v * (1 - u);
            weights[1, 1] = v * u;

            heightBuffer[flooredX, flooredY] += weights[0, 0] * amount;
            heightBuffer[bumpedX, flooredY] += weights[1, 0] * amount;
            heightBuffer[flooredX, bumpedY] += weights[0, 1] * amount;
            heightBuffer[bumpedX, bumpedY] += weights[1, 1] * amount;
        }

        public static int ErosionEvent(ref float[,] heightBuffer) {
            return ErosionEvent(ref heightBuffer, new ErosionParameters());
        }

        public static int ErosionEvent(ref float[,] heightBuffer, ErosionParameters parameters) {
            int size = heightBuffer.GetLength(0);

            Vector2 startPos = new Vector2(Random.Range(0, size), Random.Range(0, size));
            Vector2 startDir = Height.GetInterpolatedGradient(startPos, heightBuffer);

            DropParticle drop = new DropParticle(startPos, startDir);

            int numSteps = 0;

            for (int step = 0; step < parameters.maxPath; ++step) {
                // Check breaking conditions.
                if (drop.water <= 0) {
                    numSteps = step;
                    break;
                }

                if (step == parameters.maxPath - 1) {
                    numSteps = step;
                }

                // Update position and direction.
                Vector2 gradient = Height.GetInterpolatedGradient(drop.position, heightBuffer);
                Vector2 newDir = drop.direction * parameters.inertia - gradient * (1 - parameters.inertia);

                if (newDir == Vector2.zero) {
                    newDir = Random.insideUnitCircle;
                }

                drop.direction = newDir.normalized;

                float oldHeight = Height.GetInterpolatedHeight(drop.position, heightBuffer);
                Vector2 oldPosition = drop.position;
                drop.position = oldPosition + drop.direction;
                float newHeight = Height.GetInterpolatedHeight(drop.position, heightBuffer);
                float heightDifference = newHeight - oldHeight;

                // Update sediment.
                if (heightDifference > 0.0f) { // New height higher than old height, drop ran uphill.
                    // Deposit sediment in the pit the drop just left.
                    float amount = Mathf.Max(heightDifference, drop.sediment);
                    drop.sediment -= amount;
                    DepositSediment(oldPosition, ref heightBuffer, amount);
                } else { // Drop ran downhill.
                    float newCapacity = Mathf.Max(-heightDifference, parameters.minSlope) * drop.speed * drop.water * parameters.capacity;

                    if (drop.sediment >= newCapacity) {
                        // Deposit the right amount of sediment to obey the new capacity.
                        float amount = (drop.sediment - newCapacity) * parameters.deposition;
                        drop.sediment -= amount;
                        DepositSediment(oldPosition, ref heightBuffer, amount);
                    } else {
                        // Pick up as much sediment as possible with new capacity.
                        float amount = Mathf.Min((newCapacity - drop.sediment) * parameters.erosion, -heightDifference);
                        drop.sediment += amount;
                        PickUpSediment(oldPosition, ref heightBuffer, parameters.radius, amount);
                    }
                }

                // Update speed and water.
                drop.speed = Mathf.Sqrt(drop.speed * drop.speed + heightDifference * parameters.gravity);
                drop.water *= 1 - parameters.evaporation;
                drop.water -= Mathf.Epsilon;
            }

            return numSteps;
        }

        private static void PickUpSediment(Vector2 position, ref float[,] heightBuffer, float radius, float amount) {
            int size = heightBuffer.GetLength(0);

            int flooredX = Mathf.FloorToInt(position.x);
            int flooredY = Mathf.FloorToInt(position.y);
            int flooredRadius = Mathf.FloorToInt(radius);

            float[,] rawWeights = new float[2 * (flooredRadius + 1), 2 * (flooredRadius + 1)];
            float weightSum = 0.0f;

            // First pass calculates raw weights and weight sum for nodes within radius (affected nodes).
            for (int y = flooredY - flooredRadius; y <= flooredY + flooredRadius + 1; ++y) {
                for (int x = flooredX - flooredRadius; x <= flooredX + flooredRadius + 1; ++x) {
                    float weight = Mathf.Max(0.0f, radius - Vector2.Distance(new Vector2(x, y), position));
                    rawWeights[x - flooredX + flooredRadius, y - flooredY + flooredRadius] = weight;
                    weightSum += weight;
                }
            }

            float normalizationFactor = 1 / weightSum;

            // Tile the whole coordinates.
            flooredX = Height.TileCoordinate(flooredX, size);
            flooredY = Height.TileCoordinate(flooredY, size);

            // Second pass removes sediment.
            for (int y = flooredY - flooredRadius; y <= flooredY + flooredRadius + 1; ++y) {
                int tiledY = Height.TileCoordinate(y, size);
                for (int x = flooredX - flooredRadius; x <= flooredX + flooredRadius + 1; ++x) {
                    int tiledX = Height.TileCoordinate(x, size);
                    float removedHeight = Mathf.Max(0.0f, rawWeights[x - flooredX + flooredRadius, y - flooredY + flooredRadius] * normalizationFactor * amount);
                    heightBuffer[tiledX, tiledY] -= removedHeight;
                }
            }
        }
    }
}
