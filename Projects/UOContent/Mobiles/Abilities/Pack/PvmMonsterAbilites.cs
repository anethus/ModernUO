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
using System.Drawing;
using Server.Targeting;
using System.Numerics;

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
            CommandSystem.Register("OrcCleave", AccessLevel.Administrator, e => OrcCleave(e.Mobile));
            CommandSystem.Register("CageTrapp", AccessLevel.Administrator, e => CageTrap(e.Mobile));

        }

        public static void CageTrap(Mobile from)
        {
            if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
                return;

            from.PublicOverheadMessage(MessageType.Emote, 1154, true, $"*looking for a person to trap*");

            Timer.DelayCall(TimeSpan.FromSeconds(1.5), () =>
            {
                using var pool = PooledRefQueue<Mobile>.Create();
                var eable = from.GetMobilesInRange(10);

                foreach (var target in eable)
                {
                    if (target.Serial == from.Serial || !target.Alive || target.Map == null || target.Deleted)
                    {
                        continue;
                    }

                    pool.Enqueue(target);
                }

                if (pool.Count <= 0)
                {
                    return;
                }

                var toFreeze = pool.PeekRandom();
                var mTrap = new PrisoneTrap(toFreeze);
                mTrap.MoveToWorld(toFreeze.Location, from.Map);
            });
        }

        [SerializationGenerator(0, false)]
        public partial class PrisoneTrap : BaseTrap
        {
            private int hp = 50;
            private int _freezeTimer = 0;
            private Mobile _freezePersone;

            public override int PassiveTriggerRange => 1;
            public override bool CanTarget => base.CanTarget;

            [Constructible]
            public PrisoneTrap(Mobile toFreeze) : base(0x21FC)
            {
                _freezeTimer = Utility.RandomMinMax(40, 140);
                
                Timer.DelayCall(TimeSpan.FromSeconds(_freezeTimer), Explode);

                _freezePersone = toFreeze;
                _freezePersone.Freeze(TimeSpan.FromSeconds(_freezeTimer));
            }

            public override void OnDelete()
            {
                base.OnAfterDelete();
                if (_freezePersone != null)
                {
                    _freezePersone.Frozen = false;
                }
            }

            private void Explode()
            {
                Effects.PlaySound(Location, Map, 0x306);

                Delete();
            }

            public void DoDamage()
            {
                hp -= 10;

                PublicOverheadMessage(MessageType.Emote, 0x33, true, "*start breaking a trap*");
                if (hp <= 0)
                {
                    Delete();
                }
            }
        }

        [SerializationGenerator(0, false)]
        public partial class SpearTrapBreaker : Item
        {
            public override bool CanEquip(Mobile m)
            {
                return false;
            }

            [Constructible]
            public SpearTrapBreaker() : base(0x2555)
            {

            }

            public override void OnDoubleClick(Mobile from)
            {
                base.OnDoubleClick(from);

                from.SendMessage("Target Prison");
                from.Target = new SpearItemTarget();
            }
        }


        public class SpearItemTarget : Target
        {
            public SpearItemTarget() : base(5, false, TargetFlags.None)
            {
            }

            protected override void OnTarget(Mobile from, object targeted)
            {
                if (targeted is PrisoneTrap pt)
                {
                    pt.DoDamage();
                }
            }
        }

        public static void OrcCleave(Mobile from)
        {
            if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
                return;

            from.PublicOverheadMessage(MessageType.Emote, 1154, true, $"*start swinging the sword*");
            from.Animate(9, 5, 3, true, false, 0);
            from.PlaySound(0x2A);

            Timer.DelayCall(TimeSpan.FromSeconds(3.5), () =>
            {
                new OrcCleaveTimer(from).Start();
            });
        }

        private class OrcCleaveTimer : Timer
        {
            private Mobile _from;
            public OrcCleaveTimer(Mobile from) : base(TimeSpan.FromSeconds(0), TimeSpan.FromSeconds(2), 30)
            {
                _from = from;
            }

            protected override void OnTick()
            {
                if (!_from.Alive || _from.Deleted)
                {
                    Stop();
                    return;
                }
                _from.Animate(9, 5, 3, true, false, 0);
                _from.PlaySound(0x2A);

                var position = new Vector2(_from.X, _from.Y);
                var dirVector = GetNormalizedLookDirection(_from.Direction);

                var enemies = _from.Map.GetMobilesInRange(_from.Location, 10);

                foreach (var enemy in enemies)
                {
                    if (enemy.Serial == _from.Serial)
                    {
                        continue;
                    }

                    var canSee = CheckObjectInFov(new Vector2(enemy.X, enemy.Y), position, dirVector, 45f);

                    if (canSee)
                    {
                        SpellHelper.Damage(TimeSpan.FromSeconds(0.5), _from, _from, Utility.Dice(2, 4, 0));
                    }
                }
            }
        }

        public static bool CheckObjectInFov(Vector2 targetPosition, Vector2 fromPosition, Vector2 vectorLookDirection, float viewRadius)
        {
            Vector2 vectorToObject = targetPosition - fromPosition;

            float angleToObjectRad = MathF.Atan2(vectorToObject.Y, vectorToObject.X) - MathF.Atan2(vectorLookDirection.Y, vectorLookDirection.X);

            while (angleToObjectRad > MathF.PI) angleToObjectRad -= 2 * MathF.PI;
            while (angleToObjectRad < -MathF.PI) angleToObjectRad += 2 * MathF.PI;

            float fieldOfViewRad = viewRadius * 0.5f * MathF.PI / 180f;

            return MathF.Abs(angleToObjectRad) <= fieldOfViewRad;
        }

        public static Vector2 GetNormalizedLookDirection(Direction direction)
        {
            return direction switch
            {
                Direction.North => new Vector2(0, -1),
                Direction.South => new Vector2(0, 1),
                Direction.West => new Vector2(-1, 0),
                Direction.East => new Vector2(1, 0),
                Direction.Down => new Vector2(1, 1),
                Direction.Up => new Vector2(-1, -1),
                Direction.Right => new Vector2(1, -1),
                Direction.Left => new Vector2(-1, 1),
                _ => Vector2.Zero,
            };
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
