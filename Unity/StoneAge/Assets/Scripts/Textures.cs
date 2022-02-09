using UnityEngine;
using UnityEngine.UI;

namespace Utility {
    public class Textures {

        public static Color BlendColors(Color foreground, Color background, float foregroundAlpha) {
            return foreground * foregroundAlpha + background * (1 - foregroundAlpha);
        }

        public static void ColorErodedAreas(ref Color[,] color, float[,] erosion, int yearsAged, int maxAge = 1000) {
            int size = color.GetLength(0);

            float effect = Mathf.Clamp01((float) yearsAged / maxAge);

            for (int y = 0; y < size; ++y) {
                for (int x = 0; x < size; ++x) {
                    float erosionAmount = Mathf.Clamp01(erosion[x, y] * 10.0f * effect);
                    float hue, saturation, value;
                    Color.RGBToHSV(color[x, y], out hue, out saturation, out value);
                    saturation *= 1 - erosionAmount;
                    value *= Mathf.Lerp(0.4f, 1.0f, 1 - erosionAmount);
                    color[x, y] = Color.HSVToRGB(hue, saturation, value);
                }
            }
        }

        public static void DrawTexture(int size, float[,] data, string textureName = "DebugTexture") {
            DrawTexture(Conversion.CreateTexture(size, data, textureName));
        }

        public static void DrawTexture(Texture2D texture) {
            RawImage outputImage = GameObject.Find("DebugTextureImage").GetComponent<RawImage>();
            outputImage.texture = texture;
        }

        public static Color[,] GaussianBlur(Color[,] albedo, int radius) {
            int size = albedo.GetLength(0);

            float[] filter = GaussianKernel1D(radius, 0.33f * radius);

            // Horizontal blurring.
            for (int y = 0; y < size; ++y) {
                for (int x = 0; x < size; ++x) {
                    Color resultColor = Color.black;

                    for (int i = -radius; i < radius; ++i) {
                        resultColor += filter[i + radius] * albedo[Height.TileCoordinate(x + i, size), y];
                    }

                    resultColor.a = 1.0f;
                    albedo[x, y] = resultColor;
                }
            }

            // Vertical blurring.
            for (int y = 0; y < size; ++y) {
                for (int x = 0; x < size; ++x) {
                    Color resultColor = Color.black;

                    for (int i = -radius; i <= radius; ++i) {
                        resultColor += filter[i + radius] * albedo[x, Height.TileCoordinate(y + i, size)];
                    }

                    resultColor.a = 1.0f;
                    albedo[x, y] = resultColor;
                }
            }

            return albedo;
        }

        public static float[,] GaussianBlur(float[,] values, int radius) {
            int size = values.GetLength(0);

            float[] filter = GaussianKernel1D(radius, 0.33f * radius);

            // Horizontal blurring.
            for (int y = 0; y < size; ++y) {
                for (int x = 0; x < size; ++x) {
                    float result = 0.0f;

                    for (int i = -radius; i < radius; ++i) {
                        result += filter[i + radius] * values[Height.TileCoordinate(x + i, size), y];
                    }

                    values[x, y] = result;
                }
            }

            // Vertical blurring.
            for (int y = 0; y < size; ++y) {
                for (int x = 0; x < size; ++x) {
                    float result = 0.0f;

                    for (int i = -radius; i <= radius; ++i) {
                        result += filter[i + radius] * values[x, Height.TileCoordinate(y + i, size)];
                    }

                    values[x, y] = result;
                }
            }

            return values;
        }

        public static float[] GaussianKernel1D(int radius, float sigma) {
            float sqrt2PI = Mathf.Sqrt(2 * Mathf.PI);

            // Radius must be positive.
            radius = Mathf.Abs(radius);
            float[] kernel = new float[radius * 2 + 1];
            float norm = 1.0f / (sqrt2PI * sigma);
            float coefficient = 2 * sigma * sigma;
            float total = 0.0f;

            for (int x = -radius; x <= radius; ++x) {
                float value = norm * Mathf.Exp(-x * x / coefficient);
                kernel[radius + x] = value;
                total += value;
            }

            float inverseTotal = 1 / total;

            for (int i = 0; i < kernel.Length; ++i) {
                kernel[i] *= inverseTotal;
            }

            return kernel;
        }

        public static Color[,] OverlaySediment(Color[,] albedo, float[,] sediment, Gradient sedimentGradient, float sedimentOpacityModifier) {
            int size = sediment.GetLength(0);

            Color[,] result = new Color[size, size];

            for (int y = 0; y < size; ++y) {
                for (int x = 0; x < size; ++x) {
                    Color sedimentColor = sedimentGradient.Evaluate(Random.value);
                    result[x, y] = BlendColors(sedimentColor, albedo[x, y], Mathf.Clamp01(sediment[x, y] * sedimentOpacityModifier));
                }
            }

            return result;
        }

        public static void SaveTextureAsPNG(Texture2D texture, string path) {
            byte[] bytes = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);
        }
    }
}
