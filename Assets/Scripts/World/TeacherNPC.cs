using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace ChemistryGame
{
    /// <summary>
    /// The chemistry teacher works at her own desk: she faces her bench and the chalkboard,
    /// turning between them now and then and scribbling as she works. When the player talks to
    /// her she says "one sec", finishes her note and turns smoothly to face them (stepping
    /// through the in-between directions of the 8-way walk set), then the conversation starts.
    /// While talking she bobs with each voice blip; a floating "!" / "?" marks waiting quests.
    /// </summary>
    public sealed class TeacherNPC : MonoBehaviour, ISpeaker
    {
        public bool Talking { get; set; }
        public float Voice => 1f;
        /// <summary>True while she finishes her work and turns around.</summary>
        public bool Busy { get; private set; }
        /// <summary>Facing in degrees: 0 east, 90 north (back to the camera), -90 south.</summary>
        public float Facing => angle;
        private const float TurnSpeed = 320;
        private static readonly int MoveX = Animator.StringToHash("MoveX"), MoveY = Animator.StringToHash("MoveY");
        private LabGame game;
        private Transform body;
        private Animator animator;
        private TMP_Text marker, bubble;
        private CanvasGroup nameplate;
        private float hop, talkBlend, angle = 30, workTarget = 30, nextWork, nextScribble;
        private bool facePlayer;

        public void Initialize(LabGame lab, SpriteRenderer art, TMP_Text questMarker, CanvasGroup label)
        {
            game = lab; body = art.transform; marker = questMarker; nameplate = label;
            // The student walk set has her back, side and front frames; its clips drive a child called "Sprite".
            var source = lab.player != null ? lab.player.GetComponent<Animator>() : null;
            if (source != null && source.runtimeAnimatorController != null)
            {
                body.name = "Sprite";
                animator = gameObject.AddComponent<Animator>();
                animator.runtimeAnimatorController = source.runtimeAnimatorController;
                animator.SetBool("IsMoving", false);
            }
            PersonalLab.Label(transform, "ერთი წამით...", new Vector2(0, 2.9f), LabUI.Paper);
            bubble = transform.Find("ერთი წამით...").GetComponentInChildren<TMP_Text>();
            bubble.alpha = 0;
        }

        /// <summary>Called for each voiced syllable.</summary>
        public void Pulse() => hop = 1;

        /// <summary>"One sec": finishes her note, turns to the player, then runs <paramref name="then"/>.</summary>
        public void Attend(Action then)
        {
            if (Busy) return;
            if (Talking) { then?.Invoke(); return; }
            StartCoroutine(TurnAround(then));
        }

        private IEnumerator TurnAround(Action then)
        {
            Busy = true;
            bubble.alpha = 1;
            AudioManager.Instance?.Hover();
            // A couple of last scribbles before she looks up.
            for (float t = 0; t < 0.55f; t += Time.unscaledDeltaTime) { if (t % 0.25f < Time.unscaledDeltaTime) hop = 0.6f; yield return null; }
            facePlayer = true;
            while (Mathf.Abs(Mathf.DeltaAngle(angle, AngleToPlayer())) > 8) yield return null;
            bubble.alpha = 0;
            Busy = false;
            then?.Invoke();
        }

        private float AngleToPlayer()
        {
            if (game == null || game.player == null) return -90;
            Vector2 d = game.player.transform.position - transform.position;
            return d.sqrMagnitude < 0.01f ? -90 : Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
        }

        private void Update()
        {
            float t = Time.unscaledTime, dt = Time.unscaledDeltaTime;
            if (Talking) facePlayer = true;
            else if (!Busy) facePlayer = false;

            // Work: face the bench (east) or the chalkboard (north-east), switching every few seconds.
            if (!facePlayer && t > nextWork)
            {
                nextWork = t + UnityEngine.Random.Range(4f, 7f);
                workTarget = workTarget > 20 ? 0 : 50;
            }
            if (!facePlayer && t > nextScribble)
            {
                nextScribble = t + UnityEngine.Random.Range(0.35f, 0.9f);
                hop = Mathf.Max(hop, 0.35f);
            }
            angle = Mathf.MoveTowardsAngle(angle, facePlayer ? AngleToPlayer() : workTarget, TurnSpeed * dt);
            if (animator != null)
            {
                animator.SetFloat(MoveX, Mathf.Cos(angle * Mathf.Deg2Rad));
                animator.SetFloat(MoveY, Mathf.Sin(angle * Mathf.Deg2Rad));
            }

            talkBlend = Mathf.MoveTowards(talkBlend, Talking ? 1 : 0, dt * 4);
            hop = Mathf.MoveTowards(hop, 0, dt * 9);
            float bounce = Mathf.Sin(hop * Mathf.PI) * (0.03f + 0.06f * talkBlend);
            float breathe = Mathf.Sin(t * 2.1f) * 0.012f;
            float stretch = breathe + bounce * 0.6f;
            body.localScale = new Vector3(1 - stretch * 0.5f, 1 + stretch, 1);
            body.localPosition = new Vector3(0, bounce, 0);
            // Gentle side-to-side sway, as if she gestures while explaining.
            body.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 3.3f) * 3.2f * talkBlend);

            if (nameplate != null) nameplate.alpha = 1 - talkBlend;
            if (marker == null) return;
            var data = GameSaveManager.Instance.Data;
            var quest = TeacherQuests.Active;
            string symbol = !data.tutorialCompleted ? "" : TeacherGuide.Next != null || quest == null ? "!" : TeacherQuests.Done(quest) ? "?" : "";
            if (marker.text != symbol) marker.text = symbol;
            marker.color = symbol == "?" ? LabUI.Mint : LabUI.Gold;
            marker.alpha = Busy ? 0 : 1 - talkBlend;
            marker.rectTransform.localPosition = new Vector3(0, Mathf.Abs(Mathf.Sin(t * 2.6f)) * 9f, 0);
        }
    }
}
