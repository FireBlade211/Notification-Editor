using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Windows.Storage;
using System.IO;
using Windows.Storage.AccessCache;
using Windows.UI.StartScreen;
using Windows.Foundation.Metadata;
using Newtonsoft.Json.Converters;
using Windows.UI.Xaml.Controls;
using System.ComponentModel;
using Windows.Data.Xml.Dom;

namespace FireBlade.NotificationEditor.Helpers
{
    /// <summary>
    /// Represents a helper for storing the user's MRU (Most Recently Used) files.
    /// </summary>
    public static class MRUHelper
    {
        public static List<MruItem> Items { get; set; } = new List<MruItem>();

        public static async Task InitAsync()
        {
            try
            {
                var file = await ApplicationData.Current.LocalFolder.GetFileAsync("mru.json");

                Items = JsonConvert.DeserializeObject<List<MruItem>>(await FileIO.ReadTextAsync(file));
                await CleanItems();
            }
            catch (FileNotFoundException)
            {

            }
            catch (JsonException)
            { }

            if (Items is null)
                Items = new List<MruItem>();
        }

        public static async Task<MruItem> FindOrCreateForFileAsync(StorageFile file, MruItemType type)
        {
            var found = await Items.FirstOrDefaultAsync(async x =>
            {
                if (!string.IsNullOrEmpty(x.Token) && StorageApplicationPermissions.FutureAccessList.ContainsItem(x.Token))
                {
                    var f = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(x.Token);

                    return f != null && f.IsEqual(file);
                }

                return false;
            });

            if (found != null)
                return found;

            var item = new MruItem(file.DisplayName, type)
            {
                Token = StorageApplicationPermissions.FutureAccessList.Add(file)
            };

            Items.Add(item);
            await item.Update(await FileIO.ReadTextAsync(file));

            await CleanItems();

            return item;
        }

        public static async Task CleanItems()
        {
            // Remove old items.
            if (Items.Count > 15)
            {
                for (int i = Items.Count - 1; i >= 15; i--)
                {
                    await Items[i].Remove();
                }
            }

            // Determine the types of indeterminate items.
            foreach (var item in Items.Where(x => x.Type == MruItemType.Indeterminate))
            {
                try
                {
                    if (!string.IsNullOrEmpty(item.Token) && StorageApplicationPermissions.FutureAccessList.ContainsItem(item.Token))
                    {
                        var file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(item.Token);
                        item.Xml = await FileIO.ReadTextAsync(file);
                    }

                    var doc = new XmlDocument();
                    doc.LoadXml(item.Xml);

                    if (doc.DocumentElement != null)
                    {
                        if (doc.DocumentElement.NodeName.Equals("toast", StringComparison.OrdinalIgnoreCase))
                            item.Type = MruItemType.Toast;
                        else if (doc.DocumentElement.NodeName.Equals("tile", StringComparison.OrdinalIgnoreCase))
                            item.Type = MruItemType.Tile;
                    }
                }
                catch
                {
                    
                }
            }

            // Add to the jump list.
            if (App.IsUniversalContractV2Present)
            {
                if (JumpList.IsSupported())
                {
                    var list = await JumpList.LoadCurrentAsync();
                    foreach (var item in Items)
                    {
                        var found = list.Items.FirstOrDefault(x => Guid.TryParse(x.Arguments.Substring("mru:".Length), out Guid id)
                            && id == item.Id);

                        if (found != null && found.RemovedByUser)
                            await item.Remove();
                    }
                }
            }

            // Sort items.
            Items = Items.OrderByDescending(x => x.DateModified).ToList();
        }

        public static async Task FlushAsync()
        {
            try
            {
                await CleanItems();

                var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("mru.json",
                    CreationCollisionOption.OpenIfExists);

                await FileIO.WriteTextAsync(file, JsonConvert.SerializeObject(Items, Formatting.Indented));
            }
            catch
            {

            }
        }

        public static bool TryGetItemById(Guid id, out MruItem item)
        {
            item = null;

            var i = Items.FirstOrDefault(x => x.Id.Equals(id));
            if (i != null)
            {
                item = i;
                return true;
            }

            return false;
        }
    }

    public class MruItem : IEquatable<MruItem>, INotifyPropertyChanged
    {
        [JsonConstructor]
        protected MruItem()
        { }

