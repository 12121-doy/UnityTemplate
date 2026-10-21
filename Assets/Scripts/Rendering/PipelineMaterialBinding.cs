using System.Collections.Generic; // 引入 System.Collections.Generic 中使用的类型。
using Guisha.Core; // 引入 Guisha.Core 中使用的类型。
using Guisha.Framework; // 引入 Guisha.Framework 中使用的类型。
using UnityEngine; // 引入 UnityEngine 中使用的类型。

namespace Guisha.Rendering // 把渲染相关类型放入 Guisha.Rendering 命名空间。
{ // 开始命名空间。
    /// <summary>为单个 Renderer 持有运行时材质副本；切换管线时从原材质重建，销毁时配对释放。</summary>
    [DisallowMultipleComponent, RequireComponent(typeof(Renderer))] // 声明 PipelineMaterialBinding，供当前模块使用。
    public sealed class PipelineMaterialBinding : MonoBehaviour // 执行 public sealed class PipelineMaterialBinding : MonoBehaviour，完成当前渲染适配步骤。
    { // 开始类型。
        /// <summary>绑定的 Renderer 组件。</summary>
        private Renderer target; // 声明绑定的 Renderer 组件。
        /// <summary>转换前的共享材质引用快照。</summary>
        private Material[] originals; // 声明转换前的共享材质引用快照。
        /// <summary>本组件创建且需要释放的材质副本。</summary>
        private readonly List<Material> owned = new List<Material>(); // 声明本组件创建且需要释放的材质副本。
        /// <summary>管线切换事件的退订令牌。</summary>
        private System.IDisposable subscription; // 声明管线切换事件的退订令牌。
        /// <summary>上一次已应用的管线类别。</summary>
        private RenderPipelineKind applied; // 声明上一次已应用的管线类别。
        /// <summary>是否已完成第一次材质准备。</summary>
        private bool ready; // 声明是否已完成第一次材质准备。
        /// <summary>启用或对象池复用时刷新，并订阅活动管线切换。</summary>
        private void OnEnable() // 启用或对象池复用时刷新，并订阅活动管线切换。
        { // 开始当前代码块。
            Refresh(); // 执行 Refresh()，完成当前渲染适配步骤。
            subscription = EventBus.Subscribe<RenderPipelineChanged>(_ => Refresh()); // 订阅对应类型事件，把模块连接到渲染适配流程，并保存退订令牌。
        } // 结束当前代码块。

        /// <summary>禁用时退订，材质仍保留给对象池下次借出使用。</summary>
        private void OnDisable() // 禁用时退订，材质仍保留给对象池下次借出使用。
        { // 开始当前代码块。
            subscription?.Dispose(); // 释放事件订阅令牌，防止组件销毁后继续收到回调。
            subscription = null; // 更新 管线切换事件的退订令牌，取值为 null。
        } // 结束当前代码块。

        /// <summary>只在首次使用或管线类别改变时创建适配材质，避免逐帧分配。</summary>
        public void Refresh() // 只在首次使用或管线类别改变时创建适配材质，避免逐帧分配。
        { // 开始当前代码块。
            if (target == null) // 仅当 (target == null) 成立时执行后续处理。
                target = GetComponent<Renderer>(); // 更新 绑定的 Renderer 组件，取值为 GetComponent<Renderer>()。
            if (originals == null) // 仅当 (originals == null) 成立时执行后续处理。
                originals = target.sharedMaterials; // 更新 转换前的共享材质引用快照，取值为 target.sharedMaterials。
            if (ready && applied == RenderPipelineService.Current) // 仅当 (ready && applied == RenderPipelineService.Current) 成立时执行后续处理。
                return; // 当前条件无需后续处理，立即结束本次调用。
            var next = new Material[originals.Length]; // 取得或创建本次准备的新材质引用数组，保存 new Material[originals.Length] 的结果。
            var created = new List<Material>(); // 取得或创建本次事务中新建的材质集合，保存 new List<Material>() 的结果。
            try // 进入可失败操作的保护区，配合 catch/finally 清理临时对象。
            { // 开始当前代码块。
                for (int i = 0; i < originals.Length; i++) // 按 (int i = 0; i < originals.Length; i++) 中的初始化、继续条件和递增步骤循环。
                { // 开始当前代码块。
                    next[i] = RenderPipelineService.AdaptMaterial(originals[i]); // 更新 next[i]，取值为 RenderPipelineService.AdaptMaterial(originals[i])。
                    if (next[i] != originals[i]) // 仅当 (next[i] != originals[i]) 成立时执行后续处理。
                        created.Add(next[i]); // 将当前对象登记到由本模块管理的集合。
                } // 结束当前代码块。
            } // 结束当前代码块。
            catch // 捕获失败结果并释放已创建的临时资源。
            { // 开始当前代码块。
                foreach (var material in created) // 遍历 created，逐个处理并保持资源拥有权。
                    Destroy(material); // 请求 Unity 在帧末销毁本代码拥有的对象，并保留共享源资产。
                throw; // 执行 throw，完成当前渲染适配步骤。
            } // 结束当前代码块。

            target.sharedMaterials = next; // 更新 target.sharedMaterials，取值为 next。
            foreach (var material in owned) // 遍历 owned，逐个处理并保持资源拥有权。
                Destroy(material); // 请求 Unity 在帧末销毁本代码拥有的对象，并保留共享源资产。
            owned.Clear(); // 清空已失效的缓存或拥有权记录。
            owned.AddRange(created); // 记录本次新创建的材质副本，之后由组件统一销毁。
            applied = RenderPipelineService.Current; // 更新 上一次已应用的管线类别，取值为 RenderPipelineService.Current。
            ready = true; // 更新 是否已完成第一次材质准备，取值为 true。
        } // 结束当前代码块。

        /// <summary>销毁组件时还原仍然有效的原材质，并释放自己创建的副本。</summary>
        private void OnDestroy() // 销毁组件时还原仍然有效的原材质，并释放自己创建的副本。
        { // 开始当前代码块。
            subscription?.Dispose(); // 释放事件订阅令牌，防止组件销毁后继续收到回调。
            if (target != null && originals != null) // 仅当 (target != null && originals != null) 成立时执行后续处理。
                target.sharedMaterials = originals; // 更新 target.sharedMaterials，取值为 originals。
            foreach (var material in owned) // 遍历 owned，逐个处理并保持资源拥有权。
                if (material != null) // 仅当 (material != null) 成立时执行后续处理。
                    Destroy(material); // 请求 Unity 在帧末销毁本代码拥有的对象，并保留共享源资产。
            owned.Clear(); // 清空已失效的缓存或拥有权记录。
        } // 结束当前代码块。
    } // 结束类型。
} // 结束命名空间。
