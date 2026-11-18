using UnityEngine;

namespace ZB.Mobs
{
    [CreateAssetMenu(fileName = "MobData", menuName = "ZB/MobData")]
    public class MobData : ScriptableObject
    {
        [field: Header("Config")]
        [field: SerializeField] public MobModel Model { get; private set; }
        [field: SerializeField] public Mob MobTemplate { get; private set; }
        
        [field: Header("Statistics")]
        [field: SerializeField] public int Damage { get; private set; }
        [field: SerializeField] public float MinAttackCooldown { get; private set; }
        [field: SerializeField] public float MaxAttackCooldown { get; private set; }
        
        [field: Header("Offset")]
        [field: SerializeField] public Vector3 Offset { get; private set; } = new (-0.5f, 0.5f, 0.5f);
        [field: SerializeField] public Vector3 HiddenOffset { get; private set; } = new (0f, 1f, 0f);
        
        public void Spawn(Vector2Int position)
        {
            var mob = Instantiate(MobTemplate);
            mob.Init(this, position);

        }
    }
}