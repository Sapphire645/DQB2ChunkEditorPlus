using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

public static class ExtensionMethods
{
    private static readonly Action EmptyDelegate = delegate { };
    public static void Refresh(this UIElement uiElement)
    {
        uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
    }
}

namespace DQB2ChunkEditor.Controls
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class Preview : UserControl
    {
        private Brush Current;
        //private TextBlock Name;
        public Preview(string input)
        {
            InitializeComponent();
            SetColoredText(input);
            DataContext = this;
            this.Refresh();
        }
        public void SetColoredText(string input)
        {
            bool Cap = false;
            // Clear existing content
            BoxName.Visibility = Visibility.Hidden;
            BoxBGName.Visibility = Visibility.Hidden;

            BoxR.Fill = Brushes.Black;
            BoxName.Fill = Brushes.Black;

            Current = Brushes.White;
            // Split input by space or other delimiters, and process color codes
            input = input.Replace(" ", "   ");
            input = input.Replace(".", ". ");
            string[] parts = input.Split(new[] { '<' }, System.StringSplitOptions.RemoveEmptyEntries);
            ColoredTextBlock.Inlines.Add("'");
            ColoredTextBlockBlack.Text = "'";
            foreach (string part in parts)
            {
                string colorCode = GetColorCode(part, out string text);

                if (colorCode != null && colorCode.Length > 5 && colorCode.Substring(0, 5) == "iname")
                {
                    colorCode = colorCode.Replace(")", "");
                    text = GetName(Convert.ToInt16(colorCode.Substring(6))) + text;
                }
                int ind;
                if (Cap == true && part.Contains(">") && (ind = part.IndexOf('>')) +1 != part.Length)
                {
                    text = string.Concat(text[0].ToString().ToUpper(), text.AsSpan(1));
                    Cap = false;
                }
                // Create a Run element with the extracted text and color
                Run run = new Run(text);
                if (colorCode != null)
                {
                    if (colorCode == "c")
                    {
                        if(text.Length > 4 && text[0] != ' ')
                        {
                            text = string.Concat(text[0].ToString().ToUpper(), text.AsSpan(1));
                            run = new Run(text);
                        }
                        else
                        {
                            Cap = true;
                        }
                    }
                    else if (colorCode == "/n")
                    {
                        ColoredTextBlock.Inlines.Add(new LineBreak());
                        ColoredTextBlockBlack.Text += "\n";
                    }
                    else
                    {
                        if (colorCode.Substring(0, 4) == "cdef")
                        {
                            colorCode = colorCode.Replace(")","");
                            Current = GetBrushFromColorCode(Convert.ToInt16(colorCode.Substring(5)));
                        }
                        if (colorCode == "color")
                        {
                            Current = GetBrushFromColorCode(7);
                        }

                    }
                }
                run.Foreground = Current;

                // Add Run to the TextBlock
                ColoredTextBlock.Inlines.Add(run);
                ColoredTextBlockBlack.Text += run.Text;
                
            }
            ColoredTextBlockBlack.Text += "'";
            Run run2 = new Run("'");
            run2.Foreground = Current;
            ColoredTextBlock.Inlines.Add(run2);
        }

        private string GetColorCode(string input, out string text)
        {
            // Check for color codes in the format [color]text
            if (input.Contains("icon") && input.Contains(">"))
            {
                int endIndex = input.IndexOf('>');
                text = "⛝" + input.Substring(endIndex + 1);
                return "color";
            }
            else if (input.StartsWith("$") && input.Contains(">"))
            {
                int endIndex = input.IndexOf('>');
                string colorCode = input.Substring(1, endIndex - 1); // Extract color code
                text = input.Substring(endIndex + 1); // Extract text after the color code
                return colorCode.ToLower(); // Return lowercase for uniformity
            }
            else if (input.StartsWith("/") && input.Contains(">"))
            {
                int endIndex = input.IndexOf('>');
                string colorCode = input.Substring(1, endIndex - 1); // Extract color code
                text = input.Substring(endIndex + 1); // Extract text after the color code
                return colorCode.ToLower(); ; // Return lowercase for uniformity
            }
            else if ((input.Length > 3 && input.Substring(0, 4) == "key>") || (input.Length > 2 && input.Substring(0, 3) == "br>"))
            {
                BoxR.Fill = null;
                BoxName.Fill = null;
                text = input.Substring(4);
                return "/n"; ;// Return lowercase for uniformity
            }
            else if (input.Length > 3 && input.Substring(0,4) == "off>")
            {
                BoxR.Fill = null;
                BoxName.Fill = null;
                text = input.Substring(4);
                return null; ;// Return lowercase for uniformity
            }
            else if (input.Length > 3 && input.Substring(0, 4) == "cap>")
            {
                BoxR.Fill = null;
                BoxName.Fill = null;
                text = input.Substring(4);
                return "c"; ;// Return lowercase for uniformity
            }
            else if (input.Length > 4 &&  input.Substring(0, 5) == "show(" && input.Contains(")>"))
            {
                int endIndex = input.IndexOf(')');
                BoxName.Visibility = Visibility.Visible;
                BoxBGName.Visibility = Visibility.Visible;
                Name.Inlines.Add(input.Substring(5, endIndex - 5));
                NameBlack.Inlines.Add(input.Substring(5, endIndex - 5));
                BoxName.Width = MeasureTextBoxText(Name) + 43;
                BoxBGName.Width = MeasureTextBoxText(Name) + 45;
                text = input.Substring(endIndex+2);
                return null;// Return lowercase for uniformity
            }
            else if (input.Length > 4 && input.Substring(0, 5) == "note>")
            {
                BoxR.Fill = null;
                BoxName.Fill = null;
                text = "♩"+input.Substring(5);
                return null; ;// Return lowercase for uniformity
            }
            else if (input.Length > 4 && input.Substring(0, 5) == "morf(")
            {
                int startIndex = input.IndexOf('(');
                int endIndex = input.IndexOf(',');
                text = input.Substring(startIndex+1, endIndex - startIndex-1);
                return null; ;// Return lowercase for uniformity
            }
            else if (input.Length > 5 && input.Substring(0, 6) == "pname>")
            {
                text = ChunkEditor.Pname + input.Substring(6);
                return null; ;// Return lowercase for uniformity
            }
            else if (!input.Contains(">"))
            {
                text = input; // No color code, return the whole input as text
                return "null";
            }
            else
            {
                text = ""; // No color code, return the whole input as text
                return null;
            }
        }

        private Brush GetBrushFromColorCode(int colorCode)
        {
            String line = System.IO.File.ReadLines("Data/color.txt").Skip(colorCode+1).FirstOrDefault();
            // Map color codes to actual colors
            String[] values = line.Split('\t');
            BrushConverter brushConverter = new BrushConverter();
            return (Brush)brushConverter.ConvertFromString(values[1]);
        }

        private string GetName(int colorCode)
        {
            String line = System.IO.File.ReadLines("Data/item.txt").Skip(colorCode + 1).FirstOrDefault();
            // Map color codes to actual colors
            String[] values = line.Split('\t');
            if (values[1].Contains("[")){
                return values[1].Substring(0, values[1].IndexOf('[')).Trim();
            }
            return values[1].Substring(0, values[1].IndexOf('{')).Trim();
        }
        public double MeasureTextBoxText(TextBlock textBox)
        {
            // Get the Typeface from the TextBox's font properties
            Typeface typeface = new Typeface(
                textBox.FontFamily,       // The FontFamily of the TextBox
                textBox.FontStyle,        // The FontStyle of the TextBox
                textBox.FontWeight,       // The FontWeight of the TextBox
                FontStretches.Normal      // FontStretch (normal or other values)
            );

            // Create a FormattedText object to measure the text in the TextBox
            FormattedText formattedText = new FormattedText(
                textBox.Text,                              // The text inside the TextBox
                System.Globalization.CultureInfo.CurrentCulture,  // Culture info
                FlowDirection.LeftToRight,                 // Text flow direction
                typeface,                                  // The Typeface created above
                textBox.FontSize,                          // Font size (in the TextBox)
                Brushes.Black,                             // Not used for size calculation
                VisualTreeHelper.GetDpi(System.Windows.Application.Current.MainWindow).PixelsPerDip // Handle DPI scaling
            );

            // Return the width and height as a Size object
            return formattedText.Width;
        }
    }
}
