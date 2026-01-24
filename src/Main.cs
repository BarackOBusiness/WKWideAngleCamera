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
    private ConfigEntry<Quality> quality;
    private ConfigEntry<bool> renderBackface;
    private ConfigEntry<Projection> projection;

    private GameObject wideAngleCamera;
    private Shader wideAngleShader;

    private Harmony patcher;

    enum Quality {
        VeryLow = 256,
        Low = 512,
        Normal = 1024,
        Extreme = 2048
    }

    enum Projection {
        Stereographic,
        Panini
    }

    private void Awake() {
        quality = Config.Bind(
            "General", "Cubemap Resolution", Quality.Low,
            "The default side length of a face on the cubemap. This setting controls quality and performance."
        );

        renderBackface = Config.Bind(
            "General", "Enable backface", false,
            "Whether to render behind the player or not. Incurs additional performance cost and is only useful for extreme fields of view which make gameplay impractical."
        );

        projection = Config.Bind(
            "Projection Configuration", "Projection Technique", Projection.Stereographic,
            "The technique used to project the environment onto your screen. Stereographic projects from a sphere onto your view. Panini projects from a cylinder onto your view."
        );

        if (LoadAssetBundle()) {
            SceneManager.sceneLoaded += OnSceneLoaded;
            patcher = new Harmony(MyPluginInfo.PLUGIN_GUID);
            patcher.PatchAll(typeof(UT_CameraTakeoverPatches));
            Logger.LogInfo("Wide angle views are NOW possible");
        } // Abort the rest of setup if the asset bundle could not successfully load
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        if (scene.name == "Intro") return;
        // These here disgusting behemoths set the fov slider range
        if (scene.name == "Main-Menu") {
            GameObject slider = GameObject.Find("Canvas - Screens/Screens/Canvas - Screen - Settings/Settings Menu/SettingsParent/Settings Pane/Video Settings/Options Tab/Video/SliderAsset - FOV/Slider");
            slider.GetComponent<DarkMachine.UI.SubmitSlider>().maxValue = 270f;
        } else {
            Transform pause = GameObject.Find("Pause").transform;
            var slider = pause.GetChild(0).GetChild(3).GetChild(0).GetChild(0).GetChild(4).GetChild(1).GetChild(0).GetChild(7).GetChild(1);
            slider.GetComponent<DarkMachine.UI.SubmitSlider>().maxValue = 270f;

            // Setup the camera
            Transform camParent = Camera.main.transform;
            // Setup screen
            GameObject screen = SetupProjector();
            screen.GetComponent<MeshRenderer>().material = new Material(wideAngleShader);
            screen.transform.localPosition = new Vector3(0f, 0f, 0.5f);
            screen.transform.SetParent(camParent, false);
            screen.layer = 31;
            // Setup camera
            GameObject cam = GameObject.Instantiate(wideAngleCamera, camParent, false);
            cam.name = "Wide Angle Camera";
            CameraManager cMan = cam.AddComponent<CameraManager>();
            cMan.Init(screen.GetComponent<MeshRenderer>(), renderBackface.Value, (int)quality.Value);
            // Now finishing touches
            Camera.main.nearClipPlane = 0.0f;
            Camera.main.farClipPlane = 1.0f;
            Camera.main.cullingMask = 1 << 31;
            Camera.main.orthographic = true;
            Camera.main.orthographicSize = 0.75f;
            Camera.main.useOcclusionCulling = false;
            Camera.main.clearFlags = CameraClearFlags.Nothing; // This fixes the ZWrite problem
        }
    }

    // Constructs a GameObject with a fullscreen triangle mesh without any material
    private GameObject SetupProjector() {
        // Let us first generate the mesh
        Mesh m = new Mesh();
        m.name = "Triangle";

        m.vertices = new Vector3[]
        {
            new Vector3(-1, -1, 0),
            new Vector3( 3, -1, 0),
            new Vector3(-1,  3, 0),
        };

        m.uv = new Vector2[]
        {
            new Vector2(0, 0),
            new Vector2(2, 0),
            new Vector2(0, 2),
        };

        // Winds from the last vertex to the first because for some reason that faces it towards negative z
        m.triangles = new int[] { 2, 1, 0 };
        m.RecalculateBounds();

        // Now create the projector which will be returned
        var obj = new GameObject("Projector Screen");
        var mf = obj.AddComponent<MeshFilter>();
        var mr = obj.AddComponent<MeshRenderer>();

        mr.receiveShadows = false;
        mr.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
        mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        mr.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
        mr.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

        mf.sharedMesh = m;

        return obj;
    }

    private bool LoadAssetBundle() {
        string dir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        var bundle = AssetBundle.LoadFromFile($"{dir}\\WideAngleAssets");
        if (bundle == null) {
            Logger.LogError("Wide angle views are NOT possible, please ensure the asset bundle is present in the same folder as the plugin");
            return false;
        }

        wideAngleCamera = bundle.LoadAsset<GameObject>("Wide Angle Camera");
        foreach (var shader in bundle.LoadAllAssets<Shader>()) {
            if (shader.name == $"Custom/{projection.Value.ToString()}")
                wideAngleShader = shader;
        }
        if (wideAngleCamera == null || wideAngleShader == null) {
            Logger.LogError("Wide angle views are NOT possible, please reacquire the asset bundle from https://github.com/BarackOBusiness/WKWideAngleCamera");
        }

        DontDestroyOnLoad(wideAngleCamera);
        DontDestroyOnLoad(wideAngleShader);

        return true;
    }
}
