using ElementEngine;
using ElementEngine.ECS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VagabondRL
{
    public static class AISystems
    {
        public static void PathingSystem(Group group, AStarPathfinder pathfinder)
        {
            foreach (var entity in group.Entities)
            {
                ref var transform = ref entity.GetComponent<TransformComponent>();
                ref var movement = ref entity.GetComponent<MovementComponent>();
                ref var physics = ref entity.GetComponent<PhysicsComponent>();

                if (movement.MovementPath.Count == 0)
                {
                    // Populate movement component with new path
                    List<AStarPathResult> Path;
                    if (pathfinder.GetPath(transform.Position, movement.Destination, out Path) == 
                        AStarPathResultType.Success)
                        foreach (AStarPathResult result in Path)
                            movement.MovementPath.Add(result.Position);

                    // Reset movement
                    movement.Start = transform.Position;
                    movement.Destination = movement.MovementPath[0];
                    movement.CurrentTargetIndex = 0;

                    // Set speed and aim towards current target
                    physics.Speed = PhysicsComponent.DefaultSpeed;
                    physics.Velocity = Vector2.Normalize(movement.ToCurrentTarget) * physics.Speed;
                }
            }
        }

        public static void MovementSystem(Group group)
        {
            foreach (var entity in group.Entities)
            {
                ref var movement = ref entity.GetComponent<MovementComponent>();

                // Are there Targets left to aim at?
                if (movement.CurrentTargetIndex < movement.MovementPath.Count)
                {
                    ref var transform = ref entity.GetComponent<TransformComponent>();
                    ref var physics = ref entity.GetComponent<PhysicsComponent>();

                    // Have we traveled further than we need to to get to the target?
                    float Dist = movement.ToCurrentTarget.Length();
                    float DistTraveled = (transform.Position - movement.PreviousTarget).Length();

                    // Yes, snap to target and head towards next target
                    if (DistTraveled > Dist)
                    {
                        transform.Position = movement.CurrentTarget;
                        movement.CurrentTargetIndex++;
                    }

                    // Aim towards current target
                    physics.Velocity = Vector2.Normalize(movement.ToCurrentTarget) * physics.Speed;
                }

            }
        }
    }
}
