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

                // Has the entity exhausted its current movement path?
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
                ref var transform = ref entity.GetComponent<TransformComponent>();
                ref var physics = ref entity.GetComponent<PhysicsComponent>();

                // Are there Targets left to aim at?
                if (movement.CurrentTargetIndex < movement.MovementPath.Count)
                {
                    // Have we traveled further than we need to to get to the target?
                    float DistSq = movement.ToCurrentTarget.LengthSquared();
                    float DistTraveledSq = (transform.Position - movement.PreviousTarget).LengthSquared();

                    // Yes, snap to target and head towards next target
                    if (DistTraveledSq >= DistSq)
                    {
                        transform.Position = movement.CurrentTarget;
                        movement.CurrentTargetIndex++;
                    }

                    // Aim towards current target
                    physics.Velocity = Vector2.Normalize(movement.ToCurrentTarget) * physics.Speed;
                }
                // There are no targets left to aim at. Stop. Stop it.
                else
                {
                    physics.Velocity = Vector2.Zero;
                }

            }
        }
    }
}
