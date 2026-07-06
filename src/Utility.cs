using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace WideAngleCamera;

public static class Math {
	public static float ExpDecay(float a, float b, float decay, float dt) {
		return b+(a-b)*Mathf.Exp(-decay*dt);
	}
}

public static class UT_CameraTakeoverPatches {
	[HarmonyPatch(typeof(UT_CameraTakeover), "Update")]
	[HarmonyPostfix]
	public static void Postfix_Update(UT_CameraTakeover __instance, ref bool ___active) {
		var wideCam = CameraManager.Instance;
		if (___active) {
			float targetFOV = Math.ExpDecay(wideCam.FOV, __instance.fov, __instance.speed, Time.deltaTime);
			Debug.Log($"Trying to set FOV, current {wideCam.FOV}, target {targetFOV}");
			wideCam.FOV = targetFOV;
		}
	}
}

public static class DEN_Hopper_TickPatches {
	[HarmonyPatch(typeof(DEN_Hopper_Tick), "Start")]
	[HarmonyPostfix]
	public static void Postfix_Start(DEN_Hopper_Tick __instance) {
		__instance.transform.GetComponentsInChildren<Transform>(true)
			.Where(t => t.gameObject.layer == 8 && t.name != "Effect_BloodSplatter")
			.Do(t => t.gameObject.layer = 28);
	}
}
