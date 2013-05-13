using System.ComponentModel;

namespace SkinDesigner.Models {
    public class SkinScrollArrow : SkinElement {
        public string Name { get; set; }
        public string ListBox { get; set; }
        public int TabID { get; set; }
        public string Type { get; set; }
        [Browsable(false)]
        public SkinArt Art { get; set; }
        public int DrawLevel { get; set; }

        public SkinScrollArrow() {
            Art = new SkinArt();
        }
    }
}