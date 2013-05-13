using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SkinDesigner.Controllers;
using SkinDesigner.Models;
using SkinDesigner.Utils;

namespace SkinDesigner.Components {
    public class SkinElementControl :Thumb {
        public bool IsResizeMode { get; private set; }
        public SkinController SkinController { get; private set; }
        public SkinElement SkinElement { get; set; }

        public SkinElementControl(SkinController skinController, SkinElement skinElement) {
            SkinController = skinController;
            SkinElement = skinElement;
            Focusable = true;
            Canvas.SetLeft(this, skinElement.X);
            Canvas.SetTop(this, skinElement.Y);
            Template = (ControlTemplate) Application.Current.TryFindResource("ThumbTemplate");
            PreviewMouseDown += OnPreviewMouseDown;
            PreviewMouseUp += OnPreviewMouseUp;
            PreviewKeyDown += OnPreviewKeyDown;
            PreviewKeyUp += OnPreviewKeyUp;
            DragDelta += OnDragDelta;
            MouseEnter += OnMouseEnter;
            MouseLeave += OnMouseLeave;
            DrawNormal();
        }

        public void DrawNormal() {
            switch (SkinElement.GetType().Name) {
                case "SkinButton": DrawButton(SkinButtonState.Normal); break;
                case "SkinComboBox": DrawComboBox(SkinButtonState.Normal); break;
                case "SkinProgressBar": DrawProgressBar(SkinButtonState.Normal); break;
                case "SkinBrowser": DrawBrowser(SkinButtonState.Normal); break;
                case "SkinSlider": DrawSlider(SkinButtonState.Normal); break;
            }
        }

        public void DrawFocused() {
            switch (SkinElement.GetType().Name) {
                case "SkinButton": DrawButton(SkinButtonState.Focused); break;
                case "SkinComboBox": DrawComboBox(SkinButtonState.Focused); break;
                case "SkinProgressBar": DrawProgressBar(SkinButtonState.Focused); break;
                case "SkinSlider": DrawSlider(SkinButtonState.Focused); break;
            }
        }

