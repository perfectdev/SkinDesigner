using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;
using Color = System.Drawing.Color;

namespace SkinDesigner.Utils {
    public class Core {
        [DllImport("kernel32.dll")]
        public static extern bool SetProcessWorkingSetSize(IntPtr handle, int minimumWorkingSetSize, int maximumWorkingSetSize);

        public static void ClearMemory() {
            GC.Collect();
            SetProcessWorkingSetSize(System.Diagnostics.Process.GetCurrentProcess().Handle, -1, -1);
        }

        public static void DoEvents() {
            var f = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
                    (SendOrPostCallback) delegate(object arg) {
                        var fr = arg as DispatcherFrame;
                        if (fr != null) fr.Continue = false;
                    }, 
                    f
                );
            Dispatcher.PushFrame(f);
        }

        public static Color MapColor2Color(string mapColor) {
            if (string.IsNullOrEmpty(mapColor)) return Color.FromKnownColor(KnownColor.Transparent);
            var rgb = mapColor.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            return rgb.Length == 3 ? Color.FromArgb(255, Convert.ToByte(rgb[0]), Convert.ToByte(rgb[1]), Convert.ToByte(rgb[2])) : Color.FromKnownColor(KnownColor.Transparent);
        }

        public static ImageSource GetImageSourceFromFileName(string fileName) {
            if (string.IsNullOrEmpty(fileName)) return new BitmapImage();

            if (File.Exists(fileName)) {
                var img = new BitmapImage();
                using (var ms = new FileStream(fileName, FileMode.Open)) {
                    img.BeginInit();
                    img.StreamSource = ms;
                    img.CacheOption = BitmapCacheOption.OnLoad;
                    img.EndInit();
                }
                return img;
            }
            using (var memory = new MemoryStream()) {
                Properties.Resources.transparent.Save(memory, ImageFormat.Png);
                memory.Position = 0;
                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                return bitmapImage;
            }
        }

