namespace PW;

using System;

public abstract class Expr
{
    public abstract T Accept<T>(IVisitor<T> visitor);
    public  class Assign : Expr
    {
           public Expr value;
            public Token name;
            public Assign(Token name, Expr value)
            {
                this.name = name;
                this.value = value;
            }
            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.visitAssign(this);
            }
        }
    public class Binary : Expr
    {
        public Expr left;
        public Expr right;
        public Token operat;
        public Binary(Expr left, Token operat, Expr right)
        {
            this.left = left;
            this.operat = operat;
            this.right = right;
        }
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.visitBinary(this);
        }
    }
    public class Unary : Expr
    {
        public Expr right;
        public Token operat;
        public Unary(Token operat, Expr right)
        {
            this.operat = operat;
            this.right = right;
        }
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.visitUnary(this);
        }
    }
    public class Grouping : Expr
    {
        public Expr expression;
        public Grouping(Expr expression)
        {
            this.expression = expression;
        }
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.visitGrouping(this);
        }
    }
    public class Literal : Expr
    {
        public object value;
        public Literal(object value)
        {
            this.value = value;
        }
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.visitLiterals(this);
        }
    }
    public class Logical : Expr
    {
        public Expr left;
        public Expr right;
        public Token operat;
        public Logical(Expr left, Token operat, Expr right)
        {
            this.left = left;
            this.operat = operat;
            this.right = right;
        }
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.visitLogical(this);
        }
    }
    public class Var : Expr
    {
        public Token name;
        public Var(Token name)
        {
            this.name = name;
        }
        public override T Accept<T>(IVisitor<T> visitor)
        {
            return visitor.visitVar(this);
        }
    }
    // public class GetActualX : Expr
    // {
    //     public Token name;
    //     public GetActualX(Token name)
    //     {
    //         this.name = name;
    //     }
    //     public override T Accept<T>(IVisitor<T> visitor)
    //     {
    //         return visitor.visitGetActualX(this);
    //     }
    // }
    // public class GetActualY : Expr
    // {
    //     public Token name;
    //     public GetActualY(Token name)
    //     {
    //         this.name = name;
    //     }
    //     public override T Accept<T>(IVisitor<T> visitor)
    //     {
    //         return visitor.visitGetActualY(this);
    //     }
    // }
    // public class GetCanvasSize : Expr
    // {
    //     public Token name;
    //     public GetCanvasSize(Token name)
    //     {
    //         this.name = name;
    //     }
    //     public override T Accept<T>(IVisitor<T> visitor)
    //     {
    //         return visitor.visitGetCanvasSize(this);
    //     }
    // }
    // public class GetColorCount : Expr
    // {
    //     public Token name;
    //     public GetColorCount(Token name)
    //     {
    //         this.name = name;
    //     }
    //     public override T Accept<T>(IVisitor<T> visitor)
    //     {
    //         return visitor.visitGetColorCount(this);
    //     }
    // }
    // public class IsBrushColor : Expr
    // {
    //     public Token name;
    //     public IsBrushColor(Token name)
    //     {
    //         this.name = name;
    //     }
    //     public override T Accept<T>(IVisitor<T> visitor)
    //     {
    //         return visitor.visitIsBrushColor(this);
    //     }
    // }
    // public class IsBrushSize : Expr
    // {
    //     public Token name;
    //     public IsBrushSize(Token name)
    //     {
    //         this.name = name;
    //     }
    //     public override T Accept<T>(IVisitor<T> visitor)
    //     {
    //         return visitor.visitIsBrushSize(this);
    //     }
    // }
    // public class IsCanvasColor : Expr
    // {
    //     public Token name;
    //     public IsCanvasColor(Token name)
    //     {
    //         this.name = name;
    //     }
    //     public override T Accept<T>(IVisitor<T> visitor)
    //     {
    //         return visitor.visitIsCanvasColor(this);
    //     }
    // }
    public interface IVisitor<T>
    {
        T visitBinary(Binary binary);
        T visitUnary(Unary unary);
        T visitGrouping(Grouping Grouping);
        T visitLiterals(Literal Literals);
        T visitVar(Var expr);
        T visitAssign(Assign expr);
        T visitLogical(Logical expr);
        // T visitGetActualX(GetActualX expr);
        // T visitGetActualY(GetActualY expr);
        // T visitGetCanvasSize(GetCanvasSize expr);
        // T visitGetColorCount(GetColorCount expr);
        // T visitIsBrushColor(IsBrushColor expr);
        // T visitIsBrushSize(IsBrushSize expr);
        // T visitIsCanvasColor(IsCanvasColor expr);
    }


}
public abstract class Stmt
{
    public abstract void Accept(IVisitor visitor);
    public class Expression : Stmt
    {
        public Expr expression;
        public Expression(Expr expression)
        {
            this.expression = expression;
        }
        public override void Accept(IVisitor visitor)
        {
            visitor.visitExprStmt(this);
        }

    }
    public class Spawn : Stmt
    {
        public Expr x;
        public Expr y;
        public Spawn(Expr x, Expr y)
        {
            this.x = x;
            this.y = y;
        }
        public override void Accept(IVisitor visitor)
        {
            visitor.visitSpawnStmt(this);
        }
    }
    public class Color : Stmt
    {
        public Expr color;

