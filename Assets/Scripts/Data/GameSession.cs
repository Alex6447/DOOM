using System;
using System.Collections.Generic;

namespace DOOM.Data
{
    [Serializable]
    public class GameSession
    {
        public string selectedCountryId;
        public int squadSize;
        public int score;
        public int currentWave;
        public float squadDamageMultiplier;
        public float squadDefenseMultiplier;
        public int missedUpgrades;
        public WeaponType currentWeaponType;
        public List<string> unlockedUpgrades = new List<string>();

        public GameSession()
        {
            squadSize = 5;
            score = 0;
            currentWave = 1;
            squadDamageMultiplier = 1f;
            squadDefenseMultiplier = 1f;
            missedUpgrades = 0;
            currentWeaponType = WeaponType.Rifle;
        }
    }

    [Serializable]
    public enum WeaponType
    {
        Pistol,
        Rifle,
        Shotgun,
        Sniper,
        RocketLauncher
    }

    [Serializable]
    public class Country
    {
        public string id;
        public string name;
        public string capital;
        public long population;
        public string flag;        // имя спрайта / emoji
        public string targetName;  // название финальной цели (дворец, здание)
    }
}
