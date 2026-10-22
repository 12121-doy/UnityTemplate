using Guisha.Framework; // 引入 Guisha.Framework 命名空间，使用其中的类型与 API。
using UnityEngine; // 引入 UnityEngine 命名空间，使用其中的类型与 API。

namespace Guisha.Managers // 将本文件类型归入 Guisha.Managers，避免类型命名冲突。
{ // 开始命名空间。
    /// <summary>
    /// 音频门面，封装 3D、UI 音效和 BGM 事件。
    /// </summary>
    public sealed class AudioFacade : FacadeBase // 音频门面，封装 3D、UI 音效和 BGM 事件。
    { // 开始类型。
        /// <summary>
        /// 发送空间音效请求。
        /// </summary>
        /// <param name="clip">音频或动画片段。</param>
        /// <param name="position">世界位置。</param>
        /// <param name="volume">音量，按播放入口钳制到有效范围。</param>
        public void Play3D(AudioClip clip, Vector3 position, float volume = 1) => Send(new PlaySoundRequested(clip, position, volume)); // 发送空间音效请求。
        /// <summary>
        /// 发送 spatialBlend=0 的界面音效请求。
        /// </summary>
        /// <param name="clip">音频或动画片段。</param>
        /// <param name="volume">音量。</param>
        public void PlayUI(AudioClip clip, float volume = 1) => Send(new PlaySoundRequested(clip, Vector3.zero, volume, 0)); // 发送 spatialBlend=0 的界面音效请求。
        /// <summary>
        /// 发送 BgmRequested。
        /// </summary>
        /// <param name="clip">音频或动画片段。</param>
        public void SetBgm(AudioClip clip) => Send(new BgmRequested(clip)); // 发送 BgmRequested。
    } // 结束类型。
} // 结束命名空间。
