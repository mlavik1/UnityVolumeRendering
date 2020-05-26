namespace UnityVolumeRendering
{
    public class WindowGUID
    {
        private static int windowID = 0;

        public static int GetUniqueWindowID()
        {
            return windowID++;
        }
    }
}
