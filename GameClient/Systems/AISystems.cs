using ElementEngine;
using ElementEngine.ECS;
using SharpNeat.Utility;
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
        private static FastRandom _rng = new FastRandom();

        public static void GuardBehaviourSystem(Group group, Entity player, Entity tilemap)
        {
            ref var tilemapComponent = ref tilemap.GetComponent<TilemapComponent>();
            ref var playerTransform = ref player.GetComponent<TransformComponent>();

            foreach (var entity in group.Entities)
            {
                ref var guard = ref entity.GetComponent<GuardComponent>();
                guard.State = GuardStateType.Patrol;
            }

            var playerTile = GeneralSystems.GetEntityTile(player);
            var index = playerTile.X + tilemapComponent.Width * playerTile.Y;

            if (tilemapComponent.GuardsVisible[index] > -1)
            {
                foreach (var entity in group.Entities)
                {
                    ref var guard = ref entity.GetComponent<GuardComponent>();
                    ref var movement = ref entity.GetComponent<MovementComponent>();
                    guard.State = GuardStateType.Chase;
                    movement.Destination = Vector2.Zero;
                    movement.MovementPath.Clear();
                    movement.NextPoint = Vector2.Zero;
                }
            }
        } // GuardBehaviourSystem

        public static void PathingSystem(Group group, AStarPathfinder pathfinder, Entity tilemap, Entity player)
        {
            ref var tilemapComponent = ref tilemap.GetComponent<TilemapComponent>();

            foreach (var entity in group.Entities)
            {
                ref var transform = ref entity.GetComponent<TransformComponent>();
                ref var movement = ref entity.GetComponent<MovementComponent>();
                ref var guard = ref entity.GetComponent<GuardComponent>();

                // Has the entity exhausted its current movement path?
                if (movement.MovementPath.Count == 0 && movement.Destination == Vector2.Zero)
                {
                    var entityTile = GeneralSystems.GetEntityTile(entity);

                    if (guard.State == GuardStateType.Patrol)
                    {
                        var patrolDestinationTile = tilemapComponent.RoomCenters[_rng.Next(0, tilemapComponent.RoomCenters.Length)];
                        var patrolDestination = patrolDestinationTile * MapGenerator.TileSize;

                        var result = pathfinder.GetPath(entityTile, patrolDestinationTile, out var path);

                        if (result == AStarPathResultType.Success)
                        {
                            movement.Destination = patrolDestination;

                            foreach (var resultTile in path)
                                movement.MovementPath.Add(resultTile.Position * MapGenerator.TileSize);
                        }
                    }
                    else if (guard.State == GuardStateType.Chase)
                    {
                        var playerTile = GeneralSystems.GetEntityTile(player);

                        var result = pathfinder.GetPath(entityTile, playerTile, out var path);

                        if (result == AStarPathResultType.Success)
                        {
                            movement.Destination = playerTile * MapGenerator.TileSize;

                            if (path.Count > 0)
                                movement.MovementPath.Add(path[0].Position * MapGenerator.TileSize);
                        }
                    }
                }
            }
        } // PathingSystem

        public static void MovementSystem(Group group, GameTimer gameTimer)
        {
            var positionOffset = new Vector2(0, MapGenerator.TileSize.Y);

            foreach (var entity in group.Entities)
            {
                ref var movement = ref entity.GetComponent<MovementComponent>();
                ref var transform = ref entity.GetComponent<TransformComponent>();
                ref var physics = ref entity.GetComponent<PhysicsComponent>();

                if (movement.MovementPath.Count > 0 && movement.NextPoint == Vector2.Zero)
                {
                    var nextPoint = movement.MovementPath.GetLastItem();
                    movement.NextPoint = nextPoint;
                    movement.MovementPath.Remove(nextPoint);
                }

                if (movement.NextPoint != Vector2.Zero)
                {
                    if (movement.NextPoint.GetDistance(transform.Position + positionOffset) < 1f)
                    {
                        movement.NextPoint = Vector2.Zero;

                        if (movement.MovementPath.Count == 0)
                            movement.Destination = Vector2.Zero;
                    }
                    else
                    {
                        var direction = Vector2.Zero;
                        var offsetPosition = transform.Position + positionOffset;

                        if (offsetPosition.X > movement.NextPoint.X)
                            direction.X = -1;
                        else if (offsetPosition.X < movement.NextPoint.X)
                            direction.X = 1;

                        if (offsetPosition.Y > movement.NextPoint.Y)
                            direction.Y = -1;
                        else if (offsetPosition.Y < movement.NextPoint.Y)
                            direction.Y = 1;

                        physics.Velocity = direction * physics.Speed;
                    }
                }
            }
        } // MovementSystem

    } // AISystems
}
