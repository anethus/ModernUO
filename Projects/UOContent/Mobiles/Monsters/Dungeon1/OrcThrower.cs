using ModernUO.Serialization;
using Server.Ethics;

namespace Server.Mobiles;

[SerializationGenerator(0, false)]
public partial class OrcThrower : BaseCreature
{
    public override string DefaultName => "a orc thrower";

    [Constructible]
    public OrcThrower() : base(AIType.AI_Melee)
    {
        Body = 140;
        BaseSoundID = 0x1B0;

        SetStr(496, 525);
        SetDex(86, 105);
        SetInt(86, 125);

        SetHits(298, 315);

        SetDamage(16, 22);

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

        RangePerception = 10;

        VirtualArmor = 60;

        Tamable = false;
    }
    public override string CorpseName => "an orc thrower corpse";
    public override bool IsDispellable => false;
    public override bool IsBondable => false;
    public override FoodType FavoriteFood => FoodType.Meat;

    private static MonsterAbility[] _abilities = { MonsterAbilities.ThrowParalyzed };
    public override MonsterAbility[] GetMonsterAbilities() => _abilities;

    public override string ApplyNameSuffix(string suffix)
    {
        var ethic = Ethic.Evil;
        if (ethic == null)
        {
            return base.ApplyNameSuffix("");
        }

        var adjunct = ethic.Definition.Adjunct;

        if (suffix.Length == 0)
        {
            return base.ApplyNameSuffix(adjunct);
        }

        return base.ApplyNameSuffix($"{suffix} {adjunct}");
    }
}
