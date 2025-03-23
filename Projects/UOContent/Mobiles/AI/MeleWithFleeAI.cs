using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Mobiles;

public class MeleWithFleeAI : MeleeAI
{
    private BaseCreature helper;
    private long lastCheckTime;

    public int DistanceToCall = 3;

    public MeleWithFleeAI(BaseCreature bc) : base(bc)
    {

    }
    public override bool DoActionFlee()
    {
        if (m_Mobile.Combatant == null || !m_Mobile.Combatant.Alive || m_Mobile.Combatant.Map != m_Mobile.Map)
        {
            return base.DoActionFlee();
        }

        if (helper == null)
        {
            if (Core.TickCount - lastCheckTime > 5000)
            {
                m_Mobile.DebugSay("CZEKING FOR MOB");
                if (CheckForHelp())
                {
                    m_Mobile.DebugSay($"Dzieki {helper.Name} ZA POMOC!!");
                    GoToHelper();
                    return true;
                }
                else
                {
                    lastCheckTime = Core.TickCount;
                    return base.DoActionFlee();
                }
            }

            return base.DoActionFlee();
        }

        if (!helper.Alive || helper.Deleted || helper.Map != m_Mobile.Map)
        {
            m_Mobile.DebugSay($"Helper umrzyl!!");
            helper = null;
            lastCheckTime = Core.TickCount;
            return base.DoActionFlee();
        }

        if (helper.Combatant == m_Mobile.Combatant)
        {
            return base.DoActionFlee();
        }

        if (!m_Mobile.InRange(new Point2D(helper.Location), DistanceToCall))
        {
            GoToHelper();
            return true;
        }

        if (AskForHelp())
        {
            return true;
        }


        return base.DoActionFlee();
    }

    private bool CheckForHelp()
    {
        var mobiles = m_Mobile.GetMobilesInRange<BaseCreature>(m_Mobile.RangePerception);

        foreach(var mobile in mobiles)
        {
            if (mobile.Serial == m_Mobile.Serial)
            {
                continue;
            }

            if (mobile.Map != m_Mobile.Map)
            {
                continue;
            }

            if (m_Mobile.GetDistanceToSqrt(mobile) >= DistanceToCall * 2)
            {
                m_Mobile.DebugSay("MAM KOLEGE!!");
                helper = mobile;
                break;
            }
        }

        return helper != null;
    }

    private bool GoToHelper()
    {
        m_Mobile.DebugSay($"Ide to {helper.Name} PO POMOC!!");
        return MoveTo(helper, true, DistanceToCall);
    }

    private bool AskForHelp()
    {
        helper.Combatant = m_Mobile.Combatant;
        helper.Warmode = true;
        helper.AIObject.Action = ActionType.Combat;

        return true;
    }
}
