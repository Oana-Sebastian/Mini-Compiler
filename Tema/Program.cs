﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Antlr4.Runtime;

class Program
{
    static void Main(string[] args)
    {
        string sourceFile = @"../../../program.txt";
        string sourceCode = File.ReadAllText(sourceFile);

        AntlrInputStream inputStream = new AntlrInputStream(sourceCode);
        BasicLanguageLexer lexer = new BasicLanguageLexer(inputStream);
        CommonTokenStream tokenStream = new CommonTokenStream(lexer);
        BasicLanguageParser parser = new BasicLanguageParser(tokenStream);

        lexer.RemoveErrorListeners();
        lexer.AddErrorListener(new ConsoleErrorListener<int>());
        parser.RemoveErrorListeners();
        parser.AddErrorListener(new ConsoleErrorListener<IToken>());

        var tree = parser.program();

        string outputDirectory = @"../../../output/";
        Directory.CreateDirectory(outputDirectory); 
        File.WriteAllText(Path.Combine(outputDirectory, "control_structures.txt"), string.Empty);
        File.WriteAllText(Path.Combine(outputDirectory, "functions.txt"), string.Empty);
        File.WriteAllText(Path.Combine(outputDirectory, "variables.txt"), string.Empty);
        File.WriteAllText(Path.Combine(outputDirectory, "tokens.txt"), string.Empty);
        var visitor = new BasicLanguageVisitor(outputDirectory);
        visitor.Visit(tree);

        File.WriteAllText(Path.Combine(outputDirectory, "errors.txt"), string.Join(Environment.NewLine, visitor.Errors));


        Console.WriteLine("Process complete. Errors have been saved in 'errors.txt'.");
    }
}

public class BasicLanguageVisitor : BasicLanguageBaseVisitor<object>
{
    private readonly string _outputDirectory;
    public List<string> Errors { get; private set; } = new List<string>();

    private readonly Dictionary<string, string> _variableTypes = new Dictionary<string, string>();
    private readonly Dictionary<string, string> _functionReturnTypes = new Dictionary<string, string>();
    public BasicLanguageVisitor(string outputDirectory)
    {
        _outputDirectory = outputDirectory;
    }

    private void WriteToFile(string fileName, string content)
    {
        File.AppendAllText(Path.Combine(_outputDirectory, fileName), content + Environment.NewLine);
    }

    public override object VisitDeclaration(BasicLanguageParser.DeclarationContext context)
    {
        var type = context.type().GetText();
        var id = context.ID().GetText();
        var value = context.expr()?.GetText();

        if (!_variableTypes.ContainsKey(id))
            _variableTypes[id] = type;
        else
            Errors.Add($"Error: Variable '{id}' has already been declared.");

        if (value != null)
        {
            string inferredType = InferType(context.expr());
            if (inferredType != type)
            {
                Errors.Add($"Error: Variable '{id}' is declared as '{type}', but it is given a '{inferredType}' value ({value}).");
            }
        }

        WriteToFile("variables.txt", id);
        return base.VisitDeclaration(context);
    }

    private string InferType(BasicLanguageParser.ExprContext context)
    {
        if (context == null) return "unknown";

        if (context.literal() != null)
        {
            var literalText = context.literal().GetText();
            if (int.TryParse(literalText, out _)) return "int";
            if (float.TryParse(literalText, out _)) return "float";
            if (double.TryParse(literalText, out _)) return "double";
            if (literalText.StartsWith("\"") && literalText.EndsWith("\"")) return "string";
        }

        if (context.ID() != null)
        {
            var id = context.ID().GetText();
            return _variableTypes.ContainsKey(id) ? _variableTypes[id] : "unknown";
        }

        if (context.functionCall() != null)
        {
            var functionName = context.functionCall().ID().GetText();
            return _functionReturnTypes.ContainsKey(functionName) ? _functionReturnTypes[functionName] : "unknown";
        }

        if (context.expr().Length == 2)
        {
            var leftType = InferType(context.expr(0));
            var rightType = InferType(context.expr(1));

            if (leftType == rightType) return leftType;

            if ((leftType == "int" && rightType == "float") || (leftType == "float" && rightType == "int"))
                return "float";
            if ((leftType == "float" && rightType == "double") || (leftType == "double" && rightType == "float"))
                return "double";
        }

        return "unknown";
    }

    public override object VisitFunctionDecl(BasicLanguageParser.FunctionDeclContext context)
    {
        var returnType = context.type().GetText();
        var functionName = context.ID().GetText();

        _functionReturnTypes[functionName] = returnType;

        var parameters = context.parameters()?.parameter()
            .Select(p => $"{p.type().GetText()} {p.ID().GetText()}")
            .ToList() ?? new List<string>();

        var parameterString = string.Join(", ", parameters);
        WriteToFile("functions.txt", $"{returnType} {functionName}({parameterString});");

        return base.VisitFunctionDecl(context);
    }


    public override object VisitIfStatement(BasicLanguageParser.IfStatementContext context)
    {
        var line = context.Start.Line;
        WriteToFile("control_structures.txt", $"if-statement on line {line}");
        return base.VisitIfStatement(context);
    }

    public override object VisitForStatement(BasicLanguageParser.ForStatementContext context)
    {
        var line = context.Start.Line;
        WriteToFile("control_structures.txt", $"for-statement on line {line}");
        return base.VisitForStatement(context);
    }

    public override object VisitWhileStatement(BasicLanguageParser.WhileStatementContext context)
    {
        var line = context.Start.Line;
        WriteToFile("control_structures.txt", $"while-statement on line {line}");
        return base.VisitWhileStatement(context);
    }


    public override object VisitProgram(BasicLanguageParser.ProgramContext context)
    {
        var tokens = string.Join(Environment.NewLine, context.children?.Select(c => c.GetText()) ?? new string[0]);
        WriteToFile("tokens.txt", tokens);
        return base.VisitProgram(context);
    }
}
