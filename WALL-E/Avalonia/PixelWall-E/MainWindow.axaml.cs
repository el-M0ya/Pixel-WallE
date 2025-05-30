using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using YourProjectName.ViewModels; // Ajusta tu namespace

namespace YourProjectName
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            // Opcional: Si no configuras el DataContext en XAML para la ventana, hazlo aquí
            // DataContext = new MainWindowViewModel();
        }

        // Manejador de eventos para los botones de color
        private void ColorButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.CommandParameter is Color color && DataContext is MainWindowViewModel vm)
            {
                vm.SelectedColor = color;
            }
        }
    }
}




// using Avalonia;
// using Avalonia.Controls;
// using Avalonia.Input;
// using Avalonia.Markup.Xaml;
// using Avalonia.Media;
// using Avalonia.Media.Immutable;
// using System;
// namespace PixelWall_E
// {



// public partial class MainWindow:Window
// {
//     public MainWindow()
//     {
//         InitializeComponent();
//         #if DEBUG
//         this.AttachDevTools();
//         #endif
//     }
//     private void InitializeComponent()
//     {
//         AvaloniaXamlLoader.Load(this);
//     }

// }

//     public class PixelArtCanvas : Control
//     {
//         private int[,] _pixelColors;
//         private int _gridSize = 16;
        
//         // Corrección para DirectProperty
//         public static readonly StyledProperty<int> GridSizeProperty =
//             AvaloniaProperty.Register<PixelArtCanvas, int>(
//                 nameof(GridSize),
//                 defaultValue: 16);
        
//         public int GridSize
//         {
//             get => GetValue(GridSizeProperty);
//             set => SetValue(GridSizeProperty, value);
//         }
        
//         static PixelArtCanvas()
//         {
//             AffectsRender<PixelArtCanvas>(GridSizeProperty);
//         }
        
//         public PixelArtCanvas()
//         {
//             InitializePixelGrid();
//         }
        
//         private void InitializePixelGrid()
//         {
//             _pixelColors = new int[_gridSize, _gridSize];
//             for (int x = 0; x < _gridSize; x++)
//             {
//                 for (int y = 0; y < _gridSize; y++)
//                 {
//                     // Corrección para Colors.Transparent
//                     _pixelColors[x, y] = (int)Colors.Transparent.ToUInt32();
//                 }
//             }
//             InvalidateVisual();
//         }
        
//         public override void Render(DrawingContext context)
//         {
//             base.Render(context);
//             System.Console.WriteLine("Renderrrrrrrrrrrrrr");
            
//             if (_gridSize <= 0 || Bounds.Width <= 0 || Bounds.Height <= 0)
//             return;
            
//             var cellWidth = Bounds.Width / _gridSize;
//             var cellHeight = Bounds.Height / _gridSize;
            
//             for (int x = 0; x < _gridSize; x++)
//             {
//                 for (int y = 0; y < _gridSize; y++)
//                 {
//                     // Corrección para Rect
//                     var rect = new Avalonia.Rect(
//                         x * cellWidth, 
//                         y * cellHeight, 
//                         cellWidth, 
//                         cellHeight);
                    
//                     // Corrección para Color
//                     var color = Avalonia.Media.Color.FromUInt32((uint)_pixelColors[x, y]);
//                     var brush = new ImmutableSolidColorBrush(color);
                    
//                     context.FillRectangle(brush, rect);
//                     context.DrawRectangle(new Pen(Brushes.Black, 0.5), rect);
//                 }
//             }
//         }
        
//         protected override void OnPointerPressed(PointerPressedEventArgs e)
//         {
//             base.OnPointerPressed(e);
//             var point = e.GetPosition(this); // Corrección para GetPosition
//             HandlePixelClick(point);
//         }
        
//         protected override void OnPointerMoved(PointerEventArgs e)
//         {
//             base.OnPointerMoved(e);
//             if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
//             {
//                 var point = e.GetPosition(this); // Corrección para GetPosition
//                 HandlePixelClick(point);
//             }
//         }
        
//         private void HandlePixelClick(Avalonia.Point position)
//         {
//             if (_gridSize <= 0) return;
            
//             var cellWidth = Bounds.Width / _gridSize;
//             var cellHeight = Bounds.Height / _gridSize;
            
//             int x = (int)(position.X / cellWidth);
//             int y = (int)(position.Y / cellHeight);
            
//             if (x >= 0 && x < _gridSize && y >= 0 && y < _gridSize)
//             {
//                 _pixelColors[x, y] = (int)Colors.Red.ToUInt32();
//                 InvalidateVisual();
//             }
//         }
        
//         public int[,] GetPixelData() => (int[,])_pixelColors.Clone();

// public void SetPixelData(int[,] data)
//         {
//             if (data == null || data.GetLength(0) != _gridSize || data.GetLength(1) != _gridSize)
//                 throw new ArgumentException("Dimensiones incorrectas");
            
//             _pixelColors = (int[,])data.Clone();
//             InvalidateVisual();
//         }
        
//         public void ChangeGridSize(int newSize)
//         {
//             if (newSize <= 0) return;
            
//             var oldData = _pixelColors;
//             _gridSize = newSize;
//             InitializePixelGrid();
            
//             if (oldData != null)
//             {
//                 int copySize = Math.Min(newSize, oldData.GetLength(0));
//                 for (int x = 0; x < copySize; x++)
//                 {
//                     for (int y = 0; y < copySize; y++)
//                     {
//                         _pixelColors[x, y] = oldData[x, y];
//                     }
//                 }
//             }
            
//             InvalidateVisual();
//         }
//     }
}