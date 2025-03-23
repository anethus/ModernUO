using ModernUO.Serialization;
using System;

namespace Server.Items;

[SerializationGenerator(0, false)]
public partial class SpineTrap: BaseTrap
{
    private DateTime m_NextActiveTrigger;

    [Constructible]
    public SpineTrap(): base(0x1B71)
    {

    }

    public override void OnMovement(Mobile m, Point3D oldLocation)
    {
        if (m.Location == oldLocation)
        {
            return;
        }

        if (Core.Now < m_NextActiveTrigger)
        {
            return;
        }

        var range = CheckRange(m.Location, oldLocation, 3);

        if (range && !Visible)
        {
            Visible = true;
        }
        else if (!range)
        {
            Visible = false;
        }

        m_NextActiveTrigger = Core.Now + PassiveTriggerDelay;
    }
}
