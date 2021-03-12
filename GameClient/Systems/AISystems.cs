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
        public static void GuardAISystem(Group group, Entity player)
        {
            ref var playerTransform = ref player.GetComponent<TransformComponent>();

            foreach (var entity in group.Entities)
            {
                ref var movement = ref entity.GetComponent<MovementComponent>();

                movement.Destination = playerTransform.Position;
            }
        }

        public static void PathingSystem(Group group, AStarPathfinder pathfinder)
        {
            foreach (var entity in group.Entities)
            {
                ref var transform = ref entity.GetComponent<TransformComponent>();
                ref var movement = ref entity.GetComponent<MovementComponent>();

                // Has the entity exhausted its current movement path?
                if (movement.MovementPath.Count == 0)
                {
                    // Populate movement component with new path
                    List<AStarPathResult> Path = new List<AStarPathResult>();
                    Vector2 TilePosition = transform.Position / 16;
                    Vector2 DestinationTilePosition = movement.Destination / 16;

                    //AStarPathResultType Result =
                    //    pathfinder.GetPath(TilePosition, DestinationTilePosition, out Path);

                    //if (Result == AStarPathResultType.Success)
                    //    foreach (AStarPathResult result in Path)
                    //        movement.MovementPath.Add(result.Position);
                }

            }
        }

        public static void MovementSystem(Group group, GameTimer gameTimer)
        {
            foreach (var entity in group.Entities)
            {
                ref var movement = ref entity.GetComponent<MovementComponent>();
                ref var transform = ref entity.GetComponent<TransformComponent>();
                ref var physics = ref entity.GetComponent<PhysicsComponent>();

                if (movement.MovementPath.Count > 0)
                {
                    Vector2 ToTarget = movement.CurrentTarget - movement.PreviousTarget;
                    Vector2 ToTargetDir = Vector2.Normalize(ToTarget);
                    float Distance = ToTarget.Length();
                    Vector2 Traveled = transform.Position - movement.PreviousTarget;
                    float DistanceTraveled = Traveled.Length();

                    // for Linear Interpolation
                    float T = Distance / DistanceTraveled;

                    // Not yet reached destination
                    if (T < 1.0f)
                    {
                        physics.Velocity = ToTarget * physics.Speed;
                    }
                    // reached destination
                    else
                    {
                        movement.CurrentTargetIndex += 1;
                    }
                }
               

            }
        }

        public static void GuardSenseSystem(Group group)
        {
            foreach (var entity in group.Entities)
            {

            }
        }

        public static void AreaSoundSystem(AreaSoundsManager areaSoundsManager, GameTimer gameTimer)
        {
            foreach (AreaSound sound in areaSoundsManager.Sounds)
            {
                sound.SoundTimer.Update(gameTimer);
                if (sound.SoundTimer.TicThisUpdate)
                {
                    sound.Level -= 1;

                    if (sound.Level == 0)
                    {
                        areaSoundsManager.Sounds.Remove(sound);
                    }
                }
            }
        }
    }
}
