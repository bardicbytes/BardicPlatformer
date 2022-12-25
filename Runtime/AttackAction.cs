using BardicBytes.BardicFramework.Actions;
using BardicBytes.BardicFramework.Effects;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BardicBytes.BardicPlatformer
{
    public abstract class AttackAction : GenericAction<AttackAction, AttackPerformer, AttackRuntime>
    {
        public SpecialEffect attackFX;
    }
}
