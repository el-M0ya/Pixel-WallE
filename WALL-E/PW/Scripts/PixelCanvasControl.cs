// PixelCanvasControl.cs
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using System;
using System.Threading.Tasks;
using AvaloniaEdit;
using AvaloniaEdit.Highlighting;
using System.Text.RegularExpressions;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

namespace PW
{
    public class PixelCanvasControl : Control
    {
        // Almacena el estado de cada píxel
        public Color[,] _pixelData;

        // Dimensiones del canvas en píxeles
        public int CanvasDimension { get; private set; } = 32;

        // Pincel para la cuadrícula
        private readonly Pen _gridPen = new(Brushes.LightGray, 0.5);

        public PixelCanvasControl()
        {
            // Inicializa el canvas con el tamaño por defecto al crearse
            ResizeCanvas(CanvasDimension);
        }

        // --- MÉTODOS PÚBLICOS PARA TU INTÉRPRETE ---

        /// <summary>
        /// Redimensiona el canvas y lo limpia (lo pone todo en blanco).
        /// </summary>
        public void ResizeCanvas(int dimension)
        {
            if (dimension <= 0) return;

            CanvasDimension = dimension;
            _pixelData = new Color[dimension, dimension];
            Clear(); // Limpia al nuevo tamaño

        }

        /// <summary>
        /// Pinta un píxel en una coordenada específica.
        /// </summary>
        public void SetPixel(int x, int y, Color color)
        {
            if (x < 0 || x >= CanvasDimension || y < 0 || y >= CanvasDimension)
            {
                MainWindow.SetStatus("Out of range", true);
                
                return;
            }
            _pixelData[x, y] = color;
        }

        /// <summary>
        /// Obtiene el color de un píxel en una coordenada específica.
        /// </summary>
        public Color GetPixel(int x, int y)
        {
            if (x < 0 || x >= CanvasDimension || y < 0 || y >= CanvasDimension)
            {
                return Colors.Transparent; // Color especial para "fuera de límites"
            }
            return _pixelData[x, y];
        }

        /// <summary>
        /// Limpia el canvas, poniéndolo todo en blanco.
        /// </summary>
        public void Clear()
        {
            for (int i = 0; i < CanvasDimension; i++)
            {
                for (int j = 0; j < CanvasDimension; j++)
                {
                    _pixelData[i, j] = Colors.White;
                }
            }
            Refresh(); // Pide un redibujado para mostrar el canvas limpio
        }

        /// <summary>
        /// Le dice a Avalonia que este control necesita ser redibujado.
        /// ¡LLAMA A ESTE MÉTODO DESPUÉS DE UN COMANDO DE DIBUJO COMPLETO!
        /// </summary>
        public void Refresh()
        {
            this.InvalidateVisual();
        }

        // --- MÉTODO DE RENDERIZADO DE AVALONIA ---

        /// <summary>
        /// Avalonia llama a este método para dibujar el control.
        /// </summary>
        public override void Render(DrawingContext context)
        {
            base.Render(context);
            if (_pixelData == null) return;



            // Calcula el tamaño de cada "píxel" en la pantalla
            double cellWidth = this.Bounds.Width / CanvasDimension;
            double cellHeight = this.Bounds.Height / CanvasDimension;

            // Dibuja los píxeles coloreados
            for (int x = 0; x < CanvasDimension; x++)
            {
                for (int y = 0; y < CanvasDimension; y++)
                {
                    var color = _pixelData[x, y];
                    // Optimización: solo dibuja si no es el color de fondo
                    if (color != Colors.White)
                    {
                        var brush = new SolidColorBrush(color);
                        var rect = new Rect(x * cellWidth, y * cellHeight, cellWidth, cellHeight);
                        context.DrawRectangle(brush, null, rect);
                    }
                }
            }

            // Dibuja la cuadrícula
            for (int i = 0; i <= CanvasDimension; i++)
            {
                var p1 = new Point(i * cellWidth, 0);
                var p2 = new Point(i * cellWidth, this.Bounds.Height);
                context.DrawLine(_gridPen, p1, p2);
            }
            for (int i = 0; i <= CanvasDimension; i++)
            {
                var p1 = new Point(0, i * cellHeight);
                var p2 = new Point(this.Bounds.Width, i * cellHeight);
                context.DrawLine(_gridPen, p1, p2);
            }
            int wallex = Wall_E.Instance.x;
            int walley = Wall_E.Instance.y;
            if (wallex < 0 || wallex > CanvasDimension || walley < 0 || walley > CanvasDimension) return;

            var image = new Bitmap(!MainWindow.isWallEImage
            ? "Assets/EVA.png"
            : "Assets/WALL-E.png");

            Rect destRect = new Rect(wallex * cellWidth, walley * cellHeight, cellWidth, cellHeight);
            context.DrawImage(image, destRect);
        }



    }
    public class CustomHighlighting : IHighlightingDefinition
    {
        private readonly Dictionary<string, HighlightingColor> _namedColors;
        private readonly Dictionary<string, HighlightingRuleSet> _namedRuleSets;

