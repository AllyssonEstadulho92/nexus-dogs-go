using System;
using UnityEngine;

namespace NexusDogsGo.Gameplay.World
{
    public sealed class Map3DWorldAtmosphere : MonoBehaviour
    {
        [SerializeField] private Light sun;
        [SerializeField] private Camera mapCamera;
        [SerializeField] private bool useDeviceLocalTime = true;
        [SerializeField, Range(0f, 24f)] private float previewHour = 14f;
        [SerializeField, Min(1f)] private float refreshIntervalSeconds = 15f;

        private float _nextRefresh;

        public float CurrentHour { get; private set; }
        public float Daylight { get; private set; }

        public void Configure(Light directionalSun, Camera camera)
        {
            sun = directionalSun;
            mapCamera = camera;
            ApplyNow();
        }

        private void OnEnable()
        {
            ApplyNow();
        }

        private void Update()
        {
            if (Time.unscaledTime < _nextRefresh) return;
            _nextRefresh = Time.unscaledTime + refreshIntervalSeconds;
            ApplyNow();
        }

        public void SetPreviewHour(float hour)
        {
            useDeviceLocalTime = false;
            previewHour = Mathf.Repeat(hour, 24f);
            ApplyNow();
        }

        public void UseDeviceTime()
        {
            useDeviceLocalTime = true;
            ApplyNow();
        }

        public void ApplyNow()
        {
            CurrentHour = useDeviceLocalTime
                ? (float)DateTime.Now.TimeOfDay.TotalHours
                : previewHour;

            var sunWave = Mathf.Sin((CurrentHour - 6f) / 12f * Mathf.PI);
            Daylight = Mathf.Clamp01(sunWave);
            var dawn = Bell(CurrentHour, 7f, 2.1f);
            var sunset = Bell(CurrentHour, 18.8f, 2.2f);
            var warm = Mathf.Clamp01(dawn + sunset);

            var nightSky = new Color(0.012f, 0.025f, 0.055f, 1f);
            var daySky = new Color(0.42f, 0.68f, 0.84f, 1f);
            var goldenSky = new Color(0.83f, 0.46f, 0.28f, 1f);
            var sky = Color.Lerp(nightSky, daySky, Daylight);
            sky = Color.Lerp(sky, goldenSky, warm * 0.42f);

            var nightFog = new Color(0.018f, 0.035f, 0.060f, 1f);
            var dayFog = new Color(0.46f, 0.62f, 0.68f, 1f);
            var fog = Color.Lerp(nightFog, dayFog, Daylight * 0.82f);
            fog = Color.Lerp(fog, goldenSky, warm * 0.20f);

            if (sun != null)
            {
                var sunColor = Color.Lerp(
                    new Color(0.34f, 0.42f, 0.68f, 1f),
                    new Color(1f, 0.95f, 0.82f, 1f),
                    Daylight);
                sunColor = Color.Lerp(sunColor, new Color(1f, 0.56f, 0.32f, 1f), warm * 0.65f);
                sun.color = sunColor;
                sun.intensity = Mathf.Lerp(0.10f, 1.16f, Daylight);
                sun.shadows = Daylight > 0.12f ? LightShadows.Soft : LightShadows.None;

                var altitude = Mathf.Lerp(8f, 62f, Daylight);
                var azimuth = Mathf.Repeat((CurrentHour / 24f) * 360f - 110f, 360f);
                sun.transform.rotation = Quaternion.Euler(altitude, azimuth, 0f);
            }

            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = Color.Lerp(
                new Color(0.055f, 0.070f, 0.12f, 1f),
                new Color(0.40f, 0.46f, 0.43f, 1f),
                Daylight);
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Linear;
            RenderSettings.fogColor = fog;
            RenderSettings.fogStartDistance = Mathf.Lerp(13f, 25f, Daylight);
            RenderSettings.fogEndDistance = Mathf.Lerp(54f, 88f, Daylight);

            if (mapCamera != null) mapCamera.backgroundColor = sky;
        }

        private static float Bell(float hour, float center, float width)
        {
            var distance = Mathf.Abs(hour - center);
            distance = Mathf.Min(distance, 24f - distance);
            return Mathf.Clamp01(1f - distance / Mathf.Max(0.01f, width));
        }
    }
}
