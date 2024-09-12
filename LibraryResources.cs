using System.Windows.Media;

namespace pxKestrelLibrary
{
    public static class LibraryResources
    {
        public static Color BlockColor { get; } = pxCore.GlobalFunctions.ChangeColorBrightness(Colors.LightSalmon,0.5);
    }
}
