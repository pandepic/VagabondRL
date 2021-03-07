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
        public static void PathingSystem(Group group, Entity tileMap, AStarPathfinder pathfinder)
        {
            ref var tilemapComponent = ref tileMap.GetComponent<TilemapComponent>();

            foreach (var entity in group.Entities)
            {
                ref var movement = ref entity.GetComponent<MovementComponent>();

                if (movement.MovementPath.Count == 0)
                {
                    // find a path to movement.Destination
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
