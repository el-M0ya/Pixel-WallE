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
    public int x;
    public int y;
    public string currentColor;
    public int brushSize;
    public Wall_E(int x, int y, string currentColor, int brushSize)
    {
        this.x = x;
        this.y = y;
        this.currentColor = currentColor;
        this.brushSize = brushSize;
    }
}



public partial class MainWindow : Window
{
    private Wall_E walle;
    private TextEditor? CodeEditor;
    private WriteableBitmap? _canvasBitmap;
    private int _canvasDimension = 32; // Tamaño por defecto NxN
    private Color[,] _logicalPixelData; // Representación lógica de los colores en el canvas (opcional pero útil)
    private bool isWallEImage = false;
    private readonly Dictionary<string, Color> _colorNameMap = new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase)
        {
            { "Black", Colors.Black }, { "White", Colors.White },
            { "Red", Colors.Red }, { "Green", Colors.Green },
            { "Blue", Colors.Blue }, { "Yellow", Colors.Yellow },
            { "Orange", Colors.Orange }, { "Purple", Colors.Purple },
            { "Transparent", Colors.Transparent }
        };
    private Color _defaultCanvasColor = Colors.White; // El canvas inicia en blanco

    // Referencias a los controles (se asignan en el constructor después de InitializeComponent)
    private NumericUpDown? CanvasSizeTextBox;
    private Button? ResizeCanvasButton;
    private Button? ExecuteButton;
    private Button? LoadScriptButton;
    private Button? SaveScriptButton;
    private TextEditor? CodeEditorTextBox;
    private Image? PixelCanvasImage;
    private TextBlock? StatusOutputTextBlock;
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
        walle = new Wall_E(-1 , -1 , "Transparent" , 1);
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
    public void SetPixelOnCanvas(int x, int y, string colorName, int brushRawSize)
    {
        if (_canvasBitmap == null) return;
        if (x < 0 || x >= _canvasDimension || y < 0 || y >= _canvasDimension)
        {
            SetStatus($"Intento de dibujar fuera del canvas en ({x},{y}).", true);
            return;
        }

        if (!_colorNameMap.TryGetValue(colorName, out Color avaloniaColor))
        {
            SetStatus($"Color '{colorName}' no reconocido.", true);
            avaloniaColor = Colors.Fuchsia; // Color de error visual
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
    }

    public Color GetPixelColorFromCanvas(int x, int y)
    {
        if (x < 0 || x >= _canvasDimension || y < 0 || y >= _canvasDimension)
        {
            return Colors.Transparent; // O un color especial para "fuera de límites"
        }
        return _logicalPixelData[x, y];
    }

    public int GetCanvasCurrentDimension() => _canvasDimension;

    public bool IsValidColorName(string colorName) => _colorNameMap.ContainsKey(colorName);

    public Color GetAvaloniaColorByName(string colorName)
    {
        _colorNameMap.TryGetValue(colorName, out Color c);
        return c; // Devuelve color por defecto (negro Transparente) si no se encuentra. Tu intérprete debe validar.
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

        // AQUÍ LLAMAS A TU INTÉRPRETE
        interpreter.Run(code);

        // Ejemplo de prueba directa (borrar o comentar después):
        // SetPixelOnCanvas(0, 0, "Red", 1);
        // SetPixelOnCanvas(1, 1, "Green", 3);
        // SetPixelOnCanvas(_canvasDimension - 1, _canvasDimension - 1, "Blue", 1);
        // SetStatus("Prueba de dibujo completada.", false);
        // UpdateWallEStatusDisplay(1,1,"Green",3); // Ejemplo
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
    private uint ConvertAvaloniaColorToUint32(Color color)
    {
        // Formato BGRA8888: Blue en el byte menos significativo
        return (uint)(color.B | (color.G << 8) | (color.R << 16) | (color.A << 24));
    }

    public void SetStatus(string message, bool isError)
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

    public void Spawn(int x, int y)
    {
        walle.x = x;
        walle.y = y;

    }
    // Implementa aquí los métodos que tu intérprete llamará para dibujar y consultar el canvas.
    // Por ejemplo, los métodos de ICanvasController si usas esa interfaz.
    // public (int newX, int newY) DrawLine(...) { ... }
    // public void Fill(...) { ... }


    // Colores soportados por el lenguaje y su mapeo a Avalonia.Color


}
