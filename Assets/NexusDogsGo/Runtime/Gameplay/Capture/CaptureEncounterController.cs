using System;
using NexusDogsGo.Core;
using NexusDogsGo.Gameplay.World;
using NexusDogsGo.UI.Navigation;
using UnityEngine;

namespace NexusDogsGo.Gameplay.Capture
{
    public sealed class CaptureEncounterController : MonoBehaviour
    {
        [SerializeField] private MapExplorationController mapController;
        [SerializeField] private ScreenNavigator navigator;
        [SerializeField] private CaptureBallType selectedBall = CaptureBallType.PokeBall;
        [SerializeField] private bool useFood;

        private CaptureGameService _captureService;

        public DogSpawn ActiveSpawn { get; private set; }
        public CaptureBallType SelectedBall => selectedBall;
        public bool UseFood => useFood;
        public bool HasEncounter => ActiveSpawn != null && ActiveSpawn.Dog != null;

        public event Action<DogSpawn> EncounterChanged;
        public event Action<CaptureAttemptOutcome> AttemptCompleted;
        public event Action<string> FeedbackChanged;

        private void OnEnable()
        {
            if (mapController != null)
            {
                mapController.SpawnSelected += BeginEncounter;
                if (mapController.SelectedSpawn != null) BeginEncounter(mapController.SelectedSpawn);
            }
            EnsureService();
        }

        private void OnDisable()
        {
            if (mapController != null) mapController.SpawnSelected -= BeginEncounter;
        }

        private void EnsureService()
        {
            var bootstrap = GameBootstrap.Instance;
            if (bootstrap != null)
                _captureService = new CaptureGameService(bootstrap.Profile, bootstrap.Missions);
        }

        public void BeginEncounter(DogSpawn spawn)
        {
            if (spawn == null || spawn.Dog == null) return;
            ActiveSpawn = spawn;
            useFood = false;
            EncounterChanged?.Invoke(spawn);
            SetFeedback(spawn.Dog.Name + " selvagem — nível " + spawn.Level + ".");
        }

        public void SelectPokeBall() => SelectBall(CaptureBallType.PokeBall);
        public void SelectSuperBall() => SelectBall(CaptureBallType.SuperBall);

        public void SelectBall(CaptureBallType ball)
        {
            selectedBall = ball;
            SetFeedback(ball == CaptureBallType.SuperBall ? "Super Bola selecionada." : "Poké Bola selecionada.");
        }

        public void ToggleFood()
        {
            useFood = !useFood;
            SetFeedback(useFood ? "Ração Premium preparada." : "Ração Premium removida.");
        }

        public void AttemptNormal() => Attempt(ThrowQuality.Normal);
        public void AttemptNice() => Attempt(ThrowQuality.Nice);
        public void AttemptGreat() => Attempt(ThrowQuality.Great);
        public void AttemptExcellent() => Attempt(ThrowQuality.Excellent);

        public void Attempt(ThrowQuality quality)
        {
            if (!HasEncounter)
            {
                SetFeedback("Nenhum cão selecionado para captura.");
                return;
            }

            if (_captureService == null) EnsureService();
            if (_captureService == null)
            {
                SetFeedback("Serviço de captura indisponível.");
                return;
            }

            var outcome = _captureService.Attempt(ActiveSpawn, selectedBall, quality, useFood);
            AttemptCompleted?.Invoke(outcome);

            switch (outcome.Status)
            {
                case CaptureAttemptStatus.Captured:
                    SetFeedback(ActiveSpawn.Dog.Name + " foi capturado!");
                    if (mapController != null) mapController.ConsumeSelectedSpawn();
                    ActiveSpawn = null;
                    if (GameBootstrap.Instance != null) _ = GameBootstrap.Instance.SaveAsync();
                    if (navigator != null) navigator.Show(ScreenId.Map);
                    break;
                case CaptureAttemptStatus.Escaped:
                    SetFeedback("O cão escapou. Tenta um lançamento melhor.");
                    break;
                case CaptureAttemptStatus.MissingBall:
                    SetFeedback("Não tens bolas deste tipo no inventário.");
                    break;
                case CaptureAttemptStatus.MissingFood:
                    useFood = false;
                    SetFeedback("Não tens Ração Premium disponível.");
                    break;
                default:
                    SetFeedback("Encontro inválido.");
                    break;
            }
        }

        public void Flee()
        {
            ActiveSpawn = null;
            if (mapController != null) mapController.ClearSelectedSpawn();
            EncounterChanged?.Invoke(null);
            if (navigator != null) navigator.Show(ScreenId.Map);
        }

        private void SetFeedback(string message)
        {
            FeedbackChanged?.Invoke(message);
            Debug.Log("[Capture] " + message, this);
        }
    }
}
