using UnityEngine;

namespace WideAngleCamera;

public class StereographicCameraManager : MonoBehaviour {
	// Perspective as in where the player actually sees from
	private Camera perspective;
	private Camera front;
	private Camera back;
	private Camera left;
	private Camera right;
	private Camera down;
	private Camera up;

	private RenderTexture cubemap;
	
	public void Init(Renderer projector, bool useBack, int size, float fov) {
		perspective = Camera.main;
		front = transform.GetChild(0).GetComponent<Camera>();
		SetupCam(front, size);
		left = transform.GetChild(1).GetComponent<Camera>();
		SetupCam(left, size);
		right = transform.GetChild(2).GetComponent<Camera>();
		SetupCam(right, size);
		down = transform.GetChild(3).GetComponent<Camera>();
		SetupCam(down, size);
		up = transform.GetChild(4).GetComponent<Camera>();
		SetupCam(up, size);
		if (useBack) {
			var backObj = Instantiate(front.gameObject, transform, false);
			backObj.name = "Back";
			backObj.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
			back = backObj.GetComponent<Camera>();
			SetupCam(back, size);
		}

		cubemap = new RenderTexture(size, size, 16);
		cubemap.dimension = UnityEngine.Rendering.TextureDimension.Cube;

		// Assign properties to the shader
		projector.material.SetTexture("_Cube", cubemap);
		projector.material.SetFloat("_FOV", fov);
	}

	private void Update() {
		// Populate the cubemap with the newly rendered views
		Graphics.CopyTexture(front.targetTexture, 0, 0, cubemap, 4, 0);
		Graphics.CopyTexture(left.targetTexture, 0, 0, cubemap, 1, 0);
		Graphics.CopyTexture(right.targetTexture, 0, 0, cubemap, 0, 0);
		Graphics.CopyTexture(up.targetTexture, 0, 0, cubemap, 3, 0);
		Graphics.CopyTexture(down.targetTexture, 0, 0, cubemap, 2, 0);
		if (back != null) {
			Graphics.CopyTexture(back.targetTexture, 0, 0, cubemap, 5, 0);
		}
	}

	private void OnDestroy() {
		cubemap.Release();
		front.targetTexture.Release();
		left.targetTexture.Release();
		right.targetTexture.Release();
		down.targetTexture.Release();
		up.targetTexture.Release();
		if (back != null) back.targetTexture.Release();
	}

	private void SetupCam(Camera cam, int size) {
		var RT = new RenderTexture(size, size, 16);
		cam.targetTexture = RT;
		cam.cullingMask = perspective.cullingMask;
		cam.clearFlags = perspective.clearFlags;
		cam.depthTextureMode = DepthTextureMode.Depth;
		cam.depth = 1.0f;
	}
}
