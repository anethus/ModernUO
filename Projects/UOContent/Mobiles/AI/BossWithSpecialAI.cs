using Server.Engines.Spawners;
using Server.Misc;
using System;
using System.Collections.Generic;

namespace Server.Mobiles;

public abstract class SpecialPhase
{
    public bool Done { get; set; } = false;

    public virtual int Duration => 0;

    public abstract void ExecuteAction(BaseCreature source);
}

public class CallForArmyPhase: SpecialPhase
{
    public override int Duration => 25;
    private Spawner spawner;
    private bool inProgress = false;


    public override void ExecuteAction(BaseCreature source)
    {
        if (spawner == null)
        {
            var s = source.GetItemsInRange<Spawner>(20).GetEnumerator();

            if (!s.MoveNext())
            {
                source.DebugSay("No SPAWNER!!!!");
                source.AIObject.Action = ActionType.Combat;
                return;
            }
            spawner = s.Current;
        }

        if (!source.InRange(spawner.GetWorldLocation(), 2))
        {
            source.AIObject.DoMove(source.GetDirectionTo(spawner.GetWorldLocation(), true));

            return;
        }

        if (!inProgress)
        {
            inProgress = true;

            if (!spawner.Running)
            {
                spawner.Respawn();
            }

            StallMovement(source);
            Timer.StartTimer(TimeSpan.FromSeconds(2), () => StallEffect_Callback(source));
        }

        return;
    }

    private void StallEffect_Callback(BaseCreature source)
    {
        // Play animation attack
        source.Animate(4, 5, 7, true, true, 0);
        source.PlaySound(0x2EB);

        StallMovement(source);

        Timer.StartTimer(TimeSpan.FromSeconds(Duration), () => DurationEnd_Callback(source));
    }

    private void DurationEnd_Callback(BaseCreature source)
    {
        if (source == null || source.AIObject == null) return;

        source.AIObject.Action = ActionType.Combat;
    }

    private void StallMovement(BaseCreature source)
    {
        if (source.AIObject == null) return;

        source.AIObject.NextMove = Core.TickCount + (Duration * 1000);
    }
}

public class BossWithSpecialAI : MeleeAI
{
    private List<SpecialPhase> specialActions = new List<SpecialPhase>() { new CallForArmyPhase(), new CallForArmyPhase() };

    private int specialPahseIdx = -1;

    public override bool CheckFlee()
    {
        return false;
    }

    public virtual bool CheckForSpecialAction()
    {
        if (!specialActions[0].Done)
        {
            var a = (m_Mobile.Hits / m_Mobile.HitsMax);

            if ((double)m_Mobile.Hits / m_Mobile.HitsMax <= 0.66 ) {
                specialPahseIdx = 0;
                specialActions[0].Done = true;
                return true;
            }
        }

        if (!specialActions[1].Done)
        {
            if ((double)m_Mobile.Hits / m_Mobile.HitsMax <= 0.33)
            {
                specialPahseIdx = 1;
                specialActions[1].Done = true;
                return true;
            }
        }

        return false;
    }

    public BossWithSpecialAI(BaseCreature m) : base(m)
    {
    }

    public override bool DoActionCombat()
    {
        if (CheckForSpecialAction())
        {
            Action = ActionType.SpecialPhase;
            return true;
        }

        return base.DoActionCombat();
    }

    public override bool DoActionSpecialPhase()
    {
        if (specialPahseIdx < 0 || specialPahseIdx >= specialActions.Count)
        {
            Action = ActionType.Combat;
            return false;
        }

        specialActions[specialPahseIdx].ExecuteAction(m_Mobile);

        return true;
    }
}