        public Color(Expr color)
        {
            this.color = color;

        }
        public override void Accept(IVisitor visitor)
        {
            visitor.visitColorStmt(this);
        }
    }
    public class Size : Stmt
    {
        public Expr size;

        public Size(Expr size)
        {
            this.size = size;
        }
        public override void Accept(IVisitor visitor)
        {
            visitor.visitSizeStmt(this);
        }
    }
    public class DrawLine : Stmt
    {
        public Expr dirX;
        public Expr dirY;
        public Expr distance;

        public DrawLine(Expr dirX, Expr dirY, Expr distance)
        {
            this.dirX = dirX;
            this.dirY = dirY;
            this.distance = distance;
        }
        public override void Accept(IVisitor visitor)
        {
            visitor.visitDrawLineStmt(this);
        }
    }
    public class DrawCircle : Stmt
    {
        public Expr dirX;
        public Expr dirY;
        public Expr radius;

        public DrawCircle(Expr dirX, Expr dirY, Expr radius)
        {
            this.dirX = dirX;
            this.dirY = dirY;
            this.radius = radius;

        }
        public override void Accept(IVisitor visitor)
        {
            visitor.visitDrawCircleStmt(this);
        }
    }
    public class DrawRectangle : Stmt
    {
        public Expr dirX;
        public Expr dirY;
        public Expr distance;
        public Expr width;
        public Expr height;

        public DrawRectangle(Expr dirX, Expr dirY, Expr distance, Expr width, Expr height)
        {
            this.dirX = dirX;
            this.dirY = dirY;
            this.distance = distance;
            this.width = width;
            this.height = height;

        }
        public override void Accept(IVisitor visitor)
        {
            visitor.visitDrawRectangleStmt(this);
        }
    }
    public class Fill : Stmt
    {
        public int X;
        public int Y;
      

        public Fill(int X, int Y)
        {
            this.X = X;
            this.Y = Y;
        }
        public override void Accept(IVisitor visitor)
        {
            visitor.visitFillStmt(this);
        }
    }


    public class GoTo : Stmt
    {
        public Token label;
        public Expr condition;
        public GoTo(Token label, Expr condition)
        {
            this.label = label;
            this.condition = condition;
        }
        public override void Accept(IVisitor visitor)
        {
            visitor.visitGoToStmt(this);
        }

    }
    public class Var : Stmt
    {
        public Token name;
        public Expr initializer;
        public Var(Token name, Expr initializer)
        {
            this.name = name;
            this.initializer = initializer;
        }
        public override void Accept(IVisitor visitor)
        {
            visitor.visitVarStmt(this);
        }

    }
       public class Label : Stmt
    {
        public Token label;
        public Label(Token label)
        {
            this.label = label;
        }
        public override void Accept(IVisitor visitor)
        {
            visitor.visitLabelStmt(this);
        }

    }
    public interface IVisitor
    {
        void visitExprStmt(Expression expr);
        void visitSpawnStmt(Spawn expr);
        void visitColorStmt(Color expr);
        void visitSizeStmt(Size expr);
        void visitDrawLineStmt(DrawLine expr);
        void visitDrawCircleStmt(DrawCircle expr);
        void visitDrawRectangleStmt(DrawRectangle expr);
        void visitFillStmt(Fill expr);
        void visitVarStmt(Var expr);
        void visitLabelStmt(Label expr);
        void visitGoToStmt(GoTo expr);

    }
}


