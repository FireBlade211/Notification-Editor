using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Newtonsoft.Json;
using Windows.UI.Xaml;
using Newtonsoft.Json.Converters;
using System.IO;
using Windows.UI;
using System.Globalization;
using Windows.ApplicationModel;
using Windows.Storage.AccessCache;

namespace FireBlade.NotificationEditor.Helpers
{
    public class SettingsHelper
    {
        private SettingsHelper()
        { }

        public static SettingsHelper Current { get; private set; }

        public static async Task InitAsync(bool reload = false)
        {
            if (!reload && Current != null)
                return;

            // Load data.
            try
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync("config.json");

                if (file != null)
                {
                    string json = await FileIO.ReadTextAsync(file);
                    Current = JsonConvert.DeserializeObject<SettingsHelper>(json);
                }
            }
            catch (FileNotFoundException)
            {

            }

            if (Current is null)
                Current = new SettingsHelper();
        }

        [JsonProperty("theme")]
        [JsonConverter(typeof(StringEnumConverter))]
        public ElementTheme Theme { get; set; } = ElementTheme.Default;

        [JsonProperty("useRevealButtons")]
        public bool UseRevealButtons { get; set; } = false;

        [JsonProperty("previewSettings")]
        public PreviewSettings PreviewSettings { get; set; } = new PreviewSettings();

        public async Task Flush()
        {
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("config.json", CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }

    public class PreviewSettings
    {
        [JsonProperty("showAcrylicBackground")]
        public bool ShowAcrylicPaneBackground { get; set; } = true;

        [JsonProperty("tileBackground")]
        [JsonConverter(typeof(ColorJsonConverter))]
        public Color LiveTileBackground { get; set; } = Colors.BlueViolet;

        [JsonProperty("logoBackground")]
        [JsonConverter(typeof(ColorJsonConverter))]
        public Color LogoBackground { get; set; } = Colors.BlueViolet;

        [JsonProperty("displayName")]
        public string AppDisplayName { get; set; } = Package.Current.DisplayName;

        [JsonProperty("square44x44Logo")]
        public string Square44x44Logo { get; set; } = string.Empty;

        [JsonProperty("square71x71Logo")]
        public string Square71x71Logo { get; set; } = string.Empty;

        [JsonProperty("square150x150Logo")]
        public string Square150x150Logo { get; set; } = string.Empty;

        [JsonProperty("wide310x150Logo")]
        public string Wide310x150Logo { get; set; } = string.Empty;

        [JsonProperty("square310x310Logo")]
        public string Square310x310Logo { get; set; } = string.Empty;

        [JsonProperty("showNameOnSquare150x150Logo")]
        public bool ShowNameOnSquare150x150Logo { get; set; } = true;

        [JsonProperty("showNameOnSquare310x310Logo")]
        public bool ShowNameOnSquare310x310Logo { get; set; } = true;

        [JsonProperty("showNameOnWide310x150Logo")]
        public bool ShowNameOnWide310x150Logo { get; set; } = true;
    }
}