        private void DrawButton(SkinButtonState state) {
            var button = (SkinButton) SkinElement;
            var art = new SkinArtImage();
            switch (state) {
                case SkinButtonState.Normal:
                    art = button.Art.Images.Any(t => t.Type == "BackgroundImage") ? button.Art.Images.First(t => t.Type == "BackgroundImage") : new SkinArtImage();
                    break;
                case SkinButtonState.Clicked:
                    if (button.Art.Images.Any(t => t.Type == "SelectedImage" && !string.IsNullOrEmpty(t.Type))) {
                        art = button.Art.Images.First(t => t.Type == "SelectedImage");
                    } else {
                        art = button.Art.Images.Any(t => t.Type == "ClickedImage" && !string.IsNullOrEmpty(t.Type))
                            ? button.Art.Images.First(t => t.Type == "ClickedImage")
                            : (button.Art.Images.Any(t => t.Type == "BackgroundImage") ? button.Art.Images.First(t => t.Type == "BackgroundImage") : new SkinArtImage());
                    }
                    break;
                case SkinButtonState.Focused:
                    art = button.Art.Images.Any(t => t.Type == "HoverImage" && !string.IsNullOrEmpty(t.Type))
                        ? button.Art.Images.First(t => t.Type == "HoverImage")
                        : (button.Art.Images.Any(t => t.Type == "BackgroundImage") ? button.Art.Images.First(t => t.Type == "BackgroundImage") : new SkinArtImage());
                    break;
            }
            var img = (BitmapImage) Core.GetImageSourceFromFileName(SkinController.GetFullPath(art.Path));

            //+ Формирование шаблона контрола
            var xamlGrid = new FrameworkElementFactory(typeof(Grid));
            var xamlImage = new FrameworkElementFactory(typeof(Image));
            xamlImage.SetValue(WidthProperty, (double) img.PixelWidth);
            xamlImage.SetValue(HeightProperty, (double) img.PixelHeight);
            xamlImage.SetValue(Image.SourceProperty, img);
            xamlGrid.AppendChild(xamlImage);
            if (!string.IsNullOrEmpty(button.Text)) {
                var xamlTextBlock = new FrameworkElementFactory(typeof(TextBlock));
                xamlTextBlock.SetValue(TextBlock.TextProperty, string.IsNullOrEmpty(button.Text) ? button.Name : button.Text);
                var hAlign = HorizontalAlignment.Center;
                switch (button.Align) {
                    case SkinTextAligh.left: hAlign = HorizontalAlignment.Left; break;
                    case SkinTextAligh.right: hAlign = HorizontalAlignment.Right; break;
                }
                xamlTextBlock.SetValue(HorizontalAlignmentProperty, hAlign);
                xamlTextBlock.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
                if (SkinController.SkinApp.Styles.Any(t=>t.Name == button.Style)) {
                    var textStyle = SkinController.SkinApp.Styles.First(t => t.Name == button.Style);
                    xamlTextBlock.SetValue(TextBlock.FontSizeProperty, textStyle.Height);
                    if (textStyle.Weight > 0)
                        xamlTextBlock.SetValue(TextBlock.FontWeightProperty, FontWeight.FromOpenTypeWeight((int) textStyle.Weight));
                    if (Fonts.SystemFontFamilies.Any(t => t.GetTypefaces().Any(f=>f.FaceNames.Any(fn=>fn.Key.ToString().ToLower() == textStyle.Face.ToLower()))))
                        xamlTextBlock.SetValue(TextBlock.FontFamilyProperty, Fonts.SystemFontFamilies.First(t => t.GetTypefaces().Any(f => f.FaceNames.Any(fn => fn.Key.ToString().ToLower() == textStyle.Face.ToLower()))));
                    xamlTextBlock.SetValue(TextBlock.ForegroundProperty, Core.GetColorBrushFromString(textStyle.Default));
                }
                xamlGrid.AppendChild(xamlTextBlock);
            }
            Template = new ControlTemplate { VisualTree = xamlGrid };
            //- Формирование шаблона контрола
        }

        private void DrawSlider(SkinButtonState state) {
            var slider = (SkinSlider) SkinElement;
            var art = new SkinArtImage();
            var art2 = new SkinArtImage();
            switch (state) {
                case SkinButtonState.Normal:
                    art = slider.Art.Images.Any(t => t.Type == "BackgroundImage") ? slider.Art.Images.First(t => t.Type == "BackgroundImage") : new SkinArtImage();
                    art2 = slider.Art.Images.Any(t => t.Type == "FocusedImage") ? slider.Art.Images.First(t => t.Type == "FocusedImage") : new SkinArtImage();
                    break;
                case SkinButtonState.Clicked:
                    art = slider.Art.Images.Any(t => t.Type == "BackgroundImage") ? slider.Art.Images.First(t => t.Type == "BackgroundImage") : new SkinArtImage();
                    art2 = slider.Art.Images.Any(t => t.Type == "FocusedImage") ? slider.Art.Images.First(t => t.Type == "FocusedImage") : new SkinArtImage();
                    break;
                case SkinButtonState.Focused:
                    art = slider.Art.Images.Any(t => t.Type == "BackgroundImage") ? slider.Art.Images.First(t => t.Type == "BackgroundImage") : new SkinArtImage();
                    art2 = slider.Art.Images.Any(t => t.Type == "FocusedImage") ? slider.Art.Images.First(t => t.Type == "FocusedImage") : new SkinArtImage();
                    break;
            }
            var img = (BitmapImage) Core.GetImageSourceFromFileName(SkinController.GetFullPath(art.Path));
            var img2 = (BitmapImage) Core.GetImageSourceFromFileName(SkinController.GetFullPath(art2.Path));

            //+ Формирование шаблона контрола
            var xamlGrid = new FrameworkElementFactory(typeof(Grid));
            var xamlBorder = new FrameworkElementFactory(typeof(Border));
            var xamlImage = new FrameworkElementFactory(typeof(Image));
            xamlImage.SetValue(WidthProperty, (double) img.PixelWidth);
            xamlImage.SetValue(HeightProperty, (double) img.PixelHeight);
            xamlImage.SetValue(Image.SourceProperty, img);
            xamlGrid.AppendChild(xamlImage);
            xamlBorder.AppendChild(xamlGrid);
            var xamlImage2 = new FrameworkElementFactory(typeof(Image));
            xamlImage2.SetValue(Image.SourceProperty, img2);
            xamlImage2.SetValue(HorizontalAlignmentProperty, state == SkinButtonState.Clicked ? HorizontalAlignment.Right : HorizontalAlignment.Center);
            xamlImage2.SetValue(WidthProperty, (double) img2.PixelWidth);
            xamlGrid.AppendChild(xamlImage2);
            Template = new ControlTemplate { VisualTree = xamlBorder };
            //- Формирование шаблона контрола
        }

