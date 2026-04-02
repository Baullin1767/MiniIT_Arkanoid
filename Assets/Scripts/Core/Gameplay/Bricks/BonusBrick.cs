using UnityEngine;

namespace MiniIT.ARKANOID
{
    public class BonusBrick : BrickBase
    {
        [SerializeField]
        private int bonusScore = 100;

        protected override int ResolveScoreReward(BrickImpactContext context)
        {
            return base.ResolveScoreReward(context) + bonusScore;
        }
    }
}
