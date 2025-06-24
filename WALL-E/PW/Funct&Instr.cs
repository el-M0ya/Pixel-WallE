namespace PW;

using System.Threading;
using Avalonia.Media;
using System.Collections.Generic;
using System.Threading.Tasks;
public class Func
{
    //Instructions
    public static void Spawn(int x, int y)
    {
        Wall_E.MoveWalle(x, y);
    }

    public static void Color(string color)
    {
        Wall_E.Instance.currentColor = color;
    }
    public static void Size(int size)
    {
        Wall_E.Instance.brushSize = size;
    }

    public static async Task DrawLine(int dirX, int dirY, int distance, CancellationToken cancellationToken = default)
    {
        await DrawLine(Wall_E.Instance.x, Wall_E.Instance.y, dirX, dirY, distance, cancellationToken);
    }
    public static async Task DrawLine(int currentX, int currentY, int dirX, int dirY, int distance, CancellationToken cancellationToken = default)
    {
        if (dirX < -1 || dirX > 1 || dirY < -1 || dirY > 1)
        {
            MainWindow.SetStatus("Parameters out of range: directions are between -1 and 1", true);
            return;
        }


        for (int i = 0; i < distance; i++)
        {
            await MainWindow.Paint(currentX, currentY, Wall_E.Instance.currentColor, Wall_E.Instance.brushSize, cancellationToken);
            currentX += dirX;
            currentY += dirY;
            Wall_E.MoveWalle(currentX, currentY);
        }
    }

