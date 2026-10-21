using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Core // 将本文件类型归入 Guisha.Core，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 提供平面向量、距离/视锥判断、平滑系数和二次曲线计算。
    /// </summary>
    public static class MathUtility // 提供平面向量、距离/视锥判断、平滑系数和二次曲线计算。
    { // 开始类型。
        /// <summary>
        /// 把向量的 y 分量置零，得到水平分量。
        /// </summary>
        /// <param name="value">当前处理值。</param>
        /// <returns>UnityEngine.Vector3 类型的处理结果。</returns>
        public static Vector3 Planar(Vector3 value) => new Vector3(value.x, 0, value.z); // 把向量的 y 分量置零，得到水平分量。
        /// <summary>
        /// 用平方距离判断两点是否位于给定半径内。
        /// </summary>
        /// <param name="a">第一个比较或组合参数。</param>
        /// <param name="b">第二个比较或组合参数。</param>
        /// <param name="radius">碰撞检测半径。</param>
        /// <returns>true 表示距离位于指定半径内。</returns>
        public static bool InRange(Vector3 a, Vector3 b, float radius) => (a - b).sqrMagnitude <= radius * radius; // 用平方距离判断两点是否位于给定半径内。
        /// <summary>
        /// 同时检查距离和前向夹角，angle 是完整锥角。
        /// </summary>
        /// <param name="origin">检测原点。</param>
        /// <param name="forward">朝向向量。</param>
        /// <param name="point">待检测位置。</param>
        /// <param name="range">作用或检测范围。</param>
        /// <param name="angle">完整视锥角。</param>
        /// <returns>true 表示目标同时通过距离与视锥检查。</returns>
        public static bool InCone(Vector3 origin, Vector3 forward, Vector3 point, float range, float angle) => InRange(origin, point, range) && Vector3.Angle(forward, point - origin) <= angle * 0.5f; // 同时检查距离和前向夹角，angle 是完整锥角。
        /// <summary>
        /// 把平滑锐度和帧时长换算为指数插值系数。
        /// </summary>
        /// <param name="sharpness">平滑响应锐度。</param>
        /// <param name="deltaTime">当前帧秒数。</param>
        /// <returns>float 类型的处理结果。</returns>
        public static float ExpFactor(float sharpness, float deltaTime) => 1 - Mathf.Exp(-Mathf.Max(0, sharpness) * deltaTime); // 把平滑锐度和帧时长换算为指数插值系数。
        /// <summary>
        /// 计算起点、控制点、终点组成的二次贝塞尔曲线位置，t 限制到 0-1。
        /// </summary>
        /// <param name="start">起点或起始索引。</param>
        /// <param name="control">曲线控制点。</param>
        /// <param name="end">终点。</param>
        /// <param name="t">曲线插值进度，内部钳制在 0 到 1。</param>
        /// <returns>UnityEngine.Vector3 类型的处理结果。</returns>
        public static Vector3 Curve(Vector3 start, Vector3 control, Vector3 end, float t) // 计算起点、控制点、终点组成的二次贝塞尔曲线位置，t 限制到 0-1。
        { // 开始方法。
            t = Mathf.Clamp01(t); // 更新 曲线插值进度，使用 Mathf.Clamp01 的返回值。
            return (1 - t) * (1 - t) * start + 2 * (1 - t) * t * control + t * t * end; // 返回 (1 - t) * (1 - t) * start + 2 * (1 - t) * t * control + t * t * end。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
