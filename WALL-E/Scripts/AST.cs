namespace PixelWallEInterpreter;

public abstract class Expr
{
    public abstract T Accept<T>(IVisitor<T> visitor);
    public abstract class Assign : Expr
    {
        public class Variable : Assign
        {
            public Expr value;
            public Token name;
            public Variable(Token name, Expr value)
            {
                this.name = name;
                this.value = value;
            }
            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.visitAssignVar(this);
            }
        }
        public class Label : Assign
        {
            public Expr value;
            public Token name;
            public Label(Token name, Expr value)
            {
                this.name = name;
                this.value = value;
            }
            public override T Accept<T>(IVisitor<T> visitor)
            {
                return visitor.visitAssignLabel(this);
            }
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
    public interface IVisitor<T>
    {
        T visitBinary(Binary binary);
        T visitUnary(Unary unary);
        T visitGrouping(Grouping Grouping);
        T visitLiterals(Literal Literals);
        T visitVar(Var expr);
        T visitAssignVar(Assign.Variable expr);
        T visitAssignLabel(Assign.Label expr);
        T visitLogical(Logical expr);
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
        public int x;
        public int y;
        public Spawn(int x, int y)
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
        public string color;

        public Color(string color)
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
        public int size;

        public Size(int size)
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
        public int dirX;
        public int dirY;
        public int distance;

        public DrawLine(int dirX, int dirY, int distance)
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
        public int dirX;
        public int dirY;
        public int radius;

        public DrawCircle(int dirX, int dirY, int radius)
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
        public int dirX;
        public int dirY;
        public int distance;
        public int width;
        public int height;

        public DrawRectangle(int dirX, int dirY, int distance, int width, int height)
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
        void visitGoToStmt(GoTo expr);

    }
}


