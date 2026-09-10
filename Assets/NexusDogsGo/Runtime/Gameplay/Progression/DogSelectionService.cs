using System;
using NexusDogsGo.Config;
using NexusDogsGo.Domain;

namespace NexusDogsGo.Gameplay.Progression
{
    public sealed class DogSelectionService
    {
        private readonly PlayerProfile _profile;

        public DogSelectionService(PlayerProfile profile)
        {
            _profile = profile ?? throw new ArgumentNullException(nameof(profile));
        }

        public DogInstance SelectStarter(string definitionId)
        {
            var definition = StarterCatalog.Find(definitionId);
            if (definition == null) throw new ArgumentException("Unknown dog definition: " + definitionId, nameof(definitionId));

            var existing = _profile.Dogs.Find(x => x.DefinitionId == definitionId);
            if (existing == null)
            {
                existing = new DogInstance
                {
                    DefinitionId = definitionId,
                    Level = 1,
                    CurrentHp = ProgressionSystem.CalculateMaxHp(1)
                };
                _profile.Dogs.Add(existing);
            }

            _profile.ActiveCompanionInstanceId = existing.InstanceId;
            return existing;
        }
    }
}
