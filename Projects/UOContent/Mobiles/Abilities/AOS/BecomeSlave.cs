using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Mobiles;

public class BecomeSlave : MonsterAbility
{
    public override MonsterAbilityType AbilityType => MonsterAbilityType.BecomeSlave;
    public override MonsterAbilityTrigger AbilityTrigger => MonsterAbilityTrigger.Think;

    private int AmountToSummon => 1;

    public override bool CanTrigger(BaseCreature source, MonsterAbilityTrigger trigger) =>
    source.Followers < AmountToSummon && base.CanTrigger(source, trigger);
    public override void Trigger(MonsterAbilityTrigger trigger, BaseCreature source, Mobile target)
    {
        var loc = Utility.GetValidLocationInLOS(source.Map, source, 3);
        BaseCreature summon;

        summon = new Orc
        {
            Team = source.Team,
            Controlled = true,
            ControlOrder = OrderType.Follow,
            SummonMaster = source,
            FightMode = FightMode.Closest,
            Combatant = target,
            Home = source.Home,
            RangeHome = source.RangeHome
        };
        summon.MoveToWorld(loc, source.Map);
        Effects.SendLocationEffect(summon.Location, summon.Map, 0x3728, 10);
        summon.PlaySound(0x48F);
        summon.PlaySound(summon.GetAttackSound());

        base.Trigger(trigger, source, target);
    }
}

