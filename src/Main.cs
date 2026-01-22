using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WideAngleCamera;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class WideAnglePlugin : BaseUnityPlugin
{
    private ConfigEntry<int> resolution;
    private ConfigEntry<bool> useBackCam;

    private Material stereoMat;
    private GameObject projector;
    private GameObject stereoCam;

    private void Awake() {
        resolution = Config.Bind(
            "General", "Resolution", 512,
            "The default side length of a face on the cubemap."
        );
        useBackCam = Config.Bind(
            "General", "Enable backface", false,
            "Whether to render behind the player or not, this option incurs additional performance cost and is only useful if using extreme fields of view at which distortion makes gameplay impractical."
        );

        if (LoadBundleAssets()) {
            SceneManager.sceneLoaded += OnSceneLoad;
            Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll(typeof(UT_CameraTakeoverPatches));
            Logger.LogInfo("Wide angle views are now possible.");
        }
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode mode) {
        if (scene.name != "Intro" && scene.name != "Main-Menu") {
            SetupScene();
        }
        // Disgusting vile behemoth just to expand the slider range, don't even worry about it
        if (scene.name == "Main-Menu") {
            GameObject slider = GameObject.Find("Canvas - Screens/Screens/Canvas - Screen - Settings/Settings Menu/SettingsParent/Settings Pane/Video Settings/Options Tab/Video/SliderAsset - FOV/Slider");
            slider.GetComponent<DarkMachine.UI.SubmitSlider>().maxValue = 270f;
        } else if (scene.name != "Intro") {
            Transform pause = GameObject.Find("Pause").transform;
            var slider = pause.GetChild(0).GetChild(3).GetChild(0).GetChild(0).GetChild(4).GetChild(1).GetChild(0).GetChild(7).GetChild(1);
            slider.GetComponent<DarkMachine.UI.SubmitSlider>().maxValue = 270f;
        }
    }

    private void SetupScene() {
        Transform camParent = Camera.main.transform;
        GameObject camManager = Instantiate(stereoCam, camParent);
        GameObject projectorScreen = Instantiate(projector, camParent);
        projectorScreen.layer = 31;
        projectorScreen.transform.localPosition = new Vector3(0f, 0f, 0.5f);
        projectorScreen.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
        projectorScreen.transform.localScale = new Vector3(Camera.main.aspect, 1f, 1f);
        camManager.AddComponent<StereographicCameraManager>().Init(
            projectorScreen.GetComponent<MeshRenderer>(), useBackCam.Value, resolution.Value
        );
        Camera.main.orthographic = true;
        Camera.main.orthographicSize = 0.5f;
        Camera.main.cullingMask = 1 << 31;
    }

    private bool LoadBundleAssets() {
        var dllDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var path = Path.Combine(dllDir, "stereoassets");
        var bundle = AssetBundle.LoadFromFile(path);

        if (bundle == null) {
            Logger.LogError("Wide angle views are NOT possible, please check if the asset bundle is in the same directory as the plugin.");
            return false;
        }

        stereoMat = bundle.LoadAsset<Material>("Screen");
        projector = bundle.LoadAsset<GameObject>("Projector");
        stereoCam = bundle.LoadAsset<GameObject>("Stereographic Camera");

        if (stereoMat == null || projector == null || stereoCam == null) {
            Logger.LogError("Wide angle views are NOT possible, please check the checksum of the asset bundle in the plugin directory, if it does not match X, reacquire this bundle");
            return false;
        }

        DontDestroyOnLoad(stereoMat);
        DontDestroyOnLoad(projector);
        DontDestroyOnLoad(stereoCam);

        return true;
    }
}