        private void DrawComboBox(SkinButtonState state) {
            var comboBox = (SkinComboBox) SkinElement;
            var art = new SkinArtImage();
            switch (state) {
                case SkinButtonState.Normal:
                    art = comboBox.Art.Images.Any(t => t.Type == "BackgroundImage") ? comboBox.Art.Images.First(t => t.Type == "BackgroundImage") : new SkinArtImage();
                    break;
                case SkinButtonState.Clicked:
                    if (comboBox.Art.Images.Any(t => t.Type == "SelectedImage" && !string.IsNullOrEmpty(t.Type))) {
                        art = comboBox.Art.Images.First(t => t.Type == "SelectedImage");
                    } else {
                        art = comboBox.Art.Images.Any(t => t.Type == "ClickedImage" && !string.IsNullOrEmpty(t.Type))
                            ? comboBox.Art.Images.First(t => t.Type == "ClickedImage")
                            : (comboBox.Art.Images.Any(t => t.Type == "BackgroundImage") ? comboBox.Art.Images.First(t => t.Type == "BackgroundImage") : new SkinArtImage());
                    }
                    break;
                case SkinButtonState.Focused:
                    art = comboBox.Art.Images.Any(t => t.Type == "HoverImage" && !string.IsNullOrEmpty(t.Type)) 
                        ? comboBox.Art.Images.First(t => t.Type == "HoverImage")
                        : (comboBox.Art.Images.Any(t => t.Type == "BackgroundImage") ? comboBox.Art.Images.First(t => t.Type == "BackgroundImage") : new SkinArtImage());
                    break;
            }
            var img = (BitmapImage) Core.GetImageSourceFromFileName(SkinController.GetFullPath(art.Path));

            //+ Формирование шаблона контрола
            var xamlGrid = new FrameworkElementFactory(typeof(Grid));
            var xamlBorder = new FrameworkElementFactory(typeof(Border));
            var xamlImage = new FrameworkElementFactory(typeof(Image));
            xamlImage.SetValue(WidthProperty, (double) img.PixelWidth);
            xamlImage.SetValue(HeightProperty, (double) img.PixelHeight);
            xamlImage.SetValue(Image.SourceProperty, img);
            xamlGrid.AppendChild(xamlImage);
            if (!string.IsNullOrEmpty(comboBox.Text)) {
                var xamlTextBlock = new FrameworkElementFactory(typeof(TextBlock));
                xamlTextBlock.SetValue(TextBlock.TextProperty, comboBox.Text);
                xamlTextBlock.SetValue(HorizontalAlignmentProperty, HorizontalAlignment.Center);
                xamlTextBlock.SetValue(VerticalAlignmentProperty, VerticalAlignment.Center);
                if (SkinController.SkinApp.Styles.Any(t => t.Name == comboBox.Style)) {
                    var textStyle = SkinController.SkinApp.Styles.First(t => t.Name == comboBox.Style);
                    xamlTextBlock.SetValue(TextBlock.FontSizeProperty, textStyle.Height);
                    if (textStyle.Weight > 0)
                        xamlTextBlock.SetValue(TextBlock.FontWeightProperty, textStyle.Weight);
                    xamlTextBlock.SetValue(TextBlock.FontFamilyProperty, new FontFamily(textStyle.Face));
                    xamlTextBlock.SetValue(TextBlock.ForegroundProperty, Core.GetColorBrushFromString(textStyle.Default));
                }
                xamlGrid.AppendChild(xamlTextBlock);
            }
            xamlBorder.AppendChild(xamlGrid);
            Template = new ControlTemplate { VisualTree = xamlBorder };
            //- Формирование шаблона контрола
        }

