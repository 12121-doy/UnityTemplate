using System; // 引入 System 命名空间，使用其中的类型与 API。

namespace Guisha.Core.Patterns // 将本文件类型归入 Guisha.Core.Patterns，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 条件表达式求值契约。
    /// </summary>
    public interface ICondition // 条件表达式求值契约。
    { // 开始接口。
        /// <summary>
        /// 根据当前节点或策略契约计算结果。
        /// </summary>
        /// <param name="symbol">符号到布尔值的查询委托；由调用方注入具体行为。</param>
        /// <returns>当前策略或条件表达式的布尔结果。</returns>
        bool Evaluate(Func<string, bool> symbol); // 根据当前节点或策略契约计算结果。
    } // 结束接口。

    /// <summary>
    /// 解析布尔条件表达式；支持符号、true/false、!、&amp;&amp;、|| 和括号。
    /// </summary>
    public static class ConditionInterpreter // 解析布尔条件表达式；支持符号、true/false、!、&&、|| 和括号。
    { // 开始类型。
        /// <summary>
        /// 条件语法树中的符号或布尔常量节点。
        /// </summary>
        private sealed class Symbol : ICondition // 条件语法树中的符号或布尔常量节点。
        { // 开始类型。
            /// <summary>
            /// 保存名称。
            /// </summary>
            private readonly string name; // 声明 名称，后续初始化。
            /// <summary>
            /// 构造 Symbol，保存注入的依赖/配置并建立内部状态。
            /// </summary>
            /// <param name="name">名称。</param>
            public Symbol(string name) // 构造 Symbol，初始化其依赖与状态。
            { // 开始构造函数。
                this.name = name; // 更新 名称，使用 名称。
            } // 结束构造函数。

            /// <summary>
            /// 识别 true/false 常量，或调用 symbol 委托查询变量。
            /// </summary>
            /// <param name="symbol">符号到布尔值的查询委托；由调用方注入具体行为。</param>
            /// <returns>当前策略或条件表达式的布尔结果。</returns>
            public bool Evaluate(Func<string, bool> symbol) => name == "true" || (name != "false" && (symbol ?? throw new ArgumentNullException(nameof(symbol)))(name)); // 识别 true/false 常量，或调用 symbol 委托查询变量。
        } // 结束类型。

        /// <summary>
        /// 条件语法树中的逻辑非节点。
        /// </summary>
        private sealed class Not : ICondition // 条件语法树中的逻辑非节点。
        { // 开始类型。
            /// <summary>
            /// 保存当前处理值。
            /// </summary>
            private readonly ICondition value; // 声明 当前处理值，后续初始化。
            /// <summary>
            /// 构造 Not，保存注入的依赖/配置并建立内部状态。
            /// </summary>
            /// <param name="value">当前处理值。</param>
            public Not(ICondition value) // 构造 Not，初始化其依赖与状态。
            { // 开始构造函数。
                this.value = value; // 更新 当前处理值，使用 当前处理值。
            } // 结束构造函数。

            /// <summary>
            /// 对内部条件取反。
            /// </summary>
            /// <param name="symbol">符号到布尔值的查询委托；由调用方注入具体行为。</param>
            /// <returns>当前策略或条件表达式的布尔结果。</returns>
            public bool Evaluate(Func<string, bool> symbol) => !value.Evaluate(symbol); // 对内部条件取反。
        } // 结束类型。

        /// <summary>
        /// 条件语法树中的短路逻辑与/或节点。
        /// </summary>
        private sealed class Binary : ICondition // 条件语法树中的短路逻辑与/或节点。
        { // 开始类型。
            /// <summary>
            /// 保存左条件节点、右条件节点。
            /// </summary>
            private readonly ICondition left, right; // 声明 左条件节点，后续初始化；声明 右条件节点，后续初始化。
            /// <summary>
            /// 保存是否采用逻辑与。
            /// </summary>
            private readonly bool and; // 声明 是否采用逻辑与，后续初始化。
            /// <summary>
            /// 构造 Binary，保存注入的依赖/配置并建立内部状态。
            /// </summary>
            /// <param name="left">左条件节点。</param>
            /// <param name="right">右条件节点。</param>
            /// <param name="and">是否采用逻辑与。</param>
            public Binary(ICondition left, ICondition right, bool and) // 构造 Binary，初始化其依赖与状态。
            { // 开始构造函数。
                this.left = left; // 更新 左条件节点，使用 左条件节点。
                this.right = right; // 更新 右条件节点，使用 右条件节点。
                this.and = and; // 更新 是否采用逻辑与，使用 是否采用逻辑与。
            } // 结束构造函数。

            /// <summary>
            /// 以短路规则执行逻辑与或逻辑或。
            /// </summary>
            /// <param name="symbol">符号到布尔值的查询委托；由调用方注入具体行为。</param>
            /// <returns>当前策略或条件表达式的布尔结果。</returns>
            public bool Evaluate(Func<string, bool> symbol) => and ? left.Evaluate(symbol) && right.Evaluate(symbol) : left.Evaluate(symbol) || right.Evaluate(symbol); // 以短路规则执行逻辑与或逻辑或。
        } // 结束类型。

        // 语法支持符号、布尔常量、!、&&、|| 与括号；只解析条件，不通过反射执行代码。
        /// <summary>
        /// 解析并验证完整布尔表达式，返回可重复求值的 ICondition；语法错误抛 FormatException。
        /// </summary>
        /// <param name="expression">只含符号、布尔常量、!、&amp;&amp;、|| 和括号的条件文本。</param>
        /// <returns>可重复求值的条件语法树。</returns>
        public static ICondition Parse(string expression) // 解析并验证完整布尔表达式，返回可重复求值的 ICondition；语法错误抛 FormatException。
        { // 开始方法。
            if (string.IsNullOrWhiteSpace(expression)) // 当条件 string.IsNullOrWhiteSpace(expression) 成立时，执行下方分支。
                throw new FormatException("Empty condition."); // 抛出 FormatException，拒绝无效参数或状态；错误信息为 "Empty condition."。
            var parser = new Parser(expression); // 创建 Parser，保存为 parser。
            var result = parser.Or(); // 把 parser.Or 的返回值 保存为 本次操作结果。
            parser.Space(); // 略过空白字符。
            if (!parser.End) // 当条件 parser.End 不成立时，执行下方分支。
                throw new FormatException("Unexpected condition token at " + parser.Position); // 抛出 FormatException，拒绝无效参数或状态；错误信息为 "Unexpected condition token at " + parser.Position。
            return result; // 返回 本次操作结果。
        } // 结束方法。

        /// <summary>
        /// 条件表达式的递归下降解析器。
        /// </summary>
        private sealed class Parser // 条件表达式的递归下降解析器。
        { // 开始类型。
            /// <summary>
            /// 保存正文或待解析字符串。
            /// </summary>
            private readonly string text; // 声明 正文或待解析字符串，后续初始化。
            /// <summary>
            /// 获取当前解析偏移；仅解析器内部可推进。
            /// </summary>
            public int Position { get; private set; } // 获取当前解析偏移；仅解析器内部可推进。
            /// <summary>
            /// 判断解析偏移是否已到文本末尾。
            /// </summary>
            public bool End => Position >= text.Length; // 判断解析偏移是否已到文本末尾。

            /// <summary>
            /// 构造 Parser，保存注入的依赖/配置并建立内部状态。
            /// </summary>
            /// <param name="text">正文或待解析字符串。</param>
            public Parser(string text) // 构造 Parser，初始化其依赖与状态。
            { // 开始构造函数。
                this.text = text; // 更新 正文或待解析字符串，使用 正文或待解析字符串。
            } // 结束构造函数。

            /// <summary>
            /// 略过空白字符。
            /// </summary>
            public void Space() // 略过空白字符。
            { // 开始方法。
                while (!End && char.IsWhiteSpace(text[Position])) // 只要（条件 End 不成立；且条件 char.IsWhiteSpace(text[Position]) 成立），就继续循环。
                    Position++; // 递增 位置或解析偏移。
            } // 结束方法。

            /// <summary>
            /// 略过空白后尝试消费指定 token，成功则推进位置。
            /// </summary>
            /// <param name="token">取消令牌。</param>
            /// <returns>true 表示匹配并消费了解析 token。</returns>
            private bool Take(string token) // 略过空白后尝试消费指定 token，成功则推进位置。
            { // 开始方法。
                Space(); // 略过空白字符。
                if (Position + token.Length > text.Length || string.CompareOrdinal(text, Position, token, 0, token.Length) != 0) // 当（Position + token.Length大于text.Length；或条件 string.CompareOrdinal(text, Position, token, 0, token.Length) != 0 成立）时，执行下方分支。
                    return false; // 返回 false，将当前分支的布尔结果交给调用方。
                Position += token.Length; // 累加 位置或解析偏移，使用 token.Length。
                return true; // 返回 true，将当前分支的布尔结果交给调用方。
            } // 结束方法。

            /// <summary>
            /// 解析最低优先级的 || 表达式。
            /// </summary>
            /// <returns>Guisha.Core.Patterns.ICondition 类型的处理结果。</returns>
            public ICondition Or() // 解析最低优先级的 || 表达式。
            { // 开始方法。
                var value = And(); // 把 And 的返回值 保存为 当前处理值。
                while (Take("||")) // 只要条件 Take("||") 成立，就继续循环。
                    value = new Binary(value, And(), false); // 更新 当前处理值，使用 新建的 Binary 对象。
                return value; // 返回 当前处理值。
            } // 结束方法。

            /// <summary>
            /// 解析 &amp;&amp; 表达式，优先级高于 ||。
            /// </summary>
            /// <returns>Guisha.Core.Patterns.ICondition 类型的处理结果。</returns>
            private ICondition And() // 解析 && 表达式，优先级高于 ||。
            { // 开始方法。
                var value = Unary(); // 把 Unary 的返回值 保存为 当前处理值。
                while (Take("&&")) // 只要条件 Take("&&") 成立，就继续循环。
                    value = new Binary(value, Unary(), true); // 更新 当前处理值，使用 新建的 Binary 对象。
                return value; // 返回 当前处理值。
            } // 结束方法。

            /// <summary>
            /// 保存表达式嵌套深度。
            /// </summary>
            private int depth; // 声明 表达式嵌套深度，后续初始化。
            /// <summary>
            /// 解析 !、括号或符号，并限制嵌套深度为 64。
            /// </summary>
            /// <returns>Guisha.Core.Patterns.ICondition 类型的处理结果。</returns>
            private ICondition Unary() // 解析 !、括号或符号，并限制嵌套深度为 64。
            { // 开始方法。
                if (++depth > 64) // 当++depth大于64时，执行下方分支。
                    throw new FormatException("Condition nesting exceeds 64."); // 抛出 FormatException，拒绝无效参数或状态；错误信息为 "Condition nesting exceeds 64."。
                try // 进入异常保护范围；失败时由 catch/finally 配对清理。
                { // 开始受异常保护的操作。
                    if (Take("!")) // 当条件 Take("!") 成立时，执行下方分支。
                        return new Not(Unary()); // 返回 新建的 Not 对象。
                    if (Take("(")) // 当条件 Take("(") 成立时，执行下方分支。
                    { // 开始条件分支。
                        var nested = Or(); // 把 Or 的返回值 保存为 nested。
                        if (!Take(")")) // 当条件 Take(")") 不成立时，执行下方分支。
                            throw new FormatException("Missing closing parenthesis."); // 抛出 FormatException，拒绝无效参数或状态；错误信息为 "Missing closing parenthesis."。
                        return nested; // 返回 nested。
                    } // 结束条件分支。

                    Space(); // 略过空白字符。
                    int start = Position; // 把 位置或解析偏移 保存为 起点或起始索引。
                    while (!End && (char.IsLetterOrDigit(text[Position]) || text[Position] == '_' || text[Position] == '.' || text[Position] == '-')) // 只要（条件 End 不成立；且（条件 char.IsLetterOrDigit(text[Position]) 成立；或text[Position]等于'_'；或text[Position]等于'.'；或text[Position]等于'-'）），就继续循环。
                        Position++; // 递增 位置或解析偏移。
                    if (start == Position) // 当起点或起始索引等于位置或解析偏移时，执行下方分支。
                        throw new FormatException("Expected symbol at " + Position); // 抛出 FormatException，拒绝无效参数或状态；错误信息为 "Expected symbol at " + Position。
                    return new Symbol(text.Substring(start, Position - start)); // 返回 新建的 Symbol 对象。
                } // 结束受异常保护的操作。
                finally // 无论成功、失败或提前返回，都执行此清理分支。
                { // 开始最终清理。
                    depth--; // 递减 表达式嵌套深度。
                } // 结束最终清理。
            } // 结束方法。
        } // 结束类型。
    } // 结束类型。
} // 结束命名空间。
