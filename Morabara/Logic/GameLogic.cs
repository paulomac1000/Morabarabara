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
                int? idFieldToPlaceBall = null;

                //check if can make three and make first found possibility
                idFieldToPlaceBall =  GetIdOfThirdComputerFieldInLineOrNull();
                if (idFieldToPlaceBall != null)
                {
                    AssignBallTo(Convert.ToInt32(idFieldToPlaceBall), TakenBy.Computer);
                    SwitchMoveOrder();
                    return;
                }
                //chceck if player can make three and block it first found possibility
                idFieldToPlaceBall = GetIdOfThirdPlayerFieldInLineOrNull();
                if (idFieldToPlaceBall != null)
                {
                    AssignBallTo(Convert.ToInt32(idFieldToPlaceBall), TakenBy.Computer);
                    SwitchMoveOrder();
                    return;
                }

                //check if player can can make trap (trap is two possible three)
                idFieldToPlaceBall = GetIdOfPlayerTrapOrNull();
                if (idFieldToPlaceBall != null)
                {
                    AssignBallTo(Convert.ToInt32(idFieldToPlaceBall), TakenBy.Computer);
                    SwitchMoveOrder();
                    return;
                }

                //try to make trap
                idFieldToPlaceBall = GetIdOfFieldToMakeTrap();
                if (idFieldToPlaceBall != null)
                {
                    AssignBallTo(Convert.ToInt32(idFieldToPlaceBall), TakenBy.Computer);
                    SwitchMoveOrder();
                    return;
                }

                //else - random
                idFieldToPlaceBall = GetRandomFreeField();
                AssignBallTo(Convert.ToInt32(idFieldToPlaceBall), TakenBy.Computer);
                SwitchMoveOrder();
            });
        }
    }
}