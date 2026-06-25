using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

namespace WideAngleCamera;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
public class WideAnglePlugin : BaseUnityPlugin
{
    private ConfigEntry<Quality> quality;
    private ConfigEntry<Projection> projection;
    private ConfigEntry<bool> syncHands;
    private ConfigEntry<bool> renderBackface;
    private ConfigEntry<bool> allowExtreme;

    private GameObject wideAngleCamera;
    private Shader wideAngleShader;

    private Harmony patcher;

    enum Quality {
        VeryLow = 256,
        Low = 512,
        Normal = 1024,
        Extreme = 2048
    }

    public enum Projection {
        Stereographic,
        Equidistant,
        Equisolid,
        Panini
    }

    private void Awake() {
        quality = Config.Bind(
            "General", "Cubemap Resolution", Quality.Low,
            "The default side length of a face on the cubemap. This setting controls quality and performance."
        );

        projection = Config.Bind(
            "Projection Configuration", "Projection Technique", Projection.Stereographic,
            """
            The technique used to project the environment onto your screen.
            Stereographic projection draws a ray through each point on the sphere onto the projected plane. Just like standard rectilinear, only the projection point is at the north pole instead of the origin of the sphere.
            It is conformal, as in locally angles of intersections are preserved; circles remain circular at any given position on screen. This is the recommended and likely most comfortable projection to play with.
            Equidistant projection maps points on the viewing sphere to the projected plane such that distance from the center corresponds directly to angle measure between that point and the center on the globe.
            Equisolid projection maps the viewing sphere to a disk such that area on the disk accurately represents area at all regions of the sphere.
            Panini projects from a cylinder onto your view. The straightness of vertical lines is retained from rectilinear projection while allowing for a much wider field of view.
            """
        );

        syncHands = Config.Bind(
            "General", "Synchronize Hands", true,
            """
            Whether to synchronize hands or not. When synchronized, a second wide angle projection camera is created with the sole purpose to render hands in the world, which incurs a not insignificant rendering cost.
            However hand sprites will be rendered appropriately in the position where you have grabbed and should not diverge from there.
            """
        );

        renderBackface = Config.Bind(
            "General", "Enable backface", false,
            "Whether to render behind the player or not. Incurs additional performance cost but resolves black borders on the edges of the screen at high fields of view. Perhaps necessary for normal fovs on ultrawide screens."
        );

        allowExtreme = Config.Bind(
            "General", "Allow Extreme Fields of View", false,
            "Whether the mod should allow you to select a field of view value close to the theoretical limits of the projection or set a reasonable upper bound."
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
        Func<Projection, float> boundingFunction = allowExtreme.Value? HardBound : SoftBound;
        // These here disgusting behemoths set the fov slider range
        if (scene.name == "Main-Menu") {
            Transform canvas = GameObject.Find("Canvas - Screens").transform;
            var slider = canvas.Find("Screens/Canvas - Screen - Settings/Settings Menu/SettingsParent/Settings Pane/Video Settings/Main Panel/Tab - Video/Column - Video/SliderAsset - FOV/Slider");
            slider.GetComponent<DarkMachine.UI.SubmitSlider>().maxValue = boundingFunction(projection.Value);
        } else {
            Transform pause = GameObject.Find("Pause").transform;
            var slider = pause.Find("Pause Menu/Settings Menu/SettingsParent/Settings Pane/Video Settings/Main Panel/Tab - Video/Column - Video/SliderAsset - FOV/Slider");
            slider.GetComponent<DarkMachine.UI.SubmitSlider>().maxValue = boundingFunction(projection.Value);

            // Setup the camera
            Transform camParent = Camera.main.transform;
            // Setup screen
            GameObject screen = SetupProjector("Geometry Screen");
            screen.GetComponent<MeshRenderer>().material = new Material(wideAngleShader);
            screen.GetComponent<MeshRenderer>().material.renderQueue = (int)RenderQueue.Geometry;
            screen.transform.localPosition = new Vector3(0f, 0f, 0.5f);
            screen.transform.SetParent(camParent, false);
            screen.layer = 31;
            // Setup camera
            GameObject cam = GameObject.Instantiate(wideAngleCamera, camParent, false);
            cam.name = "Wide Angle Camera";
            CameraManager cMan = cam.AddComponent<CameraManager>();
            cMan.Init(screen.GetComponent<MeshRenderer>().material, Camera.main, renderBackface.Value, (int)quality.Value, projection.Value, boundingFunction);
            CameraManager.Instance = cMan;
            // Now finishing touches
            Camera.main.nearClipPlane = 0.0f;
            Camera.main.farClipPlane = 1.0f;
            Camera.main.cullingMask = 1 << 31;
            Camera.main.orthographic = true;
            Camera.main.orthographicSize = 0.75f;
            Camera.main.useOcclusionCulling = false;
            Camera.main.clearFlags = CameraClearFlags.SolidColor;

            if (!syncHands.Value) return;
            // Inventory camera, this is easily the most wasteful thing I think I've ever attempted
            // but since the inventory will mostly be transparency I hope it's not that big an impact
            // Start by setting up the screen, maybe we'll just have it overlay the main projection?
            GameObject handScreen = SetupProjector("Hand Screen");
            handScreen.GetComponent<MeshRenderer>().material = new Material(wideAngleShader);
            handScreen.transform.localPosition = new Vector3(0f, 0f, 0.25f);
            handScreen.transform.SetParent(camParent, false);
            handScreen.layer = 31;
            Camera invCam = camParent.Find("Inventory Camera").GetComponent<Camera>();
            GameObject handCam = GameObject.Instantiate(wideAngleCamera, camParent, false);
            handCam.name = "Wide Angle Hand Camera";
            // The legendary hand man, he's here...
            CameraManager handMan = handCam.AddComponent<CameraManager>();
            handMan.Init(handScreen.GetComponent<MeshRenderer>().material, invCam, renderBackface.Value, (int)quality.Value, projection.Value, boundingFunction);
            CameraManager.HandInstance = handMan;
            var handMen = handMan.GetSubCameras();
            // This joke has gone way too far but I don't really care enough to make the values actually descript
            foreach (var man in handMen) {
                man.clearFlags = CameraClearFlags.SolidColor;
                man.cullingMask = 1 << 28;
            }
            // Finally make it so that hands show up on the correct layer
            handCam.AddComponent<HelpingHand>();
        }
    }

    // A reasonable upper bound for a given projection such that the game remains legible
    public float SoftBound(Projection projection) {
        Func<float, float> Sin = Mathf.Sin;
        Func<float, float> Asin = Mathf.Asin;
        Func<float, float> Tan = Mathf.Tan;
        Func<float, float> Atan = Mathf.Atan;

        float deg = Mathf.Deg2Rad;
        float diag = Mathf.Sqrt(1 + Camera.main.aspect*Camera.main.aspect);

        // We use the diagonal here instead of the aspect ratio so that
        // the corner of the screen is at the bound instead of the horizontal edges
        float fov = projection switch {
            Projection.Stereographic => 4f*Atan(Tan(315f*deg*0.25f) / diag) / deg,
            Projection.Equidistant => 360f/diag,
            Projection.Equisolid => 4f*Asin(Sin(360f*deg*0.25f) / diag) / deg,
            Projection.Panini => 170.0f, // The pains of using vertical fov in panini
            _ => 350f // Shouldn't be possible
        };

        return Mathf.Floor(fov);
    }

    // Absolute upper bound of a given projection (pretty much)
    public float HardBound(Projection projection) {
        return projection switch {
            Projection.Stereographic => 350.0f,
            Projection.Equidistant   => 360.0f,
            Projection.Equisolid     => 360.0f, // Yeah sure
            Projection.Panini        => 170.0f,
            _ => 350.0f // Shouldn't be possible
        };
    }

    // Constructs a GameObject with a fullscreen triangle mesh without any material
    private GameObject SetupProjector(string name) {
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
        var obj = new GameObject(name);
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
