using UnityEngine;

namespace WideAngleCamera;

public class CameraManager : MonoBehaviour {
	public static CameraManager Instance;
	public static bool Ready = false;

	private Camera front;
	private Camera back;
	private Camera right;
	private Camera left;
	private Camera up;
	private Camera down;

	private RenderTexture cubemap;
	public RenderTexture equirect;
	private ENT_Player player;

	internal void Init(MeshRenderer screen, bool useBack, int size) {
		Instance = this;

		front = transform.GetChild(0).GetComponent<Camera>();
		SetupCam(front, Camera.main, size);
		left = transform.GetChild(1).GetComponent<Camera>();
		SetupCam(left, Camera.main, size);
		right = transform.GetChild(2).GetComponent<Camera>();
		SetupCam(right, Camera.main, size);
		down = transform.GetChild(3).GetComponent<Camera>();
		SetupCam(down, Camera.main, size);
		up = transform.GetChild(4).GetComponent<Camera>();
		SetupCam(up, Camera.main, size);
		if (useBack) {
			var backObj = transform.GetChild(5).gameObject;
			backObj.SetActive(true);
			back = backObj.GetComponent<Camera>();
			SetupCam(back, Camera.main, size);
		}

		cubemap = new RenderTexture(size, size, 16);
		cubemap.dimension = UnityEngine.Rendering.TextureDimension.Cube;

		screen.material.mainTexture = cubemap;
		Ready = true;
	}

	private void Update() {
		if (!Ready) return;
		Graphics.CopyTexture(front.targetTexture, 0, cubemap, 4);
		if (back != null) Graphics.CopyTexture(back.targetTexture, 0, cubemap, 5);
		Graphics.CopyTexture(right.targetTexture, 0, cubemap, 0);
		Graphics.CopyTexture(left.targetTexture, 0, cubemap, 1);
		Graphics.CopyTexture(up.targetTexture, 0, cubemap, 3);
		Graphics.CopyTexture(down.targetTexture, 0, cubemap, 2);
	}

	private void OnDestroy() {
		cubemap.Release();
	}

	private void SetupCam(Camera cam, Camera orig, int size) {
		RenderTexture rt = new RenderTexture(size, size, 16);
		cam.targetTexture = rt;
		cam.depth = orig.depth;
		cam.clearFlags = orig.clearFlags;
		cam.cullingMask = orig.cullingMask;
		cam.depthTextureMode = DepthTextureMode.Depth;
	}
}