        private void DrawProgressBar(SkinButtonState state) {
            var progressBar = (SkinProgressBar) SkinElement;
            var art = new SkinArtImage();
            switch (state) {
                case SkinButtonState.Normal:
                    art = progressBar.Art.Images.Any(t => t.Type == "UnfilledImage") ? progressBar.Art.Images.First(t => t.Type == "UnfilledImage") : new SkinArtImage();
                    break;
                case SkinButtonState.Focused:
                    art = progressBar.Art.Images.Any(t => t.Type == "FilledImage" && !string.IsNullOrEmpty(t.Type))
                        ? progressBar.Art.Images.First(t => t.Type == "FilledImage")
                        : (progressBar.Art.Images.Any(t => t.Type == "UnfilledImage") ? progressBar.Art.Images.First(t => t.Type == "UnfilledImage") : new SkinArtImage());
                    break;
            }
            var img = (BitmapImage) Core.GetImageSourceFromFileName(SkinController.GetFullPath(art.Path));

            //+ Формирование шаблона контрола
            var xamlGrid = new FrameworkElementFactory(typeof(Grid));
            var xamlBorder = new FrameworkElementFactory(typeof(Border));
            var xamlImage = new FrameworkElementFactory(typeof(Image));
            xamlImage.SetValue(WidthProperty, (double) img.PixelWidth);
            xamlImage.SetValue(HeightProperty, (double) img.PixelHeight);
            xamlImage.SetValue(Image.SourceProperty, img);
            xamlGrid.AppendChild(xamlImage);
            xamlBorder.AppendChild(xamlGrid);
            Template = new ControlTemplate { VisualTree = xamlBorder };
            //- Формирование шаблона контрола
        }

        private void DrawBrowser(SkinButtonState state) {
            var browser = (SkinBrowser) SkinElement;
            var pos = SkinController.SelectedWindow.ColorPositions.Any(t => t.MapColor == Core.MapColor2Color(browser.MapColor))
                ? SkinController.SelectedWindow.ColorPositions.First(t => t.MapColor == Core.MapColor2Color(browser.MapColor))
                : new ColorPosition();

            //+ Формирование шаблона контрола
            var xamlGrid = new FrameworkElementFactory(typeof(Grid));
            var xamlBorder = new FrameworkElementFactory(typeof(Border));
            xamlBorder.SetValue(Border.BorderThicknessProperty, browser.NoBorder ? new Thickness(0) : new Thickness(2));
            var xamlWebBrowser = new FrameworkElementFactory(typeof(WebBrowserBindable));
            xamlWebBrowser.SetValue(WebBrowserBindable.NavigationUrlProperty, browser.InitUrl);
            xamlWebBrowser.SetValue(WidthProperty, pos.Width);
            xamlWebBrowser.SetValue(HeightProperty, pos.Height);
            xamlGrid.AppendChild(xamlWebBrowser);
            xamlBorder.AppendChild(xamlGrid);
            Template = new ControlTemplate { VisualTree = xamlBorder };
            //- Формирование шаблона контрола
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e) {
            if (e.Key == Key.LeftShift)
                IsResizeMode = true;
        }

        private void OnPreviewKeyUp(object sender, KeyEventArgs keyEventArgs) {
            IsResizeMode = false;
        }

