using UnityEngine;

namespace LaunchNumbering
{
    [KSPAddon(KSPAddon.Startup.Instantly, true)]
    public class LaunchNumberTemplateInjector : MonoBehaviour
    {
        public void Start()
        {
            if (PartLoader.Instance != null && PartLoader.Instance.loadedParts.Count > 0)
                InjectModules();
            else
                GameEvents.OnPartLoaderLoaded.Add(InjectModules);
        }

        private void InjectModules()
        {
            GameEvents.OnPartLoaderLoaded.Remove(InjectModules);
            foreach (AvailablePart ap in PartLoader.Instance.loadedParts)
            {
                if (ap.partPrefab.Modules.Contains<ModuleCommand>())
                    ap.partPrefab.AddModule("LaunchNumberTemplate");
            }
        }
    }
}
