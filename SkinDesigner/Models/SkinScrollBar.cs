using System.ComponentModel;

namespace SkinDesigner.Models {
    public class SkinScrollBar : SkinElement {
        public string Name { get; set; }
        public string ListBox { get; set; }
        public int TabID { get; set; }
        [Browsable(false)]
        public SkinArt Art { get; set; }
        public int DrawLevel { get; set; }

        public SkinScrollBar() {
            Art = new SkinArt();
        }
    }
}