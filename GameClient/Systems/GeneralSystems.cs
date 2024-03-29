﻿using ElementEngine;
using ElementEngine.ECS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace VagabondRL
{
    public static class GeneralSystems
    {
        private struct DrawItem
        {
            public Vector2 Position;
            public Vector2 Origin;
            public Vector2 Scale;
            public float Rotation;
            public Rectangle SourceRect;
            public Texture2D Texture;
            public int Layer;
        }

        private static List<DrawItem> _drawList = new List<DrawItem>();

        public static void DrawableSystem(Group group, SpriteBatch2D spriteBatch, Camera2D camera)
        {
            var cameraView = camera.ScaledView;

            _drawList.Clear();

            foreach (var entity in group.Entities)
            {
                ref var drawable = ref entity.GetComponent<DrawableComponent>();
                ref var transform = ref entity.GetComponent<TransformComponent>();

                if (!drawable.IsVisible)
                    continue;

                var pos = (transform.TransformedPosition - drawable.Origin).ToVector2I();
                var entityRect = new Rectangle(pos, drawable.AtlasRect.Size);

                if (entityRect.Intersects(cameraView))
                {
                    _drawList.Add(new DrawItem()
                    {
                        Position = transform.TransformedPosition.ToVector2I().ToVector2(),
                        Origin = drawable.Origin,
                        Scale = drawable.Scale,
                        Rotation = transform.Rotation,
                        SourceRect = drawable.AtlasRect,
                        Texture = drawable.Texture,
                        Layer = drawable.Layer,
                    });
                }
            }

            if (_drawList.Count > 0)
            {
                // sort by layer then Y position
                _drawList.Sort((x, y) =>
                {
                    var val = x.Layer.CompareTo(y.Layer);

                    if (val == 0)
                        val = x.Position.Y.CompareTo(y.Position.Y);

                    return val;
                });

                foreach (var item in _drawList)
                    spriteBatch.DrawTexture2D(item.Texture, item.Position, item.SourceRect, item.Scale, item.Origin, item.Rotation);
            }

        } // DrawableSystem

        public static Vector2I GetEntityTile(Entity player)
        {
            ref var transform = ref player.GetComponent<TransformComponent>();
            var playerTile = transform.TransformedPosition.ToVector2I() / MapGenerator.TileSize;
            playerTile.Y += 1;

            return playerTile;
        }

        private static List<Vector2I> _collisionCheckList = new List<Vector2I>();

        private static bool CheckCollisions(TransformComponent transform, TilemapComponent tilemapComponent)
        {
            var entityRect = new Rectangle(transform.Position.ToVector2I() + new Vector2I(0, MapGenerator.TileSize.Y), MapGenerator.TileSize);

            _collisionCheckList.Clear();
            _collisionCheckList.Add(entityRect.Location / MapGenerator.TileSize);
            _collisionCheckList.Add(entityRect.TopRight / MapGenerator.TileSize);
            _collisionCheckList.Add(entityRect.BottomLeft / MapGenerator.TileSize);
            _collisionCheckList.Add(entityRect.BottomRight / MapGenerator.TileSize);

            foreach (var check in _collisionCheckList)
            {
                if (check.X < 0 || check.X >= tilemapComponent.Width
                    || check.Y < 0 || check.Y >= tilemapComponent.Height)
                {
                    return true;
                }

                var index = check.X + tilemapComponent.Width * check.Y;

                if (tilemapComponent.Collisions[index] == CollisionType.Blocked)
                    return true;
            }

            return false;
        }

        public static void PhysicsSystem(Group group, GameTimer gameTimer, Entity tilemap)
        {
            ref var tilemapComponent = ref tilemap.GetComponent<TilemapComponent>();

            foreach (var entity in group.Entities)
            {
                ref var transform = ref entity.GetComponent<TransformComponent>();
                ref var physics = ref entity.GetComponent<PhysicsComponent>();

                if (physics.Velocity == Vector2.Zero)
                    continue;

                var prevPosition = transform.Position;
                transform.Position.X += physics.Velocity.X * gameTimer.DeltaS;

                if (entity.HasComponent<ColliderComponent>())
                {
                    if (CheckCollisions(transform, tilemapComponent))
                        transform.Position = prevPosition;
                }

                prevPosition = transform.Position;
                transform.Position.Y += physics.Velocity.Y * gameTimer.DeltaS;

                if (entity.HasComponent<ColliderComponent>())
                {
                    if (CheckCollisions(transform, tilemapComponent))
                        transform.Position = prevPosition;
                }
            }
        }

        public static void FourDirectionSystem(Group group, GameTimer gameTimer)
        {
            foreach (var entity in group.Entities)
            {
                ref var physics = ref entity.GetComponent<PhysicsComponent>();
                ref var drawable = ref entity.GetComponent<DrawableComponent>();
                ref var four = ref entity.GetComponent<FourDirectionComponent>();

                if (physics.Velocity != Vector2.Zero)
                {
                    // Update facing
                    if (physics.Velocity.Y < 0) four.Facing = FacingType.Up;
                    if (physics.Velocity.Y > 0) four.Facing = FacingType.Down;
                    if (physics.Velocity.X < 0) four.Facing = FacingType.Left;
                    if (physics.Velocity.X > 0) four.Facing = FacingType.Right;

                    switch (four.Facing)
                    {
                        case FacingType.Up: drawable.AtlasRect.Y = 0; break;
                        case FacingType.Down: drawable.AtlasRect.Y = 32; break;
                        case FacingType.Left: drawable.AtlasRect.Y = 64; break;
                        case FacingType.Right: drawable.AtlasRect.Y = 96; break;
                    }

                    four.CurrentFrameTime += gameTimer.DeltaS;

                    if (four.CurrentFrameTime >= four.BaseFrameTime)
                    {
                        four.CurrentFrameTime = 0f;
                        four.CurrentFrame += 1;

                        if (four.CurrentFrame > 3)
                            four.CurrentFrame -= 4;
                    }
                }
                else
                {
                    four.CurrentFrame = 0;
                }

                drawable.AtlasRect.X = four.CurrentFrame * 16;
            }
        }

        private static List<Vector2I> _visionList = new List<Vector2I>();

        private static bool CanPlayerSeeEntity(Entity player, Entity entity, VisionComponent vision, TilemapComponent tilemapComponent)
        {
            var playerTile = GetEntityTile(player);
            var entityTile = GetEntityTile(entity);

            if (Vector2I.GetDistance(playerTile, entityTile) > vision.Range)
                return false;

            var lineTiles = Bresenham.GetLinePoints(playerTile, entityTile);
            lineTiles.Sort((t1, t2) => Vector2I.GetDistance(playerTile, t1).CompareTo(Vector2I.GetDistance(playerTile, t2)));

            var blocked = false;

            foreach (var tile in lineTiles)
            {
                // check if tile is out of bounds
                if (tile.X < 0 || tile.X >= tilemapComponent.Width
                    || tile.Y < 0 || tile.Y >= tilemapComponent.Height)
                {
                    continue;
                }

                var index = tile.X + tilemapComponent.Width * tile.Y;
                if (tilemapComponent.Collisions[index] == CollisionType.Blocked)
                    blocked = true;
            }

            return !blocked;
        } // CanPlayerSeeEntity

        public static void VisionSystem(Entity player, Entity tilemap, Group guards, Group loot)
        {
            ref var vision = ref player.GetComponent<VisionComponent>();
            ref var transform = ref player.GetComponent<TransformComponent>();
            ref var tilemapComponent = ref tilemap.GetComponent<TilemapComponent>();

            var playerTile = GetEntityTile(player);

            foreach (var guard in guards.Entities)
            {
                ref var guardDrawable = ref guard.GetComponent<DrawableComponent>();
                guardDrawable.IsVisible = CanPlayerSeeEntity(player, guard, vision, tilemapComponent);
            }

            foreach (var lootEntity in loot.Entities)
            {
                ref var lootDrawable = ref lootEntity.GetComponent<DrawableComponent>();
                lootDrawable.IsVisible = CanPlayerSeeEntity(player, lootEntity, vision, tilemapComponent);
            }

            for (var i = 0; i < tilemapComponent.Visible.Length; i++)
                tilemapComponent.Visible[i] = false;

            var pointCount = 200;

            for (var i = 0f; i < 2 * MathF.PI; i += 2 * MathF.PI / pointCount)
            {
                var targetTile = new Vector2I(MathF.Cos(i) * vision.Range + playerTile.X, MathF.Sin(i) * vision.Range + playerTile.Y);

                var lineTiles = Bresenham.GetLinePoints(playerTile, targetTile);
                var blocked = false;

                lineTiles.Sort((t1, t2) => Vector2I.GetDistance(playerTile, t1).CompareTo(Vector2I.GetDistance(playerTile, t2)));

                _visionList.Clear();

                foreach (var tile in lineTiles)
                {
                    // check if tile is out of bounds
                    if (tile.X < 0 || tile.X >= tilemapComponent.Width
                        || tile.Y < 0 || tile.Y >= tilemapComponent.Height)
                    {
                        continue;
                    }

                    if (blocked)
                        continue;

                    _visionList.Add(tile);

                    var index = tile.X + tilemapComponent.Width * tile.Y;
                    if (tilemapComponent.Collisions[index] == CollisionType.Blocked)
                        blocked = true;
                }

                foreach (var tile in _visionList)
                {
                    var index = tile.X + tilemapComponent.Width * tile.Y;
                    tilemapComponent.Expored[index] = true;
                    tilemapComponent.Visible[index] = true;
                }
            }

        } // VisionSystem

        public static void GuardVisionSystem(Group group, Entity tilemap)
        {
            ref var tilemapComponent = ref tilemap.GetComponent<TilemapComponent>();

            for (var i = 0; i < tilemapComponent.GuardsVisible.Length; i++)
                tilemapComponent.GuardsVisible[i] = -1;

            foreach (var entity in group.Entities)
            {
                ref var vision = ref entity.GetComponent<VisionComponent>();
                ref var transform = ref entity.GetComponent<TransformComponent>();

                var entityTile = GetEntityTile(entity);
                var pointCount = 200;

                for (var i = 0f; i < 2 * MathF.PI; i += 2 * MathF.PI / pointCount)
                {
                    var targetTile = new Vector2I(MathF.Cos(i) * vision.Range + entityTile.X, MathF.Sin(i) * vision.Range + entityTile.Y);

                    var lineTiles = Bresenham.GetLinePoints(entityTile, targetTile);
                    var blocked = false;

                    lineTiles.Sort((t1, t2) => Vector2I.GetDistance(entityTile, t1).CompareTo(Vector2I.GetDistance(entityTile, t2)));

                    _visionList.Clear();

                    foreach (var tile in lineTiles)
                    {
                        // check if tile is out of bounds
                        if (tile.X < 0 || tile.X >= tilemapComponent.Width
                            || tile.Y < 0 || tile.Y >= tilemapComponent.Height)
                        {
                            continue;
                        }

                        if (blocked)
                            continue;

                        _visionList.Add(tile);

                        var index = tile.X + tilemapComponent.Width * tile.Y;
                        if (tilemapComponent.Collisions[index] == CollisionType.Blocked)
                            blocked = true;
                    }

                    foreach (var tile in _visionList)
                    {
                        var index = tile.X + tilemapComponent.Width * tile.Y;

                        if (tilemapComponent.Collisions[index] != CollisionType.Blocked)
                            tilemapComponent.GuardsVisible[index] = entity.ID;
                    }
                }
            }

        } // GuardVisionSystem

        public static void LootSystem(Registry registry, Group group, Entity player)
        {
            foreach (var entity in group.Entities)
            {
                ref var lootTransform = ref entity.GetComponent<TransformComponent>();
                ref var playerTransform = ref player.GetComponent<TransformComponent>();

                var lootRect = new Rectangle(lootTransform.Position.ToVector2I(), MapGenerator.TileSize);
                var playerRect = new Rectangle(playerTransform.Position.ToVector2I(), new Vector2I(MapGenerator.TileSize.X, MapGenerator.TileSize.Y * 2));

                if (lootRect.Intersects(playerRect))
                    registry.DestroyEntity(entity);
            }
        } // LootSystem

        public static bool GameOverSystem(Group group, Entity player)
        {
            foreach (var entity in group.Entities)
            {
                ref var guardTransform = ref entity.GetComponent<TransformComponent>();
                ref var playerTransform = ref player.GetComponent<TransformComponent>();

                var guardRect = new Rectangle(guardTransform.Position.ToVector2I(), new Vector2I(MapGenerator.TileSize.X, MapGenerator.TileSize.Y * 2));
                var playerRect = new Rectangle(playerTransform.Position.ToVector2I(), new Vector2I(MapGenerator.TileSize.X, MapGenerator.TileSize.Y * 2));

                if (guardRect.Intersects(playerRect))
                    return true;
            }

            return false;
        } // GameOverSystem

    } // GeneralSystems
}
