using UnityEngine;
using UnityEngine.Events;

namespace ChemistryGame
{
    public sealed class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance { get; private set; }
        [SerializeField] private AudioSource effects;
        [SerializeField] private AudioSource footstepsChannel;
        [SerializeField] private AudioClip footsteps, uiOpen, uiClose, buttonClick, correctAnswer, wrongAnswer, coinPickup, milestone;
        public UnityEvent onCorrect = new UnityEvent();
        public UnityEvent onWrong = new UnityEvent();
        private float nextStep;

        private void Awake()
        {
            Instance = this;
            if (effects == null) effects = gameObject.AddComponent<AudioSource>();
            if (footstepsChannel == null) footstepsChannel = gameObject.AddComponent<AudioSource>();
            effects.playOnAwake = footstepsChannel.playOnAwake = false;
            effects.volume = 0.35f;
            footstepsChannel.volume = 0.12f;
        }
        public void Open() => Play(uiOpen);
        public void Close() => Play(uiClose);
        public void Click() => Play(buttonClick);
        /// <summary>Soft cue when an object becomes the one [E] will use.</summary>
        public void Hover() { if (buttonClick != null) effects.PlayOneShot(buttonClick, 0.35f); }
        public void Correct() { Play(correctAnswer); onCorrect.Invoke(); }
        public void Wrong() { Play(wrongAnswer); onWrong.Invoke(); }
        public void Coin() => Play(coinPickup);
        public void Milestone() => Play(milestone);
        public void Step()
        {
            if (Time.time < nextStep) return;
            nextStep = Time.time + 0.12f;
            if (footsteps != null) footstepsChannel.PlayOneShot(footsteps);
        }
        private void Play(AudioClip clip) { if (clip != null) effects.PlayOneShot(clip); }
        private void OnDestroy() { if (Instance == this) Instance = null; }
    }
}
