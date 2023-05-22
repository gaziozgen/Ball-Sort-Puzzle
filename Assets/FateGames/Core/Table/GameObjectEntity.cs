using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FateGames.Core
{
    [CreateAssetMenu(menuName = "Fate/Table Entity/GameObject Entity")]
    public class GameObjectEntity : TableEntity
    {
        [SerializeField] private GameObject gameObject;

        public GameObject GameObject { get => gameObject; }
    }
}
