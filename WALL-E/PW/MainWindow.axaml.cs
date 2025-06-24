using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using Avalonia;
using Avalonia.Media;
using Avalonia.Platform; 
using System;
using System.Collections.Generic;
using System.IO; 
using System.Linq; 
using System.Threading.Tasks;
using SkiaSharp;
namespace PW;
using System.Threading;
using TextMateSharp;
using AvaloniaEdit.TextMate;
using AvaloniaEdit.Highlighting.Xshd;

public class Wall_E
{
    public bool isSpawn = false;
    public bool isFilling = false;
    public int x;
    public int y;
    public string currentColor;
    public int brushSize;

    public static Wall_E Instance { get; private set; }
    public Wall_E(int x, int y, string currentColor, int brushSize)
    {
        if (Instance == null) Instance = this;
        else throw new Exception("Many Wall_E's in Canvas");
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
    public static void MoveWalle(int x, int y)
    {
        Instance.x = x;
        Instance.y = y;
    }
}
public partial class MainWindow : Window
{
    private CancellationTokenSource _cancellationTokenSource;
    private bool _isExecuting = false;
    public static bool isWallEImage = true;
    // Diccionario de colores, ahora accesible para todos
    public static readonly Dictionary<string, Color> _colorNameMap = new(StringComparer.OrdinalIgnoreCase)
        {
            { "Black", Colors.Black }, { "White", Colors.White },
            { "Red", Colors.Red }, { "Green", Colors.Green },
            { "Blue", Colors.Blue }, { "Yellow", Colors.Yellow },
            { "Orange", Colors.Orange }, { "Purple", Colors.Purple },
            { "Brown", Colors.Brown }, { "Peru", Colors.Peru },
            { "Violet", Colors.Violet}, { "Cyan", Colors.Cyan },
            { "Gray", Colors.Gray}, { "Pink", Colors.Pink },
            { "Transparent", Colors.Transparent } // Transparente es especial, significa "no pintar"
        };

    // Referencias a los controles de la UI
    private NumericUpDown _canvasSizeTextBox;
    private TextEditor _codeEditorTextBox;
    private static TextBlock? _statusOutputTextBlock;
    public static PixelCanvasControl? _pixelCanvas;

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
        this.FindControl<Button>("_StopButton").Click += StopButton_Click;
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
    public static async Task Paint(int x, int y, string color, int brushSize, CancellationToken cancellationToken = default)
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
                cancellationToken.ThrowIfCancellationRequested();
                // Calcular coordenada objetivo
                int targetX = x + offsetX;
                int targetY = y + offsetY;

                // Verificar si la coordenada está dentro del canvas
                if (IsInCanvas(targetX, targetY))
                {
                    // Pintar el píxel individual
                    await _pixelCanvas.SetPixel(targetX, targetY, _colorNameMap[color]);
                    _pixelCanvas.Refresh(); // Forzar actualización después de cada píxel
                    int time = Wall_E.Instance.isFilling ? (int)Values.FillTime : (int)Values.DelayTime;
                    await Task.Delay(time, cancellationToken); // Pequeño retraso para visualización

                }
            }
        }
    }
    public static async Task Paint(CancellationToken cancellationToken = default)
    {
        await Paint(Wall_E.Instance.x, Wall_E.Instance.y, Wall_E.Instance.currentColor, Wall_E.Instance.brushSize, cancellationToken);
    }
    public static async Task Paint(int x, int y, CancellationToken cancellationToken = default)
    {
        await Paint(x, y, Wall_E.Instance.currentColor, Wall_E.Instance.brushSize, cancellationToken);
    }
    public static bool IsInCanvas(int x, int y)
    {
        return x >= 0 && x < _pixelCanvas.CanvasDimension && y >= 0 && y < _pixelCanvas.CanvasDimension;
    }
    public static Color GetPixelColorFromCanvas(int x, int y)
    {
        return _pixelCanvas.GetPixel(x, y);
    }

    // Buttoms
    private void ResizeCanvasButton_Click(object? sender, RoutedEventArgs e)
    {
        int newSize = (int)_canvasSizeTextBox.Value;
        _pixelCanvas.ResizeCanvas(newSize);
        Wall_E.Set(-1, -1, "Transparent", 1);
        SetStatus($"Canvas redimensionado a {newSize}x{newSize}.", false);
    }
    private void StopButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_isExecuting && _cancellationTokenSource != null)
        {
            _cancellationTokenSource.Cancel();
            _pixelCanvas.Clear();
            Wall_E.Set(-1, -1, "Transparent", 1);
        }
    }
    private async void ExecuteButton_Click(object? sender, RoutedEventArgs e)
    {
        if (_isExecuting) return; // Evitar múltiples ejecuciones simultáneas

        var executeButton = this.FindControl<Button>("_ExecuteButton");
        var resizeButton = this.FindControl<Button>("_ResizeCanvasButton");
        var stopButton = this.FindControl<Button>("_StopButton");

        try
        {
            _isExecuting = true;
            executeButton.IsEnabled = false;
            resizeButton.IsEnabled = false;
            stopButton.IsEnabled = true;

            if (string.IsNullOrWhiteSpace(_codeEditorTextBox.Text))
            {
                SetStatus("No hay código para ejecutar.", true);
                return;
            }

            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            // Limpiar y preparar el canvas
            _pixelCanvas.Clear();
            _pixelCanvas.ResizeCanvas((int)_canvasSizeTextBox.Value);
            Wall_E.Set(-1, -1, "Transparent", 1);
            Wall_E.Instance.isSpawn = false;
            SetStatus("Ejecutando código...", false);

            interpreter = new PixelWallE();
            await interpreter.Run(_codeEditorTextBox.Text, cancellationToken);

            SetStatus("Ejecución completada.", false);
        }
        catch (OperationCanceledException)
        {
            SetStatus("Ejecución cancelada.", true);
            _pixelCanvas.Clear(); // Limpiar el canvas al cancelar

        }
        catch (RuntimeError error)
        {
            SetStatus($"Error: {error.Message}", true);
        }
        finally
        {
            _isExecuting = false;
            executeButton.IsEnabled = true;
            resizeButton.IsEnabled = true;
            stopButton.IsEnabled = false;
            _pixelCanvas.Refresh();
            Wall_E.Set(-1, -1, "Transparent", 1);
        }
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
    private async void SaveScriptButton_Click(object? sender, RoutedEventArgs e)
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
    public static void SetStatus(string message, bool isError)
    {
        if (_statusOutputTextBlock == null) return;
        _statusOutputTextBlock.Text = message;
        _statusOutputTextBlock.Foreground = isError ? Brushes.Red : Brushes.Green; // O SystemColors.ControlTextBrush para normal
    }
}