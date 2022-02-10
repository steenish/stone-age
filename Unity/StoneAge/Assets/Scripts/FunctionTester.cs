using UnityEngine;

namespace Utility {
    public class FunctionTester : MonoBehaviour {

        public Texture2D testAlbedo;
        public Texture2D testErosion;
        public int testRadius;
        public int testYears;
        public int testMaxYears;

        public void PerformTest() {
			//string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures) + "/StoneAge/Blurtest/erosion.png";
			//float[,] blurredErosion = Textures.GaussianBlur(Conversion.CreateFloatBuffer(testErosion), testMaxRadius);
			//Textures.SaveTextureAsPNG(Conversion.CreateTexture(testErosion.width, testErosion.height, blurredErosion), path);

			//path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures) + "/StoneAge/Blurtest/color.png";
			//Color[,] blurredColor = Textures.GaussianBlur(Conversion.CreateColorBuffer(testAlbedo), testMaxRadius);
			//Textures.SaveTextureAsPNG(Conversion.CreateTexture(testAlbedo.width, testAlbedo.height, blurredColor), path);

			//string path = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures) + "/StoneAge/Blurtest/recolor5.png";
			//float[,] blurredErosion = Textures.GaussianBlur(Conversion.CreateFloatBuffer(testErosion), testRadius);
			//Color[,] recoloredAlbedo = Conversion.CreateColorBuffer(testAlbedo);
			//Textures.ColorErodedAreas(ref recoloredAlbedo, blurredErosion, testYears, testMaxYears);
			//Textures.SaveTextureAsPNG(Conversion.CreateTexture(testErosion.width, recoloredAlbedo), path);
		}
	}
}