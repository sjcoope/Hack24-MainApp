using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.ProjectOxford.Face;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using Microsoft.ProjectOxford.Face.Contract;
using Windows.UI.Xaml.Shapes;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Hack24.MainApp
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private FaceAttributes faceAttributes;
        private FaceRectangle faceRectangle;
        private int score = 0;
        private readonly IFaceServiceClient faceServiceClient;

        public MainPage()
        {
            this.InitializeComponent();
            var faceServiceAPIKey = File.ReadAllText("./Assets/FaceServiceAPI.key");
            faceServiceClient = new FaceServiceClient(faceServiceAPIKey, "https://westeurope.api.cognitive.microsoft.com/face/v1.0");
            this.InitializeComponent();
            ShowPreview();
        }

        private async void ShowPreview()
        {
            MediaCapture capture = new MediaCapture();
            await capture.InitializeAsync();
            PART_Capture.Source = capture;
            await capture.StartPreviewAsync();
        }

        private async void Capture_Click(object sender, RoutedEventArgs e)
        {
            PART_Canvas.Visibility = Visibility.Collapsed;
            var stream = new InMemoryRandomAccessStream();
            await PART_Capture.Source.CapturePhotoToStreamAsync(ImageEncodingProperties.CreatePng(), stream);

            var stm = stream.AsStream();
            stm.Seek(0, SeekOrigin.Begin);
            var br = new BinaryReader(stm);
            var bytes = br.ReadBytes((int)stm.Length);

            var decoder = await BitmapDecoder.CreateAsync(stream);
            var image = await decoder.GetSoftwareBitmapAsync(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            var source = new SoftwareBitmapSource();

            var width = image.PixelWidth;
            var height = image.PixelHeight;
            var scaleX = 0; // PART_Canvas.ActualWidth / image.PixelWidth;
            var scaleY = 0; // PART_Canvas.ActualHeight / image.PixelHeight;
            stream.Seek(0);

            image = await decoder.GetSoftwareBitmapAsync(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied,
                new BitmapTransform { ScaledHeight = (uint)((double)height * scaleY), ScaledWidth = (uint)((double)width * scaleX) },
                ExifOrientationMode.IgnoreExifOrientation,
                ColorManagementMode.DoNotColorManage);

            await source.SetBitmapAsync(image);

            PART_Canvas.Children.Clear();
            var tmpImage = new Image { Source = source };
            PART_Canvas.Children.Add(tmpImage);
            var faces = await UploadAndDetectFaces(stream.AsStream());

            if (faces.Length > 0)
            {
                PART_Canvas.Visibility = Visibility.Visible;
                PART_Capture.Visibility = Visibility.Collapsed;
                ProcessFaces(faces);
            }
        }

        private void OutputFaceInfo(Face[] faces)
        {
            if (faces.Length == 0)
                return;

            //var face = faces[0]; // just take first face we find
            //txtOutput.Text = "Faces Detected: ";
            //txtOutput.Text = txtOutput.Text + $"id {face.FaceId}\nAge {face.FaceAttributes.Age}\nHappiness {face.FaceAttributes.Emotion.Happiness}\nSadness {face.FaceAttributes.Emotion.Sadness}\n";
            //txtOutput.Text = txtOutput.Text + $"Anger {face.FaceAttributes.Emotion.Anger}\nContempt {face.FaceAttributes.Emotion.Contempt}\n";
            //txtOutput.Text = txtOutput.Text + $"Disgust {face.FaceAttributes.Emotion.Disgust}\nFear {face.FaceAttributes.Emotion.Fear}\n";
            //txtOutput.Text = txtOutput.Text +
            //                 $"Surprise {face.FaceAttributes.Emotion.Surprise}\nNeutral {face.FaceAttributes.Emotion.Neutral}\n\n";
        }

        private void HighlightFaces(Face[] faces)
        {
            var left = faces[0].FaceRectangle.Left;
            var top = faces[0].FaceRectangle.Top;
            var width = faces[0].FaceRectangle.Width;
            var height = faces[0].FaceRectangle.Height;
            var rectangle1 = new Rectangle();
            rectangle1.Width = width;
            rectangle1.Height = height;
            rectangle1.Stroke = new SolidColorBrush(Windows.UI.Colors.Blue);
            rectangle1.StrokeThickness = 2;
            rectangle1.RadiusX = 25;
            rectangle1.RadiusY = 5;
            // When you create a XAML element in code, you have to add
            // it to the XAML visual tree. This example assumes you have
            // a panel named 'layoutRoot' in your XAML file, like this:
            // <Grid x:Name="layoutRoot>
            Canvas.SetTop(rectangle1, top);
            Canvas.SetLeft(rectangle1, left);
            PART_Canvas.Children.Add(rectangle1);
        }

        private void ProcessFaces(Face[] faces)
        {
            //OutputFaceInfo(faces);
            HighlightFaces(faces);
        }

        private async Task<Face[]> UploadAndDetectFaces(Stream imageStream)
        {
            // The list of Face attributes to return.
            IEnumerable<FaceAttributeType> faceAttributes =
                new FaceAttributeType[] { FaceAttributeType.Gender, FaceAttributeType.Age, FaceAttributeType.Smile, FaceAttributeType.Emotion, FaceAttributeType.Glasses, FaceAttributeType.Hair };

            // Call the Face API.
            try
            {
                using (imageStream)
                {
                    Face[] faces = await faceServiceClient.DetectAsync(imageStream, returnFaceId: true, returnFaceLandmarks: false, returnFaceAttributes: faceAttributes);
                    return faces;
                }
            }
            // Catch and display Face API errors.
            catch (FaceAPIException f)
            {
                var dialog = new MessageDialog(f.ErrorMessage, f.ErrorCode);
                await dialog.ShowAsync();
                return new Face[0];
            }
            // Catch and display all other errors.
            catch (Exception e)
            {
                var dialog = new MessageDialog(e.Message, "Error");
                await dialog.ShowAsync();
                return new Face[0];
            }
        }

        private void Click_Happy(object sender, RoutedEventArgs e)
        {
            if (faceAttributes.Emotion.Happiness > 0.5)
            {
                score += 1;
            }
            else
            {
                score -= 1;
            }

            txtScore.Text = score.ToString();
            PART_Choice.Visibility = Visibility.Collapsed;
            PART_Canvas.Visibility = Visibility.Collapsed;
        }

        private void Click_Sad(object sender, RoutedEventArgs e)
        {
            if (faceAttributes.Emotion.Sadness > 0.5)
            {
                score += 1;
            }
            else
            {
                score -= 1;
            }

            txtScore.Text = score.ToString();
            PART_Choice.Visibility = Visibility.Collapsed;
            PART_Canvas.Visibility = Visibility.Collapsed;
        }

        private void Click_Angry(object sender, RoutedEventArgs e)
        {
            if (faceAttributes.Emotion.Anger > 0.5)
            {
                score += 1;
            }
            else
            {
                score -= 1;
            }

            txtScore.Text = score.ToString();
            PART_Choice.Visibility = Visibility.Collapsed;
            PART_Canvas.Visibility = Visibility.Collapsed;
        }

        private void Click_Fear(object sender, RoutedEventArgs e)
        {
            if (faceAttributes.Emotion.Fear > 0.5)
            {
                score += 1;
            }
            else
            {
                score -= 1;
            }

            txtScore.Text = score.ToString();
            PART_Choice.Visibility = Visibility.Collapsed;
            PART_Canvas.Visibility = Visibility.Collapsed;
        }

        private void Click_Surprised(object sender, RoutedEventArgs e)
        {
            if (faceAttributes.Emotion.Surprise > 0.5)
            {
                score += 1;
            }
            else
            {
                score -= 1;
            }

            txtScore.Text = score.ToString();
            PART_Choice.Visibility = Visibility.Collapsed;
            PART_Canvas.Visibility = Visibility.Collapsed;
        }

        private void Click_Neutral(object sender, RoutedEventArgs e)
        {
            if (faceAttributes.Emotion.Neutral > 0.5)
            {
                score += 1;
            }
            else
            {
                score -= 1;
            }

            txtScore.Text = score.ToString();
            PART_Choice.Visibility = Visibility.Collapsed;
            PART_Canvas.Visibility = Visibility.Collapsed;
        }

        private void EmotionApp_Click(object sender, RoutedEventArgs e)
        {
            // Update UI
            MainViewControls.Visibility = Visibility.Collapsed;
            MainViewContent.Visibility = Visibility.Collapsed;
            EmotionAppControls.Visibility = Visibility.Visible;
            EmotionAppContent.Visibility = Visibility.Visible;
        }

        private void ConversationApp_Click(object sender, RoutedEventArgs e)
        {
            // Update UI
            MainViewControls.Visibility = Visibility.Collapsed;
            MainViewContent.Visibility = Visibility.Collapsed;
            ConversationAppControls.Visibility = Visibility.Visible;
            ConversationAppContent.Visibility = Visibility.Visible;
        }
    }
}
