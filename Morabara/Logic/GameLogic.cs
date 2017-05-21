using System;
using System.Linq;
using Morabara.Models;
using System.Threading;
using System.Threading.Tasks;

namespace Morabara.Logic
{
    public class GameLogic : Board
    {
        public bool IsPlayerMove { get; private set; }

        public GameLogic()
        {
            IsPlayerMove = true;
        }

        public void SwitchMoveOrder()
        {
            IsPlayerMove = !IsPlayerMove;
        }

        public void MakeComputerMove()
        {
            var computerMove = Task.Run(() => {
                Thread.Sleep(1500);

                //check if can make three and make first found possibility
                var third =  GetIdOfThirdFieldInLineOrNull();
                if (third != null)
                {
                    AssignBallTo(Convert.ToInt32(third), TakenBy.Computer);
                    SwitchMoveOrder();
                    return;
                }
                //chceck if player can make three and block it first found possibility
                //check if player can can make trap (trap is two possible three)
                //try to make trap
                //else - random

                var freeFields = GetIdsTakenBy(TakenBy.Nobody).ToList();

                var r = new Random(DateTime.Now.Millisecond);
                AssignBallTo(freeFields.ElementAt(r.Next(0, freeFields.Count)), TakenBy.Computer);

                SwitchMoveOrder();
            });
        }
    }
}