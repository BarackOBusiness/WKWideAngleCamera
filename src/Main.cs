using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WideAngleCamera;

[BepInPlugin("wk.barackobusiness.wideangle", "Wide Angle Camera", "1.0.0")]
public class WideAnglePlugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;

    private ConfigEntry<int> resolution;
    private ConfigEntry<bool> useBackCam;
    private ConfigEntry<float> fieldOfView;

    private Material stereoMat;
    private GameObject projector;
    private GameObject stereoCam;

    private void Awake() {
        Logger = base.Logger;

        resolution = Config.Bind(
            "General", "Resolution", 512,
            "The default side length of a face on the cubemap."
        );
        useBackCam = Config.Bind(
            "General", "Enable backface", false,
            "Whether to render behind the player or not, this option incurs additional performance cost and is only useful if using extreme fields of view at which distortion makes gameplay impractical."
        );
        fieldOfView = Config.Bind(
            "General", "Field of view", 135.0f,
            "The vertical field of view of the camera in degrees"
        );

        bool bundleStatus = LoadBundleAssets();

        if (bundleStatus) {
            SceneManager.sceneLoaded += OnSceneLoad;
            Logger.LogInfo("Wide angle views are now possible.");
        }
    }

    private void OnSceneLoad(Scene scene, LoadSceneMode mode) {
        if (scene.name != "Intro" && scene.name != "Main-Menu") {
            SetupScene();
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
            projectorScreen.GetComponent<MeshRenderer>(), useBackCam.Value, resolution.Value, fieldOfView.Value
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
