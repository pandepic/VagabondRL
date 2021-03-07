using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VagabondRL
{
    // empty tag components to filter between entity types
    public struct PlayerComponent { }
    public struct GuardComponent { }

    public struct MovementComponent
    {
        public List<Vector2> MovementPath;
        public Vector2 Destination;
    }

    public struct AStar
    {

    }
}
