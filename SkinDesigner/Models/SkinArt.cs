using System.Collections.Generic;
using System.ComponentModel;

namespace SkinDesigner.Models {
    public class SkinArt : SkinElement {
        public string BackgroundColor { get; set; }
        [Browsable(false)]
        public List<SkinArtImage> Images { get; set; }

        public SkinArt() {
            Images = new List<SkinArtImage>();
        }
    }
}