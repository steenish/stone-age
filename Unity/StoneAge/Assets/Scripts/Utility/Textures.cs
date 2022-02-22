using UnityEngine;
using UnityEngine.UI;

namespace Utility {
    public class Textures {

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

        public static Texture2D GetRTPixels(RenderTexture rt) {
            // Remember currently active render texture.
            RenderTexture currentActiveRT = RenderTexture.active;

            // Set the supplied RenderTexture as the active one.
            RenderTexture.active = rt;

            // Create a new Texture2D and read the RenderTexture image into it.
            Texture2D tex = new Texture2D(rt.width, rt.height);
            tex.ReadPixels(new Rect(0, 0, tex.width, tex.height), 0, 0);

            // Restore previously active render texture.
            RenderTexture.active = currentActiveRT;
            return tex;
        }

        public static float[,] PerlinNoise(int size, float scale) {
            float originX = Random.Range(-50000.0f, 50000.0f);
            float originY = Random.Range(-50000.0f, 50000.0f);

            float[,] result = new float[size, size];
            float step = scale / size;

            for (int y = 0; y < size; ++y) {
                for (int x = 0; x < size; ++x) {
                    result[x, y] = Mathf.PerlinNoise(originX + x * step, originY + y * step); 
                }
            }

            Height.NormalizeHeight(ref result);

            return result;
        }

        public static Texture2D PerlinNoiseTexture(int size) {
            float originX = Random.Range(-50000.0f, 50000.0f);
            float originY = Random.Range(-50000.0f, 50000.0f);

            Color32[] colors = new Color32[size * size];

            for (int y = 0; y < size; ++y) {
                for (int x = 0; x < size; ++x) {
                    byte perlinValue = System.Convert.ToByte(Mathf.Clamp01(Mathf.PerlinNoise(originX + x, originY + y)) * 255);
                    colors[x + y * size] = new Color32(perlinValue, perlinValue, perlinValue, 255);
                }
            }

            Texture2D result = new Texture2D(size, size);
            result.SetPixels32(colors);
            result.Apply();
            return result;
        }

        public static void SaveTextureAsPNG(Texture2D texture, string path) {
            byte[] bytes = texture.EncodeToPNG();
            System.IO.File.WriteAllBytes(path, bytes);
        }
    }
}
