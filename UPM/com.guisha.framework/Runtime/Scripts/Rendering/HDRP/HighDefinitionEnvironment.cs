#if GUISHA_HDRP // 仅当对应管线包已安装且版本匹配时编译。
using UnityEngine; // 引入 UnityEngine 中使用的类型。
using UnityEngine.Rendering; // 引入 UnityEngine.Rendering 中使用的类型。
using UnityEngine.Rendering.HighDefinition; // 引入 UnityEngine.Rendering.HighDefinition 中使用的类型。

namespace Guisha.Rendering // 把渲染相关类型放入 Guisha.Rendering 命名空间。
{ // 开始命名空间。
    /// <summary>持有模板运行时曝光 Profile；场景卸载或切换管线时释放，不改项目原始 Profile。</summary>
    public sealed class HighDefinitionEnvironment : MonoBehaviour // 声明 HighDefinitionEnvironment，供当前模块使用。
    { // 开始类型。
        /// <summary>此组件拥有并负责销毁的运行时曝光 Profile。</summary>
        private VolumeProfile profile; // 声明此组件拥有并负责销毁的运行时曝光 Profile。
        /// <summary>创建固定曝光 Volume，EV100 来自模板的 HDRP 配置。</summary>
        /// <param name = "exposureEV">固定曝光值，默认 10。</param>
        public void Initialize(float exposureEV) // 创建固定曝光 Volume，EV100 来自模板的 HDRP 配置。
        { // 开始当前代码块。
            profile = ScriptableObject.CreateInstance<VolumeProfile>(); // 更新 此组件拥有并负责销毁的运行时曝光 Profile，取值为 ScriptableObject.CreateInstance<VolumeProfile>()。
            profile.hideFlags = HideFlags.DontSave; // 更新 profile.hideFlags，取值为 HideFlags.DontSave。
            var exposure = profile.Add<Exposure>(true); // 取得或创建固定曝光覆盖组件，保存 profile.Add<Exposure>(true) 的结果。
            exposure.mode.Override(ExposureMode.Fixed); // 启用曝光参数的覆盖值，使模板固定曝光生效。
            exposure.fixedExposure.Override(exposureEV); // 启用曝光参数的覆盖值，使模板固定曝光生效。
            var volume = gameObject.AddComponent<Volume>(); // 取得或创建全局曝光 Volume，保存 gameObject.AddComponent<Volume>() 的结果。
            volume.isGlobal = true; // 更新 volume.isGlobal，取值为 true。
            volume.priority = -10; // 更新 volume.priority，取值为 -10。
            volume.sharedProfile = profile; // 更新 volume.sharedProfile，取值为 profile。
        } // 结束当前代码块。

        /// <summary>VolumeComponent 也是运行时对象，和 Profile 一起配对销毁。</summary>
        private void OnDestroy() // VolumeComponent 也是运行时对象，和 Profile 一起配对销毁。
        { // 开始当前代码块。
            if (profile == null) // 仅当 (profile == null) 成立时执行后续处理。
                return; // 当前条件无需后续处理，立即结束本次调用。
            foreach (var component in profile.components) // 遍历 profile.components，逐个处理并保持资源拥有权。
                if (component != null) // 仅当 (component != null) 成立时执行后续处理。
                    Destroy(component); // 请求 Unity 在帧末销毁本代码拥有的对象，并保留共享源资产。
            Destroy(profile); // 请求 Unity 在帧末销毁本代码拥有的对象，并保留共享源资产。
            profile = null; // 更新 此组件拥有并负责销毁的运行时曝光 Profile，取值为 null。
        } // 结束当前代码块。
    } // 结束类型。
} // 结束命名空间。
#endif // 结束上方条件编译范围。
