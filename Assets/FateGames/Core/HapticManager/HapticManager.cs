using Lofelt.NiceVibrations;
using UnityEngine;
namespace FateGames.Core
{
    public class HapticManager
    {
        private BoolVariable vibrationOn;

        public HapticManager(BoolVariable vibrationOn)
        {
            this.vibrationOn = vibrationOn;
        }

        public void PlayHaptic(HapticPatterns.PresetType presetType = HapticPatterns.PresetType.LightImpact)
        {
            if (!vibrationOn.Value) return;
            HapticPatterns.PlayPreset(presetType);
        }

    }
}