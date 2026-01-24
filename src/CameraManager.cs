using UnityEngine;
using System.Reflection;
using System.Collections;

namespace WideAngleCamera;

public class CameraManager : MonoBehaviour {
	public static CameraManager Instance;

	private Camera front;
	private Camera back;
	private Camera right;
	private Camera left;
	private Camera up;
	private Camera down;

	private RenderTexture cubemap;
	private Material screen;

	// FOV animation parameters
	private float curFOV;
	private float sprintFOV;
	private float smoothedFOV;

	// Player state
	private ENT_Player player;
	private FieldInfo sliding;

	internal void Init(MeshRenderer projector, bool useBack, int size) {
		Instance = this;

		// Set FOV parameters
		curFOV = SettingsManager.settings.playerFOV;
		sprintFOV = curFOV + 15f;
		smoothedFOV = curFOV;

		// Cache player fields that are private for future access
		player = ENT_Player.GetPlayer();
		sliding = typeof(ENT_Player).GetField("isSliding", BindingFlags.Instance | BindingFlags.NonPublic);

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

		screen = projector.material;
		screen.mainTexture = cubemap;
		screen.SetFloat("_FOV", curFOV);
	}

	private void Update() {
		if (screen == null) return;
		Graphics.CopyTexture(front.targetTexture, 0, cubemap, 4);
		if (back != null) Graphics.CopyTexture(back.targetTexture, 0, cubemap, 5);
		Graphics.CopyTexture(right.targetTexture, 0, cubemap, 0);
		Graphics.CopyTexture(left.targetTexture, 0, cubemap, 1);
		Graphics.CopyTexture(up.targetTexture, 0, cubemap, 3);
		Graphics.CopyTexture(down.targetTexture, 0, cubemap, 2);

		if (!player.IsLocked()) {
			curFOV = Mathf.Clamp(curFOV + player.curBuffs.GetBuff("addFOV"), 60f, 315f);
			smoothedFOV = Math.ExpDecay(smoothedFOV, curFOV, 5f, Time.deltaTime);
			curFOV = SettingsManager.settings.playerFOV;
			sprintFOV = curFOV + 15f; // This is the only mechanism I see through which this can update realtime
			screen.SetFloat("_FOV", smoothedFOV);
		}
		if (!player.IsMoveLocked() && !CommandConsole.IsConsoleVisible()) {
			var isSliding = (bool)sliding.GetValue(player);
			if (player.IsSprinting() && player.IsGrounded() && !isSliding) {
				curFOV = sprintFOV;
			}
		}
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
