using UnityEngine;
using UnityEngine.UI;

namespace Utility {
	public class Textures {

		public static Color BlendColors(Color foreground, Color background, float foregroundAlpha) {
			return foreground * foregroundAlpha + background * (1 - foregroundAlpha);
		}

		public static void DrawTexture(int width, int height, float[,] data, string textureName = "DebugTexture") {
			DrawTexture(Conversion.CreateTexture(width, height, data, textureName));
		}

		public static void DrawTexture(Texture2D texture) {
			RawImage outputImage = GameObject.Find("DebugTextureImage").GetComponent<RawImage>();
			outputImage.texture = texture;
		}

		public static Color[,] OverlaySediment(Color[,] albedo, float[,] sediment, Gradient sedimentGradient, float sedimentOpacityModifier) {
			int width = sediment.GetLength(1);
			int height = sediment.GetLength(0);

			Color[,] result = new Color[width, height];

			for (int y = 0; y < height; ++y) {
				for (int x = 0; x < width; ++x) {
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
