public class BasicLanguageInterpreter : BasicLanguageBaseVisitor<object>
{
    private readonly Dictionary<string, object> symbolTable = new Dictionary<string, object>();
    private readonly Dictionary<string, Func<List<object>, object>> functionTable = new Dictionary<string, Func<List<object>, object>>();
    private List<string> tokenList = new List<string>();

    public string TokenList { get; private set; } = "";
    public string GlobalVariables { get; private set; } = "";
    public string Functions { get; private set; } = "";
    public string ControlStructures { get; private set; } = "";

    
    public BasicLanguageInterpreter()
    {
        functionTable["addIntegers"] = args =>
        {
            int first = (int)args[0];
            int second = (int)args[1];
            return first + second;
        };

        functionTable["divideIntegers"] = args =>
        {
            int first = (int)args[0];
            int second = (int)args[1];
            if (second == 0)
            {
                return 0; 
            }
            return first / second;
        };
    }

    
    public override object VisitProgram(BasicLanguageParser.ProgramContext context)
    {
        foreach (var child in context.children)
        {
            string token = child.GetText();
            tokenList.Add(token);
        }

        foreach (var child in context.children)
        {
            Visit(child);
        }

        File.WriteAllLines("tokens.txt", tokenList);
        return null;
    }

    public override object VisitGlobalDeclaration(BasicLanguageParser.GlobalDeclarationContext context)
    {
        var type = context.type().GetText();
        var id = context.ID().GetText();

        
        object value = context.expr() != null ? Visit(context.expr()) : null;

        
        symbolTable[id] = value;

        if (value is double || value is float)
        {
            GlobalVariables += $"{type} {id} = {value:F2};\n"; 
        }
        else
        {
            GlobalVariables += $"{type} {id} = {value};\n";
        }

        return base.VisitGlobalDeclaration(context);
    }

    public override object VisitFunctionDecl(BasicLanguageParser.FunctionDeclContext context)
    {
        string funcName = context.ID().GetText();
        List<string> paramNames = context.parameters()?.parameter()
            .Select(p => p.ID().GetText())
            .ToList() ?? new List<string>();

        functionTable[funcName] = (args) =>
        {
            
            var localSymbols = new Dictionary<string, object>();
            for (int i = 0; i < paramNames.Count; i++)
            {
                localSymbols[paramNames[i]] = args[i];
            }

           
            Visit(context.block());

            return null;
        };

        return null;
    }

    public override object VisitExpr(BasicLanguageParser.ExprContext context)
    {
        

        if (context.literal() != null)
        {
            return Visit(context.literal());
        }

        if (context.ID() != null)
        {
            string varName = context.ID().GetText();

           
            if (symbolTable.ContainsKey(varName))
            {
                return symbolTable[varName];
            }
            else
            {
                throw new Exception($"Undefined variable: {varName}");
            }
        }

        
        if (context.children.Count == 3)
        {
            object left = Visit(context.expr(0));
            string op = context.children[1].GetText();
            object right = Visit(context.expr(1));

           
            if (left == null || right == null)
            {
                throw new Exception("Cannot perform operations with null values.");
            }

            switch (op)
            {
                case "+":
                    CheckTypeCompatibility(left, right, "int");
                    return (int)left + (int)right;

                case "-":
                    CheckTypeCompatibility(left, right, "int");
                    return (int)left - (int)right;

                case "*":
                    CheckTypeCompatibility(left, right, "int");
                    return (int)left * (int)right;

                case "/":
                    CheckTypeCompatibility(left, right, "int");
                    if ((int)right == 0)
                    {
                        throw new Exception("Division by zero.");
                    }
                    return (int)left / (int)right;

                case "%":
                    CheckTypeCompatibility(left, right, "int");
                    return (int)left % (int)right;

                case "<":
                    CheckTypeCompatibility(left, right, "int");
                    return (int)left < (int)right;

                case ">":
                    CheckTypeCompatibility(left, right, "int");
                    return (int)left > (int)right;

                case "==":
                    return left.Equals(right);

                case "!=":
                    return !left.Equals(right);
            }
        }

        
        if (context.children.Count == 3 && context.AND() != null)
        {
            object left = Visit(context.expr(0));
            object right = Visit(context.expr(1));
            CheckTypeCompatibility(left, right, "bool");
            return (bool)left && (bool)right;
        }

        if (context.children.Count == 3 && context.OR() != null)
        {
            object left = Visit(context.expr(0));
            object right = Visit(context.expr(1));
            CheckTypeCompatibility(left, right, "bool");
            return (bool)left || (bool)right;
        }

        
        if (context.children.Count == 2)
        {
            string op = context.children[0].GetText();
            object operand = Visit(context.expr(0));
            if (op == "!")
            {
                if (operand == null)
                {
                    throw new Exception("Cannot negate a null value.");
                }
                return !(bool)operand;
            }
        }

        return null;
    }

