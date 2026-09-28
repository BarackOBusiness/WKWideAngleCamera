using System.Linq;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
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

public static class Hooks {
	private static BindingFlags Private = BindingFlags.NonPublic;
	private static BindingFlags Instance = BindingFlags.Instance;

	private static ILHook _hook;
	private static BepInEx.Logging.ManualLogSource _logger;

	public static void Hook(BepInEx.Logging.ManualLogSource logger) {
		_logger = logger;
		logger.LogInfo("Hooking this shit rn fr fr");
		_hook = new ILHook(typeof(ENT_Player).GetMethod("SetCameraFov", Private | Instance), SetCameraFov);
	}

	public static void Unhook() {
		_hook?.Dispose();
	}

	private static void SetCameraFov(ILContext il) {
		var cursor = new ILCursor(il);

		if (cursor.TryGotoNext(MoveType.After,
			x => x.MatchLdsfld<SettingsManager>("settings"),
			x => x.MatchLdfld<SettingsManager.GameSettings>("playerFOV")
		)) {
			_logger.LogInfo("Matched IL at SetCameraFov");
			cursor.EmitDelegate((float fov) => {
				return Mathf.Clamp(fov, 60f, 140f);
			});
		} else {
			_logger.LogInfo("Failed to hook SetCameraFov");
		}
	}
}
