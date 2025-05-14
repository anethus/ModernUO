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
            CommandSystem.Register("AbilityBlizzard", AccessLevel.Administrator, e => Blizzard(e.Mobile, e.Mobile.Location, 8, 1000, 10));
            CommandSystem.Register("AbilityFirebomb", AccessLevel.Administrator, e => ThrowFirebomb(e.Mobile, new Point3D(e.Mobile.X, e.Mobile.Y + 6, e.Mobile.Z), true, TimeSpan.FromSeconds(10), null));
            //CommandSystem.Register("AbilityBarrel", AccessLevel.Administrator, e => ThrowBarrel(e.Mobile, new Point3D(e.Mobile.X, e.Mobile.Y + 6, e.Mobile.Z), true, TimeSpan.FromSeconds(10), null));
            //CommandSystem.Register("AbyssalRift", AccessLevel.Administrator, e => AbyssalRift(e.Mobile));
            CommandSystem.Register("AbyssalNuke", AccessLevel.Administrator, e => AbyssalNuke(e.Mobile));
            CommandSystem.Register("ChiefBelch", AccessLevel.Administrator, e => ChiefBelch(e.Mobile));
            CommandSystem.Register("ChiefCough", AccessLevel.Administrator, e => ChiefCough(e.Mobile));
            CommandSystem.Register("CrystalShards", AccessLevel.Administrator, e => CrystalShards(e.Mobile));
            CommandSystem.Register("CrystalBleed", AccessLevel.Administrator, e => CrystalBleed(e.Mobile));
            CommandSystem.Register("HoofStomp", AccessLevel.Administrator, e => HoofStomp(e.Mobile));
            CommandSystem.Register("TeleportAttack", AccessLevel.Administrator, e => TeleportAttack(e.Mobile));
            //CommandSystem.Register("CorruptVirtue", AccessLevel.Administrator, e => CorruptVirtue(e.Mobile));
            //CommandSystem.Register("TakeLife", AccessLevel.Administrator, e => TakeLife(e.Mobile));
            //CommandSystem.Register("AreaPainspike", AccessLevel.Administrator, e => AreaPainspike(e.Mobile));
            //CommandSystem.Register("SilenceHowl", AccessLevel.Administrator, e => SilenceHowl(e.Mobile));
            //CommandSystem.Register("SpillAcid", AccessLevel.Administrator, e => SpillAcid(e.Mobile, e.Mobile));
            CommandSystem.Register("WaterWaves", AccessLevel.Administrator, e => WaterWaves(e.Mobile));
            CommandSystem.Register("WaterClouds", AccessLevel.Administrator, e => WaterClouds(e.Mobile));
            //CommandSystem.Register("SummonHelpers", AccessLevel.Administrator, e => SummonHelpers(e.Mobile));
            CommandSystem.Register("PhantasmicMockery", AccessLevel.Administrator, e => PhantasmicMockery(e.Mobile));
            CommandSystem.Register("ChillWinds", AccessLevel.Administrator, e => ChillWinds(e.Mobile));
            CommandSystem.Register("TrapScattering", AccessLevel.Administrator, e => TrapScattering(e.Mobile));
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

        public static void ChillWinds(Mobile from, int range = 10)
        {
            if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
                return;

            from.PublicOverheadMessage(MessageType.Emote, 1154, true, $"*a chilling wind rises*");

            Timer.DelayCall(TimeSpan.FromSeconds(1.5), () =>
            {
                if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
                    return;

                //Aoe paralyse them
                using var pool = PooledRefQueue<Mobile>.Create();
                var eable = from.GetMobilesInRange(range);
                foreach (var target in eable)
                {
                    //if (!WitherSpell.ValidTarget(from, target))
                    //    continue;

                    pool.Enqueue(target);
                }
                //eable.Free();


                if (pool.Count == 0)
                    return;

                //Only debuff 2-3 people
                var debuffed = Utility.Random(2, 2);
                while (pool.Count > 0 && debuffed > 0)
                {
                    var target = pool.Dequeue();
                    //EssenceOfWindSpell.DoEffect(target, 31, 5, 60, 60, 60, TimeSpan.FromSeconds(30));

                    var spell = new EssenceOfWindSpell(from);
                    //spell.DoEffect(target, 31, 5, 60, TimeSpan.FromSeconds(30));
                    debuffed--;
                }
            });
        }

        public static void PhantasmicMockery(Mobile from, int range = 10)
        {
            if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
                return;

            from.Say("the mocking laughter grows louder...");

            Timer.DelayCall(TimeSpan.FromSeconds(1.5), () =>
            {
                if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
                    return;

                //Aoe paralyse them
                using var pool = PooledRefQueue<Mobile>.Create();
                var eable = from.GetMobilesInRange(range);
                foreach (var target in eable)
                {
                    //if (!WitherSpell.ValidTarget(from, target))
                    //    continue;

                    pool.Enqueue(target);
                }
                //eable.Free();

                //Drain 15 mana per second
                while (pool.Count > 0)
                {
                    var target = pool.Dequeue();
                    from.DoHarmful(target);
                    target.PrivateOverheadMessage(MessageType.Emote, 21, true, "*psychic attacked*", target.NetState);
                    target.FixedParticles(0x376A, 9, 32, 5005, 0, 0, EffectLayer.Waist);
                    target.Paralyze(TimeSpan.FromSeconds(4));
                    new PhantasmicTimer(from, target).Start();
                }


                Effects.PlaySound(from.Location, from.Map, 0x1F8);
            });
        }

        private class PhantasmicTimer : Timer
        {
            private Mobile _from;
            private Mobile _target;

            public PhantasmicTimer(Mobile from, Mobile target) : base(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), 3)
            {
                _from = from;
                _target = target;
            }

            protected override void OnTick()
            {
                if (!_target.Alive || _target.Deleted)
                {
                    Stop();
                    return;
                }

                if (!_target.Paralyzed)
                {
                    _target.PrivateOverheadMessage(MessageType.Emote, 21, true, "*resists psychic attack*", _target.NetState);
                    Stop();
                    return;
                }

                _target.FixedParticles(0x374A, 10, 15, 5032, EffectLayer.Head);
                _target.PlaySound(0x1F8);
                _target.Mana -= 15;
                _from.Mana += 15;
            }
        }

        public static void WaterClouds(Mobile from, int radius = 8, int count = 3, int duration = 3000)
        {
            if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
                return;

            from.Say("The ground bubbles with a mysterious water surge...");

            int trapsPerBurst = 5;         // traps to attempt per burst

            // Start after a short delay
            Timer.DelayCall(TimeSpan.FromSeconds(1.5), () =>
            {
                if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
                    return;

                for (int burst = 0; burst < count; burst++)
                {
                    // Increase the radius for each burst.
                    int currentRadius = radius + burst;

                    Timer.DelayCall(TimeSpan.FromSeconds(burst * 0.5), () =>
                    {
                        // HashSet to track positions used in this burst, so we don't stack multiple traps here.
                        HashSet<Point3D> usedPositions = new HashSet<Point3D>();

                        for (int i = 0; i < trapsPerBurst; i++)
                        {
                            // Generate a random offset within the square of currentRadius.
                            int offsetX = Utility.RandomMinMax(-currentRadius, currentRadius);
                            int offsetY = Utility.RandomMinMax(-currentRadius, currentRadius);
                            double dist = Math.Sqrt(offsetX * offsetX + offsetY * offsetY);
                            int attempts = 0;

                            // Ensure the random point is within a band: roughly at the edge of the current burst.
                            while ((dist < currentRadius - 3.1 || dist > currentRadius + 0.1) && attempts < 5)
                            {
                                offsetX = Utility.RandomMinMax(-currentRadius, currentRadius);
                                offsetY = Utility.RandomMinMax(-currentRadius, currentRadius);
                                dist = Math.Sqrt(offsetX * offsetX + offsetY * offsetY);
                                attempts++;
                            }

                            int newX = from.X + offsetX;
                            int newY = from.Y + offsetY;
                            int newZ = from.Map.GetAverageZ(newX, newY);
                            Point3D pnt = new Point3D(newX, newY, newZ);

                            // Avoid duplicates within this burst.
                            if (usedPositions.Contains(pnt))
                                continue;

                            usedPositions.Add(pnt);

                            new FireFieldItem(14232, pnt, from, from.Map, TimeSpan.FromSeconds(Utility.Random(6, 12)), 10);
                        }
                    });
                }
            });
        }

        public static void WaterWaves(Mobile from, int range = 10, int duration = 3000)
        {
            if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
                return;

            Effects.SendLocationParticles(
                EffectItem.Create(from.Location, from.Map, EffectItem.DefaultDuration),
                43000,
                1,
                30,
                96,
                3,
                9917,
                0
            );

            from.Say("The endless tide rises... prepare to be swept away!");


            Timer.DelayCall(TimeSpan.FromSeconds(1.5), () =>
            {
                if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
                    return;

                int delayPerStep = 250;      // Delay (in milliseconds) between each step of the wave

                // For each step in the wave, schedule a delayed effect.
                for (int dist = 1; dist <= range; dist++)
                {
                    // Capture the current distance for use in the closure.
                    int currentDistance = dist;
                    var expiryHits = new Dictionary<Mobile, long>();
                    Timer.DelayCall(TimeSpan.FromMilliseconds(currentDistance * delayPerStep), () =>
                    {
                        if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
                            return;

                        // Calculate the target points in each direction.
                        var points = new Point3D[]
                        {
                            new Point3D(from.X, from.Y - currentDistance, from.Map.GetAverageZ(from.X, from.Y - currentDistance)), //south
                            new Point3D(from.X + currentDistance, from.Y, from.Map.GetAverageZ(from.X + currentDistance, from.Y)), //east
                            new Point3D(from.X, from.Y + currentDistance, from.Map.GetAverageZ(from.X, from.Y + currentDistance)), //north
                            new Point3D(from.X - currentDistance, from.Y, from.Map.GetAverageZ(from.X - currentDistance, from.Y)), //west
                            //note: the index of each element matters as its a proxy for (byte)Direction
                        };

                        // Send out the directional animation effects.
                        Effects.SendLocationEffect(points[0], from.Map, 0x1FB8, 15, 10); //north
                        Effects.SendLocationEffect(points[1], from.Map, 0x1FC1, 15, 10); //east
                        Effects.SendLocationEffect(points[2], from.Map, 0x1FC6, 15, 10); //south
                        Effects.SendLocationEffect(points[3], from.Map, 0x1FBC, 15, 10); //west

                        //Get everyone near
                        for (byte i = 0; i < points.Length; i++)
                        {
                            var point = points[i];
                            try
                            {
                                using var pool = PooledRefQueue<Mobile>.Create();
                                var eable = from.Map.GetMobilesInRange(point, 1);
                                foreach (var target in eable)
                                {
                                    if ((expiryHits.TryGetValue(target, out var expiry) && Core.TickCount >= expiry))
                                        continue;

                                    pool.Enqueue(target);
                                }
                                //eable.Free();
                                //eable = null;

                                while (pool.Count > 0)
                                {
                                    var target = pool.Dequeue();

                                    expiryHits[target] = Core.TickCount + 250;

                                    //Damage them
                                    from.DoHarmful(target);
                                    AOS.Damage(target, from, Utility.RandomMinMax(12, 22), 0, 0, 50, 0, 50);

                                    //Have they died?
                                    //if (!WitherSpell.ValidTarget(from, target))
                                    //    continue;

                                    //Shove them
                                    var newX = target.X;
                                    var newY = target.Y;
                                    Server.Movement.Movement.Offset((Direction)((byte)i * 2), ref newX, ref newY);
                                    var z = target.Map.GetAverageZ(newX, newY);
                                    var teleLocation = new Point3D(newX, newY, z);
                                    if (target.Map.CanSpawnMobile(teleLocation))
                                    {
                                        target.Location = teleLocation;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.Error($"WaterWaves PvM ability exception:\n{ex}");
                                //eable?.Free();
                            }
                        }
                    });
                }

                //Play audio no matter what.
                from.PlaySound(0x20D);
            });

        }

        //public static void SpillAcid(Mobile from, Mobile targetLocation, int amount = 4)
        //{
        //    if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal
        //        || targetLocation == null || targetLocation.Map == null || targetLocation.Map == Map.Internal)
        //        return;

        //    for (var i = 0; i < amount; ++i)
        //    {
        //        var loc = targetLocation.Map.GetRandomNearbyLocation(targetLocation.Location);
        //        var acid = new PoolOfAcid(from, TimeSpan.FromSeconds(10), 30, 30);
        //        acid.MoveToWorld(loc, targetLocation.Map);
        //    }
        //}

        //public static void SilenceHowl(Mobile from, int range = 10, int duration = 5000)
        //{
        //    if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
        //        return;

        //    //Get everyone near
        //    using var pool = PooledRefQueue<Mobile>.Create();
        //    var eable = from.GetMobilesInRange(range);
        //    foreach (var target in eable)
        //    {
        //        //if (!WitherSpell.ValidTarget(from, target))
        //        //    continue;

        //        pool.Enqueue(target);
        //    }
        //    //eable.Free();

        //    //Silence everyone
        //    while (pool.Count > 0)
        //    {
        //        var target = pool.Dequeue();
        //        target.PrivateOverheadMessage(MessageType.Emote, 87, true, $"*cannot focus*", target.NetState);
        //        target.SpellSilenceExpiry = Core.TickCount + duration;
        //        (target.Spell as Spell)?.Disturb(DisturbType.Hurt, skipDisturbLeniencyCheck: true);
        //        (target.PaladinNoneCursorSpell as Spell)?.Disturb(DisturbType.Hurt, skipDisturbLeniencyCheck: true);
        //        (target.SpiritSpeakSkill as Spell)?.Disturb(DisturbType.Hurt, skipDisturbLeniencyCheck: true);

        //        Timer.DelayCall(TimeSpan.FromMilliseconds(duration), () =>
        //        {
        //            target.PrivateOverheadMessage(MessageType.Emote, 87, true, $"*regains concentration*", target.NetState);
        //        });

        //        from.DoHarmful(target);
        //    }

        //    from.PlaySound(0x584);
        //    from.PlaySound(0x3cc);
        //}

        //public static void AreaPainspike(Mobile from, int range = 10)
        //{
        //    if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
        //        return;

        //    Get everyone near
        //    using var pool = PooledRefQueue<Mobile>.Create();
        //    var eable = from.GetMobilesInRange(range);
        //    foreach (var target in eable)
        //    {
        //        if (!WitherSpell.ValidTarget(from, target))
        //            continue;

        //        pool.Enqueue(target);
        //    }
        //    eable.Free();

        //    Painspike them
        //    while (pool.Count > 0)
        //    {
        //        var target = pool.Dequeue();
        //        PainSpikeSpell.DoPainSpike(from, target, null, false);
        //        PainSpikeSpell.DoPainSpike(from, target, null, false);
        //    }

        //    from.PlaySound(0x210);
        //}

        //public static void TakeLife(Mobile from, int range = 10, bool directDamage = true)
        //{
        //    if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
        //        return;

        //    from.Say(1075117);  // Muahahaha!  Your life essence is MINE!

        //    Timer.DelayCall(TimeSpan.FromSeconds(1.5), () =>
        //    {
        //        if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
        //            return;

        //        //Get everyone near
        //        using var pool = PooledRefQueue<Mobile>.Create();
        //        var eable = from.GetMobilesInRange(range);
        //        foreach (var target in eable)
        //        {
        //            if (!WitherSpell.ValidTarget(from, target))
        //                continue;

        //            pool.Enqueue(target);
        //        }
        //        eable.Free();

        //        //Damage them
        //        while (pool.Count > 0)
        //        {
        //            var target = pool.Dequeue();
        //            (target as PlayerMobile)?.PlaySound(target.Female ? 0x14D : 0x156);
        //            (target as BaseCreature)?.PlaySound(target.GetHurtSound());

        //            from.DoHarmful(target);
        //            target.PrivateOverheadMessage(MessageType.Regular, 33, false, "Lady Melisande has drained some of your life essence.", target.NetState);
        //            target.FixedParticles(0x376A, 9, 32, 5005, 1156, 0, EffectLayer.Waist);
        //            target.FixedParticles(0x374A, 10, 15, 5013, 0x496, 0, EffectLayer.Waist);

        //            var dmg = Utility.RandomMinMax(34, 67);
        //            from.Hits += dmg;

        //            AOS.Damage(target, from, dmg, false, directDamage ? 0 : 100, 0, 0, 0, 0, direct: directDamage ? 100 : 0);
        //        }


        //        //Trash talk
        //        from.FixedParticles(0x376A, 9, 32, 5005, EffectLayer.Waist);
        //        from.PlaySound(0x1F2);
        //        from.Say(1075120); // An unholy aura surrounds Lady Melisande as her wounds begin to close.
        //    });
        //}

        //public static void CorruptVirtue(Mobile from, int range = 10)
        //{
        //    if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
        //        return;

        //    from.Say("Groshak exhales a rancid breath, spreading a wave of fetid corruption!");

        //    Timer.DelayCall(TimeSpan.FromSeconds(1.5), () =>
        //    {
        //        if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
        //            return;

        //        //Get everyone near
        //        using var pool = PooledRefQueue<Mobile>.Create();
        //        var eable = from.GetMobilesInRange(range);
        //        foreach (var target in eable)
        //        {
        //            if (!WitherSpell.ValidTarget(from, target))
        //                continue;

        //            pool.Enqueue(target);
        //        }
        //        eable.Free();

        //        //Damage them
        //        while (pool.Count > 0)
        //        {
        //            var target = pool.Dequeue();
        //            var human = target.Body.IsHuman;
        //            target.Animate(human ? 21 : 2, human ? 6 : 4, 1, true, false, 0);
        //            target.PrivateOverheadMessage(MessageType.Regular, 1154, false, $"Groshak's fetid corruption seeps into your body!", target.NetState);

        //            var dmg = Utility.RandomMinMax(34, 67);
        //            AOS.Damage(target, from, dmg, 100, 0, 0, 0, 0);
        //        }

        //        Effects.PlaySound(from.Location, from.Map, 0x1FB); // Adjust to a suitably ominous sound
        //    });
        //}

        public static Mobile ChooseRandomEnemyNearby(Mobile from, int range)
        {
            Mobile victim = null;
            var tries = 10;
            while (victim == null && tries > 0)
            {
                //Get a random attacker or damager
                victim = 0.75 > Utility.RandomDouble() && from.Aggressors.Any()
                         ? from.Aggressors.RandomElement()?.Attacker
                         : from.DamageEntries.RandomElement()?.Damager;

                //Must be in range
                if (victim != null)
                {
                    var distance = range - from.GetDistanceToSqrt(victim);
                    if (distance < 0 || distance > range)// || !WitherSpell.ValidTarget(from, victim))
                        victim = null;
                }

                tries--;
            }

            return victim;
        }

        public static void TeleportAttack(Mobile from, int strikingRange = 10, string msg = null, bool applyDp = true)
        {
            if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
                return;

            if (!from.Aggressors.Any() && !from.DamageEntries.Any())
                return;

            if (!string.IsNullOrEmpty(msg))
                from.PublicOverheadMessage(MessageType.Emote, 820, true, msg);

            Timer.DelayCall(TimeSpan.FromSeconds(1.5), () =>
            {
                if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
                    return;

                var victim = ChooseRandomEnemyNearby(from, strikingRange);
                if (victim == null)
                    return;

                // Get teleport location
                var point = victim.Location;
                for (byte i = 0; i < 0x8; i++)
                {
                    var x = victim.X;
                    var y = victim.Y;
                    var direction = (Direction)i;
                    Server.Movement.Movement.Offset(direction, ref x, ref y);
                    var z = victim.Map.GetAverageZ(x, y);
                    var teleLocation = new Point3D(x, y, z);
                    if (victim.Map.CanSpawnMobile(teleLocation) && victim.Map.LineOfSight(from, teleLocation))
                    {
                        point = teleLocation;
                        break;
                    }
                }

                from.Location = point;
                victim.FixedParticles(0x376A, 9, 32, 0x13AF, EffectLayer.Waist);
                victim.PlaySound(0x1FE);
                from.FixedParticles(0x374A, 10, 15, 5021, EffectLayer.Waist);
                from.PlaySound(0x474);
                from.DoHarmful(victim);
                if (applyDp && from is BaseCreature baseCreature)
                {
                    victim.ApplyPoison(from, baseCreature.HitPoison);
                    from.Combatant = victim;
                }
            });
        }

        public static void HoofStomp(Mobile from)
        {
            if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
                return;

            from.PublicOverheadMessage(MessageType.Emote, 820, true, "*starts bucking*");

            Timer.DelayCall(TimeSpan.FromSeconds(1.5), () =>
            {
                if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
                    return;

                using var pool = PooledRefQueue<Mobile>.Create();
                var eable = from.Map.GetMobilesInRange(from.Location, 7);
                foreach (var target in eable)
                {
                    //if (!WitherSpell.ValidTarget(from, target) || target.GetStatMod("DreadHornStr") != null)
                    //    continue;

                    pool.Enqueue(target);
                }
                //eable.Free();

                //Do the effect
                while (pool.Count > 0)
                {
                    var target = pool.Dequeue();
                    (target as PlayerMobile)?.PlaySound(target.Female ? 0x14D : 0x156);
                    (target as BaseCreature)?.PlaySound(target.GetHurtSound());

                    var human = target.Body.IsHuman;
                    target.Animate(human ? 21 : 2, human ? 6 : 4, 1, true, false, 0);

                    var percent = target.Skills.MagicResist.Value / 100;
                    var malas = (int)(-20 + (percent * 5.2));

                    var duration = TimeSpan.FromSeconds(60);
                    var str = Math.Min(target.Str - 1, Math.Abs(malas));
                    var dex = Math.Min(target.Dex - 1, Math.Abs(malas));
                    var intt = Math.Min(target.Int - 1, Math.Abs(malas));
                    target.AddStatMod(new StatMod(StatType.Str, "DreadHornStr", -str, duration));
                    target.AddStatMod(new StatMod(StatType.Dex, "DreadHornDex", -dex, duration));
                    target.AddStatMod(new StatMod(StatType.Int, "DreadHornInt", -intt, duration));
                    var args = $"{str}\t{dex}\t{intt}";

                    (target as PlayerMobile)?.AddBuff(new BuffInfo(BuffIcon.GazeDespair, 1153794, 1075840, duration, args, true));
                    target.SendLocalizedMessage(1075081); // *Dreadhorns eyes light up, his mouth almost a grin, as he slams one hoof to the ground!*
                }

                // earthquake
                from.PlaySound(0x2F3);
            });
        }

        public static void CrystalShards(Mobile from, int radius = 10)
        {
            if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
                return;

            from.Say("Crystalline Terror unleashes a barrage of razor-sharp shards!");

            // Slight delay before the shards appear
            Timer.DelayCall(TimeSpan.FromSeconds(1.5), () =>
            {
                if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
                    return;

                int shardCount = 7; // How many shards to scatter

                for (int i = 0; i < shardCount; i++)
                {
                    // Random offset within ±radius
                    var offsetX = Utility.RandomMinMax(-radius, radius);
                    var offsetY = Utility.RandomMinMax(-radius, radius);

                    var newX = from.X + offsetX;
                    var newY = from.Y + offsetY;
                    var newZ = from.Map.GetAverageZ(newX, newY);
                    Point3D shardLoc = new Point3D(newX, newY, newZ);

                    // Small chance to nudge it further
                    if (Utility.RandomDouble() < 0.15)
                    {
                        shardLoc.X += Utility.RandomMinMax(-1, 1);
                        shardLoc.Y += Utility.RandomMinMax(-1, 1);
                    }

                    // Pick a random crystal shard ID from 0x223A to 0x2249
                    var itemID = Utility.RandomMinMax(0x223A, 0x2249);

                    // Create the shard item at that location
                    var shard = new CrystalShardItem(from, itemID);
                    shard.MoveToWorld(shardLoc, from.Map);
                }

                // Optional: sound effect at the boss's location
                Effects.PlaySound(from.Location, from.Map, 0x2A1);
            });
        }

        [SerializationGenerator(0, false)]
        public partial class CrystalShardItem : Item
        {
            private static readonly Dictionary<Mobile, long> _mobilesHitExpiry = new();
            private const int footHitCooldown = 220;
            private const int mountedHitCooldown = 150;
            private const string name = "a crystal shard";

            private Mobile _from;    // The boss or source that spawned this shard
            private Timer m_Timer;      // Repeated timer for damage ticks

            [Constructible]
            public CrystalShardItem(Mobile from, int itemID) : base(itemID)
            {
                Movable = false;
                _from = from;

                Name = name;
                Hue = 1152;

                // Start the timer to apply damage once per second, up to 3 times
                m_Timer = new ShardTimer(from, this);
                m_Timer.Start();
            }

            //public override bool ShouldSerialize => false;

            public override void Delete()
            {
                m_Timer?.Stop();
                base.Delete();
            }

            public override bool OnMoveOver(Mobile m)
            {
                DoEffect(_from, m);
                return true;
            }

            public void DoEffect(Mobile from, Mobile target)
            {
                if (from == null || !Visible)// || !WitherSpell.ValidTarget(from, target))
                    return;

                if (_mobilesHitExpiry.TryGetValue(target, out var dmgExpireTicks) && dmgExpireTicks > Core.TickCount)
                    return;

                // Damage them
                int damage = Utility.RandomMinMax(15, 30);
                AOS.Damage(target, _from, damage, 100, 0, 0, 0, 0);

                // Bleed them
                //BleedAttack.BeginBleed(target, from, true);

                //target.PrivateOverheadMessage(Network.MessageType.Emote, 0, false, "*crystal shards cut into you*", target.NetState);
                _mobilesHitExpiry[target] = Core.TickCount + (target.Mounted ? mountedHitCooldown : footHitCooldown);
            }

            private class ShardTimer : Timer
            {
                private Mobile _from;
                private CrystalShardItem _crystalShard;
                private int _ticks;

                public ShardTimer(Mobile from, CrystalShardItem crystalShard, int ticks = 5) : base(TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1), ticks + 1)
                {
                    _from = from;
                    _crystalShard = crystalShard;
                    _ticks = ticks;
                }

                protected override void OnTick()
                {
                    // If the item is deleted or the map is invalid, remove it
                    if (_crystalShard == null || _crystalShard.Map == null || _crystalShard.Deleted || _crystalShard.Map == Map.Internal)
                    {
                        _crystalShard?.Delete();
                        Stop();
                        return;
                    }

                    // Damage everything in a 1-tile radius
                    using var pool = PooledRefQueue<Mobile>.Create();
                    var eable = _crystalShard.GetMobilesInRange(0);
                    foreach (var m in eable)
                    {
                        //if (!WitherSpell.ValidTarget(_from, m))
                        //    continue;

                        pool.Enqueue(m);
                    }
                    //eable.Free();

                    //Do the effect
                    while (pool.Count > 0)
                    {
                        var mobile = pool.Dequeue();
                        _crystalShard.DoEffect(_from, mobile);
                    }

                    // Decrement the tick count. If we’re done, remove the shard.
                    _ticks--;
                    if (_ticks <= 0)
                    {
                        _crystalShard.Delete();
                        Stop();
                    }
                }
            }
        }

        public static void CrystalBleed(Mobile from, int range = 10)
        {
            if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
                return;

            from.Say("Sharpening shards...");

            Timer.DelayCall(TimeSpan.FromSeconds(1.5), () =>
            {
                if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
                    return;

                //Get everyone near
                using var pool = PooledRefQueue<Mobile>.Create();
                var eable = from.GetMobilesInRange(range);
                foreach (var target in eable)
                {
                    //if (!WitherSpell.ValidTarget(from, target))
                    //    continue;

                    pool.Enqueue(target);
                }
                //eable.Free();

                //Bleed them
                //while (pool.Count > 0)
                //{
                //    var target = pool.Dequeue();
                //    BleedAttack.BeginBleed(target, from, true);
                //}

                from.PlaySound(0x51f);
                //eable.Free();
            });
        }



        public static void ChiefBelch(Mobile from, int radius = 10)
        {
            if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
                return;

            from.Say("Chief Paroxysmus lets out a sickening belch, releasing toxic fumes!");

            Timer.DelayCall(TimeSpan.FromSeconds(1.5), () =>
            {
                if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
                    return;

                // Number of random “toxic cloud” tiles
                int cloudCount = 7;

                for (int i = 0; i < cloudCount; i++)
                {
                    // Random offset within a square of ±radius
                    int offsetX = Utility.RandomMinMax(-radius, radius);
                    int offsetY = Utility.RandomMinMax(-radius, radius);

                    int newX = from.X + offsetX;
                    int newY = from.Y + offsetY;
                    int newZ = from.Map.GetAverageZ(newX, newY);
                    Point3D loc = new Point3D(newX, newY, newZ);

                    // Optional: a small chance for a bigger spread
                    if (Utility.RandomDouble() < 0.15)
                    {
                        loc.X += Utility.RandomMinMax(-1, 1);
                        loc.Y += Utility.RandomMinMax(-1, 1);
                    }

                    Effects.SendLocationEffect(loc, from.Map, 0x36BD, 10, 10, 1271, 0);

                    // Poison/damage mobiles in that tile’s immediate area
                    using var pool = PooledRefQueue<Mobile>.Create();
                    var eable = from.Map.GetMobilesInRange(loc, 1);
                    foreach (var target in eable)
                    {
                        //if (!WitherSpell.ValidTarget(from, target))
                        //    continue;

                        pool.Enqueue(target);
                    }
                    //eable.Free();

                    //Do the effect
                    while (pool.Count > 0)
                    {
                        var target = pool.Dequeue();
                        (target as PlayerMobile)?.PlaySound(target.Female ? 0x14D : 0x156);
                        (target as BaseCreature)?.PlaySound(target.GetHurtSound());

                        int d = Utility.RandomMinMax(34, 67);
                        AOS.Damage(target, from, d, 0, 0, 0, 100, 0);

                        target.ApplyPoison(from, Poison.Deadly);

                        // Optional reaction: play a gagging sound, animate, etc.
                        target.PlaySound(0x205); // Poisoned grunt
                        target.SendMessage("You choke on the vile stench!");
                    }
                }

                Effects.PlaySound(from.Location, from.Map, 0x231);
            });
        }

        public static void ChiefCough(Mobile from, int range = 10)
        {
            if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
                return;

            from.Say("Chief Paroxysmus starts coughing uncontrollably...");

            Timer.DelayCall(TimeSpan.FromSeconds(1.5), () =>
            {
                if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
                    return;

                //Get everyone near
                using var pool = PooledRefQueue<Mobile>.Create();
                var eable = from.GetMobilesInRange(7);
                foreach (var target in eable)
                {
                    //if (!WitherSpell.ValidTarget(from, target))
                    //    continue;

                    pool.Enqueue(target);
                }
                //eable.Free();

                //Do the effect
                while (pool.Count > 0)
                {
                    var target = pool.Dequeue();
                    (target as PlayerMobile)?.PlaySound(target.Female ? 0x14D : 0x156);
                    (target as BaseCreature)?.PlaySound(target.GetHurtSound());

                    var human = target.Body.IsHuman;
                    target.Animate(human ? 21 : 2, human ? 6 : 4, 1, true, false, 0);

                    target.SendMessage("Chief Paroxysmus covers you in slime.");
                    target.FixedParticles(0x36BD, 1, 10, 0x1F78, 0xA6, 0, (EffectLayer)255);

                    int d = Utility.RandomMinMax(34, 67);
                    AOS.Damage(target, from, d, 0, 0, 0, 100, 0);
                    target.ApplyPoison(from, Poison.Lethal);
                }
            });
        }



        public static void AbyssalNuke(Mobile from)
        {
            if (!from.Alive || from.Map == null || from.Deleted || from.Map == Map.Internal)
                return;

            from.Say(1112362); // You will burn to a pile of ash!

            Effects.PlaySound(from.Location, from.Map, 0x349);

            //Flame Columns
            for (int i = 0; i < 2; i++)
            {
                Misc.Geometry.Circle2D(from.Location, from.Map, i, (pnt, map) =>
                {
                    Effects.SendLocationParticles(EffectItem.Create(pnt, map, EffectItem.DefaultDuration), 0x3709, 10, 30, 2499, 0, 5052, 0);
                });
            }

            var nukeTimer = new NukeTimer(from);
            nukeTimer.Start();
        }

        private class NukeTimer : Timer
        {
            private bool _isInitialImpact = true;
            private List<Mobile> _mobilesImpacted = null;
            private Mobile _boss;
            private const int _range = 10;

            public NukeTimer(Mobile boss) : base(TimeSpan.FromSeconds(1.5), TimeSpan.FromSeconds(1.5))
            {
                _boss = boss;
            }

            protected override void OnTick()
            {
                //Unfreeze everyone
                if (!_isInitialImpact)
                {
                    foreach (var mobile in _mobilesImpacted)
                    {
                        mobile.Frozen = false;
                        mobile.SendLocalizedMessage(1005603); // You can move again!
                    }
                    Stop();
                    return;
                }

                if (_boss.Deleted || !_boss.Alive || _boss.Map == null || _boss.Map == Map.Internal)
                {
                    Stop();
                    return;
                }

                Effects.PlaySound(_boss.Location, _boss.Map, 0x44B);
                for (int i = 0; i < _range; i++)
                {
                    Misc.Geometry.Circle2D(_boss.Location, _boss.Map, i, (pnt, map) =>
                    {
                        //Shockwave effects
                        Effects.SendLocationEffect(pnt, map, 14000, 14, 10, Utility.RandomMinMax(2497, 2499), 2);

                        //Get mobiles in the sphere
                        var eable = _boss.Map.GetMobilesInRange(pnt, 0);
                        foreach (var mobile in eable)
                        {
                            if (mobile == null || mobile.Map == null || mobile.Map == Map.Internal)
                                continue;

                            //if (!WitherSpell.ValidTarget(_boss, mobile))
                            //    continue;

                            _mobilesImpacted ??= new List<Mobile>();
                            _mobilesImpacted.Add(mobile);

                        }
                        //eable.Free();

                    });
                }

                //Get everyone near
                if (_mobilesImpacted == null)
                {
                    Stop();
                    return;
                }

                //Knock em down!
                foreach (var mobile in _mobilesImpacted)
                {
                    var playerMobile = mobile as PlayerMobile;
                    var baseCreature = mobile as BaseCreature;
                    var isHuman = baseCreature?.Body.IsHuman == true;
                    var mount = mobile.Mount;

                    playerMobile?.PlaySound(mobile.Female ? 0x14D : 0x156);
                    baseCreature?.PlaySound(mobile.GetHurtSound());
                    mobile.Animate(isHuman ? 21 : 2, isHuman ? 6 : 4, 1, true, false, 0);

                    //if (mobile.Mount != null)
                    //{
                    //    if (playerMobile != null)
                    //        playerMobile?.SetMountBlock(BlockMountType.Dazed, true, DismountRecoveryType.Defensive, true);
                    //    else
                    //        mobile.Mount.Rider = null;
                    //}

                    //Damage + freeze them
                    _boss.DoHarmful(mobile);
                    var dmg = Utility.RandomMinMax(68, 134);
                    AOS.Damage(mobile, _boss, dmg, 0, 0, 0, 0, 100);
                    mobile.Frozen = true;
                }

                _isInitialImpact = false;
            }
        }

        //public static void AbyssalRift(Mobile from, bool slowPlayers = false, int radius = 8)
        //{
        //    if (!from.Alive || from.Deleted || from.Map == null || from.Map == Map.Internal)
        //        return;

        //    from.Say("The Abyssal Infernal tears open a rift to the nether...");

        //    // Slight delay for dramatic effect
        //    Timer.DelayCall(TimeSpan.FromSeconds(1.5), () =>
        //    {
        //        if (!from.Alive || from.Deleted || from.Map == null || from.Map == Map.Internal)
        //            return;

        //        var riftCount = Utility.Random(3, 3);  // How many rifts to open

        //        for (var i = 0; i < riftCount; i++)
        //        {
        //            // Pick a random spot within ±radius
        //            var offsetX = Utility.RandomMinMax(-radius, radius);
        //            var offsetY = Utility.RandomMinMax(-radius, radius);

        //            var newX = from.Location.X + offsetX;
        //            var newY = from.Location.Y + offsetY;
        //            var newZ = from.Map.GetAverageZ(newX, newY);

        //            var riftLoc = new Point3D(newX, newY, newZ);

        //            // Optional small chance to nudge it even further
        //            if (Utility.RandomDouble() < 0.2)
        //            {
        //                riftLoc.X += Utility.RandomMinMax(-1, 1);
        //                riftLoc.Y += Utility.RandomMinMax(-1, 1);
        //            }

        //            var item = new AbyssalRiftItem(from, riftLoc, from.Map, slowPlayers);
        //        }

        //        // Optional finishing sound at the Infernal’s location
        //        Effects.PlaySound(from.Location, from.Map, 0x108);

        //        AbyssalRiftItem.CleanupExpiries();
        //    });
        //}


        //[SerializationGenerator(0, false)]
        //private partial class AbyssalRiftItem : Item
        //{
        //    private static readonly Dictionary<Mobile, long> _mobilesHitExpiry = new();
        //    private const int footHitCooldown = 220;
        //    private const int mountedHitCooldown = 150;

        //    private long _endTicks;
        //    private Mobile _owner;
        //    private ItemTimer _timer;
        //    private bool _slowPlayers;

        //    public override bool ShouldSerialize => false;

        //    public AbyssalRiftItem(Mobile from, Point3D loc, Map map, bool slowPlayers) : base(0x37CC)
        //    {
        //        Light = LightType.Circle300;
        //        _owner = from;
        //        _slowPlayers = slowPlayers;
        //        _endTicks = Core.TickCount + Utility.Random(8000, 2000);
        //        _timer = new ItemTimer(from, this, _endTicks, _slowPlayers);
        //        _timer.Start();
        //        MoveToWorld(loc, map);
        //    }

        //    public override bool OnMoveOver(Mobile m)
        //    {
        //        DoEffect(_owner, m, _slowPlayers);
        //        return true;
        //    }

        //    private void DoEffect(Mobile from, Mobile target, bool slowPlayers)
        //    {
        //        if (from == null || !Visible || !WitherSpell.ValidTarget(from, target))
        //            return;

        //        if (_mobilesHitExpiry.TryGetValue(target, out var dmgExpireTicks) && dmgExpireTicks > Core.TickCount)
        //            return;

        //        //Ice slow effect
        //        from.DoHarmful(target);
        //        if (target is PlayerMobile playerMobile)
        //        {
        //            from.DoHarmful(target);
        //            var slowDuration = 2.0;
        //            if (slowPlayers)
        //            {
        //                SlowingFieldItem.SlowPlayerMobile(playerMobile, slowDuration);
        //                playerMobile.FixedParticles(0x374A, 10, 30, 5013, 1153, 2, EffectLayer.Waist);
        //            }
        //            target.PlaySound(0x204);
        //            target.FixedEffect(0x377A, 10, 16);
        //        }

        //        //Damage: 20–40 shadow (energy) damage
        //        int dmg = Utility.RandomMinMax(34, 67);
        //        AOS.Damage(target, from, dmg, 0, 0, 0, 0, 100); // 100% energy
        //        target.SendMessage("Malevolent energy from the nether deals great damage!");

        //        //Dont let them be spam hit lots of times
        //        _mobilesHitExpiry[from] = Core.TickCount + (target.Mounted ? mountedHitCooldown : footHitCooldown);
        //    }

        //    public static void CleanupExpiries()
        //    {
        //        //Cleanup old mobiles
        //        var mobilesToRemove = new List<Mobile>();
        //        foreach (var mobileHit in _mobilesHitExpiry)
        //        {
        //            if (Core.TickCount - mobileHit.Value > 30000)
        //            {
        //                mobilesToRemove.Add(mobileHit.Key);
        //            }
        //        }
        //        foreach (var mobToRemove in mobilesToRemove)
        //        {
        //            _mobilesHitExpiry.Remove(mobToRemove);
        //        }
        //    }

        //    private class ItemTimer : Timer
        //    {
        //        private Mobile _boss;
        //        private AbyssalRiftItem _vortexItem;
        //        private long _endTicks;
        //        private bool _initial = true;
        //        private bool _slowPlayers;

        //        public ItemTimer(Mobile boss, AbyssalRiftItem vortexItem, long endTicks, bool slowPlayers) : base(TimeSpan.Zero, TimeSpan.FromSeconds(1))
        //        {
        //            _boss = boss;
        //            _vortexItem = vortexItem;
        //            _endTicks = endTicks;
        //            _slowPlayers = slowPlayers;
        //        }

        //        protected override void OnTick()
        //        {
        //            if (!_vortexItem.Visible || !_boss.Alive || _boss.Deleted || _boss.Map == null || _boss.Map == Map.Internal || _vortexItem == null || _vortexItem.Deleted || Core.TickCount > _endTicks)
        //            {
        //                _vortexItem?.Delete();
        //                Stop();
        //                return;
        //            }

        //            //Get everyone near
        //            using var pool = PooledRefQueue<Mobile>.Create();
        //            var eable = _vortexItem.GetMobilesInRange(1);
        //            foreach (var m in eable)
        //            {
        //                if (!WitherSpell.ValidTarget(_boss, m))
        //                    continue;

        //                pool.Enqueue(m);
        //            }
        //            eable.Free();

        //            //Do the effect
        //            while (pool.Count > 0)
        //            {
        //                var mobile = pool.Dequeue();
        //                _vortexItem.DoEffect(_boss, mobile, _slowPlayers);
        //            }

        //            if (_initial)
        //            {
        //                _initial = false;
        //                return;
        //            }

        //            //Move the item
        //            var newX = _vortexItem.X + Utility.Random(0, 2);
        //            var newY = _vortexItem.Y + Utility.Random(0, 2);
        //            var newZ = _vortexItem.Map.GetAverageZ(newX, newY);

        //            var newLoc = new Point3D(newX, newY, newZ);
        //            if (newLoc == _vortexItem.Location)
        //                return;

        //            _vortexItem.Location = newLoc;
        //        }
        //    }
        //}

        //private static readonly double[] _offsets =
        //{
        //    Math.Cos(000.0 / 180.0 * Math.PI), Math.Sin(000.0 / 180.0 * Math.PI),
        //    Math.Cos(040.0 / 180.0 * Math.PI), Math.Sin(040.0 / 180.0 * Math.PI),
        //    Math.Cos(080.0 / 180.0 * Math.PI), Math.Sin(080.0 / 180.0 * Math.PI),
        //    Math.Cos(120.0 / 180.0 * Math.PI), Math.Sin(120.0 / 180.0 * Math.PI),
        //    Math.Cos(160.0 / 180.0 * Math.PI), Math.Sin(160.0 / 180.0 * Math.PI),
        //    Math.Cos(200.0 / 180.0 * Math.PI), Math.Sin(200.0 / 180.0 * Math.PI),
        //    Math.Cos(240.0 / 180.0 * Math.PI), Math.Sin(240.0 / 180.0 * Math.PI),
        //    Math.Cos(280.0 / 180.0 * Math.PI), Math.Sin(280.0 / 180.0 * Math.PI),
        //    Math.Cos(320.0 / 180.0 * Math.PI), Math.Sin(320.0 / 180.0 * Math.PI)
        //};

        //private static readonly string[] _speach =
        //{
        //    "arise servants! kill them all!",
        //    "help me!",
        //};


        //public static void SummonHelpers(Mobile owner,
        //                                 Func<BaseCreature> selectRandomCreature,
        //                                 string[] speach,
        //                                 List<BaseCreature> summons,
        //                                 int maxSummons = 5,
        //                                 bool summonAtBoss = false)
        //{
        //    if (!owner.Alive || owner.Map == null || owner.Deleted || owner.Map == Map.Internal)
        //        return;


        //    if (selectRandomCreature == null)
        //    {
        //        _logger.Error($"Cannot SummonHelpers, selectRandomCreature() is null");
        //        return;
        //    }

        //    if (speach != null)
        //        owner.Say(speach.RandomElement());

        //    Timer.DelayCall(TimeSpan.FromSeconds(1.5), () =>
        //    {
        //        if (!owner.Alive || owner.Map == null || owner.Deleted || owner.Map == Map.Internal)
        //            return;

        //        for (var i = 0; i < _offsets.Length && owner.Followers < owner.FollowersMax && maxSummons > 0; i += 2)
        //        {
        //            Point3D location;
        //            if (summonAtBoss)
        //            {
        //                location = owner.Location;
        //                for (byte d = 0; d < 0x8; d++)
        //                {
        //                    var x = owner.X;
        //                    var y = owner.Y;
        //                    var direction = (Direction)d;
        //                    Server.Movement.Movement.Offset(direction, ref x, ref y);
        //                    var z = owner.Map.GetAverageZ(x, y);
        //                    var teleLocation = new Point3D(x, y, z);
        //                    if (owner.Map.CanSpawnMobile(teleLocation))
        //                    {
        //                        location = teleLocation;
        //                        break;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                location = CalculateSummonLocation(owner, i);
        //            }

        //            if (location == Point3D.Zero)
        //                continue;

        //            var spawn = selectRandomCreature();
        //            if (owner is BaseCreature baseCreature)
        //                spawn.Team = baseCreature.Team;

        //            // Otherwise they're treated as "animate dead" and AOE spells wont hit them, and they'll fight other monsters
        //            spawn.MoveToWorld(location, owner.Map);
        //            spawn.DoHarmful(owner.Combatant);
        //            UndeadDoSummonEffect(spawn);
        //            summons?.Add(spawn);
        //            maxSummons--;
        //        }
        //    });
        //}

        //public static bool UndeadMoveSummonNearby(BaseCreature creature)
        //{
        //    if (creature == null || creature.Map == null || !creature.Alive || creature.Deleted)
        //        return false;

        //    var randomLocation = CalculateSummonLocation(creature);
        //    if (randomLocation == Point3D.Zero)
        //        return false;

        //    creature.MoveToWorld(randomLocation, creature.Map);
        //    return true;
        //}

        //private static void UndeadDoSummonEffect(BaseCreature creature)
        //{
        //    Effects.SendLocationEffect(creature.Location, creature.Map, 0x3789, 10);
        //    Effects.SendLocationEffect(creature.Location, creature.Map, 0x4B8F, 10);
        //    creature.PlaySound(0x48F);
        //    creature.PlaySound(creature.GetAttackSound());

        //}

        //private static Point3D CalculateSummonLocation(Mobile owner, int offsetIdx = -1)
        //{
        //    var i = offsetIdx == -1 ? Utility.Random(_offsets.Length - 1) : offsetIdx;

        //    var rx = _offsets[i];
        //    var ry = _offsets[i + 1];

        //    var dist = 0;
        //    var ok = false;
        //    int x = 0, y = 0, z = 0;

        //    while (!ok && dist < 10)
        //    {
        //        var rdist = 10 + dist;

        //        x = owner.X + (int)(rx * rdist);
        //        y = owner.Y + (int)(ry * rdist);
        //        z = owner.Map.GetAverageZ(x, y);

        //        if (!(ok = owner.Map.CanFit(x, y, owner.Z, 16, false, false)))
        //        {
        //            ok = owner.Map.CanFit(x, y, z, 16, false, false);
        //        }

        //        if (dist >= 0)
        //        {
        //            dist = -(dist + 1);
        //        }
        //        else
        //        {
        //            dist = -(dist - 1);
        //        }
        //    }

        //    if (!ok)
        //    {
        //        return Point3D.Zero;
        //    }

        //    return new Point3D(x, y, z);
        //}

        //public static void ArrowsMultiShot(BaseCreature from, Mobile target)
        //{
        //    if (target == null || !target.Alive)
        //        return;

        //    Effects.PlaySound(from.Location, from.Map, 0x349);

        //    var targetLocation = target.Location;

        //    using var queue = PooledRefQueue<Mobile>.Create();
        //    foreach (var m in target.GetMobilesInRange(3))
        //    {
        //        if (m.Alive && (m.Player || (m is BaseCreature baseCreature && baseCreature.Team != from.Team)))
        //        {
        //            queue.Enqueue(m);
        //        }
        //    }

        //    while (queue.Count > 0)
        //    {

        //        //DoEffectTarget(source, queue.Dequeue());
        //    }
        //}

        public static void Blizzard(Mobile from, Point3D targetArea, int radius, int intervalMs, int countProcs)
        {
            from.PublicOverheadMessage(MessageType.Emote, 1151, false, $"*chilled winds blow furiously*");

            var damageTimer = new BlizzardAbilitySpamTimer(TimeSpan.Zero, TimeSpan.FromMilliseconds(intervalMs), countProcs, radius, targetArea, from, from.Map, false);
            damageTimer.Start();

            var totalMs = countProcs * intervalMs;
            var effectCountProcs = totalMs / 250;
            var effectIntervalMs = totalMs / effectCountProcs;
            var effectTimer = new BlizzardAbilitySpamTimer(TimeSpan.Zero, TimeSpan.FromMilliseconds(effectIntervalMs), effectCountProcs, radius, targetArea, from, from.Map, true);
            effectTimer.Start();
        }

        private static void DoBlizzardParticleEffects(Mobile from, Map map, Point3D targetLocation, int radius, int doEffectEvery)
        {
            //Create the effect in a circle
            var currentEvery = 0;

            for (int i = 0; i < radius; i++)
            {
                Geometry.Circle2D(targetLocation, map, i, (circlePoint, circleMap) =>
                {
                    //To avoid a "strobing effect", from particles falling too closely to each other
                    if (doEffectEvery != 0)
                    {
                        currentEvery++;
                        if (currentEvery != doEffectEvery)
                            return;

                        currentEvery = 0;
                    }

                    var procChance = i == radius - 1
                                     ? 0.2 > Utility.RandomDouble()
                                     : 0.15 > Utility.RandomDouble();
                    if (!procChance)
                        return;

                    Effects.SendMovingParticles(new Entity(Serial.Zero, new Point3D(circlePoint.X - 2, circlePoint.Y - 2, circlePoint.Z + 15), circleMap),
                                                new Entity(Serial.Zero, circlePoint, circleMap),
                                                Utility.RandomBool() ? 14036 : 14052,
                                                0,//Utility.Random(0, 1),
                                                0,
                                                false,
                                                false,
                                                Utility.RandomBool() ? 1153 : 1152,
                                                0,
                                                9502,
                                                1,
                                                0,
                                                (EffectLayer)255,
                                                0x100);
                });
            }
        }

        private static void DoBlizzardDamage(Mobile from, Map map, Point3D targetArea, int radius)
        {
            //Get everyone in range
            using var queue = PooledRefQueue<Mobile>.Create();
            var fromBaseCreature = from as BaseCreature;
            var isMonster = fromBaseCreature?.Controlled == false && (fromBaseCreature.IsAnimatedDead || !fromBaseCreature.Summoned);

            var mobilesInRange = map.GetMobilesInRange(targetArea, radius);
            foreach (var mobile in mobilesInRange)
            {
                //if (!IsValidAbilityTarget(from, mobile))
                //    continue;

                queue.Enqueue(mobile);
            }
            //mobilesInRange.Free();

            //Damage everyone in range
            while (queue.Count > 0)
            {
                var mobile = queue.Dequeue();
                var randomDamage = Utility.RandomMinMax(20, 50);
                AOS.Damage(mobile, randomDamage, 0, 0, 100, 0, 0);

                //Slow them
                //if (mobile is PlayerMobile player)
                //{
                //    SlowingFieldItem.SlowPlayerMobile(player, 1.5);
                //}
            }
        }

        private class BlizzardAbilitySpamTimer : Timer
        {
            private Mobile _from;
            private Map _map;
            private int _radius;
            private Point3D _targetArea;
            private bool _effectOnlyTrueDamageOnlyFalse;
            private bool _floorRunesCreated;
            //private List<InstanceStatic> _floorRunes = new();

            //private int _doEffectEveryOtherLocation = 5;

            public BlizzardAbilitySpamTimer(TimeSpan delay, TimeSpan interval, int count,
                                            int radius, Point3D targetArea,
                                            Mobile from, Map map,
                                            bool effectOnlyTrueDamageOnlyFalse) : base(delay, interval, count)
            {
                _from = from;
                _map = map;
                _radius = radius;
                _targetArea = targetArea;
                _effectOnlyTrueDamageOnlyFalse = effectOnlyTrueDamageOnlyFalse;
            }


            protected override void OnTick()
            {
                if (_from == null || _from.Map == null || _from.Map == Map.Internal || _from.Deleted || !_from.Alive)
                {
                    //foreach (var floorRune in _floorRunes)
                    //{
                    //    floorRune.Delete();
                    //}
                    //_floorRunes = null;

                    Stop();
                    return;
                }

                //Create runes on the floor to indicate the boundary of the blizzard
                if (!_floorRunesCreated)
                {
                    var currentEvery = 0;
                    var doEffectEvery = 3;
                    Geometry.Circle2D(_targetArea, _map, _radius, (circlePoint, circleMap) =>
                    {
                        currentEvery++;
                        if (currentEvery != doEffectEvery)
                            return;

                        currentEvery = 0;

                        var z = _map.GetAverageZ(circlePoint.X, circlePoint.Y);
                        circlePoint.Z = z;

                        //var floorRune = new InstanceStatic(3676);
                        //floorRune.MoveToWorld(circlePoint, circleMap);
                        //floorRune.Hue = 1153;
                        //floorRune.Movable = false;
                        //_floorRunes.Add(floorRune);
                    });
                    _floorRunesCreated = true;
                }

                //Delete the runes if the timer is about to stop
                if (RemainingCount == 0)
                {
                    //foreach (var floorRune in _floorRunes)
                    //{
                    //    floorRune.Delete();
                    //}
                    //_floorRunes = null;
                }

                //Blizzard effect ticks faster than damage
                if (_effectOnlyTrueDamageOnlyFalse)
                    DoBlizzardParticleEffects(_from, _map, _targetArea, _radius, 5);
                else
                    DoBlizzardDamage(_from, _map, _targetArea, _radius);
            }
        }


        //public static void ThrowBarrel(Mobile from, object target, bool createFlamesOnTarget, TimeSpan flamesDuration, Action<Item> createdFireItemAction)
        //{
        //    if (from == null || from.Map == null || !from.Alive || target is not IPoint3D targetPoint)
        //    {
        //        return;
        //    }

        //    var explosiveBarrel = new ExplosiveBarrel(createFlamesOnTarget, flamesDuration, createdFireItemAction);
        //    BaseExplosionPotion.ExplosionPotionThrowTarget.Target(from, target, explosiveBarrel);
        //}

        public static void ThrowFirebomb(Mobile from, object target, bool createFlamesOnTarget, TimeSpan flamesDuration, Action<Item> createdFireItemAction)
        {
            if (from == null || from.Map == null || !from.Alive || target is not IPoint3D targetPoint)
            {
                return;
            }

            //barrel = 4014
            SpellHelper.GetSurfaceTop(ref targetPoint);

            var targetLocation = new Point3D(targetPoint);
            from.RevealingAction();

            IEntity to = new Entity(Serial.Zero, targetLocation, from.Map);
            Effects.SendMovingEffect(from, to, 7193, 7, 0, false, false, 0);

            var delay = TimeSpan.FromSeconds(0.1 * from.GetDistanceToSqrt(targetLocation));
            Timer.DelayCall(delay, () => Explosion(from,
                                                   targetLocation, from.Map,
                                                   20, 25, 3, //min max radius
                                                   false,
                                                   createFlamesOnTarget,
                                                   createdFireItemAction,
                                                   flamesDuration));
        }

        public static void Explosion(Mobile from, Point3D targetLocation, Map actualMap,
                                     int minDamage, int maxDamage, int radius,
                                     bool explode,
                                     bool createFlamesOnTarget, Action<Item> createdFireItemAction, TimeSpan flamesDuration)
        {
            if (actualMap == null || actualMap == Map.Internal)
                return;

            //Damage everyone in range
            if (explode)
            {
                Effects.PlaySound(targetLocation, actualMap, 0x307);
                Effects.SendLocationEffect(targetLocation, actualMap, 0x36B0, 9);
                using var queue = PooledRefQueue<Mobile>.Create();
                foreach (var mobile in from.GetMobilesInRange(radius))
                {
                    if (!IsValidAbilityTarget(from, mobile))
                        continue;

                    queue.Enqueue(mobile);
                }

                while (queue.Count > 0)
                {
                    var mobile = queue.Dequeue();
                    var randomDamage = Utility.RandomMinMax(minDamage * 2, maxDamage * 2);
                    AOS.Damage(mobile, randomDamage, 0, 100, 0, 0, 0);
                }
            }
            else
            {
                //shattering noise
                Effects.PlaySound(targetLocation, actualMap, 0x15E);
            }

            //Create a fire on the floor
            //if (createFlamesOnTarget)
            //{
            //    var flamesItem = new FlamesItem(from, minDamage, maxDamage, 1000, flamesDuration);
            //    flamesItem.MoveToWorld(targetLocation, actualMap);
            //    if (createdFireItemAction != null)
            //    {
            //        createdFireItemAction.Invoke(flamesItem);
            //    }
            //}
        }

        public static bool IsValidAbilityTarget(Mobile from, Mobile mobile)
        {
            var fromBaseCreature = from as BaseCreature;
            var isMonster = fromBaseCreature?.Controlled == false && (fromBaseCreature.IsAnimatedDead || !fromBaseCreature.Summoned);
            if (from == mobile
                || !from.InLOS(mobile)
                || (!isMonster && !SpellHelper.ValidIndirectTarget(from, mobile))
                || !from.CanBeHarmful(mobile, false)
                || (fromBaseCreature?.IsAnimatedDead == true && mobile.Player))
            {
                return false;
            }

            if (mobile is BaseCreature nearbyBaseCreature
                &&
                (
                    //Monster on the same team
                    (isMonster && !nearbyBaseCreature.Controlled && !nearbyBaseCreature.Summoned && nearbyBaseCreature.Team == fromBaseCreature?.Team)
                    //Pet hitting another pet from the same owner
                    || (nearbyBaseCreature.Summoned && nearbyBaseCreature.SummonMaster != null && nearbyBaseCreature.SummonMaster == from)
                    //Animate dead casting shouldn't hit familiars or player or pets
                    || (fromBaseCreature?.IsAnimatedDead == true && nearbyBaseCreature.IsNecroFamiliar)
                    || (fromBaseCreature?.IsAnimatedDead == true && nearbyBaseCreature.IsAnimatedDead)
                    || (fromBaseCreature?.IsAnimatedDead == true && nearbyBaseCreature.Summoned)
                    || (fromBaseCreature?.IsAnimatedDead == true && nearbyBaseCreature.Controlled)
                    //Event boss monsters shouldn't fight each other
                    || (fromBaseCreature?.Summoned == true && fromBaseCreature?.SummonMaster?.Player == false && nearbyBaseCreature.Summoned && nearbyBaseCreature.SummonMaster?.Player == false)
                ))
            {
                return false;
            }

            return true;
        }
    }
}
