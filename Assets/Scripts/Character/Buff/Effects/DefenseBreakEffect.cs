using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Endfield
{
    public class DefenseBreakEffect : IBuffEffect
    {
        public void OnApply(BuffInstance instance, IBuffTarget target)
        {
            EventCenter.DispatchMessage(new Events.OnDefenseBreakApplied());
        }

        public void OnRemove(BuffInstance instance, IBuffTarget target)
        {
            EventCenter.DispatchMessage(new Events.OnDefenseBreakConsumed { breakStack = instance.stackCount });
        }

        public void OnStackChanged(BuffInstance instance, IBuffTarget target, int deltaStack)
        {
            if(deltaStack > 0)
            {
                EventCenter.DispatchMessage(new Events.OnDefenseBreakApplied());
            }
        }

        public void OnTick(BuffInstance instance, IBuffTarget target, float deltaTime)
        {
            
        }
    }
}