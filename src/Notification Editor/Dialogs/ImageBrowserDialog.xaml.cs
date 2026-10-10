using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.Foundation.Metadata;
using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.ApplicationModel;
using System.Threading.Tasks;
using Windows.Storage.FileProperties;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FireBlade.NotificationEditor.Dialogs
{
    public sealed partial class ImageBrowserDialog : ContentDialog
    {
        private ObservableCollection<ImageBrowserItem> _items { get; set; } = new ObservableCollection<ImageBrowserItem>();
        private ObservableCollection<ImageBrowserDirItem> _dirs { get; set; } = new ObservableCollection<ImageBrowserDirItem>();

        public ImageBrowserDialog()
        {
            this.InitializeComponent();

            if (ApiInformation.IsWriteablePropertyPresent("Windows.UI.Xaml.Controls.ContentDialog", "DefaultButton"))
                DefaultButton = ContentDialogButton.Primary;
        }

        public static readonly DependencyProperty SelectedUriProperty = DependencyProperty.Register(
            "SelectedUri",
            typeof(string),
            typeof(ImageBrowserDialog),
            new PropertyMetadata(string.Empty));

        public string SelectedUri
        {
            get => (string)GetValue(SelectedUriProperty);
            set => SetValue(SelectedUriProperty, value);
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            if (itemGv.SelectedItem is ImageBrowserItem item && dirBox.SelectedItem is ImageBrowserDirItem dir)
            {
                if (dir.IsRoot)
                    SelectedUri = $"ms-appx:///Assets/{item.File.Name}";
                else
                    SelectedUri = $"ms-appx:///Assets{dir.Path}/{item.File.Name}";
            }
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private async Task AddDirsRecursiveAsync(IEnumerable<StorageFolder> folders, string path)
        {
            foreach (var dir in folders)
            {
                if (!dir.Name.Equals("Apps", StringComparison.OrdinalIgnoreCase) && !dir.Name.Equals("MDL2", StringComparison.OrdinalIgnoreCase))
                    _dirs.Add(new ImageBrowserDirItem
                    {
                        DisplayName = dir.DisplayName,
                        Folder = dir,
                        Path = path + "/" + dir.Name
                    });

                await AddDirsRecursiveAsync(await dir.GetFoldersAsync(), path + "/" + dir.Name);
            }
        }

        private async void ContentDialog_Loaded(object sender, RoutedEventArgs e)
        {
            // don't reload this to save performance
            if (!_dirs.Any())
            {
                var assets = await Package.Current.InstalledLocation.GetFolderAsync("Assets");
                _dirs.Clear();

                _dirs.Add(new ImageBrowserDirItem
                {
                    DisplayName = "Root",
                    Folder = assets,
                    IsRoot = true
                });

                await AddDirsRecursiveAsync(await assets.GetFoldersAsync(), string.Empty);

                dirBox.SelectedIndex = 0;
            }

            if (!string.IsNullOrEmpty(SelectedUri) && Uri.TryCreate(SelectedUri, UriKind.Absolute, out Uri uri))
            {
                if (uri.Scheme.Equals("ms-appx", StringComparison.OrdinalIgnoreCase))
                {
                    string abs = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));

                    if (abs.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                    {
                        string relative = abs.Substring("Assets/".Length);

                        int lastSlash = relative.LastIndexOf('/');

                        string dirPath = lastSlash >= 0
                            ? relative.Substring(0, lastSlash)
                            : string.Empty;

                        string filename = lastSlash >= 0
                            ? relative.Substring(lastSlash + 1)
                            : relative;

                        _shutUpAndDontReloadOnSelected = true;

                        try
                        {
                            ImageBrowserDirItem d;

                            if (string.IsNullOrEmpty(dirPath))
                            {
                                d = _dirs.FirstOrDefault(x => x.IsRoot);
                            }
                            else
                            {
                                d = _dirs.FirstOrDefault(x =>
                                    x.Path?.Equals("/" + dirPath, StringComparison.OrdinalIgnoreCase) == true);
                            }

                            if (d != null)
                                dirBox.SelectedItem = d;
                            else
                                dirBox.SelectedIndex = 0;

                            await LoadFilesIntoViewFromStorageFolderAsync(
                                (dirBox.SelectedItem as ImageBrowserDirItem).Folder);

                            var item = _items.FirstOrDefault(x =>
                                x.File.Name.Equals(filename, StringComparison.OrdinalIgnoreCase));

                            if (item != null)
                            {
                                itemGv.SelectedItem = item;

                                (itemGv.ContainerFromItem(item) as FrameworkElement)?
                                    .StartBringIntoView(new BringIntoViewOptions
                                    {
                                        AnimationDesired = true
                                    });
                            }

                            _ = LoadFileThumbnails((dirBox.SelectedItem as ImageBrowserDirItem).Folder);
                        }
                        finally
                        {
                            _shutUpAndDontReloadOnSelected = false;
                        }
                    }
                }
            }
        }

        private static string FormatFileSize(ulong bytes)
        {
            if (bytes < 1024)
                return $"{bytes} B";

            double size = bytes;
            string[] units = { "KB", "MB", "GB", "TB" };
            int unit = -1;

            while (size >= 1024 && unit < units.Length - 1)
            {
                size /= 1024;
                unit++;
            }

            return $"{size:0.##} {units[unit]}";
        }

        private async Task<BitmapImage> InvertThumbnailAsync(StorageItemThumbnail thumbnail)
        {
            var decoder = await BitmapDecoder.CreateAsync(thumbnail);

            var pixels = (await decoder.GetPixelDataAsync()).DetachPixelData();

            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = (byte)(255 - pixels[i]);     // B
                pixels[i + 1] = (byte)(255 - pixels[i + 1]); // G
                pixels[i + 2] = (byte)(255 - pixels[i + 2]); // R
                                                             // pixels[i + 3] = alpha; leave it alone
            }

            var stream = new InMemoryRandomAccessStream();

            var encoder = await BitmapEncoder.CreateAsync(
                BitmapEncoder.PngEncoderId,
                stream);

            encoder.SetPixelData(
                BitmapPixelFormat.Bgra8,
                BitmapAlphaMode.Premultiplied,
                decoder.PixelWidth,
                decoder.PixelHeight,
                96,
                96,
                pixels);

            await encoder.FlushAsync();

            stream.Seek(0);

            var bitmap = new BitmapImage();
            await bitmap.SetSourceAsync(stream);

            return bitmap;
        }

        private bool _shutUpAndDontReloadOnSelected = false;

        private async Task LoadFilesIntoViewFromStorageFolderAsync(StorageFolder dir)
        {
            _items.Clear();

            foreach (var file in await dir.GetFilesAsync())
            {
                if (file.Name.StartsWith("BadgeLogo", StringComparison.OrdinalIgnoreCase)) continue;

                if (file.FileType.Equals(".png", StringComparison.OrdinalIgnoreCase) || file.FileType.Equals(".jpg", StringComparison.OrdinalIgnoreCase)
                    || file.FileType.Equals(".jpeg", StringComparison.OrdinalIgnoreCase))
                {
                    _items.Add(new ImageBrowserItem
                    {
                        Name = file.DisplayName,
                        Size = FormatFileSize((await file.GetBasicPropertiesAsync()).Size),
                        File = file
                    });
                }
            }
        }

        private async Task LoadFileThumbnails(StorageFolder dir)
        {
            foreach (var item in _items.ToArray())
            {
                StorageItemThumbnail thumb = null;

                try
                {
                    thumb = await item.File.GetThumbnailAsync(ThumbnailMode.PicturesView, 150);

                    if (ActualTheme == ElementTheme.Light && !(new[] { "Food", "Weather", "Hipstame", "Originals" })
                        .Contains(dir.Name, StringComparer.OrdinalIgnoreCase) && !item.Name.EndsWith("Light",
                        StringComparison.OrdinalIgnoreCase) && !item.Name.Equals("map", StringComparison.OrdinalIgnoreCase))

                        item.Thumbnail = await InvertThumbnailAsync(thumb);
                    else
                    {
                        var bmp = new BitmapImage();

                        await bmp.SetSourceAsync(thumb);
                        item.Thumbnail = bmp;
                    }

                    item.LoadingThumbnail = false;
                }
                finally
                {
                    thumb?.Dispose();
                }
            }
        }

        private async void DirBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_shutUpAndDontReloadOnSelected) return;

            var dir = dirBox.SelectedItem as ImageBrowserDirItem;
            if (dir is null) return;

            await LoadFilesIntoViewFromStorageFolderAsync(dir.Folder);
            await LoadFileThumbnails(dir.Folder);
        }
    }

    public class ImageBrowserItem : DependencyObject
    {
        public string Name { get; set; }
        public string Size { get; set; }
        public StorageFile File { get; set; }

        public ImageSource Thumbnail
        {
            get => (ImageSource)GetValue(ThumbnailProperty);
            set => SetValue(ThumbnailProperty, value);
        }

        public static readonly DependencyProperty ThumbnailProperty = DependencyProperty.Register(
            "Thumbnail",
            typeof(ImageSource),
            typeof(ImageBrowserItem),
            new PropertyMetadata(null));

        public bool LoadingThumbnail
        {
            get => (bool)GetValue(LoadingThumbnailProperty);
            set => SetValue(LoadingThumbnailProperty, value);
        }

        public static readonly DependencyProperty LoadingThumbnailProperty = DependencyProperty.Register(
            "LoadingThumbnail",
            typeof(bool),
            typeof(ImageBrowserItem),
            new PropertyMetadata(true));
    }

    public class ImageBrowserDirItem
    {
        public string DisplayName { get; set; }
        public StorageFolder Folder { get; set; }
        public bool IsRoot { get; set; }

        public string Path { get; set; }
    }
}
