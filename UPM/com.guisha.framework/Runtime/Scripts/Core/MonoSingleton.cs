using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。
using System; // 引入 System 命名空间，使用其中的类型与 API。

namespace Guisha.Core // 将本文件类型归入 Guisha.Core，避免类型命名冲突。
{ // 开始命名空间。
    // 使用显式启动；读取 Instance 不会创建隐藏管理器，也不会在退出时重新创建对象。
    /// <summary>
    /// 继承 MonoBehaviour 的泛型单例基类，默认跨场景，实例由显式启动流程创建。
    /// </summary>
    /// <typeparam name="T">具体的 Mono 管理器类型；采用自身类型继承 MonoSingleton&lt;T&gt;。</typeparam>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T> // 继承 MonoBehaviour 的泛型单例基类，默认跨场景，实例由显式启动流程创建。
    { // 开始类型。
        /// <summary>
        /// 获取当前主单例；仅基类生命周期可以写入，读取不会隐式创建。
        /// </summary>
        public static T Instance { get; private set; } // 获取当前主单例；仅基类生命周期可以写入，读取不会隐式创建。
        /// <summary>
        /// 获取主单例是否存在。
        /// </summary>
        public static bool Exists => Instance != null; // 获取主单例是否存在。

        /// <summary>
        /// 以 out 参数取当前实例并返回是否存在，不隐式创建对象。
        /// </summary>
        /// <param name="instance">输出当前主单例；返回 false 时为 null。</param>
        /// <returns>true 表示主实例存在；false 表示尚未建立实例。</returns>
        public static bool TryGetInstance(out T instance) // 以 out 参数取当前实例并返回是否存在，不隐式创建对象。
        { // 开始方法。
            instance = Instance; // 通过 out 参数把当前主单例交给调用方；这里不会创建实例。
            return instance != null; // 用返回值告知调用方是否成功取得已存在的主单例。
        } // 结束方法。

        /// <summary>
        /// 返回当前实例；尚未启动时抛 InvalidOperationException。
        /// </summary>
        /// <returns>已经完成显式启动的主单例实例。</returns>
        public static T RequireInstance() // 返回当前实例；尚未启动时抛 InvalidOperationException。
        { // 开始方法。
            if (Instance == null) // 当尚未建立当前类型的单例时，执行下方分支。
                throw new InvalidOperationException(typeof(T).Name + " has not been bootstrapped."); // 尚未显式启动该类型管理器，抛出异常提醒组合根补齐初始化。
            return Instance; // 返回 当前类型的主单例实例。
        } // 结束方法。

        /// <summary>
        /// 获取是否跨场景保留。
        /// </summary>
        protected virtual bool PersistAcrossScenes => true; // 获取是否跨场景保留。
        /// <summary>
        /// 获取当前组件是否为主实例。
        /// </summary>
        protected bool IsPrimaryInstance => Instance == this; // 获取当前组件是否为主实例。

        /// <summary>
        /// 建立当前类型的主实例；重复实例销毁自身 GameObject，按 PersistAcrossScenes 决定是否常驻。
        /// </summary>
        /// <remarks>派生类重写时保留 base 调用；重复实例不要执行主实例业务初始化。</remarks>
        protected virtual void Awake() // 建立当前类型的主实例；重复实例销毁自身 GameObject，按 PersistAcrossScenes 决定是否常驻。
        { // 开始方法。
            if (Instance != null && Instance != this) // 当主实例已经存在，且不是当前组件时，执行下方分支。
            { // 开始条件分支。
                Destroy(gameObject); // 请求 Unity 销毁指定对象，实际销毁遵循 Unity 生命周期。
                return; // 提前结束当前方法/回调，跳过后续处理。
            } // 结束条件分支。

            Instance = (T)this; // 把当前组件登记为该泛型类型的主单例实例。
            if (PersistAcrossScenes) // 当配置要求跨场景保留时，执行下方分支。
            { // 开始条件分支。
                if (transform.parent != null) // 当本对象父 Transform不为空时，执行下方分支。
                    transform.SetParent(null); // 先脱离父层级，确保 DontDestroyOnLoad 作用在独立根 GameObject 上。
                DontDestroyOnLoad(gameObject); // 将对象标记为跨场景保留。
            } // 结束条件分支。
        } // 结束方法。

        /// <summary>
        /// 仅当销毁的是主实例时清空静态 Instance。
        /// </summary>
        /// <remarks>派生类重写时保留 base 调用；重复实例不要执行主实例业务初始化。</remarks>
        protected virtual void OnDestroy() // 仅当销毁的是主实例时清空静态 Instance。
        { // 开始方法。
            if (Instance == this) // 当当前组件就是主单例时，执行下方分支。
                Instance = null; // 清空主单例引用，允许后续显式创建新的实例。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
