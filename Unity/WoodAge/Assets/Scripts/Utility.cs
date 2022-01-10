using System;
using UnityEngine;

public class Utility : MonoBehaviour {
    
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

    public static Texture2D CreateTexture(int width, int height, double[,] data, string textureName = "Texture") {
        return CreateTexture(width, height, data, (double value) => new Color((float) value, (float) value, (float) value), textureName);
    }

    public static Texture2D CreateTexture(int width, int height, DoubleColor[,] data, string textureName = "Texture") {
        return CreateTexture(width, height, data, (DoubleColor color) => new Color((float) color.r, (float) color.g, (float) color.b), textureName);
    }

    public static Texture2D CreateTexture(int width, int height, float[,] data, string textureName = "Texture") {
        return CreateTexture(width, height, data, (float value) => new Color(value, value, value), textureName);
    }

    private static Texture2D CreateTexture<T>(int width, int height, T[,] data, Func<T, Color> colorExtractionFunction, string textureName = "Texture") {
        Texture2D texture = new Texture2D(width, height);
        texture.name = textureName;
        Color[] colors = new Color[width * height];

        for (int y = 0; y < height; ++y) {
            for (int x = 0; x < width; ++x) {
                colors[y * width + x] = colorExtractionFunction(data[x, y]); ;
            }
        }

        texture.SetPixels(colors);
        texture.Apply();

        return texture;
    }

    public static double[,] HeightFromNormals(Texture2D normalMap) {
        // Code inspired by function make_heightmap from normalmap.c in the normalmap GIMP plugin (https://code.google.com/archive/p/gimp-normalmap/)
        int width = normalMap.width;
        int height = normalMap.height;

        Vector2[,] normals = new Vector2[width, height];
        double[,,] heights = new double[width, height, 4];
        double[,] resultHeights = new double[width, height];

        // Extract normal vectors from the normal map.
        for (int x = 0; x < width; ++x) {
            for (int y = 0; y < height; ++y) {
                Color normalColor = normalMap.GetPixel(x, y);
                // Uncompress DXT5nm-compressed normals. Map values into -1 to 1 range.
                normals[x, y] = (new Vector2(normalColor.a, normalColor.g) * 2.0f) - Vector2.one;
            }
        }

        // Essentially, the normals indicate slope, which is used to add (or subtract) to the height.
        // This has to be done in all directions, and saved separately.

        // Top-left to bottom-right.
        for (int x = 1; x < width; ++x) {
            heights[x, 0, 0] = heights[x - 1, 0, 0] + normals[x - 1, 0].x;
        }
        for (int y = 1; y < height; ++y) {
            heights[0, y, 0] = heights[0, y - 1, 0] + normals[0, y - 1].y;
        }
        for (int y = 1; y < height; ++y) {
            for (int x = 1; x < width; ++x) {
                heights[x, y, 0] = (heights[x, y - 1, 0] + heights[x - 1, y, 0] + normals[x - 1, y].x + normals[x, y - 1].y) * 0.5;
            }
        }

        // Top-right to bottom-left.
        for (int x = width - 2; x >= 0; --x) {
            heights[x, 0, 1] = heights[x + 1, 0, 1] - normals[x + 1, 0].x;
        }
        for (int y = 1; y < height; ++y) {
            heights[0, y, 1] = heights[0, y - 1, 1] + normals[0, y - 1].y;
        }
        for (int y = 1; y < height; ++y) {
            for (int x = width - 2; x >= 0; --x) {
                heights[x, y, 1] = (heights[x, y - 1, 1] + heights[x + 1, y, 1] - normals[x + 1, y].x + normals[x, y - 1].y) * 0.5;
            }
        }

        // Bottom-left to top-right.
        for (int x = 1; x < width; ++x) {
            heights[x, 0, 2] = heights[x - 1, 0, 2] + normals[x - 1, 0].x;
        }
        for (int y = height - 2; y >= 0; --y) {
            heights[0, y, 2] = heights[0, y + 1, 2] - normals[0, y + 1].y;
        }
        for (int y = height - 2; y >= 0; --y) {
            for (int x = 1; x < width; ++x) {
                heights[x, y, 2] = (heights[x, y + 1, 2] + heights[x - 1, y, 2] + normals[x - 1, y].x - normals[x, y + 1].y) * 0.5;
            }
        }

        // Bottom-right to top-left.
        for (int x = width - 2; x >= 0; --x) {
            heights[x, 0, 3] = heights[x + 1, 0, 3] - normals[x + 1, 0].x;
        }
        for (int y = height - 2; y >= 0; --y) {
            heights[0, y, 3] = heights[0, y + 1, 3] - normals[0, y + 1].y;
        }
        for (int y = height - 2; y >= 0; --y) {
            for (int x = width - 2; x >= 0; --x) {
                heights[x, y, 3] = (heights[x, y + 1, 3] + heights[x + 1, y, 3] - normals[x + 1, y].x - normals[x, y + 1].y) * 0.5;
            }
        }

        // Accumulate the separate results, find min and max height.
        double minHeight = double.MaxValue;
        double maxHeight = double.MinValue;
        for (int x = 0; x < width; ++x) {
            for (int y = 0; y < height; ++y) {
                resultHeights[x, y] = heights[x, y, 0] + heights[x, y, 1] + heights[x, y, 2] + heights[x, y, 3];
                if (resultHeights[x, y] < minHeight) minHeight = resultHeights[x, y];
                if (resultHeights[x, y] > maxHeight) maxHeight = resultHeights[x, y];
            }
        }

        // Scale into 0 - 1 range.
        double normalizingDenominator = 1 / (maxHeight - minHeight);
        for (int x = 0; x < width; ++x) {
            for (int y = 0; y < height; ++y) {
                resultHeights[x, y] = (resultHeights[x, y] - minHeight) * normalizingDenominator;
            }
        }

		return resultHeights;
	}

    public static Texture2D NormalsFromHeight(double[,] height) {
        // TODO
        throw new System.NotImplementedException();
    }
}
