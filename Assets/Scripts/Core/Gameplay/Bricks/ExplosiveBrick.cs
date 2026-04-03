using UnityEngine;

namespace MiniIT.ARKANOID
{
    public class ExplosiveBrick : BrickBase
    {
        [SerializeField] private GameObject animPref;
        protected override void OnDestroyed(BrickImpactContext context)
        {
            LevelManager?.QueueExplosion(GridPosition, context);
            Instantiate(animPref, transform.position, Quaternion.identity);
        }
    }
}
