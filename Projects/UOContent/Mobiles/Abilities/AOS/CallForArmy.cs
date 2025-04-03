using Server.Items;
using System;

namespace Server.Mobiles;

public class CallForArmy: MonsterAbility
{
    public override MonsterAbilityTrigger AbilityTrigger => MonsterAbilityTrigger.CombatAction;
    public override MonsterAbilityType AbilityType => MonsterAbilityType.CallForArmy;

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

        Timer.StartTimer(TimeSpan.FromSeconds(ThrowEffectDelay / 2), () => CallForArmy_Callback(source));
    }

    private void CallForArmy_Callback(BaseCreature source)
    {
        var waypoints = source.GetItemsInRange<WayPoint>(3).GetEnumerator();

        if (!waypoints.MoveNext())
        {
            if (source.Debug) { 
                source.DebugSay("No WAYPOINT!!");
            }
            return;
        }
        var point = waypoints.Current;


        foreach (var m in source.GetMobilesInRange<BaseCreature>(30))
        {
            if (m.Serial == source.Serial)
            {
                continue;
            }

            if (m.Map != source.Map)
            {
                continue;
            }

            if (source.Debug)
            {
                source.DebugSay($"Calling: {m.Name}");
            }

            if (m.AIObject.Action != ActionType.Wander)
            {
                continue;
            }

            m.CurrentWayPoint = point;
            m.AIObject.Action = ActionType.Backoff;
            m.MoveSpeedMod = 0.2;
            m.Warmode = true;
        }
    }

    private void StallMovement(BaseCreature source)
    {
        if (source.AIObject == null) return;

        source.AIObject.NextMove = Core.TickCount + (int)(StallTime * 1000);
    }

    private void PlayAngerSound(BaseCreature source)
    {
        source.PlaySound(0x2EB);
    }

    private void PlayAngerAnimation(BaseCreature source)
    {
        source.Animate(AnimationId, 5, 7, true, false, 0);
    }
}

