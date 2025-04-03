namespace Server.Mobiles;

public class ThreatBossAI : MeleeAI
{
    public ThreatBossAI(BaseCreature m) : base(m)
    {
    }

    public override bool DoActionCombat()
    {
        if (m_Mobile is not RageCreature rc)
        {
            return base.DoActionCombat();
        }

        if (m_Mobile.Combatant != rc.GetTopRageMobile())
        {
            if (!AcquireFocusMob(m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true))
            {
                return base.DoActionCombat();
            }

            //m_Mobile.DebugSay($">> CHANGING TARGET BY RAGE: {m_Mobile.FocusMob.Name}");

            m_Mobile.Combatant = m_Mobile.FocusMob;
            return base.DoActionCombat();
        }

        return base.DoActionCombat();
    }

    public override bool AcquireFocusMob(int iRange, FightMode acqType, bool bPlayerOnly, bool bFacFriend, bool bFacFoe)
    {
        if (acqType == FightMode.RageLevel && m_Mobile is RageCreature rc)
        {
            if (rc.TauntedBy != null)
            {
                m_Mobile.FocusMob = rc.TauntedBy;
                return true;
            }

            var rageMob = rc.GetTopRageMobile();

            if (rageMob != null && rageMob.Alive && rageMob.Map == m_Mobile.Map)
            {
                m_Mobile.FocusMob = rageMob;
                return true;
            }
        }

        return base.AcquireFocusMob(iRange, acqType, bPlayerOnly, bFacFriend, bFacFoe);
    }
}


