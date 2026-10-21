using System; // 引入 System 命名空间，使用其中的类型与 API。
using System.Collections.Generic; // 引入 System.Collections.Generic 命名空间，使用其中的类型与 API。
using Guisha.Core.Patterns; // 引入 Guisha.Core.Patterns 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Framework // 将本文件类型归入 Guisha.Framework，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 统一动画参数、触发器和状态播放接口。
    /// </summary>
    public interface IAnimationDriver // 统一动画参数、触发器和状态播放接口。
    { // 开始接口。
        /// <summary>
        /// 获取动画后端是否可用。
        /// </summary>
        bool Available { get; } // 获取动画后端是否可用。

        /// <summary>
        /// 设置浮点动画参数。
        /// </summary>
        /// <param name="parameter">动画参数名称或哈希。</param>
        /// <param name="value">当前处理值。</param>
        void Float(string parameter, float value); // 设置浮点动画参数。
        /// <summary>
        /// 设置布尔动画参数。
        /// </summary>
        /// <param name="parameter">动画参数名称或哈希。</param>
        /// <param name="value">当前处理值。</param>
        void Bool(string parameter, bool value); // 设置布尔动画参数。
        /// <summary>
        /// 触发动画参数。
        /// </summary>
        /// <param name="parameter">动画参数名称或哈希。</param>
        void Trigger(string parameter); // 触发动画参数。
        /// <summary>
        /// 播放指定动作状态。
        /// </summary>
        /// <param name="state">Animator 状态或 Legacy Animation 片段名称。</param>
        /// <param name="fade">淡入淡出时长或协程句柄。</param>
        void Play(string state, float fade = 0.15f); // 播放指定动作状态。
    } // 结束接口。

    /// <summary>
    /// 没有动画组件时使用的空对象驱动。
    /// </summary>
    public sealed class NullAnimationDriver : IAnimationDriver // 没有动画组件时使用的空对象驱动。
    { // 开始类型。
        /// <summary>
        /// 保存当前类型的主单例实例。
        /// </summary>
        public static readonly NullAnimationDriver Instance = new NullAnimationDriver(); // 创建 NullAnimationDriver，保存为 当前类型的主单例实例。
        /// <summary>
        /// 私有构造函数；外部使用共享 Instance。
        /// </summary>
        private NullAnimationDriver() // 构造 NullAnimationDriver，初始化其依赖与状态。
        { // 开始构造函数。
        } // 结束构造函数。

        /// <summary>
        /// 获取动画后端是否可用。
        /// </summary>
        public bool Available => false; // 获取动画后端是否可用。

        /// <summary>
        /// 空对象安全实现，不执行动画操作。
        /// </summary>
        /// <param name="parameter">动画参数名称或哈希。</param>
        /// <param name="value">当前处理值。</param>
        public void Float(string parameter, float value) // 空对象安全实现，不执行动画操作。
        { // 开始方法。
        } // 结束方法。

        /// <summary>
        /// 空对象安全实现，不执行动画操作。
        /// </summary>
        /// <param name="parameter">动画参数名称或哈希。</param>
        /// <param name="value">当前处理值。</param>
        public void Bool(string parameter, bool value) // 空对象安全实现，不执行动画操作。
        { // 开始方法。
        } // 结束方法。

        /// <summary>
        /// 空对象安全实现，不执行动画操作。
        /// </summary>
        /// <param name="parameter">动画参数名称或哈希。</param>
        public void Trigger(string parameter) // 空对象安全实现，不执行动画操作。
        { // 开始方法。
        } // 结束方法。

        /// <summary>
        /// 空对象安全实现，不执行动画操作。
        /// </summary>
        /// <param name="state">目标状态或保存的状态数据。</param>
        /// <param name="fade">淡入淡出时长或协程句柄。</param>
        public void Play(string state, float fade = 0.15f) // 空对象安全实现，不执行动画操作。
        { // 开始方法。
        } // 结束方法。
    } // 结束类型。

    // 共享整型哈希是享元数据，不涉及 Unity 对象拥有权或销毁。
    /// <summary>
    /// 以享元缓存共享 Animator 参数/状态哈希。
    /// </summary>
    public static class AnimationHashes // 以享元缓存共享 Animator 参数/状态哈希。
    { // 开始类型。
        /// <summary>
        /// 保存动画哈希缓存。
        /// </summary>
        private static readonly FlyweightCache<string, int> Hashes = new FlyweightCache<string, int>(Animator.StringToHash); // 创建 FlyweightCache<string, int>，保存为 动画哈希缓存。
        /// <summary>
        /// 取得共享的 Animator.StringToHash 结果。
        /// </summary>
        /// <param name="name">名称。</param>
        /// <returns>int 类型的处理结果。</returns>
        public static int Get(string name) => Hashes.Get(name); // 取得共享的 Animator.StringToHash 结果。
    } // 结束类型。

    /// <summary>
    /// 适配 Animator，按实际控制器验证参数与状态。
    /// </summary>
    public sealed class AnimatorDriver : IAnimationDriver // 适配 Animator，按实际控制器验证参数与状态。
    { // 开始类型。
        /// <summary>
        /// 保存Animator 动画组件。
        /// </summary>
        private readonly Animator animator; // 声明 Animator 动画组件，后续初始化。
        /// <summary>
        /// 保存当前 Animator 控制器。
        /// </summary>
        private RuntimeAnimatorController controller; // 声明 当前 Animator 控制器，后续初始化。
        /// <summary>
        /// 保存动画参数名哈希到类型的映射。
        /// </summary>
        private readonly Dictionary<int, AnimatorControllerParameterType> parameters = new Dictionary<int, AnimatorControllerParameterType>(); // 创建 Dictionary<int, AnimatorControllerParameterType>，保存为 动画参数名哈希到类型的映射。
        /// <summary>
        /// 构造 AnimatorDriver，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="animator">Animator 动画组件。</param>
        public AnimatorDriver(Animator animator) // 构造 AnimatorDriver，初始化其依赖与状态。
        { // 开始构造函数。
            this.animator = animator; // 更新 Animator 动画组件，使用 Animator 动画组件。
            Refresh(); // 重新读取当前 runtimeAnimatorController 的参数名称哈希与类型。
        } // 结束构造函数。

        /// <summary>
        /// 获取动画后端是否可用。
        /// </summary>
        public bool Available => animator != null && animator.runtimeAnimatorController != null && animator.isActiveAndEnabled; // 获取动画后端是否可用。

        /// <summary>
        /// 重新读取当前 runtimeAnimatorController 的参数名称哈希与类型。
        /// </summary>
        private void Refresh() // 重新读取当前 runtimeAnimatorController 的参数名称哈希与类型。
        { // 开始方法。
            parameters.Clear(); // 清空 动画参数名哈希到类型的映射 中保存的记录。
            controller = animator != null ? animator.runtimeAnimatorController : null; // 更新 当前 Animator 控制器，使用 按条件 animator != null 选择的值。
            if (controller == null) // 当当前 Animator 控制器为空时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            foreach (var parameter in animator.parameters) // 遍历 animator.parameters，每次把一个元素保存为 parameter。
                parameters[parameter.nameHash] = parameter.type; // 更新 parameters[parameter.nameHash]，使用 parameter.type。
        } // 结束方法。

        /// <summary>
        /// 验证组件可用、参数存在且类型匹配，必要时刷新控制器缓存。
        /// </summary>
        /// <param name="name">名称。</param>
        /// <param name="type">Animator 参数类型。</param>
        /// <param name="hash">输出动画参数名哈希。</param>
        /// <returns>true 表示动画参数存在且类型匹配。</returns>
        private bool Supports(string name, AnimatorControllerParameterType type, out int hash) // 验证组件可用、参数存在且类型匹配，必要时刷新控制器缓存。
        { // 开始方法。
            hash = 0; // 更新 动画参数名哈希，使用 0。
            if (!Available || string.IsNullOrEmpty(name)) // 当（条件 Available 不成立；或条件 string.IsNullOrEmpty(name) 成立）时，执行下方分支。
                return false; // 返回 false，将当前分支的布尔结果交给调用方。
            if (controller != animator.runtimeAnimatorController) // 当当前 Animator 控制器不等于animator.runtimeAnimatorController时，执行下方分支。
                Refresh(); // 重新读取当前 runtimeAnimatorController 的参数名称哈希与类型。
            hash = AnimationHashes.Get(name); // 更新 动画参数名哈希，使用 AnimationHashes.Get 的返回值。
            return parameters.TryGetValue(hash, out var found) && found == type; // 返回 parameters.TryGetValue(hash, out var found) && found == type。
        } // 结束方法。

        /// <summary>
        /// 参数存在且为 Float 时设置数值。
        /// </summary>
        /// <param name="parameter">动画参数名。</param>
        /// <param name="value">当前处理值。</param>
        public void Float(string parameter, float value) // 参数存在且为 Float 时设置数值。
        { // 开始方法。
            if (Supports(parameter, AnimatorControllerParameterType.Float, out var hash)) // 当条件 Supports(parameter, AnimatorControllerParameterType.Float, out var hash) 成立时，执行下方分支。
                animator.SetFloat(hash, value); // 向 Animator 写入/触发已验证的动画参数 hash、value。
        } // 结束方法。

        /// <summary>
        /// 参数存在且为 Bool 时设置开关。
        /// </summary>
        /// <param name="parameter">动画参数名。</param>
        /// <param name="value">当前处理值。</param>
        public void Bool(string parameter, bool value) // 参数存在且为 Bool 时设置开关。
        { // 开始方法。
            if (Supports(parameter, AnimatorControllerParameterType.Bool, out var hash)) // 当条件 Supports(parameter, AnimatorControllerParameterType.Bool, out var hash) 成立时，执行下方分支。
                animator.SetBool(hash, value); // 向 Animator 写入/触发已验证的动画参数 hash、value。
        } // 结束方法。

        /// <summary>
        /// 参数存在且为 Trigger 时触发。
        /// </summary>
        /// <param name="parameter">动画参数名。</param>
        public void Trigger(string parameter) // 参数存在且为 Trigger 时触发。
        { // 开始方法。
            if (Supports(parameter, AnimatorControllerParameterType.Trigger, out var hash)) // 当条件 Supports(parameter, AnimatorControllerParameterType.Trigger, out var hash) 成立时，执行下方分支。
                animator.SetTrigger(hash); // 向 Animator 写入/触发已验证的动画参数 hash。
        } // 结束方法。

        /// <summary>
        /// 验证第 0 层包含状态后，以固定时间 CrossFade 播放。
        /// </summary>
        /// <param name="state">Animator 第 0 层中存在的状态名称。</param>
        /// <param name="fade">淡入淡出时长或协程句柄。</param>
        public void Play(string state, float fade = 0.15f) // 验证第 0 层包含状态后，以固定时间 CrossFade 播放。
        { // 开始方法。
            if (!Available || string.IsNullOrEmpty(state)) // 当（条件 Available 不成立；或条件 string.IsNullOrEmpty(state) 成立）时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            int hash = AnimationHashes.Get(state); // 把 AnimationHashes.Get 的返回值 保存为 动画参数名哈希。
            if (animator.HasState(0, hash)) // 当条件 animator.HasState(0, hash) 成立时，执行下方分支。
                animator.CrossFadeInFixedTime(hash, Mathf.Max(0, fade)); // 按固定秒数淡入淡出切换到 hash、Mathf.Max(0, fade)。
        } // 结束方法。
    } // 结束类型。

    /// <summary>
    /// 适配旧 Animation 的片段播放和淡入淡出；不提供参数式移动。
    /// </summary>
    public sealed class LegacyAnimationDriver : IAnimationDriver // 适配旧 Animation 的片段播放和淡入淡出；不提供参数式移动。
    { // 开始类型。
        /// <summary>
        /// 保存旧版 Animation 组件。
        /// </summary>
        private readonly Animation animation; // 声明 旧版 Animation 组件，后续初始化。
        /// <summary>
        /// 构造 LegacyAnimationDriver，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="animation">旧版 Animation 组件。</param>
        public LegacyAnimationDriver(Animation animation) // 构造 LegacyAnimationDriver，初始化其依赖与状态。
        { // 开始构造函数。
            this.animation = animation; // 更新 旧版 Animation 组件，使用 旧版 Animation 组件。
        } // 结束构造函数。

        /// <summary>
        /// 获取动画后端是否可用。
        /// </summary>
        public bool Available => animation != null && animation.isActiveAndEnabled; // 获取动画后端是否可用。

        /// <summary>
        /// Legacy 不提供参数式移动，此方法安全空操作。
        /// </summary>
        /// <param name="parameter">动画参数名称或哈希。</param>
        /// <param name="value">当前处理值。</param>
        public void Float(string parameter, float value) // Legacy 不提供参数式移动，此方法安全空操作。
        { // 开始方法。
        } // 结束方法。

        /// <summary>
        /// Legacy 不提供布尔参数，此方法安全空操作。
        /// </summary>
        /// <param name="parameter">动画参数名称或哈希。</param>
        /// <param name="value">当前处理值。</param>
        public void Bool(string parameter, bool value) // Legacy 不提供布尔参数，此方法安全空操作。
        { // 开始方法。
        } // 结束方法。

        /// <summary>
        /// 把触发名称作为片段名称交给 Play。
        /// </summary>
        /// <param name="parameter">动画参数名称或哈希。</param>
        public void Trigger(string parameter) // 把触发名称作为片段名称交给 Play。
        { // 开始方法。
            Play(parameter); // 片段存在且组件可用时调用 Animation.CrossFade。
        } // 结束方法。

        /// <summary>
        /// 片段存在且组件可用时调用 Animation.CrossFade。
        /// </summary>
        /// <param name="state">旧 Animation 组件中存在的 Clip 名称。</param>
        /// <param name="fade">淡入淡出时长或协程句柄。</param>
        public void Play(string state, float fade = 0.15f) // 片段存在且组件可用时调用 Animation.CrossFade。
        { // 开始方法。
            if (Available && !string.IsNullOrEmpty(state) && animation[state] != null) // 当（动画后端是否可用为 true；且条件 string.IsNullOrEmpty(state) 不成立；且animation[state]不为空）时，执行下方分支。
                animation.CrossFade(state, Mathf.Max(0, fade)); // 淡入淡出切换到 state、Mathf.Max(0, fade)。
        } // 结束方法。
    } // 结束类型。

    /// <summary>
    /// 动画驱动的工厂方法基类。
    /// </summary>
    public abstract class AnimationDriverFactory : Factory<IAnimationDriver> // 动画驱动的工厂方法基类。
    { // 开始类型。
    } // 结束类型。

    /// <summary>
    /// 创建 Animator 驱动，组件为空时返回空驱动。
    /// </summary>
    public sealed class AnimatorDriverFactory : AnimationDriverFactory // 创建 Animator 驱动，组件为空时返回空驱动。
    { // 开始类型。
        /// <summary>
        /// 保存Animator 动画组件。
        /// </summary>
        private readonly Animator animator; // 声明 Animator 动画组件，后续初始化。
        /// <summary>
        /// 构造 AnimatorDriverFactory，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="animator">Animator 动画组件。</param>
        public AnimatorDriverFactory(Animator animator) // 构造 AnimatorDriverFactory，初始化其依赖与状态。
        { // 开始构造函数。
            this.animator = animator; // 更新 Animator 动画组件，使用 Animator 动画组件。
        } // 结束构造函数。

        /// <summary>
        /// 返回 AnimatorDriver；没有 Animator 引用时使用共享 NullAnimationDriver。
        /// </summary>
        /// <returns>本次创建或复制的 Guisha.Framework.IAnimationDriver 对象。</returns>
        protected override IAnimationDriver CreateProduct() => animator != null ? (IAnimationDriver)new AnimatorDriver(animator) : NullAnimationDriver.Instance; // 返回 AnimatorDriver；没有 Animator 引用时使用共享 NullAnimationDriver。
    } // 结束类型。

    /// <summary>
    /// 创建 Legacy 驱动，组件为空时返回空驱动。
    /// </summary>
    public sealed class LegacyAnimationDriverFactory : AnimationDriverFactory // 创建 Legacy 驱动，组件为空时返回空驱动。
    { // 开始类型。
        /// <summary>
        /// 保存旧版 Animation 组件。
        /// </summary>
        private readonly Animation animation; // 声明 旧版 Animation 组件，后续初始化。
        /// <summary>
        /// 构造 LegacyAnimationDriverFactory，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="animation">旧版 Animation 组件。</param>
        public LegacyAnimationDriverFactory(Animation animation) // 构造 LegacyAnimationDriverFactory，初始化其依赖与状态。
        { // 开始构造函数。
            this.animation = animation; // 更新 旧版 Animation 组件，使用 旧版 Animation 组件。
        } // 结束构造函数。

        /// <summary>
        /// 返回 LegacyAnimationDriver；没有 Animation 引用时使用空驱动。
        /// </summary>
        /// <returns>本次创建或复制的 Guisha.Framework.IAnimationDriver 对象。</returns>
        protected override IAnimationDriver CreateProduct() => animation != null ? (IAnimationDriver)new LegacyAnimationDriver(animation) : NullAnimationDriver.Instance; // 返回 LegacyAnimationDriver；没有 Animation 引用时使用空驱动。
    } // 结束类型。

    /// <summary>
    /// 在原动画驱动外增加动作/触发日志。
    /// </summary>
    public sealed class AnimationLoggingDecorator : IAnimationDriver // 在原动画驱动外增加动作/触发日志。
    { // 开始类型。
        /// <summary>
        /// 保存被包装的内部实现。
        /// </summary>
        private readonly IAnimationDriver inner; // 声明 被包装的内部实现，后续初始化。
        /// <summary>
        /// 保存日志输出开关或回调。
        /// </summary>
        private readonly Action<string> log; // 声明 日志输出开关或回调，后续初始化。
        /// <summary>
        /// 构造 AnimationLoggingDecorator，保存注入的依赖/配置并建立内部状态。
        /// </summary>
        /// <param name="inner">被包装的内部实现。</param>
        /// <param name="log">日志输出开关或回调；由调用方注入具体行为。</param>
        public AnimationLoggingDecorator(IAnimationDriver inner, Action<string> log) // 构造 AnimationLoggingDecorator，初始化其依赖与状态。
        { // 开始构造函数。
            this.inner = inner ?? throw new ArgumentNullException(nameof(inner)); // 更新 被包装的内部实现，使用 inner ?? throw new ArgumentNullException(nameof(inner))。
            this.log = log ?? throw new ArgumentNullException(nameof(log)); // 更新 日志输出开关或回调，使用 log ?? throw new ArgumentNullException(nameof(log))。
        } // 结束构造函数。

        /// <summary>
        /// 获取动画后端是否可用。
        /// </summary>
        public bool Available => inner.Available; // 获取动画后端是否可用。

        /// <summary>
        /// 直接转发 Float。
        /// </summary>
        /// <param name="parameter">动画触发器名称。</param>
        /// <param name="value">当前处理值。</param>
        public void Float(string parameter, float value) => inner.Float(parameter, value); // 直接转发 Float。
        /// <summary>
        /// 直接转发 Bool。
        /// </summary>
        /// <param name="parameter">动画触发器名称。</param>
        /// <param name="value">当前处理值。</param>
        public void Bool(string parameter, bool value) => inner.Bool(parameter, value); // 直接转发 Bool。
        /// <summary>
        /// 记录触发器名称，再转发 Trigger。
        /// </summary>
        /// <param name="parameter">动画触发器名称。</param>
        public void Trigger(string parameter) // 记录触发器名称，再转发 Trigger。
        { // 开始方法。
            log("Trigger: " + parameter); // 执行日志输出回调。
            inner.Trigger(parameter); // 触发动画参数。
        } // 结束方法。

        /// <summary>
        /// 记录状态名称，再转发 Play。
        /// </summary>
        /// <param name="state">要播放的动画状态名。</param>
        /// <param name="fade">淡入淡出时长或协程句柄。</param>
        public void Play(string state, float fade = 0.15f) // 记录状态名称，再转发 Play。
        { // 开始方法。
            log("Play: " + state); // 执行日志输出回调。
            inner.Play(state, fade); // 播放指定动作状态。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
