using ElementEngine;
using ElementEngine.ECS;
using System;
using System.Collections.Generic;
using System.Linq;
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

                if (movement.MovementPath.Count == 0)
                {
                    List<AStarPathResult> Path;
                    if (pathfinder.GetPath(transform.Position, movement.Destination, out Path) == 
                        AStarPathResultType.Success)
                        foreach (AStarPathResult result in Path)
                            movement.MovementPath.Add(result.Position);
                }
            }
        }

        public static void MovementSystem(Group group)
        {
            foreach (var entity in group.Entities)
            {
                ref var movement = ref entity.GetComponent<MovementComponent>();

                
                // do stuff to move them through the movement path
            }
        }
    }
}
