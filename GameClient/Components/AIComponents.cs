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

    public struct GuardStateComponent
    {
        public enum GuardStateType
        {
            PATROL,
            CAUTIOUS,
            ALERT
        }

        GuardStateType State;
    }

    public struct GuardSensesComponent
    {

    }

    public struct GuardMemoryComponent
    {

    }



    public struct MovementComponent
    {
        public static float DefaultSpeed = 50.0f;

        public Vector2 Start;
        public Vector2 Destination;

        public List<Vector2> MovementPath;
        public int CurrentTargetIndex;

        public Vector2 CurrentTarget => MovementPath[CurrentTargetIndex];
        public Vector2 PreviousTarget
            => (CurrentTargetIndex > 0) ?
                MovementPath[CurrentTargetIndex - 1] :
                Start;
        public Vector2 ToCurrentTarget => CurrentTarget - PreviousTarget;
    }
}
