using UnityEngine;

namespace Utility {
    public class Height {

        public static float[,] FinalizeHeight(ref float[,,] layers) {
            int width = layers.GetLength(1);
            int height = layers.GetLength(0);
            int numLayers = layers.GetLength(2);

            float[,] aggregateHeight = new float[width, height];

            for (int y = 0; y < height; ++y) {
                for (int x = 0; x < width; ++x) {
                    for (int i = 0; i < numLayers; ++i) {
                        aggregateHeight[x, y] += layers[x, y, i];
					}
				}
            }

            NormalizeHeight(ref aggregateHeight);

            return aggregateHeight;
        }

        public static Vector2 GetInterpolatedGradient(Vector2 position, float[,,] values) {
            System.Func<float, float, float, float, float, float, Vector2> interpolationFunction =
                (float Pxy, float Px1y, float Pxy1, float Px1y1, float u, float v) => new Vector2( ((Px1y - Pxy) * (1 - v) + (Px1y1 - Pxy1) * v),  ((Pxy1 - Pxy) * (1 - u) + (Px1y1 - Px1y) * u));

            // Linear interpolation of gradients in both directions.
            return GetInterpolatedValue(position, values, interpolationFunction);
        }

        public static float GetInterpolatedHeight(Vector2 position, float[,,] values) {
            System.Func<float, float, float, float, float, float, float> interpolationFunction =
                (float Pxy, float Px1y, float Pxy1, float Px1y1, float u, float v) => (Pxy * (1 - u) + Px1y * u) * (1 - v) + (Pxy1 * (1 - u) + Px1y1 * u) * v;

            // Linear interpolation of surrounding heights.
            return GetInterpolatedValue(position, values, interpolationFunction);
        }

        private static T GetInterpolatedValue<T>(Vector2 position, float[,,] values, System.Func<float, float, float, float, float, float, T> interpolationFunction) {
            // Get whole and fractional parts of the coordinates.
            int x = Mathf.FloorToInt(position.x);
            int y = Mathf.FloorToInt(position.y);
            float u = position.x - x;
            float v = position.y - y;

            float Px1y = GetValueOrBoundary(x + 1, y, values);
            float Pxy = GetValueOrBoundary(x, y, values);
            float Px1y1 = GetValueOrBoundary(x + 1, y + 1, values);
            float Pxy1 = GetValueOrBoundary(x, y + 1, values);

            return interpolationFunction(Pxy, Px1y, Pxy1, Px1y1, u, v);
        }

        public static T GetValueOrBoundary<T>(int x, int y, T[,] values) {
            int width = values.GetLength(1);
            int height = values.GetLength(0);

            // Convert coordinates to boundary if needed.
            x = Mathf.Clamp(x, 0, width - 1);
            y = Mathf.Clamp(y, 0, height - 1);

            return values[x, y];
        }
        
        public static float GetValueOrBoundary(int x, int y, float[,,] values) {
            int width = values.GetLength(1);
            int height = values.GetLength(0);
            int numLayers = values.GetLength(2);

            // Convert coordinates to boundary if needed.
            x = Mathf.Clamp(x, 0, width - 1);
            y = Mathf.Clamp(y, 0, height - 1);

            float sum = 0.0f;

            for (int i = 0; i < numLayers; ++i) {
                sum += values[x, y, i];
			}

            return sum;
        }

        public static void NormalizeHeight(ref float[,] heightBuffer) {
            int size = heightBuffer.GetLength(0);

            // Find min and max height.
            float minHeight = float.MaxValue;
            float maxHeight = -float.MaxValue;
            for (int x = 0; x < size; ++x) {
                for (int y = 0; y < size; ++y) {
                    if (heightBuffer[x, y] < minHeight) minHeight = heightBuffer[x, y];
                    if (heightBuffer[x, y] > maxHeight) maxHeight = heightBuffer[x, y];
                }
            }

            // Normalize into 0 - 1 range.
            float normalizingDenominator = 1 / (maxHeight - minHeight);
            for (int x = 0; x < size; ++x) {
                for (int y = 0; y < size; ++y) {
                    heightBuffer[x, y] = (heightBuffer[x, y] - minHeight) * normalizingDenominator;
                }
            }
        }

        public static void NormalizeHeightLayer(ref float[,,] layers, int layerIndex) {
            int size = layers.GetLength(0);

            // Find min and max height.
            float minHeight = float.MaxValue;
            float maxHeight = -float.MaxValue;
            for (int x = 0; x < size; ++x) {
                for (int y = 0; y < size; ++y) {
                    if (layers[x, y, layerIndex] < minHeight) minHeight = layers[x, y, layerIndex];
                    if (layers[x, y, layerIndex] > maxHeight) maxHeight = layers[x, y, layerIndex];
                }
            }

            // Normalize into 0 - 1 range.
            float normalizingDenominator = 1 / (maxHeight - minHeight);
            for (int x = 0; x < size; ++x) {
                for (int y = 0; y < size; ++y) {
                    layers[x, y, layerIndex] = (layers[x, y, layerIndex] - minHeight) * normalizingDenominator;
                }
            }
        }
    }
}
