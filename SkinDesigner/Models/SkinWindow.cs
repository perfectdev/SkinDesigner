using System.Collections.Generic;
using System.ComponentModel;

namespace SkinDesigner.Models {
    public class SkinWindow : SkinElement {
        public string Name { get; set; }
        public string Text { get; set; }
        public bool MainWindow { get; set; }
        [Browsable(false)]
        public SkinArt Art { get; set; }
        [Browsable(false)]
        public List<SkinCommand> Commands { get; set; }
        [Browsable(false)]
        public List<SkinButton> Buttons { get; set; }
        [Browsable(false)]
        public List<SkinEvent> Events { get; set; }
        [Browsable(false)]
        public List<SkinProgressBar> ProgressBars { get; set; }
        [Browsable(false)]
        public List<SkinComboBox> ComboBoxes { get; set; }
        [Browsable(false)]
        public List<SkinListBox> ListBoxes { get; set; }
        [Browsable(false)]
        public List<SkinScrollBar> ScrollBars { get; set; }
        [Browsable(false)]
        public List<SkinScrollArrow> ScrollArrows { get; set; }
        [Browsable(false)]
        public List<SkinSlider> Sliders { get; set; }
        [Browsable(false)]
        public List<SkinBrowser> Browsers { get; set; }

        public SkinWindow() {
            Art = new SkinArt();
            Commands = new List<SkinCommand>();
            Buttons = new List<SkinButton>();
            Events = new List<SkinEvent>();
            ProgressBars = new List<SkinProgressBar>();
            ComboBoxes = new List<SkinComboBox>();
            ListBoxes = new List<SkinListBox>();
            ScrollBars = new List<SkinScrollBar>();
            ScrollArrows = new List<SkinScrollArrow>();
            Sliders = new List<SkinSlider>();
            Browsers = new List<SkinBrowser>();
        }
    }
}