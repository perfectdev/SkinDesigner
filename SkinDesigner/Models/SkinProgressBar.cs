using System.ComponentModel;

namespace SkinDesigner.Models {
    public class SkinProgressBar : SkinElement {
        public string Name { get; set; }
        [Browsable(false)]
        public SkinArt Art { get; set; }
        public bool Vertical { get; set; }

        public SkinProgressBar() {
            Art = new SkinArt();
        }
    }
}