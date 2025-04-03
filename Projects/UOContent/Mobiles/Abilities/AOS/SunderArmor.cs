using Server.Engines.BuffIcons;
using System;
using System.Xml.Linq;

namespace Server.Mobiles;

public class SunderArmor : MonsterAbilitySingleTargetDoT
{
    public override MonsterAbilityType AbilityType => MonsterAbilityType.ShatterArmor;

    public override MonsterAbilityTrigger AbilityTrigger => MonsterAbilityTrigger.GiveMeleeDamage;

    public override double ChanceToTrigger => 1.0;

    public override TimeSpan MinTriggerCooldown => TimeSpan.FromSeconds(10);

    public override TimeSpan MaxTriggerCooldown => TimeSpan.FromSeconds(20);
    public virtual TimeSpan MinDelay => TimeSpan.FromSeconds(duration);
    public virtual TimeSpan MaxDelay => TimeSpan.FromSeconds(duration);

    private double duration = 30;

    private const string Name = "SunderArmor";

    public override bool CanTrigger(BaseCreature source, MonsterAbilityTrigger trigger)
    {
        return true;
    }

    protected override void OnBeforeTarget(MonsterAbilityTrigger trigger, BaseCreature source, Mobile defender)
    {

    }

    protected override void OnTarget(MonsterAbilityTrigger trigger, BaseCreature source, Mobile defender)
    {
        base.OnTarget(trigger, source, defender);

        source.DoHarmful(defender);

        var value = (defender.PhysicalResistance * 15 / 100);

        var mod = new ResistanceMod(ResistanceType.Physical, Name, -value);
        defender.AddResistanceMod(mod);

        defender.FixedEffect(0x37B9, 10, 5);
        (defender as PlayerMobile)?.AddBuffStack(new BuffInfo(BuffIcon.ArmorPierce, 1075637, TimeSpan.FromSeconds(duration), null, false, 5));
    }

    protected override void EffectTick(BaseCreature source, Mobile defender, ref TimeSpan nextDelay)
    {
    }

    protected override void EndEffect(BaseCreature source, Mobile defender)
    {
        defender.RemoveResistanceMod(Name);
    }

    protected override void OnEffectExpired(BaseCreature source, Mobile defender)
    {
        defender.SendLocalizedMessage(1070838); // Your resistance to physical attacks has returned.
    }
}
