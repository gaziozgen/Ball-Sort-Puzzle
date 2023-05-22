using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FateGames.Core
{
    [CreateAssetMenu(menuName = "Fate/Sound/Sound Entity")]
    public class SoundEntity : TableEntity
    {
        [SerializeField] private AudioClip clip;
        [SerializeField] private bool loop = false;
        [Range(0f, 1f)]
        [SerializeField] private float spatialBlend = 1;
        [Range(-3f, 3f)]
        [SerializeField] private float pitchRangeMin = 1, pitchRangeMax = 1;
        [Range(0f, 1f)]
        [SerializeField] private float volume = 1;

        public AudioClip Clip { get => clip; }
        public bool Loop { get => loop; }
        public float SpatialBlend { get => spatialBlend; }
        public float PitchRangeMin { get => pitchRangeMin; }
        public float PitchRangeMax { get => pitchRangeMax; }
        public float Volume { get => volume; }
    }
}
