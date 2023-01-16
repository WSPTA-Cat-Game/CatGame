using UnityEngine;

namespace CatGame.Audio
{
    public class FadeAudio : MonoBehaviour
    {
        public bool fadeOnAudioEnd = true;
        public bool fadeOnAudioStart = true;
        public float fadeTime;

        public float minVolume = 0;
        public float maxVolume = 1;

        public AudioSource source;
        public AudioClip Clip
        {
            get => source.clip;
            set => source.clip = value;
        }

        public void Play()
        {
            source.volume = minVolume;
            source.Play();
        }

        private void Update()
        {
            if (source == null || source.clip == null || source.clip.length == 0)
            {
                return;
            }

            if (fadeOnAudioStart && source.time < fadeTime)
            {
                source.volume = Mathf.Lerp(minVolume, maxVolume, source.time / fadeTime);
            }
            else if (fadeOnAudioEnd && source.clip.length - source.time < fadeTime)
            {
                source.volume = Mathf.Lerp(maxVolume, minVolume, source.clip.length - source.time / fadeTime);
            }
            else
            {
                source.volume = maxVolume;
            }
        }
    }
}
