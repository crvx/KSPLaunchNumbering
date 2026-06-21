using System.Collections.Generic;
using System.Linq;
using KSP.UI.Screens;
using UnityEngine;

namespace LaunchNumbering
{
    [KSPAddon(KSPAddon.Startup.FlightEditorAndKSC, false)]
    public class ManageNumbersUI : MonoBehaviour
    {
        private ApplicationLauncherButton toolbarButton;
        private bool guiEnabled = false;
        private Rect windowRect;
        private Vector2 scrollPos;

        private Dictionary<string, string> editValues = new Dictionary<string, string>();
        private Dictionary<string, int> lastSavedValues = new Dictionary<string, int>();

        private GUIStyle rowEvenStyle;
        private GUIStyle rowOddStyle;

        private const int WINDOW_WIDTH = 420;
        private const int WINDOW_HEIGHT = 320;

        void Start()
        {
            windowRect = new Rect(Screen.width - WINDOW_WIDTH - 50, Screen.height - WINDOW_HEIGHT - 40, WINDOW_WIDTH, WINDOW_HEIGHT);

            var texEven = new Texture2D(1, 1);
            texEven.SetPixel(0, 0, new Color(1f, 1f, 1f, 0.1f));
            texEven.Apply();

            var texOdd = new Texture2D(1, 1);
            texOdd.SetPixel(0, 0, new Color(0f, 0f, 0f, 0.1f));
            texOdd.Apply();

            rowEvenStyle = new GUIStyle { normal = { background = texEven }, padding = new RectOffset(0, 0, 0, 0) };
            rowOddStyle = new GUIStyle { normal = { background = texOdd }, padding = new RectOffset(0, 0, 0, 0) };

            if (ApplicationLauncher.Ready)
                OnAppLauncherReady();
            else
                GameEvents.onGUIApplicationLauncherReady.Add(OnAppLauncherReady);
        }

        void OnDestroy()
        {
            GameEvents.onGUIApplicationLauncherReady.Remove(OnAppLauncherReady);
            if (toolbarButton != null)
            {
                ApplicationLauncher.Instance.RemoveModApplication(toolbarButton);
                toolbarButton = null;
            }
            if (rowEvenStyle != null && rowEvenStyle.normal.background != null)
                Destroy(rowEvenStyle.normal.background);
            if (rowOddStyle != null && rowOddStyle.normal.background != null)
                Destroy(rowOddStyle.normal.background);
        }

        void OnAppLauncherReady()
        {
            if (ApplicationLauncher.Ready && toolbarButton == null)
            {
                var texture = GameDatabase.Instance.GetTexture("LaunchNumbering/Textures/LaunchNumbering", false);
                toolbarButton = ApplicationLauncher.Instance.AddModApplication(
                    OnToolbarToggleOn,
                    OnToolbarToggleOff,
                    null, null, null, null,
                    ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH |
                    ApplicationLauncher.AppScenes.FLIGHT | ApplicationLauncher.AppScenes.MAPVIEW |
                    ApplicationLauncher.AppScenes.SPACECENTER,
                    texture
                );
            }
        }

        void OnToolbarToggleOn()
        {
            guiEnabled = true;
        }

        void OnToolbarToggleOff()
        {
            guiEnabled = false;
        }

        void OnGUI()
        {
            if (!guiEnabled || LaunchNumberer.Instance == null)
                return;

            if (!HighLogic.CurrentGame.Parameters.CustomParams<LNSettings>().useAltSkin)
                GUI.skin = HighLogic.Skin;

            windowRect = GUI.Window(4946387, windowRect, WindowGUI, "Launch Number Manager");
        }

        void WindowGUI(int windowID)
        {
            GUILayout.BeginVertical();

            var entries = LaunchNumberer.Instance.Numbering;

            if (entries.Count == 0)
            {
                GUILayout.Label("No numbered launches yet.");
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("Vessel Tag", GUILayout.Width(210));
                GUILayout.Label("Last Number", GUILayout.Width(80));
                GUILayout.EndHorizontal();

                GUILayout.Space(4);

                scrollPos = GUILayout.BeginScrollView(scrollPos);

                string keyToDelete = null;
                int index = 0;
                foreach (var key in entries.Keys.OrderBy(k => k))
                {
                    int entry = entries[key];
                    if (!editValues.ContainsKey(key))
                        editValues[key] = entry.ToString();
                    if (!lastSavedValues.ContainsKey(key))
                        lastSavedValues[key] = entry;

                    GUILayout.BeginHorizontal(index % 2 == 0 ? rowEvenStyle : rowOddStyle);

                    GUILayout.Label(key, GUILayout.Width(210));

                    if (GUILayout.Button("-", GUILayout.Width(20)))
                    {
                        int cur = lastSavedValues[key];
                        if (cur > 1)
                        {
                            int newVal = cur - 1;
                            LaunchNumberer.Instance.SetVesselNumber(key, newVal);
                            lastSavedValues[key] = newVal;
                            editValues[key] = newVal.ToString();
                        }
                    }

                    string newText = GUILayout.TextField(editValues[key], GUILayout.Width(46));
                    editValues[key] = newText;

                    if (int.TryParse(newText, out int newNum) && newNum != lastSavedValues[key])
                    {
                        LaunchNumberer.Instance.SetVesselNumber(key, newNum);
                        lastSavedValues[key] = newNum;
                    }

                    if (GUILayout.Button("+", GUILayout.Width(20)))
                    {
                        int cur = lastSavedValues[key];
                        int newVal = cur + 1;
                        LaunchNumberer.Instance.SetVesselNumber(key, newVal);
                        lastSavedValues[key] = newVal;
                        editValues[key] = newVal.ToString();
                    }

                    if (GUILayout.Button("Delete", GUILayout.Width(60)))
                        keyToDelete = key;

                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();
                    index++;
                }

                GUILayout.EndScrollView();

                if (keyToDelete != null)
                {
                    LaunchNumberer.Instance.DeleteVessel(keyToDelete);
                    editValues.Remove(keyToDelete);
                    lastSavedValues.Remove(keyToDelete);
                }
            }

            GUILayout.EndVertical();
            GUI.DragWindow();
        }
    }
}
