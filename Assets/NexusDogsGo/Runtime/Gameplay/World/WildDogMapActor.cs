using NexusDogsGo.Domain;
using UnityEngine;

namespace NexusDogsGo.Gameplay.World
{
    public sealed class WildDogMapActor : MonoBehaviour
    {
        private DogSpawn _spawn;
        private Transform _dogVisual;
        private Transform _head;
        private Transform _tail;
        private Transform _aura;
        private Renderer[] _dogRenderers;
        private Renderer[] _auraRenderers;
        private Vector3 _dogBasePosition;
        private Vector3 _auraBaseScale;
        private float _phase;

        public DogSpawn Spawn => _spawn;

        private void Awake()
        {
            CacheVisuals();
            _phase = Mathf.Abs(GetInstanceID() % 997) * 0.017f;
        }

        public void Bind(DogSpawn spawn)
        {
            _spawn = spawn;
            CacheVisuals();
            if (spawn == null || spawn.Dog == null) return;

            var size = Mathf.Lerp(0.88f, 1.16f, Mathf.InverseLerp(1f, 20f, spawn.Level));
            if (spawn.Dog.Rarity == DogRarity.Legendary) size *= 1.08f;
            if (_dogVisual != null) _dogVisual.localScale = Vector3.one * size;

            ApplyDogPalette(spawn.Dog.Id);
            ApplyRarityPalette(spawn.Dog.Rarity);
        }

        private void Update()
        {
            if (_dogVisual == null) return;

            var t = Time.unscaledTime + _phase;
            _dogVisual.localPosition = _dogBasePosition + Vector3.up * (Mathf.Sin(t * 2.15f) * 0.025f);

            if (_head != null)
            {
                var lookYaw = Mathf.Sin(t * 0.72f) * 9f;
                var lookPitch = Mathf.Sin(t * 1.05f) * 3f;
                _head.localRotation = Quaternion.Euler(lookPitch, lookYaw, 0f);
            }

            if (_tail != null)
            {
                var wag = Mathf.Sin(t * 7.2f) * 34f;
                _tail.localRotation = Quaternion.Euler(-22f, wag, 18f);
            }

            if (_aura != null)
            {
                var pulse = 1f + Mathf.Sin(t * 2.8f) * 0.075f;
                _aura.localScale = _auraBaseScale * pulse;
            }
        }

        private void CacheVisuals()
        {
            if (_dogVisual == null) _dogVisual = transform.Find("Visual/Dog");
            if (_dogVisual != null)
            {
                if (_head == null) _head = _dogVisual.Find("HeadPivot");
                if (_tail == null) _tail = _dogVisual.Find("Tail");
                _dogRenderers = _dogVisual.GetComponentsInChildren<Renderer>(true);
                _dogBasePosition = _dogVisual.localPosition;
            }

            if (_aura == null) _aura = transform.Find("Visual/Aura");
            if (_aura != null)
            {
                _auraRenderers = _aura.GetComponentsInChildren<Renderer>(true);
                _auraBaseScale = _aura.localScale;
            }
        }

        private void ApplyDogPalette(string dogId)
        {
            if (_dogRenderers == null) return;
            var color = GetDogColor(dogId);
            var secondary = Color.Lerp(color, Color.white, 0.28f);

            for (var i = 0; i < _dogRenderers.Length; i++)
            {
                var renderer = _dogRenderers[i];
                if (renderer == null) continue;
                var material = renderer.material;
                material.color = renderer.gameObject.name.Contains("Muzzle") || renderer.gameObject.name.Contains("Chest")
                    ? secondary
                    : color;
            }
        }

        private void ApplyRarityPalette(DogRarity rarity)
        {
            if (_auraRenderers == null) return;
            var color = RarityColor(rarity);
            for (var i = 0; i < _auraRenderers.Length; i++)
            {
                var renderer = _auraRenderers[i];
                if (renderer == null) continue;
                var material = renderer.material;
                material.color = color;
                if (material.HasProperty("_EmissionColor"))
                {
                    material.EnableKeyword("_EMISSION");
                    material.SetColor("_EmissionColor", color * (rarity == DogRarity.Legendary ? 2.4f : 1.35f));
                }
            }
        }

        private static Color GetDogColor(string id)
        {
            if (string.IsNullOrEmpty(id)) return new Color(0.62f, 0.48f, 0.34f, 1f);
            unchecked
            {
                uint hash = 2166136261u;
                for (var i = 0; i < id.Length; i++) hash = (hash ^ id[i]) * 16777619u;
                var hue = (hash % 1000u) / 1000f;
                return Color.HSVToRGB(hue, 0.38f, 0.82f);
            }
        }

        private static Color RarityColor(DogRarity rarity)
        {
            switch (rarity)
            {
                case DogRarity.Rare:
                    return new Color(0.12f, 0.78f, 1f, 1f);
                case DogRarity.Epic:
                    return new Color(0.74f, 0.25f, 1f, 1f);
                case DogRarity.Legendary:
                    return new Color(1f, 0.72f, 0.16f, 1f);
                default:
                    return new Color(0.18f, 0.92f, 0.54f, 1f);
            }
        }
    }
}
