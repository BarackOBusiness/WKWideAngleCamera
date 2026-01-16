using BepInEx;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HighFOV;

[BepInPlugin("wk.barackobusiness.stereographic", "High FOV Camera", "0.1.0")]
public class HighFOVPlugin : BaseUnityPlugin
{
    internal static new ManualLogSource Logger;
        
    private void Awake()
    {
        // Plugin startup logic
        Logger = base.Logger;
        Logger.LogInfo($"Wide angle views are now possible.");
    }
}
