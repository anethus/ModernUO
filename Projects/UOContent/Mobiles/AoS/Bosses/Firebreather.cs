using ModernUO.Serialization;

namespace Server.Mobiles.AoS.Bosses
{
    [SerializationGenerator(0, false)]
    public partial class Firebreather: BaseCreature
    {
        [Constructible]
        public Firebreather(): base(AIType.AI_BossWithSpecial, FightMode.Aggressor, 8)
        {
            Body = 808;
            BaseSoundID = 0xA3;
            Hue = 1964;

            SetStr(760, 1000);
            SetDex(560, 750);
            SetInt(110, 140);

            SetHits(4600, 6000);
            SetMana(0);

            SetDamage(4, 10);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 20, 25);
            SetResistance(ResistanceType.Cold, 10, 15);
            SetResistance(ResistanceType.Poison, 5, 10);

            SetSkill(SkillName.MagicResist, 20.1, 40.0);
            SetSkill(SkillName.Tactics, 40.1, 60.0);
            SetSkill(SkillName.Wrestling, 40.1, 60.0);

            Fame = 450;
            Karma = 0;

            VirtualArmor = 24;
        }

        public override string CorpseName => "a Firebreather corpse";
        public override string DefaultName => "Firebreather";
    }
}
