using System.Collections.Generic;
using System.ComponentModel;

namespace SkinDesigner.Models {
    public class SkinComboBox : SkinElement {
        public string Name { get; set; }
        public string Text { get; set; }
        public int TabID { get; set; }
        public SkinTextAligh Align { get; set; }
        public string Style { get; set; }
        [Browsable(false)]
        public SkinArt Art { get; set; }
        [Browsable(false)]
        public List<SkinCommand> Commands { get; set; }

        public SkinComboBox() {
            Art = new SkinArt();
            Commands = new List<SkinCommand>();
        }
    }
}