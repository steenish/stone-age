using UnityEngine;

namespace Utility {
    public class Height {

        public static Vector2 GetInterpolatedGradient(Vector2 position, double[,] values) {
            System.Func<double, double, double, double, float, float, Vector2> interpolationFunction =
                (double Pxy, double Px1y, double Pxy1, double Px1y1, float u, float v) => new Vector2((float) ((Px1y - Pxy) * (1 - v) + (Px1y1 - Pxy1) * v), (float) ((Pxy1 - Pxy) * (1 - u) + (Px1y1 - Px1y) * u));

            // Linear interpolation of gradients in both directions.
            return GetInterpolatedValue(position, values, interpolationFunction);
        }

        public static double GetInterpolatedHeight(Vector2 position, double[,] values) {
            System.Func<double, double, double, double, float, float, double> interpolationFunction =
                (double Pxy, double Px1y, double Pxy1, double Px1y1, float u, float v) => (Pxy * (1 - u) + Px1y * u) * (1 - v) + (Pxy1 * (1 - u) + Px1y1 * u) * v;

            // Linear interpolation of surrounding heights.
            return GetInterpolatedValue(position, values, interpolationFunction);
        }

        private static T GetInterpolatedValue<T>(Vector2 position, double[,] values, System.Func<double, double, double, double, float, float, T> interpolationFunction) {
            // Get whole and fractional parts of the coordinates.
            int x = Mathf.FloorToInt(position.x);
            int y = Mathf.FloorToInt(position.y);
            float u = position.x - x;
            float v = position.y - y;

            double Px1y = GetValueOrBoundary(x + 1, y, values);
            double Pxy = GetValueOrBoundary(x, y, values);
            double Px1y1 = GetValueOrBoundary(x + 1, y + 1, values);
            double Pxy1 = GetValueOrBoundary(x, y + 1, values);

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

        public static double[,] HeightFromAlbedo(Texture2D albedoMap) {
            int size = albedoMap.width;
            double[,] resultHeights = new double[size, size];

            // Extract colors from the albedo map.
            Color[] colors = albedoMap.GetPixels();
            for (int y = 0; y < size; ++y) {
                for (int x = 0; x < size; ++x) {
                    resultHeights[x, y] = colors[y * size + x].grayscale;
                }
            }

            NormalizeHeight(ref resultHeights);

			return resultHeights;
        }

        public static void NormalizeHeight(ref double[,] heightBuffer) {
            int size = heightBuffer.GetLength(0);

            // Find min and max height.
            double minHeight = double.MaxValue;
            double maxHeight = -double.MaxValue;
            for (int x = 0; x < size; ++x) {
                for (int y = 0; y < size; ++y) {
                    if (heightBuffer[x, y] < minHeight) minHeight = heightBuffer[x, y];
                    if (heightBuffer[x, y] > maxHeight) maxHeight = heightBuffer[x, y];
                }
            }

            // Normalize into 0 - 1 range.
            double normalizingDenominator = 1 / (maxHeight - minHeight);
            for (int x = 0; x < size; ++x) {
                for (int y = 0; y < size; ++y) {
                    heightBuffer[x, y] = (heightBuffer[x, y] - minHeight) * normalizingDenominator;
                }
            }
        }
    }
}
