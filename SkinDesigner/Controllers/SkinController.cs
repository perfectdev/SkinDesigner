using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Xml;
using ICSharpCode.AvalonEdit;
using Microsoft.Win32;
using SkinDesigner.Components;
using SkinDesigner.Models;
using SkinDesigner.Utils;
using Color = System.Drawing.Color;
using Image = System.Windows.Controls.Image;

namespace SkinDesigner.Controllers {
    public class SkinController {
        public AppProgressBarController AppProgressBarController { get; set; }
        public Canvas Canvas { get; private set; }
        public TextEditor XmlEditor { get; set; }
        public SkinElementControl SelectedElement { get; set; }
        public SkinWindow SelectedWindow { get; set; }
        public SkinApp SkinApp { get; set; }
        public string FileName { get; set; }
        public string PatcherFolder { get; private set; }
        public bool IsPreviewMode { get; set; }

        public SkinController(Canvas canvas, TextEditor xmlEditor, AppProgressBarController appProgressBarController) {
            Canvas = canvas;
            AppProgressBarController = appProgressBarController;
            XmlEditor = xmlEditor;
            SkinApp = new SkinApp();
        }

        public void SelectElement(SkinElementControl element) {
            SelectedElement = element;
        }

        public void LoadSkin() {
            var dlg = new OpenFileDialog {
                Title = "Открыть файл разметки SkinApp",
                Filter = "XML File (*.xml)|*.xml"
            };
            if (!(bool) dlg.ShowDialog() || !File.Exists(dlg.FileName)) return;
            FileName = dlg.FileName;
            var directoryName = Path.GetDirectoryName(FileName);
            if (directoryName != null) {
                var folders = directoryName.Split(new[] {"\\"}, StringSplitOptions.RemoveEmptyEntries).Reverse().ToArray();
                PatcherFolder = string.Format("{0}{1}",
                                                directoryName.Split(
                                                    new[] {string.Format("\\{0}\\{1}", folders[1], folders[0])},
                                                    StringSplitOptions.RemoveEmptyEntries
                                                    )[0],
                                                string.Format("\\{0}\\", folders[1])
                    );
            }
            LoadSkinModel();
        }

