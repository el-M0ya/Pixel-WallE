namespace PixelWallEInterpreter;

public abstract class Expr
{
    public abstract T Accept<T>(IVisitor<T> visitor);
    public  class Binary : Expr
    {
       public  Expr left;
        public Expr right;
       public  Token operat;
        public Binary(Expr left , Token operat , Expr right)
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
       public  Expr right;
        public Token operat;
       public Unary(Token operat , Expr right)
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
        public Grouping (Expr expression)
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
    
}
public interface IVisitor<T> {
 T visitBinary(Expr.Binary binary); 
 T visitUnary(Expr.Unary unary);
 T visitGrouping(Expr.Grouping Grouping);
 T visitLiterals(Expr.Literal Literals);

 }

