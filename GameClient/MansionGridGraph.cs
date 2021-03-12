using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElementEngine;

namespace VagabondRL
{
    public class MansionGridGraph : AStarGridGraph
    {
        CollisionType[] Collisions;

        public MansionGridGraph(int width, int height, CollisionType[] collisions)
            :base(AStarGridGraphType.EightEdges, width, height)
        {
            Collisions = collisions;
        }

        public override bool IsNodeBlocked(AStarNode node, Vector2I end)
        {
            return node == null ||
                Collisions[node.Position.X * Width + node.Position.Y] == CollisionType.Blocked;
        }

    }
}