    public override object VisitIfStatement(BasicLanguageParser.IfStatementContext context)
    {
        bool condition = (bool)Visit(context.expr()); 

        if (condition)
        {
            Visit(context.statement(0)); 
        }
        else if (context.statement(1) != null)
        {
            Visit(context.statement(1)); 
        }

        return null;
    }


    public override object VisitForStatement(BasicLanguageParser.ForStatementContext context)
    {
        
        if (context.forInit() != null)
        {
            Visit(context.forInit());
        }

        while (context.forCondition() == null || (bool)Visit(context.forCondition()))
        {
            Visit(context.statement()); 
         
            if (context.forIncrement() != null)
            {
                Visit(context.forIncrement());
            }
        }

        return null;
    }

    public override object VisitWhileStatement(BasicLanguageParser.WhileStatementContext context)
    {
        while ((bool)Visit(context.expr()))
        {
            Visit(context.statement());
        }

        return null;
    }



    public override object VisitLiteral(BasicLanguageParser.LiteralContext context)
    {
        if (context.NUMBER() != null)
        {
            string text = context.NUMBER().GetText();

            if (text.Contains("."))
            {
                if (double.TryParse(text, out double doubleValue))
                {
                    return doubleValue;
                }
                else
                {
                    throw new FormatException($"Invalid float or double format: {text}");
                }
            }
            return int.Parse(text);
        }

        if (context.STRING_LITERAL() != null)
        {
            return context.STRING_LITERAL().GetText().Trim('"'); 
        }

        return null;
    }

    public override object VisitFunctionCall(BasicLanguageParser.FunctionCallContext context)
    {
        string funcName = context.ID().GetText();
        var args = context.arguments()?.expr().Select(e => Visit(e)).ToList() ?? new List<object>();

        if (!functionTable.ContainsKey(funcName))
        {
            throw new Exception($"Undefined function: {funcName}");
        }

        return functionTable[funcName](args);
    }

    public void SaveVariablesToFile()
    {
        var variableLines = symbolTable.Select(kv => $"{kv.Key} = {kv.Value}").ToList();

        Console.WriteLine("Saving variables to file:");
        foreach (var line in variableLines)
        {
            Console.WriteLine(line);
        }

        File.WriteAllLines("variables.txt", variableLines);
    }

    public void SaveFunctionsToFile()
    {
        var functionLines = functionTable.Select(kv =>
        {
            var paramList = kv.Value.Method.GetParameters()
                .Select(p => p.Name).ToList();
            return $"{kv.Key}({string.Join(", ", paramList)})";
        }).ToList();
        File.WriteAllLines("functions.txt", functionLines);
    }

    private void CheckTypeCompatibility(object left, object right, string expectedType)
    {
        if (left.GetType().Name != expectedType || right.GetType().Name != expectedType)
        {
            throw new Exception($"Incompatible types: {left.GetType().Name} and {right.GetType().Name}. Expected: {expectedType}.");
        }
    }
}
