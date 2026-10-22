using System; // 引入 System 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Framework // 将本文件类型归入 Guisha.Framework，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 把一个资源实例 lease 绑定到 GameObject 销毁生命周期。
    /// </summary>
    public sealed class InstanceLifetime : MonoBehaviour // 把一个资源实例 lease 绑定到 GameObject 销毁生命周期。
    { // 开始类型。
        /// <summary>
        /// 保存资源拥有权令牌。
        /// </summary>
        private IDisposable lease; // 声明 资源拥有权令牌，后续初始化。
        /// <summary>
        /// 绑定实例拥有权令牌；设计为实例创建后绑定一次。
        /// </summary>
        /// <param name="value">当前处理值。</param>
        public void Bind(IDisposable value) // 绑定实例拥有权令牌；设计为实例创建后绑定一次。
        { // 开始方法。
            lease = value; // 更新 资源拥有权令牌，使用 当前处理值。
        } // 结束方法。

        /// <summary>
        /// 销毁时释放已绑定的 lease，并清空引用。
        /// </summary>
        private void OnDestroy() // 销毁时释放已绑定的 lease，并清空引用。
        { // 开始方法。
            lease?.Dispose(); // 仅在 资源拥有权令牌 非空时，释放 资源拥有权令牌 持有的资源或注册；具体拥有权由该对象负责。
            lease = null; // 清空 资源拥有权令牌 引用，表示当前不再持有该对象。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