        public MruItem(string title, MruItemType type)
        {
            Id = Guid.NewGuid();
            DateModified = DateTime.UtcNow;
            Title = title;
            Type = type;
        }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        private string _title = string.Empty;

        [JsonProperty("title")]
        public string Title
        {
            get => _title;
            set
            {
                if (value != _title)
                {
                    _title = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Title)));
                }
            }
        }

        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("dateModified")]
        public DateTime DateModified { get; set; }

        // for data binding
        [JsonIgnore]
        public string DateModifiedString => DateModified.ToLocalTime().ToString();

        [JsonProperty("type")]
        [JsonConverter(typeof(StringEnumConverter))]
        public MruItemType Type { get; set; }

        // for data binding
        [JsonIgnore]
        public IconElement Icon => new FontIcon
        {
            Glyph = Type == MruItemType.Tile ? "\uECA5" : Type == MruItemType.Toast ? "\uE7E7" : "\uE897"
        };

        [JsonProperty("xml")]
        public string Xml { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        public bool Equals(MruItem other) => other?.Id.Equals(Id) == true;

        public override bool Equals(object obj) => obj is MruItem i && i.Id.Equals(Id);
        public override int GetHashCode() => Id.GetHashCode();

        /// <summary>
        /// Removes the item from the MRU list.
        /// </summary>
        public async Task Remove()
        {
            MRUHelper.Items.Remove(this);

            if (!string.IsNullOrEmpty(Token) && StorageApplicationPermissions.FutureAccessList.ContainsItem(Token))
                StorageApplicationPermissions.FutureAccessList.Remove(Token);

            if (App.IsUniversalContractV2Present)
            {
                if (JumpList.IsSupported())
                {
                    var list = await JumpList.LoadCurrentAsync();
                    var found = list.Items.FirstOrDefault(x => Guid.TryParse(x.Arguments.Substring("mru:".Length),
                        out Guid id)
                    && id == Id);

                    if (found != null)
                        list.Items.Remove(found);

                    await list.SaveAsync();
                }
            }

            await MRUHelper.FlushAsync();
        }

        /// <summary>
        /// Updates the MRU item with the specified XML.
        /// </summary>
        /// <param name="xml">The XML</param>
        public async Task Update(string xml)
        {
            Xml = xml;
            DateModified = DateTime.UtcNow;

            if (App.IsUniversalContractV2Present)
            {
                if (JumpList.IsSupported())
                {
                    var list = await JumpList.LoadCurrentAsync();

                    list.SystemGroupKind = JumpListSystemGroupKind.Recent;

                    var found = list.Items.FirstOrDefault(x => Guid.TryParse(x.Arguments.Substring("mru:".Length), out Guid id)
                    && id == Id);
                    if (found != null)
                    {
                        list.Items.Remove(found);
                        list.Items.Insert(0, found);
                    }
                    else
                    {
                        var item = JumpListItem.CreateWithArguments("mru:" + Id.ToString("D"), Title);

                        item.GroupName = "Recent";
                        item.Logo = Type == MruItemType.Toast ? new Uri("ms-appx:///Assets/MDL2/toast.png") : 
                            new Uri("ms-appx:///Assets/MDL2/tile.png");
                        
                        if (!string.IsNullOrEmpty(Token) && StorageApplicationPermissions.FutureAccessList.ContainsItem(Token))
                        {
                            var file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(Token);

                            if (file != null)
                                item.Description = file.Path;
                        }

                        list.Items.Add(item);
                    }

                    await list.SaveAsync();
                }
            }

            if (!string.IsNullOrEmpty(Token) && StorageApplicationPermissions.FutureAccessList.ContainsItem(Token))
            {
                var file = await StorageApplicationPermissions.FutureAccessList.GetFileAsync(Token);

                if (file != null)
                    await FileIO.WriteTextAsync(file, xml);
            }
        }
    }

    public enum MruItemType
    {
        /// <summary>
        /// The MRU entry is a live tile.
        /// </summary>
        Tile,
        /// <summary>
        /// The MRU entry is a toast notification.
        /// </summary>
        Toast,
        /// <summary>
        /// The type of the MRU entry hasn't been determined yet. The type will be determined when the <see cref="MRUHelper.CleanItems"/>
        /// functions is called.
        /// </summary>
        Indeterminate
    }
}
