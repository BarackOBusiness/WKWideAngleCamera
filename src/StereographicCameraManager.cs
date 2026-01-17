using System.IO;
using UnityEngine;

namespace HighFOV;

public class StereographicCameraManager : MonoBehaviour {
	internal Camera Forward;
	internal Camera Back;
	internal Camera Left;
	internal Camera Right;
	internal Camera Bottom;
	internal Camera Top;

	internal RenderTexture cubeRT;
	internal RenderTexture equirectRT;

	private void Awake() {
		// Setup the cubemap for copying into
		cubeRT = new RenderTexture(WideAnglePlugin.cubemapSize, WideAnglePlugin.cubemapSize, 16);
		cubeRT.dimension = UnityEngine.Rendering.TextureDimension.Cube;
		// Setup the equirect for previews
		equirectRT = new RenderTexture(cubeRT.width * 2, cubeRT.height, 16);
	
		Forward = BuildCamera("Forward", transform);
		Forward.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
		Back = BuildCamera("Back", transform);
		Back.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
		Left = BuildCamera("Left", transform);
		Left.transform.localRotation = Quaternion.Euler(0f, 270f, 0f);
		Right = BuildCamera("Right", transform);
		Right.transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
		Bottom = BuildCamera("Bottom", transform);
		Bottom.transform.localRotation = Quaternion.Euler(270f, 0f, 0f);
		Top = BuildCamera("Top", transform);
		Top.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

		// Camera stereoCam = gameObject.AddComponent<Camera>();
		// stereoCam.clearFlags = CameraClearFlags.SolidColor;
		// stereoCam.backgroundColor = Color.black;
		// stereoCam.orthographic = true;
		// stereoCam.orthographicSize = 0.5f;
		// stereoCam.cullingMask = LayerMask.GetMask()
	}

	private Camera BuildCamera(string name, Transform parent) {
		GameObject camObject = new GameObject(name);
		camObject.transform.SetParent(parent, false);
		Camera cam = camObject.AddComponent<Camera>();
		int resolution = WideAnglePlugin.cubemapSize;
		cam.targetTexture = new RenderTexture(resolution, resolution, 16);
		cam.backgroundColor = Color.black;
		cam.fieldOfView = 90.0f;
		cam.aspect = 1.0f;
		return cam;
	}

    public void ExportCubemap() {
        // First we have to render the cameras to the cubemap
        Graphics.CopyTexture(Forward.targetTexture, 0, 0, cubeRT, 4, 0);
        Graphics.CopyTexture(Back.targetTexture, 0, 0, cubeRT, 5, 0);
        Graphics.CopyTexture(Left.targetTexture, 0, 0, cubeRT, 1, 0);
        Graphics.CopyTexture(Right.targetTexture, 0, 0, cubeRT, 0, 0);
        Graphics.CopyTexture(Bottom.targetTexture, 0, 0, cubeRT, 3, 0);
        Graphics.CopyTexture(Top.targetTexture, 0, 0, cubeRT, 2, 0);

        RenderTexture previousMain = RenderTexture.active;

        cubeRT.ConvertToEquirect(equirectRT);
        RenderTexture.active = equirectRT;

        int width = equirectRT.width, height = equirectRT.height;
        Texture2D tex = new Texture2D(width, height, TextureFormat.RGB24, 0, true);
        tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        tex.Apply();

        byte[] imageData = tex.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/cubemap.png", imageData);

        Destroy(tex);
        RenderTexture.active = previousMain;
    }
}
