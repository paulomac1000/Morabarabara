using System.Collections.Generic;

namespace Morabara.Models
{
    public class Neighborhood
    {
        public Neighborhood(int id, IEnumerable<int> neighbors)
        {
            Id = id;
            Neighbors = neighbors;
        }

        public int Id { get; set; }
        public IEnumerable<int> Neighbors { get; set; }
    }
}
