using UnityEngine;
using System.Reflection;
using System.Collections;

namespace WideAngleCamera;

public class CameraManager : MonoBehaviour {
	public static CameraManager Instance;
	public static CameraManager HandInstance;

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

	// Player state
	private ENT_Player player;
	private SettingsManager.GameSettings settings;
	private FieldInfo sliding;

	public float FOV {
		get { return screen.GetFloat("_FOV"); }
		internal set { screen.SetFloat("_FOV", value); }
	}

	internal void Init(Material projector, Camera orig, bool useBack, int size) {
		// Cache common data for less verbosity
		player = ENT_Player.GetPlayer();
		settings = SettingsManager.settings;

		// Set FOV parameters
		curFOV = settings.playerFOV;
		sprintFOV = curFOV + 15f;

		// isSliding is private, so cache the field info to access with reflection
		sliding = typeof(ENT_Player).GetField("isSliding", BindingFlags.Instance | BindingFlags.NonPublic);

		front = transform.Find("Front").GetComponent<Camera>();
		back = transform.Find("Back").GetComponent<Camera>();
		left = transform.Find("Left").GetComponent<Camera>();
		right = transform.Find("Right").GetComponent<Camera>();
		up = transform.Find("Up").GetComponent<Camera>();
		down = transform.Find("Down").GetComponent<Camera>();
		back.gameObject.SetActive(useBack);
		SetupCams(orig, size);

		cubemap = new RenderTexture(size, size, 16);
		cubemap.dimension = UnityEngine.Rendering.TextureDimension.Cube;

		screen = projector;
		screen.mainTexture = cubemap;
		FOV = curFOV;
	}

	private void Update() {
		if (screen == null) return;
		Graphics.CopyTexture(front.targetTexture, 0, cubemap, 4);
		if (back.gameObject.activeSelf) Graphics.CopyTexture(back.targetTexture, 0, cubemap, 5);
		Graphics.CopyTexture(right.targetTexture, 0, cubemap, 0);
		Graphics.CopyTexture(left.targetTexture, 0, cubemap, 1);
		Graphics.CopyTexture(up.targetTexture, 0, cubemap, 3);
		Graphics.CopyTexture(down.targetTexture, 0, cubemap, 2);

		if (!player.IsLocked()) {
			curFOV = Mathf.Clamp(curFOV + player.curBuffs.GetBuff("addFOV"), 60f, 315f);
			FOV = Math.ExpDecay(FOV, curFOV, 5f, Time.deltaTime);
			curFOV = settings.playerFOV;
			sprintFOV = curFOV + 15f; // This is the only mechanism I see through which this can update realtime
		}
		if (!player.IsMoveLocked() && !CommandConsole.IsConsoleVisible()) {
			var isSliding = (bool)sliding.GetValue(player);
			if (player.IsSprinting() && player.IsGrounded() && !isSliding && !settings.disableSprintFov) {
				curFOV = sprintFOV;
			}
		}
	}

	private void OnDestroy() {
		cubemap.Release();
	}

	private void SetupCams(Camera orig, int size) {
		foreach (var cam in GetSubCameras()) {
			cam.targetTexture = new RenderTexture(size, size, 16);
			cam.depth = orig.depth;
			cam.clearFlags = orig.clearFlags;
			cam.cullingMask = orig.cullingMask;
			cam.depthTextureMode = DepthTextureMode.Depth;
		}
	}

	internal Camera[] GetSubCameras() {
		return new Camera[]{ front, back, left, right, up, down };
	}
}
