using Guisha.Framework;
using UnityEngine;
namespace Guisha.Managers
{
    public sealed class AudioFacade : FacadeBase
    {
        public void Play3D(AudioClip clip, Vector3 position, float volume = 1) => Send(new PlaySoundRequested(clip, position, volume));
        public void PlayUI(AudioClip clip, float volume = 1) => Send(new PlaySoundRequested(clip, Vector3.zero, volume, 0));
        public void SetBgm(AudioClip clip) => Send(new BgmRequested(clip));
    }
}
