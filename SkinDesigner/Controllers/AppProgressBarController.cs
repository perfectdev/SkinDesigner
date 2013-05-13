using System.Timers;
using System.Windows;
using System.Windows.Controls;
using SkinDesigner.Utils;

namespace SkinDesigner.Controllers {
    public class AppProgressBarController {
        public Grid ProgressForm { get; set; }
        public TextBlock ProgressText { get; set; }
        public ProgressBar ProgressBar { get; set; }
        protected RefreshTimer RefreshTimer;

        public AppProgressBarController(Grid form, ProgressBar bar, TextBlock text) {
            ProgressForm = form;
            ProgressText = text;
            ProgressBar = bar;
            RefreshTimer = new RefreshTimer { AutoReset = true, Interval = 100, Object = this };
            RefreshTimer.Elapsed += TimerElapsed;
        }

        public void EnableTimer() {
            RefreshTimer.Enabled = true;
        }

        public void DisableTimer() {
            RefreshTimer.Enabled = false;
        }

        public void DoEvents() {
            Core.DoEvents();
        }

        private void TimerElapsed(object sender, ElapsedEventArgs e) {
            DoEvents();
        }

        public void Show() {
            ProgressForm.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = false;
            Core.DoEvents();
        }

        public void ShowIndeterminate() {
            ProgressForm.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = true;
            //EnableTimer();
            Core.DoEvents();
        }

        public void Hide() {
            ProgressForm.Visibility = Visibility.Hidden;
            Core.DoEvents();
            RefreshTimer.Dispose();
        }

        public void Clear() {
            ProgressBar.Value = 0;
            ProgressText.Text = "";
            Core.DoEvents();
        }

        public void SetValue(double value, double maximum, string text) {
            ProgressBar.Maximum = maximum;
            ProgressBar.Value = value;
            ProgressText.Text = text;
            Core.DoEvents();
        }
    }
}
