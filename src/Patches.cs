using HarmonyLib;

namespace WideAngleCamera;

public static class UT_CameraTakeoverPatches {
	[HarmonyPatch(typeof(UT_CameraTakeover), "Start")]
	[HarmonyPostfix]
	public static void Postfix_Start(UT_CameraTakeover __instance) {
		var wideCam = StereographicCameraManager.Instance;
		__instance.activateEvent.AddListener(() => { wideCam.StartCoroutine(wideCam.LerpFOV(120.0f)); });
		__instance.deactivateEvent.AddListener(() => { wideCam.StartCoroutine(wideCam.LerpFOV(SettingsManager.settings.playerFOV)); });
	}
}
