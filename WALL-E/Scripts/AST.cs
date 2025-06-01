namespace PixelWallEInterpreter;

public abstract class Expr
{
    public abstract T Accept<T>(IVisitor<T> visitor);
    public class Assign : Expr
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
            T visitAssign(Assign expr);
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
        public class Print : Stmt
        {
            public Expr expression;
            public Print(Expr expression)
            {
                this.expression = expression;
            }
            public override void Accept(IVisitor visitor)
            {
                visitor.visitPrintStmt(this);
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
            void visitPrintStmt(Print expr);
            void visitVarStmt(Var expr);

        }
    }


