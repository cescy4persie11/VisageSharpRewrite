using Ensage;
using Ensage.Common;
using Ensage.Common.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisageSharpRewrite.Abilities;

namespace VisageSharpRewrite.Features
{
    public class Follow
    {

        private Hero me
        {
            get
            {
                return Variables.Hero;
            }          
        }

        private FamiliarControl familiarControl
        {
            get
            {
                return Variables.familiarControl;
            }
        }

        public Follow()
        {

        }

        public void Execute(List<Unit> familiars)
        {
            if (!familiarControl.AnyFamiliarNearMe(familiars, 500))
            {
                if (Utils.SleepCheck("fmove"))
                {
                    foreach (var f in familiars)
                    {
                        if (f.CanMove())
                        {
                            f.Follow(me);
                        }
                    }
                    Utils.Sleep(100, "fmove");
                }
            }
        }

        public void PlayerExecution(ExecuteOrderEventArgs args, List<Unit> familiars)
        {
            if (familiarControl.AnyFamiliarNearMe(familiars, 500))
            {
                if (args.Order == Order.MoveLocation)
                {
                    foreach (var f in familiars)
                    {
                        if (f.CanMove())
                        {
                            f.Move(Game.MousePosition);
                        }
                    }
                }
            }
        }


    }
}
