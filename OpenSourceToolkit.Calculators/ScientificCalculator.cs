using System;
using System.Collections.Generic;
using System.Globalization;

namespace OpenSourceToolkit.Calculators
{
    /// <summary>
    /// Evaluates mathematical expressions with common scientific functions and operators.
    /// </summary>
    public static class ScientificCalculator
    {
        /// <summary>
        /// Evaluates a mathematical expression and returns the calculated result.
        /// </summary>
        /// <param name="expression">The expression to evaluate.</param>
        /// <returns>The calculated result, zero for empty input, or <see cref="double.NaN"/> when evaluation fails.</returns>
        public static double Evaluate(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression)) return 0;

            // Remove spaces
            expression = expression.Replace(" ", "").Replace(",", ".");

            try
            {
                var rpn = ShuntingYard(expression);
                return EvaluateRPN(rpn);
            }
            catch (Exception)
            {
                return double.NaN;
            }
        }

        private static Queue<string> ShuntingYard(string expression)
        {
            var outputQueue = new Queue<string>();
            var operatorStack = new Stack<string>();

            // Tokenize
            var tokens = Tokenize(expression);

            foreach (var token in tokens)
            {
                if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
                {
                    outputQueue.Enqueue(token);
                }
                else if (IsFunction(token))
                {
                    operatorStack.Push(token);
                }
                else if (token == ",")
                {
                    while (operatorStack.Count > 0 && operatorStack.Peek() != "(")
                    {
                        outputQueue.Enqueue(operatorStack.Pop());
                    }
                }
                else if (IsOperator(token))
                {
                    while (operatorStack.Count > 0 &&
                           IsOperator(operatorStack.Peek()) &&
                           GetPrecedence(operatorStack.Peek()) >= GetPrecedence(token))
                    {
                        outputQueue.Enqueue(operatorStack.Pop());
                    }
                    operatorStack.Push(token);
                }
                else if (token == "(")
                {
                    operatorStack.Push(token);
                }
                else if (token == ")")
                {
                    while (operatorStack.Count > 0 && operatorStack.Peek() != "(")
                    {
                        outputQueue.Enqueue(operatorStack.Pop());
                    }
                    if (operatorStack.Count > 0 && operatorStack.Peek() == "(")
                    {
                        operatorStack.Pop();
                    }
                    if (operatorStack.Count > 0 && IsFunction(operatorStack.Peek()))
                    {
                        outputQueue.Enqueue(operatorStack.Pop());
                    }
                }
                else if (token == "pi" || token == "e")
                {
                     outputQueue.Enqueue(token);
                }
            }

            while (operatorStack.Count > 0)
            {
                outputQueue.Enqueue(operatorStack.Pop());
            }

            return outputQueue;
        }

        private static List<string> Tokenize(string expression)
        {
            var tokens = new List<string>();
            var buffer = "";

            for (int i = 0; i < expression.Length; i++)
            {
                char c = expression[i];

                if (char.IsDigit(c) || c == '.')
                {
                    buffer += c;
                }
                else
                {
                    if (!string.IsNullOrEmpty(buffer))
                    {
                        tokens.Add(buffer);
                        buffer = "";
                    }

                    if (char.IsLetter(c))
                    {
                        // Function or constant
                        string word = "" + c;
                        while (i + 1 < expression.Length && char.IsLetter(expression[i + 1]))
                        {
                            word += expression[++i];
                        }
                        tokens.Add(word.ToLower());
                    }
                    else
                    {
                        // Operator or parenthesis
                        tokens.Add(c.ToString());
                    }
                }
            }

            if (!string.IsNullOrEmpty(buffer))
            {
                tokens.Add(buffer);
            }

            // Handle unary minus: if '-' is first or follows an operator/paren
            for (int i = 0; i < tokens.Count; i++)
            {
                if (tokens[i] == "-")
                {
                    bool isUnary = (i == 0) || (IsOperator(tokens[i - 1]) || tokens[i - 1] == "(");
                    if (isUnary)
                    {
                        tokens[i] = "neg"; // Replace with unary negation operator
                    }
                }
            }

            return tokens;
        }

        private static double EvaluateRPN(Queue<string> rpn)
        {
            var stack = new Stack<double>();

            while (rpn.Count > 0)
            {
                string token = rpn.Dequeue();

                if (double.TryParse(token, NumberStyles.Any, CultureInfo.InvariantCulture, out double num))
                {
                    stack.Push(num);
                }
                else if (token == "pi") stack.Push(Math.PI);
                else if (token == "e") stack.Push(Math.E);
                else if (IsOperator(token) || token == "neg")
                {
                    if (token == "neg")
                    {
                        if (stack.Count < 1) throw new InvalidOperationException();
                        stack.Push(-stack.Pop());
                    }
                    else
                    {
                        if (stack.Count < 2) throw new InvalidOperationException();
                        double b = stack.Pop();
                        double a = stack.Pop();
                        stack.Push(ApplyOperator(token, a, b));
                    }
                }
                else if (IsFunction(token))
                {
                    if (stack.Count < 1) throw new InvalidOperationException();
                    double a = stack.Pop();
                    stack.Push(ApplyFunction(token, a));
                }
            }

            return stack.Count == 1 ? stack.Pop() : double.NaN;
        }

        private static bool IsOperator(string token)
        {
            return token == "+" || token == "-" || token == "*" || token == "/" || token == "^" || token == "%" || token == "neg";
        }

        private static bool IsFunction(string token)
        {
            return token == "sin" || token == "cos" || token == "tan" ||
                   token == "asin" || token == "acos" || token == "atan" ||
                   token == "sqrt" || token == "log" || token == "ln" ||
                   token == "abs" || token == "floor" || token == "ceil" ||
                   token == "round" || token == "trunc";
        }

        private static int GetPrecedence(string op)
        {
            if (op == "neg") return 5; // Unary negation has highest precedence
            if (op == "^") return 4;
            if (op == "*" || op == "/" || op == "%") return 3;
            if (op == "+" || op == "-") return 2;
            return 0;
        }

        private static double ApplyOperator(string op, double a, double b)
        {
            switch (op)
            {
                case "+": return a + b;
                case "-": return a - b;
                case "*": return a * b;
                case "/": return a / b;
                case "%": return a % b;
                case "^": return Math.Pow(a, b);
                default: return 0;
            }
        }

        private static double ApplyFunction(string func, double a)
        {
            switch (func)
            {
                case "sin": return Math.Sin(a);
                case "cos": return Math.Cos(a);
                case "tan": return Math.Tan(a);
                case "asin": return Math.Asin(a);
                case "acos": return Math.Acos(a);
                case "atan": return Math.Atan(a);
                case "sqrt": return Math.Sqrt(a);
                case "log": return Math.Log10(a);
                case "ln": return Math.Log(a);
                case "abs": return Math.Abs(a);
                case "floor": return Math.Floor(a);
                case "ceil": return Math.Ceiling(a);
                case "round": return Math.Round(a);
                case "trunc": return Math.Truncate(a);
                default: return 0;
            }
        }
    }
}
