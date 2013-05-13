namespace SkinDesigner.Models {
    public class SkinAppStyle : SkinElement {
        public string Name { get; set; }
        public string CharSet { get; set; }
        public string Face { get; set; }
        public double Height { get; set; }
        public double Weight { get; set; }
        public string Default { get; set; }
        public string Disabled { get; set; }
        public string Hover { get; set; }
        public string Selected { get; set; }
    }
}