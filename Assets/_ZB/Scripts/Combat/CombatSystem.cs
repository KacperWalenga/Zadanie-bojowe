using UnityEngine;
using ZB;
using ZB.Mobs;

public class CombatSystem : MonoBehaviour
{

    public void StartCombat(MobData mobData)
    {
        EventManager.Trigger(Consts.Events.Combat.COMBAT_STARTED);
        
    }
}
