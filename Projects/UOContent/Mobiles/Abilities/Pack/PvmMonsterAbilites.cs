using Server.Logging;
using Server.Collections;
using Server.Items;
using Server.Misc;
using Server.Mobiles;
using Server.Spells;
using System;
using System.Collections.Generic;
using ModernUO.Serialization;
using Server.Engines.BuffIcons;
using System.Linq;
using Server.Spells.Spellweaving;
using Server.Spells.Fourth;

namespace Server.Custom.PvM
{
    /// <summary>
    ///     todo: refactor into seperate classes
    /// </summary>
    public partial class PvmMonsterAbilities
    {
        private static readonly ILogger _logger = LogFactory.GetLogger(typeof(PvmMonsterAbilities));

        public static void Configure()
        {
            // CommandSystem.Register("AbilityBlizzard", AccessLevel.Administrator, e => Blizzard(e.Mobile, e.Mobile.Location, 8, 1000, 10));
            // CommandSystem.Register("AbilityFirebomb", AccessLevel.Administrator, e => ThrowFirebomb(e.Mobile, new Point3D(e.Mobile.X, e.Mobile.Y + 6, e.Mobile.Z), true, TimeSpan.FromSeconds(10), null));
            // //CommandSystem.Register("AbilityBarrel", AccessLevel.Administrator, e => ThrowBarrel(e.Mobile, new Point3D(e.Mobile.X, e.Mobile.Y + 6, e.Mobile.Z), true, TimeSpan.FromSeconds(10), null));
            // //CommandSystem.Register("AbyssalRift", AccessLevel.Administrator, e => AbyssalRift(e.Mobile));
            // CommandSystem.Register("AbyssalNuke", AccessLevel.Administrator, e => AbyssalNuke(e.Mobile));
            // CommandSystem.Register("ChiefBelch", AccessLevel.Administrator, e => ChiefBelch(e.Mobile));
            // CommandSystem.Register("ChiefCough", AccessLevel.Administrator, e => ChiefCough(e.Mobile));
            // CommandSystem.Register("CrystalShards", AccessLevel.Administrator, e => CrystalShards(e.Mobile));
            // CommandSystem.Register("CrystalBleed", AccessLevel.Administrator, e => CrystalBleed(e.Mobile));
            // CommandSystem.Register("HoofStomp", AccessLevel.Administrator, e => HoofStomp(e.Mobile));
            // CommandSystem.Register("TeleportAttack", AccessLevel.Administrator, e => TeleportAttack(e.Mobile));
            // //CommandSystem.Register("CorruptVirtue", AccessLevel.Administrator, e => CorruptVirtue(e.Mobile));
            // //CommandSystem.Register("TakeLife", AccessLevel.Administrator, e => TakeLife(e.Mobile));
            // //CommandSystem.Register("AreaPainspike", AccessLevel.Administrator, e => AreaPainspike(e.Mobile));
            // //CommandSystem.Register("SilenceHowl", AccessLevel.Administrator, e => SilenceHowl(e.Mobile));
            // //CommandSystem.Register("SpillAcid", AccessLevel.Administrator, e => SpillAcid(e.Mobile, e.Mobile));
            // CommandSystem.Register("WaterWaves", AccessLevel.Administrator, e => WaterWaves(e.Mobile));
            // CommandSystem.Register("WaterClouds", AccessLevel.Administrator, e => WaterClouds(e.Mobile));
            // //CommandSystem.Register("SummonHelpers", AccessLevel.Administrator, e => SummonHelpers(e.Mobile));
            // CommandSystem.Register("PhantasmicMockery", AccessLevel.Administrator, e => PhantasmicMockery(e.Mobile));
            // CommandSystem.Register("ChillWinds", AccessLevel.Administrator, e => ChillWinds(e.Mobile));
            // CommandSystem.Register("TrapScattering", AccessLevel.Administrator, e => TrapScattering(e.Mobile));
            CommandSystem.Register("TrapScattering", AccessLevel.Administrator, e => TrapScattering(e.Mobile));
            CommandSystem.Register("HeartwarmingSong", AccessLevel.Administrator, e => HeartwarmingSong(e.Mobile));
        }

        public static void HeartwarmingSong(Mobile from)
        {
            if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
                return;

            from.PublicOverheadMessage(MessageType.Emote, 1154, true, $"*start screaming*");
            
            from.Animate(9, 5, 3, true, false, 0);
            from.PlaySound(0x2A);
            
            Timer.DelayCall(TimeSpan.FromSeconds(3.5), () =>
            {
                var summons = from.Map.GetMobilesInRange<BaseCreature>(from.Location, 15);

                foreach (var sum in summons)
                {
                    if (!sum.Alive || sum.SummonMaster?.Serial != from.Serial)
                    {
                        continue;
                    }

                    var length = TimeSpan.FromSeconds(30);
                    SpellHelper.AddStatBonus(from, sum, StatType.Str, length, false);
                    SpellHelper.AddStatBonus(from, sum, StatType.Dex, length);
                    SpellHelper.AddStatBonus(from, sum, StatType.Int, length);

                    sum.FixedParticles(0x373A, 10, 15, 5018, EffectLayer.Waist);
                    sum.PlaySound(0x1EA);
                }
            });
        }

        public static void TrapScattering(Mobile from)
        {
            if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
                return;

            from.PublicOverheadMessage(MessageType.Emote, 1154, true, $"*start thowing a traps*");

            Timer.DelayCall(TimeSpan.FromSeconds(1.5), () =>
            {
                var cnt = 10;
                var radius = 20;

                for (var i = 0; i < cnt; i++)
                {
                    // Random offset within ±radius
                    var offsetX = Utility.RandomMinMax(-radius, radius);
                    var offsetY = Utility.RandomMinMax(-radius, radius);

                    var newX = from.X + offsetX;
                    var newY = from.Y + offsetY;
                    var newZ = from.Map.GetAverageZ(newX, newY);
                    Point3D shardLoc = new Point3D(newX, newY, newZ);

                    // Pick a random crystal shard ID from 0x223A to 0x2249
                    var itemID = Utility.RandomMinMax(0x223A, 0x2249);

                    // Create the shard item at that location
                    var trap = new MushroomTrap();
                    trap.MoveToWorld(shardLoc, from.Map);
                }
            });

            // Optional: sound effect at the boss's location
            Effects.PlaySound(from.Location, from.Map, 0x2A1);
        }

        [SerializationGenerator(0, false)]
        public partial class TimedMushroomTrap : BaseTrap
        {
            public override int PassiveTriggerRange => 1;

            [Constructible]
            public TimedMushroomTrap() : base(0x1125)
            {
                var rng = Utility.RandomMinMax(10, 40);
                Timer.DelayCall(TimeSpan.FromSeconds(rng), Explode);
            }

            public override void OnTrigger(Mobile from)
            {
                if (!from.Alive || ItemID != 0x1125)// || from.AccessLevel > AccessLevel.Player)
                {
                    return;
                }
                SpellHelper.Damage(TimeSpan.FromSeconds(0.5), from, from, Utility.Dice(2, 4, 0));
                Explode();
            }

            private void Explode()
            {
                ItemID = 0x1126;
                Effects.PlaySound(Location, Map, 0x306);

                Timer.StartTimer(TimeSpan.FromSeconds(2.0), Delete);
            }
        }

        }
}
