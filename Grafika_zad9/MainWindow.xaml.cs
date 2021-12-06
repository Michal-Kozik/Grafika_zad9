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
            // TODO: walidacja.
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
            //TODO: Walidacja
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
