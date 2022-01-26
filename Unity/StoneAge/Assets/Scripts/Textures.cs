using UnityEngine;
using UnityEngine.UI;

namespace Utility {
	public class Textures {

		public static void DrawTexture(int width, int height, float[,] data, string textureName = "DebugTexture") {
			DrawTexture(Conversion.CreateTexture(width, height, data, textureName));
		}

		public static void DrawTexture(int width, int height, double[,] data, string textureName = "Debug Texture") {
			DrawTexture(Conversion.CreateTexture(width, height, data, textureName));
		}

		public static void DrawTexture(int width, int height, DoubleColor[,] data, string textureName = "Debug Texture") {
			DrawTexture(Conversion.CreateTexture(width, height, data, textureName));
		}

		public static void DrawTexture(Texture2D texture) {
			RawImage outputImage = GameObject.Find("DebugTextureImage").GetComponent<RawImage>();
			outputImage.texture = texture;
		}

		public static void SaveTextureAsPNG(Texture2D texture, string path) {
			byte[] bytes = texture.EncodeToPNG();
			System.IO.File.WriteAllBytes(path, bytes);
		}
	}
}
