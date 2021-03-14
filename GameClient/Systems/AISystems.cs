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

        public static void GuardAISystem(Group group, Entity player)
        {
            ref var playerTransform = ref player.GetComponent<TransformComponent>();

            foreach (var entity in group.Entities)
            {
                ref var movement = ref entity.GetComponent<MovementComponent>();

                movement.Destination = playerTransform.Position;
            }
        }

        public static void PathingSystem(Group group, AStarPathfinder pathfinder, Entity tilemap)
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

                    //// Populate movement component with new path
                    //List<AStarPathResult> Path = new List<AStarPathResult>();
                    //Vector2 TilePosition = transform.Position / 16;
                    //Vector2 DestinationTilePosition = movement.Destination / 16;

                    //AStarPathResultType Result =
                    //    pathfinder.GetPath(TilePosition, DestinationTilePosition, out Path);

                    //if (Result == AStarPathResultType.Success)
                    //    foreach (AStarPathResult result in Path)
                    //        movement.MovementPath.Add(result.Position * 16);
                }

            }
        }

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

                        //var movementVector = movement.NextPoint - (transform.Position + positionOffset);
                        //physics.Velocity = Vector2.Normalize(movementVector) * physics.Speed;
                    }
                }
            }
        } // MovementSystem

        public static void GuardSenseSystem(Group group)
        {
            foreach (var entity in group.Entities)
            {
            }
        }

        //public static void AreaSoundSystem(AreaSoundsManager areaSoundsManager, GameTimer gameTimer)
        //{
        //    foreach (AreaSound sound in areaSoundsManager.Sounds)
        //    {
        //        sound.SoundTimer.Update(gameTimer);
        //        if (sound.SoundTimer.TicThisUpdate)
        //        {
        //            sound.Level -= 1;

        //            if (sound.Level == 0)
        //            {
        //                areaSoundsManager.Sounds.Remove(sound);
        //            }
        //        }
        //    }
        //}
    }
}
