using HarmonyLib;

namespace WideAngleCamera;

public static class UT_CameraTakeoverPatches {
	[HarmonyPatch(typeof(UT_CameraTakeover), "Start")]
	[HarmonyPostfix]
	public static void Postfix_Start(UT_CameraTakeover __instance) {
		var wideCam = StereographicCameraManager.Instance;
		// Don't need the smoothing on the deactivation event I think, the camera
		// already tries to handle this and I think- counterintuitively- doing it
		// here makes the transition less smooth even though it should neatly handle
		// getting back to configured fov
		__instance.activateEvent.AddListener(() => { wideCam.StartCoroutine(wideCam.LerpFOV(120.0f)); });
	}
}
