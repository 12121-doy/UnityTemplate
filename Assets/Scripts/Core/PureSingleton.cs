using System; // 引入 System 命名空间，使用其中的类型与 API。

namespace Guisha.Core // 将本文件类型归入 Guisha.Core，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 以 Lazy&lt;T&gt; 保存纯 C# 单例；派生类型需满足 class、new() 约束。
    /// </summary>
    /// <typeparam name="T">纯 C# 服务类型；必须有 public 无参构造函数。</typeparam>
    public abstract class PureSingleton<T> // 以 Lazy<T> 保存纯 C# 单例；派生类型需满足 class、new() 约束。
        where T : class, new() // 限定泛型类型必须满足此处的继承、接口或构造约束。
    { // 开始类型。
        /// <summary>
        /// 保存封装的数据值。
        /// </summary>
        private static readonly Lazy<T> Value = new Lazy<T>(() => new T()); // 创建 Lazy<T>，保存为 封装的数据值。
        /// <summary>
        /// 获取由 Lazy&lt;T&gt; 初始化的纯 C# 单例实例。
        /// </summary>
        public static T Instance => Value.Value; // 获取由 Lazy<T> 初始化的纯 C# 单例实例。
    } // 结束类型。
} // 结束命名空间。
