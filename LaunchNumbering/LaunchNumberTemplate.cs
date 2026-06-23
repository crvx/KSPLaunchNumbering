using UnityEngine;

namespace LaunchNumbering
{
    public class LaunchNumberTemplate : PartModule
    {
        [KSPField(isPersistant = true)]
        public string templateName = "";

        [KSPField(isPersistant = true)]
        public bool used = false;

        [KSPEvent(guiActiveEditor = true, guiName = "Edit Template")]
        public void EditTemplate()
        {
            _editBuffer = templateName;
            _editing = true;
            _editRect = new Rect(Screen.width / 2f - 125f, Screen.height / 2f - 40f, 250f, 80f);
        }

        public override void OnStart(StartState state)
        {
            UpdateButtonLabel();
        }

        private void UpdateButtonLabel()
        {
            Events["EditTemplate"].guiName = string.IsNullOrEmpty(templateName)
                ? "Edit Template"
                : "Edit Template: " + templateName;
        }

        private bool _editing;
        private string _editBuffer;
        private Rect _editRect;

        public void OnGUI()
        {
            if (!_editing || !HighLogic.LoadedSceneIsEditor)
                return;

            _editRect = GUI.Window(GetInstanceID(), _editRect, EditWindow, "Launch Template");
        }

        private void EditWindow(int id)
        {
            GUILayout.BeginVertical();
            _editBuffer = GUILayout.TextField(_editBuffer, GUILayout.Width(220));
            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("OK", GUILayout.Width(70)))
            {
                templateName = _editBuffer;
                UpdateButtonLabel();
                _editing = false;
            }
            if (GUILayout.Button("Cancel", GUILayout.Width(70)))
            {
                _editing = false;
            }
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
    }
}
