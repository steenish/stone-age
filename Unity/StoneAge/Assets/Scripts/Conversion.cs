using System;
using UnityEngine;

namespace Utility {
    public class Conversion {

        public static float[] CreateFloatBuffer(Texture2D map) {
            int size = map.width;
            int numPixels = size * size;

            Color[] colors = map.GetPixels();
            float[] values = new float[numPixels];

            for (int i = 0; i < numPixels; ++i) {
                values[i] = colors[i].r;
			}

            return values;
        }

        public static Color[,] Create2DColorBuffer(Texture2D map) {
            return Create2DBuffer(map, (Color pixelColor) => pixelColor);
        }

        public static float[,] Create2DFloatBuffer(Texture2D map) {
            return Create2DBuffer(map, (Color pixelColor) => pixelColor.r);
        }

        private static T[,] Create2DBuffer<T>(Texture2D map, Func<Color, T> valueExtractionFunction) {
            T[,] colors = new T[map.width, map.height];

            for (int y = 0; y < map.width; ++y) {
                for (int x = 0; x < map.height; ++x) {
                    colors[x, y] = valueExtractionFunction(map.GetPixel(x, y));
                }
            }

            return colors;
        }

        public static Texture2D CreateTexture(int size, Color[] data, string textureName = "Texture") {
            return CreateTexture(size, data, (Color color) => color, textureName);
        }

        public static Texture2D CreateTexture(int size, float[] data, string textureName = "Texture") {
            return CreateTexture(size, data, (float value) => new Color(value, value, value, 1.0f), textureName);
        }

        private static Texture2D CreateTexture<T>(int size, T[] data, Func<T, Color> colorExtractionFunction, string textureName = "Texture") {
            Texture2D texture = new Texture2D(size, size) {
                name = textureName
            };
            Color[] colors = new Color[size * size];

            for (int i = 0; i < size * size; ++i) {
                colors[i] = colorExtractionFunction(data[i]);
            }

            texture.SetPixels(colors);
            texture.Apply();

            return texture;
        }

        public static Texture2D CreateTexture(int size, Color[,] data, string textureName = "Texture") {
            return CreateTexture(size, data, (Color color) => color, textureName);
        }

        public static Texture2D CreateTexture(int size, float[,] data, string textureName = "Texture") {
            return CreateTexture(size, data, (float value) => new Color(value, value, value), textureName);
        }

        private static Texture2D CreateTexture<T>(int size, T[,] data, Func<T, Color> colorExtractionFunction, string textureName = "Texture") {
            Texture2D texture = new Texture2D(size, size) {
                name = textureName
            };
            Color[] colors = new Color[size * size];

            for (int y = 0; y < size; ++y) {
                for (int x = 0; x < size; ++x) {
                    colors[y * size + x] = colorExtractionFunction(data[x, y]);
                }
            }

            texture.SetPixels(colors);
            texture.Apply();

            return texture;
        }

        public static float[,] DifferenceMap(float[,] leftHandSide, float[,] rightHandSide) {
            return BinaryArithmeticMap(leftHandSide, rightHandSide, (float lhs, float rhs) => lhs - rhs);
        }

        public static float Sum(float[,] buffer) {
            int size = buffer.GetLength(0);

            float sum = 0.0f;

            for (int y = 0; y < size; ++y) {
                for (int x = 0; x < size; ++x) {
                    sum += buffer[x, y];
                }
            }

            return sum;
        }

        public static float[,] SumMap(float[,] leftHandSide, float[,] rightHandSide) {
            return BinaryArithmeticMap(leftHandSide, rightHandSide, (float lhs, float rhs) => lhs + rhs);
        }
        
        private static float[,] BinaryArithmeticMap(float[,] leftHandSide, float[,] rightHandSide, Func<float, float, float> binaryArithmeticFunction) {
            int size = leftHandSide.GetLength(0);

            float[,] result = new float[size, size];

            for (int y = 0; y < size; ++y) {
                for (int x = 0; x < size; ++x) {
                    result[x, y] = binaryArithmeticFunction(leftHandSide[x, y], rightHandSide[x, y]);
                }
            }

            return result;
        }
    }
}
