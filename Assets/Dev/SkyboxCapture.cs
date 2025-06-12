using UnityEngine;
using System.IO;

public class SkyboxCapture : MonoBehaviour
{
    public int resolution = 1024;
    public string outputFolder = "SkyboxCapture";

    private Camera captureCamera;

    void Start()
    {
        CaptureSkybox();
    }

    void CaptureSkybox()
    {
        // Create a temporary camera
        GameObject camObj = new GameObject("SkyboxCaptureCamera");
        captureCamera = camObj.AddComponent<Camera>();
        captureCamera.enabled = false;
        captureCamera.transform.position = transform.position;

        // Setup render texture and cubemap
        RenderTexture cubemapRT = new RenderTexture(resolution, resolution, 24);
        cubemapRT.dimension = UnityEngine.Rendering.TextureDimension.Cube;
        cubemapRT.Create();

        // Render to cubemap
        captureCamera.RenderToCubemap(cubemapRT);

        // Extract and save 6 faces
        SaveCubemapFaces(cubemapRT);

        // Clean up
        DestroyImmediate(camObj);
        cubemapRT.Release();
    }

    void SaveCubemapFaces(RenderTexture cubemapRT)
    {
        Directory.CreateDirectory(Path.Combine(Application.dataPath, outputFolder));

        CubemapFace[] faces = {
            CubemapFace.PositiveX,
            CubemapFace.NegativeX,
            CubemapFace.PositiveY,
            CubemapFace.NegativeY,
            CubemapFace.PositiveZ,
            CubemapFace.NegativeZ
        };

        string[] faceNames = {
            "Right(+X)", "Left(-X)", "Up(+Y)", "Down(-Y)", "Front(+Z)", "Back(-Z)"
        };

        for (int i = 0; i < faces.Length; i++)
        {
            Texture2D faceTex = new Texture2D(resolution, resolution, TextureFormat.RGBA32, false);
            RenderTexture.active = RenderTexture.GetTemporary(resolution, resolution);
            Graphics.SetRenderTarget(cubemapRT, 0, faces[i]);
            faceTex.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
            faceTex.Apply();

            byte[] bytes = faceTex.EncodeToPNG();
            string path = Application.dataPath + "/" + outputFolder + "/" + faceNames[i] + ".png";
            File.WriteAllBytes(path, bytes);

            Object.DestroyImmediate(faceTex);
            RenderTexture.active = null;
        }

        Debug.Log("Skybox images saved to: " + Path.Combine(Application.dataPath, outputFolder));
    }
}