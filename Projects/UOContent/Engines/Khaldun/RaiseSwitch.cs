using System;
using System.Linq;
using Server.Network;

namespace Server.Items
{
    public class RaiseSwitch : Item
    {
        private ResetTimer m_ResetTimer;

        [Constructible]
        public RaiseSwitch(int itemID = 0x1093) : base(itemID) => Movable = false;

        public RaiseSwitch(Serial serial) : base(serial)
        {
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public RaisableItem RaisableItem { get; set; }

        public override void OnDoubleClick(Mobile m)
        {
            if (!m.InRange(this, 2))
            {
                m.LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019045); // I can't reach that.
                return;
            }

            if (RaisableItem?.Deleted == true)
            {
                RaisableItem = null;
            }

            Flip();

            if (RaisableItem == null)
            {
                return;
            }

            if (RaisableItem.IsRaisable)
            {
                RaisableItem.Raise();
                m.LocalOverheadMessage(
                    MessageType.Regular,
                    0x5A,
                    true,
                    "You hear a grinding noise echoing in the distance."
                );
            }
            else
            {
                m.LocalOverheadMessage(
                    MessageType.Regular,
                    0x5A,
                    true,
                    "You flip the switch again, but nothing happens."
                );
            }
        }

        protected virtual void Flip()
        {
            if (ItemID != 0x1093)
            {
                ItemID = 0x1093;

                StopResetTimer();
            }
            else
            {
                ItemID = 0x1095;

                StartResetTimer(
                    RaisableItem?.CloseDelay >= TimeSpan.Zero ? RaisableItem.CloseDelay : TimeSpan.FromMinutes(2.0)
                );
            }

            Effects.PlaySound(Location, Map, 0x3E8);
        }

        protected void StartResetTimer(TimeSpan delay)
        {
            StopResetTimer();

            m_ResetTimer = new ResetTimer(this, delay);
            m_ResetTimer.Start();
        }

        protected void StopResetTimer()
        {
            if (m_ResetTimer != null)
            {
                m_ResetTimer.Stop();
                m_ResetTimer = null;
            }
        }

        protected virtual void Reset()
        {
            if (ItemID != 0x1093)
            {
                Flip();
            }
        }

        public override void Serialize(IGenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version

            writer.Write(RaisableItem);
        }

        public override void Deserialize(IGenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadEncodedInt();

            RaisableItem = (RaisableItem)reader.ReadEntity<Item>();

            Reset();
        }

        private class ResetTimer : Timer
        {
            private readonly RaiseSwitch m_RaiseSwitch;

            public ResetTimer(RaiseSwitch raiseSwitch, TimeSpan delay) : base(delay)
            {
                m_RaiseSwitch = raiseSwitch;
            }

            protected override void OnTick()
            {
                if (m_RaiseSwitch.Deleted)
                {
                    return;
                }

                m_RaiseSwitch.m_ResetTimer = null;

                m_RaiseSwitch.Reset();
            }
        }
    }

    public class DisappearingRaiseSwitch : RaiseSwitch
    {
        [Constructible]
        public DisappearingRaiseSwitch() : base(0x108F)
        {
        }

        public DisappearingRaiseSwitch(Serial serial) : base(serial)
        {
        }

        public int CurrentRange => Visible ? 3 : 2;

        public override bool HandlesOnMovement => true;

        protected override void Flip()
        {
        }

        protected override void Reset()
        {
        }

        public override void OnMovement(Mobile m, Point3D oldLocation)
        {
            if (Utility.InRange(m.Location, Location, CurrentRange) || Utility.InRange(oldLocation, Location, CurrentRange))
            {
                Refresh();
            }
        }

        public override void OnMapChange()
        {
            if (!Deleted)
            {
                Refresh();
            }
        }

        public override void OnLocationChange(Point3D oldLoc)
        {
            if (!Deleted)
            {
                Refresh();
            }
        }

        public void Refresh()
        {
            Visible = GetMobilesInRange(CurrentRange).Any(mob => !mob.Hidden || mob.AccessLevel <= AccessLevel.Player);
        }

        public override void Serialize(IGenericWriter writer)
        {
            if (RaisableItem?.Deleted == true)
            {
                RaisableItem = null;
            }

            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(IGenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadEncodedInt();

            Timer.DelayCall(Refresh);
        }
    }
}
