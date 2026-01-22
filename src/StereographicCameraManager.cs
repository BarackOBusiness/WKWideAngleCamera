using UnityEngine;
using System.Collections;
using System.Reflection;

namespace WideAngleCamera;

public class StereographicCameraManager : MonoBehaviour {
	public static StereographicCameraManager Instance;

	// Perspective as in where the player actually sees from
	private Camera perspective;
	private Camera front;
	private Camera back;
	private Camera left;
	private Camera right;
	private Camera down;
	private Camera up;

	private RenderTexture cubemap;

	private Material screen;

	private ENT_Player player;
	private float curFOV = 0.0f;
	private float sprintFOV = 0.0f;
	private float smoothedFOV = 0.0f;
	// TODO: make this actually get changed from 0 by ShockFOV() or its callers
	private float slowFOVAdjust = 0.0f;

	private FieldInfo sprinting;
	private FieldInfo isGrounded;
	private FieldInfo isSliding;

	bool pauseFOVAdjust = false;

	public void Init(Renderer projector, bool useBack, int size) {
		// Make a public reference to this instance for the takeover patch
		Instance = this;
		// Initialize reference to the main camera, player, and get FOV setting
		perspective = Camera.main;
		player = ENT_Player.GetPlayer();
		curFOV = SettingsManager.settings.playerFOV;
		sprintFOV = curFOV + 15f;
		// Next let's cache the fields we want to be able to access in the future that are private
		BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
		sprinting = typeof(ENT_Player).GetField("sprinting", flags);
		isGrounded = typeof(ENT_Player).GetField("isGrounded", flags);
		isSliding = typeof(ENT_Player).GetField("isSliding", flags);
		// Now setup reference to our camera setup
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
		// If it has been configured to render the back face, duplicate the front and turn it around
		if (useBack) {
			var backObj = Instantiate(front.gameObject, transform, false);
			backObj.name = "Back";
			backObj.transform.Rotate(new Vector3(0f, 180f, 0f), Space.Self);
			back = backObj.GetComponent<Camera>();
			SetupCam(back, size);
		}
		// Initialize the cubemap
		cubemap = new RenderTexture(size, size, 16);
		cubemap.dimension = UnityEngine.Rendering.TextureDimension.Cube;
		// Assign screen properties
		screen = projector.material;
		screen.SetTexture("_Cube", cubemap);
		screen.SetFloat("_FOV", curFOV);
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

		// Live FOV reaction
		if (!pauseFOVAdjust && !player.IsLocked()) {
			curFOV = Mathf.Clamp(curFOV + player.curBuffs.GetBuff("addFOV"), 60f, 315f);
			smoothedFOV = expDecay(smoothedFOV, curFOV + slowFOVAdjust, 16f, Time.deltaTime);
			slowFOVAdjust = expDecay(slowFOVAdjust, 0f, 8f, Time.deltaTime);
			curFOV = SettingsManager.settings.playerFOV;
			sprintFOV = curFOV + 15f;
			if (!SettingsManager.settings.disableSprintFov) {
				if (!player.IsMoveLocked() && !CommandConsole.IsConsoleVisible()) {
					bool isSprinting = (bool)sprinting.GetValue(player);
					bool grounded = (bool)isGrounded.GetValue(player);
					bool sliding = (bool)isSliding.GetValue(player);
					if (isSprinting && grounded && !sliding) curFOV = sprintFOV;
				}
			}
			screen.SetFloat("_FOV", smoothedFOV);
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

	public IEnumerator LerpFOV(float target) {
		float timer = 0f;
		pauseFOVAdjust = true;
		while (timer < 1f) {
			timer += Time.deltaTime * 5f;
			SetFOV(expDecay(curFOV, target, 5f, timer));
			yield return new WaitForEndOfFrame();
		}
		pauseFOVAdjust = false;
		curFOV = target;
		yield break;
	}

	// Really wanted an excuse to use exponential decay smoothing instead of lerp
	private static float expDecay(float a, float b, float decay, float dt) {
		return b+(a-b)*Mathf.Exp(-decay*dt);
	}
}
