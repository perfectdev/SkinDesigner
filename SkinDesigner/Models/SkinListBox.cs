using System.ComponentModel;

namespace SkinDesigner.Models {
    public class SkinListBox : SkinElement {
        public string Name { get; set; }
        public int TabID { get; set; }
        public string ListFile { get; set; }
        public string Option { get; set; }
        public new string MapColor { get; set; }
        public string ComboBox { get; set; }
        public string DefaultSel { get; set; }
        public string BgSelected { get; set; }
        public string BgHover { get; set; }
        public SkinTextAligh Align { get; set; }
        public string Style { get; set; }
        [Browsable(false)]
        public SkinArt Art { get; set; }
        public int DrawLevel { get; set; }

        public SkinListBox() {
            Art = new SkinArt();
        }
    }
}