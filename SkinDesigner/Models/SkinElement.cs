using System.Collections.Generic;
using System.ComponentModel;

namespace SkinDesigner.Models {
    public class SkinElement {
        [Browsable(false)]
        public bool IsFolder { get; set; }
        [Browsable(false)]
        public bool IsAlreadyDrawn { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string MapColor { get; set; }
        [Browsable(false)]
        public List<ColorPosition> ColorPositions { get; set; }

        public SkinElement() {
            ColorPositions = new List<ColorPosition>();
        }
    }
}
