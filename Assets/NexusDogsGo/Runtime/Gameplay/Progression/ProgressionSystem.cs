using System;
using NexusDogsGo.Domain;

namespace NexusDogsGo.Gameplay.Progression
{
    public static class ProgressionSystem
    {
        public static int ExperienceRequiredForNextLevel(int level)
        {
            level = Math.Max(1, level);
            return checked(250 + (level - 1) * 125);
        }

        public static bool AddPlayerExperience(PlayerProfile profile, int amount)
        {
            if (amount <= 0) return false;
            profile.Experience = checked(profile.Experience + amount);
            var leveledUp = false;

            while (profile.Experience >= ExperienceRequiredForNextLevel(profile.Level))
            {
                profile.Experience -= ExperienceRequiredForNextLevel(profile.Level);
                profile.Level++;
                leveledUp = true;
            }

            return leveledUp;
        }

        public static void AddDogExperience(DogInstance dog, int amount)
        {
            if (amount <= 0) return;
            dog.Experience = checked(dog.Experience + amount);
            while (dog.Experience >= DogExperienceRequired(dog.Level))
            {
                dog.Experience -= DogExperienceRequired(dog.Level);
                dog.Level++;
                dog.CurrentHp = CalculateMaxHp(dog.Level);
            }
        }

        public static int DogExperienceRequired(int level) => checked(100 + Math.Max(1, level) * 35);
        public static int CalculateMaxHp(int level) => checked(90 + Math.Max(1, level) * 10);
    }
}
