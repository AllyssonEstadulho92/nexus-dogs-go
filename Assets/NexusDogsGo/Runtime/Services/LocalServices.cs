using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using NexusDogsGo.Domain;
using UnityEngine;

namespace NexusDogsGo.Services
{
    public sealed class LocalGuestAuthService : IAuthService
    {
        private const string GuestIdKey = "nexus-dogs-go.guest-id";

        public Task<AuthSession> SignInAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var id = PlayerPrefs.GetString(GuestIdKey, string.Empty);
            if (string.IsNullOrWhiteSpace(id))
            {
                id = "guest-" + Guid.NewGuid().ToString("N");
                PlayerPrefs.SetString(GuestIdKey, id);
                PlayerPrefs.Save();
            }
            return Task.FromResult(new AuthSession(id, "Nuno"));
        }

        public Task SignOutAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }
    }

    public sealed class JsonFileDataStore : IDataStore
    {
        private readonly string _directory;

        public JsonFileDataStore(string directory = null)
        {
            _directory = directory ?? Path.Combine(Application.persistentDataPath, "profiles");
        }

        public Task<PlayerProfile> LoadProfileAsync(string playerId, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var path = PathFor(playerId);
            if (!File.Exists(path)) return Task.FromResult<PlayerProfile>(null);
            var json = File.ReadAllText(path);
            return Task.FromResult(JsonUtility.FromJson<PlayerProfile>(json));
        }

        public Task SaveProfileAsync(PlayerProfile profile, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Directory.CreateDirectory(_directory);
            var path = PathFor(profile.PlayerId);
            var tmp = path + ".tmp";
            var json = JsonUtility.ToJson(profile, true);
            File.WriteAllText(tmp, json);
            if (File.Exists(path)) File.Delete(path);
            File.Move(tmp, path);
            return Task.CompletedTask;
        }

        private string PathFor(string playerId)
        {
            foreach (var invalid in Path.GetInvalidFileNameChars()) playerId = playerId.Replace(invalid, '_');
            return Path.Combine(_directory, playerId + ".json");
        }
    }
}
