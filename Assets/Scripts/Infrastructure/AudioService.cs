using System.Collections.Generic;
using UnityEngine;
using Zenject;

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
        
        private readonly AudioSource mainAudioSource = null;
        private readonly AudioSource soundAudioSource = null;
        private readonly Dictionary<SoundType, AudioClip> audioClips = new Dictionary<SoundType, AudioClip>();
        private readonly SignalBus signalBus = null;

        public AudioService(AudioSource mainAudioSource, AudioSource soundAudioSource,
            Dictionary<SoundType, AudioClip> audioClips, SignalBus signalBus)
        {
            this.mainAudioSource = mainAudioSource;
            this.soundAudioSource = soundAudioSource;
            this.audioClips = audioClips;
            this.signalBus = signalBus;

            mainAudioSource.clip = audioClips[SoundType.MainTheme];
            
            signalBus.Subscribe<LevelResetSignal>(StartMainTheme);
            signalBus.Subscribe<GameOverSignal>(() =>
            {
                StopMainTheme();
                PlaySound(SoundType.GameOverSound);
            });
            
            signalBus.Subscribe<LevelCompletedSignal>(() =>
            {
                StopMainTheme();
                PlaySound(SoundType.WinSound);
            });

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
