using System;
using System.Threading;
using System.Threading.Tasks;
using NexusDogsGo.Domain;
using NexusDogsGo.Gameplay.Missions;
using NexusDogsGo.Gameplay.World;
using NexusDogsGo.Services;
using UnityEngine;

namespace NexusDogsGo.Core
{
    public sealed class GameBootstrap : MonoBehaviour
    {
        public static GameBootstrap Instance { get; private set; }
        public PlayerProfile Profile { get; private set; } = new PlayerProfile();
        public GameStateMachine StateMachine { get; } = new GameStateMachine();
        public MissionTracker Missions { get; private set; } = new MissionTracker();
        public DogSpawnService Spawns { get; private set; } = new DogSpawnService();
        public ILocationProvider Location { get; private set; } = new UnityLocationProvider();
        public IAuthService Auth { get; private set; } = new LocalGuestAuthService();
        public IDataStore DataStore { get; private set; }

        private CancellationTokenSource _lifetime;

        private async void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            _lifetime = new CancellationTokenSource();
            DataStore = new JsonFileDataStore();

            try
            {
                await InitializeAsync(_lifetime.Token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private async Task InitializeAsync(CancellationToken cancellationToken)
        {
            var session = await Auth.SignInAsync(cancellationToken);
            var loaded = await DataStore.LoadProfileAsync(session.UserId, cancellationToken);
            Profile = loaded ?? new PlayerProfile
            {
                PlayerId = session.UserId,
                DisplayName = session.DisplayName
            };
            StateMachine.Set(GameState.Home);
        }

        public Task SaveAsync()
        {
            var token = _lifetime != null ? _lifetime.Token : CancellationToken.None;
            return DataStore.SaveProfileAsync(Profile, token);
        }

        private void OnApplicationPause(bool pause)
        {
            if (pause && !string.IsNullOrWhiteSpace(Profile.PlayerId) && DataStore != null) _ = SaveAsync();
        }

        private void OnApplicationQuit()
        {
            if (!string.IsNullOrWhiteSpace(Profile.PlayerId) && DataStore != null) _ = SaveAsync();
            Location.Stop();
            if (_lifetime != null)
            {
                _lifetime.Cancel();
                _lifetime.Dispose();
                _lifetime = null;
            }
        }
    }
}
