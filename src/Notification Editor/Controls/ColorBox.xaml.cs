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
using System.Threading.Tasks;
using Windows.UI;
using Windows.Foundation.Metadata;
using System.Reflection;
using System.Globalization;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace FireBlade.NotificationEditor.Controls
{
    public sealed partial class ColorBox : UserControl
    {
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register(
            "Color",
            typeof(Color),
            typeof(ColorBox),
            new PropertyMetadata(Colors.Transparent, OnColorChanged));

        public Color Color
        {
            get => (Color)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header",
            typeof(object),
            typeof(ColorBox),
            new PropertyMetadata(null));

        public object Header
        {
            get => GetValue(HeaderProperty);
            set => SetValue(HeaderProperty, value);
        }

        public event EventHandler ColorChanged;

        private static readonly Dictionary<Color, string> _knownColors = new Dictionary<Color, string>()
        {
            [Colors.Transparent] = nameof(Colors.Transparent),

            [Colors.AliceBlue] = nameof(Colors.AliceBlue),
            [Colors.AntiqueWhite] = nameof(Colors.AntiqueWhite),
            [Colors.Aqua] = nameof(Colors.Aqua),
            [Colors.Aquamarine] = nameof(Colors.Aquamarine),
            [Colors.Azure] = nameof(Colors.Azure),
            [Colors.Beige] = nameof(Colors.Beige),
            [Colors.Bisque] = nameof(Colors.Bisque),
            [Colors.Black] = nameof(Colors.Black),
            [Colors.BlanchedAlmond] = nameof(Colors.BlanchedAlmond),
            [Colors.Blue] = nameof(Colors.Blue),
            [Colors.BlueViolet] = nameof(Colors.BlueViolet),
            [Colors.Brown] = nameof(Colors.Brown),
            [Colors.BurlyWood] = nameof(Colors.BurlyWood),
            [Colors.CadetBlue] = nameof(Colors.CadetBlue),
            [Colors.Chartreuse] = nameof(Colors.Chartreuse),
            [Colors.Chocolate] = nameof(Colors.Chocolate),
            [Colors.Coral] = nameof(Colors.Coral),
            [Colors.CornflowerBlue] = nameof(Colors.CornflowerBlue),
            [Colors.Cornsilk] = nameof(Colors.Cornsilk),
            [Colors.Crimson] = nameof(Colors.Crimson),
            [Colors.Cyan] = nameof(Colors.Cyan),

            [Colors.DarkBlue] = nameof(Colors.DarkBlue),
            [Colors.DarkCyan] = nameof(Colors.DarkCyan),
            [Colors.DarkGoldenrod] = nameof(Colors.DarkGoldenrod),
            [Colors.DarkGray] = nameof(Colors.DarkGray),
            [Colors.DarkGreen] = nameof(Colors.DarkGreen),
            [Colors.DarkKhaki] = nameof(Colors.DarkKhaki),
            [Colors.DarkMagenta] = nameof(Colors.DarkMagenta),
            [Colors.DarkOliveGreen] = nameof(Colors.DarkOliveGreen),
            [Colors.DarkOrange] = nameof(Colors.DarkOrange),
            [Colors.DarkOrchid] = nameof(Colors.DarkOrchid),
            [Colors.DarkRed] = nameof(Colors.DarkRed),
            [Colors.DarkSalmon] = nameof(Colors.DarkSalmon),
            [Colors.DarkSeaGreen] = nameof(Colors.DarkSeaGreen),
            [Colors.DarkSlateBlue] = nameof(Colors.DarkSlateBlue),
            [Colors.DarkSlateGray] = nameof(Colors.DarkSlateGray),
            [Colors.DarkTurquoise] = nameof(Colors.DarkTurquoise),
            [Colors.DarkViolet] = nameof(Colors.DarkViolet),
            [Colors.DeepPink] = nameof(Colors.DeepPink),
            [Colors.DeepSkyBlue] = nameof(Colors.DeepSkyBlue),
            [Colors.DimGray] = nameof(Colors.DimGray),
            [Colors.DodgerBlue] = nameof(Colors.DodgerBlue),

            [Colors.Firebrick] = nameof(Colors.Firebrick),
            [Colors.FloralWhite] = nameof(Colors.FloralWhite),
            [Colors.ForestGreen] = nameof(Colors.ForestGreen),
            [Colors.Fuchsia] = nameof(Colors.Fuchsia),

            [Colors.Gainsboro] = nameof(Colors.Gainsboro),
            [Colors.GhostWhite] = nameof(Colors.GhostWhite),
            [Colors.Gold] = nameof(Colors.Gold),
            [Colors.Goldenrod] = nameof(Colors.Goldenrod),
            [Colors.Gray] = nameof(Colors.Gray),
            [Colors.Green] = nameof(Colors.Green),
            [Colors.GreenYellow] = nameof(Colors.GreenYellow),

            [Colors.Honeydew] = nameof(Colors.Honeydew),
            [Colors.HotPink] = nameof(Colors.HotPink),

            [Colors.IndianRed] = nameof(Colors.IndianRed),
            [Colors.Indigo] = nameof(Colors.Indigo),
            [Colors.Ivory] = nameof(Colors.Ivory),

            [Colors.Khaki] = nameof(Colors.Khaki),

            [Colors.Lavender] = nameof(Colors.Lavender),
            [Colors.LavenderBlush] = nameof(Colors.LavenderBlush),
            [Colors.LawnGreen] = nameof(Colors.LawnGreen),
            [Colors.LemonChiffon] = nameof(Colors.LemonChiffon),
            [Colors.LightBlue] = nameof(Colors.LightBlue),
            [Colors.LightCoral] = nameof(Colors.LightCoral),
            [Colors.LightCyan] = nameof(Colors.LightCyan),
            [Colors.LightGoldenrodYellow] = nameof(Colors.LightGoldenrodYellow),
            [Colors.LightGray] = nameof(Colors.LightGray),
            [Colors.LightGreen] = nameof(Colors.LightGreen),
            [Colors.LightPink] = nameof(Colors.LightPink),
            [Colors.LightSalmon] = nameof(Colors.LightSalmon),
            [Colors.LightSeaGreen] = nameof(Colors.LightSeaGreen),
            [Colors.LightSkyBlue] = nameof(Colors.LightSkyBlue),
            [Colors.LightSlateGray] = nameof(Colors.LightSlateGray),
            [Colors.LightSteelBlue] = nameof(Colors.LightSteelBlue),
            [Colors.LightYellow] = nameof(Colors.LightYellow),
            [Colors.Lime] = nameof(Colors.Lime),
            [Colors.LimeGreen] = nameof(Colors.LimeGreen),
            [Colors.Linen] = nameof(Colors.Linen),

            [Colors.Magenta] = nameof(Colors.Magenta),
            [Colors.Maroon] = nameof(Colors.Maroon),
            [Colors.MediumAquamarine] = nameof(Colors.MediumAquamarine),
            [Colors.MediumBlue] = nameof(Colors.MediumBlue),
            [Colors.MediumOrchid] = nameof(Colors.MediumOrchid),
            [Colors.MediumPurple] = nameof(Colors.MediumPurple),
            [Colors.MediumSeaGreen] = nameof(Colors.MediumSeaGreen),
            [Colors.MediumSlateBlue] = nameof(Colors.MediumSlateBlue),
            [Colors.MediumSpringGreen] = nameof(Colors.MediumSpringGreen),
            [Colors.MediumTurquoise] = nameof(Colors.MediumTurquoise),
            [Colors.MediumVioletRed] = nameof(Colors.MediumVioletRed),
            [Colors.MidnightBlue] = nameof(Colors.MidnightBlue),
            [Colors.MintCream] = nameof(Colors.MintCream),
            [Colors.MistyRose] = nameof(Colors.MistyRose),
            [Colors.Moccasin] = nameof(Colors.Moccasin),

            [Colors.NavajoWhite] = nameof(Colors.NavajoWhite),
            [Colors.Navy] = nameof(Colors.Navy),

            [Colors.OldLace] = nameof(Colors.OldLace),
            [Colors.Olive] = nameof(Colors.Olive),
            [Colors.OliveDrab] = nameof(Colors.OliveDrab),
            [Colors.Orange] = nameof(Colors.Orange),
            [Colors.OrangeRed] = nameof(Colors.OrangeRed),
            [Colors.Orchid] = nameof(Colors.Orchid),

            [Colors.PaleGoldenrod] = nameof(Colors.PaleGoldenrod),
            [Colors.PaleGreen] = nameof(Colors.PaleGreen),
            [Colors.PaleTurquoise] = nameof(Colors.PaleTurquoise),
            [Colors.PaleVioletRed] = nameof(Colors.PaleVioletRed),
            [Colors.PapayaWhip] = nameof(Colors.PapayaWhip),
            [Colors.PeachPuff] = nameof(Colors.PeachPuff),
            [Colors.Peru] = nameof(Colors.Peru),
            [Colors.Pink] = nameof(Colors.Pink),
            [Colors.Plum] = nameof(Colors.Plum),
            [Colors.PowderBlue] = nameof(Colors.PowderBlue),
            [Colors.Purple] = nameof(Colors.Purple),

            [Colors.Red] = nameof(Colors.Red),
            [Colors.RosyBrown] = nameof(Colors.RosyBrown),
            [Colors.RoyalBlue] = nameof(Colors.RoyalBlue),

            [Colors.SaddleBrown] = nameof(Colors.SaddleBrown),
            [Colors.Salmon] = nameof(Colors.Salmon),
            [Colors.SandyBrown] = nameof(Colors.SandyBrown),
            [Colors.SeaGreen] = nameof(Colors.SeaGreen),
            [Colors.SeaShell] = nameof(Colors.SeaShell),
            [Colors.Sienna] = nameof(Colors.Sienna),
            [Colors.Silver] = nameof(Colors.Silver),
            [Colors.SkyBlue] = nameof(Colors.SkyBlue),
            [Colors.SlateBlue] = nameof(Colors.SlateBlue),
            [Colors.SlateGray] = nameof(Colors.SlateGray),
            [Colors.Snow] = nameof(Colors.Snow),
            [Colors.SpringGreen] = nameof(Colors.SpringGreen),
            [Colors.SteelBlue] = nameof(Colors.SteelBlue),

            [Colors.Tan] = nameof(Colors.Tan),
            [Colors.Teal] = nameof(Colors.Teal),
            [Colors.Thistle] = nameof(Colors.Thistle),
            [Colors.Tomato] = nameof(Colors.Tomato),
            [Colors.Turquoise] = nameof(Colors.Turquoise),

            [Colors.Violet] = nameof(Colors.Violet),

            [Colors.Wheat] = nameof(Colors.Wheat),
            [Colors.White] = nameof(Colors.White),
            [Colors.WhiteSmoke] = nameof(Colors.WhiteSmoke),

            [Colors.Yellow] = nameof(Colors.Yellow),
            [Colors.YellowGreen] = nameof(Colors.YellowGreen)
        };


        public ColorBox()
        {
            this.InitializeComponent();
        }

        static ColorBox()
        {
            // Doesn't work in Release mode, so we have to just hardcode it
            //if (!_knownColors.Any())
            //    foreach (var property in typeof(Colors).GetProperties(BindingFlags.Public | BindingFlags.Static))
            //        if (property.GetValue(null) is Color c)
            //            _knownColors[c] = property.Name;
        }

        public void ClearUndoRedoHistory() => textBox.ClearUndoRedoHistory();
        public void CopySelectionToClipboard() => textBox.CopySelectionToClipboard();
        public void CutSelectionToClipboard() => textBox.CutSelectionToClipboard();
        public async Task<IReadOnlyList<string>> GetLinguisticAlternativesAsync() => await textBox.GetLinguisticAlternativesAsync();
        public Rect GetRectFromCharacterIndex(int charIndex, bool trailingEdge) => textBox.GetRectFromCharacterIndex(charIndex, trailingEdge);
        public void PasteFromClipboard() => textBox.PasteFromClipboard();
        public void Redo() => textBox.Redo();
        public void Select(int start, int len) => textBox.Select(start, len);
        public void SelectAll() => textBox.SelectAll();
        public void Undo() => textBox.Undo();

        private ColorPicker _picker;

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (ApiInformation.IsApiContractPresent("Windows.Foundation.UniversalApiContract", 5))
            {
                _picker = new ColorPicker
                {
                    Color = Color,
                    IsAlphaEnabled = true,
                    IsAlphaSliderVisible = true,
                    IsAlphaTextInputVisible = true,
                    IsMoreButtonVisible = true
                };

                _picker.PointerReleased += OnPointerSyncColor;
                _picker.PointerCaptureLost += OnPointerSyncColor;
                _picker.LostFocus += OnLostFocusSyncColor;

                var button = new Button
                {
                    Content = new SymbolIcon(Symbol.More),
                    Flyout = new Flyout
                    {
                        Content = _picker
                    },
                    VerticalAlignment = VerticalAlignment.Bottom,
                    Margin = new Thickness(8, 0, 0, 0)
                };

                ToolTipService.SetToolTip(button, "Pick color");
                Grid.SetColumn(button, 2);

                rootGrid.Children.Add(button);
            }

            textBox.LostFocus += TextBox_LostFocus;
        }

        private void OnPointerSyncColor(object sender, PointerRoutedEventArgs args)
        {
            _lockPickerUpdate = true;
            this.Color = _picker.Color;
        }

        private void OnLostFocusSyncColor(object sender, RoutedEventArgs args)
        {
            _lockPickerUpdate = true;
            this.Color = _picker.Color;
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            var c = NameToColor(textBox.Text);

            if (c != null)
                Color = (Color)c;
            else
                textBox.Text = ColorToName(Color);
        }

        private static Color? NameToColor(string s)
        {
            foreach (var kvp in _knownColors)
            {
                if (kvp.Value.Equals(s, StringComparison.OrdinalIgnoreCase))
                    return kvp.Key;
            }

            if (s.StartsWith("#", StringComparison.Ordinal))
            {
                try
                {
                    if (s.Length == 9)
                    {
                        string sa = s.Substring(1, 2);
                        string sr = s.Substring(3, 2);
                        string sg = s.Substring(5, 2);
                        string sb = s.Substring(7, 2);

                        byte a = byte.Parse(sa, NumberStyles.HexNumber);
                        byte r = byte.Parse(sr, NumberStyles.HexNumber);
                        byte g = byte.Parse(sg, NumberStyles.HexNumber);
                        byte b = byte.Parse(sb, NumberStyles.HexNumber);

                        return Color.FromArgb(a, r, g, b);
                    }
                    else if (s.Length == 7)
                    {
                        string sr = s.Substring(1, 2);
                        string sg = s.Substring(3, 2);
                        string sb = s.Substring(5, 2);

                        byte r = byte.Parse(sr, NumberStyles.HexNumber);
                        byte g = byte.Parse(sg, NumberStyles.HexNumber);
                        byte b = byte.Parse(sb, NumberStyles.HexNumber);

                        return Color.FromArgb(255, r, g, b);
                    }
                }
                catch
                {
                    // fall through and return null
                }
            }

            return null;
        }

        private static string ColorToName(Color c)
        {
            if (_knownColors.TryGetValue(c, out string name))
                return name;
            else
                return $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";
        }

        private bool _lockPickerUpdate = false;

        private static void OnColorChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (obj is ColorBox box)
            {
                box.textBox.Text = ColorToName(box.Color);
                box.ColorChanged?.Invoke(box, EventArgs.Empty);

                if (box._lockPickerUpdate)
                {
                    box._lockPickerUpdate = false;
                    return;
                }

                if (box._picker != null)
                    box._picker.Color = box.Color;
            }
        }
    }
}
