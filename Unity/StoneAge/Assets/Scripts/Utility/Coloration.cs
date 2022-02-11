using UnityEngine;
using UnityEngine.UI;

namespace Utility {
    public class Coloration {

        public static Color BeersLaw(Color color, float alpha) {
            Color result = color * alpha;
            result.a = 1.0f;
            return result;
        }

        public static Color BlendColors(Color foreground, Color background, float foregroundAlpha) {
            return foreground * foregroundAlpha + background * (1 - foregroundAlpha);
        }

        public static void ColorErodedAreas(ref Color[,] color, float[,] erosion, float lowerValueBound, int yearsAged, int maxAge = 1000) {
            int size = color.GetLength(0);

            float effect = Mathf.Clamp01((float) yearsAged / maxAge);

            for (int y = 0; y < size; ++y) {
                for (int x = 0; x < size; ++x) {
                    float erosionAmount = Mathf.Clamp01(erosion[x, y] * 10.0f * effect);
                    Color.RGBToHSV(color[x, y], out float hue, out float saturation, out float value);
                    saturation *= 1 - erosionAmount;
                    value *= Mathf.Lerp(lowerValueBound, 1.0f, 1 - erosionAmount);
                    color[x, y] = Color.HSVToRGB(hue, saturation, value);
                }
            }
        }

        public static void OverlaySediment(ref Color[,] albedo, float[,] sediment, Gradient sedimentGradient, float sedimentOpacityModifier, float[,] noise) {
            int size = sediment.GetLength(0);

            for (int y = 0; y < size; ++y) {
                for (int x = 0; x < size; ++x) {
                    Color sedimentColor = sedimentGradient.Evaluate(noise[x, y]);
                    albedo[x, y] = BlendColors(sedimentColor, albedo[x, y], Mathf.Clamp01(sediment[x, y] * sedimentOpacityModifier));
                }
            }
        }

        public static void SurfaceSolutionDiscoloration(ref Color[,] albedo, float[,] solutionDistribution, float granularity, Color substanceColor, float opacityModifier, int yearsAged, int maxAge = 1000, bool darken = false) {
            int size = albedo.GetLength(0);
            float[,] noise = Textures.PerlinNoise(size, granularity);
            float effect = Mathf.Clamp01((float) yearsAged / maxAge);

            for (int y = 0; y < size; ++y) {
                for (int x = 0; x < size; ++x) {
                    float amount = solutionDistribution[x, y] * noise[x, y];
                    Color color = BeersLaw(substanceColor, darken ? 1 - amount : amount);
                    albedo[x, y] = BlendColors(color, albedo[x, y], Mathf.Clamp01(amount * effect * opacityModifier));
                }
            }
        }
    }
}
