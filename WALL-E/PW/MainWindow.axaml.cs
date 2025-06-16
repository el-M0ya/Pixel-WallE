using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform; // Para PixelFormat y AlphaFormat
using System;
using System.Collections.Generic;
using System.IO; // Para carga/guardado de archivos
using System.Linq; // Para el diccionario de colores
using System.Threading.Tasks;
using SkiaSharp; // Para diálogos asíncronos
namespace PW;

using TextMateSharp;
using AvaloniaEdit.TextMate;
using AvaloniaEdit.Highlighting.Xshd;

public class Wall_E
{
    public bool isSpawn = false;
    public int x;
    public int y;
    public string currentColor;
    public int brushSize;

    public static Wall_E Instance { get; private set; }
    public Wall_E(int x, int y, string currentColor, int brushSize)
    {
        if (Instance == null) Instance = this;
        else throw new Exception("Many Wall_E's in scene");
        this.x = x;
        this.y = y;
        this.currentColor = currentColor;
        this.brushSize = brushSize;
    }
    public static void Set(int x, int y, string color, int brush)
    {
        Instance.x = x;
        Instance.y = y;
        Instance.currentColor = color;
        Instance.brushSize = brush;
    }
}
public partial class MainWindow : Window
{
    public static bool isWallEImage = true;
    // Diccionario de colores, ahora accesible para todos
    public static readonly Dictionary<string, Color> _colorNameMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Black", Colors.Black }, { "White", Colors.White },
            { "Red", Colors.Red }, { "Green", Colors.Green },
            { "Blue", Colors.Blue }, { "Yellow", Colors.Yellow },
            { "Orange", Colors.Orange }, { "Purple", Colors.Purple },
            { "Transparent", Colors.Transparent } // Transparente es especial, usualmente significa "no pintar"
        };

    // Referencias a los controles de la UI
    private NumericUpDown _canvasSizeTextBox;
    private TextEditor _codeEditorTextBox;
    private static TextBlock _statusOutputTextBlock;
    private static PixelCanvasControl _pixelCanvas;

    private PixelWallE interpreter;
    public Wall_E walle;

    public MainWindow()
    {
        InitializeComponent();

        // Enlazar controles
        _canvasSizeTextBox = this.FindControl<NumericUpDown>("_CanvasSizeTextBox");
        _codeEditorTextBox = this.FindControl<TextEditor>("_CodeEditorTextBox");
        _statusOutputTextBlock = this.FindControl<TextBlock>("_StatusOutputTextBlock");
        _pixelCanvas = this.FindControl<PixelCanvasControl>("_PixelCanvas"); // ¡Obtenemos el control del canvas!

        // Conectar eventos
        this.FindControl<Button>("_ResizeCanvasButton").Click += ResizeCanvasButton_Click;
        this.FindControl<Button>("_ExecuteButton").Click += ExecuteButton_Click;
        this.FindControl<Button>("_LoadScriptButton").Click += LoadScriptButton_Click;
        this.FindControl<Button>("_SaveScriptButton").Click += SaveScriptButton_Click;

        // Configuración inicial
        _canvasSizeTextBox.Value = _pixelCanvas.CanvasDimension;
        interpreter = new PixelWallE(); // Pasamos la MainWindow al intérprete para que pueda interactuar
        walle = new Wall_E(-1, -1, "Transparent", 1);


        var definition = new HighlightingRuleSet();

        var highlightingdefinition = new CustomHighlighting();

        _codeEditorTextBox.SyntaxHighlighting = highlightingdefinition;
    }

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
    private void _ChangeImg(object sender, RoutedEventArgs e)
    {
        var image = (Image)((Button)sender).Content;
        image.Source = new Bitmap(isWallEImage
        ? "Assets/EVA.png"
        : "Assets/WALL-E.png");
        isWallEImage = !isWallEImage;
    }
    public static void Paint(int x, int y, string color, int brushSize)
    {
        // Asegurar que el color exista
        if (_colorNameMap.TryGetValue(color, out Color colorname))
        {
            if (colorname == Colors.Transparent) // No pintamos si el color es transparente
            {
                return;
            }
        }
        else
        {
            SetStatus($"Error: Color '{color}' no es válido.", true);
        }
        // Asegurar que el tamaño del pincel sea impar 
        if (brushSize % 2 == 0)
        {
            brushSize--; // Tomar el número impar inmediatamente menor
            if (brushSize < 1) brushSize = 1; // Mínimo tamaño 1
        }

        // Calcular el radio del pincel (mitad del tamaño)
        int radius = (brushSize - 1) / 2;

        // Iterar sobre el área cuadrada que cubre el pincel
        for (int offsetX = -radius; offsetX <= radius; offsetX++)
        {
            for (int offsetY = -radius; offsetY <= radius; offsetY++)
            {
                // Calcular coordenada objetivo
                int targetX = x + offsetX;
                int targetY = y + offsetY;

                // Verificar si la coordenada está dentro del canvas
                if (IsInCanvas(targetX, targetY))
                {
                    // Pintar el píxel individual
                    _pixelCanvas.SetPixel(targetX, targetY, _colorNameMap[color]);
                }
            }
        }
    }
    private static bool IsInCanvas(int x, int y)
    {
        return x >= 0 && x < _pixelCanvas.CanvasDimension && y>=0 && y < _pixelCanvas.CanvasDimension;
    }


    public static void Paint()
    {
        Paint(Wall_E.Instance.x, Wall_E.Instance.y, Wall_E.Instance.currentColor, Wall_E.Instance.brushSize);
    }
    public static void Paint(int x, int y)
    {
        Paint(x, y, Wall_E.Instance.currentColor, Wall_E.Instance.brushSize);
    }


    public static Color GetPixelColorFromCanvas(int x, int y)
    {
        return _pixelCanvas.GetPixel(x, y);
    }
    public static bool IsValidColorName(string colorName) => _colorNameMap.ContainsKey(colorName);

    public static Color GetAvaloniaColor(string colorName)
    {
        if (IsValidColorName(colorName)) return _colorNameMap[colorName];
        else throw new Exception("Invalid color name");
    }

    private void ResizeCanvasButton_Click(object? sender, RoutedEventArgs e)
    {
        int newSize = (int)_canvasSizeTextBox.Value;
        _pixelCanvas.ResizeCanvas(newSize);
        Wall_E.Set(-1, -1, "Transparent", 1);
        SetStatus($"Canvas redimensionado a {newSize}x{newSize}.", false);
    }
    private void ExecuteButton_Click(object? sender, RoutedEventArgs e)
    {

        if (string.IsNullOrWhiteSpace(_codeEditorTextBox.Text))
        {
            SetStatus("No hay código para ejecutar.", true);
            return;
        }
        _pixelCanvas.Clear();
        _pixelCanvas.ResizeCanvas((int)_canvasSizeTextBox.Value);
        Wall_E.Set(-1, -1, "Transparent", 1);
        Wall_E.Instance.isSpawn = false;
        SetStatus("Ejecutando código...", false);

        interpreter = new PixelWallE();
        try
        {
            interpreter.Run(_codeEditorTextBox.Text);
        }
        catch (RuntimeError error)
        {
            SetStatus($"Error: {error.Message}", true);
        }
        // Después de que el intérprete termine, refresca el canvas UNA VEZ.
        _pixelCanvas.Refresh();


    }
    private async void LoadScriptButton_Click(object? sender, RoutedEventArgs e)
    {
        var openDialog = new OpenFileDialog
        {
            Title = "Cargar Script Wall-E (.pw)",
            Filters = new List<FileDialogFilter> { new FileDialogFilter { Name = "Archivos PW", Extensions = { "pw" } } }
        };

        string[]? result = await openDialog.ShowAsync(this);

        if (result != null && result.Length > 0 && File.Exists(result[0]))
        {
            try
            {
                string code = await File.ReadAllTextAsync(result[0]);
                if (_codeEditorTextBox != null) _codeEditorTextBox.Text = code;
                SetStatus($"Script '{Path.GetFileName(result[0])}' cargado.", false);
            }
            catch (Exception ex)
            {
                SetStatus($"Error al cargar el script: {ex.Message}", true);
            }
        }
    }

    private async void SaveScriptButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (_codeEditorTextBox == null || string.IsNullOrWhiteSpace(_codeEditorTextBox.Text))
        {
            SetStatus("No hay código para guardar.", true);
            return;
        }

        var saveDialog = new SaveFileDialog
        {
            Title = "Guardar Script Wall-E (.pw)",
            DefaultExtension = "pw",
            Filters = new List<FileDialogFilter> { new FileDialogFilter { Name = "Archivos PW", Extensions = { "pw" } } }
        };

        string? result = await saveDialog.ShowAsync(this);

        if (!string.IsNullOrEmpty(result))
        {
            try
            {
                await File.WriteAllTextAsync(result, _codeEditorTextBox.Text);
                SetStatus($"Script guardado como '{Path.GetFileName(result)}'.", false);
            }
            catch (Exception ex)
            {
                SetStatus($"Error al guardar el script: {ex.Message}", true);
            }
        }
    }

    // --- Utilidades ---
    // private static uint ConvertAvaloniaColorToUint32(Color color)
    // {
    //     // Formato BGRA8888: Blue en el byte menos significativo
    //     return (uint)(color.B | (color.G << 8) | (color.R << 16) | (color.A << 24));
    // }

    public static void SetStatus(string message, bool isError)
    {
        if (_statusOutputTextBlock == null) return;
        _statusOutputTextBlock.Text = message;
        _statusOutputTextBlock.Foreground = isError ? Brushes.Red : Brushes.Green; // O SystemColors.ControlTextBrush para normal
    }

    // public void UpdateWallEStatusDisplay()
    // {

    //     if (WallEStatusXTextBlock != null) WallEStatusXTextBlock.Text = $"X: {walle.x}";
    //     if (WallEStatusYTextBlock != null) WallEStatusYTextBlock.Text = $"Y: {walle.y}";
    //     if (WallEStatusColorTextBlock != null) WallEStatusColorTextBlock.Text = $"Color Pincel: {walle.currentColor}";
    //     if (WallEStatusSizeTextBlock != null) WallEStatusSizeTextBlock.Text = $"Tamaño Pincel: {walle.brushSize}";
    // }
    private static void MoveWalle(int x, int y)
    {
        Wall_E.Instance.x = x;
        Wall_E.Instance.y = y;
    }
    public static void Spawn(int x, int y)
    {
        MoveWalle(x, y);
    }

    public static void Color(string color)
    {
        Wall_E.Instance.currentColor = color;
    }
    public static void Size(int size)
    {
        Wall_E.Instance.brushSize = size;
    }

    public static void DrawLine(int dirX, int dirY, int distance)
    {
        DrawLine(Wall_E.Instance.x, Wall_E.Instance.y, dirX, dirY, distance);

    }
    public static void DrawLine(int currentX, int currentY, int dirX, int dirY, int distance)
    {
        if (dirX < -1 || dirX > 1 || dirY < -1 || dirY > 1)
        {
            SetStatus("Parameters out of range: directions are between -1 and 1", true);
            return;
        }


        for (int i = 0; i < distance; i++)
        {
            Paint(currentX, currentY, Wall_E.Instance.currentColor, Wall_E.Instance.brushSize);
            currentX += dirX;
            currentY += dirY;
            MoveWalle(currentX, currentY);
        }
    }

    // 1. Algoritmo del Punto Medio (Midpoint)
    public static void DrawCircle(int dirX, int dirY, int radius)
    {


        if (dirX == 0 && dirY == 0) DrawCircle(radius);
        else
        {
            MoveWalle(Wall_E.Instance.x + dirX * radius, Wall_E.Instance.y + dirY * radius);
            DrawCircle(radius);
        }
    }
    public static void DrawCircle(int radius)
    {
        if (radius == 0)
        {
            Paint(Wall_E.Instance.x, Wall_E.Instance.y);
            return;
        }
        if (radius <= 0)
        {
            DrawCircle(-radius);
            return;
        }

        int xc = Wall_E.Instance.x;
        int yc = Wall_E.Instance.y;
        int x = 0;
        int y = radius;
        int d = 1 - radius;

        PaintCirclePoints(xc, yc, x, y);

        while (x < y)
        {
            x++;
            if (d < 0)
            {
                d += 2 * x + 1;
            }
            else
            {
                y--;
                d += 2 * (x - y) + 1;
            }
            PaintCirclePoints(xc, yc, x, y);
        }
    }

    private static void PaintCirclePoints(int xc, int yc, int x, int y)
    {
        Paint(xc + x, yc + y);
        Paint(xc - x, yc + y);
        Paint(xc + x, yc - y);
        Paint(xc - x, yc - y);
        Paint(xc + y, yc + x);
        Paint(xc - y, yc + x);
        Paint(xc + y, yc - x);
        Paint(xc - y, yc - x);
    }
    public static void DrawRectangle(int dirx, int diry, int distance, int width, int height)
    {
        if ((dirx == 0 && diry == 0) || distance == 0) DrawRectangle(width, height);
        if (width == 0 || height == 0) return;
        else
        {
            MoveWalle(Wall_E.Instance.x + dirx * distance, Wall_E.Instance.y + diry * distance);
            DrawRectangle(width, height);
        }
    }
    public static void DrawRectangle(int width, int height)
    {
        int wallex = Wall_E.Instance.x;
        int walley = Wall_E.Instance.y;
        int x1 = wallex - (width - 1);
        int x2 = wallex + (width - 1);
        int y1 = walley - (height - 1);
        int y2 = walley + (height - 1);

        //(x1 , y1) to (x2 , y1)
        DrawLine(x1, y1, 1, 0, x2 - x1);
        //(x2 , y1) to (x2 , y2)
        DrawLine(x2, y1, 0, 1, y2 - y1);
        //(x2 , y2) to (x1 , y2)
        DrawLine(x2, y2, -1, 0, x2 - x1);
        //(x1 , y2) to (x1 , y1)
        DrawLine(x1, y2, 0, -1, y2 - y1);

        MoveWalle(wallex, walley);

    }
    public static void Fill()
    {
        int x = Wall_E.Instance.x;
        int y = Wall_E.Instance.y;
        if (GetPixelColorFromCanvas(x, y) == _colorNameMap[Wall_E.Instance.currentColor]) return;

        if (!_colorNameMap.ContainsKey(Wall_E.Instance.currentColor))
        {
            SetStatus("Invalid Color", true);
            return;
        }
        // Change 0s for walle.x , walle.y
        Fill(x, y, GetPixelColorFromCanvas(x, y));

    }
    private static void Fill(int x, int y, Color color)
    {
        if (color != GetPixelColorFromCanvas(x, y)) return;

        Paint(x, y, Wall_E.Instance.currentColor, Wall_E.Instance.brushSize);
        Fill(x + 1, y, color);
        Fill(x - 1, y, color);
        Fill(x, y + 1, color);
        Fill(x, y - 1, color);
    }

    public static int GetActualX() => Wall_E.Instance.x;
    public static int GetActualY() => Wall_E.Instance.y;
    public static int GetCanvasSize() => _pixelCanvas.CanvasDimension;
    public static int GetColorCount(string color, int x1, int y1, int x2, int y2)
    {
        if (x1 > x2) return GetColorCount(color, x2, y1, x1, y2);
        if (y1 > y2) return GetColorCount(color, x1, y2, x2, y1);
        if (x1 < 0 || y1 < 0 || x2 >= _pixelCanvas.CanvasDimension || y2 >= _pixelCanvas.CanvasDimension) return 0;

        int count = 0;
        for (int i = x1; i <= x2; i++)
        {
            for (int j = y1; j <= y2; j++)
            {
                if (GetPixelColorFromCanvas(i, j) == _colorNameMap[color]) count++;
            }
        }
        return count;
    }
    public static int IsBrushColor(string color)
    {

        return color == Wall_E.Instance.currentColor ? 1 : 0;
    }
    public static int IsBrushSize(int size)
    {
        return size == Wall_E.Instance.brushSize ? 1 : 0;
    }
    public static int IsCanvasColor(string color, int vertical, int horizontal)
    {

        return GetPixelColorFromCanvas(Wall_E.Instance.x + vertical,
                                       Wall_E.Instance.y + horizontal) == _colorNameMap[color] ? 1 : 0;
    }

}
