using Morabara.Models;
using SFML.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Morabara.Logic
{
    public class GameLogic
    {
        #region properties & constructor

        public bool IsPlayerMove { get; set; }
        public bool IsFirstStage { get; private set; }
        public string ActionName { get; set; }
        public int PlayerPoints { get; private set; }
        public int ComputerPoints { get; private set; }
        public int MoveNumber { get; set; }
        public bool PlayerCanTakeComputerBall;

        private List<Field> fields;
        private List<int[]> possibleThrees;
        private bool IsPlayerStarting { get; }
        private int FirstStageRound { get; set; }
        private Random Random { get; }

        public GameLogic()
        {
            var dialogResult = MessageBox.Show("Do You want to start?", "Who has to start", MessageBoxButtons.YesNo);
            IsPlayerStarting = dialogResult != DialogResult.No;
            IsFirstStage = true;
            Random = new Random(DateTime.Now.Millisecond);
            ActionName = "Placing ball";

            InitFieldList();
            InitPossibleThrees();
            IsPlayerMove = IsPlayerStarting;

            if (!IsPlayerMove)
            {
                MakeComputerMove();
            }
        }

        #endregion properties & constructor

        private void MakeComputerMove()
        {
            if (IsFirstStage)
            {
                ActionName = "Placing ball";
                PlaceComputerBall();
            }
            else
            {
                ActionName = "Moving ball";
                MoveComputerBall();
            }
        }

        #region computer SI methods

        #region first stage

        private void PlaceComputerBall()
        {
            if (!GetIdsTakenBy(TakenBy.Nobody).Any()) return;

            Task firstStage = new Task(() =>
            {
                Thread.Sleep(1500);
                int? idFieldToPlaceBall = null;

                //check if can make three and make first found possibility
                idFieldToPlaceBall = GetIdOfThirdComputerFieldInLineOrNull();
                if (idFieldToPlaceBall != null)
                {
                    AssignBallTo(Convert.ToInt32(idFieldToPlaceBall), TakenBy.Computer);
                    SwitchMoveOrder();
                    Debug.WriteLine("Making three in line.");
                    TakePlayerBall();
                    return;
                }
                //check if player can make three and block it first found possibility
                idFieldToPlaceBall = GetIdOfThirdPlayerFieldInLineOrNull();
                if (idFieldToPlaceBall != null)
                {
                    AssignBallTo(Convert.ToInt32(idFieldToPlaceBall), TakenBy.Computer);
                    SwitchMoveOrder();
                    Debug.WriteLine("Prevent making three by Player.");
                    return;
                }

                //check if player can can make trap (trap is two possible three)
                idFieldToPlaceBall = GetIdOfPlayerTrapOrNull();
                if (idFieldToPlaceBall != null)
                {
                    AssignBallTo(Convert.ToInt32(idFieldToPlaceBall), TakenBy.Computer);
                    SwitchMoveOrder();
                    Debug.WriteLine("Prevent the player from creating a trap.");
                    return;
                }

                //try to create a trap
                idFieldToPlaceBall = GetIdOfFieldToMakeTrap();
                if (idFieldToPlaceBall != null)
                {
                    AssignBallTo(Convert.ToInt32(idFieldToPlaceBall), TakenBy.Computer);
                    SwitchMoveOrder();
                    Debug.WriteLine("Try to create a trap.");
                    return;
                }

                //try make a three
                idFieldToPlaceBall = GetIdForCreatingThreeOrNull();
                if (idFieldToPlaceBall != null)
                {
                    AssignBallTo(Convert.ToInt32(idFieldToPlaceBall), TakenBy.Computer);
                    SwitchMoveOrder();
                    Debug.WriteLine("Try to create three in line.");
                    return;
                }

                //else - random
                idFieldToPlaceBall = GetRandomFreeField();
                AssignBallTo(Convert.ToInt32(idFieldToPlaceBall), TakenBy.Computer);
                SwitchMoveOrder();
                Debug.WriteLine("Push ball at random place.");
            });

            firstStage.Start();
        }

        public int? GetIdOfThirdComputerFieldInLineOrNull()
        {
            var computerFieldsIds = fields.Where(cf => cf.TakenBy == TakenBy.Computer).Select(i => i.Id).ToList();

            foreach (var field in possibleThrees)
            {
                var intersected = computerFieldsIds.Intersect(field).ToList();
                if (intersected.Count != 2) continue;

                var third = field.First(f => computerFieldsIds.All(cf => cf != f));

                if (GetAssigment(third) != TakenBy.Nobody) continue;
                return third;
            }
            return null;
        }

        public int? GetIdOfThirdPlayerFieldInLineOrNull()
        {
            var playerIdsFields = fields.Where(pf => pf.TakenBy == TakenBy.Player).Select(i => i.Id).ToList();

            foreach (var field in possibleThrees)
            {
                var intersected = playerIdsFields.Intersect(field).ToList();
                if (intersected.Count != 2) continue;

                var third = field.First(f => playerIdsFields.All(cf => cf != f));

                if (GetAssigment(third) != TakenBy.Nobody) continue;
                return third;
            }
            return null;
        }

        public int? GetIdOfPlayerTrapOrNull()
        {
            var playerFields = GetIdsTakenBy(TakenBy.Player);
            var nobodyFields = GetIdsTakenBy(TakenBy.Nobody);

            //find where player have only one ball and rest of fields is unassigned
            var linesWhereOnlyPlayerHasOneBall = possibleThrees.Where(t => t.Intersect(playerFields).Count() == 1)
                .Where(t2 => t2.Intersect(nobodyFields).Count() == 2).ToList();

            //check if any of this lines interseted by nobodyField
            foreach (var line in linesWhereOnlyPlayerHasOneBall)
            {
                foreach (var line2 in linesWhereOnlyPlayerHasOneBall)
                {
                    if (line.Intersect(line2).Count() == 1 && GetAssigment(line2.Intersect(line).First()) == TakenBy.Nobody)
                    {
                        return line2.Intersect(line).First();
                    }
                }
            }

            return null;
        }

        public int? GetIdOfFieldToMakeTrap()
        {
            var computerFields = GetIdsTakenBy(TakenBy.Computer);
            var nobodyFields = GetIdsTakenBy(TakenBy.Nobody);
            //find where computer have only one ball and rest of fields is unassigned
            var linesWhereOnlyComputerHasOneBall = possibleThrees.Where(t => t.Intersect(computerFields).Count() == 1)
                .Where(t2 => t2.Intersect(nobodyFields).Count() == 2).ToList();

            //place third ball
            //check if any of this lines interseted by nobodyField
            foreach (var line in linesWhereOnlyComputerHasOneBall)
            {
                foreach (var line2 in linesWhereOnlyComputerHasOneBall)
                {
                    if (line.Intersect(line2).Count() == 1 && GetAssigment(line2.Intersect(line).First()) == TakenBy.Nobody)
                    {
                        return line2.Intersect(line).First();
                    }
                }
            }

            //place second ball
            //check if exist intersection by all unassigned line
            var linesWithoutBalls = possibleThrees.Where(t2 => t2.Intersect(nobodyFields).Count() == 3).ToList();
            foreach (var line in linesWhereOnlyComputerHasOneBall)
            {
                foreach (var line2 in linesWithoutBalls)
                {
                    if (line.Intersect(line2).Count() == 1)
                    {
                        return line2.First(it => it != line.Intersect(line2).First());
                    }
                }
            }

            return null;
        }

        public int? GetIdForCreatingThreeOrNull()
        {
            var computerFields = GetIdsTakenBy(TakenBy.Computer);
            var nobodyFields = GetIdsTakenBy(TakenBy.Nobody);
            //find where computer have only one ball and rest of fields is unassigned
            var linesWhereOnlyComputerHasOneBall = possibleThrees.Where(t => t.Intersect(computerFields).Count() == 1)
                .Where(t2 => t2.Intersect(nobodyFields).Count() == 2).ToList();

            return linesWhereOnlyComputerHasOneBall.FirstOrDefault()?.First(f => GetAssigment(f) == TakenBy.Nobody);
        }

        public int GetRandomFreeField()
        {
            var freeFields = GetIdsTakenBy(TakenBy.Nobody).ToList();

            var r = new Random(DateTime.Now.Millisecond);
            return freeFields.ElementAt(r.Next(0, freeFields.Count));
        }

        #endregion first stage

        #region removing player ball

        public void TakePlayerBall()
        {
            Task takeEnemyBall = new Task(() =>
            {
                Thread.Sleep(500);

                //when all player ball are in three computer can take one from any three
                var playerBalls = fields.Where(f => f.TakenBy == TakenBy.Player).ToList();
                if (playerBalls.All(b => b.BelongsToThree))
                {
                    RemoveAssignmentFromBall(playerBalls.FirstOrDefault(f => f.Id == (Random.Next(0, playerBalls.Count - 1))).Id);
                    Debug.WriteLine("All player ball making three so deleting random player ball.");
                    return;
                }

                int? idPlayerBallToRemove = null;
                idPlayerBallToRemove = GetIdOfBallConnectedPlayerTrap();
                if (idPlayerBallToRemove != null)
                {
                    RemoveAssignmentFromBall(Convert.ToInt32(idPlayerBallToRemove));
                    Debug.WriteLine("Removed player ball concatenated trap.");
                    return;
                }

                idPlayerBallToRemove = GetIdOfSecondBallWhichCouldMakeThree();
                if (idPlayerBallToRemove != null)
                {
                    RemoveAssignmentFromBall(Convert.ToInt32(idPlayerBallToRemove));
                    Debug.WriteLine("Removed player second ball concatenated trap.");
                    return;
                }

                idPlayerBallToRemove = GetIdOfRandomPlayerBall();
                RemoveAssignmentFromBall(Convert.ToInt32(idPlayerBallToRemove));
                Debug.WriteLine("Removed random player ball.");
            });

            takeEnemyBall.Start();
            takeEnemyBall.Wait();
        }

        public int? GetIdOfBallConnectedPlayerTrap()
        {
            var playerFields = GetIdsTakenBy(TakenBy.Player).ToList();
            var nobodyFields = GetIdsTakenBy(TakenBy.Nobody);

            //find where player have two ball and third is unassigned
            var linesWhereOnlyPlayerHasTwoBalls = possibleThrees.Where(t => t.Intersect(playerFields).Count() == 2)
                .Where(t2 => t2.Intersect(nobodyFields).Count() == 1).ToList();

            foreach (var l1 in linesWhereOnlyPlayerHasTwoBalls)
            {
                foreach (var l2 in linesWhereOnlyPlayerHasTwoBalls)
                {
                    var commonElements = l1.Intersect(l2).ToList();
                    if (commonElements.Count == 1 && playerFields.Contains(commonElements.First()))
                    {
                        return commonElements.First();
                    }
                }
            }

            return null;
        }

        public int? GetIdOfSecondBallWhichCouldMakeThree()
        {
            var playerFields = GetIdsTakenBy(TakenBy.Player);
            var nobodyFields = GetIdsTakenBy(TakenBy.Nobody);

            var firstLineWhereOnlyPlayerHasTwoBalls = possibleThrees
                .Where(t => t.Intersect(playerFields).Count() == 2)
                .FirstOrDefault(t2 => t2.Intersect(nobodyFields).Count() == 1);

            return firstLineWhereOnlyPlayerHasTwoBalls?.First(l => playerFields.Contains(l));
        }

        public int GetIdOfRandomPlayerBall()
        {
            var nonInThreePlayerBall = fields.Where(f => f.TakenBy == TakenBy.Player).Where(b => b.BelongsToThree == false).ToList();
            return nonInThreePlayerBall.ElementAt(Random.Next(0, nonInThreePlayerBall.Count - 1)).Id;
        }

        #endregion removing player ball

        #region second stage

        private void MoveComputerBall()
        {
            //TODO
            Task secondStage = new Task(() =>
            {
                Thread.Sleep(1500);
                Debug.WriteLine("Move computer ball.");
            });

            secondStage.Start();
        }

        #endregion second stage

        #endregion computer SI methods

        public void TakeComputerBall(int id)
        {
            RemoveAssignmentFromBall(id);
        }

        public void SwitchMoveOrder()
        {
            IsPlayerMove = !IsPlayerMove;
            if (!IsPlayerMove)
            {
                MakeComputerMove();
            }
        }

        public IEnumerable<CircleShape> GetAllBalls()
        {
            return fields.Select(s => s.Circle);
        }

        public IEnumerable<Field> GetAllFields()
        {
            return fields;
        }

        public void AssignBallTo(int id, TakenBy takenBy)
        {
            CheckAndIncrementRound(takenBy);

            fields.Find(f => f.Id == id).TakenBy = takenBy;

            if (takenBy == TakenBy.Nobody) return;
            CheckIfCreatedThree(id, takenBy);
        }

        private void CheckIfCreatedThree(int id, TakenBy takenBy)
        {
            var linesWhereFieldIdBelongs = possibleThrees.Where(t => t.Contains(id));
            var handleactionOfMakedThree = false;
            foreach (var line in linesWhereFieldIdBelongs)
            {
                if (!line.All(field => GetAssigment(field) == takenBy)) continue;
                foreach (var field in fields)
                {
                    if (!line.Contains(field.Id)) continue;

                    field.BelongsToThree = true;

                    if (!handleactionOfMakedThree)
                    {
                        if (takenBy == TakenBy.Player)
                        {
                            PlayerCanTakeComputerBall = true;
                            ActionName = "Take ball";
                            PlayerPoints++;
                        }
                        else if (takenBy == TakenBy.Computer)
                        {
                            ActionName = "Taking ball";
                            ComputerPoints++;
                        }

                        handleactionOfMakedThree = true;
                    }
                }
            }
        }

        private void RemoveAssignmentFromBall(int id)
        {
            fields.FirstOrDefault(f => f.Id == id).TakenBy = TakenBy.Nobody;

            //if not belongs to three - end method
            if (!fields.FirstOrDefault(f => f.Id == id).BelongsToThree) return;

            //but id, there is necessery to remove assigment to three for all three ball
            var threesWithThisElement = possibleThrees.Where(t1 => t1.Contains(id))
                .Where(t2 => t2.All(e => GetAssigment(e) == TakenBy.Player));

            foreach (var t in threesWithThisElement)
            {
                foreach (var e in t)
                {
                    fields.FirstOrDefault(f => f.Id == e).BelongsToThree = false; ;
                }
            }
        }

        private IEnumerable<int> GetIdsTakenBy(TakenBy takenBy)
        {
            return fields.Where(f => f.TakenBy == takenBy).Select(i => i.Id);
        }

        private TakenBy GetAssigment(int id)
        {
            return fields.Find(f => f.Id == id).TakenBy;
        }

        private void CheckAndIncrementRound(TakenBy takenBy)
        {
            if (FirstStageRound == Setting.NumberOfPlayerBall)
            {
                IsFirstStage = false;
                ActionName = "Moving ball";
            }
            else
            {
                if (IsPlayerStarting && takenBy == TakenBy.Player)
                {
                    FirstStageRound++;
                }
                else if (!IsPlayerStarting && takenBy == TakenBy.Computer)
                {
                    FirstStageRound++;
                }
            }
        }

        #region init methods

        private void InitPossibleThrees()
        {
            possibleThrees = new List<int[]>
            {
                //vertically |
                new [] {1, 2, 3},
                new [] {4,5,6},
                new [] {7,8,9},
                new [] {10,11,12},
                new [] {13,14,15},
                new [] {16,17,18},
                new [] {19,20,21},
                new [] {22,23,24},

                //horizontally -
                new [] {1,10,22},
                new [] {4,11,19},
                new [] {7,12,16},
                new [] {2,5,8},
                new [] {17,20,23},
                new [] {9,13,18},
                new [] {6,14,21},
                new [] {3,15,24},

                //obliquely /
                new [] {1,4,7},
                new [] {3,6,9},
                new [] {16,19,22},
                new [] {18,21,24}
            };
        }

        private void InitFieldList()
        {
            fields = new List<Field>
            {
                new Field(1, 21 - Setting.BallRadius + Setting.BoardMarginX, 16 - Setting.BallRadius + Setting.BoardMarginY),
                new Field(2, 237 - Setting.BallRadius + Setting.BoardMarginX, 16 - Setting.BallRadius + Setting.BoardMarginY),
                new Field(3, 457 - Setting.BallRadius + Setting.BoardMarginX, 16 - Setting.BallRadius + Setting.BoardMarginY),

                new Field(4, 95 - Setting.BallRadius + Setting.BoardMarginX, 90 - Setting.BallRadius + Setting.BoardMarginY),
                new Field(5, 236 - Setting.BallRadius + Setting.BoardMarginX, 90 - Setting.BallRadius + Setting.BoardMarginY),
                new Field(6, 383 - Setting.BallRadius + Setting.BoardMarginX, 90 - Setting.BallRadius + Setting.BoardMarginY),

                new Field(7, 167 - Setting.BallRadius + Setting.BoardMarginX, 162 - Setting.BallRadius + Setting.BoardMarginY),
                new Field(8, 237 - Setting.BallRadius + Setting.BoardMarginX, 162 - Setting.BallRadius + Setting.BoardMarginY),
                new Field(9, 307 - Setting.BallRadius + Setting.BoardMarginX, 162 - Setting.BallRadius + Setting.BoardMarginY),

                new Field(10, 21 - Setting.BallRadius + Setting.BoardMarginX, 232 - Setting.BallRadius + Setting.BoardMarginY),
                new Field(11, 95 - Setting.BallRadius + Setting.BoardMarginX, 232 - Setting.BallRadius + Setting.BoardMarginY),
                new Field(12, 167 - Setting.BallRadius + Setting.BoardMarginX, 232 - Setting.BallRadius + Setting.BoardMarginY),
                new Field(13, 307 - Setting.BallRadius + Setting.BoardMarginX, 232 - Setting.BallRadius + Setting.BoardMarginY),
                new Field(14, 387 - Setting.BallRadius + Setting.BoardMarginX, 232 - Setting.BallRadius + Setting.BoardMarginY),
                new Field(15, 457 - Setting.BallRadius + Setting.BoardMarginX, 232 - Setting.BallRadius + Setting.BoardMarginY),

                new Field(16, 167 - Setting.BallRadius + Setting.BoardMarginX, 302 - Setting.BallRadius + Setting.BoardMarginY),
                new Field(17, 237 - Setting.BallRadius + Setting.BoardMarginX, 302 - Setting.BallRadius + Setting.BoardMarginY),
                new Field(18, 307 - Setting.BallRadius + Setting.BoardMarginX, 302 - Setting.BallRadius + Setting.BoardMarginY),

                new Field(19, 95 - Setting.BallRadius + Setting.BoardMarginX, 378 - Setting.BallRadius + Setting.BoardMarginY),
                new Field(20, 236 - Setting.BallRadius + Setting.BoardMarginX, 378 - Setting.BallRadius + Setting.BoardMarginY),
                new Field(21, 383 - Setting.BallRadius + Setting.BoardMarginX, 378 - Setting.BallRadius + Setting.BoardMarginY),

                new Field(22, 21 - Setting.BallRadius + Setting.BoardMarginX, 452 - Setting.BallRadius + Setting.BoardMarginY),
                new Field(23, 237 - Setting.BallRadius + Setting.BoardMarginX, 452 - Setting.BallRadius + Setting.BoardMarginY),
                new Field(24, 457 - Setting.BallRadius + Setting.BoardMarginX, 452 - Setting.BallRadius + Setting.BoardMarginY)
            };
        }

        #endregion init methods
    }
}