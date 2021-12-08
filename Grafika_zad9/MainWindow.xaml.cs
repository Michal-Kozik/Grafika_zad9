using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Grafika_zad9
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private int Ymin = 0;
        private int Ymax = 18;
        private int Umin = 4;
        private int Umax = 10;
        private int Vmin = 0;
        private int Vmax = 9;

        public MainWindow()
        {
            InitializeComponent();
        }

        private bool IsPictureLoaded()
        {
            if (imgSource.Source == null)
                return false;
            else
                return true;
        }

        private bool IsInputValid()
        {
            if (!Byte.TryParse(redFrom.Text, out byte redFromInput))
                return false;
            if (!Byte.TryParse(greenFrom.Text, out byte greenFromInput))
                return false;
            if (!Byte.TryParse(blueFrom.Text, out byte blueFromInput))
                return false;
            if (!Byte.TryParse(redTo.Text, out byte redToInput))
                return false;
            if (!Byte.TryParse(greenTo.Text, out byte greenToInput))
                return false;
            if (!Byte.TryParse(blueTo.Text, out byte blueToInput))
                return false;
            if (redFromInput < 0 || redFromInput > 255 || greenFromInput < 0 || greenFromInput > 255 || blueFromInput < 0 || blueFromInput > 255)
                return false;
            if (redToInput < 0 || redToInput > 255 || greenToInput < 0 || greenToInput > 255 || blueToInput < 0 || blueToInput > 255)
                return false;
            if (redToInput < redFromInput || greenToInput < greenFromInput || blueToInput < blueFromInput)
                return false;
            return true;
        }

        private void OpenFileDialog(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                imgSource.Source = new BitmapImage(new Uri(openFileDialog.FileName));
            }
        }



        private void Analize(object sender, RoutedEventArgs e)
        {
            if (!IsPictureLoaded())
            {
                MessageBox.Show("Nie można przeanalizować obrazu, ponieważ nie został załadowany.", "Nie załadowano obrazu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Bitmap imgSourceBitmap = ConvertImgToBitmap(imgSource);
            BitmapData sourceBitmapData = imgSourceBitmap.LockBits(new System.Drawing.Rectangle(0, 0, imgSourceBitmap.Width, imgSourceBitmap.Height),
                                                            ImageLockMode.ReadOnly,
                                                            System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            byte[] pixelBuffer = new byte[sourceBitmapData.Stride * sourceBitmapData.Height];
            Marshal.Copy(sourceBitmapData.Scan0, pixelBuffer, 0, pixelBuffer.Length);

            byte[] pixelBufferResult = new byte[sourceBitmapData.Stride * sourceBitmapData.Height];
            Marshal.Copy(sourceBitmapData.Scan0, pixelBufferResult, 0, pixelBufferResult.Length);
            imgSourceBitmap.UnlockBits(sourceBitmapData);

            double greenField = 0;
            double blueField = 0;
            double redField = 0;
            double other = 0;
            // Analiza obrazu.
            for (int i = 0; i + 4 < pixelBuffer.Length; i += 4)
            {
                // Tereny zielone.
                if (pixelBuffer[i + 1] > pixelBuffer[i] + 20 && pixelBuffer[i + 1] > pixelBuffer[i + 2] + 20)
                {
                    // BGR.
                    if (pixelBuffer[i] < 229 && pixelBuffer[i + 1] > 51 && pixelBuffer[i + 2] < 229)
                    {
                        pixelBufferResult[i] = 0;
                        pixelBufferResult[i + 1] = 255;
                        pixelBufferResult[i + 2] = 0;
                        greenField++;
                    }
                    else
                    {
                        pixelBufferResult[i] = 0;
                        pixelBufferResult[i + 1] = 0;
                        pixelBufferResult[i + 2] = 0;
                        other++;
                    }
                }
                // Tereny niebieskie.
                else if (pixelBuffer[i] >= pixelBuffer[i + 1] && pixelBuffer[i] > pixelBuffer[i + 2] + 20)
                {
                    // BGR.
                    if (pixelBuffer[i] > 51 && pixelBuffer[i + 2] < 204)
                    {
                        pixelBufferResult[i] = 255;
                        pixelBufferResult[i + 1] = 0;
                        pixelBufferResult[i + 2] = 0;
                        blueField++;
                    }
                    else
                    {
                        pixelBufferResult[i] = 0;
                        pixelBufferResult[i + 1] = 0;
                        pixelBufferResult[i + 2] = 0;
                        other++;
                    }
                }
                // Tereny czerwone.
                else if (pixelBuffer[i + 2] > pixelBuffer[i + 1] + 40 && pixelBuffer[i + 2] > pixelBuffer[i] + 40)
                {
                    // BGR.
                    if (pixelBuffer[i] < 229 && pixelBuffer[i + 1] < 204 && pixelBuffer[i + 2] > 51)
                    {
                        pixelBufferResult[i] = 0;
                        pixelBufferResult[i + 1] = 0;
                        pixelBufferResult[i + 2] = 255;
                        redField++;
                    }
                    else
                    {
                        pixelBufferResult[i] = 0;
                        pixelBufferResult[i + 1] = 0;
                        pixelBufferResult[i + 2] = 0;
                        other++;
                    }
                }
                // Tereny innych kolorow.
                else
                {
                    pixelBufferResult[i] = 0;
                    pixelBufferResult[i + 1] = 0;
                    pixelBufferResult[i + 2] = 0;
                    other++;
                }
            }
            double r = Math.Round(redField / (redField + greenField + blueField + other), 5) * 100;
            redLabel.Content = $"Czerwone: {r}%";
            double g = Math.Round(greenField / (redField + greenField + blueField + other), 5) * 100;
            greenLabel.Content = $"Zielone: {g}%";
            double b = Math.Round(blueField / (redField + greenField + blueField + other), 5) * 100;
            blueLabel.Content = $"Niebieskie: {b}%";
            double others = Math.Round(other / (redField + greenField + blueField + other), 5) * 100;
            otherLabel.Content = $"Inne: {others}%";
            CreateChart(r, g, b, others);

            // Rezultat.
            Bitmap imgResultBitmap = new Bitmap(imgSourceBitmap.Width, imgSourceBitmap.Height);
            BitmapData resultBitmapData = imgResultBitmap.LockBits(new System.Drawing.Rectangle(0, 0, imgResultBitmap.Width, imgResultBitmap.Height),
                                                            ImageLockMode.WriteOnly,
                                                            System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Marshal.Copy(pixelBufferResult, 0, resultBitmapData.Scan0, pixelBufferResult.Length);
            imgResultBitmap.UnlockBits(resultBitmapData);
            imgResult.Source = ConvertBitmapToImageSource(imgResultBitmap);
        }

        private void CustomAnalize(object sender, RoutedEventArgs e)
        {
            if (!IsPictureLoaded())
            {
                MessageBox.Show("Nie można przeanalizować obrazu, ponieważ nie został załadowany.", "Nie załadowano obrazu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (!IsInputValid())
            {
                MessageBox.Show("Podane dane nie są prawidłowe.", "Niepoprawne dane", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Bitmap imgSourceBitmap = ConvertImgToBitmap(imgSource);
            BitmapData sourceBitmapData = imgSourceBitmap.LockBits(new System.Drawing.Rectangle(0, 0, imgSourceBitmap.Width, imgSourceBitmap.Height),
                                                            ImageLockMode.ReadOnly,
                                                            System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            byte[] pixelBuffer = new byte[sourceBitmapData.Stride * sourceBitmapData.Height];
            Marshal.Copy(sourceBitmapData.Scan0, pixelBuffer, 0, pixelBuffer.Length);

            byte[] pixelBufferResult = new byte[sourceBitmapData.Stride * sourceBitmapData.Height];
            Marshal.Copy(sourceBitmapData.Scan0, pixelBufferResult, 0, pixelBufferResult.Length);
            imgSourceBitmap.UnlockBits(sourceBitmapData);

            byte redFromInput = Convert.ToByte(redFrom.Text);
            byte greenFromInput = Convert.ToByte(greenFrom.Text);
            byte blueFromInput = Convert.ToByte(blueFrom.Text);
            byte redToInput = Convert.ToByte(redTo.Text);
            byte greenToInput = Convert.ToByte(greenTo.Text);
            byte blueToInput = Convert.ToByte(blueTo.Text);

            double inRange = 0;
            double outOfRange = 0;
            // Analiza obrazu.
            for (int i = 0; i + 4 < pixelBuffer.Length; i += 4)
            {
                // Tereny in range.
                if (pixelBuffer[i] >= blueFromInput && pixelBuffer[i] <= blueToInput 
                && pixelBuffer[i + 1] >= greenFromInput && pixelBuffer[i + 1] <= greenToInput
                && pixelBuffer[i + 2] >= redFromInput && pixelBuffer[i + 2] <= redToInput)
                {
                    pixelBufferResult[i] = 0;
                    pixelBufferResult[i + 1] = 255;
                    pixelBufferResult[i + 2] = 255;
                    inRange++;
                }
                // Tereny out of range.
                else
                {
                    pixelBufferResult[i] = 0;
                    pixelBufferResult[i + 1] = 0;
                    pixelBufferResult[i + 2] = 0;
                    outOfRange++;
                }
            }
            double inRangePercent = Math.Round(inRange / (inRange + outOfRange), 5) * 100;
            inRangeLabel.Content = $"Należy: {inRangePercent}%";
            double outOfRangePercent = Math.Round(outOfRange / (inRange + outOfRange), 5) * 100;
            outOfRangeLabel.Content = $"Nie należy: {outOfRangePercent}%";
            CreateCustomChart(inRangePercent, outOfRangePercent);

            // Rezultat.
            Bitmap imgResultBitmap = new Bitmap(imgSourceBitmap.Width, imgSourceBitmap.Height);
            BitmapData resultBitmapData = imgResultBitmap.LockBits(new System.Drawing.Rectangle(0, 0, imgResultBitmap.Width, imgResultBitmap.Height),
                                                            ImageLockMode.WriteOnly,
                                                            System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Marshal.Copy(pixelBufferResult, 0, resultBitmapData.Scan0, pixelBufferResult.Length);
            imgResultBitmap.UnlockBits(resultBitmapData);
            imgResult.Source = ConvertBitmapToImageSource(imgResultBitmap);
        }

        private void YUVAnalize(object sender, RoutedEventArgs e)
        {
            if (!IsPictureLoaded())
            {
                MessageBox.Show("Nie można przeanalizować obrazu, ponieważ nie został załadowany.", "Nie załadowano obrazu", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Bitmap imgSourceBitmap = ConvertImgToBitmap(imgSource);
            BitmapData sourceBitmapData = imgSourceBitmap.LockBits(new System.Drawing.Rectangle(0, 0, imgSourceBitmap.Width, imgSourceBitmap.Height),
                                                            ImageLockMode.ReadOnly,
                                                            System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            byte[] pixelBuffer = new byte[sourceBitmapData.Stride * sourceBitmapData.Height];
            Marshal.Copy(sourceBitmapData.Scan0, pixelBuffer, 0, pixelBuffer.Length);

            double[] YUV = new double[pixelBuffer.Length];
            int[] convertedYUV = new int[pixelBuffer.Length];

            byte[] pixelBufferResult = new byte[sourceBitmapData.Stride * sourceBitmapData.Height];
            Marshal.Copy(sourceBitmapData.Scan0, pixelBufferResult, 0, pixelBufferResult.Length);
            imgSourceBitmap.UnlockBits(sourceBitmapData);

            // Uzupelnienie tablic.
            Button button = sender as Button;
            switch (button.Name)
            {
                case "YminAdd":
                    Ymin++;
                    Ymin = (Ymin > 20) ? 20 : Ymin;
                    break;
                case "YminSubtract":
                    Ymin--;
                    Ymin = (Ymin < 0) ? 0 : Ymin;
                    break;
                case "YmaxAdd":
                    Ymax++;
                    Ymax = (Ymax > 20) ? 20 : Ymax;
                    break;
                case "YmaxSubtract":
                    Ymax--;
                    Ymax = (Ymax < 0) ? 20 : Ymax;
                    break;
                case "UminAdd":
                    Umin++;
                    Umin = (Umin > 20) ? 20 : Umin;
                    break;
                case "UminSubtract":
                    Umin--;
                    Umin = (Umin < 0) ? 20 : Umin;
                    break;
                case "UmaxAdd":
                    Umax++;
                    Umax = (Umax > 20) ? 20 : Umax;
                    break;
                case "UmaxSubtract":
                    Umax--;
                    Umax = (Umax < 0) ? 20 : Umax;
                    break;
                case "VminAdd":
                    Vmin++;
                    Vmin = (Vmin > 20) ? 20 : Vmin;
                    break;
                case "VminSubtract":
                    Vmin--;
                    Vmin = (Vmin < 0) ? 20 : Vmin;
                    break;
                case "VmaxAdd":
                    Vmax++;
                    Vmax = (Vmax > 20) ? 20 : Vmax;
                    break;
                case "VmaxSubtract":
                    Vmax--;
                    Vmax = (Vmax < 0) ? 20 : Vmax;
                    break;
            }
            YminLabel.Content = $"Ymin: {Ymin}";
            YmaxLabel.Content = $"Ymax: {Ymax}";
            UminLabel.Content = $"Umin: {Umin}";
            UmaxLabel.Content = $"Umax: {Umax}";
            VminLabel.Content = $"Vmin: {Vmin}";
            VmaxLabel.Content = $"Vmax: {Vmax}";
            int[] Y = new int[21] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0 };
            int[] U = new int[21] { 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            int[] V = new int[21] { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            for (int i = 0; i < 21; i++)
            {
                Y[i] = (i < Ymin || i > Ymax) ? 0 : 1;
                U[i] = (i < Umin || i > Umax) ? 0 : 1;
                V[i] = (i < Vmin || i > Vmax) ? 0 : 1;
            }
            //int[] s = Y;
            //s = U;
            //s = V;

            // Zielony z ksiazki
            //int[] Y = new int[21] { 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };
            //int[] U = new int[21] { 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            //int[] V = new int[21] { 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            double a = 0.001;
            double foregroundPixel = 0;
            double backgroundPixel = 0;
            // Analiza obrazu.
            for (int i = 0; i + 4 < pixelBuffer.Length; i += 4)
            {
                double y = pixelBuffer[i + 2] * 0.299 + pixelBuffer[i + 1] * 0.587 + pixelBuffer[i] * 0.114;
                double u = pixelBuffer[i + 2] * -0.147 + pixelBuffer[i + 1] * -0.289 + pixelBuffer[i] * 0.436;
                double v = pixelBuffer[i + 2] * 0.615 + pixelBuffer[i + 1] * -0.515 + pixelBuffer[i] * -0.100;

                YUV[i] = y;
                YUV[i + 1] = u;
                YUV[i + 2] = v;
                YUV[i + 3] = 0;

                convertedYUV[i] = Convert.ToInt32(y / (12.75 + a));
                convertedYUV[i + 1] = Convert.ToInt32((u + 111.18) / (11.118 + a));
                convertedYUV[i + 2] = Convert.ToInt32((v + 156.825) / (15.6825 + a));
                convertedYUV[i + 3] = 0;

                if (Y[convertedYUV[i]] == 1 && U[convertedYUV[i + 1]] == 1 && V[convertedYUV[i + 2]] == 1)
                {
                    pixelBufferResult[i] = pixelBuffer[i];
                    pixelBufferResult[i + 1] = pixelBuffer[i + 1];
                    pixelBufferResult[i + 2] = pixelBuffer[i + 2];
                    foregroundPixel++;
                }
                else
                {
                    pixelBufferResult[i] = 0;
                    pixelBufferResult[i + 1] = 0;
                    pixelBufferResult[i + 2] = 0;
                    backgroundPixel++;
                }
            }
            double greenPercent = Math.Round(foregroundPixel / (foregroundPixel + backgroundPixel), 5) * 100;
            greenLabelYUV.Content = $"Zielone: {greenPercent}%";
            double otherPercent = Math.Round(backgroundPixel / (foregroundPixel + backgroundPixel), 5) * 100;
            otherLabelYUV.Content = $"Inne: {otherPercent}%";
            CreateYUVChart(greenPercent, otherPercent);

            // Rezultat.
            Bitmap imgResultBitmap = new Bitmap(imgSourceBitmap.Width, imgSourceBitmap.Height);
            BitmapData resultBitmapData = imgResultBitmap.LockBits(new System.Drawing.Rectangle(0, 0, imgResultBitmap.Width, imgResultBitmap.Height),
                                                            ImageLockMode.WriteOnly,
                                                            System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            Marshal.Copy(pixelBufferResult, 0, resultBitmapData.Scan0, pixelBufferResult.Length);
            imgResultBitmap.UnlockBits(resultBitmapData);
            imgResult.Source = ConvertBitmapToImageSource(imgResultBitmap);
        }

        private void CreateYUVChart(double foreground, double background)
        {
            yuvStatistics.Children.Clear();
            for (int i = 0; i < foreground; i++)
            {
                System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle();
                rectangle.Width = 10;
                rectangle.Height = 1;
                rectangle.Fill = new SolidColorBrush(Colors.Green);
                yuvStatistics.Children.Add(rectangle);
            }
            for (int i = 0; i < background; i++)
            {
                System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle();
                rectangle.Width = 10;
                rectangle.Height = 1;
                rectangle.Fill = new SolidColorBrush(Colors.Gray);
                yuvStatistics.Children.Add(rectangle);
            }
        }

        private void CreateCustomChart(double inRangePercent, double outOfRangePercent)
        {
            customStatistics.Children.Clear();
            for (int i = 0; i < inRangePercent; i++)
            {
                System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle();
                rectangle.Width = 10;
                rectangle.Height = 1;
                rectangle.Fill = new SolidColorBrush(Colors.Orange);
                customStatistics.Children.Add(rectangle);
            }
            for (int i = 0; i < outOfRangePercent; i++)
            {
                System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle();
                rectangle.Width = 10;
                rectangle.Height = 1;
                rectangle.Fill = new SolidColorBrush(Colors.Gray);
                customStatistics.Children.Add(rectangle);
            }
        }

        private void CreateChart(double r, double g, double b, double others)
        {
            statistics.Children.Clear();
            for (int i = 0; i < r; i++)
            {
                System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle();
                rectangle.Width = 10;
                rectangle.Height = 1;
                rectangle.Fill = new SolidColorBrush(Colors.Red);
                statistics.Children.Add(rectangle);
            }
            for (int i = 0; i < g; i++)
            {
                System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle();
                rectangle.Width = 10;
                rectangle.Height = 1;
                rectangle.Fill = new SolidColorBrush(Colors.Green);
                statistics.Children.Add(rectangle);
            }
            for (int i = 0; i < b; i++)
            {
                System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle();
                rectangle.Width = 10;
                rectangle.Height = 1;
                rectangle.Fill = new SolidColorBrush(Colors.Blue);
                statistics.Children.Add(rectangle);
            }
            for (int i = 0; i < others; i++)
            {
                System.Windows.Shapes.Rectangle rectangle = new System.Windows.Shapes.Rectangle();
                rectangle.Width = 10;
                rectangle.Height = 1;
                rectangle.Fill = new SolidColorBrush(Colors.Gray);
                statistics.Children.Add(rectangle);
            }
        }

        private void ShowColor(object sender, RoutedEventArgs e)
        {
            if (!IsInputValid())
            {
                MessageBox.Show("Podane dane nie są prawidłowe.", "Niepoprawne dane", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            byte redFromInput = Convert.ToByte(redFrom.Text);
            byte greenFromInput = Convert.ToByte(greenFrom.Text);
            byte blueFromInput = Convert.ToByte(blueFrom.Text);
            byte redToInput = Convert.ToByte(redTo.Text);
            byte greenToInput = Convert.ToByte(greenTo.Text);
            byte blueToInput = Convert.ToByte(blueTo.Text);

            colorFrom.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(redFromInput, greenFromInput, blueFromInput));
            colorTo.Fill = new SolidColorBrush(System.Windows.Media.Color.FromRgb(redToInput, greenToInput, blueToInput));
        }

        private Bitmap ConvertImgToBitmap(System.Windows.Controls.Image source)
        {
            RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)source.ActualWidth, (int)source.ActualHeight, 96.0, 96.0, PixelFormats.Pbgra32);
            source.Measure(new System.Windows.Size((int)source.ActualWidth, (int)source.ActualHeight));
            source.Arrange(new Rect(new System.Windows.Size((int)source.ActualWidth, (int)source.ActualHeight)));
            renderTargetBitmap.Render(source);

            PngBitmapEncoder encoder = new PngBitmapEncoder();
            MemoryStream stream = new MemoryStream();
            encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));

            encoder.Save(stream);
            Bitmap bitmap = new Bitmap(stream);
            stream.Close();
            renderTargetBitmap.Clear();
            return bitmap;
        }

        private BitmapImage ConvertBitmapToImageSource(Bitmap bitmap)
        {
            MemoryStream memoryStream = new MemoryStream();
            bitmap.Save(memoryStream, ImageFormat.Png);
            memoryStream.Position = 0;
            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();
            return bitmapImage;
        }

        
    }
}
