using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input; // Para RelayCommand
using System.Collections.ObjectModel;
using System.Linq; // Para Select
using System; // Para Random

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private int _gridWidth = 16; // Ancho del grid en píxeles

    [ObservableProperty]
    private int _gridHeight = 16; // Alto del grid en píxeles

    [ObservableProperty]
    private int _pixelSize = 20; // Tamaño de cada píxel en la UI

    public ObservableCollection<PixelViewModel> Pixels { get; } = new();

    // Un color seleccionado para pintar (puedes expandir esto con un selector de color)
    [ObservableProperty]
    private Color _selectedColor = Colors.Red;

    public MainWindowViewModel()
    {
        InitializePixelGrid();
    }

    private void InitializePixelGrid()
    {
        Pixels.Clear();
        for (int y = 0; y < GridHeight; y++)
        {
            for (int x = 0; x < GridWidth; x++)
            {
                // Color inicial aleatorio o por defecto
                var initialColor = Colors.LightGray; // O Colors.White, etc.
                Pixels.Add(new PixelViewModel(x, y, initialColor));
            }
        }
    }

    [RelayCommand] // Necesitas CommunityToolkit.Mvvm para esto
    private void PixelClicked(PixelViewModel pixel)
    {
        if (pixel != null)
        {
            // Aquí cambias el color del píxel al color seleccionado
            pixel.SetColor(SelectedColor);
            Console.WriteLine($"Pixel at ({pixel.X}, {pixel.Y}) changed to {SelectedColor}");
        }
    }

    // Si quieres poder cambiar el tamaño del grid dinámicamente
    partial void OnGridWidthChanged(int value) => InitializePixelGrid();
    partial void OnGridHeightChanged(int value) => InitializePixelGrid();

    // Método para acceder y cambiar un píxel por coordenadas (si lo necesitas desde código)
    public void SetPixelColor(int x, int y, Color color)
    {
        if (x >= 0 && x < GridWidth && y >= 0 && y < GridHeight)
        {
            // La colección está almacenada linealmente, así que calculamos el índice
            int index = y * GridWidth + x;
            if (index < Pixels.Count)
            {
                Pixels[index].SetColor(color);
            }
        }
    }

    public Color GetPixelColor(int x, int y)
    {
        if (x >= 0 && x < GridWidth && y >= 0 && y < GridHeight)
        {
            int index = y * GridWidth + x;
            if (index < Pixels.Count && Pixels[index].PixelBrush is SolidColorBrush scb)
            {
                return scb.Color;
            }
        }
        return Colors.Transparent; // O un color por defecto si no se encuentra
    }
}
