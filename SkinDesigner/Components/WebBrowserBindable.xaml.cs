using System;
using System.Windows;

namespace SkinDesigner.Components {
    public partial class WebBrowserBindable {
        public WebBrowserBindable() {
            InitializeComponent();
        }

        /// <summary>
        /// Identifies the <see>
        ///                    <cref>NavigationUrl</cref>
        ///                </see>
        ///     dependency property.
        /// </summary>
        public static DependencyProperty NavigationUrlProperty = DependencyProperty.Register("NavigationUrl", typeof(string), typeof(WebBrowserBindable));
        public string NavigationUrl {
            get { 
                return (string) GetValue(NavigationUrlProperty);
            }
            set {
                SetValue(NavigationUrlProperty, value);
            }
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
            base.OnPropertyChanged(e);
            if (e.Property == NavigationUrlProperty) {
                if (!string.IsNullOrEmpty(e.NewValue as string)) {
                    Uri uri;
                    Uri.TryCreate(e.NewValue as string, UriKind.Absolute, out uri);
                    Browser.Source = uri;
                }
            }
        }
    }
}
