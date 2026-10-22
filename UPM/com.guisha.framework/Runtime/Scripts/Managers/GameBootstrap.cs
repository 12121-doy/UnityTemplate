using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Managers // 将本文件类型归入 Guisha.Managers，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 项目提供 FrameworkSettings 配置时，在首场景加载前显式创建框架管理器。
    /// </summary>
    public static class GameBootstrap // 在首场景加载前显式创建框架管理器。
    { // 开始类型。
        /// <summary>
        /// 读取 FrameworkSettings 日志开关，显式创建资源、音频、UI、场景、玩法和流程管理器。
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] // 指定由 Unity 在配置的运行时初始化阶段自动调用。
        private static void Boot() // 读取 FrameworkSettings 日志开关，显式创建资源、音频、UI、场景、玩法和流程管理器。
        { // 开始方法。
            var settings = Resources.Load<FrameworkSettings>("Guisha/FrameworkSettings"); // 把 Resources.Load<FrameworkSettings> 的返回值 保存为 框架或动画配置。
            if (settings == null) // 引入 UPM 包不等于启用整套游戏流程；由项目自己的配置资产显式启用。
                return; // 未接入框架玩法的项目仍可独立使用 Core 和设计模式工具。
            if (settings != null) // 当框架或动画配置不为空时，执行下方分支。
            { // 开始条件分支。
                Guisha.Core.Logger.Enabled = settings.enableLogs; // 更新 Guisha.Core.Logger.Enabled，使用 settings.enableLogs。
                Guisha.Core.Logger.WriteFile = settings.writeLogFile; // 更新 Guisha.Core.Logger.WriteFile，使用 settings.writeLogFile。
            } // 结束条件分支。

            Ensure<RenderingManager>(); // 建立渲染适配入口；在场景加载与实体生成时处理当前活动管线。
            Ensure<ResourceManager>(); // 当前类型实例不存在时新建独立根对象并 AddComponent。
            Ensure<AudioManager>(); // 当前类型实例不存在时新建独立根对象并 AddComponent。
            Ensure<UIManager>(); // 当前类型实例不存在时新建独立根对象并 AddComponent。
            Ensure<SceneFlowManager>(); // 当前类型实例不存在时新建独立根对象并 AddComponent。
            Ensure<GameModeManager>(); // 当前类型实例不存在时新建独立根对象并 AddComponent。
            Ensure<GameManager>(); // 当前类型实例不存在时新建独立根对象并 AddComponent。
        } // 结束方法。

        /// <summary>
        /// 当前类型实例不存在时新建独立根对象并 AddComponent。
        /// </summary>
        /// <typeparam name="T">该契约处理的数据或对象类型。</typeparam>
        private static void Ensure<T>() // 当前类型实例不存在时新建独立根对象并 AddComponent。
            where T : MonoSingleton<T> // 限定泛型类型必须满足此处的继承、接口或构造约束。
        { // 开始方法。
            if (!MonoSingleton<T>.Exists) // 当前类型没有主实例时，显式创建独立根对象。
                new GameObject(typeof(T).Name).AddComponent<T>(); // 给 new GameObject(typeof(T).Name) 添加 T 组件。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
