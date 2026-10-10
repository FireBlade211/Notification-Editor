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

        private static Dictionary<Color, string> _knownColors = new Dictionary<Color, string>();

        public ColorBox()
        {
            this.InitializeComponent();
        }

        static ColorBox()
        {
            if (!_knownColors.Any())
                foreach (var property in typeof(Colors).GetProperties(BindingFlags.Public | BindingFlags.Static))
                    if (property.GetValue(null) is Color c)
                        _knownColors[c] = property.Name;
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