        public static Bitmap BitmapImage2Bitmap(BitmapImage bitmapImage) {
            using (var outStream = new MemoryStream()) {
                BitmapEncoder enc = new BmpBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(bitmapImage));
                enc.Save(outStream);
                var bitmap = new Bitmap(outStream);
                return new Bitmap(bitmap);
            }
        }

        public static ImageSource TrueStretchImage(string fileName, double targetWidth, double targetHeight) {
            if (string.IsNullOrEmpty(fileName)) return new BitmapImage();
            if (targetWidth == 0 || targetHeight == 0) return new BitmapImage();
            GetImageSourceFromFileName(fileName);
            var pngFileName = fileName.Replace(".dds", ".png").
                Replace(".DDS", ".png").
                Replace(".tga", ".png").
                Replace(".TGA", ".png");
            if (!File.Exists(pngFileName)) return new BitmapImage();
            var sourceImage = Image.FromFile(pngFileName);
            var targetImage = new BitmapImage();

            var img = new Bitmap((int) targetWidth, (int) targetHeight);
            using (var g = Graphics.FromImage(img)) {
                /*var backgroundImage = new Bitmap(1, 1);
                using (var tg = Graphics.FromImage(backgroundImage)) {
                    tg.DrawImage(
                        sourceImage,
                        new Rectangle(0, 0, 1, 1),
                        new Rectangle(sourceImage.Width / 2, sourceImage.Height / 2, 1, 1),
                        GraphicsUnit.Pixel
                        );
                }
                g.FillRectangle(new TextureBrush(backgroundImage), new Rectangle(0, 0, (int) targetWidth, (int) targetHeight));*/

                var topLeftAngle = new Rectangle(0, 0, sourceImage.Width / 2, sourceImage.Height / 2 + 1);
                var topRightAngle = new Rectangle(sourceImage.Width / 2, 0, sourceImage.Width / 2, sourceImage.Height / 2);
                var bottomLeftAngle = new Rectangle(0, sourceImage.Height / 2, sourceImage.Width / 2, sourceImage.Height / 2);
                var bottomRighttAngle = new Rectangle(sourceImage.Width / 2, sourceImage.Height / 2, sourceImage.Width / 2, sourceImage.Height / 2);
                var dstTopLeftAngle = topLeftAngle;
                var dstTopRightAngle = new Rectangle((int) targetWidth - sourceImage.Width / 2, 0, sourceImage.Width / 2, sourceImage.Height / 2);
                var dstBottomLeftAngle = new Rectangle(0, (int) targetHeight - sourceImage.Height / 2, sourceImage.Width / 2, sourceImage.Height / 2);
                var dstBottomRightAngle = new Rectangle((int) targetWidth - sourceImage.Width / 2, (int) targetHeight - sourceImage.Height / 2, sourceImage.Width / 2, sourceImage.Height / 2);

                g.DrawImage(sourceImage, dstTopLeftAngle, topLeftAngle, GraphicsUnit.Pixel);
                g.DrawImage(sourceImage, dstTopRightAngle, topRightAngle, GraphicsUnit.Pixel);
                g.DrawImage(sourceImage, dstBottomLeftAngle, bottomLeftAngle, GraphicsUnit.Pixel);
                g.DrawImage(sourceImage, dstBottomRightAngle, bottomRighttAngle, GraphicsUnit.Pixel);

                var topTextureImg = new Bitmap(1, sourceImage.Height / 2);
                using (var tg = Graphics.FromImage(topTextureImg)) {
                    tg.DrawImage(
                        sourceImage,
                        new Rectangle(0, 0, 1, sourceImage.Height / 2),
                        new Rectangle(sourceImage.Width / 2, 0, sourceImage.Width / 2 + sourceImage.Width % 2, sourceImage.Height / 2),
                        GraphicsUnit.Pixel
                        );
                }
                var topBrush = new TextureBrush(topTextureImg);
                var topBorderRect = new Rectangle(sourceImage.Width / 2, 0, (int) targetWidth - sourceImage.Width + 2, sourceImage.Height / 2);
                g.FillRectangle(topBrush, topBorderRect);

                var bottomTextureImg = new Bitmap(1, sourceImage.Height / 2);
                using (var tg = Graphics.FromImage(bottomTextureImg)) {
                    tg.DrawImage(
                        sourceImage,
                        new Rectangle(0, 0, 1, sourceImage.Height / 2),
                        new Rectangle(sourceImage.Width / 2, sourceImage.Height / 2, 1, sourceImage.Height / 2),
                        GraphicsUnit.Pixel
                        );
                }
                for (var x = sourceImage.Width / 2; x < (int) targetWidth - sourceImage.Width / 2; x++) {
                    var bottomBorderRect = new Rectangle(x, (int) targetHeight - sourceImage.Height / 2, 1, sourceImage.Height / 2);
                    g.DrawImage(bottomTextureImg, bottomBorderRect);
                }

                var leftTextureImg = new Bitmap(sourceImage.Width == 1 ? 1 : sourceImage.Width / 2, 1);
                using (var tg = Graphics.FromImage(leftTextureImg)) {
                    tg.DrawImage(
                        sourceImage,
                        new Rectangle(0, 0, sourceImage.Width / 2, 1),
                        new Rectangle(0, sourceImage.Height / 2, sourceImage.Width / 2, sourceImage.Height / 2 + 1),
                        GraphicsUnit.Pixel
                        );
                }
                var leftBrush = new TextureBrush(leftTextureImg);
                var leftBorderRect = new Rectangle(0, sourceImage.Height / 2, sourceImage.Width / 2, (int) targetHeight - sourceImage.Height);
                g.FillRectangle(leftBrush, leftBorderRect);

                var rightTextureImg = new Bitmap(sourceImage.Width == 1 ? 1 : sourceImage.Width / 2, 1);
                using (var tg = Graphics.FromImage(rightTextureImg)) {
                    tg.DrawImage(
                        sourceImage,
                        new Rectangle(0, 0, sourceImage.Width / 2, 1),
                        new Rectangle(sourceImage.Width / 2, sourceImage.Height / 2, sourceImage.Width / 2, sourceImage.Height / 2),
                        GraphicsUnit.Pixel
                        );
                }
                for (var y = sourceImage.Height / 2; y < (int) targetHeight - sourceImage.Height / 2; y++) {
                    var rightBorderRect = new Rectangle((int) targetWidth - sourceImage.Width / 2, y, sourceImage.Width / 2, 1);
                    g.DrawImage(rightTextureImg, rightBorderRect);
                }
            }
            using (var ms = new MemoryStream()) {
                img.Save(ms, ImageFormat.Png);
                ms.Position = 0;
                targetImage.BeginInit();
                targetImage.StreamSource = ms;
                targetImage.CacheOption = BitmapCacheOption.OnLoad;
                targetImage.EndInit();
                ms.Close();
            }
            return targetImage;
        }

        public static SolidColorBrush GetColorBrushFromString(string color) {
            if (color == null) return new SolidColorBrush(Colors.Transparent);
            var ret = new SolidColorBrush();
            var dyes = color.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            byte alfa, r, g, b;
            Byte.TryParse(dyes.Length == 4 ? dyes[3] : "255", NumberStyles.Integer, Thread.CurrentThread.CurrentCulture, out alfa);
            Byte.TryParse(dyes[0], NumberStyles.Integer, Thread.CurrentThread.CurrentCulture, out r);
            Byte.TryParse(dyes[1], NumberStyles.Integer, Thread.CurrentThread.CurrentCulture, out g);
            Byte.TryParse(dyes[2], NumberStyles.Integer, Thread.CurrentThread.CurrentCulture, out b);
            ret.Color = dyes.Length == 4
                ? System.Windows.Media.Color.FromArgb(alfa, r, g, b)
                : (dyes.Length == 3 ? System.Windows.Media.Color.FromRgb(r, g, b) : Colors.White);
            return ret;
        }

        public static void PlaySound(string fileName) {
            using (var fs = new FileStream(fileName, FileMode.Open)) {
                using (var snd = new SoundPlayer(fs)) {
                    snd.Play();
                }
            }
        }

        public static string PrettyXml(string xml) {
            var stringBuilder = new StringBuilder();

            var element = XElement.Parse(xml);

            var settings = new XmlWriterSettings {
                OmitXmlDeclaration = true,
                Indent = true,
                IndentChars = "\t",
                NewLineOnAttributes = false
            };

            using (var xmlWriter = XmlWriter.Create(stringBuilder, settings)) {
                element.Save(xmlWriter);
            }

            return stringBuilder.ToString();
        }
    }
}
