using UnityEngine;

namespace Game.StatusEffects.Effects
{
    public class PoisonEffect : BaseStatusEffects
    {
        public override string EffectID => "poison";
        public override float Duration => 8f;
        private const float DamagePerSecond = 5f;

        private PlayerHealth _playerHealth;

        public override void Apply(GameObject target)
        {
            Log.Info($"The player has been poisoned!");
        }

        public override void Tick(GameObject target)
        {
            if (target.TryGetComponent<PlayerHealth>(out var health))
            {
                health.Take(DamagePerSecond * Time.deltaTime);
            }
        }

        public override void Remove(GameObject target)
        {
            Log.Info($"{target.name} is no longer poisoned.");
        }
    }
}
