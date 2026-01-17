using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HighFOV;

[BepInPlugin("wk.barackobusiness.wideangle", "High FOV Camera", "0.1.0")]
public class WideAnglePlugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    internal const int cubemapSize = 512;

    private void Awake() {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Wide angle views are now possible.");

        SceneManager.sceneLoaded += OnSceneLoad;
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode mode) {
        if (scene.name == "Game-Main") {
            Transform camParent = GameObject.Find("CL_Player/Main Cam Root/Main Camera Shake Root").transform;
            Transform camManager = new GameObject("Stereographic Camera").transform;
            camManager.SetParent(camParent, false);
            camManager.gameObject.AddComponent<StereographicCameraManager>();
        }
    }
}
