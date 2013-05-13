using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using SkinDesigner.Components;
using SkinDesigner.Controllers;
using SkinDesigner.Models;
using SkinDesigner.Utils;
using Xceed.Wpf.Toolkit.PropertyGrid;

namespace SkinDesigner.Windows {
    public partial class MainWindow {
        public SkinController SkinController { get; set; }
        public ExceptionController ExceptionController { get; set; }
        public AppProgressBarController AppProgressBarController { get; set; }

        public MainWindow() {
            ExceptionController = new ExceptionController();
            Dispatcher.UnhandledException += ApplicationOnDispatcherUnhandledException;

            InitializeComponent();

            AppProgressBarController = new AppProgressBarController(ProgressForm, AppProgressBar, AppProgressBarLabel);
            SkinController = new SkinController(CanvasPreview, TeFile, AppProgressBarController);
            CanvasPreview.MouseMove += CanvasPreviewOnMouseMove;
            CanvasPreview.MouseDown += CanvasPreviewOnPreviewMouseDown;
            CanvasPreview.PreviewMouseUp += CanvasPreviewOnPreviewMouseUp;
            TvSkinProject.SelectedItemChanged += TvSkinProjectOnSelectedItemChanged;
            PgElement.PropertyValueChanged += PgElementOnPropertyValueChanged;
        }

        private void PgElementOnPropertyValueChanged(object sender, PropertyValueChangedEventArgs e) {
            var skinElement = (SkinElement) PgElement.SelectedObject;
            if (SkinController.SelectedWindow != null &&
                SkinController.SelectedWindow.ColorPositions.Any(t => t.MapColor == Core.MapColor2Color(SkinController.SelectedElement.SkinElement.MapColor))) {
                var pos = SkinController.SelectedWindow.ColorPositions.First(t => t.MapColor == Core.MapColor2Color(SkinController.SelectedElement.SkinElement.MapColor));
                switch (((PropertyItem) e.OriginalSource).DisplayName) {
                    case "X": pos.X = (double) e.NewValue; break;
                    case "Y": pos.Y = (double) e.NewValue; break;
                    case "Width": pos.Width = (double) e.NewValue; break;
                    case "Height": pos.Height = (double) e.NewValue; break;
                }
            }
            SkinController.DrawElement(skinElement);
        }

        private void TvSkinProjectOnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e) {
            if (e.NewValue == null) {
                PgElement.SelectedObject = null;
            } else {
                var item = (TreeViewItemSkinElement) e.NewValue;
                if (item.SkinElement.GetType() == typeof (SkinWindow)) SkinController.SelectedWindow = (SkinWindow) item.SkinElement;
                PgElement.SelectedObject = item.SkinElement.IsFolder ? null : item.SkinElement;
                SkinController.DrawElement(item);
            }
        }

        private void ApplicationOnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e) {
            e.Handled = true;

            ExceptionController = ExceptionController ?? new ExceptionController();
            ExceptionController.ShowException(e.Exception);
        }

        private void GenTestElements() {
            CanvasPreview.Children.Add(
                    new SkinElementControl(
                            SkinController,
                            new SkinButton {
                                Name = "TestButton1"
                            }
                        )
                );
        }

        private void CanvasPreviewOnPreviewMouseDown(object sender, MouseButtonEventArgs e) {
            if (SkinController.SelectedElement != null) {
                PgElement.SelectedObject = null;
                PgElement.SelectedObject = SkinController.SelectedElement.SkinElement;
            }
        }

        private void CanvasPreviewOnPreviewMouseUp(object sender, MouseButtonEventArgs e) {
            if (SkinController.SelectedElement != null) {
                PgElement.SelectedObject = null;
                PgElement.SelectedObject = SkinController.SelectedElement.SkinElement;
            }
        }

        private void CanvasPreviewOnMouseMove(object sender, MouseEventArgs e) {
            TbStatusText.Text = string.Format("X: {0}, Y: {1:0}", e.GetPosition(CanvasPreview).X, e.GetPosition(CanvasPreview).Y);
            if (e.LeftButton != MouseButtonState.Pressed) return;
            if (SkinController.SelectedElement != null) {
                PgElement.SelectedObject = null;
                PgElement.SelectedObject = SkinController.SelectedElement.SkinElement;
            }
        }

        private void BtnGotoGithub_OnClick(object sender, RoutedEventArgs e) {
            System.Diagnostics.Process.Start("https://github.com/perfectdev/");
        }

        private void BtnOpen_OnClick(object sender, RoutedEventArgs e) {
            SkinController.LoadSkin();
            TvSkinProject.ItemsSource = null;
            TvSkinProject.ItemsSource = SkinController.GetTreeViewItemsSource();
        }

        private void ChkPreviewMode_OnChecked(object sender, RoutedEventArgs e) {
            SkinController.IsPreviewMode = true;
        }

        private void ChkPreviewMode_OnUnchecked(object sender, RoutedEventArgs e) {
            SkinController.IsPreviewMode = false;
        }

        private void BtnSave_OnClick(object sender, RoutedEventArgs e) {
            SkinController.Save();
            TcEditors.SelectedIndex = 1;
        }
    }
}
