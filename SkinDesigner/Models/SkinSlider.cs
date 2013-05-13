using System.ComponentModel;

namespace SkinDesigner.Models {
    public class SkinSlider : SkinElement {
        public string Name { get; set; }
        public int Min { get; set; }
        public int Max { get; set; }
        public bool DrawAllEx { get; set; }
        public int ExpandWidth { get; set; }
        public string Option { get; set; }
        [Browsable(false)]
        public SkinArt Art { get; set; }

        public SkinSlider() {
            Art = new SkinArt();
        }
    }
}