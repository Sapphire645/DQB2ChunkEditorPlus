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
        private Brush ?Current;
        //private TextBlock Name;
        public Preview()
        {
            InitializeComponent();
        }
        public void LoadPreview(string input)
        {
            SetColoredText(input);
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
                string append = "";
                string text = part;
                //Check for Op Code
                if (part.Contains(">")) 
                {
                    //extract future text
                    int endIndex = part.IndexOf('>');
                    if (endIndex != part.Length) text = part.Substring(endIndex + 1);
                    else text = "";
                    //Has arguments. Process arguments.
                    if (part[endIndex - 1] == ')' && part.Contains('('))
                    {
                        int BracketBeggining = part.IndexOf('(')+1;
                        int BracketEnd = part.IndexOf(')');
                        append = part.Substring(BracketBeggining, BracketEnd - BracketBeggining);
                        //Argument is INT
                        if (part[0] == '$')
                        {
                            if (part.Contains("$iname(") || part.Contains("_iname("))
                            {
                                append = ExtractData(Convert.ToInt16(append), 0);
                            }else
                            if (part.Contains("$cdef("))
                            {
                                Current = GetBrushFromColorCode(ExtractData(Convert.ToInt16(append),1));
                                append = "";
                            }else
                            if (part.Contains("$cname("))
                            {
                                append = ExtractData(Convert.ToInt16(append),2);
                            }
                            else append = "[NotFound]";

                        }
                        else//Argument is STRING
                        {
                            if (part.Contains("show("))
                            {
                                BoxName.Visibility = Visibility.Visible;
                                BoxBGName.Visibility = Visibility.Visible;
                                Name.Inlines.Add(append);
                                NameBlack.Inlines.Add(append);
                                append = "";
                                BoxName.Width = MeasureTextBoxText(Name) + 43;
                                BoxBGName.Width = MeasureTextBoxText(Name) + 45;
                            }else
                            if (part.Contains("morph("))
                            {
                                BracketEnd = part.IndexOf(',');
                                append = append.Substring(0, BracketEnd);
                                Name.Inlines.Add(append);
                                NameBlack.Inlines.Add(append);
                            }
                        }
                    }
                    else
                    if (part[0] == '/')
                    {
                        Current = Brushes.White;
                    }
                    else
                    {
                        switch (endIndex)
                        {
                            case 2:
                                if (part.Contains("br>"))
                                {
                                    ColoredTextBlock.Inlines.Add(new LineBreak());
                                    ColoredTextBlockBlack.Text += "\n";
                                    break;
                                }
                                if (part.Contains("-->"))
                                {
                                    append = "-";
                                    break;
                                }
                                break;
                            case 3:
                                if (part.Contains("key>"))
                                {
                                    ColoredTextBlock.Inlines.Add(new LineBreak());
                                    ColoredTextBlockBlack.Text += "\n";
                                    break;
                                }
                                if (part.Contains("off>"))
                                {
                                    BoxR.Fill = null;
                                    BoxName.Fill = null;
                                    break;
                                }
                                if (part.Contains("...>"))
                                {
                                    append = "...";
                                    break;
                                }
                                if (part.Contains("cap>"))
                                {
                                    if (text != null)
                                    {
                                        text = string.Concat(text[0].ToString().ToUpper(), text.AsSpan(1));
                                    }
                                    else
                                    {
                                        Cap = true;
                                    }
                                    break;
                                }
                                break;
                            case 4:
                                if (part.Contains("note>"))
                                    append = "♪";
                                break;
                            case 5:
                                if (part.Contains("pname>"))
                                    append = ChunkEditor.Pname;
                                break;
                        }

                    }
                }

                Run run = new Run(append + text);
                if (Cap == true && run.Text.Length > 0)
                {
                    run.Text = string.Concat(run.Text[0].ToString().ToUpper(), run.Text.AsSpan(1));
                    Cap = false;
                }
                run.Foreground = Current;
                ColoredTextBlock.Inlines.Add(run);
                ColoredTextBlockBlack.Text += run.Text;

                // Add Run to the TextBlock

            }
            ColoredTextBlockBlack.Text += "'";
            Run run2 = new Run("'");
            run2.Foreground = Current;
            ColoredTextBlock.Inlines.Add(run2);
        }

        private Brush GetBrushFromColorCode(string brush)
        {
            BrushConverter brushConverter = new BrushConverter();
            return (Brush)brushConverter.ConvertFromString(brush);
        }

        private string ExtractData(int Code,int Type)
        {
            String line = System.IO.File.ReadLines("Data/Preview.txt").Skip(Code + 1).FirstOrDefault();
            String[] values = line.Split('\t');
            if (values.Length > Type) { 
                return values[Type];
            }
            return "";
        }
        public double MeasureTextBoxText(TextBlock textBox) //not me
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
