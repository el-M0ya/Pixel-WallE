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
using System.Threading.Tasks; // Para diálogos asíncronos
namespace PW;

public class Wall_E
{
    public bool isSpawn = false;
    public int x;
    public int y;
    public string currentColor;
    public int brushSize;
    public static Wall_E Instance{get; private set;}
    public Wall_E(int x, int y, string currentColor, int brushSize)
    {
        if (Instance == null) Instance = this;
        else throw new Exception("Many Wall_E's in scene");
        this.x = x;
        this.y = y;
        this.currentColor = currentColor;
        this.brushSize = brushSize;
    }
}
public partial class MainWindow : Window
{
    public Wall_E walle;
    private static WriteableBitmap? _canvasBitmap;
    private static int _canvasDimension = 32; // Tamaño por defecto NxN
    private static Color[,] _logicalPixelData; // Representación lógica de los colores en el canvas (opcional pero útil)
    private bool isWallEImage = false;
    private static readonly Dictionary<string, Color> _colorNameMap = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase)
        {
            { "Black", Colors.Black }, { "White", Colors.White },
            { "Red", Colors.Red }, { "Green", Colors.Green },
            { "Blue", Colors.Blue }, { "Yellow", Colors.Yellow },
            { "Orange", Colors.Orange }, { "Purple", Colors.Purple },
            { "Transparent", Colors.Transparent }
        };
    private static Color _defaultCanvasColor = Colors.White; // El canvas inicia en blanco

    // Referencias a los controles (se asignan en el constructor después de InitializeComponent)
    private NumericUpDown? CanvasSizeTextBox;
    private Button? ResizeCanvasButton;
    private Button? ExecuteButton;
    private Button? LoadScriptButton;
    private Button? SaveScriptButton;
    private TextEditor? CodeEditorTextBox;
    private static Image? PixelCanvasImage;
    private static TextBlock? StatusOutputTextBlock;
    private TextBlock? WallEStatusXTextBlock;
    private TextBlock? WallEStatusYTextBlock;
    private TextBlock? WallEStatusColorTextBlock;
    private TextBlock? WallEStatusSizeTextBlock;
    private PixelWallE interpreter;

    public MainWindow()
    {
        InitializeComponent();



#if DEBUG
        this.AttachDevTools(); // Útil para depurar la UI (F12)
#endif

        CanvasSizeTextBox = this.FindControl<NumericUpDown>("_CanvasSizeTextBox");
        ResizeCanvasButton = this.FindControl<Button>("_ResizeCanvasButton");
        ExecuteButton = this.FindControl<Button>("_ExecuteButton");
        LoadScriptButton = this.FindControl<Button>("_LoadScriptButton");
        SaveScriptButton = this.FindControl<Button>("_SaveScriptButton");
        CodeEditorTextBox = this.FindControl<TextEditor>("_CodeEditorTextBox");
        PixelCanvasImage = this.FindControl<Image>("_PixelCanvasImage");
        StatusOutputTextBlock = this.FindControl<TextBlock>("_StatusOutputTextBlock");

        WallEStatusXTextBlock = this.FindControl<TextBlock>("_WallEStatusXTextBlock");
        WallEStatusYTextBlock = this.FindControl<TextBlock>("_WallEStatusYTextBlock");
        WallEStatusColorTextBlock = this.FindControl<TextBlock>("_WallEStatusColorTextBlock");
        WallEStatusSizeTextBlock = this.FindControl<TextBlock>("_WallEStatusSizeTextBlock");


        if (CanvasSizeTextBox != null) CanvasSizeTextBox.Text = _canvasDimension.ToString();
        if (ResizeCanvasButton != null) ResizeCanvasButton.Click += _ResizeCanvasButton_Click;
        if (ExecuteButton != null) ExecuteButton.Click += _ExecuteButton_Click;
        if (LoadScriptButton != null) LoadScriptButton.Click += _LoadScriptButton_Click;
        if (SaveScriptButton != null) SaveScriptButton.Click += _SaveScriptButton_Click;

        CreateOrResizeCanvas(_canvasDimension);

        interpreter = new PixelWallE();
        walle = new Wall_E(1, 1, "Red", 1);
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    private void _ChangeImg(object sender, RoutedEventArgs e)
    {
        var image = (Image)((Button)sender).Content;
        image.Source = new Bitmap(isWallEImage
        ? "C:/000mio/Universidad/Programacion/Avalonia Pojects/WALL-E/Avalonia/PW/PW/Assets/EVA.png"
        : "C:/000mio/Universidad/Programacion/Avalonia Pojects/WALL-E/Avalonia/PW/PW/Assets/WALL-E.png");
        isWallEImage = !isWallEImage;
    }

    private void CreateOrResizeCanvas(int newDimension)
    {
        if (newDimension <= 0)
        {
            SetStatus("Error: La dimensión del canvas debe ser un número positivo.", true);
            return;
        }

        _canvasDimension = newDimension;
        _logicalPixelData = new Color[_canvasDimension, _canvasDimension];

        // Crear/Recrear el WriteableBitmap
        _canvasBitmap = new WriteableBitmap(
            new PixelSize(_canvasDimension, _canvasDimension),
            new Vector(96, 96), // DPI estándar
            PixelFormat.Bgra8888, // Formato común: Blue, Green, Red, Alpha
            AlphaFormat.Premul); // O Unpremultiplied, Premultiplied es común

        if (PixelCanvasImage != null)
        {
            PixelCanvasImage.Source = _canvasBitmap;
            // Ajustar el tamaño del control Image si es necesario, aunque Stretch="Uniform" ayuda.
            // PixelCanvasImage.Width = _canvasDimension * pixelDisplaySize; // Si quieres píxeles más grandes
            // PixelCanvasImage.Height = _canvasDimension * pixelDisplaySize;
        }

        // Limpiar el canvas a blanco según especificación
        ClearCanvasToDefault();
        SetStatus($"Canvas inicializado/redimensionado a {_canvasDimension}x{_canvasDimension}. Listo.", false);
        UpdateWallEStatusDisplay(); // Resetear estado visual
    }

    private void ClearCanvasToDefault()
    {
        if (_canvasBitmap == null) return;

        // Rellenar la representación lógica
        for (int y = 0; y < _canvasDimension; y++)
        {
            for (int x = 0; x < _canvasDimension; x++)
            {
                _logicalPixelData[x, y] = _defaultCanvasColor;
            }
        }

        // Rellenar el WriteableBitmap
        using (var framebuffer = _canvasBitmap.Lock())
        {
            unsafe
            {
                uint colorValue = ConvertAvaloniaColorToUint32(_defaultCanvasColor);
                for (int i = 0; i < framebuffer.Size.Width * framebuffer.Size.Height; i++)
                {
                    *((uint*)framebuffer.Address + i) = colorValue;
                }
            }
        }
        PixelCanvasImage?.InvalidateVisual(); // Forzar redibujado
    }
    public static void Paint(int x, int y, string colorName, int brushRawSize)
    {
        if (_canvasBitmap == null) return;
        if (x < 0 || x >= _canvasDimension || y < 0 || y >= _canvasDimension)
        {
            throw new RuntimeError($"Intento de dibujar fuera del canvas en ({x},{y}).");
        }

        if (!_colorNameMap.TryGetValue(colorName, out Color avaloniaColor))
        {
            throw new RuntimeError($"Color '{colorName}' no reconocido.");
        }

        // "Utilizar el color transparente implica no realizar ningún cambio sobre el canvas."
        if (colorName.Equals("Transparent", StringComparison.OrdinalIgnoreCase))
        {
            return; // No se pinta nada
        }

        // Ajustar tamaño del pincel según reglas
        int brushSize = brushRawSize;
        if (brushSize < 1) brushSize = 1;
        if (brushSize % 2 == 0 && brushSize > 0) brushSize--;

        int halfBrush = (brushSize - 1) / 2;

        using (var framebuffer = _canvasBitmap.Lock())
        {
            unsafe
            {
                uint colorUint = ConvertAvaloniaColorToUint32(avaloniaColor);
                for (int brushY = -halfBrush; brushY <= halfBrush; brushY++)
                {
                    for (int brushX = -halfBrush; brushX <= halfBrush; brushX++)
                    {
                        int cX = x + brushX;
                        int cY = y + brushY;

                        if (cX >= 0 && cX < _canvasDimension && cY >= 0 && cY < _canvasDimension)
                        {
                            _logicalPixelData[cX, cY] = avaloniaColor; // Actualizar representación lógica

                            // Escribir en el bitmap
                            uint* pixelPtr = (uint*)((byte*)framebuffer.Address + cY * framebuffer.RowBytes + cX * 4);
                            *pixelPtr = colorUint;
                        }
                    }
                }
            }
        }
        PixelCanvasImage?.InvalidateVisual(); // Podrías invalidar solo la región afectada para optimizar
        SetStatus("pintando", false);
    }
    public static void PaintInWallE()
    {
        Paint(Wall_E.Instance.x, Wall_E.Instance.y, Wall_E.Instance.currentColor, Wall_E.Instance.brushSize);
    }

    public static Color GetPixelColorFromCanvas(int x, int y)
    {
        if (x < 0 || x >= _canvasDimension || y < 0 || y >= _canvasDimension)
        {
            return Colors.Transparent; // O un color especial para "fuera de límites"
        }
        return _logicalPixelData[x, y];
    }


    public static bool IsValidColorName(string colorName) => _colorNameMap.ContainsKey(colorName);

    public static Color GetAvaloniaColor(string colorName)
    {
        if (IsValidColorName(colorName)) return _colorNameMap[colorName];
        else throw new Exception("Invalid color name");
    }

    private void _ResizeCanvasButton_Click(object? sender, RoutedEventArgs e)
    {
        if (CanvasSizeTextBox != null && int.TryParse(CanvasSizeTextBox.Text, out int newSize))
        {
            CreateOrResizeCanvas(newSize);
        }
        else
        {
            SetStatus("Error: Dimensión de canvas inválida. Debe ser un número entero.", true);
        }
    }

    private void _ExecuteButton_Click(object? sender, RoutedEventArgs e)
    {
        if (CodeEditorTextBox == null || string.IsNullOrWhiteSpace(CodeEditorTextBox.Text))
        {
            SetStatus("No hay código para ejecutar.", true);
            return;
        }
        string code = CodeEditorTextBox.Text;
        SetStatus("Ejecutando código...", false);


        interpreter.Run(code);

    }

    private async void _LoadScriptButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
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
                if (CodeEditorTextBox != null) CodeEditorTextBox.Text = code;
                SetStatus($"Script '{Path.GetFileName(result[0])}' cargado.", false);
            }
            catch (Exception ex)
            {
                SetStatus($"Error al cargar el script: {ex.Message}", true);
            }
        }
    }

    private async void _SaveScriptButton_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (CodeEditorTextBox == null || string.IsNullOrWhiteSpace(CodeEditorTextBox.Text))
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
                await File.WriteAllTextAsync(result, CodeEditorTextBox.Text);
                SetStatus($"Script guardado como '{Path.GetFileName(result)}'.", false);
            }
            catch (Exception ex)
            {
                SetStatus($"Error al guardar el script: {ex.Message}", true);
            }
        }
    }

    // --- Utilidades ---
    private static uint ConvertAvaloniaColorToUint32(Color color)
    {
        // Formato BGRA8888: Blue en el byte menos significativo
        return (uint)(color.B | (color.G << 8) | (color.R << 16) | (color.A << 24));
    }

    public static void SetStatus(string message, bool isError)
    {
        if (StatusOutputTextBlock == null) return;
        StatusOutputTextBlock.Text = message;
        StatusOutputTextBlock.Foreground = isError ? Brushes.Red : Brushes.Green; // O SystemColors.ControlTextBrush para normal
    }

    public void UpdateWallEStatusDisplay()
    {

        if (WallEStatusXTextBlock != null) WallEStatusXTextBlock.Text = $"X: {walle.x}";
        if (WallEStatusYTextBlock != null) WallEStatusYTextBlock.Text = $"Y: {walle.y}";
        if (WallEStatusColorTextBlock != null) WallEStatusColorTextBlock.Text = $"Color Pincel: {walle.currentColor}";
        if (WallEStatusSizeTextBlock != null) WallEStatusSizeTextBlock.Text = $"Tamaño Pincel: {walle.brushSize}";
    }
    public static void MoveWalle(int x, int y)
    {
        Wall_E.Instance.x = x;
        Wall_E.Instance.y = y;
    }
    public static void Spawn(int x, int y)
    {
        SetStatus("Is Working", false);
        if (Wall_E.Instance.isSpawn)
        {
            throw new RuntimeError("1");
        }    
        MoveWalle(x, y);
        Wall_E.Instance.isSpawn = true;

    }

    public static void Color(string color)
    {
        if (_colorNameMap.ContainsKey(color)) Wall_E.Instance.currentColor = color;
        // else
        // {
        //     throw new RuntimeError("Wrong Color");
        // }
        
    }
    public static void Size(int size)
    {
        Wall_E.Instance.brushSize = size;
    }
    public static void DrawLine(int dirx, int diry, int distance)
    {
        if (dirx < -1 || dirx > 1 || diry < -1 || diry > 1) throw new RuntimeError("Parameters out of range: directions are between -1 and 1");
        if (distance < 0) DrawLine(-dirx, -diry, -distance);

        for (int i = 0; i < distance; i++)
        {
            MoveWalle(dirx, diry);
            PaintInWallE();
            distance--;
        }
    }
    public static void DrawCircle(int dirx, int diry, int r)
    {
        //MoveWalle(walle.x + r*dirx , walle.y + r*diry );




    }
    public static void DrawRectangle(int dirx, int diry, int distance, int width, int high)
    {
        //MoveWalle(walle.x + r*dirx , walle.y + r*diry );




    }
    public static void Fill()
    {
        Color color = GetPixelColorFromCanvas(Wall_E.Instance.x, Wall_E.Instance.y);
        // Change 0s for walle.x , walle.y
        Fill(Wall_E.Instance.x, Wall_E.Instance.y, color);

    }
    private static void Fill(int x, int y, Color color)
    {
        if (color != GetPixelColorFromCanvas(x, y)) return;

        Fill(x + 1, y, color);
        Fill(x - 1, y, color);
        Fill(x, y + 1, color);
        Fill(x, y - 1, color);
    }

    public static int GetActualX() => Wall_E.Instance.x;
    public static int GetActualY() => Wall_E.Instance.y;
    public static int GetCanvasSize() => _canvasDimension;
    public static int GetColorCount(string color, int x1, int y1, int x2, int y2)
    {
        if (x1 > x2) return GetColorCount(color, x2, y1, x1, y2);
        if (y1 > y2) return GetColorCount(color, x1, y2, x2, y1);

        int count = 0;
        for (int i = x1; i <= x2; i++)
        {
            for (int j = y1; j <= y2; j++)
            {
                if (GetPixelColorFromCanvas(i , j) == _colorNameMap[color]) count++;
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

    // public static string GetColor()
    // {
    //     return GetColor(/*Walle.x , walle.y*/);
    // }
    // public static string GetColor(int x , int y)
    // {
    //     return "";
    // }

}
