using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。
using UnityEngine.SceneManagement; // 引入 UnityEngine.SceneManagement 命名空间，使用其中的类型与 API。
using System.Collections.Generic; // 引入 System.Collections.Generic 命名空间，使用其中的类型与 API。

namespace Guisha.Framework // 将本文件类型归入 Guisha.Framework，避免类型命名冲突。
{ // 开始命名空间。
    // 将组件挂在场景根对象，所有玩法内容放在独立的子节点下。
    /// <summary>
    /// 场景内容门控，先停用旧场景内容，再激活新场景。
    /// </summary>
    public sealed class ActiveSceneContent : MonoBehaviour // 场景内容门控，先停用旧场景内容，再激活新场景。
    { // 开始类型。
        /// <summary>
        /// 保存按活动场景门控的内容子节点。
        /// </summary>
        public GameObject content; // 声明 按活动场景门控的内容子节点，后续初始化。
        /// <summary>
        /// 保存场景内容根集合。
        /// </summary>
        private static readonly List<ActiveSceneContent> Roots = new List<ActiveSceneContent>(); // 创建 List<ActiveSceneContent>，保存为 场景内容根集合。
        /// <summary>
        /// 登记内容根；首个根订阅活动场景切换，然后重算显隐。
        /// </summary>
        private void OnEnable() // 登记内容根；首个根订阅活动场景切换，然后重算显隐。
        { // 开始方法。
            if (Roots.Count == 0) // 当Roots.Count等于0时，执行下方分支。
                SceneManager.activeSceneChanged += Changed; // 累加 SceneManager.activeSceneChanged，使用 Changed。
            Roots.Add(this); // 向 场景内容根集合 加入 this。
            Refresh(); // 按当前活动场景立即重算全部已注册内容根。
        } // 结束方法。

        /// <summary>
        /// 注销内容根；没有根时退订 Unity 活动场景事件。
        /// </summary>
        private void OnDisable() // 注销内容根；没有根时退订 Unity 活动场景事件。
        { // 开始方法。
            Roots.Remove(this); // 从 场景内容根集合 移除 this。
            if (Roots.Count == 0) // 当Roots.Count等于0时，执行下方分支。
                SceneManager.activeSceneChanged -= Changed; // 扣减 SceneManager.activeSceneChanged，使用 Changed。
        } // 结束方法。

        /// <summary>
        /// 先停用所有非活动场景的内容，再激活目标场景内容，避免重复面板/玩家同时注册。
        /// </summary>
        /// <param name="before">切换前的活动场景。</param>
        /// <param name="after">切换后的活动场景。</param>
        private static void Changed(Scene before, Scene after) // 先停用所有非活动场景的内容，再激活目标场景内容，避免重复面板/玩家同时注册。
        { // 开始方法。
            var roots = Roots.ToArray(); // 把 Roots.ToArray 的返回值 保存为 roots。
            // 先停用所有旧场景内容，再让新场景的面板/玩家启用注册。
            foreach (var root in roots) // 遍历 roots，每次把一个元素保存为 root。
                if (root != null && root.gameObject.scene != after) // 当（场景内容根组件不为空；且root.gameObject.scene不等于切换后的活动场景）时，执行下方分支。
                    root.SetContent(false); // 对独立 content 子对象执行 SetActive。
            foreach (var root in roots) // 遍历 roots，每次把一个元素保存为 root。
                if (root != null && root.gameObject.scene == after) // 当（场景内容根组件不为空；且root.gameObject.scene等于切换后的活动场景）时，执行下方分支。
                    root.SetContent(true); // 对独立 content 子对象执行 SetActive。
        } // 结束方法。

        /// <summary>
        /// 对独立 content 子对象执行 SetActive。
        /// </summary>
        /// <param name="value">当前处理值。</param>
        private void SetContent(bool value) // 对独立 content 子对象执行 SetActive。
        { // 开始方法。
            if (content != null && content != gameObject) // 当（按活动场景门控的内容子节点不为空；且按活动场景门控的内容子节点不等于gameObject）时，执行下方分支。
                content.SetActive(value); // 将 按活动场景门控的内容子节点 的激活状态设为 value，同时影响其组件生命周期。
        } // 结束方法。

        /// <summary>
        /// 按当前活动场景立即重算全部已注册内容根。
        /// </summary>
        public void Refresh() // 按当前活动场景立即重算全部已注册内容根。
        { // 开始方法。
            Changed(default, SceneManager.GetActiveScene()); // 先停用所有非活动场景的内容，再激活目标场景内容，避免重复面板/玩家同时注册。
        } // 结束方法。

        /// <summary>
        /// SubsystemRegistration 阶段重置静态状态，支持重新进入 Play。
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)] // 指定由 Unity 在配置的运行时初始化阶段自动调用。
        private static void Reset() // SubsystemRegistration 阶段重置静态状态，支持重新进入 Play。
        { // 开始方法。
            SceneManager.activeSceneChanged -= Changed; // 扣减 SceneManager.activeSceneChanged，使用 Changed。
            Roots.Clear(); // 清空 场景内容根集合 中保存的记录。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