        private void LoadSkinModel() {
            AppProgressBarController.SetValue(0, 0, "Opening file...");
            AppProgressBarController.ShowIndeterminate();
            var xmlDoc = new XmlDocument();
            var xml = File.ReadAllText(FileName);
            xml = xml.Replace("&", "&amp;");
            xmlDoc.LoadXml(xml);
            SkinApp = new SkinApp();
            var skinAppXmlNode = xmlDoc.ChildNodes.Cast<XmlElement>().First(t => t.Name.ToLower() == "skinapp");
            Core.ClearMemory();
            AppProgressBarController.Hide();
            //  Чтение SkinAppStyles
            AppProgressBarController.SetValue(0, 0, "Analyzing file...");
            AppProgressBarController.ShowIndeterminate();
            foreach (var item in skinAppXmlNode.ChildNodes.Cast<XmlElement>().Where(t => t.Name.ToLower() == "style")) {
                var skinAppStyle = new SkinAppStyle {
                    Name = item.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Name") ? item.Attributes["Name"].Value : "",
                    CharSet = item.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "CharSet") ? item.Attributes["CharSet"].Value : "",
                    Face = item.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Face") ? item.Attributes["Face"].Value : "",
                    Height = item.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Height") ? Convert.ToInt32(item.Attributes["Height"].Value) : 0,
                    Weight = item.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Weight") ? Convert.ToInt32(item.Attributes["Weight"].Value) : 0,
                    Default = item.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Default") ? item.Attributes["Default"].Value : "",
                    Disabled = item.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Disabled") ? item.Attributes["Disabled"].Value : "",
                    Selected = item.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Selected") ? item.Attributes["Selected"].Value : "",
                    Hover = item.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Hover") ? item.Attributes["Hover"].Value : ""
                };
                SkinApp.Styles.Add(skinAppStyle);
            }
            //  Чтение SkinAppOptions
            foreach (var item in skinAppXmlNode.ChildNodes.Cast<XmlElement>().Where(t => t.Name.ToLower() == "option")) {
                var skinAppOption = new SkinAppOption {
                    Name = item.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Name") ? item.Attributes["Name"].Value : "",
                    FileName = item.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName") ? item.Attributes["FileName"].Value : ""
                };
                SkinApp.Options.Add(skinAppOption);
            }
            //  Чтение SkinWindows
            foreach (var xmlWindow in skinAppXmlNode.ChildNodes.Cast<XmlElement>().Where(t => t.Name.ToLower() == "skinwindow")) {
                var skinWindow = new SkinWindow {
                    Name = xmlWindow.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Name") ? xmlWindow.Attributes["Name"].Value : "",
                    Text = xmlWindow.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Text") ? xmlWindow.Attributes["Text"].Value : "",
                    MainWindow = xmlWindow.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "MainWindow") && Convert.ToBoolean(xmlWindow.Attributes["MainWindow"].Value.ToLower())
                };
                var winArt = xmlWindow.ChildNodes.Cast<XmlElement>().First(t => t.Name.ToLower() == "skinart");
                skinWindow.Art = new SkinArt {
                    BackgroundColor = winArt.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "BackgroundColor") ? winArt.Attributes["BackgroundColor"].Value : ""
                };
                foreach (var xmlArtImage in winArt.ChildNodes.Cast<XmlElement>().Where(t => t.Name.ToLower() == "image")) {
                    skinWindow.Art.Images.Add(
                            new SkinArtImage {
                                Type = xmlArtImage.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Type") ? xmlArtImage.Attributes["Type"].Value : "",
                                Path = xmlArtImage.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Path") ? xmlArtImage.Attributes["Path"].Value : ""
                            }
                        );
                }
                foreach (var xmlCommand in xmlWindow.ChildNodes.Cast<XmlElement>().Where(t => t.Name.ToLower() == "command")) {
                    skinWindow.Commands.Add(
                            new SkinCommand {
                                Name = xmlCommand.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Name") ? xmlCommand.Attributes["Name"].Value : "",
                                Event = xmlCommand.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Event") ? xmlCommand.Attributes["Event"].Value : "",
                                FileName = xmlCommand.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName") ? xmlCommand.Attributes["FileName"].Value : "",
                                Url = xmlCommand.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "URL") ? xmlCommand.Attributes["URL"].Value : ""
                            }
                        );
                }
                foreach (var xmlBrowser in xmlWindow.ChildNodes.Cast<XmlElement>().Where(t => t.Name.ToLower() == "skinbrowser")) {
                    skinWindow.Browsers.Add(
                            new SkinBrowser {
                                Name = xmlBrowser.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Name") ? xmlBrowser.Attributes["Name"].Value : "",
                                MapColor = xmlBrowser.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "MapColor") ? xmlBrowser.Attributes["MapColor"].Value : "",
                                NoBorder = xmlBrowser.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "NoBorder") && xmlBrowser.Attributes["NoBorder"].Value.ToLower() == "true",
                                InitUrl = xmlBrowser.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "InitURL") ? xmlBrowser.Attributes["InitURL"].Value : ""
                            }
                        );
                }
                foreach (var xmlEvent in xmlWindow.ChildNodes.Cast<XmlElement>().Where(t => t.Name.Length >= 8 && t.Name.ToLower().Substring(0, 8) == "onwindow")) {
                    var _event = new SkinEvent {
                        Name = xmlEvent.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Name") ? xmlEvent.Attributes["Name"].Value : ""
                        
                    };
                    SkinEventType typeValue;
                    if (xmlEvent.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Type") &&
                        Enum.TryParse(xmlEvent.Attributes["Type"].Value, true, out typeValue)) {
                        _event.Type = typeValue;
                    }
                    skinWindow.Events.Add(_event);
                }
                foreach (var xmlButton in xmlWindow.ChildNodes.Cast<XmlElement>().Where(t => t.Name.ToLower() == "skinbutton")) {
                    var button = new SkinButton {
                        Name = xmlButton.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Name") ? xmlButton.Attributes["Name"].Value : "",
                        Text = xmlButton.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Text") ? xmlButton.Attributes["Text"].Value : "",
                        PlainText = xmlButton.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "PlainText") && xmlButton.Attributes["PlainText"].Value.ToLower() == "true",
                        TabID = xmlButton.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "TabID") ? Convert.ToInt32(xmlButton.Attributes["TabID"].Value) : 0,
                        Style = xmlButton.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Style") ? xmlButton.Attributes["Style"].Value : "",
                        Option = xmlButton.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Option") ? xmlButton.Attributes["Option"].Value : "",
                        MapColor = xmlButton.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "MapColor") ? xmlButton.Attributes["MapColor"].Value : ""
                    };
                    SkinTextAligh alignValue;
                    if (xmlButton.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Align") &&
                        Enum.TryParse(xmlButton.Attributes["Align"].Value, true, out alignValue)) {
                        button.Align = alignValue;
                    }
                    foreach (var xmlCommand in xmlButton.ChildNodes.Cast<XmlElement>().Where(t=>t.Name.ToLower() == "command")) {
                        button.Commands.Add(
                                new SkinCommand {
                                    Name = xmlCommand.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Name") ? xmlCommand.Attributes["Name"].Value : "",
                                    Event = xmlCommand.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Event") ? xmlCommand.Attributes["Event"].Value : "",
                                    FileName = xmlCommand.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "FileName") ? xmlCommand.Attributes["FileName"].Value : "",
                                    Window = xmlCommand.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Window") ? xmlCommand.Attributes["Window"].Value : "",
                                    Url = xmlCommand.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "URL") ? xmlCommand.Attributes["URL"].Value : ""
                                }
                            );
                    }
                    foreach (var xmlButtonArt in xmlButton.ChildNodes.Cast<XmlElement>().Where(t => t.Name.ToLower() == "skinart")) {
                        button.Art = new SkinArt {
                            BackgroundColor = xmlButtonArt.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "BackgroundColor") ? xmlButtonArt.Attributes["BackgroundColor"].Value : ""
                        };
                        foreach (var xmlButtonArtImage in xmlButtonArt.ChildNodes.Cast<XmlElement>().Where(t => t.Name.ToLower() == "image")) {
                            button.Art.Images.Add(
                                    new SkinArtImage {
                                        Type = xmlButtonArtImage.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Type") ? xmlButtonArtImage.Attributes["Type"].Value : "",
                                        Path = xmlButtonArtImage.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Path") ? xmlButtonArtImage.Attributes["Path"].Value : ""
                                    }
                                );
                        }
                    }
                    skinWindow.Buttons.Add(button);
                }
                foreach (var xmlProgressBar in xmlWindow.ChildNodes.Cast<XmlElement>().Where(t => t.Name.ToLower() == "skinprogressbar")) {
                    var progressBar = new SkinProgressBar {
                        Name = xmlProgressBar.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Name") ? xmlProgressBar.Attributes["Name"].Value : "",
                        MapColor = xmlProgressBar.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "MapColor") ? xmlProgressBar.Attributes["MapColor"].Value : "",
                        Vertical = xmlProgressBar.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Vertical") && xmlProgressBar.Attributes["Vertical"].Value.ToLower() == "true"
                    };
                    foreach (var xmlProgressBarArt in xmlProgressBar.ChildNodes.Cast<XmlElement>().Where(t => t.Name.ToLower() == "skinart")) {
                        progressBar.Art = new SkinArt {
                            BackgroundColor = xmlProgressBarArt.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "BackgroundColor") ? xmlProgressBarArt.Attributes["BackgroundColor"].Value : ""
                        };
                        foreach (var xmlProgressBarArtImage in xmlProgressBarArt.ChildNodes.Cast<XmlElement>().Where(t => t.Name.ToLower() == "image")) {
                            progressBar.Art.Images.Add(
                                    new SkinArtImage {
                                        Type = xmlProgressBarArtImage.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Type") ? xmlProgressBarArtImage.Attributes["Type"].Value : "",
                                        Path = xmlProgressBarArtImage.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Path") ? xmlProgressBarArtImage.Attributes["Path"].Value : ""
                                    }
                                );
                        }
                        skinWindow.ProgressBars.Add(progressBar);
                    }
                }
                foreach (var xmlComboBox in xmlWindow.ChildNodes.Cast<XmlElement>().Where(t => t.Name.ToLower() == "skincombobox")) {
                    var comboBox = new SkinComboBox {
                        Name = xmlComboBox.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Name") ? xmlComboBox.Attributes["Name"].Value : "",
                        Text = xmlComboBox.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Text") ? xmlComboBox.Attributes["Text"].Value : "",
                        TabID = xmlComboBox.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "TabID") ? Convert.ToInt32(xmlComboBox.Attributes["TabID"].Value) : 0,
                        Style = xmlComboBox.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Style") ? xmlComboBox.Attributes["Style"].Value : "",
                        MapColor = xmlComboBox.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "MapColor") ? xmlComboBox.Attributes["MapColor"].Value : ""
                    };
                    SkinTextAligh alignValue;
                    if (xmlComboBox.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Align") &&
                        Enum.TryParse(xmlComboBox.Attributes["Align"].Value, true, out alignValue)) {
                        comboBox.Align = alignValue;
                    }
                    foreach (var xmlComboBoxArt in xmlComboBox.ChildNodes.Cast<XmlElement>().Where(t => t.Name.ToLower() == "skinart")) {
                        comboBox.Art = new SkinArt {
                            BackgroundColor = xmlComboBoxArt.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "BackgroundColor") ? xmlComboBoxArt.Attributes["BackgroundColor"].Value : ""
                        };
                        foreach (var xmlComboBoxArtImage in xmlComboBoxArt.ChildNodes.Cast<XmlElement>().Where(t => t.Name.ToLower() == "image")) {
                            comboBox.Art.Images.Add(
                                    new SkinArtImage {
                                        Type = xmlComboBoxArtImage.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Type") ? xmlComboBoxArtImage.Attributes["Type"].Value : "",
                                        Path = xmlComboBoxArtImage.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Path") ? xmlComboBoxArtImage.Attributes["Path"].Value : ""
                                    }
                                );
                        }
                        skinWindow.ComboBoxes.Add(comboBox);
                    }
                }
                foreach (var xmlListBox in xmlWindow.ChildNodes.Cast<XmlElement>().Where(t => t.Name.ToLower() == "skinlistbox")) {
                    var listBox = new SkinListBox {
                        Name = xmlListBox.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Name") ? xmlListBox.Attributes["Name"].Value : "",
                        ListFile = xmlListBox.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "ListFile") ? xmlListBox.Attributes["ListFile"].Value : "",
                        ComboBox = xmlListBox.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "ComboBox") ? xmlListBox.Attributes["ComboBox"].Value : "",
                        DefaultSel = xmlListBox.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "DefaultSel") ? xmlListBox.Attributes["DefaultSel"].Value : "",
                        Option = xmlListBox.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Option") ? xmlListBox.Attributes["Option"].Value : "",
                        TabID = xmlListBox.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "TabID") ? Convert.ToInt32(xmlListBox.Attributes["TabID"].Value) : 0,
                        DrawLevel = xmlListBox.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "DrawLevel") ? Convert.ToInt32(xmlListBox.Attributes["DrawLevel"].Value) : 0,
                        Style = xmlListBox.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Style") ? xmlListBox.Attributes["Style"].Value : "",
                        BgSelected = xmlListBox.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "bgSelected") ? xmlListBox.Attributes["bgSelected"].Value : "",
                        BgHover = xmlListBox.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "bgHover") ? xmlListBox.Attributes["bgHover"].Value : "",
                        MapColor = xmlListBox.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "MapColor") ? xmlListBox.Attributes["MapColor"].Value : ""
                    };
                    SkinTextAligh alignValue;
                    if (xmlListBox.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Align") &&
                        Enum.TryParse(xmlListBox.Attributes["Align"].Value, true, out alignValue)) {
                        listBox.Align = alignValue;
                    }
                    foreach (var xmlListBoxArt in xmlListBox.ChildNodes.Cast<XmlElement>().Where(t => t.Name.ToLower() == "skinart")) {
                        listBox.Art = new SkinArt {
                            BackgroundColor = xmlListBoxArt.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "BackgroundColor") ? xmlListBoxArt.Attributes["BackgroundColor"].Value : ""
                        };
                        foreach (var xmlListBoxArtImage in xmlListBoxArt.ChildNodes.Cast<XmlElement>().Where(t => t.Name.ToLower() == "image")) {
                            listBox.Art.Images.Add(
                                    new SkinArtImage {
                                        Type = xmlListBoxArtImage.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Type") ? xmlListBoxArtImage.Attributes["Type"].Value : "",
                                        Path = xmlListBoxArtImage.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Path") ? xmlListBoxArtImage.Attributes["Path"].Value : ""
                                    }
                                );
                        }
                        skinWindow.ListBoxes.Add(listBox);
                    }
                }
                foreach (var xmlScrollBar in xmlWindow.ChildNodes.Cast<XmlElement>().Where(t => t.Name.ToLower() == "skinscrollbar")) {
                    var scrollBar = new SkinScrollBar {
                        Name = xmlScrollBar.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Name") ? xmlScrollBar.Attributes["Name"].Value : "",
                        ListBox = xmlScrollBar.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "ListBox") ? xmlScrollBar.Attributes["ListBox"].Value : "",
                        TabID = xmlScrollBar.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "TabID") ? Convert.ToInt32(xmlScrollBar.Attributes["TabID"].Value) : 0,
                        DrawLevel = xmlScrollBar.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "DrawLevel") ? Convert.ToInt32(xmlScrollBar.Attributes["DrawLevel"].Value) : 0,
                        MapColor = xmlScrollBar.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "MapColor") ? xmlScrollBar.Attributes["MapColor"].Value : ""
                    };
                    foreach (var xmlScrollBarArt in xmlScrollBar.ChildNodes.Cast<XmlElement>().Where(t => t.Name.ToLower() == "skinart")) {
                        scrollBar.Art = new SkinArt {
                            BackgroundColor = xmlScrollBarArt.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "BackgroundColor") ? xmlScrollBarArt.Attributes["BackgroundColor"].Value : ""
                        };
                        foreach (var xmlScrollBarArtImage in xmlScrollBarArt.ChildNodes.Cast<XmlElement>().Where(t => t.Name.ToLower() == "image")) {
                            scrollBar.Art.Images.Add(
                                    new SkinArtImage {
                                        Type = xmlScrollBarArtImage.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Type") ? xmlScrollBarArtImage.Attributes["Type"].Value : "",
                                        Path = xmlScrollBarArtImage.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Path") ? xmlScrollBarArtImage.Attributes["Path"].Value : ""
                                    }
                                );
                        }
                        skinWindow.ScrollBars.Add(scrollBar);
                    }
                }
                foreach (var xmlScrollArrow in xmlWindow.ChildNodes.Cast<XmlElement>().Where(t => t.Name.ToLower() == "skinscrollarrow")) {
                    var scrollArrow = new SkinScrollArrow {
                        Name = xmlScrollArrow.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Name") ? xmlScrollArrow.Attributes["Name"].Value : "",
                        ListBox = xmlScrollArrow.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "ListBox") ? xmlScrollArrow.Attributes["ListBox"].Value : "",
                        Type = xmlScrollArrow.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Type") ? xmlScrollArrow.Attributes["Type"].Value : "",
                        DrawLevel = xmlScrollArrow.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "DrawLevel") ? Convert.ToInt32(xmlScrollArrow.Attributes["DrawLevel"].Value) : 0,
                        TabID = xmlScrollArrow.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "TabID") ? Convert.ToInt32(xmlScrollArrow.Attributes["TabID"].Value) : 0,
                        MapColor = xmlScrollArrow.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "MapColor") ? xmlScrollArrow.Attributes["MapColor"].Value : ""
                    };
                    foreach (var xmlScrollArrowArt in xmlScrollArrow.ChildNodes.Cast<XmlElement>().Where(t => t.Name.ToLower() == "skinart")) {
                        scrollArrow.Art = new SkinArt {
                            BackgroundColor = xmlScrollArrowArt.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "BackgroundColor") ? xmlScrollArrowArt.Attributes["BackgroundColor"].Value : ""
                        };
                        foreach (var xmlScrollBarArtImage in xmlScrollArrowArt.ChildNodes.Cast<XmlElement>().Where(t => t.Name.ToLower() == "image")) {
                            scrollArrow.Art.Images.Add(
                                    new SkinArtImage {
                                        Type = xmlScrollBarArtImage.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Type") ? xmlScrollBarArtImage.Attributes["Type"].Value : "",
                                        Path = xmlScrollBarArtImage.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Path") ? xmlScrollBarArtImage.Attributes["Path"].Value : ""
                                    }
                                );
                        }
                        skinWindow.ScrollArrows.Add(scrollArrow);
                    }
                }
                foreach (var xmlSlider in xmlWindow.ChildNodes.Cast<XmlElement>().Where(t => t.Name.ToLower() == "skinslider")) {
                    var slider = new SkinSlider {
                        Name = xmlSlider.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Name") ? xmlSlider.Attributes["Name"].Value : "",
                        Min = xmlSlider.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Min") ? Convert.ToInt32(xmlSlider.Attributes["Min"].Value) : 0,
                        Max = xmlSlider.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Max") ? Convert.ToInt32(xmlSlider.Attributes["Max"].Value) : 0,
                        ExpandWidth = xmlSlider.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "ExpandWidth") ? Convert.ToInt32(xmlSlider.Attributes["ExpandWidth"].Value) : 0,
                        DrawAllEx = xmlSlider.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "DrawAllEx") && Convert.ToBoolean(xmlSlider.Attributes["DrawAllEx"].Value),
                        Option = xmlSlider.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Option") ? xmlSlider.Attributes["Option"].Value : "",
                        MapColor = xmlSlider.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "MapColor") ? xmlSlider.Attributes["MapColor"].Value : ""
                    };
                    foreach (var xmlSliderArt in xmlSlider.ChildNodes.Cast<XmlElement>().Where(t => t.Name.ToLower() == "skinart")) {
                        slider.Art = new SkinArt {
                            BackgroundColor = xmlSliderArt.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "BackgroundColor") ? xmlSliderArt.Attributes["BackgroundColor"].Value : ""
                        };
                        foreach (var xmlScrollBarArtImage in xmlSliderArt.ChildNodes.Cast<XmlElement>().Where(t => t.Name.ToLower() == "image")) {
                            slider.Art.Images.Add(
                                    new SkinArtImage {
                                        Type = xmlScrollBarArtImage.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Type") ? xmlScrollBarArtImage.Attributes["Type"].Value : "",
                                        Path = xmlScrollBarArtImage.Attributes.Cast<XmlAttribute>().Any(t => t.Name == "Path") ? xmlScrollBarArtImage.Attributes["Path"].Value : ""
                                    }
                                );
                        }
                        skinWindow.Sliders.Add(slider);
                    }
                }
                
                SkinApp.Windows.Add(skinWindow);
                AppProgressBarController.Hide();
            }
        }

        public List<TreeViewItemSkinElement> GetTreeViewItemsSource() {
            AppProgressBarController.SetValue(0, 0, "Building structures...");
            AppProgressBarController.ShowIndeterminate();

            var items = new List<TreeViewItemSkinElement>();

            var nodeSkinApp = new TreeViewItemSkinElement {
                SkinElement = SkinApp,
                IsExpanded = true
            };
            var nodeSkinAppStyles = new TreeViewItemSkinElement { SkinElement = new SkinFolderAppStyles() };
            foreach (var item in SkinApp.Styles) {
                nodeSkinAppStyles.Items.Add(new TreeViewItemSkinElement { SkinElement = item });
            }
            nodeSkinApp.Items.Add(nodeSkinAppStyles);
            var nodeSkinAppOptions = new TreeViewItemSkinElement { SkinElement = new SkinFolderAppOptions() };
            foreach (var item in SkinApp.Options) {
                nodeSkinAppOptions.Items.Add(new TreeViewItemSkinElement { SkinElement = item });
            }
            nodeSkinApp.Items.Add(nodeSkinAppOptions);
            var nodeSkinAppWindows = new TreeViewItemSkinElement { SkinElement = new SkinFolderAppWindows() };
            foreach (var window in SkinApp.Windows) {
                var nodeSkinArts = new TreeViewItemSkinElement();
                var nodeSkinArtImages = new TreeViewItemSkinElement {SkinElement = window.Art};
                foreach (var image in window.Art.Images) {
                    nodeSkinArtImages.Items.Add(new TreeViewItemSkinElement { SkinElement = image });
                }
                nodeSkinArts.Items.Add(nodeSkinArtImages);
                nodeSkinAppWindows.Items.Add(
                        new TreeViewItemSkinElement { 
                            SkinElement = window,
                            Items = nodeSkinArts.Items
                        }
                    );

                var nodeSkinCommands = new TreeViewItemSkinElement { SkinElement = new SkinFolderCommands() };
                foreach (var command in window.Commands) {
                    nodeSkinCommands.Items.Add(new TreeViewItemSkinElement { SkinElement = command });
                }
                nodeSkinArts.Items.Add(nodeSkinCommands);

                var nodeSkinBrowsers = new TreeViewItemSkinElement { SkinElement = new SkinFolderBrowsers() };
                foreach (var browser in window.Browsers) {
                    nodeSkinBrowsers.Items.Add(new TreeViewItemSkinElement { SkinElement = browser });
                }
                nodeSkinArts.Items.Add(nodeSkinBrowsers);

                var nodeSkinEvents = new TreeViewItemSkinElement { SkinElement = new SkinFolderEvents() };
                foreach (var _event in window.Events) {
                    nodeSkinEvents.Items.Add(new TreeViewItemSkinElement { SkinElement = _event });
                }
                nodeSkinArts.Items.Add(nodeSkinEvents);

                var nodeSkinButtons = new TreeViewItemSkinElement { SkinElement = new SkinFolderButtons() };
                foreach (var button in window.Buttons) {
                    var nodeButton = new TreeViewItemSkinElement { SkinElement = button };

                    var nodeSkinButtonCommands = new TreeViewItemSkinElement { SkinElement = new SkinFolderCommands() };
                    foreach (var command in button.Commands) {
                        nodeSkinButtonCommands.Items.Add(new TreeViewItemSkinElement { SkinElement = command });
                    }
                    nodeButton.Items.Add(nodeSkinButtonCommands);

                    var nodeSkinButtonArts = new TreeViewItemSkinElement { SkinElement = button.Art };
                    foreach (var image in button.Art.Images) {
                        nodeSkinButtonArts.Items.Add(new TreeViewItemSkinElement { SkinElement = image });
                    }
                    nodeButton.Items.Add(nodeSkinButtonArts);

                    nodeSkinButtons.Items.Add(nodeButton);
                }
                nodeSkinArts.Items.Add(nodeSkinButtons);

                var nodeSkinProgressBars = new TreeViewItemSkinElement { SkinElement = new SkinFolderProgressBars() };
                foreach (var progressBar in window.ProgressBars) {
                    var nodeProgressBar = new TreeViewItemSkinElement { SkinElement = progressBar };

                    var nodeSkinProgressBarArts = new TreeViewItemSkinElement { SkinElement = progressBar.Art };
                    foreach (var image in progressBar.Art.Images) {
                        nodeSkinProgressBarArts.Items.Add(new TreeViewItemSkinElement { SkinElement = image });
                    }
                    nodeProgressBar.Items.Add(nodeSkinProgressBarArts);

                    nodeSkinProgressBars.Items.Add(nodeProgressBar);
                }
                nodeSkinArts.Items.Add(nodeSkinProgressBars);

                var nodeSkinComboBox = new TreeViewItemSkinElement { SkinElement = new SkinFolderComboBoxes() };
                foreach (var comboBox in window.ComboBoxes) {
                    var nodeComboBox = new TreeViewItemSkinElement { SkinElement = comboBox };

                    var nodeSkinComboBoxArts = new TreeViewItemSkinElement { SkinElement = comboBox.Art };
                    foreach (var image in comboBox.Art.Images) {
                        nodeSkinComboBoxArts.Items.Add(new TreeViewItemSkinElement { SkinElement = image });
                    }
                    nodeComboBox.Items.Add(nodeSkinComboBoxArts);

                    nodeSkinComboBox.Items.Add(nodeComboBox);
                }
                nodeSkinArts.Items.Add(nodeSkinComboBox);

                var nodeSkinListBox = new TreeViewItemSkinElement { SkinElement = new SkinFolderListBoxes() };
                foreach (var listBox in window.ListBoxes) {
                    var nodeListBox = new TreeViewItemSkinElement { SkinElement = listBox };

                    var nodeSkinListBoxArts = new TreeViewItemSkinElement { SkinElement = listBox.Art };
                    foreach (var image in listBox.Art.Images) {
                        nodeSkinListBoxArts.Items.Add(new TreeViewItemSkinElement { SkinElement = image });
                    }
                    nodeListBox.Items.Add(nodeSkinListBoxArts);

                    nodeSkinListBox.Items.Add(nodeListBox);
                }
                nodeSkinArts.Items.Add(nodeSkinListBox);

                var nodeSkinScrollBar = new TreeViewItemSkinElement { SkinElement = new SkinFolderScrollBars() };
                foreach (var scrollBar in window.ScrollBars) {
                    var nodeScrollBar = new TreeViewItemSkinElement { SkinElement = scrollBar };

                    var nodeSkinScrollBarArts = new TreeViewItemSkinElement { SkinElement = scrollBar.Art };
                    foreach (var image in scrollBar.Art.Images) {
                        nodeSkinScrollBarArts.Items.Add(new TreeViewItemSkinElement { SkinElement = image });
                    }
                    nodeScrollBar.Items.Add(nodeSkinScrollBarArts);

                    nodeSkinScrollBar.Items.Add(nodeScrollBar);
                }
                nodeSkinArts.Items.Add(nodeSkinScrollBar);

                var nodeSkinScrollArrow = new TreeViewItemSkinElement { SkinElement = new SkinFolderScrollArrows() };
                foreach (var scrollArrow in window.ScrollArrows) {
                    var nodeScrollArrow = new TreeViewItemSkinElement { SkinElement = scrollArrow };

                    var nodeSkinScrollArrowArts = new TreeViewItemSkinElement { SkinElement = scrollArrow.Art };
                    foreach (var image in scrollArrow.Art.Images) {
                        nodeSkinScrollArrowArts.Items.Add(new TreeViewItemSkinElement { SkinElement = image });
                    }
                    nodeScrollArrow.Items.Add(nodeSkinScrollArrowArts);

                    nodeSkinScrollArrow.Items.Add(nodeScrollArrow);
                }
                nodeSkinArts.Items.Add(nodeSkinScrollArrow);

                var nodeSkinSlider = new TreeViewItemSkinElement { SkinElement = new SkinFolderSliders() };
                foreach (var slider in window.Sliders) {
                    var nodeSlider = new TreeViewItemSkinElement { SkinElement = slider };

                    var nodeSkinSliderArts = new TreeViewItemSkinElement { SkinElement = slider.Art };
                    foreach (var image in slider.Art.Images) {
                        nodeSkinSliderArts.Items.Add(new TreeViewItemSkinElement { SkinElement = image });
                    }
                    nodeSlider.Items.Add(nodeSkinSliderArts);

                    nodeSkinSlider.Items.Add(nodeSlider);
                }
                nodeSkinArts.Items.Add(nodeSkinSlider);
            }

            nodeSkinApp.Items.Add(nodeSkinAppWindows);
            items.Add(nodeSkinApp);

            AppProgressBarController.Hide();

            return items;
        }

        public void DrawElement(TreeViewItemSkinElement element) {
            DrawElement(element.SkinElement);
        }

        public void DrawElement(SkinElement element) {
            switch (element.GetType().Name) {
                case "SkinWindow":
                    RedrawWindow((SkinWindow) element);
                    break;
                case "SkinButton":
                    RedrawButton((SkinButton) element);
                    break;
                case "SkinComboBox":
                    RedrawComboBox((SkinComboBox) element);
                    break;
                case "SkinSlider":
                    RedrawSlider((SkinSlider) element);
                    break;
            }
        }

        private void RedrawSlider(SkinSlider slider) {
            var color = Core.MapColor2Color(slider.MapColor);
            if (SelectedWindow.ColorPositions.All(t => t.MapColor != color))
                return;
            var pos = SelectedWindow.ColorPositions.First(t => t.MapColor == color);
            slider.X = pos.X;
            slider.Y = pos.Y;
            slider.Width = pos.Width;
            slider.Height = pos.Height;
            SkinElementControl sliderControl;
            if (Canvas.Children.Cast<UIElement>().Where(t => t.GetType() == typeof(SkinElementControl)).Cast<SkinElementControl>().Any(t => t.SkinElement == slider)) {
                sliderControl = Canvas.Children.Cast<UIElement>().Where(t => t.GetType() == typeof(SkinElementControl)).Cast<SkinElementControl>().First(t => t.SkinElement == slider);
                Canvas.Children.Remove(sliderControl);
            }

            sliderControl = new SkinElementControl(this, slider);
            Canvas.Children.Add(sliderControl);
        }

        private void RedrawButton(SkinButton button) {
            var color = Core.MapColor2Color(button.MapColor);
            if (SelectedWindow.ColorPositions.All(t => t.MapColor != color))
                return;
            var pos = SelectedWindow.ColorPositions.First(t => t.MapColor == color);
            button.X = pos.X;
            button.Y = pos.Y;
            button.Width = pos.Width;
            button.Height = pos.Height;
            SkinElementControl buttonControl;
            if (Canvas.Children.Cast<UIElement>().Where(t => t.GetType() == typeof(SkinElementControl)).Cast<SkinElementControl>().Any(t => t.SkinElement == button)) {
                buttonControl = Canvas.Children.Cast<UIElement>().Where(t => t.GetType() == typeof(SkinElementControl)).Cast<SkinElementControl>().First(t => t.SkinElement == button);
                Canvas.Children.Remove(buttonControl);
            }

            buttonControl = new SkinElementControl(this, button);
            Canvas.Children.Add(buttonControl);
        }

        private void RedrawComboBox(SkinComboBox comboBox) {
            var color = Core.MapColor2Color(comboBox.MapColor);
            if (SelectedWindow.ColorPositions.All(t => t.MapColor != color))
                return;
            var pos = SelectedWindow.ColorPositions.First(t => t.MapColor == color);
            comboBox.X = pos.X;
            comboBox.Y = pos.Y;
            comboBox.Width = pos.Width;
            comboBox.Height = pos.Height;
            SkinElementControl comboBoxControl;
            if (Canvas.Children.Cast<UIElement>().Where(t => t.GetType() == typeof(SkinElementControl)).Cast<SkinElementControl>().Any(t => t.SkinElement == comboBox)) {
                comboBoxControl = Canvas.Children.Cast<UIElement>().Where(t => t.GetType() == typeof(SkinElementControl)).Cast<SkinElementControl>().First(t => t.SkinElement == comboBox);
                Canvas.Children.Remove(comboBoxControl);
            }

            comboBoxControl = new SkinElementControl(this, comboBox);
            Canvas.Children.Add(comboBoxControl);
        }

        private void RedrawProgressBar(SkinProgressBar progressBar) {
            var color = Core.MapColor2Color(progressBar.MapColor);
            if (SelectedWindow.ColorPositions.All(t => t.MapColor != color))
                return;
            var pos = SelectedWindow.ColorPositions.First(t => t.MapColor == color);
            progressBar.X = pos.X;
            progressBar.Y = pos.Y;
            progressBar.Width = pos.Width;
            progressBar.Height = pos.Height;
            SkinElementControl progressBarControl;
            if (Canvas.Children.Cast<UIElement>().Where(t => t.GetType() == typeof(SkinElementControl)).Cast<SkinElementControl>().Any(t => t.SkinElement == progressBar)) {
                progressBarControl = Canvas.Children.Cast<UIElement>().Where(t => t.GetType() == typeof(SkinElementControl)).Cast<SkinElementControl>().First(t => t.SkinElement == progressBar);
                Canvas.Children.Remove(progressBarControl);
            }

            progressBarControl = new SkinElementControl(this, progressBar);
            Canvas.Children.Add(progressBarControl);
        }

        private void RedrawBrowser(SkinBrowser browser) {
            var color = Core.MapColor2Color(browser.MapColor);
            if (SelectedWindow.ColorPositions.All(t => t.MapColor != color))
                return;
            var pos = SelectedWindow.ColorPositions.First(t => t.MapColor == color);
            browser.X = pos.X;
            browser.Y = pos.Y;
            browser.Width = pos.Width;
            browser.Height = pos.Height;
            SkinElementControl browserControl;
            if (Canvas.Children.Cast<UIElement>().Where(t => t.GetType() == typeof(SkinElementControl)).Cast<SkinElementControl>().Any(t => t.SkinElement == browser)) {
                browserControl = Canvas.Children.Cast<UIElement>().Where(t => t.GetType() == typeof(SkinElementControl)).Cast<SkinElementControl>().First(t => t.SkinElement == browser);
                Canvas.Children.Remove(browserControl);
            }

            browserControl = new SkinElementControl(this, browser);
            Canvas.Children.Add(browserControl);
        }

        private void RedrawWindow(SkinWindow window) {
            AppProgressBarController.SetValue(0, 0, "Redraw window...");
            AppProgressBarController.ShowIndeterminate();
            Canvas.Children.Clear();
            var artBgMap = window.Art.Images.Any(t => t.Type == "MapImage") ? window.Art.Images.First(t => t.Type == "MapImage") : new SkinArtImage();
            if (!window.IsAlreadyDrawn)
                RebuildImageMap(artBgMap, window);
            AppProgressBarController.DoEvents();
            window.IsAlreadyDrawn = true;
            Core.ClearMemory();
            var artBgImage = window.Art.Images.Any(t => t.Type == "BackgroundImage") ? window.Art.Images.First(t => t.Type == "BackgroundImage") : new SkinArtImage();
            var bgImage = (BitmapImage) Core.GetImageSourceFromFileName(GetFullPath(artBgImage.Path));
            Canvas.Children.Add(new Image { Source = bgImage, Width = bgImage.PixelWidth, Height = bgImage.PixelHeight });
            AppProgressBarController.DoEvents();
            DrawButtons(window);
            DrawComboBoxes(window);
            DrawProgressBars(window);
            DrawBrowsers(window);
            DrawSliders(window);
            AppProgressBarController.Hide();
        }

        private void DrawButtons(SkinWindow window) {
            foreach (var button in window.Buttons) {
                RedrawButton(button);
            }
        }

        private void DrawComboBoxes(SkinWindow window) {
            foreach (var comboBox in window.ComboBoxes) {
                RedrawComboBox(comboBox);
            }
        }

        private void DrawProgressBars(SkinWindow window) {
            foreach (var progressBar in window.ProgressBars) {
                RedrawProgressBar(progressBar);
            }
        }

        private void DrawBrowsers(SkinWindow window) {
            foreach (var browser in window.Browsers) {
                RedrawBrowser(browser);
            }
        }

        private void DrawSliders(SkinWindow window) {
            foreach (var slider in window.Sliders) {
                RedrawSlider(slider);
            }
        }

        public string GetFullPath(string file) {
            return string.Format("{0}{1}", PatcherFolder, string.IsNullOrEmpty(file) ? file : file.Replace("/", "\\"));
        }

        private void RebuildImageMap(SkinArtImage artBgMap, SkinElement element) {
            element.ColorPositions.Clear();

            var imageMap = (BitmapImage) Core.GetImageSourceFromFileName(GetFullPath(artBgMap.Path));
            var bmp = Core.BitmapImage2Bitmap(imageMap);

            var transparentColor = Color.FromKnownColor(KnownColor.Transparent);
            AppProgressBarController.SetValue(0, bmp.Height, "Analyzing image map...");
            AppProgressBarController.Show();
            var i = 0;
            for (var y = 0; y < imageMap.PixelHeight; y++) {
                i++;
                if (i%10 == 0) {
                    AppProgressBarController.SetValue(i, bmp.Height, "Analyzing image map...");
                    AppProgressBarController.DoEvents();
                }
                for (var x = 0; x < imageMap.PixelWidth; x++) {
                    //  Получаем цвет фона карты
                    var pixel = bmp.GetPixel(x, y);
                    if (x == 0 && y == 0) {
                        transparentColor = Color.FromArgb(pixel.A, pixel.R, pixel.G, pixel.B);
                        continue;
                    }
                    var currentPixelColor = Color.FromArgb(pixel.A, pixel.R, pixel.G, pixel.B);
                    if (currentPixelColor != transparentColor) {
                        if (element.ColorPositions.Any(t => t.MapColor == currentPixelColor)) {
                            var pos = element.ColorPositions.First(t => t.MapColor == currentPixelColor);
                            pos.Width = x > pos.X + pos.Width - 1 ? x - pos.X + 1 : pos.Width;
                            pos.Height = y > pos.Y + pos.Height - 1 ? y - pos.Y + 1 : pos.Height;
                        } else {
                            element.ColorPositions.Add(new ColorPosition { MapColor = currentPixelColor, X = x, Y = y });
                        }
                    }
                }
            }
            AppProgressBarController.Hide();
        }

        public void SaveImageMap(SkinElement element) {
            SkinArtImage artBgMap = null;
            if (element.ColorPositions.Count == 0) return;
            switch (element.GetType().Name) {
                case "SkinWindow": artBgMap = ((SkinWindow) element).Art.Images.FirstOrDefault(t => t.Type == "MapImage"); break;
            }
            if (artBgMap == null) return;

            var imageMap = (BitmapImage) Core.GetImageSourceFromFileName(GetFullPath(artBgMap.Path));
            var bmp = Core.BitmapImage2Bitmap(imageMap);
            bmp = new Bitmap(bmp.Width, bmp.Height);
            using (var g = Graphics.FromImage(bmp)) {
                g.Clear(Color.FromKnownColor(KnownColor.Transparent));
                foreach (var pos in element.ColorPositions) {
                    g.FillRectangle(
                            new SolidBrush(pos.MapColor),
                            (int) pos.X,
                            (int) pos.Y,
                            (int) pos.Width,
                            (int) pos.Height
                        );
                }
            }
            bmp.Save(GetFullPath(artBgMap.Path), ImageFormat.Png);
        }

        public void Save() {
            AppProgressBarController.SetValue(0, 0, "Save...");
            AppProgressBarController.ShowIndeterminate();
            var xmlDoc = new XmlDocument();
            var xmlSkinApp = xmlDoc.CreateElement("SkinApp");
            xmlDoc.AppendChild(xmlSkinApp);
            foreach (var style in SkinApp.Styles) {
                var xmlStyle = xmlDoc.CreateElement("Style");
                xmlStyle.SetAttribute("Name", style.Name);
                xmlStyle.SetAttribute("Face", style.Face);
                xmlStyle.SetAttribute("Height", style.Height.ToString(CultureInfo.InvariantCulture));
                if (style.Weight > 0)
                    xmlStyle.SetAttribute("Weight", style.Weight.ToString(CultureInfo.InvariantCulture));
                xmlStyle.SetAttribute("Default", style.Default);
                if (!string.IsNullOrEmpty(style.Disabled))
                    xmlStyle.SetAttribute("Disabled", style.Disabled);
                if (!string.IsNullOrEmpty(style.Hover))
                    xmlStyle.SetAttribute("Hover", style.Hover);
                if (!string.IsNullOrEmpty(style.Selected))
                    xmlStyle.SetAttribute("Selected", style.Selected);
                xmlSkinApp.AppendChild(xmlStyle);
            }
            foreach (var option in SkinApp.Options) {
                var xmlOption = xmlDoc.CreateElement("Option");
                xmlOption.SetAttribute("Name", option.Name);
                xmlOption.SetAttribute("FileName", option.FileName);
                xmlSkinApp.AppendChild(xmlOption);
            }
            foreach (var window in SkinApp.Windows) {
                SaveImageMap(window);

                var xmlWindow = xmlDoc.CreateElement("SkinWindow");
                xmlWindow.SetAttribute("Name", window.Name);
                xmlWindow.SetAttribute("Text", window.Text);
                if (window.MainWindow)
                    xmlWindow.SetAttribute("MainWindow", window.MainWindow.ToString().ToLower());

                foreach (var _event in window.Events) {
                    var xmlEvent = xmlDoc.CreateElement(_event.Type.ToString());
                    xmlEvent.SetAttribute("Name", _event.Name);
                    xmlWindow.AppendChild(xmlEvent);
                }

                var xmlSkinArt = xmlDoc.CreateElement("SkinArt");
                xmlSkinArt.SetAttribute("BackgroundColor", window.Art.BackgroundColor);
                foreach (var artImage in window.Art.Images) {
                    var xmlArtImage = xmlDoc.CreateElement("Image");
                    xmlArtImage.SetAttribute("Type", artImage.Type);
                    xmlArtImage.SetAttribute("Path", artImage.Path);
                    xmlSkinArt.AppendChild(xmlArtImage);
                }
                xmlWindow.AppendChild(xmlSkinArt);

                foreach (var command in window.Commands) {
                    var xmlCommand = xmlDoc.CreateElement("Command");
                    xmlCommand.SetAttribute("Event", command.Event);
                    xmlCommand.SetAttribute("Name", command.Name);
                    xmlWindow.AppendChild(xmlCommand);
                }

                foreach (var button in window.Buttons) {
                    var xmlButton = xmlDoc.CreateElement("SkinButton");
                    xmlButton.SetAttribute("Name", button.Name);
                    xmlButton.SetAttribute("TabID", button.TabID.ToString(CultureInfo.InvariantCulture));
                    xmlButton.SetAttribute("MapColor", button.MapColor);
                    if (button.Align != SkinTextAligh.no)
                        xmlButton.SetAttribute("Align", button.Align.ToString());
                    if (button.PlainText)
                        xmlButton.SetAttribute("PlainText", button.PlainText.ToString().ToLower());
                    if (!string.IsNullOrEmpty(button.Text))
                        xmlButton.SetAttribute("Text", button.Text);
                    if (!string.IsNullOrEmpty(button.Option))
                        xmlButton.SetAttribute("Option", button.Option);
                    if (!string.IsNullOrEmpty(button.Style))
                        xmlButton.SetAttribute("Style", button.Style);

                    if (button.Art.Images.Count > 0) {
                        xmlSkinArt = xmlDoc.CreateElement("SkinArt");
                        if (!string.IsNullOrEmpty(button.Art.BackgroundColor))
                            xmlSkinArt.SetAttribute("BackgroundColor", button.Art.BackgroundColor);
                        foreach (var artImage in button.Art.Images) {
                            var xmlArtImage = xmlDoc.CreateElement("Image");
                            xmlArtImage.SetAttribute("Type", artImage.Type);
                            xmlArtImage.SetAttribute("Path", artImage.Path);
                            xmlSkinArt.AppendChild(xmlArtImage);
                        }
                        xmlButton.AppendChild(xmlSkinArt);
                    }

                    foreach (var command in button.Commands) {
                        var xmlCommand = xmlDoc.CreateElement("Command");
                        xmlCommand.SetAttribute("Event", command.Event);
                        xmlCommand.SetAttribute("Name", command.Name);
                        if (!string.IsNullOrEmpty(command.Window))
                            xmlCommand.SetAttribute("Window", command.Window);
                        if (!string.IsNullOrEmpty(command.FileName))
                            xmlCommand.SetAttribute("FileName", command.FileName);
                        if (!string.IsNullOrEmpty(command.Url))
                            xmlCommand.SetAttribute("URL", command.Url);
                        xmlButton.AppendChild(xmlCommand);
                    }
                    xmlWindow.AppendChild(xmlButton);
                }

                foreach (var progressBar in window.ProgressBars) {
                    var xmlProgressBar = xmlDoc.CreateElement("SkinProgressBar");
                    xmlProgressBar.SetAttribute("MapColor", progressBar.MapColor);
                    xmlProgressBar.SetAttribute("Name", progressBar.Name);
                    xmlProgressBar.SetAttribute("Vertical", progressBar.Vertical.ToString().ToLower());

                    xmlSkinArt = xmlDoc.CreateElement("SkinArt");
                    if (!string.IsNullOrEmpty(progressBar.Art.BackgroundColor))
                        xmlSkinArt.SetAttribute("BackgroundColor", progressBar.Art.BackgroundColor);
                    foreach (var artImage in progressBar.Art.Images) {
                        var xmlArtImage = xmlDoc.CreateElement("Image");
                        xmlArtImage.SetAttribute("Type", artImage.Type);
                        xmlArtImage.SetAttribute("Path", artImage.Path);
                        xmlSkinArt.AppendChild(xmlArtImage);
                    }
                    xmlProgressBar.AppendChild(xmlSkinArt);

                    xmlWindow.AppendChild(xmlProgressBar);
                }

                foreach (var comboBox in window.ComboBoxes) {
                    var xmlComboBox = xmlDoc.CreateElement("SkinComboBox");
                    xmlComboBox.SetAttribute("MapColor", comboBox.MapColor);
                    xmlComboBox.SetAttribute("Name", comboBox.Name);
                    xmlComboBox.SetAttribute("Text", comboBox.Text);
                    xmlComboBox.SetAttribute("TabID", comboBox.TabID.ToString(CultureInfo.InvariantCulture));
                    if (comboBox.Align != SkinTextAligh.no)
                        xmlComboBox.SetAttribute("Align", comboBox.Align.ToString());
                    xmlComboBox.SetAttribute("Style", comboBox.Style);

                    xmlSkinArt = xmlDoc.CreateElement("SkinArt");
                    if (!string.IsNullOrEmpty(comboBox.Art.BackgroundColor))
                        xmlSkinArt.SetAttribute("BackgroundColor", comboBox.Art.BackgroundColor);
                    foreach (var artImage in comboBox.Art.Images) {
                        var xmlArtImage = xmlDoc.CreateElement("Image");
                        xmlArtImage.SetAttribute("Type", artImage.Type);
                        xmlArtImage.SetAttribute("Path", artImage.Path);
                        xmlSkinArt.AppendChild(xmlArtImage);
                    }
                    xmlComboBox.AppendChild(xmlSkinArt);

                    foreach (var command in comboBox.Commands) {
                        var xmlCommand = xmlDoc.CreateElement("Command");
                        xmlCommand.SetAttribute("Event", command.Event);
                        xmlCommand.SetAttribute("Name", command.Name);
                        if (!string.IsNullOrEmpty(command.FileName))
                            xmlCommand.SetAttribute("FileName", command.FileName);
                        if (!string.IsNullOrEmpty(command.Url))
                            xmlCommand.SetAttribute("URL", command.Url);
                        xmlComboBox.AppendChild(xmlCommand);
                    }

                    xmlWindow.AppendChild(xmlComboBox);
                }

                foreach (var listBox in window.ListBoxes) {
                    var xmlListBox = xmlDoc.CreateElement("SkinListBox");
                    xmlListBox.SetAttribute("MapColor", listBox.MapColor);
                    xmlListBox.SetAttribute("Name", listBox.Name);
                    xmlListBox.SetAttribute("TabID", listBox.TabID.ToString(CultureInfo.InvariantCulture));
                    xmlListBox.SetAttribute("ListFile", listBox.ListFile);
                    xmlListBox.SetAttribute("Option", listBox.Option);
                    xmlListBox.SetAttribute("ComboBox", listBox.ComboBox);
                    xmlListBox.SetAttribute("DefaultSel", listBox.DefaultSel);
                    xmlListBox.SetAttribute("bgSelected", listBox.BgSelected);
                    xmlListBox.SetAttribute("bgHover", listBox.BgHover);
                    if (listBox.Align != SkinTextAligh.no)
                        xmlListBox.SetAttribute("Align", listBox.Align.ToString());
                    xmlListBox.SetAttribute("DrawLevel", listBox.DrawLevel.ToString(CultureInfo.InvariantCulture));
                    xmlListBox.SetAttribute("Style", listBox.Style);

                    xmlSkinArt = xmlDoc.CreateElement("SkinArt");
                    if (!string.IsNullOrEmpty(listBox.Art.BackgroundColor))
                        xmlSkinArt.SetAttribute("BackgroundColor", listBox.Art.BackgroundColor);
                    foreach (var artImage in listBox.Art.Images) {
                        var xmlArtImage = xmlDoc.CreateElement("Image");
                        xmlArtImage.SetAttribute("Type", artImage.Type);
                        xmlArtImage.SetAttribute("Path", artImage.Path);
                        xmlSkinArt.AppendChild(xmlArtImage);
                    }
                    xmlListBox.AppendChild(xmlSkinArt);

                    xmlWindow.AppendChild(xmlListBox);
                }

                foreach (var scrollBar in window.ScrollBars) {
                    var xmlScrollBar = xmlDoc.CreateElement("SkinScrollBar");
                    xmlScrollBar.SetAttribute("Name", scrollBar.Name);
                    xmlScrollBar.SetAttribute("MapColor", scrollBar.MapColor);
                    xmlScrollBar.SetAttribute("TabID", scrollBar.TabID.ToString(CultureInfo.InvariantCulture));
                    xmlScrollBar.SetAttribute("DrawLevel", scrollBar.DrawLevel.ToString(CultureInfo.InvariantCulture));
                    xmlScrollBar.SetAttribute("ListBox", scrollBar.ListBox);

                    xmlSkinArt = xmlDoc.CreateElement("SkinArt");
                    if (!string.IsNullOrEmpty(scrollBar.Art.BackgroundColor))
                        xmlSkinArt.SetAttribute("BackgroundColor", scrollBar.Art.BackgroundColor);
                    foreach (var artImage in scrollBar.Art.Images) {
                        var xmlArtImage = xmlDoc.CreateElement("Image");
                        xmlArtImage.SetAttribute("Type", artImage.Type);
                        xmlArtImage.SetAttribute("Path", artImage.Path);
                        xmlSkinArt.AppendChild(xmlArtImage);
                    }
                    xmlScrollBar.AppendChild(xmlSkinArt);

                    xmlWindow.AppendChild(xmlScrollBar);
                }

                foreach (var scrollArrow in window.ScrollArrows) {
                    var xmlScrollBar = xmlDoc.CreateElement("SkinScrollArrow");
                    xmlScrollBar.SetAttribute("Name", scrollArrow.Name);
                    xmlScrollBar.SetAttribute("MapColor", scrollArrow.MapColor);
                    xmlScrollBar.SetAttribute("TabID", scrollArrow.TabID.ToString(CultureInfo.InvariantCulture));
                    xmlScrollBar.SetAttribute("DrawLevel", scrollArrow.DrawLevel.ToString(CultureInfo.InvariantCulture));
                    xmlScrollBar.SetAttribute("ListBox", scrollArrow.ListBox);
                    xmlScrollBar.SetAttribute("Type", scrollArrow.Type);

                    xmlSkinArt = xmlDoc.CreateElement("SkinArt");
                    if (!string.IsNullOrEmpty(scrollArrow.Art.BackgroundColor))
                        xmlSkinArt.SetAttribute("BackgroundColor", scrollArrow.Art.BackgroundColor);
                    foreach (var artImage in scrollArrow.Art.Images) {
                        var xmlArtImage = xmlDoc.CreateElement("Image");
                        xmlArtImage.SetAttribute("Type", artImage.Type);
                        xmlArtImage.SetAttribute("Path", artImage.Path);
                        xmlSkinArt.AppendChild(xmlArtImage);
                    }
                    xmlScrollBar.AppendChild(xmlSkinArt);

                    xmlWindow.AppendChild(xmlScrollBar);
                }

                foreach (var slider in window.Sliders) {
                    var xmlSlider = xmlDoc.CreateElement("SkinSlider");
                    xmlSlider.SetAttribute("Name", slider.Name);
                    xmlSlider.SetAttribute("MapColor", slider.MapColor);
                    xmlSlider.SetAttribute("Min", slider.Min.ToString(CultureInfo.InvariantCulture));
                    xmlSlider.SetAttribute("Max", slider.Max.ToString(CultureInfo.InvariantCulture));
                    xmlSlider.SetAttribute("DrawAllEx", slider.DrawAllEx.ToString().ToLower());
                    xmlSlider.SetAttribute("ExpandWidth", slider.ExpandWidth.ToString(CultureInfo.InvariantCulture));
                    xmlSlider.SetAttribute("Option", slider.Option);

                    xmlSkinArt = xmlDoc.CreateElement("SkinArt");
                    if (!string.IsNullOrEmpty(slider.Art.BackgroundColor))
                        xmlSkinArt.SetAttribute("BackgroundColor", slider.Art.BackgroundColor);
                    foreach (var artImage in slider.Art.Images) {
                        var xmlArtImage = xmlDoc.CreateElement("Image");
                        xmlArtImage.SetAttribute("Type", artImage.Type);
                        xmlArtImage.SetAttribute("Path", artImage.Path);
                        xmlSkinArt.AppendChild(xmlArtImage);
                    }
                    xmlSlider.AppendChild(xmlSkinArt);

                    xmlWindow.AppendChild(xmlSlider);
                }

                foreach (var browser in window.Browsers) {
                    var xmlBrowser = xmlDoc.CreateElement("SkinBrowser");
                    xmlBrowser.SetAttribute("Name", browser.Name);
                    xmlBrowser.SetAttribute("MapColor", browser.MapColor);
                    xmlBrowser.SetAttribute("NoBorder", browser.NoBorder.ToString(CultureInfo.InvariantCulture).ToLower());
                    xmlBrowser.SetAttribute("InitURL", browser.InitUrl);
                    xmlWindow.AppendChild(xmlBrowser);
                }

                xmlSkinApp.AppendChild(xmlWindow);
                AppProgressBarController.Hide();
            }

            XmlEditor.Text = Core.PrettyXml(xmlDoc.InnerXml);
            File.WriteAllText(FileName, XmlEditor.Text, Encoding.GetEncoding("utf-16LE"));
        }
    }
}