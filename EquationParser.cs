using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Calcuhandy {
    public static class EquationParser {
        public static Token inverseToken = new Token("inverse", 1, Token.Type.Operator, 4);
        public static Token[] globalTokens = {
            // Operators //
            new Token("+", 2, Token.Type.Operator, 1),
            new Token("-", 2, Token.Type.Operator, 1),
            new Token("*", 2, Token.Type.Operator, 2),
            new Token("/", 2, Token.Type.Operator, 2),
            new Token("%", 2, Token.Type.Operator, 2),
            new Token("^", 2, Token.Type.Operator, 3, false),
            inverseToken,
            // Special //
            new Token("(", Token.Type.Special),
            new Token(")", Token.Type.Special),
            new Token(",", Token.Type.Special),
            new Token("=", Token.Type.Special),
            // Functions //
            new Token("abs", 1, Token.Type.Function),
            new Token("acos", 1, Token.Type.Function),
            new Token("asin", 1, Token.Type.Function),
            new Token("atan2", 2, Token.Type.Function),
            new Token("atan", 1, Token.Type.Function),
            new Token("ceil", 1, Token.Type.Function),
            new Token("cos", 1, Token.Type.Function),
            new Token("floor", 1, Token.Type.Function),
            new Token("max", 2, Token.Type.Function),
            new Token("min", 2, Token.Type.Function),
            new Token("sin", 1, Token.Type.Function),
            new Token("tan", 1, Token.Type.Function),
            // Constants //
            new Token("pi", Token.Type.Constant),
            new Token("tau", Token.Type.Constant),
            new Token("e", Token.Type.Constant),
        };

        static Regex whitespaceRegex = new Regex("\\s");
        static Queue<String> errorQueue = new();
        public static string ParseText(string? input) {
            if(input == null) return "Null Input";
            if(input == "") return "0";
            errorQueue.Clear();

            try {
                List<Token> elements = Tokenize(input);
                SyntaxCleaner(ref elements);
                Postfix(ref elements);

                //return errorQueue.Count > 0 ? errorQueue.Peek() : string.Join(" ", elements);
                string result = Evaluate(elements).ToString();
                return errorQueue.Count > 0 ? errorQueue.Peek() : result;
            }catch(Exception e) {
                return $"Program Error: {e}";
            }
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
            double register = 0.0;
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
                        switch(elements[i].value) {
                            case "pi":
                                register = Math.PI;
                                break;
                            case "tau":
                                register = Math.Tau;
                                break;
                            case "e":
                                register = Math.E;
                                break;
                            default:
                                FlagError($"Unimplemented const {elements[i].value}");
                                break;
                        }
                        mainStack.Push(register);
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
                            case "acos":
                                mainStack.Push(Math.Acos(argStack.Pop()));
                                break;
                            case "asin":
                                mainStack.Push(Math.Asin(argStack.Pop()));
                                break;
                            case "atan2":
                                mainStack.Push(Math.Atan2(argStack.Pop(), argStack.Pop()));
                                break;
                            case "atan":
                                mainStack.Push(Math.Atan(argStack.Pop()));
                                break;
                            case "ceil":
                                mainStack.Push(Math.Ceiling(argStack.Pop()));
                                break;
                            case "cos":
                                mainStack.Push(Math.Cos(argStack.Pop()));
                                break;
                            case "floor":
                                mainStack.Push(Math.Floor(argStack.Pop()));
                                break;
                            case "max":
                                mainStack.Push(Math.Max(argStack.Pop(), argStack.Pop()));
                                break;
                            case "min":
                                mainStack.Push(Math.Min(argStack.Pop(), argStack.Pop()));
                                break;
                            case "sin":
                                mainStack.Push(Math.Sin(argStack.Pop()));
                                break;
                            case "tan":
                                mainStack.Push(Math.Tan(argStack.Pop()));
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
            public int priority = 0;
            public int argCount = 0;
            public bool leftAssociative = true;

            public static Token empty = new("", Type.Special);
            public int Length {
                get => value.Length;
            }

            public Token(string value, Type type) {
                this.value = value;
                this.type = type;
            }
            public Token(string value, Type type, byte priority) {
                this.value = value;
                this.type = type;
                this.priority = priority;
            }
            public Token(string value, Type type, bool leftAssociative) {
                this.value = value;
                this.type = type;
                this.leftAssociative = leftAssociative;
            }
            public Token(string value, Type type, byte priority, bool leftAssociative) {
                this.value = value;
                this.type = type;
                this.priority = priority;
                this.leftAssociative = leftAssociative;
            }
            public Token(string value, int argCount, Type type) {
                this.value = value;
                this.argCount = argCount;
                this.type = type;
            }
            public Token(string value, int argCount, Type type, byte priority) {
                this.value = value;
                this.argCount = argCount;
                this.type = type;
                this.priority = priority;
            }
            public Token(string value, int argCount, Type type, bool leftAssociative) {
                this.value = value;
                this.argCount = argCount;
                this.type = type;
                this.leftAssociative = leftAssociative;
            }
            public Token(string value, int argCount, Type type, byte priority, bool leftAssociative) {
                this.value = value;
                this.argCount = argCount;
                this.type = type;
                this.priority = priority;
                this.leftAssociative = leftAssociative;
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
