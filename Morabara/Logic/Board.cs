using System;
using Morabara.Models;
using SFML.Graphics;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Morabara.Logic
{
    public class Board
    {
        private List<Field> fields;
        private List<int[]> possibleThrees;

        public Board()
        {
            InitFieldList();
            InitPossibleThrees();
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
            fields.Find(f => f.Id == id).TakenBy = takenBy;
        }

        public void RemoveAssignmentFromBall(int id)
        {
            fields.Find(f => f.Id == id).TakenBy = TakenBy.Nobody;
        }

        public IEnumerable<int> GetIdsTakenBy(TakenBy takenBy)
        {
            return fields.Where(f => f.TakenBy == takenBy).Select(i => i.Id);
        }

        public TakenBy GetAssigment(int id)
        {
            return fields.Find(f => f.Id == id).TakenBy;
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

        public int GetRandomFreeFIeld()
        {
            var freeFields = GetIdsTakenBy(TakenBy.Nobody).ToList();

            var r = new Random(DateTime.Now.Millisecond);
            return freeFields.ElementAt(r.Next(0, freeFields.Count));
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
    }
}