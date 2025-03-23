using ModernUO.Serialization;

namespace Server.Items;


[SerializationGenerator(0, false)]
public partial class ScoutGong: BaseTrap
{
    [SerializableField(0)]
    [SerializedCommandProperty(AccessLevel.GameMaster)]
    private int _trapLevel;

    [SerializableField(1)]
    [SerializedCommandProperty(AccessLevel.GameMaster)]
    private int _trapPower;

    [SerializableField(2)]
    [SerializedCommandProperty(AccessLevel.GameMaster)]
    private TrapType _trapType;

    [Constructible]
    public ScoutGong() : base(0x0420)
    {
        Movable = false;
        Weight = 20.0;

        TrapPower = 10;
        TrapLevel = 2;

        TrapType = TrapType.Alarm;
    }
}