    // 1. Algoritmo del Punto Medio (Midpoint)
    public static async Task DrawCircle(int dirX, int dirY, int radius, CancellationToken cancellationToken = default)
    {


        if (dirX == 0 && dirY == 0) await DrawCircle(radius, cancellationToken);
        else
        {
            Wall_E.MoveWalle(Wall_E.Instance.x + dirX * radius, Wall_E.Instance.y + dirY * radius);
            await DrawCircle(radius, cancellationToken);
        }
    }
    public static async Task DrawCircle(int radius, CancellationToken cancellationToken = default)
    {
        if (radius == 0)
        {
            await MainWindow.Paint(Wall_E.Instance.x, Wall_E.Instance.y, cancellationToken);
            return;
        }
        if (radius <= 0)
        {
            await DrawCircle(-radius, cancellationToken);
            return;
        }

        int xc = Wall_E.Instance.x;
        int yc = Wall_E.Instance.y;
        int x = 0;
        int y = radius;
        int d = 1 - radius;

        await PaintCirclePoints(xc, yc, x, y, cancellationToken);

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
            await PaintCirclePoints(xc, yc, x, y, cancellationToken);
        }
    }

    private static async Task PaintCirclePoints(int xc, int yc, int x, int y, CancellationToken cancellationToken = default)
    {
        await MainWindow.Paint(xc + x, yc + y, cancellationToken);
        await MainWindow.Paint(xc - x, yc + y, cancellationToken);
        await MainWindow.Paint(xc + x, yc - y, cancellationToken);
        await MainWindow.Paint(xc - x, yc - y, cancellationToken);
        await MainWindow.Paint(xc + y, yc + x, cancellationToken);
        await MainWindow.Paint(xc - y, yc + x, cancellationToken);
        await MainWindow.Paint(xc + y, yc - x, cancellationToken);
        await MainWindow.Paint(xc - y, yc - x, cancellationToken);
    }
    public static async Task DrawRectangle(int dirx, int diry, int distance, int width, int height, CancellationToken cancellationToken = default)
    {
        if ((dirx == 0 && diry == 0) || distance == 0) await DrawRectangle(width, height, cancellationToken);
        if (width == 0 || height == 0) return;
        else
        {
            Wall_E.MoveWalle(Wall_E.Instance.x + dirx * distance, Wall_E.Instance.y + diry * distance);
            await DrawRectangle(width, height, cancellationToken);
        }
    }
    public static async Task DrawRectangle(int width, int height, CancellationToken cancellationToken = default)
    {
        int wallex = Wall_E.Instance.x;
        int walley = Wall_E.Instance.y;

        if (width == 1 && height == 1) await MainWindow.Paint(wallex, walley, cancellationToken);

        int x1 = wallex - (width - 1);
        int x2 = wallex + (width - 1);
        int y1 = walley - (height - 1);
        int y2 = walley + (height - 1);

        //(x1 , y1) to (x2 , y1)
        await DrawLine(x1, y1, 1, 0, x2 - x1, cancellationToken);
        //(x2 , y1) to (x2 , y2)
        await DrawLine(x2, y1, 0, 1, y2 - y1, cancellationToken);
        //(x2 , y2) to (x1 , y2)
        await DrawLine(x2, y2, -1, 0, x2 - x1, cancellationToken);
        //(x1 , y2) to (x1 , y1)
        await DrawLine(x1, y2, 0, -1, y2 - y1, cancellationToken);

        Wall_E.MoveWalle(wallex, walley);
    }
    public static async Task Fill(CancellationToken cancellationToken = default)
    {
        Wall_E.Instance.isFilling = true;
        int x = Wall_E.Instance.x;
        int y = Wall_E.Instance.y;
        Color targetColor = MainWindow.GetPixelColorFromCanvas(x, y);
        Color newColor = MainWindow._colorNameMap[Wall_E.Instance.currentColor];

        if (targetColor == newColor)
        {
            Wall_E.Instance.isFilling = false;
            return;
        }

        if (!MainWindow._colorNameMap.ContainsKey(Wall_E.Instance.currentColor))
        {
            MainWindow.SetStatus("Invalid Color", true);
            Wall_E.Instance.isFilling = false;
            return;
        }

        Stack<(int, int)> pixels = new Stack<(int, int)>();
        pixels.Push((x, y));
        // Save if is visited position
        HashSet<(int, int)> visited = new HashSet<(int, int)>();

        while (pixels.Count > 0)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var (currentX, currentY) = pixels.Pop();

            if (visited.Contains((currentX, currentY))) continue;

            visited.Add((currentX, currentY));

            if (!MainWindow.IsInCanvas(currentX, currentY) ||
                 MainWindow.GetPixelColorFromCanvas(currentX, currentY) != targetColor)
                    continue;

            await MainWindow.Paint(currentX, currentY, Wall_E.Instance.currentColor,
                       Wall_E.Instance.brushSize, cancellationToken);

            pixels.Push((currentX + 1, currentY));
            pixels.Push((currentX - 1, currentY));
            pixels.Push((currentX, currentY + 1));
            pixels.Push((currentX, currentY - 1));
        }
        Wall_E.Instance.isFilling = false;
    }

    // Functions
    public static int GetActualX() => Wall_E.Instance.x;
    public static int GetActualY() => Wall_E.Instance.y;
    public static int GetCanvasSize() => MainWindow._pixelCanvas.CanvasDimension;
    public static int GetColorCount(string color, int x1, int y1, int x2, int y2)
    {
        if (x1 > x2) return GetColorCount(color, x2, y1, x1, y2);
        if (y1 > y2) return GetColorCount(color, x1, y2, x2, y1);
        if (x1 < 0 || y1 < 0 || x2 >= MainWindow._pixelCanvas.CanvasDimension || y2 >= MainWindow._pixelCanvas.CanvasDimension) return 0;

        int count = 0;
        for (int i = x1; i <= x2; i++)
        {
            for (int j = y1; j <= y2; j++)
            {
                if (MainWindow.GetPixelColorFromCanvas(i, j) == MainWindow._colorNameMap[color]) count++;
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

        return MainWindow.GetPixelColorFromCanvas(Wall_E.Instance.x + vertical,
                                       Wall_E.Instance.y + horizontal) == MainWindow._colorNameMap[color] ? 1 : 0;
    }

}