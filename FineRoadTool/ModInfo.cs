using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using System;
using System.Reflection;
using UnityEngine;
using static RenderManager;

[assembly: AssemblyVersion("2.0.4.0")]
namespace FineRoadTool
{
    public class ModInfo : LoadingExtensionBase, IUserMod
    {
        public ModInfo()
        {
            try
            {
                // Creating setting file
                if (GameSettings.FindSettingsFileByName(FineRoadTool.settingsFileName) == null)
                {
                    GameSettings.AddSettingsFile(new SettingsFile[] { new SettingsFile() { fileName = FineRoadTool.settingsFileName } });
                }
            }
            catch (Exception e)
            {
                DebugUtils.Log("Couldn't load/create the setting file.");
                DebugUtils.LogException(e);
            }
        }

        public void OnEnabled() {
            if (LoadingManager.instance.m_loadingComplete)
                OnLevelLoaded();
        }

        public void OnDisabled() => GameObject.Destroy(FineRoadTool.instance.gameObject);

        public override void OnLevelLoaded(LoadMode mode = default) {
            if (FineRoadTool.instance != null) {
                FineRoadTool.instance = new GameObject("FineRoadTool").AddComponent<FineRoadTool>();

                // Don't destroy it
                GameObject.DontDestroyOnLoad(FineRoadTool.instance);
            } else {
                FineRoadTool.instance.enabled = true;
            }
        }
        public override void OnLevelUnloading() {
            if (FineRoadTool.instance != null) {
                FineRoadTool.instance.enabled = false;
            }
        }

        public string Name => "Klyte's Fine Road Tool " + Version;

        public string Description => "More road tool options";

        public void OnSettingsUI(UIHelperBase helper)
        {
            try
            {
                var group = helper.AddGroup(Name) as UIHelper;
                var panel = group.self as UIPanel;

                var checkBox = (UICheckBox) group.AddCheckbox("Disable debug messages logging", DebugUtils.hideDebugMessages.value, (b) =>
                 {
                     DebugUtils.hideDebugMessages.value = b;
                 });
                checkBox.tooltip = "If checked, debug messages won't be logged.";

                group.AddSpace(10);

                checkBox = (UICheckBox) group.AddCheckbox("Reduce rail catenary masts", FineRoadTool.reduceCatenary.value, (b) =>
                 {
                     FineRoadTool.reduceCatenary.value = b;
                     if (FineRoadTool.instance != null)
                     {
                         FineRoadTool.instance.UpdateCatenary();
                     }
                 });
                checkBox.tooltip = "Reduce the number of catenary mast of rail lines from 3 to 1 per segment.\n";

                group.AddSpace(10);

                checkBox = (UICheckBox) group.AddCheckbox("Change max turn angle for more realistic tram tracks turns", FineRoadTool.changeMaxTurnAngle.value, (b) =>
                 {
                     FineRoadTool.changeMaxTurnAngle.value = b;

                     if (b)
                     {
                         RoadPrefab.SetMaxTurnAngle(FineRoadTool.maxTurnAngle);
                     }
                     else
                     {
                         RoadPrefab.ResetMaxTurnAngle();
                     }
                 });
                checkBox.tooltip = "Change all roads with tram tracks max turn angle by the value below if current value is higher";

                group.AddTextfield("Max turn angle: ", FineRoadTool.maxTurnAngle.ToString(), (f) => { },
                    (s) =>
                    {
                        float.TryParse(s, out var f);

                        FineRoadTool.maxTurnAngle.value = Mathf.Clamp(f, 0f, 180f);

                        if (FineRoadTool.changeMaxTurnAngle.value)
                        {
                            RoadPrefab.SetMaxTurnAngle(FineRoadTool.maxTurnAngle.value);
                        }
                    });

                group.AddSpace(10);

                panel.gameObject.AddComponent<OptionsKeymapping>();

                group.AddSpace(10);

                group.AddButton("Reset tool window position", () =>
                {
                    UIToolOptionsButton.savedWindowX.Delete();
                    UIToolOptionsButton.savedWindowY.Delete();

                    if (UIToolOptionsButton.toolOptionsPanel)
                    {
                        UIToolOptionsButton.toolOptionsPanel.absolutePosition = new Vector3(-1000, -1000);
                    }
                });
            }
            catch (Exception e)
            {
                DebugUtils.Log("OnSettingsUI failed");
                DebugUtils.LogException(e);
            }
        }

        public static string MinorVersion => MajorVersion + "." + typeof(ModInfo).Assembly.GetName().Version.Build;
        public static string MajorVersion => typeof(ModInfo).Assembly.GetName().Version.Major + "." + typeof(ModInfo).Assembly.GetName().Version.Minor;
        public static string FullVersion => MinorVersion + " r" + typeof(ModInfo).Assembly.GetName().Version.Revision;

        public static string Version
        {
            get {
                if (typeof(ModInfo).Assembly.GetName().Version.Minor == 0 && typeof(ModInfo).Assembly.GetName().Version.Build == 0)
                {
                    return typeof(ModInfo).Assembly.GetName().Version.Major.ToString();
                }
                if (typeof(ModInfo).Assembly.GetName().Version.Build > 0)
                {
                    return MinorVersion;
                }
                else
                {
                    return MajorVersion;
                }
            }
        }
    }
}
