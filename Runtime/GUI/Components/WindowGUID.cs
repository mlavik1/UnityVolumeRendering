using UnityEngine.Scripting.APIUpdating;

namespace MingRay
{
    /// <summary>
    /// Generates a unique window ID.
    /// Used for creating windows with GUI.Window.
    /// </summary>
    [MovedFrom(true, sourceNamespace: "UnityVolumeRendering")]
    public class WindowGUID
    {
        private static int windowID = 0;

        public static int GetUniqueWindowID()
        {
            return windowID++;
        }
    }
}
