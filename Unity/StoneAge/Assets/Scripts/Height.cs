using UnityEngine;

namespace Utility {
    public class Height {

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

			// Find min and max height.
			double minHeight = double.MaxValue;
			double maxHeight = -double.MaxValue;
			for (int x = 0; x < size; ++x) {
				for (int y = 0; y < size; ++y) {
					if (resultHeights[x, y] < minHeight) minHeight = resultHeights[x, y];
					if (resultHeights[x, y] > maxHeight) maxHeight = resultHeights[x, y];
				}
			}

			// Normalize into 0 - 1 range.
			double normalizingDenominator = 1 / (maxHeight - minHeight);
			for (int x = 0; x < size; ++x) {
				for (int y = 0; y < size; ++y) {
					resultHeights[x, y] = (resultHeights[x, y] - minHeight) * normalizingDenominator;
				}
			}

			return resultHeights;
        }
    }
}
