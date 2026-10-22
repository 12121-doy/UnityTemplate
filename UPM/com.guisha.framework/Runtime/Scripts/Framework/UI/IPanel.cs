namespace Guisha.Framework // 将本文件类型归入 Guisha.Framework，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// UI 管理器依赖的面板契约。
    /// </summary>
    public interface IPanel // UI 管理器依赖的面板契约。
    { // 开始接口。
        /// <summary>
        /// 获取稳定标识。
        /// </summary>
        string Id { get; } // 获取稳定标识。

        /// <summary>
        /// 获取面板是否模态。
        /// </summary>
        bool Modal { get; } // 获取面板是否模态。

        /// <summary>
        /// 获取面板是否打开。
        /// </summary>
        bool IsOpen { get; } // 获取面板是否打开。

        /// <summary>
        /// 显示面板。
        /// </summary>
        void Open(); // 显示面板。
        /// <summary>
        /// 隐藏面板。
        /// </summary>
        void Close(); // 隐藏面板。
        /// <summary>
        /// 设置面板是否参与交互。
        /// </summary>
        /// <param name="value">当前处理值。</param>
        void SetInteractable(bool value); // 设置面板是否参与交互。
    } // 结束接口。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct PanelRegistered // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存面板接口。
        /// </summary>
        public readonly IPanel Panel; // 声明 面板接口，后续初始化。
        /// <summary>
        /// 创建 PanelRegistered 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="panel">传入的 panel，类型为 Guisha.Framework.IPanel。</param>
        public PanelRegistered(IPanel panel) // 构造 PanelRegistered，初始化其依赖与状态。
        { // 开始构造函数。
            Panel = panel; // 更新 面板接口，使用 panel。
        } // 结束构造函数。
    } // 结束数据结构。

    /// <summary>
    /// 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    /// </summary>
    public readonly struct PanelUnregistered // 模块间共享的不可变事件/数据载荷；通过构造函数填入字段。
    { // 开始数据结构。
        /// <summary>
        /// 保存面板接口。
        /// </summary>
        public readonly IPanel Panel; // 声明 面板接口，后续初始化。
        /// <summary>
        /// 创建 PanelUnregistered 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="panel">传入的 panel，类型为 Guisha.Framework.IPanel。</param>
        public PanelUnregistered(IPanel panel) // 构造 PanelUnregistered，初始化其依赖与状态。
        { // 开始构造函数。
            Panel = panel; // 更新 面板接口，使用 panel。
        } // 结束构造函数。
    } // 结束数据结构。
} // 结束命名空间。
