using Server.Items;
using Server.Spells.Bushido;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Mobiles
{
    public class ThrowParalyzed: MonsterAbility
    {
        public override MonsterAbilityTrigger AbilityTrigger => MonsterAbilityTrigger.CombatAction;
        public override MonsterAbilityType AbilityType => MonsterAbilityType.ThrowParalyzed;

        public override TimeSpan MinTriggerCooldown => TimeSpan.FromSeconds(15.0);
        public override TimeSpan MaxTriggerCooldown => TimeSpan.FromSeconds(50.0);
        public virtual double ThrowDamageScale => Core.AOS ? 0.16 : 0.05;

        public virtual double ThrowTimer => 1.3;
        public virtual double ThrowEffectDelay => 1.3;
        public virtual int ThrowRange(BaseCreature source) => source.RangePerception;
        // Damage Types
        public virtual int ChaosDamage => 0;
        public virtual int PhysicalDamage => 100;
        public virtual int FireDamage => 0;
        public virtual int ColdDamage => 0;
        public virtual int PoisonDamage => 0;
        public virtual int EnergyDamage => 0;

        // Effect details and sound
        public virtual int ThrowEffectItemID => 0x573E;
        public virtual int ThrowEffectSpeed => 5;
        public virtual int ThrowEffectDuration => 0;
        public virtual bool ThrowEffectExplodes => false;
        public virtual bool ThrowEffectFixedDir => false;
        public virtual int ThrowEffectHue => 0;
        public virtual int ThrowEffectRenderMode => 0;

        public virtual int ThrowEffectSound => 0x227;

        public virtual int ThrowAngerAnimation => 4;

        public override bool CanTrigger(BaseCreature source, MonsterAbilityTrigger trigger) => !source.Summoned && base.CanTrigger(source, trigger);

        public override void Trigger(MonsterAbilityTrigger trigger, BaseCreature source, Mobile target)
        {
            if (!CanThrowOnTargetTarget(source, target)) return;

            ThrowStallMovement(source);
            Timer.StartTimer(TimeSpan.FromSeconds(ThrowEffectDelay / 2), () => StallEffect_Callback(source, target));


            base.Trigger(trigger, source, target);
        }

        private void StallEffect_Callback(BaseCreature source, Mobile target)
        {
            ThrowAngerSound(source);
            ThrowPlayAngerAnimation(source);
            ThrowStallMovement(source);

            source.Direction = source.GetDirectionTo(target);
            Timer.StartTimer(TimeSpan.FromSeconds(ThrowEffectDelay/2), () => ThrowEffect_Callback(source, target));
        }

        public virtual bool CanThrowOnTargetTarget(BaseCreature source, Mobile target)
        {
            var range = ThrowRange(source);

            return !source.IsDeadBondedPet && !source.BardPacified && target is { Alive: true, IsDeadBondedPet: false } &&
                   target.Map == source.Map && source.CanBeHarmful(target) && target.InRange(source, range) &&
                   source.InLOS(target);
        }
        public virtual int ThrowAngerSound(BaseCreature source) => source.GetAngerSound();

        public virtual void ThrowPlayAngerSound(BaseCreature source)
        {
            var sound = ThrowAngerSound(source);
            source.PlaySound(sound);
        }

        public virtual void ThrowStallMovement(BaseCreature source)
        {
            if (source.AIObject == null) return;

            source.AIObject.NextMove = Core.TickCount + (int)(ThrowTimer * 1000);
        }

        public virtual void ThrowPlayAngerAnimation(BaseCreature source)
        {
            source.Animate(ThrowAngerAnimation, 4, 1, true, false, 1);
        }

        public virtual void ThrowEffect_Callback(BaseCreature source, Mobile target)
        {
            if (!target.Alive || !source.CanBeHarmful(target))
            {
                return;
            }

            ThrowPlayEffectSound(source);
            ThrowPlayEffect(source, target);

            Timer.StartTimer(TimeSpan.FromSeconds(ThrowEffectDelay), () => ThrowDamage_Callback(source, target));
        }

        public virtual void ThrowDamage_Callback(BaseCreature source, Mobile target)
        {
            if (target is BaseCreature creature && creature.Paralyzed)
            {
                return;
            }

            if (source.CanBeHarmful(target))
            {
                source.DoHarmful(target);
                ThrowDealDamage(source, target);
            }
        }

        public virtual void ThrowDealDamage(BaseCreature source, Mobile target)
        {
            if (Evasion.CheckSpellEvasion(target)) return;

            target.Animate(20, 5, 1, false, false, 0);

            var physDamage = PhysicalDamage;
            var fireDamage = FireDamage;
            var coldDamage = ColdDamage;
            var poisDamage = PoisonDamage;
            var nrgyDamage = EnergyDamage;

            if (physDamage == 0 && fireDamage == 0 && coldDamage == 0 && poisDamage == 0 && nrgyDamage == 0)
            {
                target.Damage(ThrowComputeDamage(source), source); // Unresistable damage even in AOS
            }
            else
            {
                AOS.Damage(
                    target,
                    source,
                    ThrowComputeDamage(source),
                    physDamage,
                    fireDamage,
                    coldDamage,
                    poisDamage,
                    nrgyDamage
                );
            }

            target.Paralyze(TimeSpan.FromSeconds(3));
            target.PlaySound(0x204);
            target.FixedEffect(0x376A, 6, 1);
        }

        public virtual int ThrowComputeDamage(BaseCreature source)
        {
            var damage = (int)(source.Hits * ThrowDamageScale);

            if (source.IsParagon)
            {
                damage = (int)(damage / Paragon.HitsBuff);
            }

            return Math.Min(damage, 200);
        }

        public virtual void ThrowPlayEffectSound(BaseCreature source)
        {
            source.PlaySound(ThrowEffectSound);
        }

        public virtual void ThrowPlayEffect(BaseCreature source, Mobile target)
        {
            Effects.SendMovingEffect(
                source,
                target,
                ThrowEffectItemID,
                ThrowEffectSpeed,
                ThrowEffectDuration,
                ThrowEffectFixedDir,
                ThrowEffectExplodes,
                ThrowEffectHue,
                ThrowEffectRenderMode
            );
        }
    }
}
