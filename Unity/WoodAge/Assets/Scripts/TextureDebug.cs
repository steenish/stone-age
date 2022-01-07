using System;
using UnityEngine;
using UnityEngine.UI;

public class TextureDebug {
	
	public static void DrawTexture(int width, int height, float[,] data, string textureName = "DebugTexture") {
		DrawTexture(Utility.CreateTexture(width, height, data, textureName));
	}

	public static void DrawTexture(int width, int height, double[,] data, string textureName = "Debug Texture") {
		DrawTexture(Utility.CreateTexture(width, height, data, textureName));
	}
	public static void DrawTexture(int width, int height, DoubleColor[,] data, string textureName = "Debug Texture") {
		DrawTexture(Utility.CreateTexture(width, height, data, textureName));
	}

	private static void DrawTexture(Texture2D texture) {
		RawImage outputImage = GameObject.Find("DebugTextureImage").GetComponent<RawImage>();
		outputImage.texture = texture;
	}
}
