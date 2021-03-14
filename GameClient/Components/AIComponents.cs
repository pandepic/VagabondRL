using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VagabondRL
{
    public enum GuardStateType
    {
        Patrol,
        Chase,
    }

    public struct PlayerComponent { }

    public struct GuardComponent
    {
        public GuardStateType State;
    }

    public struct GuardSensesComponent
    {
    }

    public struct GuardMemoryComponent
    {
    }

    public struct MovementComponent
    {
        public Vector2 Destination;
        public Vector2 NextPoint;
        public List<Vector2> MovementPath;
    }
}
