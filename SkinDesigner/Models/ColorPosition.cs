using System.Drawing;

namespace SkinDesigner.Models {
    public class ColorPosition {
        public Color MapColor { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public ColorPosition() {
            MapColor = Color.FromArgb(255, 255, 255, 255);
        }
    }
}
