using BardicBytes.BardicFramework.EventVars;
using System.Collections.Generic;
using UnityEngine;

namespace BardicBytes.BardicPlatformer
{
    [CreateAssetMenu(menuName = Prefixes.Platformer + "EventVar: Movement Config")]
    public class PlatformMovementConfigEventVar : EvaluatingEventVar<int, PlatformMovementConfig>
    {
        [SerializeField]
        private List<PlatformMovementConfig> configs;

        public override PlatformMovementConfig Eval(int val) => configs[val];

        public override string ToString()
        {
            return string.Format("{1} ({0} [{2}])", name, Eval(initialValue).name, initialValue);
        }

        public override void SetInitialValue(EVInstData bc)
        {
            base.SetInitialValue(bc);
        }

        public override int To(BardicFramework.EventVars.EVInstData f) => f.IntValue;
#if UNITY_EDITOR
        protected override void SetInitialvalueOfInstanceConfig(int val, BardicFramework.EventVars.EVInstData config) => config.IntValue = val;
#endif
    }
}
