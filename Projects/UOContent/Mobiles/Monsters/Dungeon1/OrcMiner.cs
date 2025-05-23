using ModernUO.Serialization;

namespace Server.Mobiles;

[SerializationGenerator(0, false)]
public partial class OrcMiner: BaseCreature
{
    public override string DefaultName => "an orc master";

    [Constructible]
    public OrcMiner() : base(AIType.AI_BossWithSpecial)
    {
        Body = 7;
        BaseSoundID = 0x1B0;
        RangePerception = 10;
        Debug = true;

        SetStr(496, 525);
        SetDex(86, 105);
        SetInt(86, 125);

        SetHits(298, 315);

        SetDamage(2, 4);

        SetDamageType(ResistanceType.Physical, 40);
        SetDamageType(ResistanceType.Fire, 40);
        SetDamageType(ResistanceType.Energy, 20);

        SetResistance(ResistanceType.Physical, 55, 65);
        SetResistance(ResistanceType.Fire, 30, 40);
        SetResistance(ResistanceType.Cold, 30, 40);
        SetResistance(ResistanceType.Poison, 30, 40);
        SetResistance(ResistanceType.Energy, 20, 30);

        SetSkill(SkillName.MagicResist, 25.1, 30.0);
        SetSkill(SkillName.Tactics, 97.6, 100.0);
        SetSkill(SkillName.Wrestling, 80.5, 92.5);

        Fame = 14000;
        Karma = -14000;

        VirtualArmor = 60;

        Tamable = false;
        FollowersMax = 1;
    }

    private static MonsterAbility[] _abilities = { MonsterAbilities.SunderArmor };
    public override MonsterAbility[] GetMonsterAbilities() => _abilities;
}
