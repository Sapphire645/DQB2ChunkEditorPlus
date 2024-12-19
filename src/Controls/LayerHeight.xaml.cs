using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.Generic;
using System.Windows.Shapes;
using System.Drawing;
using DQB2ChunkEditor.Models;
using System;
using System.Security.Cryptography;
using System.Collections;
using System.Reflection;
using System.Linq;

namespace DQB2ChunkEditor.Controls
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class LayerHeight : UserControl
    {
        public event Action<byte> Layer;
        private List<RadioButton> RectLines = new();
        private RadioButton SelectedRadioButton;
        public LayerHeight()
        {
            InitializeComponent();
            CreateRadioButtons();
            var RadioButton = RectLines.ElementAt(0);
            RadioButton.IsChecked = true;
            SelectedRadioButton = RadioButton;
        }
        

        private void CreateRadioButtons()
        {
            short lineW = 5;

            //I just wanted orange

            Style RadioButtonStyle = new Style(typeof(RadioButton));
            ControlTemplate template = new ControlTemplate(typeof(RadioButton));
            FrameworkElementFactory borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.Name = "border";
            borderFactory.SetValue(Border.BackgroundProperty, Brushes.Transparent);
            borderFactory.SetValue(Border.BorderBrushProperty, Brushes.Transparent);
            borderFactory.SetValue(Border.BorderThicknessProperty, new Thickness(0));
            FrameworkElementFactory contentPresenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            contentPresenterFactory.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            contentPresenterFactory.SetValue(ContentPresenter.VerticalAlignmentProperty, VerticalAlignment.Center);
            borderFactory.AppendChild(contentPresenterFactory);
            template.VisualTree = borderFactory;
            Trigger hoverTrigger = new Trigger
            {
                Property = RadioButton.IsMouseOverProperty,
                Value = true
            };
            hoverTrigger.Setters.Add(new Setter(Border.BackgroundProperty, Brushes.Orange, "border"));

            // Add Triggers
            Trigger checkedTrigger = new Trigger
            {
                Property = RadioButton.IsCheckedProperty,
                Value = true
            };
            checkedTrigger.Setters.Add(new Setter(Border.BackgroundProperty, Brushes.Orange, "border"));

            Trigger unCheckedTrigger = new Trigger
            {
                Property = RadioButton.IsCheckedProperty,
                Value = false
            };
            unCheckedTrigger.Setters.Add(new Setter(Border.BackgroundProperty, Brushes.Transparent, "border"));

            template.Triggers.Add(checkedTrigger);
            template.Triggers.Add(unCheckedTrigger);

            template.Triggers.Add(hoverTrigger);
            RadioButtonStyle.Setters.Add(new Setter(RadioButton.TemplateProperty, template));

            for (uint i = ChunkEditor.LayerHeight; i > 0; i--)
            {
                var RadioButton = new RadioButton
                {
                    Style = RadioButtonStyle,
                    Tag = i - 1,
                    Width = 15
                };
                if (i % 10 == 1)
                {
                    lineW = 15;
                    Numbers.Children.Add(new TextBlock()
                    {
                        Text = (i - 1).ToString(),
                        FontSize = 15,
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Margin = new Thickness(0, 20 - 20 * ((double)i/ (double)ChunkEditor.LayerHeight), 0, 0 + 20 * ((double)i / (double)ChunkEditor.LayerHeight))
                    });
                }
                else if (i % 5 == 1)
                {
                    lineW = 10;
                    Numbers.Children.Add(new TextBlock()
                    {
                        Text = (i - 1).ToString(),
                        HorizontalAlignment = HorizontalAlignment.Right,
                        Margin = new Thickness(0, 20 - 20 * ((double)i / (double)ChunkEditor.LayerHeight), 0, 0 + 20 * ((double)i / (double)ChunkEditor.LayerHeight))
                    });
                }
                else lineW = 5;
                System.Windows.Shapes.Rectangle line = new System.Windows.Shapes.Rectangle
                {
                    Height = 2,
                    Width = lineW,
                    Fill = Brushes.Black,
                    Margin = new Thickness(0, 0, 0, 0)
                };
                RectLines.Insert(0, RadioButton);
                RadioButton.Click += (_, _) => { RadioButton_Click(RadioButton); };
                RadioButton.Content = line;
                GridLine.Children.Add(RadioButton);
            }
        }
        public void UpdateSeaLevel()
        {
            ushort i = 0;
            foreach(var Button in RectLines)
            {
                if (i <= ChunkEditor.TemporalSeaLevel)
                {
                    ((System.Windows.Shapes.Rectangle)(Button.Content)).Fill = Brushes.Blue;
                }
                else
                {
                    ((System.Windows.Shapes.Rectangle)(Button.Content)).Fill = Brushes.Black;
                }
                i++;
            }
        }
        private void RadioButton_Click(RadioButton sender)
        {
            Layer?.Invoke(byte.Parse(sender.Tag.ToString()));
        }

        public void LayerChange(byte Layer)
        {
            var RadioButton = RectLines.ElementAt(Layer);
            System.Windows.Point RadioButtonPosition = RadioButton.TransformToAncestor(this).Transform(new System.Windows.Point(0, 0));
            ArrowLayer.Margin = new Thickness(YLayerLine.ActualWidth / 2, RadioButtonPosition.Y - (7 * YLayerLine.ActualHeight / 960), 0, 0);
            UpButton.Margin = new Thickness(YLayerLine.ActualWidth / 2, RadioButtonPosition.Y - (7 * YLayerLine.ActualHeight / 960) - 20, 0, 0);
            DownButton.Margin = new Thickness(YLayerLine.ActualWidth / 2, RadioButtonPosition.Y - (7 * YLayerLine.ActualHeight / 960) + 20, 0, 0);
            DownButton.Visibility = Visibility.Visible;
            UpButton.Visibility = Visibility.Visible;
            if (Layer == 0) DownButton.Visibility = Visibility.Collapsed;
            else if (Layer == ChunkEditor.LayerHeight-1) UpButton.Visibility = Visibility.Collapsed;
            if (SelectedRadioButton != null)
            SelectedRadioButton.IsChecked = false;
            RadioButton.IsChecked = true;
            SelectedRadioButton = RadioButton;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Layer?.Invoke((byte)(byte.Parse(SelectedRadioButton.Tag.ToString())+1));
        }

        private void ButtonDown_Click(object sender, RoutedEventArgs e)
        {
            Layer?.Invoke((byte)(byte.Parse(SelectedRadioButton.Tag.ToString()) - 1));
        }
    }
}
