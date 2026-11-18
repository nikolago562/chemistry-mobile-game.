using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChemistryGame
{
    public sealed class VisualExperiment : MonoBehaviour
    {
        private Vector3 activeDesk;
        private Sprite bubble;
        private readonly List<SpriteRenderer> flasks = new List<SpriteRenderer>();
        public void Initialize(Vector3 teacherDesk)
        {
            bubble = PixelArt.Make(4, 4, (x, y) => x == 0 && y == 0 || x == 3 && y == 3 ? Color.clear : Color.white);
            AddApparatus(teacherDesk);
        }
        public void AddApparatus(Vector3 position)
        {
            var go = new GameObject("ექსპერიმენტის კოლბა", typeof(SpriteRenderer));
            go.transform.position = position;
            go.transform.localScale = Vector3.one * 0.55f;
            var renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = PixelArt.Flask();
            renderer.sortingOrder = 8;
            flasks.Add(renderer);
        }
        public void SetDesk(Vector3 position) => activeDesk = position + new Vector3(0, 2.5f);
        public void React(bool correct) => StartCoroutine(Animate(correct));
        private IEnumerator Animate(bool correct)
        {
            Color color = correct ? new Color(0.36f, 1f, 0.62f) : new Color(0.73f, 0.75f, 0.79f);
            var effects = new List<SpriteRenderer>();
            Vector3 origin = activeDesk == Vector3.zero ? flasks[0].transform.position : activeDesk;
            for (int i = 0; i < 18; i++)
            {
                var go = new GameObject(correct ? "ბუშტი" : "უვნებელი კვამლი", typeof(SpriteRenderer));
                go.transform.position = origin + new Vector3(Random.Range(-0.8f, 0.8f), Random.Range(-0.1f, 0.4f));
                go.transform.localScale = Vector3.one * (correct ? 1 : 2);
                var renderer = go.GetComponent<SpriteRenderer>();
                renderer.sprite = bubble;
                renderer.color = color;
                renderer.sortingOrder = 16;
                effects.Add(renderer);
            }
            for (float time = 0; time < 1.5f; time += Time.deltaTime)
            {
                foreach (var effect in effects)
                {
                    effect.transform.position += Vector3.up * Time.deltaTime * 1.4f;
                    effect.color = new Color(color.r, color.g, color.b, 1 - time / 1.5f);
                }
                foreach (var flask in flasks)
                    flask.color = Color.Lerp(Color.white, color, Mathf.Sin(time * 7) * 0.5f + 0.5f);
                yield return null;
            }
            foreach (var effect in effects) Destroy(effect.gameObject);
            foreach (var flask in flasks) flask.color = Color.white;
        }
    }
}
