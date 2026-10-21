namespace Guisha.Framework // 把渲染相关类型放入 Guisha.Framework 命名空间。
{ // 开始命名空间。
    /// <summary>框架识别的当前活动渲染管线；安装了某个包不代表正在使用该管线。</summary>
    public enum RenderPipelineKind // 声明 RenderPipelineKind，供当前模块使用。
    { // 开始枚举。
        /// <summary>Unity 内置渲染管线。</summary>
        BuiltIn, // 声明Unity 内置渲染管线。
        /// <summary>Universal Render Pipeline（URP）。</summary>
        Universal, // 声明Universal Render Pipeline（URP）。
        /// <summary>High Definition Render Pipeline（HDRP）。</summary>
        HighDefinition, // 声明High Definition Render Pipeline（HDRP）。
        /// <summary>尚未适配的自定义 SRP。</summary>
        Unsupported // 声明尚未适配的自定义 SRP。
    } // 结束枚举。

    /// <summary>活动管线发生变化后通知材质、相机和模板灯光重新适配。</summary>
    public readonly struct RenderPipelineChanged // 声明 RenderPipelineChanged，供当前模块使用。
    { // 开始结构体。
        /// <summary>切换前的管线类别。</summary>
        public readonly RenderPipelineKind Previous; // 声明切换前的管线类别。
        /// <summary>当前实际活动的管线类别。</summary>
        public readonly RenderPipelineKind Current; // 声明当前实际活动的管线类别。
        /// <summary>保存管线切换前后的类别。</summary>
        /// <param name = "previous">此前使用的管线。</param>
        /// <param name = "current">现在使用的管线。</param>
        public RenderPipelineChanged(RenderPipelineKind previous, RenderPipelineKind current) // 保存管线切换前后的类别。
        { // 开始当前代码块。
            Previous = previous; // 更新 切换前的管线类别，取值为 previous。
            Current = current; // 更新 当前实际活动的管线类别，取值为 current。
        } // 结束当前代码块。
    } // 结束结构体。
} // 结束命名空间。
