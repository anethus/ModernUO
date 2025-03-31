using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.Mobiles
{
    public abstract partial class RageCreature : BaseCreature
    {
        public List<RageMobile> RageMeters = new();
        public Mobile TauntedBy = null;

        public RageCreature(AIType ai, FightMode mode = FightMode.RageLevel, int iRangePerception = 16, int iRangeFight = 1) : base(ai, mode, iRangePerception, iRangeFight)
        {
        }

        public RageCreature(Serial serial): base(serial)
        {

        }

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            // TODO: Add Modifier
            var element = RageMeters.Find(x => x.Source.Serial == from.Serial);
            if (element != null)
            {
                element.RageLevel += amount;
            }
            else
            {
                RageMeters.Add(new RageMobile(from, amount));
            }

            RageMeters.Sort((x, y) => y.RageLevel.CompareTo(x.RageLevel));

            DebugSay($"======================");
            DebugSay($"RAGE level for {from.Name} = {RageMeters.Where(x => x.Source.Serial == from.Serial).FirstOrDefault().RageLevel}");
            DebugSay($"RAGE Najbardziej mnie wkurwia: {RageMeters.FirstOrDefault().Source.Name}");
            DebugSay($"======================");

            base.OnDamage(amount, from, willKill);
        }

        public void ClearFromRageMeter(Mobile mobile)
        {
            var element = RageMeters.Find(x => x.Source.Serial == mobile.Serial);
            if (element != null)
            {
                RageMeters.Remove(element);
                RageMeters.Sort((x, y) => x.RageLevel.CompareTo(y.RageLevel));
            }
        }

        public Mobile GetTopRageMobile()
        {
            return RageMeters.FirstOrDefault()?.Source;
        }
    }

    public class RageMobile : IEqualityComparer<RageMobile>
    {
        public Mobile Source;
        public int RageLevel;

        public RageMobile(Mobile mobile, int rageLevel)
        {
            Source = mobile;
            RageLevel = rageLevel;
        }

        public bool Equals(RageMobile x, RageMobile y)
        {
            if (x == null || y == null)
            {
                return false;
            }

            return x.Source.Serial == y.Source.Serial;
        }

        public int GetHashCode([DisallowNull] RageMobile obj)
        {
            return obj?.Source.GetHashCode() ?? 0;
        }
    }
}
