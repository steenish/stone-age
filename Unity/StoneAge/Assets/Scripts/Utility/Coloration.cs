using UnityEngine;
using UnityEngine.UI;

namespace Utility {
    public class Coloration {

        [System.Serializable]
        public class ColorationParameters {
            [Range(1, 4)]
            public int blurRadius;
            [Range(0.0f, 2.0f)]
            public float erosionDarkening;
            [Range(1.0f, 20.0f)]
            public float noiseScale;
            public Gradient sedimentColor;
            [Range(0.0f, 2.0f)]
            public float sedimentOpacityModifier;
            public Color ironColor;
            [Range(0.0f, 2.0f)]
            public float ironOpacityModifier;
            [Range(50, 300)]
            public float ironGranularity;
            public Color efflorescenceColor;
            [Range(0.0f, 2.0f)]
            public float efflorescenceOpacityModifier;
            [Range(50, 300)]
            public float efflorescenceGranularity;
            public ColorationParameters(Gradient sedimentColor, Color ironColor, Color efflorescenceColor, int blurRadius = 2, float erosionDarkening = 0.4f, float noiseScale = 5.0f, float sedimentOpacityModifier = 2.0f, float ironOpacityModifier = 0.6f, float ironGranularity = 200, float efflorescenceOpacityModifier = 0.6f, float efflorescenceGranularity = 200) {
                this.blurRadius = blurRadius;
                this.erosionDarkening = erosionDarkening;
                this.noiseScale = noiseScale;
                if (sedimentColor == null) {
                    sedimentColor = new Gradient();
                    GradientColorKey[] keys = new GradientColorKey[] {
                        new GradientColorKey(new Color(0.5607843f, 0.5764706f, 0.5803922f), 0.0f),
                        new GradientColorKey(new Color(0.2745098f, 0.2941177f, 0.2901961f), 1.0f)
                    };
                    sedimentColor.colorKeys = keys;
                } else {
                    this.sedimentColor = sedimentColor;
                }
                this.sedimentOpacityModifier = sedimentOpacityModifier;
                if (ironColor == null) {
                    this.ironColor = new Color(1.0f, 0.5309945f, 0.0f);
                } else {
                    this.ironColor = ironColor;
                }
                this.ironOpacityModifier = ironOpacityModifier;
                this.ironGranularity = ironGranularity;
                if (efflorescenceColor == null) {
                    this.efflorescenceColor = Color.white;
                } else {
                    this.efflorescenceColor = efflorescenceColor;
                }
                this.efflorescenceOpacityModifier = efflorescenceOpacityModifier;
                this.efflorescenceGranularity = efflorescenceGranularity;
            }
        }

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
