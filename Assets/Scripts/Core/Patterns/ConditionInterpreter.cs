using System;
namespace Guisha.Core.Patterns
{
    public interface ICondition { bool Evaluate(Func<string, bool> symbol); }
    public static class ConditionInterpreter
    {
        private sealed class Symbol : ICondition
        {
            private readonly string name;
            public Symbol(string name) { this.name = name; }
            public bool Evaluate(Func<string, bool> symbol) => name == "true" || (name != "false" && (symbol ?? throw new ArgumentNullException(nameof(symbol)))(name));
        }
        private sealed class Not : ICondition
        {
            private readonly ICondition value;
            public Not(ICondition value) { this.value = value; }
            public bool Evaluate(Func<string, bool> symbol) => !value.Evaluate(symbol);
        }
        private sealed class Binary : ICondition
        {
            private readonly ICondition left, right;
            private readonly bool and;
            public Binary(ICondition left, ICondition right, bool and) { this.left = left; this.right = right; this.and = and; }
            public bool Evaluate(Func<string, bool> symbol) => and ? left.Evaluate(symbol) && right.Evaluate(symbol) : left.Evaluate(symbol) || right.Evaluate(symbol);
        }
        // Grammar: symbol / true / false / ! / && / || / parentheses. No reflection or code execution.
        public static ICondition Parse(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression)) throw new FormatException("Empty condition.");
            var parser = new Parser(expression); var result = parser.Or(); parser.Space();
            if (!parser.End) throw new FormatException("Unexpected condition token at " + parser.Position);
            return result;
        }
        private sealed class Parser
        {
            private readonly string text;
            public int Position { get; private set; }
            public bool End => Position >= text.Length;
            public Parser(string text) { this.text = text; }
            public void Space() { while (!End && char.IsWhiteSpace(text[Position])) Position++; }
            private bool Take(string token)
            { Space(); if (Position + token.Length > text.Length || string.CompareOrdinal(text, Position, token, 0, token.Length) != 0) return false; Position += token.Length; return true; }
            public ICondition Or() { var value = And(); while (Take("||")) value = new Binary(value, And(), false); return value; }
            private ICondition And() { var value = Unary(); while (Take("&&")) value = new Binary(value, Unary(), true); return value; }
            private int depth;
            private ICondition Unary()
            {
                if (++depth > 64) throw new FormatException("Condition nesting exceeds 64.");
                try
                {
                    if (Take("!")) return new Not(Unary());
                    if (Take("(")) { var nested = Or(); if (!Take(")")) throw new FormatException("Missing closing parenthesis."); return nested; }
                    Space(); int start = Position;
                    while (!End && (char.IsLetterOrDigit(text[Position]) || text[Position] == '_' || text[Position] == '.' || text[Position] == '-')) Position++;
                    if (start == Position) throw new FormatException("Expected symbol at " + Position);
                    return new Symbol(text.Substring(start, Position - start));
                }
                finally { depth--; }
            }
        }
    }
}
