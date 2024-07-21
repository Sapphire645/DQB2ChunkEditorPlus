using System;
using System.Windows;
using System.Collections.ObjectModel;
using System.Windows.Controls;
using DQB2ChunkEditor.Controls;
using System.Text.Json;
using System.Text.Json.Serialization;
using DQB2ChunkEditor.Models;
using DQB2ChunkEditor.Windows;
using System.Collections.Generic;
using System.IO;

namespace DQB2ChunkEditor.Windows;

public partial class ChangeWeather : Window
{
    public ObservableCollection<ComboBoxWeather> WeatherComboBox { get; set; } = new();
    public List<Weather> WeatherList { get; set; } = new();
    public ObservableProperty<Weather> SelectedWeather { get; set; } = new();
    public ushort Id { get; set; } = 0;

    public ChangeWeather()
    {
        InitializeComponent();
        CreateComboBoxTiles();
        DataContext = this;
    }
    private void CreateComboBoxTiles()
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };

            var json = File.ReadAllText(@"Data\Weather.json");

            var weathers = JsonSerializer.Deserialize<WeatherList>(json, options);

            for (var i = 0; i < weathers!.Weathers.Count; i++)
            {
                WeatherComboBox.Add(new ComboBoxWeather
                {
                    Id = i,
                    Weather = weathers.Weathers[i]
                });

                WeatherList.Add(weathers.Weathers[i]);
            }
           WeatherComboBoxX.SelectedIndex = ChunkEditor.Weather;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
    }

    private void SetWeather_OnClick(Object sender, SelectionChangedEventArgs e)
    {
        Id = ((ComboBoxWeather)e.AddedItems[0]!).Weather.Id;
    }

    private void OkButtonW_OnClick(Object sender,  RoutedEventArgs e)
    {
        DialogResult = true;
    }
}
