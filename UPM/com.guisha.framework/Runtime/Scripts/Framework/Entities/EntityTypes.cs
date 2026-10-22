using System; // 引入 System 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Framework // 将本文件类型归入 Guisha.Framework，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 枚举定义，可选值与实际源码一致。
    /// </summary>
    public enum EntityType // 枚举定义，可选值与实际源码一致。
    { // 开始枚举。
        /// <summary>
        /// 玩家实体。
        /// </summary>
        Player, // 玩家实体。
        /// <summary>
        /// 人类 NPC 实体。
        /// </summary>
        Human, // 人类 NPC 实体。
        /// <summary>
        /// 鬼怪实体。
        /// </summary>
        Ghost // 鬼怪实体。
    } // 结束枚举。

    /// <summary>
    /// 枚举定义，可选值与实际源码一致。
    /// </summary>
    public enum EntityId // 枚举定义，可选值与实际源码一致。
    { // 开始枚举。
        /// <summary>
        /// 玩家宁秋水的实体配置 ID。
        /// </summary>
        NingQiuShui, // 玩家宁秋水的实体配置 ID。
        /// <summary>
        /// 人类 NPC 良言的实体配置 ID。
        /// </summary>
        LiangYan, // 人类 NPC 良言的实体配置 ID。
        /// <summary>
        /// 人类 NPC 白潇潇的实体配置 ID。
        /// </summary>
        BaiXiaoXiao, // 人类 NPC 白潇潇的实体配置 ID。
        /// <summary>
        /// 人类 NPC 刘承峰的实体配置 ID。
        /// </summary>
        LiuChengFeng, // 人类 NPC 刘承峰的实体配置 ID。
        /// <summary>
        /// 人类 NPC 田勋的实体配置 ID。
        /// </summary>
        TianXun, // 人类 NPC 田勋的实体配置 ID。
        /// <summary>
        /// 通用幸存者 NPC 的实体配置 ID。
        /// </summary>
        Survivor, // 通用幸存者 NPC 的实体配置 ID。
        /// <summary>
        /// 红衣女鬼的实体配置 ID。
        /// </summary>
        RedGhost, // 红衣女鬼的实体配置 ID。
        /// <summary>
        /// 阮氏鬼怪的实体配置 ID。
        /// </summary>
        RuanShi, // 阮氏鬼怪的实体配置 ID。
        /// <summary>
        /// 人皮女鬼的实体配置 ID。
        /// </summary>
        SkinGhost, // 人皮女鬼的实体配置 ID。
        /// <summary>
        /// 无头僧人的实体配置 ID。
        /// </summary>
        HeadlessMonk // 无头僧人的实体配置 ID。
    } // 结束枚举。

    /// <summary>
    /// 枚举定义，可选值与实际源码一致。
    /// </summary>
    public enum EntityLifeState // 枚举定义，可选值与实际源码一致。
    { // 开始枚举。
        /// <summary>
        /// 实体仍然存活。
        /// </summary>
        Alive, // 实体仍然存活。
        /// <summary>
        /// 实体已经死亡。
        /// </summary>
        Dead // 实体已经死亡。
    } // 结束枚举。

    /// <summary>
    /// 可承受伤害的契约。
    /// </summary>
    public interface IDamageable // 可承受伤害的契约。
    { // 开始接口。
        /// <summary>
        /// 获取实体存活状态。
        /// </summary>
        bool IsAlive { get; } // 获取实体存活状态。

        /// <summary>
        /// 可受伤对象的统一伤害契约，由实现类型处理 DamageInfo。
        /// </summary>
        /// <param name="damage">伤害数据或承伤倍率。</param>
        void TakeDamage(DamageInfo damage); // 可受伤对象的统一伤害契约，由实现类型处理 DamageInfo。
    } // 结束接口。

    /// <summary>
    /// 不可变伤害载荷，记录伤害数值、来源和命中位置。
    /// </summary>
    public readonly struct DamageInfo // 不可变伤害载荷，记录伤害数值、来源和命中位置。
    { // 开始数据结构。
        /// <summary>
        /// 保存伤害数值。
        /// </summary>
        public readonly float Amount; // 声明 伤害数值，后续初始化。
        /// <summary>
        /// 保存来源对象或音源。
        /// </summary>
        public readonly GameObject Source; // 声明 来源对象或音源，后续初始化。
        /// <summary>
        /// 保存命中位置。
        /// </summary>
        public readonly Vector3 Point; // 声明 命中位置，后续初始化。
        /// <summary>
        /// 创建 DamageInfo 载荷，并按参数保存其字段。
        /// </summary>
        /// <param name="amount">本次伤害量。</param>
        /// <param name="source">原始来源。</param>
        /// <param name="point">待检测位置。</param>
        public DamageInfo(float amount, GameObject source, Vector3 point) // 构造 DamageInfo，初始化其依赖与状态。
        { // 开始构造函数。
            Amount = amount; // 更新 伤害数值，使用 本次伤害量。
            Source = source; // 更新 来源对象或音源，使用 原始来源。
            Point = point; // 更新 命中位置，使用 待检测位置。
        } // 结束构造函数。
    } // 结束数据结构。
} // 结束命名空间。
