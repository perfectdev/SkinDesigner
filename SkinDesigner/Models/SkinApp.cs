using System.Collections.Generic;
using System.ComponentModel;

namespace SkinDesigner.Models {
    public class SkinApp : SkinElement {
        [Browsable(false)]
        public List<SkinAppStyle> Styles { get; set; }
        [Browsable(false)]
        public List<SkinAppOption> Options { get; set; }
        [Browsable(false)]
        public List<SkinWindow> Windows { get; set; }

        public SkinApp() {
            Styles = new List<SkinAppStyle>();
            Options = new List<SkinAppOption>();
            Windows = new List<SkinWindow>();
        }
    }
}
