using Microsoft.Graphics.Canvas;
using System;
using System.Collections.Generic;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace uwpTestLoadImage
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        
        public MainPage()
        {
            this.InitializeComponent();
        }

        private async void loadImage_Click(object sender, RoutedEventArgs e)
        {
            FileOpenPicker fileOpenPicker = new FileOpenPicker();            
            fileOpenPicker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fileOpenPicker.FileTypeFilter.Add(".jpg");
            fileOpenPicker.ViewMode = PickerViewMode.Thumbnail;

            var inputFile = await fileOpenPicker.PickSingleFileAsync();

            if (inputFile == null)
            {
                // The user cancelled the picking operation
                return;
            }

            SoftwareBitmap softwareBitmap;

            using (IRandomAccessStream stream = await inputFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                // Create the decoder from the stream
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);

                // Get the SoftwareBitmap representation of the file
                softwareBitmap = await decoder.GetSoftwareBitmapAsync();
            }

            if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || softwareBitmap.BitmapAlphaMode == BitmapAlphaMode.Straight)
            {
                softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
            }

            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(softwareBitmap);

            imageControl.Source = source;

            var device = CanvasDevice.GetSharedDevice();
            var image = default(CanvasBitmap);
            using (var s = await inputFile.OpenReadAsync())
            {
                image = await CanvasBitmap.LoadAsync(device, s);
            }

            var offscreen = new CanvasRenderTarget(device, (float)image.Bounds.Width, (float)image.Bounds.Height, 96);
            using (var ds = offscreen.CreateDrawingSession())
            {
                ds.DrawImage(image, 0, 0);
                ds.DrawText("Hello note", 10, 400, Colors.Aqua);
            }


            var displayInformation = DisplayInformation.GetForCurrentView();
            var savepicker = new FileSavePicker();
            savepicker.FileTypeChoices.Add("png", new List<string> { ".png" });
            var destFile = await savepicker.PickSaveFileAsync();
            using (var s = await destFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, s);
                encoder.SetPixelData(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Ignore,
                    (uint)offscreen.Size.Width,
                    (uint)offscreen.Size.Height,
                    displayInformation.LogicalDpi,
                    displayInformation.LogicalDpi,
                    offscreen.GetPixelBytes());
                await encoder.FlushAsync();
            }


        }
    }
}
