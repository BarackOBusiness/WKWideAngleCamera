using HarmonyLib;
using UnityEngine;

namespace WideAngleCamera;

public static class Math {
	public static float ExpDecay(float a, float b, float decay, float dt) {
		return b+(a-b)*Mathf.Exp(-decay*dt);
	}
}

public static class UT_CameraTakeoverPatches {
	[HarmonyPatch(typeof(UT_CameraTakeover), "Start")]
	[HarmonyPostfix]
	public static void Postfix_Start(UT_CameraTakeover __instance) {
		__instance.fov = 90.0f * Camera.main.aspect;
	}

	[HarmonyPatch(typeof(UT_CameraTakeover), "Update")]
	[HarmonyPostfix]
	public static void Postfix_Update(UT_CameraTakeover __instance, ref bool ___active) {
		var wideCam = CameraManager.Instance;
		if (___active) {
			wideCam.SetFOV(Math.ExpDecay(wideCam.GetFOV(), __instance.fov, __instance.speed, Time.deltaTime));
		}
	}
}
