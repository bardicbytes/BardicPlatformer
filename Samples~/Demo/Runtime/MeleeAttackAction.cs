using BardicBytes.BardicFramework.Effects;
using BardicBytes.BardicPlatformer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BardicBytes.BardicPlatformerSamples
{
    [CreateAssetMenu(menuName = "Bardic/Platformer/Melee Attack")]
    public class MeleeAttackAction : AttackAction
    {
        public override int PhaseDataCount => 1;

        public override PhaseData GetPhaseData(int i)
        {
            return new PhaseData() { name = "phase", duration = 0f };
        }

        public override AttackRuntime CreateRuntime(AttackPerformer actionPerformer)
        {
            return new AttackRuntime(this, actionPerformer);
        }
    }
}
