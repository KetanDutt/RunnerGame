using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace RunnerGame
{
    /// <summary>
    /// Lightweight pooled particle effects created entirely in code
    /// (coin sparkles, hit bursts, landing dust, confetti).
    /// Attach to the player; effects are spawned in world space.
    /// </summary>
    public class ParticleFX : MonoBehaviour
    {
        public static ParticleFX Instance { get; private set; }

        private enum EffectType
        {
            Coin,
            Hit,
            Dust,
            Confetti
        }

        private class PooledEffect
        {
            public GameObject gameObject;
            public ParticleSystem system;
            public float availableAt;
        }

        [SerializeField] private int maxSystems = 24;

        private static readonly Color ColorGold = new Color(1f, 0.85f, 0.35f);
        private static readonly Color ColorHitA = new Color(1f, 0.3f, 0.2f);
        private static readonly Color ColorHitB = new Color(1f, 0.9f, 0.7f);
        private static readonly Color ColorDust = new Color(0.85f, 0.8f, 0.75f);

        private readonly List<PooledEffect> _pool = new List<PooledEffect>();
        private Material _softMaterial;
        private Material _additiveMaterial;
        private Texture2D _particleTexture;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            _particleTexture = CreateSoftParticleTexture();
            _softMaterial = CreateMaterial(BlendType.Soft);
            _additiveMaterial = CreateMaterial(BlendType.Additive);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            // Runtime-created GPU resources must be released explicitly.
            if (_softMaterial != null)
            {
                Destroy(_softMaterial);
            }

            if (_additiveMaterial != null)
            {
                Destroy(_additiveMaterial);
            }

            if (_particleTexture != null)
            {
                Destroy(_particleTexture);
            }
        }

        // ---- Public API ----

        public void BurstCoin(Vector3 worldPosition)
        {
            Emit(EffectType.Coin, worldPosition);
        }

        public void BurstHit(Vector3 worldPosition)
        {
            Emit(EffectType.Hit, worldPosition);
        }

        public void BurstDust(Vector3 worldPosition)
        {
            Emit(EffectType.Dust, worldPosition);
        }

        public void BurstConfetti()
        {
            var camera = Camera.main;
            Vector3 position = camera != null ? camera.transform.position + camera.transform.forward * 10f + Vector3.up * 2f : transform.position + Vector3.up * 3f;
            Emit(EffectType.Confetti, position);
        }

        // ---- Internals ----

        private void Emit(EffectType type, Vector3 worldPosition)
        {
            var effect = Acquire();
            if (effect == null)
            {
                return;
            }

            var ps = effect.system;
            ps.transform.position = worldPosition;
            ps.transform.rotation = Quaternion.identity;

            ApplyEffectSettings(type);
            ps.gameObject.SetActive(true);
            ps.Clear();
            ps.Play();
            ps.Emit(GetEmitCount(type));

            effect.availableAt = Time.unscaledTime + GetLifetime(type) + 0.5f;
            StartCoroutine(DeactivateAfter(effect, GetLifetime(type) + 0.2f));
        }

        private void ApplyEffectSettings(EffectType type)
        {
            var main = _currentMain;
            var emission = _currentEmission;
            var shape = _currentShape;
            var renderer = _currentRenderer;

            switch (type)
            {
                case EffectType.Coin:
                    main.startLifetime = 0.45f;
                    main.startSpeed = 2.6f;
                    main.startSize = 0.16f;
                    main.startColor = ColorGold;
                    main.gravityModifier = 3f;
                    renderer.material = _additiveMaterial;
                    shape.shapeType = ParticleSystemShapeType.Sphere;
                    shape.radius = 0.3f;
                    break;

                case EffectType.Hit:
                    main.startLifetime = 0.55f;
                    main.startSpeed = 5f;
                    main.startSize = 0.28f;
                    main.startColor = new Color32(255, 120, 90, 255);
                    main.gravityModifier = 4f;
                    renderer.material = _additiveMaterial;
                    shape.shapeType = ParticleSystemShapeType.Sphere;
                    shape.radius = 0.4f;
                    break;

                case EffectType.Dust:
                    main.startLifetime = 0.5f;
                    main.startSpeed = 1.6f;
                    main.startSize = 0.3f;
                    main.startColor = new Color(ColorDust.r, ColorDust.g, ColorDust.b, 0.55f);
                    main.gravityModifier = -0.4f;
                    renderer.material = _softMaterial;
                    shape.shapeType = ParticleSystemShapeType.Cone;
                    shape.radius = 0.5f;
                    shape.angle = 35f;
                    break;

                case EffectType.Confetti:
                    main.startLifetime = 1.3f;
                    main.startSpeed = 6f;
                    main.startSize = 0.14f;
                    main.startColor = Color.white;
                    main.gravityModifier = 5f;
                    renderer.material = _softMaterial;
                    shape.shapeType = ParticleSystemShapeType.Cone;
                    shape.radius = 1.2f;
                    shape.angle = 25f;
                    break;
            }

            emission.rateOverTime = 0;
        }

        private ParticleSystemMainModule _currentMain;
        private ParticleSystemEmissionModule _currentEmission;
        private ParticleSystemShapeModule _currentShape;
        private ParticleSystemRenderer _currentRenderer;

        private void PrepareModules(ParticleSystem ps)
        {
            _currentMain = ps.main;
            _currentEmission = ps.emission;
            _currentShape = ps.shape;
            _currentRenderer = ps.GetComponent<ParticleSystemRenderer>();
        }

        private int GetEmitCount(EffectType type)
        {
            switch (type)
            {
                case EffectType.Coin: return 14;
                case EffectType.Hit: return 22;
                case EffectType.Dust: return 10;
                case EffectType.Confetti: return 60;
                default: return 10;
            }
        }

        private float GetLifetime(EffectType type)
        {
            switch (type)
            {
                case EffectType.Coin: return 0.5f;
                case EffectType.Hit: return 0.6f;
                case EffectType.Dust: return 0.6f;
                case EffectType.Confetti: return 1.4f;
                default: return 0.6f;
            }
        }

        private PooledEffect Acquire()
        {
            float now = Time.unscaledTime;
            for (int i = 0; i < _pool.Count; i++)
            {
                var effect = _pool[i];
                if (!effect.gameObject.activeInHierarchy && effect.availableAt <= now)
                {
                    PrepareModules(effect.system);
                    return effect;
                }
            }

            if (_pool.Count >= maxSystems)
            {
                return null;
            }

            var go = new GameObject("FX_Particle");
            var ps = go.AddComponent<ParticleSystem>();
            ps.main.loop = false;
            ps.main.playOnAwake = false;
            ps.main.simulationSpace = ParticleSystemSimulationSpace.World;
            ps.main.stopAction = ParticleSystemStopAction.Stop;
            ps.main.maxParticles = 128;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbes = ParticleSystemLightProbes.Off;
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortMode = ParticleSystemSortMode.OldestInFront;

            go.SetActive(false);

            var effect = new PooledEffect
            {
                gameObject = go,
                system = ps,
                availableAt = 0f
            };
            _pool.Add(effect);

            PrepareModules(ps);
            return effect;
        }

        private IEnumerator DeactivateAfter(PooledEffect effect, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (effect.gameObject != null)
            {
                effect.system.Stop();
                effect.gameObject.SetActive(false);
            }
        }

        // ---- Material / texture helpers ----

        private enum BlendType
        {
            Soft,
            Additive
        }

        private Material CreateMaterial(BlendType blend)
        {
            var shader = Shader.Find("Sprites/Default");
            var material = new Material(shader);
            material.mainTexture = _particleTexture;

            if (blend == BlendType.Additive)
            {
                material.EnableKeyword("_ALPHADD_ON");
                material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)BlendMode.One);
            }
            else
            {
                material.EnableKeyword("_ALPHABLEND_ON");
                material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            }

            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.renderQueue = (int)RenderQueue.Transparent + 1;
            return material;
        }

        private static Texture2D CreateSoftParticleTexture()
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            var pixels = new Color[size * size];

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float dx = (x / (float)size) * 2f - 1f;
                    float dy = (y / (float)size) * 2f - 1f;
                    float distance = Mathf.Sqrt(dx * dx + dy * dy) * 0.7071f;
                    float alpha = Mathf.Clamp01(1f - distance);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha * alpha);
                }
            }

            texture.SetPixels(pixels);
            texture.Apply(false);
            return texture;
        }
    }
}
