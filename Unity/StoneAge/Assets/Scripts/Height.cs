using UnityEngine;

namespace Utility {
    public class Height {

        public static Vector2 GetInterpolatedGradient(Vector2 position, float[,] values) {
            static Vector2 interpolationFunction(float Pxy, float Px1y, float Pxy1, float Px1y1, float u, float v) => new Vector2((Px1y - Pxy) * (1 - v) + (Px1y1 - Pxy1) * v, (Pxy1 - Pxy) * (1 - u) + (Px1y1 - Px1y) * u);

            // Linear interpolation of gradients in both directions.
            return GetInterpolatedValue(position, values, interpolationFunction);
        }

        public static float GetInterpolatedHeight(Vector2 position, float[,] values) {
            static float interpolationFunction(float Pxy, float Px1y, float Pxy1, float Px1y1, float u, float v) => (Pxy * (1 - u) + Px1y * u) * (1 - v) + (Pxy1 * (1 - u) + Px1y1 * u) * v;

            // Linear interpolation of surrounding heights.
            return GetInterpolatedValue(position, values, interpolationFunction);
        }

        private static T GetInterpolatedValue<T>(Vector2 position, float[,] values, System.Func<float, float, float, float, float, float, T> interpolationFunction) {
            int size = values.GetLength(0);

            // Get whole and fractional parts of the coordinates.
            int x = Mathf.FloorToInt(position.x);
            int y = Mathf.FloorToInt(position.y);
            float u = position.x - x;
            float v = position.y - y;

            // Ensure positive coordinates, then tile the whole coordinates as well as the incremented coordinates.
            x = TileCoordinate(x, size);
            y = TileCoordinate(y, size);
            int bumpedX = (x + 1) % size;
            int bumpedY = (y + 1) % size;

            float Px1y = values[bumpedX, y];
            float Pxy = values[x, y];
            float Px1y1 = values[bumpedX, bumpedY];
            float Pxy1 = values[x, bumpedY];

            return interpolationFunction(Pxy, Px1y, Pxy1, Px1y1, u, v);
        }

        public static void NormalizeHeight(ref float[] heightBuffer) {
            int size = heightBuffer.GetLength(0);

            // Find min and max height.
            float minHeight = float.MaxValue;
            float maxHeight = -float.MaxValue;
            for (int i = 0; i < size * size; ++i) {
                if (heightBuffer[i] < minHeight) minHeight = heightBuffer[i];
                if (heightBuffer[i] > maxHeight) maxHeight = heightBuffer[i];
            }

            // Normalize into 0 - 1 range.
            float normalizingDenominator = 1 / (maxHeight - minHeight + Mathf.Epsilon);
            for (int i = 0; i < size * size; ++i) {
                heightBuffer[i] = (heightBuffer[i] - minHeight) * normalizingDenominator;
            }
        }

        public static int TileCoordinate(int coordinate, int bound) {
            int tiledCoordinate = coordinate;

            // Ensure tiledCoordinate is positve by adding the right amount of bounds to reach positive.
            if (tiledCoordinate < 0) {
                tiledCoordinate += bound * (1 + (Mathf.Abs(coordinate) / bound));
            }

            // Tile tiledCoordinate to within bound.
            if (tiledCoordinate >= bound) {
                tiledCoordinate %= bound;
            }

            return tiledCoordinate;
        }
    }
}
