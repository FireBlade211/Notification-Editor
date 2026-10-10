using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Collections.Generic;
using NotificationsVisualizerLibrary;
using Windows.Data.Xml.Dom;
using Windows.System.Profile;
using System.Linq;
using Windows.Storage;
using System;
using Windows.ApplicationModel.DataTransfer;
using NotificationsVisualizerLibrary.Parsers;
using FireBlade.NotificationEditor.DataModel;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.UI.ViewManagement;
using Windows.UI.StartScreen;
using FireBlade.NotificationEditor.Helpers;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.Graphics.Imaging;
using System.IO;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI;
using Windows.System;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml.Media;
using Windows.Storage.AccessCache;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FireBlade.NotificationEditor.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class TilePage : Page, INotificationEditorPage
    {
        public WinBuildEntry[] Builds = new WinBuildEntry[]
        {
            new WinBuildEntry
            {
                ShortBuildNumber = "1507",
                LongBuildNumber = 10240
            },
            new WinBuildEntry
            {
                ShortBuildNumber = "1511",
                LongBuildNumber = 10586
            },
            new WinBuildEntry
            {
                ShortBuildNumber = "1607",
                LongBuildNumber = 14393
            },
            new WinBuildEntry
            {
                ShortBuildNumber = "1703",
                LongBuildNumber = 15063
            },
            new WinBuildEntry
            {
                ShortBuildNumber = "1709",
                LongBuildNumber = 16299
            },
            new WinBuildEntry
            {
                ShortBuildNumber = "20H2",
                LongBuildNumber = 19042
            }
        };

        public TilePage()
        {
            this.InitializeComponent();
        }

        private async void TilePage_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            if (!_shareImage)
            {
                args.Request.Data.SetText(XmlContent);

                if (OpenedFile != null)
                    args.Request.Data.SetStorageItems(new[] { OpenedFile });

                args.Request.Data.Properties.Title = "Live Tile";
                args.Request.Data.Properties.Description = "Live Tile XML payload";
            }
            else
            {
                _shareImage = false;

                var deferral = args.Request.GetDeferral();

                var bitmap = new RenderTargetBitmap();
                await bitmap.RenderAsync(_elementToCapture);

                IBuffer pixels = await bitmap.GetPixelsAsync();
                var stream = new InMemoryRandomAccessStream();

                byte[] bytes = new byte[pixels.Length];

                using (DataReader reader = DataReader.FromBuffer(pixels))
                {
                    reader.ReadBytes(bytes);
                }

                var encoder = await BitmapEncoder.CreateAsync(
                    BitmapEncoder.BmpEncoderId,
                    stream);

                encoder.SetPixelData(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Premultiplied,
                    (uint)bitmap.PixelWidth,
                    (uint)bitmap.PixelHeight,
                    96,
                    96,
                    bytes);

                await encoder.FlushAsync();

                stream.Seek(0);

                var r = RandomAccessStreamReference.CreateFromStream(stream);

                args.Request.Data.SetBitmap(r);

                args.Request.Data.Properties.Title = "Image";
                args.Request.Data.Properties.Description = "Image of Live Tile";
                args.Request.Data.Properties.Thumbnail = r;

                deferral.Complete();
            }
        }

        protected async override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            DataTransferManager.GetForCurrentView().DataRequested -= TilePage_DataRequested;
            await MRUHelper.FlushAsync();
        }

        public static readonly DependencyProperty DocumentProperty = DependencyProperty.Register(
            "Document",
            typeof(XmlDocument),
            typeof(TilePage),
            new PropertyMetadata(null));

        public XmlDocument Document
        {
            get => (XmlDocument)GetValue(DocumentProperty);
            set => SetValue(DocumentProperty, value);
        }

        public string XmlContent
        {
            get
            {
                var systemDocument = new System.Xml.XmlDocument();
                systemDocument.LoadXml(Document.GetXml());

                var settings = new System.Xml.XmlWriterSettings
                {
                    Indent = true,
                    OmitXmlDeclaration = true
                };

                using (var stringWriter = new System.IO.StringWriter())
                {
                    using (var xmlWriter = System.Xml.XmlWriter.Create(stringWriter, settings))
                    {
                        systemDocument.WriteTo(xmlWriter);
                        xmlWriter.Flush();

                        return stringWriter.ToString();
                    }
                }
            }
        }

        public void UpdatePreview()
        {
            if (Document == null) return;

            UpdateXml();

            try
            {
                var tile = new TileNotification(Document);
                var results = new List<ParseResult>();

                foreach (var updater in _tileUpdaters)
                    results.Add(updater.Update(tile));

                warningLv.ItemsSource = results.SelectMany(x => x.Errors.Select(error => new ParseWarning
                {
                    Error = error
                }))
                // Remove duplicates
                .GroupBy(x => new { x.Error.Message, x.Error.Position.LineNumber })
                .Select(g => g.First())
                // Convert
                .ToArray();
            }
            catch
            { }
        }

        public void UpdateXml()
        {
            xmlBox.Text = XmlContent;
            _ = MruItem.Update(XmlContent);
        }

        private StorageFile OpenedFile = null;
        private MruItem MruItem = null;

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            shareButton.IsEnabled = DataTransferManager.IsSupported();
            DataTransferManager.GetForCurrentView().DataRequested += TilePage_DataRequested;

            if (e.Parameter is NotificationEditorPageNavArgs args)
            {
                string xml = args.Xml;

                OpenedFile = args.File;
                MruItem = args.MruItem;

                if (!string.IsNullOrEmpty(xml))
                {
                    Document = new XmlDocument();
                    Document.LoadXml(xml);
                }
                else
                {
                    Document = new XmlDocument();
                    Document.LoadXml(@"
<tile>
  <visual>

    <binding template=""TileMedium"">

    </binding>

    <binding template=""TileWide"">

    </binding>

    <binding template=""TileLarge"">

    </binding>
  </visual>
</tile>");
                }

                await MruItem.Update(XmlContent);

                if (IsLoaded) UpdatePreview();
            }
        }

        private List<PreviewTileUpdater> _tileUpdaters = new List<PreviewTileUpdater>();

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            ImageSource square44x44Logo = await SettingsPage.GetImageAsync(SettingsHelper.Current.PreviewSettings.Square44x44Logo)
                ?? new BitmapImage(new Uri("ms-appx:///Assets/Square44x44Logo.png"));

            ImageSource square71x71Logo = await SettingsPage.GetImageAsync(SettingsHelper.Current.PreviewSettings.Square71x71Logo)
                ?? new BitmapImage(new Uri("ms-appx:///Assets/SmallTile.png"));

            ImageSource square150x150Logo = await SettingsPage.GetImageAsync(SettingsHelper.Current.PreviewSettings.Square150x150Logo)
                ?? new BitmapImage(new Uri("ms-appx:///Assets/Square150x150Logo.png"));

            ImageSource wide310x150Logo = await SettingsPage.GetImageAsync(SettingsHelper.Current.PreviewSettings.Wide310x150Logo)
                ?? new BitmapImage(new Uri("ms-appx:///Assets/Wide310x150Logo.png"));

            ImageSource square310x310Logo = await SettingsPage.GetImageAsync(SettingsHelper.Current.PreviewSettings.Square310x310Logo)
                ?? new BitmapImage(new Uri("ms-appx:///Assets/LargeTile.png"));

            foreach (var preview in new[] { desktopSmallTile, desktopMediumTile, desktopWideTile, desktopLargeTile, mobileSmallTile,
                    mobileMediumTile, mobileWideTile, mobileLargeTile, tabletSmallTile, tabletMediumTile, tabletWideTile, tabletLargeTile })
            {
                preview.VisualElements.Square44x44Logo = square44x44Logo;
                preview.VisualElements.Square71x71Logo = square71x71Logo;
                preview.VisualElements.Square150x150Logo = square150x150Logo;
                preview.VisualElements.Wide310x150Logo = wide310x150Logo;
                preview.VisualElements.Square310x310Logo = square310x310Logo;
                preview.VisualElements.ShowNameOnSquare150x150Logo = SettingsHelper.Current.PreviewSettings.ShowNameOnSquare150x150Logo;
                preview.VisualElements.ShowNameOnSquare310x310Logo = SettingsHelper.Current.PreviewSettings.ShowNameOnSquare310x310Logo;
                preview.VisualElements.ShowNameOnWide310x150Logo   = SettingsHelper.Current.PreviewSettings.ShowNameOnWide310x150Logo;
                preview.VisualElements.BackgroundColor = SettingsHelper.Current.PreviewSettings.LiveTileBackground;
                preview.DisplayName = SettingsHelper.Current.PreviewSettings.AppDisplayName;

                _tileUpdaters.Add(preview.CreateTileUpdater());
            }

            UpdatePreview();
            ulong version = ulong.Parse(
                AnalyticsInfo.VersionInfo.DeviceFamilyVersion);

            int build = (int)((version >> 16) & 0xFFFF);
            var nearest = Builds
                .Where(x => x.LongBuildNumber <= build)
                .OrderByDescending(x => x.LongBuildNumber)
                .FirstOrDefault();

            osBuildCombo.SelectedItem = nearest;

            shareButton.IsEnabled = DataTransferManager.IsSupported();

            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5))
            {
                saveButton.KeyboardAccelerators.Add(new KeyboardAccelerator
                {
                    Key = VirtualKey.S,
                    Modifiers = VirtualKeyModifiers.Control
                });
                saveAsButton.KeyboardAccelerators.Add(new KeyboardAccelerator
                {
                    Key = VirtualKey.S,
                    Modifiers = VirtualKeyModifiers.Control | VirtualKeyModifiers.Shift
                });
            }

            InitControls();
        }

        private Guid _tileId = Guid.Empty;

        private async void PinButton_Click(object sender, RoutedEventArgs e)
        {
            if (_tileId == Guid.Empty)
                _tileId = Guid.NewGuid();

            string id = $"NotificationEditor_Tile-{_tileId}";

            var tile = new SecondaryTile(
                id,
                "Notification Editor",
                $"action=userTileClicked&id={Uri.EscapeDataString(_tileId.ToString())}",
                new Uri("ms-appx:///Assets/Square150x150Logo.png"),
                Windows.UI.StartScreen.TileSize.Default);

            tile.VisualElements.Square44x44Logo = new Uri("ms-appx:///Assets/Square44x44Logo.png");
            tile.VisualElements.Wide310x150Logo = new Uri("ms-appx:///Assets/Wide310x150Logo.png");

            bool pinned = true;

            if (!SecondaryTile.Exists(id))
                pinned = await tile.RequestCreateAsync();

            // If the tile is pinned
            if (pinned)
            {
                // Get its updater
                var updater = TileUpdateManager.CreateTileUpdaterForSecondaryTile(id);
                var notification = new TileNotification(Document);

                // And send the notification
                updater.Update(notification);
            }
        }

        private void InitControls()
        {
            if (Document.DocumentElement.SelectSingleNode("visual") is XmlElement visual)
            {
                addImageQueryToggle.IsOn = visual.GetAttribute("addImageQuery").Equals("true", StringComparison.OrdinalIgnoreCase);
                baseUriBox.Text = visual.GetAttribute("baseUri");
                brandingCombo.SelectedValue = visual.GetAttribute("branding");
                contentIdBox.Text = visual.GetAttribute("contentId");
                tileLangBox.Text = visual.GetAttribute("lang");
                versionCombo.SelectedValue = visual.GetAttribute("version");
                displayNameBox.Text = visual.GetAttribute("displayName");

                TileBindings.Clear();

                foreach (var binding in visual.ChildNodes.OfType<XmlElement>().Where(x => x.NodeType == NodeType.ElementNode
                    && x.NodeName.Equals("binding", StringComparison.OrdinalIgnoreCase)).ToList())
                {
                    bool parsedOverlay = double.TryParse(binding.GetAttribute("hint-overlay"), out double d);
                    string addQuery = binding.GetAttribute("addImageQuery");

                    var it = new TileBinding(this, binding)
                    {
                        TemplateName = binding.GetAttribute("template"),
                        FallbackName = binding.GetAttribute("fallback"),
                        BaseUri = binding.GetAttribute("baseUri"),
                        AddImageQuery = string.IsNullOrEmpty(addQuery) ? null : (bool?)addQuery.Equals("true", StringComparison.OrdinalIgnoreCase),
                        Branding = binding.GetAttribute("branding"),
                        ContentId = binding.GetAttribute("contentId"),
                        DisplayName = binding.GetAttribute("displayName"),
                        Language = binding.GetAttribute("lang"),
                        UseOverlay = parsedOverlay,
                        Overlay = parsedOverlay ? d : 0.0f,
                        Presentation = binding.GetAttribute("hint-presentation"),
                        TextStacking = binding.GetAttribute("hint-textStacking")
                    };

                    TileBindings.Add(it);

                    int iGroup = 0;
                    foreach (var el in binding.ChildNodes.OfType<XmlElement>().Where(x => x.NodeName.Equals("group", StringComparison.OrdinalIgnoreCase))
                        .ToList())
                    {
                        var group = new AdaptiveGroup
                        {
                            Name = $"Adaptive Group {++iGroup}",
                            Element = el
                        };

                        int iSubgroup = 0;
                        foreach (var child in el.ChildNodes.OfType<XmlElement>().Where(x => x.NodeName.Equals("subgroup", StringComparison.OrdinalIgnoreCase))
                            .ToList())
                        {
                            var subgroup = new AdaptiveSubgroup
                            {
                                Name = $"Subgroup {++iSubgroup}",
                                Element = child,
                                Group = group,
                                Page = this,
                                Weight = int.TryParse(child.GetAttribute("hint-weight"), out int weight) ? weight : -1,
                                TextStacking = child.GetAttribute("hint-textStacking")
                            };

                            foreach (var item in child.ChildNodes.OfType<XmlElement>().ToList())
                            {
                                if (item.NodeName.Equals("text", StringComparison.OrdinalIgnoreCase))
                                {
                                    var text = new AdaptiveText
                                    {
                                        Subgroup = subgroup,
                                        Page = this,
                                        Element = item,
                                        Id = int.TryParse(item.GetAttribute("id"), out int id) ? id : 0,
                                        Language = item.GetAttribute("lang"),
                                        Text = item.InnerText,
                                        Align = item.GetAttribute("hint-align"),
                                        MaxLines = int.TryParse(item.GetAttribute("hint-maxLines"), out int max) ? max : -1,
                                        MinLines = int.TryParse(item.GetAttribute("hint-minLines"), out int min) ? min : -1,
                                        Wrap = bool.TryParse(item.GetAttribute("hint-wrap"), out bool wrap) ? wrap : false,
                                        Style = item.GetAttribute("hint-style")
                                    };

                                    subgroup.Elements.Add(text);
                                }
                                else if (item.NodeName.Equals("image", StringComparison.OrdinalIgnoreCase))
                                {
                                    var img = new AdaptiveImage
                                    {
                                        Element = item,
                                        Page = this,
                                        Subgroup = subgroup,
                                        Id = int.TryParse(item.GetAttribute("id"), out int id) ? id : 0,
                                        Src = item.GetAttribute("src"),
                                        Alt = item.GetAttribute("alt"),
                                        AddQuery = bool.TryParse(item.GetAttribute("addImageQuery"), out bool add) ? (bool?)add : null,
                                        Align = item.GetAttribute("hint-align"),
                                        Crop = item.GetAttribute("hint-crop"),
                                        RemoveMargin = bool.TryParse(item.GetAttribute("hint-removeMargin"), out bool remove) ? remove : false
                                    };

                                    subgroup.Elements.Add(img);
                                }
                            }

                            group.Add(subgroup);
                        }

                        it.AdaptiveGroupsSource.Add(group);
                    }

                    foreach (var el in binding.ChildNodes.OfType<XmlElement>().ToList())
                    {
                        if (el.NodeName.Equals("text", StringComparison.OrdinalIgnoreCase))
                            it.Content.Add(new AdaptiveText
                            {
                                Page = this,
                                Element = el,
                                Id = int.TryParse(el.GetAttribute("id"), out int id) ? id : 0,
                                Language = el.GetAttribute("lang"),
                                Text = el.InnerText,
                                Align = el.GetAttribute("hint-align"),
                                MaxLines = int.TryParse(el.GetAttribute("hint-maxLines"), out int max) ? max : -1,
                                MinLines = int.TryParse(el.GetAttribute("hint-minLines"), out int min) ? min : -1,
                                Wrap = bool.TryParse(el.GetAttribute("hint-wrap"), out bool wrap) ? wrap : false,
                                Style = el.GetAttribute("hint-style"),
                                Binding = it
                            });
                        else if (el.NodeName.Equals("image", StringComparison.OrdinalIgnoreCase))
                            it.Content.Add(new AdaptiveImage
                            {
                                Element = el,
                                Page = this,
                                Id = int.TryParse(el.GetAttribute("id"), out int id) ? id : 0,
                                Src = el.GetAttribute("src"),
                                Alt = el.GetAttribute("alt"),
                                AddQuery = bool.TryParse(el.GetAttribute("addImageQuery"), out bool add) ? (bool?)add : null,
                                Align = el.GetAttribute("hint-align"),
                                Crop = el.GetAttribute("hint-crop"),
                                RemoveMargin = bool.TryParse(el.GetAttribute("hint-removeMargin"), out bool remove) ? remove : false,
                                Placement = el.GetAttribute("placement"),
                                Binding = it
                            });
                    }
                }
            }
        }

        private void SaveXmlButton_Click(object sender, RoutedEventArgs e)
        {
            string xml = xmlBox.Text;
            XmlDocument old = Document;

            try
            {
                Document = new XmlDocument();
                Document.LoadXml(xml, new XmlLoadSettings
                {
                    ElementContentWhiteSpace = true
                });

                UpdatePreview();
                InitControls();

                var dlg = new ContentDialog
                {
                    Content = $"The XML was loaded successfully.",
                    Title = "XML Loaded Successfully",
                    PrimaryButtonText = "OK"
                };

                _ = dlg.ShowAsync();
            }
            catch (Exception ex)
            {
                var dlg = new ContentDialog
                {
                    Content = $"An error occured.\n\n{ex.Message}",
                    Title = "Failed to load XML",
                    PrimaryButtonText = "OK"
                };

                _ = dlg.ShowAsync();

                Document = old;
            }
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }

        private async void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (OpenedFile == null)
            {
                _ = DoSaveAs();
                return;
            }

            await MruItem.Update(XmlContent);
            _ = FileIO.WriteTextAsync(OpenedFile, XmlContent);
        }

        private void SaveAsButton_Click(object sender, RoutedEventArgs e)
        {
            _ = DoSaveAs();
        }

        private async Task DoSaveAs()
        {
            FileSavePicker picker = new FileSavePicker();
            picker.FileTypeChoices.Add("XML files", new[] { ".xml" });
            picker.SuggestedSaveFile = OpenedFile;
            picker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;

            var file = await picker.PickSaveFileAsync();
            if (file != null)
            {
                OpenedFile = file;
                _ = FileIO.WriteTextAsync(OpenedFile, XmlContent);

                if (!string.IsNullOrEmpty(MruItem.Token) && StorageApplicationPermissions.FutureAccessList.ContainsItem(MruItem.Token))
                    StorageApplicationPermissions.FutureAccessList.Remove(MruItem.Token);

                MruItem.Token = StorageApplicationPermissions.FutureAccessList.Add(file);
                MruItem.Title = file.DisplayName;

                ApplicationView.GetForCurrentView().Title = file.DisplayName;
                await MruItem.Update(XmlContent);
            }
        }

        private void WarningLv_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is ParseWarning warn)
            {
                int lineNumber = warn.Error.Position.LineNumber - 1;
                int index = 0;

                for (int i = 0; i < lineNumber; i++)
                {
                    int newline = xmlBox.Text.IndexOf('\n', index);

                    if (newline == -1)
                    {
                        index = xmlBox.Text.Length;
                        break;
                    }

                    index = newline + 1;
                }

                xmlBox.Focus(FocusState.Programmatic);
                xmlBox.Select(index, 0);
            }
        }

        private void VersionCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;

            var visual = Document.DocumentElement.CreateOrSelectSingleElement("visual");

            if (!string.IsNullOrEmpty(versionCombo.SelectedValue.ToString()))
                visual.SetAttribute("version", versionCombo.SelectedValue.ToString());
            else
                visual.RemoveAttribute("version");

            UpdatePreview();
        }

        private void AddImageQueryToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;

            var visual = Document.DocumentElement.CreateOrSelectSingleElement("visual");

            if (addImageQueryToggle.IsOn)
                visual.SetAttribute("addImageQuery", "true");
            else
                visual.RemoveAttribute("addImageQuery");

            UpdatePreview();
        }

        private void BaseUriBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;

            var visual = Document.DocumentElement.CreateOrSelectSingleElement("visual");

            if (!string.IsNullOrEmpty(baseUriBox.Text))
                visual.SetAttribute("baseUri", baseUriBox.Text);
            else
                visual.RemoveAttribute("baseUri");

            UpdatePreview();
        }

        private void BrandingCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;

            var visual = Document.DocumentElement.CreateOrSelectSingleElement("visual");

            if (!string.IsNullOrEmpty(brandingCombo.SelectedValue.ToString()))
                visual.SetAttribute("branding", brandingCombo.SelectedValue.ToString());
            else
                visual.RemoveAttribute("branding");

            UpdatePreview();
        }

        private void ContentIdBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;

            var visual = Document.DocumentElement.CreateOrSelectSingleElement("visual");

            if (!string.IsNullOrEmpty(contentIdBox.Text))
                visual.SetAttribute("contentId", contentIdBox.Text);
            else
                visual.RemoveAttribute("contentId");

            UpdateXml();
        }

        private void TileLangBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;

            var visual = Document.DocumentElement.CreateOrSelectSingleElement("visual");

            if (!string.IsNullOrEmpty(tileLangBox.Text))
                visual.SetAttribute("lang", tileLangBox.Text);
            else
                visual.RemoveAttribute("lang");

            UpdatePreview();
        }

        private ObservableCollection<TileBinding> TileBindings { get; set; } = new ObservableCollection<TileBinding>();

        private void AddBindingButton_Click(object sender, RoutedEventArgs e)
        {
            var visual = Document.DocumentElement.CreateOrSelectSingleElement("visual");
            var element = Document.CreateElement("binding");

            visual.AppendChild(element);

            TileBindings.Add(new TileBinding(this, element));
            UpdatePreview();
        }

        private void DisplayNameBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var visual = Document.DocumentElement.CreateOrSelectSingleElement("visual");

            if (!string.IsNullOrEmpty(displayNameBox.Text))
                visual.SetAttribute("displayName", displayNameBox.Text);
            else
                visual.RemoveAttribute("displayName");

            UpdatePreview();
        }

        private bool _shareImage = false;
        private FrameworkElement _elementToCapture = null;

        private void TilePreview_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement element && Resources["TileImageFlyout"] is MenuFlyout flyout)
            {
                if (shareImageButton != null)
                    shareImageButton.IsEnabled = DataTransferManager.IsSupported();

                _elementToCapture = element;
                flyout.ShowAt(element, new FlyoutShowOptions
                {
                    Placement = FlyoutPlacementMode.Auto
                });
            }
        }

        private async void CopyImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_elementToCapture != null)
            {
                var bitmap = new RenderTargetBitmap();
                await bitmap.RenderAsync(_elementToCapture);

                IBuffer pixels = await bitmap.GetPixelsAsync();
                var stream = new InMemoryRandomAccessStream();

                byte[] bytes = new byte[pixels.Length];

                using (DataReader reader = DataReader.FromBuffer(pixels))
                {
                    reader.ReadBytes(bytes);
                }

                var encoder = await BitmapEncoder.CreateAsync(
                    BitmapEncoder.BmpEncoderId,
                    stream);

                encoder.SetPixelData(
                    BitmapPixelFormat.Bgra8,
                    BitmapAlphaMode.Premultiplied,
                    (uint)bitmap.PixelWidth,
                    (uint)bitmap.PixelHeight,
                    96,
                    96,
                    bytes);

                await encoder.FlushAsync();

                stream.Seek(0);

                DataPackage package = new DataPackage();
                package.SetBitmap(RandomAccessStreamReference.CreateFromStream(stream));

                Clipboard.SetContent(package);
                //stream.Dispose();
            }
        }

        private async void SaveImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (_elementToCapture != null)
            {
                var bitmap = new RenderTargetBitmap();
                await bitmap.RenderAsync(_elementToCapture);

                IBuffer buffer = await bitmap.GetPixelsAsync();
                FileSavePicker picker = new FileSavePicker();

                Dictionary<string[], Guid> encoders = new Dictionary<string[], Guid>();

                // order by reverse alphabetical
                foreach (var encoder in BitmapEncoder.GetEncoderInformationEnumerator().OrderBy(x => x.FriendlyName))
                {
                    string friendlyName = encoder.FriendlyName;

                    if (friendlyName.EndsWith("Encoder"))
                        friendlyName = friendlyName.Substring(0, friendlyName.Length - 8);

                    picker.FileTypeChoices.Add(friendlyName, encoder.FileExtensions.ToArray());
                    encoders.Add(encoder.FileExtensions.ToArray(), encoder.CodecId);
                }

                picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;

                var file = await picker.PickSaveFileAsync();
                if (file != null)
                {
                    // There's no way to retrieve the selected file type index...
                    Guid encoderId = Guid.Empty;
                    foreach (var kvp in encoders)
                    {
                        foreach (string ext in kvp.Key)
                        {
                            if (ext.Equals(file.FileType, StringComparison.OrdinalIgnoreCase))
                            {
                                encoderId = kvp.Value;
                                break;
                            }
                        }

                        if (encoderId != Guid.Empty)
                            break;
                    }

                    if (encoderId != Guid.Empty)
                    {
                        var stream = new InMemoryRandomAccessStream();
                        var encoder = await BitmapEncoder.CreateAsync(
                            encoderId,
                            stream);

                        byte[] pixels = new byte[buffer.Length];

                        using (DataReader reader = DataReader.FromBuffer(buffer))
                        {
                            reader.ReadBytes(pixels);
                        }

                        encoder.SetPixelData(
                            BitmapPixelFormat.Bgra8,
                            BitmapAlphaMode.Premultiplied,
                            (uint)bitmap.PixelWidth,
                            (uint)bitmap.PixelHeight,
                            96,
                            96,
                            pixels);

                        await encoder.FlushAsync();

                        stream.Seek(0);

                        using (var input = stream.GetInputStreamAt(0))
                        using (var output = (await file.OpenStreamForWriteAsync()).AsOutputStream())
                        {
                            await RandomAccessStream.CopyAsync(input, output);
                        }
                    }
                }
            }
        }

        private void ShareImageButton_Click(object sender, RoutedEventArgs e)
        {
            _shareImage = true;
            DataTransferManager.ShowShareUI();
        }

        private void TilePreview_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            if (sender is FrameworkElement element && Resources["TileImageFlyout"] is MenuFlyout flyout)
            {
                if (shareImageButton != null)
                    shareImageButton.IsEnabled = DataTransferManager.IsSupported();

                _elementToCapture = element;
                flyout.ShowAt(element, new FlyoutShowOptions
                {
                    Position = e.GetPosition(element),
                    Placement = FlyoutPlacementMode.RightEdgeAlignedBottom,
                    ShowMode = FlyoutShowMode.Standard
                });
            }
        }

        private void SetPrimaryButton_Click(object sender, RoutedEventArgs e)
        {
            var notification = new TileNotification(Document);
            
            TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);
        }

        private void RemoveBindingButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement el && el.Tag is TileBinding binding)
            {
                TileBindings.Remove(binding);
                binding.Element.ParentNode.RemoveChild(binding.Element);

                UpdatePreview();
            }
        }

        private void AddAdaptiveGroupButton_Click(object sender, RoutedEventArgs e)
        {
            if (adaptiveBindingCombo.SelectedItem is TileBinding binding)
            {
                XmlElement el = Document.CreateElement("group");
                binding.Element.AppendChild(el);

                binding.AdaptiveGroupsSource.Add(new AdaptiveGroup
                {
                    Name = $"Adaptive Group {binding.AdaptiveGroupsSource.Count + 1}",
                    Element = el
                });

                UpdatePreview();
            }
        }

        private void AddSubgroupBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                if (element.Tag is AdaptiveGroup grp)
                {
                    XmlElement el = Document.CreateElement("subgroup");
                    grp.Element.AppendChild(el);

                    var sub = new AdaptiveSubgroup
                    {
                        Group = grp,
                        Name = $"Subgroup {grp.Count + 1}",
                        Element = el,
                        Page = this
                    };

                    grp.Add(sub);

                    adaptiveGroupsGV.SelectedItem = sub;
                    UpdatePreview();
                }
            }
        }

        private void RemoveSubgroupBtn_Click(object sender, RoutedEventArgs e)
        {
            if (adaptiveBindingCombo.SelectedItem is TileBinding binding)
            {
                if (sender is FrameworkElement element)
                    if (element.Tag is AdaptiveGroup grp)
                    {
                        binding.AdaptiveGroupsSource.Remove(grp);

                        if (Document.SelectSingleNode("/toast/visual/binding") is XmlElement el)
                            el.RemoveChild(grp.Element);

                        UpdatePreview();
                    }
            }
        }

        private void AddTextButton_Click(object sender, RoutedEventArgs e)
        {
            if (adaptiveGroupsGV.SelectedItem is AdaptiveSubgroup subgroup)
            {
                XmlElement el = Document.CreateElement("text");
                subgroup.Element.AppendChild(el);

                subgroup.Elements.Add(new AdaptiveText
                {
                    Element = el,
                    Page = this,
                    Subgroup = subgroup
                });

                UpdatePreview();
            }
        }

        private void AddImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (adaptiveGroupsGV.SelectedItem is AdaptiveSubgroup subgroup)
            {
                XmlElement el = Document.CreateElement("image");
                subgroup.Element.AppendChild(el);

                subgroup.Elements.Add(new AdaptiveImage
                {
                    Element = el,
                    Page = this,
                    Subgroup = subgroup
                });

                UpdatePreview();
            }
        }

        private void BrowseAssetsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement el && el.Tag is Dialogs.ImageBrowserDialog dlg)
                _ = dlg.ShowAsync();
        }

        private void RemoveAdaptiveElementButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement el)
            {
                if (el.Tag is AdaptiveElement adaptive)
                {
                    adaptive.Subgroup.Elements.Remove(adaptive);
                    adaptive.Subgroup.Element.RemoveChild(adaptive.Element);

                    UpdatePreview();
                }
            }
        }

        private void RemoveSubgroupButton2_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement el)
            {
                if (el.Tag is AdaptiveSubgroup subgroup)
                {
                    subgroup.Group.Element.RemoveChild(subgroup.Element);
                    subgroup.Group.Remove(subgroup);

                    UpdatePreview();
                }
            }
        }

        private void AddContentTextButton_Click(object sender, RoutedEventArgs e)
        {
            if (contentBindingCombo.SelectedItem is TileBinding binding)
            {
                XmlElement el = Document.CreateElement("text");
                binding.Element.AppendChild(el);

                binding.Content.Add(new AdaptiveText
                {
                    Element = el,
                    Page = this,
                    Binding = binding
                });

                UpdatePreview();
            }
        }

        private void AddContentImageButton_Click(object sender, RoutedEventArgs e)
        {
            if (contentBindingCombo.SelectedItem is TileBinding binding)
            {
                XmlElement el = Document.CreateElement("image");
                binding.Element.AppendChild(el);

                binding.Content.Add(new AdaptiveImage
                {
                    Element = el,
                    Page = this,
                    Binding = binding
                });

                UpdatePreview();
            }
        }

        private void RemoveContentElementButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement el && el.Tag is AdaptiveElement a)
            {
                a.Element.ParentNode.RemoveChild(a.Element);
                a.Binding.Content.Remove(a);

                UpdatePreview();
            }
        }

        private void ListView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            if (args.DropResult != DataPackageOperation.Move)
                return;

            foreach (var item in args.Items.OfType<AdaptiveElement>())
            {
                var parent = item.Element.ParentNode;

                IList<AdaptiveElement> elements = null;

                if (item.Subgroup != null)
                    elements = item.Subgroup.Elements;
                else if (item.Binding != null)
                    elements = item.Binding.Content;

                if (elements == null)
                    continue;

                int index = elements.IndexOf(item);

                if (index < 0)
                    continue;

                var next = index + 1 < elements.Count
                    ? elements[index + 1]
                    : null;

                parent.RemoveChild(item.Element);

                if (next != null)
                    parent.InsertBefore(item.Element, next.Element);
                else
                    parent.AppendChild(item.Element);
            }

            UpdatePreview();
        }
    }
}
