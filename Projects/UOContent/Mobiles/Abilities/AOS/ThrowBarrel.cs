using Server.Items;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Mobiles;

public class ThrowBarrel: MonsterAbility
{
    public override TimeSpan MinTriggerCooldown => TimeSpan.FromMinutes(1);
    public override TimeSpan MaxTriggerCooldown => TimeSpan.FromSeconds(90);

    public override MonsterAbilityTrigger AbilityTrigger => MonsterAbilityTrigger.CombatAction;
    public override MonsterAbilityType AbilityType => MonsterAbilityType.ThrowBarrel;
    public override double ChanceToTrigger => 1.0;
    public override bool CanTrigger(BaseCreature source, MonsterAbilityTrigger trigger) => true;

    public override void Trigger(MonsterAbilityTrigger trigger, BaseCreature source, Mobile target)
    {
        if (source == null || source.Map == null || !source.Alive || target is not IPoint3D targetPoint)
        {
            return;
        }

        //    if (from == null || from.Map == null || !from.Alive || target is not IPoint3D targetPoint)
        //    {
        //        return;
        //    }

        //    var explosiveBarrel = new ExplosiveBarrel(createFlamesOnTarget, flamesDuration, createdFireItemAction);
        //    BaseExplosionPotion.ExplosionPotionThrowTarget.Target(from, target, explosiveBarrel);
    }


}
