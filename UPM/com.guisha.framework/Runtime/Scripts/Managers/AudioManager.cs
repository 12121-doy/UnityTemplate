using System.Collections; // 引入 System.Collections 命名空间，使用其中的类型与 API。
using System.Collections.Generic; // 引入 System.Collections.Generic 命名空间，使用其中的类型与 API。
using Guisha.Core; // 引入 Guisha.Core 命名空间，使用其中的类型与 API。
using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Managers // 将本文件类型归入 Guisha.Managers，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 常驻音频管理器，管理声部池、遮挡低通和 BGM 淡入淡出。
    /// </summary>
    public sealed class AudioManager : MonoSingleton<AudioManager> // 常驻音频管理器，管理声部池、遮挡低通和 BGM 淡入淡出。
    { // 开始类型。
        /// <summary>
        /// 池化声部，保存 AudioSource、低通滤镜和原音量。
        /// </summary>
        private sealed class Voice // 池化声部，保存 AudioSource、低通滤镜和原音量。
        { // 开始类型。
            /// <summary>
            /// 保存声部的 AudioSource 音源。
            /// </summary>
            public AudioSource Source; // 声明 声部的 AudioSource 音源，后续初始化。
            /// <summary>
            /// 保存遮挡低通滤镜。
            /// </summary>
            public AudioLowPassFilter Filter; // 声明 遮挡低通滤镜，后续初始化。
            /// <summary>
            /// 保存音量。
            /// </summary>
            public float Volume; // 声明 音量，后续初始化。
        } // 结束类型。

        /// <summary>
        /// 保存遮挡物检测层掩码。
        /// </summary>
        [SerializeField] // 将字段暴露给 Unity 序列化与 Inspector 配置。
        private LayerMask occlusionMask = ~0; // 把 ~0 保存为 遮挡物检测层掩码。
        /// <summary>
        /// 保存最大并发声部数。
        /// </summary>
        [SerializeField, Min(1)] // 将字段暴露给 Unity 序列化与 Inspector 配置。
        private int maxVoices = 32; // 把 32 保存为 最大并发声部数。
        /// <summary>
        /// 保存淡入淡出秒数。
        /// </summary>
        [SerializeField, Min(0.1f)] // 将字段暴露给 Unity 序列化与 Inspector 配置。
        private float fadeSeconds = 1; // 把 1 保存为 淡入淡出秒数。
        /// <summary>
        /// 保存可复用对象池。
        /// </summary>
        private ObjectPool<Voice> pool; // 声明 可复用对象池，后续初始化。
        /// <summary>
        /// 保存活动声部列表。
        /// </summary>
        private readonly List<Voice> active = new List<Voice>(); // 创建 List<Voice>，保存为 活动声部列表。
        /// <summary>
        /// 保存本组件的订阅令牌集合。
        /// </summary>
        private readonly SubscriptionBag subscriptions = new SubscriptionBag(); // 创建 SubscriptionBag，保存为 本组件的订阅令牌集合。
        /// <summary>
        /// 保存两个轮换使用的 BGM 声源。
        /// </summary>
        private AudioSource[] bgm; // 声明 两个轮换使用的 BGM 声源，后续初始化。
        /// <summary>
        /// 保存当前使用的 BGM 声源索引。
        /// </summary>
        private int currentBgm; // 声明 当前使用的 BGM 声源索引，后续初始化。
        /// <summary>
        /// 保存淡入淡出时长或协程句柄。
        /// </summary>
        private Coroutine fade; // 声明 淡入淡出时长或协程句柄，后续初始化。
        /// <summary>
        /// 保存音频监听器 Transform。
        /// </summary>
        private AudioListener listener; // 声明 音频监听器 Transform，后续初始化。
        /// <summary>
        /// 保存下次遮挡检测时间。
        /// </summary>
        private float nextOcclusion; // 声明 下次遮挡检测时间，后续初始化。
        /// <summary>
        /// 先建立主单例，再读取音频配置、创建声部池/BGM 声源并订阅音效请求。
        /// </summary>
        protected override void Awake() // 先建立主单例，再读取音频配置、创建声部池/BGM 声源并订阅音效请求。
        { // 开始方法。
            base.Awake(); // 执行基类 Awake，保留基类初始化流程。
            if (Instance != this) // 当当前类型的主单例实例不等于this时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            var settings = Resources.Load<FrameworkSettings>("Guisha/FrameworkSettings"); // 把 Resources.Load<FrameworkSettings> 的返回值 保存为 框架或动画配置。
            if (settings != null) // 当框架或动画配置不为空时，执行下方分支。
            { // 开始条件分支。
                occlusionMask = settings.audioOcclusionMask; // 更新 遮挡物检测层掩码，使用 settings.audioOcclusionMask。
                maxVoices = settings.audioMaxVoices; // 更新 最大并发声部数，使用 settings.audioMaxVoices。
                fadeSeconds = settings.bgmFadeSeconds; // 更新 淡入淡出秒数，使用 settings.bgmFadeSeconds。
            } // 结束条件分支。

            pool = new ObjectPool<Voice>(() => // 更新 可复用对象池，使用 新建的 ObjectPool<Voice> 对象。
            { // 开始回调。
                var go = new GameObject("PooledVoice"); // 创建 GameObject，保存为 go。
                go.transform.SetParent(transform); // 设置 Transform 父节点，将对象归到所属场景或实体层级。
                var source = go.AddComponent<AudioSource>(); // 把 go.AddComponent<AudioSource> 的返回值 保存为 当前 AudioSource 音源。
                source.playOnAwake = false; // 更新 source.playOnAwake，使用 false（关闭/不满足）。
                source.rolloffMode = AudioRolloffMode.Logarithmic; // 设置音源距离衰减曲线模式；值来自 AudioRolloffMode.Logarithmic。
                source.minDistance = 1; // 更新 source.minDistance，使用 1。
                source.maxDistance = 25; // 设置 3D 音源最远衰减距离；值来自 25。
                return new Voice // 返回 新建的 Voice 对象。
                { // 开始成员/集合初始化。
                    Source = source, // 更新 来源对象或音源，使用 当前 AudioSource 音源。
                    Filter = go.AddComponent<AudioLowPassFilter>() // 更新 遮挡低通滤镜，使用 go.AddComponent<AudioLowPassFilter> 的返回值。
                }; // 结束成员/集合初始化。
            }, v => v.Source.gameObject.SetActive(true), v => // 结束回调。
            { // 开始回调。
                v.Source.Stop(); // 停止播放 v.Source。
                v.Source.clip = null; // 清空 v.Source.clip 引用，表示当前不再持有该对象。
                v.Source.gameObject.SetActive(false); // 将 v.Source.gameObject 的激活状态设为 false，同时影响其组件生命周期。
            }, v => // 结束回调。
            { // 开始回调。
                if (v.Source != null) // 当v.Source不为空时，执行下方分支。
                    Destroy(v.Source.gameObject); // 请求 Unity 销毁指定对象，实际销毁遵循 Unity 生命周期。
            }, maxVoices); // 结束回调。
            bgm = new[] // 更新 两个轮换使用的 BGM 声源，使用 new[] {     gameObject.AddComponent<AudioSource>(),     gameObject.AddComponent<AudioSource>() }。
            { // 开始成员/集合初始化。
                gameObject.AddComponent<AudioSource>(), // 提供初始化/参数元素 gameObject.AddComponent<AudioSource>()，交给上方的对象或集合使用。
                gameObject.AddComponent<AudioSource>() // 提供初始化/参数元素 gameObject.AddComponent<AudioSource>()，交给上方的对象或集合使用。
            }; // 结束成员/集合初始化。
            foreach (var source in bgm) // 遍历 两个轮换使用的 BGM 声源，每次把一个元素保存为 source。
            { // 开始遍历循环。
                source.loop = true; // 设置音源是否循环播放；值来自 true（启用/满足）。
                source.playOnAwake = false; // 更新 source.playOnAwake，使用 false（关闭/不满足）。
                source.spatialBlend = 0; // 设置音源的 2D/3D 空间混合比例；值来自 0。
            } // 结束遍历循环。

            subscriptions.Add(EventBus.Subscribe<PlaySoundRequested>(Play)); // 订阅 PlaySoundRequested 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            subscriptions.Add(EventBus.Subscribe<BgmRequested>(e => // 订阅 BgmRequested 事件，并登记退订令牌以便生命周期结束时释放；回调在发布事件时执行。
            { // 开始回调。
                if (fade != null) // 当淡入淡出时长或协程句柄不为空时，执行下方分支。
                    StopCoroutine(fade); // 停止之前启动的指定协程。
                fade = StartCoroutine(CrossFade(e.Clip)); // 更新 淡入淡出时长或协程句柄，使用 StartCoroutine 的返回值。
            })); // 结束回调。
        } // 结束方法。

        /// <summary>
        /// 有音频且声部未满时从池借出声源并设置位置、混合、音量后播放。
        /// </summary>
        /// <param name="e">事件或异步完成载荷。</param>
        private void Play(PlaySoundRequested e) // 有音频且声部未满时从池借出声源并设置位置、混合、音量后播放。
        { // 开始方法。
            if (e.Clip == null || active.Count >= maxVoices) // 当（e.Clip为空；或active.Count不小于最大并发声部数）时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            var voice = pool.Get(); // 把 pool.Get 的返回值 保存为 voice。
            voice.Source.transform.position = e.Position; // 更新 voice.Source.transform.position，使用 e.Position。
            voice.Source.clip = e.Clip; // 更新 voice.Source.clip，使用 e.Clip。
            voice.Source.spatialBlend = Mathf.Clamp01(e.SpatialBlend); // 更新 voice.Source.spatialBlend，使用 Mathf.Clamp01 的返回值。
            voice.Volume = Mathf.Clamp01(e.Volume); // 更新 voice.Volume，使用 Mathf.Clamp01 的返回值。
            voice.Source.volume = voice.Volume; // 更新 voice.Source.volume，使用 voice.Volume。
            voice.Source.pitch = 1; // 更新 voice.Source.pitch，使用 1。
            voice.Source.loop = false; // 更新 voice.Source.loop，使用 false（关闭/不满足）。
            voice.Filter.cutoffFrequency = 22000; // 更新 voice.Filter.cutoffFrequency，使用 22000。
            voice.Source.Play(); // 开始播放 本声部 AudioSource。
            active.Add(voice); // 向 活动声部列表 加入 voice。
        } // 结束方法。

        /// <summary>
        /// 归还播放结束的声部，按间隔查找监听器并更新遮挡低通与音量。
        /// </summary>
        private void Update() // 归还播放结束的声部，按间隔查找监听器并更新遮挡低通与音量。
        { // 开始方法。
            for (int i = active.Count - 1; i >= 0; i--) // 按初始化与递增步骤循环；继续条件为 i >= 0。
                if (!active[i].Source.isPlaying) // 当条件 active[i].Source.isPlaying 不成立时，执行下方分支。
                { // 开始条件分支。
                    pool.Return(active[i]); // 校验对象属于本池且仍被借出；执行 onReturn，空闲容量不足或池已释放时执行 onDestroy。
                    active.RemoveAt(i); // 从 活动声部列表 移除 i。
                } // 结束条件分支。

            if (Time.unscaledTime < nextOcclusion) // 当当前非缩放时间小于下次遮挡检测时间时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            nextOcclusion = Time.unscaledTime + 0.15f; // 更新 下次遮挡检测时间，使用 Time.unscaledTime + 0.15f。
            if (listener == null) // 当音频监听器 Transform为空时，执行下方分支。
                listener = FindObjectOfType<AudioListener>(); // 更新当前音频监听器 Transform 缓存；值来自 FindObjectOfType<AudioListener> 的返回值。
            if (listener == null) // 当音频监听器 Transform为空时，执行下方分支。
                return; // 提前结束当前方法/回调，跳过后续处理。
            foreach (var voice in active) // 遍历 活动声部列表，每次把一个元素保存为 voice。
            { // 开始遍历循环。
                var delta = listener.transform.position - voice.Source.transform.position; // 把 listener.transform.position - voice.Source.transform.position 保存为 变化量。
                bool blocked = Physics.Raycast(voice.Source.transform.position, delta.normalized, delta.magnitude, occlusionMask, QueryTriggerInteraction.Ignore); // 把 Physics.Raycast 的返回值 保存为 是否存在阻挡。
                voice.Filter.cutoffFrequency = blocked ? 1500 : 22000; // 更新 voice.Filter.cutoffFrequency，使用 按条件 blocked 选择的值。
                voice.Source.volume = voice.Volume * (blocked ? 0.45f : 1); // 更新 voice.Source.volume，使用 voice.Volume * (blocked ? 0.45f : 1)。
            } // 结束遍历循环。
        } // 结束方法。

        /// <summary>
        /// 用两个 2D AudioSource 以非缩放时间淡出旧 BGM、淡入新 BGM。
        /// </summary>
        /// <param name="clip">音频或动画片段。</param>
        /// <returns>协程枚举器，由 Unity 协程系统或 Test Runner 逐步推进。</returns>
        private IEnumerator CrossFade(AudioClip clip) // 用两个 2D AudioSource 以非缩放时间淡出旧 BGM、淡入新 BGM。
        { // 开始方法。
            var from = bgm[currentBgm]; // 把 bgm[currentBgm] 保存为 from。
            var to = bgm[1 - currentBgm]; // 把 bgm[1 - currentBgm] 保存为 to。
            float fromVolume = from.volume; // 把 from.volume 保存为 fromVolume。
            to.Stop(); // 停止播放 to。
            to.clip = clip; // 更新 to.clip，使用 音频或动画片段。
            to.volume = 0; // 更新 to.volume，使用 0。
            if (clip != null) // 当音频或动画片段不为空时，执行下方分支。
                to.Play(); // 开始播放 to。
            for (float time = 0; time < fadeSeconds; time += Time.unscaledDeltaTime) // 按初始化与递增步骤循环；继续条件为 time < fadeSeconds。
            { // 开始计数循环。
                float t = time / fadeSeconds; // 把 time / fadeSeconds 保存为 曲线插值进度。
                from.volume = fromVolume * (1 - t); // 更新 from.volume，使用 fromVolume * (1 - t)。
                to.volume = t; // 更新 to.volume，使用 曲线插值进度。
                yield return null; // 暂停协程到下一帧，再从下一行继续。
            } // 结束计数循环。

            from.Stop(); // 停止播放 from。
            to.volume = 1; // 更新 to.volume，使用 1。
            currentBgm = 1 - currentBgm; // 更新 当前使用的 BGM 声源索引，使用 1 - currentBgm。
            fade = null; // 清空 淡入淡出时长或协程句柄 引用，表示当前不再持有该对象。
        } // 结束方法。

        /// <summary>
        /// 释放音频订阅、归还活动声部、清空对象池，最后清理主单例引用。
        /// </summary>
        protected override void OnDestroy() // 释放音频订阅、归还活动声部、清空对象池，最后清理主单例引用。
        { // 开始方法。
            subscriptions.Dispose(); // 幂等释放自身持有的注册、订阅或资源。
            if (pool != null) // 当可复用对象池不为空时，执行下方分支。
            { // 开始条件分支。
                foreach (var voice in active) // 遍历 活动声部列表，每次把一个元素保存为 voice。
                    pool.Return(voice); // 校验对象属于本池且仍被借出；执行 onReturn，空闲容量不足或池已释放时执行 onDestroy。
                active.Clear(); // 清空 活动声部列表 中保存的记录。
                pool.Dispose(); // 释放所有空闲对象；借出对象保持由调用方持有，之后 Return 时再销毁。
            } // 结束条件分支。

            base.OnDestroy(); // 执行基类销毁清理，保留单例引用等释放约定。
        } // 结束方法。
    } // 结束类型。
} // 结束命名空间。
