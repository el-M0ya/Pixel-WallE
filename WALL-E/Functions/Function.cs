class Function
{
    public static int GetActualX()
    {
        throw new NotImplementedException();
    }
    public static int GetActualY()
    {
        throw new NotImplementedException();
    }
    public static int GetCanvasSize()
    {
        throw new NotImplementedException();
    }
    public static int GetColorCount(string color, int x1, int y1, int x2, int y2)
    {
        if (x1 > x2) return GetColorCount(color, x2, y1, x1, y2);
        if (y1 > y2) return GetColorCount(color, x1, y2, x2, y1);

        int count = 0;
        for (int i = x1; i <= x2; i++)
        {
            for (int j = y1; j <= y2; j++)
            {
                // Change this
                if (true) count++;
            }
        }
        return count;
    }
    public static int IsBrushColor(string color)
    {
        // Change this
        return color == "this" ? 1 : 0;
    }
    public static int IsBrushSize(int size)
    {
        // Change this
        return size == 0 ? 1 : 0;
    }
    public static int IsCanvasColor(string color, int vertical, int horizontal)
    {
        // Change true
        return /*tiles.color(wally.x + vertical) == color &&
                 tiles.color(wally.y + horizontal) == color*/ true ? 1 : 0;
    }

    public static string GetColor()
    {
        return GetColor(/*Walle.x , walle.y*/);
    }
    public static string GetColor(int x , int y)
    {
        return "";
    }




    public static void Spawn(int x, int y)
    {
        throw new NotImplementedException();
    }
    public static void Color(string color)
    {
        throw new NotImplementedException();
    }

    public static void Size(int k)
    {
        throw new NotImplementedException();
    }

    public static void DrawLine(int dirx, int diry, int distance)
    {
        if (dirx < -1 || dirx > 1 || diry < -1 || diry > 1) throw new Exception("Parameters out of range: directions are between -1 and 1");
        if (distance < 0) DrawLine(-dirx, -diry, -distance);

        //Paint(walle.x , walle.y , color , size);

        while (distance > 0)
        {
            //MoveWalle(dirx , diry)
            // Paint(walle.x , walle.y , color , size);
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
        string color = "";// GetColor(walle.x , walle.y);
        // Change 0s for walle.x , walle.y
        if(color != GetColor())  Fill(0 , 0 , color);
            
    }
    private static void Fill(int x, int y, string color)
    {
        if (color != GetColor(x, y)) return;

        Fill(x + 1 , y , color);
        Fill(x - 1 , y , color);
        Fill(x , y + 1 , color);
        Fill(x , y - 1 , color);
    }
    
    
}