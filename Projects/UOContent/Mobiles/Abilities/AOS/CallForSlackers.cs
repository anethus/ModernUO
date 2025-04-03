using Server.Engines.Spawners;
using System;

namespace Server.Mobiles;

public class CallForSlackers: MonsterAbility
{
    public override MonsterAbilityTrigger AbilityTrigger => MonsterAbilityTrigger.CombatAction;
    public override MonsterAbilityType AbilityType => MonsterAbilityType.CallForSlackers;

    public override TimeSpan MinTriggerCooldown => TimeSpan.FromSeconds(5.0);
    public override TimeSpan MaxTriggerCooldown => TimeSpan.FromSeconds(10.0);
    public double ThrowEffectDelay => 1.3;
    public double StallTime => 4;

    public int AnimationId = 4;

    public override bool CanTrigger(BaseCreature source, MonsterAbilityTrigger trigger) => !source.Summoned && base.CanTrigger(source, trigger);

    public override void Trigger(MonsterAbilityTrigger trigger, BaseCreature source, Mobile target)
    {
        StallMovement(source);

        Timer.StartTimer(TimeSpan.FromSeconds(ThrowEffectDelay / 2), () => StallEffect_Callback(source, target));
        base.Trigger(trigger, source, target);
    }

    private void StallEffect_Callback(BaseCreature source, Mobile target)
    {
        PlayAngerAnimation(source);
        PlayAngerSound(source);

        StallMovement(source);

        Timer.StartTimer(TimeSpan.FromSeconds(ThrowEffectDelay / 2), () => CallForSlacker_Callback(source));
    }

    private void CallForSlacker_Callback(BaseCreature source)
    {
        var spawners = source.GetItemsInRange<Spawner>(2).GetEnumerator();

        if (!spawners.MoveNext())
        {
            if (source.Debug)
            {
                source.DebugSay("No SPAWNER!!!!");
            }
            
            return;
        }
        var spawner = spawners.Current;

        if (!spawner.Running)
        {
            spawner.Respawn();
        }
    }

    public void StallMovement(BaseCreature source)
    {
        if (source.AIObject == null) return;

        source.AIObject.NextMove = Core.TickCount + (int)(StallTime * 1000);
    }

    public virtual void PlayAngerSound(BaseCreature source)
    {
        source.PlaySound(0x2EB);
    }

    public virtual void PlayAngerAnimation(BaseCreature source)
    {
        source.Animate(AnimationId, 5, 7, true, false, 0);
    }
}
