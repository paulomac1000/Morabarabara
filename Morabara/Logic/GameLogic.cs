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
        public bool BallIsMoving { get; set; }
        public bool PlayerCanTakeComputerBall;

        private List<Field> fields;
        private List<int[]> possibleThrees;
        private List<Neighborhood> neighborhoods;
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

            initFieldList();
            initPossibleThrees();
            initNeighbors();
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
            if (!getIdsTakenBy(TakenBy.Nobody).Any()) return;

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

                if (getAssigment(third) != TakenBy.Nobody) continue;
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

                if (getAssigment(third) != TakenBy.Nobody) continue;
                return third;
            }
            return null;
        }

        public int? GetIdOfPlayerTrapOrNull()
        {
            var playerFields = getIdsTakenBy(TakenBy.Player);
            var nobodyFields = getIdsTakenBy(TakenBy.Nobody);

            //find where player have only one ball and rest of fields is unassigned
            var linesWhereOnlyPlayerHasOneBall = possibleThrees.Where(t => t.Intersect(playerFields).Count() == 1)
                .Where(t2 => t2.Intersect(nobodyFields).Count() == 2).ToList();

            //check if any of this lines interseted by nobodyField
            foreach (var line in linesWhereOnlyPlayerHasOneBall)
            {
                foreach (var line2 in linesWhereOnlyPlayerHasOneBall)
                {
                    if (line.Intersect(line2).Count() == 1 && getAssigment(line2.Intersect(line).First()) == TakenBy.Nobody)
                    {
                        return line2.Intersect(line).First();
                    }
                }
            }

            return null;
        }

        public int? GetIdOfFieldToMakeTrap()
        {
            var computerFields = getIdsTakenBy(TakenBy.Computer);
            var nobodyFields = getIdsTakenBy(TakenBy.Nobody);
            //find where computer have only one ball and rest of fields is unassigned
            var linesWhereOnlyComputerHasOneBall = possibleThrees.Where(t => t.Intersect(computerFields).Count() == 1)
                .Where(t2 => t2.Intersect(nobodyFields).Count() == 2).ToList();

            //place third ball
            //check if any of this lines interseted by nobodyField
            foreach (var line in linesWhereOnlyComputerHasOneBall)
            {
                foreach (var line2 in linesWhereOnlyComputerHasOneBall)
                {
                    if (line.Intersect(line2).Count() == 1 && getAssigment(line2.Intersect(line).First()) == TakenBy.Nobody)
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
            var computerFields = getIdsTakenBy(TakenBy.Computer);
            var nobodyFields = getIdsTakenBy(TakenBy.Nobody);
            //find where computer have only one ball and rest of fields is unassigned
            var linesWhereOnlyComputerHasOneBall = possibleThrees.Where(t => t.Intersect(computerFields).Count() == 1)
                .Where(t2 => t2.Intersect(nobodyFields).Count() == 2).ToList();

            return linesWhereOnlyComputerHasOneBall.FirstOrDefault()?.First(f => getAssigment(f) == TakenBy.Nobody);
        }

        public int GetRandomFreeField()
        {
            var freeFields = getIdsTakenBy(TakenBy.Nobody).ToList();

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
                    removeAssignmentFromBall(playerBalls.FirstOrDefault(f => f.Id == (Random.Next(0, playerBalls.Count - 1))).Id);
                    Debug.WriteLine("All player ball making three so deleting random player ball.");
                    return;
                }

                int? idPlayerBallToRemove = null;
                idPlayerBallToRemove = GetIdOfBallConnectedPlayerTrap();
                if (idPlayerBallToRemove != null)
                {
                    removeAssignmentFromBall(Convert.ToInt32(idPlayerBallToRemove));
                    Debug.WriteLine("Removed player ball concatenated trap.");
                    return;
                }

                idPlayerBallToRemove = GetIdOfSecondBallWhichCouldMakeThree();
                if (idPlayerBallToRemove != null)
                {
                    removeAssignmentFromBall(Convert.ToInt32(idPlayerBallToRemove));
                    Debug.WriteLine("Removed player second ball concatenated trap.");
                    return;
                }

                idPlayerBallToRemove = GetIdOfRandomPlayerBall();
                removeAssignmentFromBall(Convert.ToInt32(idPlayerBallToRemove));
                Debug.WriteLine("Removed random player ball.");
            });

            takeEnemyBall.Start();
            takeEnemyBall.Wait();
        }

        public int? GetIdOfBallConnectedPlayerTrap()
        {
            var playerFields = getIdsTakenBy(TakenBy.Player).ToList();
            var nobodyFields = getIdsTakenBy(TakenBy.Nobody);

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
            var playerFields = getIdsTakenBy(TakenBy.Player);
            var nobodyFields = getIdsTakenBy(TakenBy.Nobody);

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
            Task secondStage = new Task(() =>
            {
                //scenario 1 - move to create three, then TakePlayerBall();
                //scenario 2 - move to destroy three in way to not allow block by player
                //scenario 3 - move to destroy three
                //scenario 4 - move random

                Thread.Sleep(750);
                MoveOrSelectComputerBall(fields.FirstOrDefault(f => f.TakenBy == TakenBy.Computer).Id);

                Thread.Sleep(750);
                MoveOrSelectComputerBall(fields.FirstOrDefault(f => f.TakenBy == TakenBy.Nobody).Id);
            });

            secondStage.Start();
        }

        #endregion second stage

        #endregion computer SI methods

        public void TakeComputerBall(int id)
        {
            removeAssignmentFromBall(id);
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
            checkAndIncrementRound(takenBy);

            fields.Find(f => f.Id == id).TakenBy = takenBy;

            if (takenBy == TakenBy.Nobody) return;
            checkIfCreatedThree(id, takenBy);
        }

        public void CheckIsEndOfGame()
        {
            if (!IsFirstStage)
            {
                if (fields.Where(f => f.TakenBy == TakenBy.Computer).Count() == 2)
                {
                    Thread.Sleep(500);
                    MessageBox.Show("You won!");
                    WindowsStack.CloseLastWindow();
                }
                else if (fields.Where(f => f.TakenBy == TakenBy.Player).Count() == 2)
                {
                    Thread.Sleep(500);
                    MessageBox.Show("You lost!");
                    WindowsStack.CloseLastWindow();
                }
            }
        }

        private void checkIfDestroyedThree(int id, TakenBy takenBy)
        {
            var linesWhereFieldIdBelongs = possibleThrees.Where(t => t.Contains(id));
            var fieldsToRemove = new List<Field>();

            foreach (var line in linesWhereFieldIdBelongs)
            {
                //all have to belongs to three
                if (!line.All(field => fields.FirstOrDefault(f => f.Id == field).BelongsToThree)) continue;

                //two have to belongs to one player, one should be second player
                if (!(line.Where(l => getAssigment(l) == takenBy).Count() == 2 && line.Where(l => getAssigment(l) == TakenBy.Nobody).Count() == 1)) continue;

                //not deleteing assigment to three here because field could belong to two threes before move
                foreach (var it in line)
                {
                    fieldsToRemove.Add(fields.FirstOrDefault(f => f.Id == it));
                }
            }

            foreach (var f in fieldsToRemove)
            {
                f.BelongsToThree = false;
            }
        }

        private void checkIfCreatedThree(int id, TakenBy takenBy)
        {
            var linesWhereFieldIdBelongs = possibleThrees.Where(t => t.Contains(id));
            var handleActionOfMakedThree = false;
            foreach (var line in linesWhereFieldIdBelongs)
            {
                if (!line.All(field => getAssigment(field) == takenBy)) continue;
                foreach (var field in fields)
                {
                    if (!line.Contains(field.Id)) continue;

                    field.BelongsToThree = true;

                    if (!handleActionOfMakedThree)
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

                        handleActionOfMakedThree = true;
                    }
                }
            }
        }

        public void MoveOrSelectComputerBall(int id)
        {
            var searched = getSelectedBallOrNull();
            if (getSelectedBallOrNull() == null)
            {
                SelectOrDeselectBall(id);
                return;
            }
            var newPosition = fields.Find(f => f.Id == id);

            BallIsMoving = true;
            searched.Selected = false;
            searched.TakenBy = TakenBy.Nobody;
            checkIfDestroyedThree(id, TakenBy.Computer);

            newPosition.TakenBy = TakenBy.Computer;

            BallIsMoving = false;
            checkIfCreatedThree(newPosition.Id, newPosition.TakenBy);

            SwitchMoveOrder();
        }

        public bool MoveOrSelectPlayerBall(int id)
        {
            if (BallIsMoving)
            {
                Debug.WriteLine("You can't move/select ball when BallIsMoving = true.");
                return false;
            }

            var searched = getSelectedBallOrNull();
            if (getSelectedBallOrNull() == null || searched.Id == id)
            {
                SelectOrDeselectBall(id);
                return false;
            }
            var newPosition = fields.Find(f => f.Id == id);

            //new position has owner
            if (newPosition.TakenBy != TakenBy.Nobody)
            {
                Debug.WriteLine("You can't place ball in taken field");
                return false;
            }

            //new position is not neibor to old
            if (!neighborhoods.FirstOrDefault(n => n.Id == searched.Id).Neighbors.Contains(id))
            {
                Debug.WriteLine("You can't place ball in field which is not in neighborhood.");
                return false;
            }

            BallIsMoving = true;
            searched.Selected = false;
            searched.TakenBy = TakenBy.Nobody;
            checkIfDestroyedThree(id, TakenBy.Player);

            newPosition.TakenBy = TakenBy.Player;

            BallIsMoving = false;
            MoveNumber++;
            checkIfCreatedThree(newPosition.Id, newPosition.TakenBy);
            return true;
        }

        private Field getSelectedBallOrNull()
        {
            return fields.FirstOrDefault(f => f.Selected);
        }

        private void SelectOrDeselectBall(int id)
        {
            var searched = fields.FirstOrDefault(f => f.Id == id);
            if (IsPlayerMove)
            {
                if (searched.TakenBy != TakenBy.Player)
                {
                    Debug.WriteLine("You can't (de)select ball in field taken by Computer/Nobody");
                    return;
                }
            }
            else
            {
                if (searched.TakenBy != TakenBy.Computer)
                {
                    throw new Exception("Computer tried to select ball taken by Player/Nobody!");
                }
            }

            searched.Selected = !searched.Selected;
            Debug.WriteLine("Player (de)select ball.");
        }

        private void removeAssignmentFromBall(int id)
        {
            fields.FirstOrDefault(f => f.Id == id).TakenBy = TakenBy.Nobody;

            //if not belongs to three - end method
            if (!fields.FirstOrDefault(f => f.Id == id).BelongsToThree) return;

            //but id, there is necessery to remove assigment to three for all three ball
            var threesWithThisElement = possibleThrees.Where(t1 => t1.Contains(id))
                .Where(t2 => t2.All(e => getAssigment(e) == TakenBy.Player));

            foreach (var t in threesWithThisElement)
            {
                foreach (var e in t)
                {
                    fields.FirstOrDefault(f => f.Id == e).BelongsToThree = false;
                }
            }
        }

        private IEnumerable<int> getIdsTakenBy(TakenBy takenBy)
        {
            return fields.Where(f => f.TakenBy == takenBy).Select(i => i.Id);
        }

        private TakenBy getAssigment(int id)
        {
            return fields.Find(f => f.Id == id).TakenBy;
        }

        private void checkAndIncrementRound(TakenBy takenBy)
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

        private void initNeighbors()
        {
            neighborhoods = new List<Neighborhood>
            {
                new Neighborhood(1, new List<int>{ 2,4,10 }),
                new Neighborhood(2, new List<int>{ 1,3,5 }),
                new Neighborhood(3, new List<int>{ 2,6,15 }),

                new Neighborhood(4, new List<int>{ 1,5,7,11 }),
                new Neighborhood(5, new List<int>{ 2,4,6,8 }),
                new Neighborhood(6, new List<int>{ 3,5,9,15 }),

                new Neighborhood(7, new List<int>{ 4,8,12 }),
                new Neighborhood(8, new List<int>{ 5,7,9 }),
                new Neighborhood(9, new List<int>{ 6,8,13 }),

                new Neighborhood(10, new List<int>{ 1,11,22 }),
                new Neighborhood(11, new List<int>{ 4,10,12,19 }),
                new Neighborhood(12, new List<int>{ 7,11,16 }),

                new Neighborhood(13, new List<int>{ 9,14,18 }),
                new Neighborhood(14, new List<int>{ 6,13,15,21 }),
                new Neighborhood(15, new List<int>{ 3,14,24 }),

                new Neighborhood(16, new List<int>{ 12,17,19 }),
                new Neighborhood(17, new List<int>{ 16,18,20 }),
                new Neighborhood(18, new List<int>{ 13,17,21 }),

                new Neighborhood(19, new List<int>{ 11,16,20,22 }),
                new Neighborhood(20, new List<int>{ 17,19,21,23 }),
                new Neighborhood(21, new List<int>{ 14,18,20,24 }),

                new Neighborhood(22, new List<int>{ 10,19,23 }),
                new Neighborhood(23, new List<int>{ 20,22,24 }),
                new Neighborhood(24, new List<int>{ 15,21,23 })
            };

        }


        private void initPossibleThrees()
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

        private void initFieldList()
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