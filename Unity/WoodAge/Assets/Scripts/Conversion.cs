using System;
using UnityEngine;

namespace Utility {
    public class Conversion {

        public static double[,] CreateDoubleBuffer(Texture2D map) {
            return CreateBuffer(map, (Color pixelColor) => (double) pixelColor.r);
        }

        public static DoubleColor[,] CreateColorBuffer(Texture2D map) {
            return CreateBuffer(map, (Color pixelColor) => new DoubleColor(pixelColor.r, pixelColor.g, pixelColor.b));
        }

        public static float[,] CreateFloatBuffer(Texture2D map) {
            return CreateBuffer(map, (Color pixelColor) => pixelColor.r);
        }

        private static T[,] CreateBuffer<T>(Texture2D map, Func<Color, T> valueExtractionFunction) {
            T[,] colors = new T[map.width, map.height];

            for (int y = 0; y < map.width; ++y) {
                for (int x = 0; x < map.height; ++x) {
                    colors[x, y] = valueExtractionFunction(map.GetPixel(x, y));
                }
            }

            return colors;
        }

        public static Texture2D CreateTexture(int width, int height, Color[,] data, string textureName = "Texture") {
            return CreateTexture(width, height, data, (Color color) => color, textureName);
        }

        public static Texture2D CreateTexture(int width, int height, double[,] data, string textureName = "Texture") {
            return CreateTexture(width, height, data, (double value) => new Color((float) value, (float) value, (float) value), textureName);
        }

        public static Texture2D CreateTexture(int width, int height, DoubleColor[,] data, string textureName = "Texture") {
            return CreateTexture(width, height, data, (DoubleColor color) => new Color((float) color.r, (float) color.g, (float) color.b), textureName);
        }

        public static Texture2D CreateTexture(int width, int height, float[,] data, string textureName = "Texture") {
            return CreateTexture(width, height, data, (float value) => new Color(value, value, value), textureName);
        }

        public static Texture2D CreateTexture(int width, int height, Vector3[,] data, string textureName = "Texture") {
            return CreateTexture(width, height, data, (Vector3 value) => new Color(value.x * 0.5f + 0.5f, value.y * 0.5f + 0.5f, value.z * 0.5f + 0.5f), textureName);
        }

        private static Texture2D CreateTexture<T>(int width, int height, T[,] data, Func<T, Color> colorExtractionFunction, string textureName = "Texture") {
            Texture2D texture = new Texture2D(width, height);
            texture.name = textureName;
            Color[] colors = new Color[width * height];

            for (int y = 0; y < height; ++y) {
                for (int x = 0; x < width; ++x) {
                    colors[y * width + x] = colorExtractionFunction(data[x, y]);
                }
            }

            texture.SetPixels(colors);
            texture.Apply();

            return texture;
        }
    }
}
