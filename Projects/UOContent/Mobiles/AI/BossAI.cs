using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Mobiles
{
    public class BossAI : BaseAI
    {
        public BossAI(BaseCreature m) : base(m)
        {
            m_Mobile.PublicOverheadMessage(MessageType.Regular, 44, false, ">> BossAI Generated <<");
        }

        public override bool DoActionWander()
        {
            if (AcquireFocusMob(m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true))
            {
                if (m_Mobile.Debug)
                {
                    m_Mobile.DebugSay($"I have detected {m_Mobile.FocusMob.Name}, attacking");
                }
                m_Mobile.Combatant = m_Mobile.FocusMob;
                Action = ActionType.Combat;
            }

            return base.DoActionWander();
        }

        public override bool DoActionCombat()
        {
            var combatant = m_Mobile.Combatant;

            if (combatant == null || combatant.Deleted || combatant.Map != m_Mobile.Map || !combatant.Alive ||
                combatant.IsDeadBondedPet)
            {
                if (m_Mobile.Debug)
                {
                    m_Mobile.DebugSay("My combatant is gone, so my guard is up");
                }

                Action = ActionType.Guard;
                return true;
            }

            if (!m_Mobile.InRange(combatant, m_Mobile.RangePerception))
            {
                // They are somewhat far away, can we find something else?

                if (AcquireFocusMob(m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true))
                {
                    m_Mobile.Combatant = m_Mobile.FocusMob;
                    m_Mobile.FocusMob = null;
                }
                else if (!m_Mobile.InRange(combatant, m_Mobile.RangePerception * 3))
                {
                    m_Mobile.Combatant = null;
                }

                combatant = m_Mobile.Combatant;

                if (combatant == null)
                {
                    if (m_Mobile.Debug)
                    {
                        m_Mobile.DebugSay("My combatant has fled, so I am on guard");
                    }

                    Action = ActionType.Guard;
                    return true;
                }
            }

            if (!MoveTo(combatant, true, m_Mobile.RangeFight))
            {
                m_Mobile.Direction = m_Mobile.GetDirectionTo(combatant);
                if (AcquireFocusMob(m_Mobile.RangePerception, m_Mobile.FightMode, false, false, true))
                {
                    if (m_Mobile.Debug)
                    {
                        m_Mobile.DebugSay($"My move is blocked, so I am going to attack {m_Mobile.FocusMob!.Name}");
                    }

                    m_Mobile.Combatant = m_Mobile.FocusMob;
                    Action = ActionType.Combat;
                    return true;
                }

                if (m_Mobile.GetDistanceToSqrt(combatant) > m_Mobile.RangePerception + 1)
                {
                    if (m_Mobile.Debug)
                    {
                        m_Mobile.DebugSay($"I cannot find {combatant.Name}, so my guard is up");
                    }

                    Action = ActionType.Guard;
                    return true;
                }

                if (m_Mobile.Debug)
                {
                    m_Mobile.DebugSay($"I cannot find {combatant.Name}, so my guard is up");
                }
            }
            else if (Core.TickCount - m_Mobile.LastMoveTime > 400)
            {
                m_Mobile.Direction = m_Mobile.GetDirectionTo(combatant);
            }

            if (!m_Mobile.Controlled && !m_Mobile.Summoned && m_Mobile.CanFlee)
            {
                if (m_Mobile.Hits < m_Mobile.HitsMax * 20 / 100)
                {
                    Action = ActionType.SpecialPhase;
                    return true;
                }
            }

            if (m_Mobile.TriggerAbility(MonsterAbilityTrigger.CombatAction, combatant))
            {
                if (m_Mobile.Debug)
                {
                    m_Mobile.DebugSay("I used my abilities!");
                }
            }

            return true;
        }
    }
}
