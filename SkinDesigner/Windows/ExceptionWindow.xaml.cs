using System;
using System.Windows;

namespace SkinDesigner.Windows {
    public partial class ExceptionWindow {
        public ExceptionWindow() {
            InitializeComponent();
        }

        private void CloseClick(object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        public void SetException(Exception exception) {
            TxtMessage.Text = exception.Message;
            TxtStackTrace.Text = exception.StackTrace;
        }
    }
}
