
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private byte R;
        private byte G;
        private byte B;

        public MainWindow()
        {
            InitializeComponent();
            R = 0;
            G = 0;
            B = 0;
            Rectangle.Fill = new SolidColorBrush(Color.FromRgb(R, G, B));
            RedTextBox.Text = "0";
            GreenTextBox.Text = "0";
            BlueTextBox.Text = "0";
            Hex.Text = "#000000";
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !char.IsDigit(e.Text, 0);
        }

        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
        private void RedTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            RedSlider.ValueChanged -= RedSlider_ValueChanged;
            RedTextBox.TextChanged -= RedTextBox_TextChanged;
            if (int.TryParse(RedTextBox.Text, out int value))
            {
                RedSlider.Value = value;
                if (value <= 255)
                {
                    R = (byte)value;
                    Rectangle.Fill = new SolidColorBrush(Color.FromRgb(R,G,B));
                    
                    Hex.Text = $"#{R:X2}{G:X2}{B:X2}";
                } else
                {
                    MessageBox.Show("ZADEJ HODNOTU V ROZMEZÍ 0-255");
                    RedTextBox.Text = "255";
                    R = 255;
                    Rectangle.Fill = new SolidColorBrush(Color.FromRgb(R, G, B));
                    Hex.Text = $"#{R:X2}{G:X2}{B:X2}";
                }
            } else
            {
                RedTextBox.Clear();
                R = 0;
                RedSlider.Value = 0;
                Rectangle.Fill = new SolidColorBrush(Color.FromRgb(R, G, B));
                Hex.Text = $"#{R:X2}{G:X2}{B:X2}";
            }
            RedSlider.ValueChanged += RedSlider_ValueChanged;
            RedTextBox.TextChanged += RedTextBox_TextChanged;
            RedTextBox.CaretIndex = RedTextBox.Text.Length;
        }
        private void GreenTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            GreenSlider.ValueChanged -= GreenSlider_ValueChanged;
            GreenTextBox.TextChanged -= GreenTextBox_TextChanged;
            if (int.TryParse(GreenTextBox.Text, out int value))
            {
                GreenSlider.Value = value;
                if (value <= 255)
                {
                    G = (byte)value;
                    Rectangle.Fill = new SolidColorBrush(Color.FromRgb(R, G, B));
                    Hex.Text = $"#{R:X2}{G:X2}{B:X2}";
                }
                else
                {
                    MessageBox.Show("ZADEJ HODNOTU V ROZMEZÍ 0-255");
                    GreenTextBox.Text = "255";
                    G = 255;
                    Rectangle.Fill = new SolidColorBrush(Color.FromRgb(R, G, B));
                    Hex.Text = $"#{R:X2}{G:X2}{B:X2}";
                }
            }
            else
            {
                GreenTextBox.Text = "";
                G = 0;
                GreenSlider.Value = 0;
                Rectangle.Fill = new SolidColorBrush(Color.FromRgb(R, G, B));
                Hex.Text = $"#{R:X2}{G:X2}{B:X2}";
            }
            GreenSlider.ValueChanged += GreenSlider_ValueChanged;
            GreenTextBox.TextChanged += GreenTextBox_TextChanged;
            GreenTextBox.CaretIndex = GreenTextBox.Text.Length;
        }
        private void BlueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            BlueSlider.ValueChanged -= BlueSlider_ValueChanged;
            BlueTextBox.TextChanged -= BlueTextBox_TextChanged;
            if (int.TryParse(BlueTextBox.Text, out int value))
            {
                BlueSlider.Value = value;
                if (value <= 255)
                {
                    B = (byte)value;
                    Rectangle.Fill = new SolidColorBrush(Color.FromRgb(R, G, B));
                    Hex.Text = $"#{R:X2}{G:X2}{B:X2}";
                }
                else
                {
                    MessageBox.Show("ZADEJ HODNOTU V ROZMEZÍ 0-255");
                    BlueTextBox.Text = "255";
                    B = 255;
                    Rectangle.Fill = new SolidColorBrush(Color.FromRgb(R, G, B));
                    Hex.Text = $"#{R:X2}{G:X2}{B:X2}";
                }
            }
            else
            {
                BlueTextBox.Text = "";
                B = 0;
                BlueSlider.Value = 0;
                Rectangle.Fill = new SolidColorBrush(Color.FromRgb(R, G, B));
                Hex.Text = $"#{R:X2}{G:X2}{B:X2}";
            }
            BlueSlider.ValueChanged += BlueSlider_ValueChanged;
            BlueTextBox.TextChanged += BlueTextBox_TextChanged;
            BlueTextBox.CaretIndex = BlueTextBox.Text.Length;
        }

        private void RedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            RedTextBox.TextChanged -= RedTextBox_TextChanged;
            byte value = (byte)RedSlider.Value;
            RedTextBox.Text = value.ToString();
            R = value;
            Hex.Text = $"#{R:X2}{G:X2}{B:X2}";
            RedTextBox.TextChanged += RedTextBox_TextChanged;
        }
        private void GreenSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            GreenTextBox.TextChanged -= GreenTextBox_TextChanged;
            byte value = (byte)GreenSlider.Value;
            GreenTextBox.Text = value.ToString();
            G = value;
            Hex.Text = $"#{R:X2}{G:X2}{B:X2}";
            GreenTextBox.TextChanged += GreenTextBox_TextChanged;
        }
        private void BlueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            BlueTextBox.TextChanged -= BlueTextBox_TextChanged;
            byte value = (byte)BlueSlider.Value;
            BlueTextBox.Text = value.ToString();
            B = value;
            Hex.Text = $"#{R:X2}{G:X2}{B:X2}";
            GreenTextBox.TextChanged += GreenTextBox_TextChanged;
        }
    }
}
