using System;
using UnityEngine;

namespace Utility {
    public class Conversion {

        public static Color[,] CreateColorBuffer(Texture2D map) {
            return CreateBuffer(map, (Color pixelColor) => pixelColor);
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

        public static Texture2D CreateTexture(int width, int height, float[,] data, string textureName = "Texture") {
            return CreateTexture(width, height, data, (float value) => new Color(value, value, value), textureName);
        }

        private static Texture2D CreateTexture<T>(int width, int height, T[,] data, Func<T, Color> colorExtractionFunction, string textureName = "Texture") {
			Texture2D texture = new Texture2D(width, height) {
				name = textureName
			};
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

        public static T[,] ExtractBufferLayer<T>(T[,,] layers, int layerIndex) {
            int width = layers.GetLength(1);
            int height = layers.GetLength(0);

            T[,] extractedLayer = new T[width, height];

            for (int y = 0; y < height; ++y) {
                for (int x = 0; x < width; ++x) {
                    extractedLayer[x, y] = layers[x, y, layerIndex];
				}
			}

            return extractedLayer;
        }

        public static void FillFloatBufferLayer(Texture2D map, ref float[,,] layers, int layerIndex) {
            FillBufferLayer(map, ref layers, layerIndex, (Color pixelColor) => pixelColor.r);
        }

        private static void FillBufferLayer<T>(Texture2D map, ref T[,,] layers, int layerIndex, Func<Color, T> valueExtractionFunction) {
            int width = map.width;
            int height = map.height;
            Color[] colors = map.GetPixels();

            for (int y = 0; y < width; ++y) {
                for (int x = 0; x < height; ++x) {
                    layers[x, y, layerIndex] = valueExtractionFunction(colors[x + y * width]);
                }
            }
        }
    }
}
