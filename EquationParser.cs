using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Calcuhandy {
    public static class EquationParser {
        public static Token inverseToken = new Token("inverse", Token.Type.Operator, 1, priority:4);
        /// <summary>
        /// Tokens higher up the list will match first, so atan2 needs to be before atan, etc.
        /// </summary>
        public static Token[] globalTokens = {
            // Operators //
            new Token("+", Token.Type.Operator, 2, priority:1),
            new Token("-", Token.Type.Operator, 2, priority:1),
            new Token("*", Token.Type.Operator, 2, priority:2),
            new Token("/", Token.Type.Operator, 2, priority:2),
            new Token("%", Token.Type.Operator, 2, priority:2),
            new Token("^", Token.Type.Operator, 2, priority:3, leftAssociative:false),
            inverseToken,
            // Special //
            new Token("(", Token.Type.Special),
            new Token(")", Token.Type.Special),
            new Token(",", Token.Type.Special),
            new Token("=", Token.Type.Special),
            // Functions //
            new Token("abs", Token.Type.Function, 1),
            new Token("acosh", Token.Type.Function, 1),
            new Token("acos", Token.Type.Function, 1),
            new Token("asinh", Token.Type.Function, 1),
            new Token("asin", Token.Type.Function, 1),
            new Token("atan2", Token.Type.Function, 2),
            new Token("atanh", Token.Type.Function, 1),
            new Token("atan", Token.Type.Function, 1),
            new Token("cbrt", Token.Type.Function, 1),
            new Token("ceil", Token.Type.Function, 1),
            new Token("clamp", Token.Type.Function, 3),
            new Token("cosh", Token.Type.Function, 1),
            new Token("cos", Token.Type.Function, 1),
            new Token("floor", Token.Type.Function, 1),

            new Token("log10e", Token.Type.Constant, constantValue:Math.Log10(Math.E)),
            new Token("log2e", Token.Type.Constant, constantValue:Math.Log2(Math.E)),

            new Token("log10", Token.Type.Function, 1),
            new Token("log2", Token.Type.Function, 1),
            new Token("log", Token.Type.Function, 1),
            new Token("max", Token.Type.Function, 2),
            new Token("min", Token.Type.Function, 2),
            new Token("mod", Token.Type.Function, 2),
            new Token("round", Token.Type.Function, 1),
            new Token("sign", Token.Type.Function, 1),
            new Token("sinh", Token.Type.Function, 1),
            new Token("sin", Token.Type.Function, 1),
            new Token("sqrt", Token.Type.Function, 1),
            new Token("tanh", Token.Type.Function, 1),
            new Token("tan", Token.Type.Function, 1),
            new Token("trunc", Token.Type.Function, 1),
            // Constants //
            new Token("deg2rad", Token.Type.Constant, constantValue:Math.Tau/360.0),
            new Token("rad2deg", Token.Type.Constant, constantValue:360.0/Math.Tau),
            new Token("pi", Token.Type.Constant, constantValue:Math.PI),
            new Token("tau", Token.Type.Constant, constantValue:Math.Tau),
            new Token("phi", Token.Type.Constant, constantValue:(1+Math.Sqrt(5))/2),
            new Token("e", Token.Type.Constant, constantValue:Math.E),
            new Token("ln10", Token.Type.Constant, constantValue:Math.Log(10.0)),
            new Token("ln2", Token.Type.Constant, constantValue:Math.Log(2.0)),
            new Token("nan", Token.Type.Constant, constantValue:double.NaN),
            new Token("infinity", Token.Type.Constant, constantValue:double.PositiveInfinity),
        };

        static Regex whitespaceRegex = new Regex("\\s");
        static Queue<String> errorQueue = new();
        public static string ParseText(string? input) {
            try {
                string result = ParseToDouble(input).ToString();
                if(input == null || input == "") result = "";
                //return errorQueue.Count > 0 ? errorQueue.Peek() : string.Join(" ", elements);
                return errorQueue.Count > 0 ? errorQueue.Peek() : result;
            } catch(Exception e) {
                return $"Program Error: {e}";
            }
        }
        public static double ParseToDouble(string? input) {
            errorQueue.Clear();
            if(input == null) return double.NaN;

            List<Token> elements = Tokenize(input.ToLower());
            SyntaxCleaner(ref elements);
            Postfix(ref elements);
            return Evaluate(elements);
        }
        private static List<Token> Tokenize(string input) {
            input = whitespaceRegex.Replace(input, "");
            List<Token> elements = new();

            string numberStack = "";
            Token elementStack = Token.empty;
            for(int i = 0; i < input.Length; i++) {
                if("0123456789.".Contains(input[i])) {
                    numberStack += input[i];
                } else {
                    if(numberStack.Length > 0) {
                        elements.Add(new Token(numberStack, Token.Type.Number));
                        numberStack = "";
                    }
                    elementStack = MatchPrefix(input.Substring(i), globalTokens);
                    if(elementStack != Token.empty) {
                        elements.Add(elementStack);
                        i += elementStack.Length - 1;
                    } else{
                        FlagError($"Unknown at {i}, {input[i]}");
                    }
                }
            }
            if(numberStack.Length > 0) elements.Add(new Token(numberStack, Token.Type.Number));
            return elements;
        }
        private static void SyntaxCleaner(ref List<Token> elements) {
            // Ensure parenthesis continuity
            int appendCount = 0;
            int prependCount = 0;
            for(int i = 0; i < elements.Count; i++) {
                if(elements[i].value == "(") appendCount++;
                else if(elements[i].value == ")") {
                    if(appendCount == 0) {
                        prependCount++;
                    } else {
                        appendCount--;
                    }
                } else if(elements[i].value == "-" && (i == 0 || elements[i-1].type != Token.Type.Number && elements[i-1].type != Token.Type.Constant && elements[i-1].value != ")")) {
                    elements[i] = inverseToken;
                }
            }
            for(int i = 0; i < prependCount; i++) {
                elements.Insert(0, new Token("(", Token.Type.Special));
            }
            for(int i = 0; i < appendCount; i++) {
                elements.Add(new Token(")", Token.Type.Special));
            }
        }
        private static void Postfix(ref List<Token> elements) {
            List<Token> outputQueue = new();
            Stack<Token> operatorStack = new();
            for(int i=0; i<elements.Count; i++) {
                switch(elements[i].type) {
                    case Token.Type.Number:
                    case Token.Type.Constant:
                        outputQueue.Add(elements[i]);
                        break;
                    case Token.Type.Function:
                        operatorStack.Push(elements[i]);
                        break;
                    case Token.Type.Operator:
                        while(operatorStack.Count > 0 && operatorStack.Peek().value != "(" && (operatorStack.Peek().priority > elements[i].priority || (operatorStack.Peek().priority == elements[i].priority && elements[i].leftAssociative))) {
                            outputQueue.Add(operatorStack.Pop());
                        }
                        operatorStack.Push(elements[i]);
                        break;
                    default:
                        switch(elements[i].value) {
                            case ",":
                                while(operatorStack.Count > 0 && operatorStack.Peek().value != "(") {
                                    outputQueue.Add(operatorStack.Pop());
                                }
                                break;
                            case "(":
                                operatorStack.Push(elements[i]);
                                break;
                            case ")":
                                while(operatorStack.Count > 0 && operatorStack.Peek().value != "(") {
                                    outputQueue.Add(operatorStack.Pop());
                                }
                                if(operatorStack.Count > 0) operatorStack.Pop();
                                if(operatorStack.Count > 0 && operatorStack.Peek().type == Token.Type.Function) {
                                    outputQueue.Add(operatorStack.Pop());
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                }
            }
            while(operatorStack.Count() > 0) {
                outputQueue.Add(operatorStack.Pop());
            }
            elements = outputQueue;
        }
        private static double Evaluate(List<Token> elements) {
            if(elements.Count == 0) return 0.0;

            Stack<double> mainStack = new();
            Stack<double> argStack = new();
            for(int i = 0; i < elements.Count; i++) {
                if(elements[i].argCount > mainStack.Count) {
                    FlagError($"Not enough Arguments for {elements[i].value}");
                    break;
                }
                //Flip the order of the args
                for(int j = 0; j < elements[i].argCount; j++) argStack.Push(mainStack.Pop());
                switch(elements[i].type) {
                    case Token.Type.Number:
                        double numParseResult;
                        if(double.TryParse(elements[i].value, out numParseResult)) mainStack.Push(numParseResult);
                        else FlagError($"Invalid number {elements[i].value}");
                        break;
                    case Token.Type.Constant:
                        if(elements[i].constantValue != 0.0) mainStack.Push(elements[i].constantValue);
                        else FlagError($"Unimplemented const {elements[i].value}");
                        break;
                    case Token.Type.Operator:
                        switch(elements[i].value) {
                            case "+":
                                mainStack.Push(argStack.Pop() + argStack.Pop());
                                break;
                            case "-":
                                mainStack.Push(argStack.Pop() - argStack.Pop());
                                break;
                            case "*":
                                mainStack.Push(argStack.Pop() * argStack.Pop());
                                break;
                            case "/":
                                mainStack.Push(argStack.Pop() / argStack.Pop());
                                break;
                            case "%":
                                mainStack.Push(argStack.Pop() % argStack.Pop());
                                break;
                            case "^":
                                mainStack.Push(Math.Pow(argStack.Pop(), argStack.Pop()));
                                break;
                            case "inverse":
                                mainStack.Push(-argStack.Pop());
                                break;
                        }
                        break;
                    case Token.Type.Function:
                        switch(elements[i].value) {
                            case "abs":
                                mainStack.Push(Math.Abs(argStack.Pop()));
                                break;
                            case "acosh":
                                mainStack.Push(Math.Acosh(argStack.Pop()));
                                break;
                            case "acos":
                                mainStack.Push(Math.Acos(argStack.Pop()));
                                break;
                            case "asinh":
                                mainStack.Push(Math.Asinh(argStack.Pop()));
                                break;
                            case "asin":
                                mainStack.Push(Math.Asin(argStack.Pop()));
                                break;
                            case "atan2":
                                mainStack.Push(Math.Atan2(argStack.Pop(), argStack.Pop()));
                                break;
                            case "atanh":
                                mainStack.Push(Math.Atanh(argStack.Pop()));
                                break;
                            case "atan":
                                mainStack.Push(Math.Atan(argStack.Pop()));
                                break;
                            case "cbrt":
                                mainStack.Push(Math.Cbrt(argStack.Pop()));
                                break;
                            case "ceil":
                                mainStack.Push(Math.Ceiling(argStack.Pop()));
                                break;
                            case "clamp":
                                mainStack.Push(Math.Clamp(argStack.Pop(), argStack.Pop(), argStack.Pop()));
                                break;
                            case "cosh":
                                mainStack.Push(Math.Cosh(argStack.Pop()));
                                break;
                            case "cos":
                                mainStack.Push(Math.Cos(argStack.Pop()));
                                break;
                            case "floor":
                                mainStack.Push(Math.Floor(argStack.Pop()));
                                break;
                            case "log10":
                                mainStack.Push(Math.Log10(argStack.Pop()));
                                break;
                            case "log2":
                                mainStack.Push(Math.Log2(argStack.Pop()));
                                break;
                            case "log":
                                mainStack.Push(Math.Log(argStack.Pop()));
                                break;
                            case "max":
                                mainStack.Push(Math.Max(argStack.Pop(), argStack.Pop()));
                                break;
                            case "min":
                                mainStack.Push(Math.Min(argStack.Pop(), argStack.Pop()));
                                break;
                            case "mod":
                                double a = argStack.Pop();
                                double b = argStack.Pop();
                                if(b == 0) mainStack.Push(double.NaN);
                                else mainStack.Push(a - b * Math.Floor(a / b));
                                break;
                            case "round":
                                mainStack.Push(Math.Round(argStack.Pop()));
                                break;
                            case "sign":
                                mainStack.Push(Math.Sign(argStack.Pop()));
                                break;
                            case "sinh":
                                mainStack.Push(Math.Sinh(argStack.Pop()));
                                break;
                            case "sin":
                                mainStack.Push(Math.Sin(argStack.Pop()));
                                break;
                            case "sqrt":
                                mainStack.Push(Math.Sqrt(argStack.Pop()));
                                break;
                            case "tanh":
                                mainStack.Push(Math.Tanh(argStack.Pop()));
                                break;
                            case "tan":
                                mainStack.Push(Math.Tan(argStack.Pop()));
                                break;
                            case "trunc":
                                mainStack.Push(Math.Truncate(argStack.Pop()));
                                break;
                        }
                        break;
                }
            }
            if(mainStack.Count == 0) {
                FlagError("Nothing left in stack");
                return 0.0;
            }
            double result = mainStack.Pop();
            if(Math.Abs(result) < 0.00000000000001) return 0.0;
            return result;
        }
        private static string MatchPrefix(string input, string[] value) {
            for(int i=0; i<value.Length; i++) {
                if(input.StartsWith(value[i])) return value[i];
            }
            return "";
        }
        private static Token MatchPrefix(string input, Token[] tokens) {
            for(int i=0; i<tokens.Length; i++) {
                if(input.StartsWith(tokens[i].value)) return tokens[i];
            }
            return Token.empty;
        }
        private static void FlagError(string extraInfo) {
            errorQueue.Enqueue($"Error: {extraInfo}");
        }

        public struct Token {
            public String value;
            public enum Type { Number, Operator, Constant, Function, Special };
            public Type type;
            public int priority;
            public int argCount;
            public bool leftAssociative;
            public double constantValue;

            public static Token empty = new("", Type.Special);
            public int Length {
                get => value.Length;
            }

            public Token(string value, Type type, int argCount = 0, byte priority = 0, bool leftAssociative = true, double constantValue = 0.0) {
                this.value = value;
                this.type = type;
                this.argCount = argCount;
                this.priority = priority;
                this.leftAssociative = leftAssociative;
                this.constantValue = constantValue;
            }
        public static bool operator ==(Token a, Token b) =>
                a.value == b.value && a.type == b.type;
            public static bool operator !=(Token a, Token b) =>
                a.value != b.value || a.type != b.type;
            public override string ToString() =>
                value;
                //$"{value}:{type.ToString()}";
        }
    }
}
