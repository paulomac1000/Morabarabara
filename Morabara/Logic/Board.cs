using Morabara.Models;
using SFML.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace Morabara.Logic
{
    public class Board
    {
        private List<Field> fields;

        public Board()
        {
            InitFieldList();
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

        private void InitFieldList()
        {
            fields = new List<Field>
            {
                new Field(1, 21 - Setting.BallRadius + Setting.BoardMarginX, 16 - Setting.BallRadius + Setting.BoardMarginY),
                new Field(2, 237 - Setting.BallRadius + Setting.BoardMarginX, 16 - Setting.BallRadius + Setting.BoardMarginY),
                new Field(3, 457 - Setting.BallRadius + Setting.BoardMarginX, 16 - Setting.BallRadius + Setting.BoardMarginY),
            };
        }
    }
}