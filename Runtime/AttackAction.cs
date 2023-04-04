using BardicBytes.BardicFramework.Actions;
using BardicBytes.BardicFramework.Effects;

namespace BardicBytes.BardicPlatformer
{
    public abstract class AttackAction : GenericAction<AttackAction, AttackPerformer, AttackRuntime>
    {
        public SpecialEffect attackFX;
    }
}
