using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using UnityEngine;

namespace WideAngleCamera;

public static class UT_CameraTakeoverPatches {
	[HarmonyPatch(typeof(UT_CameraTakeover), "Update")]
	[HarmonyPostfix]
	public static void Postfix_Update(UT_CameraTakeover __instance, ref bool ___active) {
		var wideCam = CameraManager.Instance;
		if (___active) {
			float targetFOV = Utility.ExpDecay(wideCam.FOV, __instance.fov, __instance.speed, Time.deltaTime);
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

	private static List<ILHook> _hooks;
	private static BepInEx.Logging.ManualLogSource _logger;

	public static void Hook(BepInEx.Logging.ManualLogSource logger) {
		_logger = logger;
		logger.LogInfo("Hooking `ENT_Player::SetCameraFov`");
		_hooks = new List<ILHook>(2);
		_hooks.Add(new ILHook(GetMethod<ENT_Player>("SetCameraFov", Private | Instance), SetCameraFov));
	}

	public static void Unhook() {
		foreach (var hook in _hooks) {
			hook.Dispose();
		}
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

	// Helps to factor this out to make reading the patch list easier once there
	// are multiple
	private static MethodInfo GetMethod<T>(string name, BindingFlags flags) {
		return typeof(T).GetMethod(name, flags);
	}
}
