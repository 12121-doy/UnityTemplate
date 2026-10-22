using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.UI // 将本文件类型归入 Guisha.UI，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 基于 CanvasGroup 的 UI 面板基类，保持对象激活以接收事件。
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))] // 声明本组件必须同时具备的依赖组件。
    public class UIPanelBase : MonoBehaviour, IPanel // 基于 CanvasGroup 的 UI 面板基类，保持对象激活以接收事件。
    { // 开始类型。
        /// <summary>
        /// 保存UI 面板注册标识。
        /// </summary>
        public string panelId = "Panel"; // 把 "Panel" 保存为 UI 面板注册标识。
        /// <summary>
        /// 保存是否使用模态输入锁。
        /// </summary>
        public bool modal = true; // 把 true（启用/满足） 保存为 是否使用模态输入锁。
        /// <summary>
        /// 保存Addressables 资源分组。
        /// </summary>
        private CanvasGroup group; // 声明 Addressables 资源分组，后续初始化。
        /// <summary>
        /// 获取稳定标识。
        /// </summary>
        public string Id => panelId; // 获取稳定标识。
        /// <summary>
        /// 读取是否采用模态面板输入锁。
        /// </summary>
        public bool Modal => modal; // 读取是否采用模态面板输入锁。
        /// <summary>
        /// 获取面板是否打开。
        /// </summary>
        public bool IsOpen { get; private set; } // 获取面板是否打开。

        /// <summary>
        /// 缓存用于控制面板显示与交互的 CanvasGroup。
        /// </summary>
        protected virtual void Awake() // 缓存用于控制面板显示与交互的 CanvasGroup。
        { // 开始方法。
            group = GetComponent<CanvasGroup>(); // 更新 Addressables 资源分组，使用 GetComponent<CanvasGroup> 的返回值。
        } // 结束方法。

        /// <summary>
        /// 发布 PanelRegistered，把自身登记到 UIManager。
        /// </summary>
        protected virtual void OnEnable() // 发布 PanelRegistered，把自身登记到 UIManager。
        { // 开始方法。
            EventBus.Publish(new PanelRegistered(this)); // 广播 PanelRegistered 事件，让对应订阅者处理该载荷。
        } // 结束方法。

        /// <summary>
        /// 发布 PanelUnregistered，让 UIManager 移除自身与输入锁。
        /// </summary>
        protected virtual void OnDisable() // 发布 PanelUnregistered，让 UIManager 移除自身与输入锁。
        { // 开始方法。
            EventBus.Publish(new PanelUnregistered(this)); // 广播 PanelUnregistered 事件，让对应订阅者处理该载荷。
        } // 结束方法。

        /// <summary>
        /// 设置打开状态、CanvasGroup 可见和交互，并移到同级末尾。
        /// </summary>
        public virtual void Open() // 设置打开状态、CanvasGroup 可见和交互，并移到同级末尾。
        { // 开始方法。
            IsOpen = true; // 更新 面板是否打开，使用 true（启用/满足）。
            group.alpha = 1; // 设置面板透明度；值来自 1。
            transform.SetAsLastSibling(); // 调用 transform.SetAsLastSibling，提交当前对象的配置更新。
            SetInteractable(true); // 结合 IsOpen 设置 CanvasGroup.interactable / blocksRaycasts。
        } // 结束方法。

        /// <summary>
        /// 设置关闭状态、隐藏 CanvasGroup 并禁用交互，GameObject 仍激活。
        /// </summary>
        public virtual void Close() // 设置关闭状态、隐藏 CanvasGroup 并禁用交互，GameObject 仍激活。
        { // 开始方法。
            IsOpen = false; // 更新 面板是否打开，使用 false（关闭/不满足）。
            group.alpha = 0; // 设置面板透明度；值来自 0。
            SetInteractable(false); // 结合 IsOpen 设置 CanvasGroup.interactable / blocksRaycasts。
        } // 结束方法。

        /// <summary>
        /// 结合 IsOpen 设置 CanvasGroup.interactable / blocksRaycasts。
        /// </summary>
        /// <param name="value">当前处理值。</param>
        public void SetInteractable(bool value) // 结合 IsOpen 设置 CanvasGroup.interactable / blocksRaycasts。
        { // 开始方法。
            group.interactable = value && IsOpen; // 设置面板控件是否可交互；值来自 value && IsOpen。
            group.blocksRaycasts = value && IsOpen; // 设置面板是否阻挡 UI 射线；值来自 value && IsOpen。
        } // 结束方法。
    // 保持面板 GameObject 激活，让隐藏面板仍能接收事件总线消息。
    } // 结束类型。
} // 结束命名空间。
