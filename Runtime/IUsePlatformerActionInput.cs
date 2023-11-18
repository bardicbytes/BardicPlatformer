using BardicBytes.BardicFramework.Actions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BardicBytes.BardicPlatformer
{
    public interface IUsePlatformerActionInput
    {
        void ChangeInput(IProvideActionInput newInputSource);

    }
}