        public CustomHighlighting()
        {
            MainRuleSet = new HighlightingRuleSet();
            _namedColors = new Dictionary<string, HighlightingColor>();
            _namedRuleSets = new Dictionary<string, HighlightingRuleSet>();

            // Define tus colores con nombre
            _namedColors["Instruction"] = new HighlightingColor
            {
                Foreground = new SimpleHighlightingBrush(Colors.Violet),
                FontWeight = FontWeight.Bold
            };

            _namedColors["Function"] = new HighlightingColor
            {
                Foreground = new SimpleHighlightingBrush(Colors.Green),
                FontWeight = FontWeight.Bold
            };

            _namedColors["String"] = new HighlightingColor
            {
                Foreground = new SimpleHighlightingBrush(Colors.Peru)
            };
            _namedColors["Number"] = new HighlightingColor
            {
                Foreground = new SimpleHighlightingBrush(Colors.DarkRed)
            };
            _namedColors["Operator"] = new HighlightingColor
            {
                Foreground = new SimpleHighlightingBrush(Colors.RoyalBlue)
            };

            // Define tus reglas
            var instructionRule = new HighlightingRule
            {
                Regex = new Regex(@"\b(Spawn|Color|Size|DrawLine|DrawCircle|DrawRectangle|Fill)\b"),
                Color = _namedColors["Instruction"]
            };

            var functionRule = new HighlightingRule
            {
                Regex = new Regex(@"\b(GetActualX|GetActualY|GetCanvasSize|GetColorCount|IsBrushSize|IsBrushColor|IsBrushColor|GoTo)\b"),
                Color = _namedColors["Function"]
            };


            var stringRule = new HighlightingRule
            {
                Regex = new Regex("\".*?\"|'.*?'"),
                Color = _namedColors["String"]
            };

            var numberRule = new HighlightingRule
            {
                Regex = new Regex(@"\bd+\b"),
                Color = _namedColors["Number"]
            };
            var operatorRule = new HighlightingRule
            {
                Regex = new Regex(@"\b(-|/|=|<|>|!|<-|%)\b"),
                Color = _namedColors["Number"]
            };

            MainRuleSet.Rules.Add(instructionRule);
            MainRuleSet.Rules.Add(functionRule);
            MainRuleSet.Rules.Add(stringRule);
            MainRuleSet.Rules.Add(numberRule);
            MainRuleSet.Rules.Add(operatorRule);
        }

        // Implementación de la interfaz
        public string Name => "CustomHighlighting";
        public HighlightingRuleSet MainRuleSet { get; }

        public IEnumerable<HighlightingColor> NamedHighlightingColors =>
            _namedColors.Values;

        public IDictionary<string, string> Properties =>
            new Dictionary<string, string>();

        public HighlightingColor GetNamedColor(string name)
        {
            _namedColors.TryGetValue(name, out var color);
            return color;
        }

        public HighlightingRuleSet GetNamedRuleSet(string name)
        {
            _namedRuleSets.TryGetValue(name, out var ruleSet);
            return ruleSet;
        }
    }
}