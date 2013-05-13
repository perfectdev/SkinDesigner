using System.Collections.Generic;
using System.ComponentModel;

namespace SkinDesigner.Models {
    public class SkinButton : SkinElement {
        public string Name { get; set; }
        public string Text { get; set; }
        public int TabID { get; set; }
        public SkinTextAligh Align { get; set; }
        public bool PlainText { get; set; }
        public string Style { get; set; }
        public string Option { get; set; }
        [Browsable(false)]
        public SkinArt Art { get; set; }
        [Browsable(false)]
        public List<SkinCommand> Commands { get; set; }

        public SkinButton() {
            Art = new SkinArt();
            Commands = new List<SkinCommand>();
        }
    }
}