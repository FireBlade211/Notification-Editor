using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml;
using Windows.Data.Xml.Dom;
using NotificationsVisualizerLibrary;
using Windows.ApplicationModel;
using System;
using Windows.UI;
using Windows.System.Profile;
using System.Linq;
using Windows.UI.Notifications;
using Windows.UI.Core;
using NotificationsVisualizerLibrary.Parsers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using FireBlade.NotificationEditor.Helpers;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Data;
using System.Globalization;
using Windows.Graphics.Display;
using FireBlade.NotificationEditor.DataModel;
using Windows.Storage;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.UI.ViewManagement;
using Windows.Foundation.Metadata;
using Newtonsoft.Json;
using Windows.UI.Xaml.Input;
using Windows.System;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.AccessCache;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace FireBlade.NotificationEditor.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ToastPage : Page, INotificationEditorPage
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
            },
            new WinBuildEntry
            {
                ShortBuildNumber = "Windows 11",
                LongBuildNumber = 22000
            }
        };

        public static readonly DependencyProperty DocumentProperty = DependencyProperty.Register(
            "Document",
            typeof(XmlDocument),
            typeof(ToastPage),
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

        //private bool _smallDisplayMode = false;
        //private Flyout _smallPreviewFlyout = null;

        public ToastPage()
        {
            this.InitializeComponent();

            adaptiveGroupsCVS.Source = AdaptiveGroups;

            timestampDatePicker.MaxWidth = double.PositiveInfinity;
            timestampTimePicker.MaxWidth = double.PositiveInfinity;

            //var display = DisplayInformation.GetForCurrentView();
            //if ((display.ScreenWidthInRawPixels * display.RawDpiX) < 640)
            //{
            //    showPreviewButton.IsChecked = false;
            //    //previewPane.Visibility = Visibility.Visible; // prevent the binding from updating when you check the menu item

            //    _smallDisplayMode = true;

            //    rootGrid.Children.Remove(previewPane);

            //    previewPane.Width = double.NaN;
            //    previewPane.HorizontalAlignment = HorizontalAlignment.Stretch;

            //    _smallPreviewFlyout = new Flyout
            //    {
            //        Placement = Windows.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.Full,
            //        Content = previewPane
            //    };
            //}
        }

        private void ToastPage_DataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            args.Request.Data.SetText(XmlContent);

            if (OpenedFile != null)
                args.Request.Data.SetStorageItems(new[] { OpenedFile });

            args.Request.Data.Properties.Title = "Notification";
            args.Request.Data.Properties.Description = "Toast Notification XML payload";
        }

        protected async override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            base.OnNavigatingFrom(e);

            DataTransferManager.GetForCurrentView().DataRequested -= ToastPage_DataRequested;
            await MRUHelper.FlushAsync();
        }

        public void UpdatePreview()
        {
            var results = new Dictionary<string, ParseResult>();

            try
            {
                var data = new PreviewNotificationData(DataItems.Select(x => new KeyValuePair<string, string>(x.Key, x.Value)), 0u);

                results = new Dictionary<string, ParseResult>
                {
                    { "Desktop", desktopToast.Initialize(Document, data) },
                    { "Mobile", mobileToast.Initialize(Document, data) },
                    { "Xbox", xboxToast.Initialize(Document, data) },
                    { "Xbox Action Center", xboxCenterToast.Initialize(Document, data) }
                };
            }
            catch
            { }

            warningLv.ItemsSource = results.SelectMany(x => x.Value.Errors.Select(error => new ParseWarning
            {
                Error = error,
                Source = x.Key
            }))
            // Remove duplicates
            .GroupBy(x => new { x.Source, x.Error.Message, x.Error.Position.LineNumber })
            .Select(g => g.First())
            // Convert
            .ToArray();

            UpdateXml();
        }

        public void UpdateXml()
        {
            if (!IsLoaded) return;

            xmlBox.Text = XmlContent;
            _ = MruItem.Update(XmlContent);
        }

        private StorageFile OpenedFile = null;
        private MruItem MruItem = null;

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            shareButton.IsEnabled = DataTransferManager.IsSupported();
            DataTransferManager.GetForCurrentView().DataRequested += ToastPage_DataRequested;

            if (e.Parameter is NotificationEditorPageNavArgs args)
            {
                string xml = args.Xml;

                OpenedFile = args.File;
                MruItem    = args.MruItem;

                if (!string.IsNullOrEmpty(xml))
                {
                    Document = new XmlDocument();
                    Document.LoadXml(xml);
                }
                else
                {
                    Document = new XmlDocument();
                    Document.LoadXml(
                        "<toast>\r\n" +
                        "  <visual>\r\n" +
                        "   <binding template=\"ToastGeneric\"/>\r\n" +
                        "  </visual>\r\n" +
                        "</toast>");
                }

                await MruItem.Update(XmlContent);

                foreach (XmlComment comment in Document.ChildNodes.Where(x => x.NodeType == NodeType.CommentNode).OfType<XmlComment>().ToList())
                {
                    string text = comment.NodeValue.ToString();

                    if (text.StartsWith("NotificationData"))
                    {
                        string json = text.Substring("NotificationData".Length);
                        var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

                        foreach (var item in data)
                            DataItems.Add(new NotificationDataItem(this)
                            {
                                Key = item.Key,
                                Value = item.Value
                            });

                        Document.RemoveChild(comment);
                        break;
                    }
                }

                if (IsLoaded) UpdatePreview();
            }
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            var props = new PreviewToastProperties
            {
                DisplayName = SettingsHelper.Current.PreviewSettings.AppDisplayName,
                Square44x44Logo = await SettingsPage.GetImageAsync(SettingsHelper.Current.PreviewSettings.Square44x44Logo) ??
                new BitmapImage(new Uri("ms-appx:///Assets/Square44x44Logo.png")),
                BackgroundColor = SettingsHelper.Current.PreviewSettings.LogoBackground
            };

            desktopToast.Properties = props;
            mobileToast.Properties = props;
            xboxToast.Properties = props;
            xboxCenterToast.Properties = props;

            UpdatePreview();
            ulong version = ulong.Parse(
                AnalyticsInfo.VersionInfo.DeviceFamilyVersion);

            int build = (int)((version >> 16) & 0xFFFF);
            var nearest = Builds
                .Where(x => x.LongBuildNumber <= build)
                .OrderByDescending(x => x.LongBuildNumber)
                .FirstOrDefault();

            osBuildCombo.SelectedItem = nearest;

            _notifier = ToastNotificationManager.CreateToastNotifier();
            _notifier.ScheduledToastNotificationShowing += ToastNotifier_ScheduledToastNotificationShowing;

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

        private void ToastNotifier_ScheduledToastNotificationShowing(ToastNotifier sender, ScheduledToastNotificationShowingEventArgs args)
        {
            _ = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                _scheduled = null;
                cancelScheduledBtn.IsEnabled = false;
            });
        }

        private Guid _lastNotificationGuid = Guid.Empty;

        private async void SendButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var toast = new ToastNotification(Document);
                toast.Data = new NotificationData();

                foreach (var item in DataItems)
                    toast.Data.Values[item.Key] = item.Value;

                _lastNotificationGuid = Guid.NewGuid();
                toast.Tag = "NotificationEditor_Toast-" + _lastNotificationGuid.ToString();

                _notifier.Show(toast);
            }
            catch (Exception ex)
            {
                var dlg = new ContentDialog
                {
                    Title = "An error occured.",
                    Content = ex.Message,
                    PrimaryButtonText = "OK"
                };

                if (ApiInformation.IsWriteablePropertyPresent("Windows.UI.Xaml.Controls.ContentDialog", "DefaultButton"))
                    dlg.DefaultButton = ContentDialogButton.Primary;

                _ = await dlg.ShowAsync();
            }
        }

        private ScheduledToastNotification _scheduled;
        private ToastNotifier _notifier;

        private async void ScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            if (_scheduled != null)
            {
                var d = new ContentDialog
                {
                    Title = "A toast notification is already scheduled.",
                    Content = "You cannot schedule a notification while another one is scheduled. " +
                    "Tap the Cancel Scheduled button to cancel the notification, or restart the app. " +
                    "(Note: This is not recommended, as you will not be able to cancel the old notification.)",
                    PrimaryButtonText = "OK"
                };

                _ = await d.ShowAsync();

                return;
            }

            var date = new DatePicker
            {
                Date = DateTimeOffset.Now,
                Header = "Date",
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 350,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var time = new TimePicker
            {
                Time = DateTime.Now.TimeOfDay,
                Header = "Time",
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 350,
                Margin = new Thickness(0, 0, 0, 8)
            };

            var dlg = new ContentDialog
            {
                Title = "Schedule a toast notification",
                Content = new StackPanel
                {
                    Children =
                    {
                        new TextBlock
                        {
                            Text = "Select the date and time to schedule the notification. " +
                            "Scheduled notifications have a delivery window of 5 minutes.",
                            TextWrapping = TextWrapping.WrapWholeWords,
                            Margin = new Thickness(0, 0, 0, 16)
                        },
                        date,
                        time
                    }
                },
                PrimaryButtonText = "OK",
                SecondaryButtonText = "Cancel"
            };

            var result = await dlg.ShowAsync();
            if (result == ContentDialogResult.Primary && date.SelectedDate != null && time.SelectedTime != null)
            {
                try
                {
                    var toast = new ScheduledToastNotification(Document, new DateTimeOffset(date.Date.Date + time.Time));
                    _notifier.AddToSchedule(toast);

                    _scheduled = toast;
                    cancelScheduledBtn.IsEnabled = true;
                }
                catch (Exception ex)
                {
                    dlg = new ContentDialog
                    {
                        Title = "An error occured.",
                        Content = ex.Message,
                        PrimaryButtonText = "OK"
                    };

                    _ = await dlg.ShowAsync();
                }
            }
        }

        private void CancelScheduledBtn_Click(object sender, RoutedEventArgs e)
        {
            _notifier.RemoveFromSchedule(_scheduled);
            _scheduled = null;

            cancelScheduledBtn.IsEnabled = false;
        }

        private void InitControls()
        {
            launchBox.Text = Document.DocumentElement.GetAttribute("launch");
            durationCombo.SelectedValue = Document.DocumentElement.GetAttribute("duration");

            string iso = Document.DocumentElement.GetAttribute("displayTimestamp");

            if (!string.IsNullOrEmpty(iso))
            {
                timestampCheck.IsOn = true;

                DateTimeOffset timestamp = DateTimeOffset.Parse(iso);

                timestampDatePicker.Date = timestamp;
                timestampTimePicker.Time = timestamp.TimeOfDay;
            }

            scenarioBox.SelectedValue = Document.DocumentElement.GetAttribute("scenario");

            if (Document.SelectSingleNode("/toast/actions") is XmlElement actions)
            {
                Actions.Clear();
                Inputs.Clear();
                InputOptions.Clear();

                InputOptions.Add(ToastInput.Empty);

                foreach (var node in actions.ChildNodes)
                    if (node is XmlElement element)
                    {
                        if (element.NodeName.Equals("action", StringComparison.OrdinalIgnoreCase))
                            Actions.Add(new ToastAction(this)
                            {
                                Content = element.GetAttribute("content"),
                                Args = element.GetAttribute("arguments"),
                                Type = element.GetAttribute("type"),
                                ActivationType = element.GetAttribute("activationType").OrDefault("foreground"),
                                AfterActivationBehavior = element.GetAttribute("afterActivationBehavior"),
                                Placement = element.GetAttribute("placement"),
                                ImageUri = element.GetAttribute("imageUri"),
                                ButtonStyle = element.GetAttribute("hint-buttonStyle"),
                                InputId = element.GetAttribute("hint-inputId"),
                                ToolTip = element.GetAttribute("hint-toolTip"),
                                Element = element
                            });
                        else if (element.NodeName.Equals("input", StringComparison.OrdinalIgnoreCase))
                        {
                            var inp = new ToastInput(this, element)
                            {
                                Id = element.GetAttribute("id"),
                                PlaceholderContent = element.GetAttribute("placeHolderContent"),
                                Title = element.GetAttribute("title"),
                                Type = element.GetAttribute("type")
                            };

                            foreach (var subnode in node.ChildNodes)
                            {
                                if (subnode is XmlElement el && el.NodeName.Equals("selection", StringComparison.OrdinalIgnoreCase))
                                    inp.Options.Add(new ToastSelection
                                    {
                                        Content = el.GetAttribute("content"),
                                        Id = el.GetAttribute("id"),
                                        Input = inp
                                    });
                            }

                            inp.UpdateDisplayString();

                            Inputs.Add(inp);
                            InputOptions.Add(inp);
                        }
                    }

                snoozeDismissCheck.IsChecked = actions.GetAttribute("hint-systemCommands").Equals("snoozeAndDismiss", StringComparison.OrdinalIgnoreCase);

                UpdateXml();
            }

            if (Document.SelectSingleNode("/toast/visual") is XmlElement visual)
            {
                baseUriBox.Text = visual.GetAttribute("baseUri");

                if (visual.SelectSingleNode("binding") is XmlElement binding)
                {
                    addQueryToggle.IsOn = binding.GetAttribute("addImageQuery").Equals("true", StringComparison.OrdinalIgnoreCase);
                    localeBox.Text = binding.GetAttribute("lang");

                    AdaptiveGroups.Clear();

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

                        AdaptiveGroups.Add(group);
                    }

                    ToastContent.Clear();
                    foreach (var el in binding.ChildNodes.OfType<XmlElement>().ToList())
                    {
                        if (el.NodeName.Equals("text", StringComparison.OrdinalIgnoreCase))
                            ToastContent.Add(new ToastText
                            {
                                Element = el,
                                Page = this,
                                Id = int.TryParse(el.GetAttribute("id"), out int id) ? id : 0,
                                Text = el.InnerText,
                                Placement = el.GetAttribute("placement"),
                                CallScenarioCenterAlign = el.GetAttribute("hint-callScenarioCenterAlign").Equals("true", StringComparison.OrdinalIgnoreCase)
                            });
                        else if (el.NodeName.Equals("image"))
                            ToastContent.Add(new ToastImage
                            {
                                Element = el,
                                Page = this,
                                Src = el.GetAttribute("src"),
                                Alt = el.GetAttribute("alt"),
                                Id = int.TryParse(el.GetAttribute("id"), out int id) ? id : 0,
                                AddQuery = el.GetAttribute("addImageQuery").Equals("true", StringComparison.OrdinalIgnoreCase),
                                Crop = el.GetAttribute("hint-crop"),
                                Placement = el.GetAttribute("placement")
                            });
                        else if (el.NodeName.Equals("progress"))
                        {
                            string valString = el.GetAttribute("value");

                            var progress = new ToastProgress
                            {
                                Element = el,
                                Page = this,
                                Value = double.TryParse(valString, out double v) ? (v * 100.0) : 0,
                                Indeterminate = el.GetAttribute("value").Equals("indeterminate", StringComparison.OrdinalIgnoreCase),
                                Title = el.GetAttribute("title"),
                                Status = el.GetAttribute("status"),
                                ValueStringOverride = el.GetAttribute("valueStringOverride")
                            };

                            if (valString.StartsWith("{", StringComparison.Ordinal) && valString.EndsWith("}", StringComparison.Ordinal))
                                progress.ValueBind = valString.Substring(1, valString.Length - 2);

                            ToastContent.Add(progress);
                        }
                    }
                }
            }

            if (Document.SelectSingleNode("/toast/header") is XmlElement header)
            {
                useHeaderSwitch.IsOn = true;

                hdrIdBox.Text = header.GetAttribute("id");
                hdrTitleBox.Text = header.GetAttribute("title");
                hdrArgsBox.Text = header.GetAttribute("arguments");
                hdrActBox.SelectedValue = header.GetAttribute("activationType");
            }
            else
                useHeaderSwitch.IsOn = false;

            if (Document?.DocumentElement.SelectSingleNode("audio") is XmlElement audio)
            {
                useAudioSwitch.IsOn = false; // don't use default audio

                string src = audio.GetAttribute("src");

                if (string.IsNullOrEmpty(src))
                {
                    defaultSoundRadio.IsChecked = true;
                    customSoundRadio.IsChecked = false;

                    defaultSoundBox.SelectedIndex = 0;
                }
                else if (new[] { "ms-winsoundevent:Notification.Default", "ms-winsoundevent:Notification.IM",
                "ms-winsoundevent:Notification.Mail", "ms-winsoundevent:Notification.Reminder",
                "ms-winsoundevent:Notification.SMS", "ms-winsoundevent:Notification.Looping.Alarm",
                "ms-winsoundevent:Notification.Looping.Alarm2", "ms-winsoundevent:Notification.Looping.Alarm3",
                "ms-winsoundevent:Notification.Looping.Alarm4", "ms-winsoundevent:Notification.Looping.Alarm5",
                "ms-winsoundevent:Notification.Looping.Alarm6", "ms-winsoundevent:Notification.Looping.Alarm7",
                "ms-winsoundevent:Notification.Looping.Alarm8", "ms-winsoundevent:Notification.Looping.Alarm9",
                "ms-winsoundevent:Notification.Looping.Alarm10", "ms-winsoundevent:Notification.Looping.Call",
                "ms-winsoundevent:Notification.Looping.Call2", "ms-winsoundevent:Notification.Looping.Call3",
                "ms-winsoundevent:Notification.Looping.Call4", "ms-winsoundevent:Notification.Looping.Call5",
                "ms-winsoundevent:Notification.Looping.Call6", "ms-winsoundevent:Notification.Looping.Call7",
                "ms-winsoundevent:Notification.Looping.Call8", "ms-winsoundevent:Notification.Looping.Call9",
                "ms-winsoundevent:Notification.Looping.Call10"}.Contains(src, StringComparer.Ordinal))
                {
                    // don't ignore case else SelectedValue won't match since it's case-sensitive
                    defaultSoundRadio.IsChecked = true;
                    customSoundRadio.IsChecked = false;

                    defaultSoundBox.SelectedValue = src;
                }
                else
                {
                    defaultSoundRadio.IsChecked = false;
                    customSoundRadio.IsChecked = true;

                    customSoundUriBox.Text = src;
                }

                loopAudioSwitch.IsOn = audio.GetAttribute("loop").Equals("true", StringComparison.OrdinalIgnoreCase);
                audioSilentSwitch.IsOn = audio.GetAttribute("silent").Equals("true", StringComparison.OrdinalIgnoreCase);
            }
            else
                useAudioSwitch.IsOn = true;

            UpdateSoundRadioButtonsState();
        }

        private void LaunchBox_LostFocus(object sender, RoutedEventArgs e)
        {
            Document.DocumentElement.SetAttribute("launch", launchBox.Text);
            UpdateXml();
        }

        private void DurationCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string dur = durationCombo.SelectedValue as string;

            if (!string.IsNullOrEmpty(dur))
                Document.DocumentElement.SetAttribute("duration", dur);
            else
                Document.DocumentElement.RemoveAttribute("duration");

            UpdateXml();
        }

        private void WriteDisplayTimestamp()
        {
            DateTimeOffset timestamp = new DateTimeOffset(
                timestampDatePicker.Date.Date +
                timestampTimePicker.Time,
                timestampDatePicker.Date.Offset);

            Document.DocumentElement.SetAttribute("displayTimestamp", timestamp.ToString("o"));
        }

        private void TimestampDatePicker_DateChanged(object sender, DatePickerValueChangedEventArgs e)
        {
            WriteDisplayTimestamp();
            UpdateXml();
        }

        private void TimestampCheck_Toggled(object sender, RoutedEventArgs e)
        {
            if (timestampCheck.IsOn)
                WriteDisplayTimestamp();
            else
                Document.DocumentElement.RemoveAttribute("displayTimestamp");

            UpdateXml();
        }

        private void TimestampTimePicker_TimeChanged(object sender, TimePickerValueChangedEventArgs e)
        {
            WriteDisplayTimestamp();
            UpdateXml();
        }

        private void ScenarioBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var scenario = scenarioBox.SelectedValue as string;

            if (!string.IsNullOrEmpty(scenario))
                Document.DocumentElement.SetAttribute("scenario", scenario);
            else
                Document.DocumentElement.RemoveAttribute("scenario");

            UpdatePreview();
        }

        private void OsBuildCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdatePreview();
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

        public ObservableCollection<ToastAction> Actions { get; set; } = new ObservableCollection<ToastAction>();
        public ObservableCollection<ToastInput> Inputs { get; set; } = new ObservableCollection<ToastInput>();
        public ObservableCollection<ToastInput> InputOptions { get; set; } = new ObservableCollection<ToastInput>();

        private void AddActionBtn_Click(object sender, RoutedEventArgs e)
        {
            var actions = Document.SelectSingleNode("/toast/actions");
            XmlElement element = null;

            if (actions == null)
            {
                actions = Document.CreateElement("actions");
                Document.DocumentElement.AppendChild(actions);
            }

            if (actions != null)
            {
                element = Document.CreateElement("action");
                actions.AppendChild(element);
            }

            var action = new ToastAction(this, element);
            Actions.Add(action);

            actionsLv.ScrollIntoView(action);
            UpdatePreview();
        }

        private void RemoveActionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
                if (btn.Tag is ToastAction action)
                {
                    Actions.Remove(action);

                    action.Element.ParentNode.RemoveChild(action.Element);
                    UpdatePreview();
                }
        }

        private void ButtonStylesToggle_Toggled(object sender, RoutedEventArgs e)
        {
            if (buttonStylesToggle.IsOn)
                Document.DocumentElement.SetAttribute("useButtonStyle", "true");
            else
                Document.DocumentElement.RemoveAttribute("useButtonStyle");

            UpdatePreview();
        }

        private void AddInputBtn_Click(object sender, RoutedEventArgs e)
        {
            var actions = Document.DocumentElement.CreateOrSelectSingleElement("actions");

            XmlElement el = Document.CreateElement("input");
            el.SetAttribute("type", "text");
            actions.AppendChild(el);

            var action = new ToastInput(this, el);

            Inputs.Add(action);
            InputOptions.Add(action);

            inputsLv.ScrollIntoView(action);
            UpdatePreview();
        }

        private void RemoveInputBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
                if (btn.Tag is ToastInput input)
                {
                    Inputs.Remove(input);
                    InputOptions.Remove(input);

                    input.Element.ParentNode.RemoveChild(input.Element);
                    UpdatePreview();
                }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is ComboBox cb)
                cb.SelectedItem = null;
        }

        private void RemoveSelectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement f && f.Tag is ToastSelection sel)
            {
                sel.Input.Element.RemoveChild(sel.Element);

                sel.Input.Options.Remove(sel);
                UpdatePreview();
            }
        }

        private void AddSelectionBtn_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement f && f.Tag is ToastInput input)
            {
                input.Options.Add(new ToastSelection
                {
                    Input = input
                });

                XmlElement el = Document.CreateElement("selection");
                input.Element.AppendChild(el);

                UpdatePreview();
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

        private void SnoozeDismissCheck_Checked(object sender, RoutedEventArgs e)
        {
            var actions = Document.SelectSingleNode("/toast/actions") as XmlElement;

            if (actions == null)
            {
                actions = Document.CreateElement("actions");
                Document.DocumentElement.AppendChild(actions);
            }

            if (actions != null)
            {
                if (snoozeDismissCheck.IsChecked == true)
                    actions.SetAttribute("hint-systemCommands", "snoozeAndDismiss");
                else
                    actions.RemoveAttribute("hint-systemCommands");
            }

            UpdatePreview();
        }

        private void AddQueryToggle_Toggled(object sender, RoutedEventArgs e)
        {
            var binding = Document.CreateOrSelectSingleElement("/toast/visual/binding");

            if (binding != null)
            {
                if (addQueryToggle.IsOn)
                    binding.SetAttribute("addImageQuery", "true");
                else
                    binding.RemoveAttribute("addImageQuery");
            }

            UpdatePreview();
        }

        private void ShareButton_Click(object sender, RoutedEventArgs e)
        {
            DataTransferManager.ShowShareUI();
        }

        private void BaseUriBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var binding = Document.CreateOrSelectSingleElement("/toast/visual");

            if (binding != null)
            {
                if ((!string.IsNullOrEmpty(baseUriBox.Text)) && !baseUriBox.Text.Equals("ms-appx:///", StringComparison.OrdinalIgnoreCase))
                    binding.SetAttribute("baseUri", baseUriBox.Text);
                else
                    binding.RemoveAttribute("baseUri");
            }

            UpdatePreview();
        }

        private void LocaleBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var binding = Document.CreateOrSelectSingleElement("/toast/visual/binding");

            if (binding != null)
            {
                if (!string.IsNullOrEmpty(localeBox.Text))
                    binding.SetAttribute("lang", localeBox.Text);
                else
                    binding.RemoveAttribute("lang");
            }

            UpdatePreview();
        }

        private ObservableCollection<AdaptiveGroup> AdaptiveGroups = new ObservableCollection<AdaptiveGroup>();

        private void AddAdaptiveGroupButton_Click(object sender, RoutedEventArgs e)
        {
            XmlElement el = Document.CreateElement("group");
            Document.CreateOrSelectSingleElement("/toast/visual/binding").AppendChild(el);

            AdaptiveGroups.Add(new AdaptiveGroup
            {
                Name = $"Adaptive Group {AdaptiveGroups.Count + 1}",
                Element = el
            });

            UpdatePreview();
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
            if (sender is FrameworkElement element)
                if (element.Tag is AdaptiveGroup grp)
                {
                    AdaptiveGroups.Remove(grp);

                    if (Document.SelectSingleNode("/toast/visual/binding") is XmlElement el)
                        el.RemoveChild(grp.Element);

                    UpdatePreview();
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

        private void ShowPreviewButton_Click(object sender, RoutedEventArgs e)
        {
            //if (_smallDisplayMode)
            //{
            //    _ = Dispatcher.RunIdleAsync(args =>
            //    {
            //        _smallPreviewFlyout?.ShowAt(this);
            //        showPreviewButton.IsChecked = true;
            //    });

            //}
        }

        private void RemoveElementButton_Click(object sender, RoutedEventArgs e)
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

        private void Button_Click(object sender, RoutedEventArgs e)
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

        private async Task WriteXmlToFile(StorageFile file)
        {
            string xml = XmlContent;

            if (DataItems.Any())
            {
                var dict = new Dictionary<string, string>();

                foreach (var item in DataItems)
                    dict[item.Key] = item.Value;

                xml += "<!--NotificationData\r\n\r\n";
                xml += JsonConvert.SerializeObject(dict, Formatting.Indented);
                xml += "\r\n-->";
            }

            await FileIO.WriteTextAsync(OpenedFile, xml);
            await MruItem.Update(xml);
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (OpenedFile == null)
            {
                _ = DoSaveAs();
                return;
            }

            _ = WriteXmlToFile(OpenedFile);
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
                _ = WriteXmlToFile(file);

                if (!string.IsNullOrEmpty(MruItem.Token) && StorageApplicationPermissions.FutureAccessList.ContainsItem(MruItem.Token))
                    StorageApplicationPermissions.FutureAccessList.Remove(MruItem.Token);

                MruItem.Token = StorageApplicationPermissions.FutureAccessList.Add(file);
                MruItem.Title = file.DisplayName;

                ApplicationView.GetForCurrentView().Title = file.DisplayName;

                await MruItem.Update(XmlContent);
            }
        }

        //private void ButtonContentBox_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    if (sender is TextBox box)
        //    {
        //        if (box.Tag is ToastAction action)
        //        {
        //            int i = Actions.IndexOf(action);

        //            if (Document.SelectSingleNode("/toast/actions") is XmlElement element)
        //            {
        //                element = element.ChildNodes[i] as XmlElement;

        //                if (element != null)
        //                {
        //                    element.SetAttribute("content", box.Text);
        //                }
        //            }
        //        }
        //    }
        //}

        //private void ButtonArgsBox_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    if (sender is TextBox box)
        //    {
        //        if (box.Tag is ToastAction action)
        //        {
        //            int i = Actions.IndexOf(action);

        //            if (Document.SelectSingleNode("/toast/actions") is XmlElement element)
        //            {
        //                element = element.ChildNodes[i] as XmlElement;

        //                if (element != null)
        //                {
        //                    element.SetAttribute("arguments", box.Text);
        //                }
        //            }
        //        }
        //    }
        //}

        //private void ButtonTypeBox_LostFocus(object sender, RoutedEventArgs e)
        //{
        //    if (sender is TextBox box)
        //    {
        //        if (box.Tag is ToastAction action)
        //        {
        //            int i = Actions.IndexOf(action);

        //            if (Document.SelectSingleNode("/toast/actions") is XmlElement element)
        //            {
        //                element = element.ChildNodes[i] as XmlElement;

        //                if (element != null)
        //                {
        //                    element.SetAttribute("type", box.Text);
        //                }
        //            }
        //        }
        //    }
        //}

        public ObservableCollection<ToastElement> ToastContent { get; set; } = new ObservableCollection<ToastElement>();

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var binding = Document.DocumentElement.CreateOrSelectSingleElement("/visual/binding");
            var element = Document.CreateElement("image");

            binding.AppendChild(element);

            var item = new ToastImage
            {
                Element = element,
                Page = this
            };

            ToastContent.Add(item);
            UpdatePreview();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            var binding = Document.DocumentElement.CreateOrSelectSingleElement("/visual/binding");
            var element = Document.CreateElement("progress");

            element.SetAttribute("status", string.Empty);
            element.SetAttribute("value", "0.0");

            binding.AppendChild(element);

            var item = new ToastProgress
            {
                Element = element,
                Page = this
            };

            ToastContent.Add(item);
            UpdatePreview();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            var binding = Document.DocumentElement.CreateOrSelectSingleElement("/visual/binding");
            var element = Document.CreateElement("text");

            binding.AppendChild(element);

            var item = new ToastText
            {
                Element = element,
                Page = this
            };

            ToastContent.Add(item);
            UpdatePreview();
        }

        private void RemoveToastElementButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement ui && ui.Tag is ToastElement element)
            {
                element.Element.ParentNode.RemoveChild(element.Element);
                ToastContent.Remove(element);

                UpdatePreview();
            }
        }

        private void BrowseAssetsButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement el && el.Tag is Dialogs.ImageBrowserDialog dlg)
                _ = dlg.ShowAsync();
        }

        private void UseHeaderSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (useHeaderSwitch.IsOn)
            {
                var element = Document.CreateElement("header");
                Document.DocumentElement.AppendChild(element);

                element.SetAttribute("id", hdrIdBox.Text);
                element.SetAttribute("title", hdrTitleBox.Text);
                element.SetAttribute("arguments", hdrArgsBox.Text);

                string type = hdrActBox.SelectedValue.ToString();

                if (!string.IsNullOrEmpty(type))
                    element.SetAttribute("activationType", type);
            }
            else
            {
                var element = Document.DocumentElement.SelectSingleNode("header");

                if (element != null)
                    Document.DocumentElement.RemoveChild(element);
            }

            UpdatePreview();
        }

        private void HdrIdBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Document?.DocumentElement.SelectSingleNode("header") is XmlElement element)
                element.SetAttribute("id", hdrIdBox.Text);

            UpdatePreview();
        }

        private void HdrTitleBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Document?.DocumentElement.SelectSingleNode("header") is XmlElement element)
                element.SetAttribute("title", hdrTitleBox.Text);

            UpdatePreview();
        }

        private void HdrArgsBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Document?.DocumentElement.SelectSingleNode("header") is XmlElement element)
                element.SetAttribute("arguments", hdrArgsBox.Text);

            UpdatePreview();
        }

        private void HdrActBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;

            if (Document?.DocumentElement.SelectSingleNode("header") is XmlElement element)
            {
                string type = hdrActBox.SelectedValue.ToString();

                if (string.IsNullOrEmpty(type))
                    element.RemoveAttribute("activationType");
                else
                    element.SetAttribute("activationType", type);
            }

            UpdatePreview();
        }

        private void UseAudioSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            UpdateSoundRadioButtonsState();

            if (useAudioSwitch.IsOn)
            {
                if (Document?.DocumentElement.SelectSingleNode("audio") is XmlElement audio)
                    Document?.DocumentElement.RemoveChild(audio);
            }
            else
            {
                var audio = Document.CreateElement("audio");
                Document?.DocumentElement.AppendChild(audio);

                string uri = defaultSoundRadio.IsChecked == true ? defaultSoundBox.SelectedValue.ToString() : customSoundUriBox.Text;

                if (!string.IsNullOrEmpty(uri))
                    audio.SetAttribute("src", uri);

                if (loopAudioSwitch.IsOn)
                    audio.SetAttribute("loop", "true");

                if (audioSilentSwitch.IsOn)
                    audio.SetAttribute("silent", "true");
            }

            UpdateXml();
        }

        private void UpdateSoundRadioButtonsState()
        {
            if (!IsLoaded) return;

            defaultSoundRadio.IsEnabled = !useAudioSwitch.IsOn;
            defaultSoundBox.IsEnabled = defaultSoundRadio.IsChecked == true && !useAudioSwitch.IsOn;
            customSoundRadio.IsEnabled = !useAudioSwitch.IsOn;
            customSoundUriBox.IsEnabled = customSoundRadio.IsChecked == true && !useAudioSwitch.IsOn;
        }

        private void DefaultSoundRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsLoaded) return;

            if (defaultSoundRadio.IsChecked == true)
            {
                customSoundRadio.IsChecked = false;

                if (Document?.DocumentElement.SelectSingleNode("audio") is XmlElement audio)
                {
                    string uri = defaultSoundBox.SelectedValue.ToString();

                    if (!string.IsNullOrEmpty(uri))
                        audio.SetAttribute("src", uri);
                    else
                        audio.RemoveAttribute("src");
                }
            }

            UpdateXml();
            UpdateSoundRadioButtonsState();
        }

        private void CustomSoundRadio_Checked(object sender, RoutedEventArgs e)
        {
            if (customSoundRadio.IsChecked == true)
            {
                defaultSoundRadio.IsChecked = false;

                if (Document?.DocumentElement.SelectSingleNode("audio") is XmlElement audio)
                {
                    string uri = customSoundUriBox.Text;

                    if (!string.IsNullOrEmpty(uri))
                        audio.SetAttribute("src", uri);
                    else
                        audio.RemoveAttribute("src");
                }

                UpdateXml();
            }

            UpdateSoundRadioButtonsState();
        }

        private void DefaultSoundBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (Document?.DocumentElement.SelectSingleNode("audio") is XmlElement audio)
            {
                string uri = defaultSoundBox.SelectedValue.ToString();

                if (!string.IsNullOrEmpty(uri))
                    audio.SetAttribute("src", uri);
                else
                    audio.RemoveAttribute("src");
            }

            UpdateXml();
        }

        private void CustomSoundUriBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Document?.DocumentElement.SelectSingleNode("audio") is XmlElement audio)
            {
                string uri = customSoundUriBox.Text;

                if (!string.IsNullOrEmpty(uri))
                    audio.SetAttribute("src", uri);
                else
                    audio.RemoveAttribute("src");
            }

            UpdateXml();
        }

        private void LoopAudioSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (Document?.DocumentElement.SelectSingleNode("audio") is XmlElement audio)
            {
                if (loopAudioSwitch.IsOn)
                    audio.SetAttribute("loop", "true");
                else
                    audio.RemoveAttribute("loop");

                UpdateXml();
            }
        }

        private void AudioSilentSwitch_Toggled(object sender, RoutedEventArgs e)
        {
            if (Document?.DocumentElement.SelectSingleNode("audio") is XmlElement audio)
            {
                if (audioSilentSwitch.IsOn)
                    audio.SetAttribute("silent", "true");
                else
                    audio.RemoveAttribute("silent");

                UpdateXml();
            }
        }

        public ObservableCollection<NotificationDataItem> DataItems { get; set; } = new ObservableCollection<NotificationDataItem>();

        private void AddDataEntryButton_Click(object sender, RoutedEventArgs e)
        {
            DataItems.Add(new NotificationDataItem(this));
        }

        private void DeleteDataItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement el && el.Tag is NotificationDataItem item)
                DataItems.Remove(item);
        }

        private void UpdateToastButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                NotificationData data = new NotificationData();

                foreach (var item in DataItems)
                    data.Values[item.Key] = item.Value;

                _notifier.Update(data, "NotificationEditor_Toast-" + _lastNotificationGuid.ToString());
            }
            catch (Exception ex)
            {
                var dlg = new ContentDialog
                {
                    Title = "An error occured.",
                    Content = ex.Message,
                    PrimaryButtonText = "OK"
                };

                if (ApiInformation.IsWriteablePropertyPresent("Windows.UI.Xaml.Controls.ContentDialog", "DefaultButton"))
                    dlg.DefaultButton = ContentDialogButton.Primary;

                _ = dlg.ShowAsync();
            }
        }

        private void ClearProgressBindButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement el && el.Tag is ToastProgress prog)
                prog.ValueBind = string.Empty;
        }

        private void ListView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            if (args.DropResult != DataPackageOperation.Move)
                return;

            foreach (var item in args.Items.OfType<AdaptiveElement>())
            {
                var parent = item.Element.ParentNode;

                IList<AdaptiveElement> elements = item.Subgroup.Elements;

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

        private void ContentListView_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            if (args.DropResult != DataPackageOperation.Move)
                return;

            foreach (var item in args.Items.OfType<ToastElement>())
            {
                var parent = item.Element.ParentNode;
                int index = ToastContent.IndexOf(item);

                if (index < 0)
                    continue;

                var next = index + 1 < ToastContent.Count
                    ? ToastContent[index + 1]
                    : null;

                parent.RemoveChild(item.Element);

                if (next != null)
                    parent.InsertBefore(item.Element, next.Element);
                else
                    parent.AppendChild(item.Element);
            }

            UpdatePreview();
        }

        private void ActionsLv_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            if (args.DropResult != DataPackageOperation.Move)
                return;

            foreach (var item in args.Items.OfType<ToastAction>())
            {
                var parent = item.Element.ParentNode;
                int index = Actions.IndexOf(item);

                if (index < 0)
                    continue;

                var next = index + 1 < Actions.Count
                    ? Actions[index + 1]
                    : null;

                parent.RemoveChild(item.Element);

                if (next != null)
                    parent.InsertBefore(item.Element, next.Element);
                else
                    parent.AppendChild(item.Element);
            }

            UpdatePreview();
        }

        private void InputsLv_DragItemsCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            if (args.DropResult != DataPackageOperation.Move)
                return;

            foreach (var item in args.Items.OfType<ToastInput>())
            {
                var parent = item.Element.ParentNode;
                int index = Inputs.IndexOf(item);

                if (index < 0)
                    continue;

                var next = index + 1 < Inputs.Count
                    ? Inputs[index + 1]
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

    public class WinBuildEntry
    {
        public string ShortBuildNumber { get; set; }
        public int LongBuildNumber { get; set; }

        public string DisplayString => $"{ShortBuildNumber} ({LongBuildNumber})";
    }
}