        private void OnDragDelta(object sender, DragDeltaEventArgs e) {
            var thumb = e.Source as SkinElementControl;
            if (thumb == null) return;

            var left = Canvas.GetLeft(thumb) + e.HorizontalChange;
            var top = Canvas.GetTop(thumb) + e.VerticalChange;
            if (IsResizeMode) {
                /*var width = thumb.RenderSize.Width + (e.HorizontalChange > 0 ? 1 : -1);
                var height = thumb.RenderSize.Height + (e.VerticalChange > 0 ? 1 : -1);
                thumb.Width = width > 10 ? width : 10;
                thumb.Height = height > 10 ? height : 10;*/
            } else {
                Canvas.SetLeft(thumb, left);
                Canvas.SetTop(thumb, top);
                SkinController.SelectedElement.SkinElement.X = left;
                SkinController.SelectedElement.SkinElement.Y = top;
                if (SkinController.SelectedWindow != null &&
                    SkinController.SelectedWindow.ColorPositions.Any(t=>t.MapColor == Core.MapColor2Color(SkinController.SelectedElement.SkinElement.MapColor))) {
                    var pos = SkinController.SelectedWindow.ColorPositions.First(t=>t.MapColor == Core.MapColor2Color(SkinController.SelectedElement.SkinElement.MapColor));
                    pos.X = left;
                    pos.Y = top;
                }
            }
        }

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e) {
            SkinController.SelectElement(this);

            if (SkinController.IsPreviewMode) {
                switch (SkinElement.GetType().Name) {
                    case "SkinButton":
                        var button = (SkinButton) SkinElement;
                        DrawButton(SkinButtonState.Clicked);
                        if (button.Commands.Any(t => t.Name == "PlaySound"))
                            Core.PlaySound(SkinController.GetFullPath(button.Commands.First(t => t.Name == "PlaySound").FileName));
                        if (button.Commands.Any(t => !string.IsNullOrEmpty(t.Url)))
                            System.Diagnostics.Process.Start(button.Commands.First(t => !string.IsNullOrEmpty(t.Url)).Url);
                        break;
                    case "SkinComboBox":
                        var comboBox = (SkinComboBox) SkinElement;
                        DrawComboBox(SkinButtonState.Clicked);
                        if (comboBox.Commands.Any(t => t.Name == "PlaySound"))
                            Core.PlaySound(SkinController.GetFullPath(comboBox.Commands.First(t => t.Name == "PlaySound").FileName));
                        break;
                    case "SkinSlider":
                        DrawSlider(SkinButtonState.Clicked);
                        break;
                }
            } else {
                if (e.LeftButton == MouseButtonState.Pressed)
                    Cursor = Cursors.SizeAll;

            }
        }

        private void OnPreviewMouseUp(object sender, MouseButtonEventArgs e) {
            Cursor = Cursors.Arrow;
            if (SkinController.IsPreviewMode) {
                switch (SkinElement.GetType().Name) {
                    case "SkinButton": DrawButton(SkinButtonState.Normal); break;
                    case "SkinComboBox": DrawComboBox(SkinButtonState.Normal); break;
                    case "SkinSlider": DrawSlider(SkinButtonState.Normal); break;
                }
            }
        }

        private void OnMouseEnter(object sender, MouseEventArgs e) {
            if (SkinController.IsPreviewMode) {
                switch (SkinElement.GetType().Name) {
                    case "SkinButton": DrawButton(SkinButtonState.Focused); break;
                    case "SkinComboBox": DrawComboBox(SkinButtonState.Focused); break;
                    case "SkinProgressBar": DrawProgressBar(SkinButtonState.Focused); break;
                    case "SkinSlider": DrawSlider(SkinButtonState.Focused); break;
                }
            }
        }

        private void OnMouseLeave(object sender, MouseEventArgs e) {
            if (SkinController.IsPreviewMode) {
                switch (SkinElement.GetType().Name) {
                    case "SkinButton": DrawButton(SkinButtonState.Normal); break;
                    case "SkinComboBox": DrawComboBox(SkinButtonState.Normal); break;
                    case "SkinProgressBar": DrawProgressBar(SkinButtonState.Normal); break;
                    case "SkinSlider": DrawSlider(SkinButtonState.Normal); break;
                }
            }
        }
    }
}
