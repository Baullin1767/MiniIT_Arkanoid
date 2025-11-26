using System.Collections.Generic;
using MiniIT.ARKANOID;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

public class AudioServiceInstaller : MonoInstaller
{
    [Header("AudioSources")]
    [SerializeField] private AudioSource mainAudioSource = null;
    [SerializeField] private AudioSource soundAudioSource = null;
    [Header("Audio Clips")]
    [SerializeField] private AudioClip mainTheme = null;
    [SerializeField] private AudioClip winSound = null;
    [SerializeField] private AudioClip gameOverSound = null;
    [SerializeField] private AudioClip hitSound = null;
    [SerializeField] private AudioClip launchSound = null;
    public override void InstallBindings()
    {
        Dictionary<AudioService.SoundType, AudioClip> audioClips = 
            new Dictionary<AudioService.SoundType, AudioClip>
            {
                { AudioService.SoundType.MainTheme, mainTheme },
                { AudioService.SoundType.WinSound, winSound },
                { AudioService.SoundType.GameOverSound, gameOverSound },
                { AudioService.SoundType.HitSound, hitSound },
                { AudioService.SoundType.LaunchSound, launchSound }
            };
        
        Container.Bind<AudioService>().AsSingle()
            .WithArguments(
                mainAudioSource, 
                soundAudioSource,
                audioClips);
    }
}