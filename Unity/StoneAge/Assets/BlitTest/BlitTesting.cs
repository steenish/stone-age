using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

namespace StoneAge {
    public class BlitTesting : MonoBehaviour {

        public Texture2D inputTexture1;
        public Texture2D inputTexture2;
		public Shader blitShader;

		public void TestBlit() {
            RenderTexture previous = RenderTexture.active;
            Material blitMaterial = new Material(blitShader);

            // Pass 1
            RenderTexture tempRT = RenderTexture.GetTemporary(inputTexture1.width, inputTexture1.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            Graphics.Blit(inputTexture1, tempRT, blitMaterial, 0);
            
            // Pass 2
            RenderTexture finalRT = RenderTexture.GetTemporary(inputTexture1.width, inputTexture1.height, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            blitMaterial.SetTexture("_DataTexture", inputTexture2);
            Graphics.Blit(tempRT, finalRT, blitMaterial, 1);
            Texture2D resultTexture = Textures.GetRTPixels(finalRT);

            // Save texture.
            string savePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures) + "/StoneAgeGPU/";
            System.IO.Directory.CreateDirectory(savePath);
            Textures.SaveTextureAsPNG(resultTexture, savePath + "Test.png");

            RenderTexture.ReleaseTemporary(tempRT);
            RenderTexture.ReleaseTemporary(finalRT);
            RenderTexture.active = previous;
        }
    }
}

