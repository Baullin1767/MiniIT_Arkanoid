using System.Collections.Generic;
using UnityEngine;

namespace MiniIT.ARKANOID
{
    public class AudioService
    {
        public enum SoundType
        {
            MainTheme,
            WinSound,
            GameOverSound,
            HitSound,
            LaunchSound
        }
        
        private readonly AudioSource mainAudioSource;
        private readonly AudioSource soundAudioSource;
        private readonly Dictionary<SoundType, AudioClip> audioClips;

        public AudioService(AudioSource mainAudioSource, AudioSource soundAudioSource,
            Dictionary<SoundType, AudioClip> audioClips)
        {
            this.mainAudioSource = mainAudioSource;
            this.soundAudioSource = soundAudioSource;
            this.audioClips = audioClips;
            
            mainAudioSource.clip = audioClips[SoundType.MainTheme];
        }

        public void StartMainTheme()
        {
            mainAudioSource.Play();
        }

        public void StopMainTheme()
        {
            mainAudioSource.Stop();
        }

        public void PlaySound(SoundType soundType)
        {
            soundAudioSource.clip = audioClips[soundType];
            soundAudioSource.Play();
        }
    }
}
