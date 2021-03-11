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

                if (entity.HasComponent<GuardComponent>())
                {
                    // Has the entity exhausted its current movement path?
                    if (movement.MovementPath.Count == 0)
                    {
                        // Populate movement component with new path
                        List<AStarPathResult> Path;
                        if (pathfinder.GetPath(transform.Position, movement.Destination, out Path) ==
                            AStarPathResultType.Success)
                            foreach (AStarPathResult result in Path)
                                movement.MovementPath.Add(result.Position);
                    }
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
