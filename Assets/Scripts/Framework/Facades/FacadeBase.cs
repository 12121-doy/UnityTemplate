using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。

namespace Guisha.Framework // 将本文件类型归入 Guisha.Framework，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 通过受保护的 Send 方法为复杂子系统提供简单事件入口。
    /// </summary>
    public abstract class FacadeBase // 通过受保护的 Send 方法为复杂子系统提供简单事件入口。
    { // 开始类型。
        /// <summary>
        /// 向 EventBus 发布一个强类型请求。
        /// </summary>
        /// <typeparam name="T">该契约处理的数据或对象类型。</typeparam>
        /// <param name="request">收到的请求载荷。</param>
        protected void Send<T>(T request) => EventBus.Publish(request); // 向 EventBus 发布一个强类型请求。
    } // 结束类型。
} // 结束命名空间。
