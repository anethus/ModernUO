using Server.Items;

namespace Server.Mobiles
{
    public class ScoutAI : BaseAI
    {
        private ScoutGong gongItem;
        private bool doesAlarm = false;
        private long spoteTimeTick = Core.TickCount;

        public ScoutAI(BaseCreature m) : base(m)
        {
        }

        public override bool DoActionWander()
        {
            if (m_Mobile.Combatant != null)
            {
                if (Core.TickCount - spoteTimeTick <= 3000)
                {
                    m_Mobile.Direction = m_Mobile.GetDirectionTo(m_Mobile.Combatant);
                    return true;
                }

                m_Mobile.NextReacquireTime = Core.TickCount;

                if (AcquireFocusMob(m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true))
                {
                    var gongItems = m_Mobile.GetItemsInRange<ScoutGong>(30);

                    var enumerator = gongItems.GetEnumerator();

                    if (!enumerator.MoveNext())
                    {
                        if (m_Mobile.Debug)
                        {
                            m_Mobile.DebugSay($"NO GONG SPOTED, FLEE!!!");
                        }

                        Action = ActionType.Flee;
                        return true;
                    }

                    gongItem = enumerator.Current;
                    Action = ActionType.Combat;
                    return true;
                }
            }

            if (AcquireFocusMob(m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true))
            {
                if (m_Mobile.Debug)
                {
                    m_Mobile.DebugSay($"ENEMY SPOTED: {m_Mobile.FocusMob.Name}");
                }

                m_Mobile.Combatant = m_Mobile.FocusMob;
                spoteTimeTick = Core.TickCount;
            }
            else
            {
                m_Mobile.Combatant = null;
            }

             return true;
        }

        public override bool DoActionCombat()
        {
            if (gongItem == null)
            {
                if (m_Mobile.Debug)
                {
                    m_Mobile.DebugSay("GONG IS NULL!!");
                }
                Action = ActionType.Flee;
                return true;
            }

            if (!m_Mobile.InRange(gongItem.GetWorldLocation(), 2))
            {
                DoMove(m_Mobile.GetDirectionTo(gongItem.GetWorldLocation(), true));

                return true;
            }

            if (gongItem.TrapType == TrapType.None)
            {
                m_Mobile.DebugSay("KURWA KTOS ZJEBAL ALARM!!!");
                Action = ActionType.Flee;
                return true;
            }

            if (!doesAlarm)
            {
                if (m_Mobile.TriggerAbility(MonsterAbilityTrigger.CombatAction, m_Mobile))
                {
                    if (m_Mobile.Debug)
                    {
                        m_Mobile.DebugSay($"CALL FOR ARMY!!!");
                    }
                }
                doesAlarm = true;
                Action = ActionType.Flee;
                return true;
            }

            Action = ActionType.Flee;
            return true;
        }
    }
}
