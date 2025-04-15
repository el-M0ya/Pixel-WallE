using System;
using System.Drawing;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace PixelWall_E;

public partial class MainWindow : Window
{
    
    public MainWindow()
    {
        
        InitializeComponent();
        
    }
    private void Button_Click(object sender , RoutedEventArgs e)
    {
        Console.WriteLine("Boton clickeado");
    }
    private void btnSaludo_Click(object sender , RoutedEventArgs e)
    {
        var button = (Button)sender;
        button.Content = "Hola";
    }
}