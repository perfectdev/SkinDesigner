using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SkinDesigner.Models {
    public class TreeViewItemSkinElement {
        public SkinElement SkinElement { get; set; }
        public List<TreeViewItemSkinElement> Items { get; set; }
        public bool IsExpanded { get; set; }
        public string Text {
            get {
                var value = "";
                switch (SkinElement.GetType().Name) {
                    case "SkinApp": value = "Skin Application"; break;
                    case "SkinFolderAppStyles": value = "Styles"; break;
                    case "SkinFolderAppOptions": value = "Options"; break;
                    case "SkinFolderAppWindows": value = "Windows"; break;
                    case "SkinFolderButtons": value = "Buttons"; break;
                    case "SkinFolderComboBoxes": value = "Combo Boxes"; break;
                    case "SkinFolderCommands": value = "Commands"; break;
                    case "SkinFolderEvents": value = "Events"; break;
                    case "SkinFolderListBoxes": value = "List Boxes"; break;
                    case "SkinFolderProgressBars": value = "Progress Bars"; break;
                    case "SkinFolderScrollBars": value = "Scroll Bars"; break;
                    case "SkinFolderScrollArrows": value = "Scroll Arrows"; break;
                    case "SkinFolderSliders": value = "Sliders"; break;
                    case "SkinFolderBrowsers": value = "Browsers"; break;
                    case "SkinAppStyle": value = ((SkinAppStyle) SkinElement).Name; break;
                    case "SkinAppOption": value = ((SkinAppOption) SkinElement).Name; break;
                    case "SkinArt": value = "Skin Art"; break;
                    case "SkinArtImage": value = ((SkinArtImage) SkinElement).Type; break;
                    case "SkinButton": value = ((SkinButton) SkinElement).Name; break;
                    case "SkinComboBox": value = ((SkinComboBox) SkinElement).Name; break;
                    case "SkinCommand": value = ((SkinCommand) SkinElement).Name; break;
                    case "SkinEvent": value = ((SkinEvent) SkinElement).Name; break;
                    case "SkinListBox": value = ((SkinListBox) SkinElement).Name; break;
                    case "SkinProgressBar": value = ((SkinProgressBar) SkinElement).Name; break;
                    case "SkinScrollArrow": value = ((SkinScrollArrow) SkinElement).Name; break;
                    case "SkinScrollBar": value = ((SkinScrollBar) SkinElement).Name; break;
                    case "SkinSlider": value = ((SkinSlider) SkinElement).Name; break;
                    case "SkinBrowser": value = ((SkinBrowser) SkinElement).Name; break;
                    case "SkinWindow": value = ((SkinWindow) SkinElement).Name; break;
                }
                return value;
            }
        }

        public ImageSource Icon {
            get {
                var value = Properties.Resources.folder;
                switch (SkinElement.GetType().Name) {
                    case "SkinApp": value = Properties.Resources.app; break;
                    case "SkinAppStyle": value = Properties.Resources.style; break;
                    case "SkinAppOption": value = Properties.Resources.option; break;
                    case "SkinArt": value = Properties.Resources.art; break;
                    case "SkinArtImage": value = Properties.Resources.image; break;
                    case "SkinButton": value = Properties.Resources.button; break;
                    case "SkinComboBox": value = Properties.Resources.combobox; break;
                    case "SkinCommand": value = Properties.Resources.command; break;
                    case "SkinEvent": value = Properties.Resources._event; break;
                    case "SkinListBox": value = Properties.Resources.listbox; break;
                    case "SkinProgressBar": value = Properties.Resources.progressbar; break;
                    case "SkinScrollArrow": value = Properties.Resources.scrollarrow; break;
                    case "SkinScrollBar": value = Properties.Resources.scroll; break;
                    case "SkinSlider": value = Properties.Resources.slider; break;
                    case "SkinBrowser": value = Properties.Resources.ie; break;
                    case "SkinWindow": value = Properties.Resources.form; break;
                }
                using (var memory = new MemoryStream()) {
                    value.Save(memory, ImageFormat.Png);
                    memory.Position = 0;
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    return bitmapImage;
                }
            }
        }

        public TreeViewItemSkinElement() {
            Items = new List<TreeViewItemSkinElement>();
        }
    }
}
