
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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
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

        private void RedTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(RedTextBox.Text, out int value))
            {
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
                RedTextBox.Text = "";
                R = 0;
                Rectangle.Fill = new SolidColorBrush(Color.FromRgb(R, G, B));
                Hex.Text = $"#{R:X2}{G:X2}{B:X2}";
            }
               
        }
        private void GreenTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(GreenTextBox.Text, out int value))
            {
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
                Rectangle.Fill = new SolidColorBrush(Color.FromRgb(R, G, B));
                Hex.Text = $"#{R:X2}{G:X2}{B:X2}";
            }
        }
        private void BlueTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(BlueTextBox.Text, out int value))
            {
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
                Rectangle.Fill = new SolidColorBrush(Color.FromRgb(R, G, B));
                Hex.Text = $"#{R:X2}{G:X2}{B:X2}";
            }
        }
        private void RedSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            byte value = (byte)RedSlider.Value;
            RedTextBox.Text = value.ToString();
            R = value;
            Hex.Text = $"#{R:X2}{G:X2}{B:X2}";
        }
        private void GreenSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            byte value = (byte)GreenSlider.Value;
            GreenTextBox.Text = value.ToString();
            G = value;
            Hex.Text = $"#{R:X2}{G:X2}{B:X2}";
        }
        private void BlueSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            byte value = (byte)BlueSlider.Value;
            BlueTextBox.Text = value.ToString();
            B = value;
            Hex.Text = $"#{R:X2}{G:X2}{B:X2}";
        }
    }
}
