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

                Vector2I Pos = transform.TransformedPosition - drawable.Origin;
                Vector2I Size = drawable.AtlasRect.Size;
                var entityRect = new Rectangle(Pos, Size);

                if (entityRect.Intersects(cameraView))
                {
                    _drawList.Add(new DrawItem()
                    {
                        Position = transform.TransformedPosition.ToVector2I(),
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

        public static void MovementSystem(Group group)
        {
            foreach (var entity in group.Entities)
            {
                ref var transform = ref entity.GetComponent<TransformComponent>();
                ref var movement = ref entity.GetComponent<MovementComponent>();

                if (movement.MovementPath.Count > 0)
                {
                    transform.Position += movement.MovementPath.GetLastItem();
                    movement.MovementPath.RemoveLastItem();
                }
            }
        } // MovementSystem

        public static void PhysicsSystem(Group group, GameTimer gameTimer)
        {
            foreach (var entity in group.Entities)
            {
                ref var transform = ref entity.GetComponent<TransformComponent>();
                ref var physics = ref entity.GetComponent<PhysicsComponent>();

                if (physics.Velocity == Vector2.Zero)
                    continue;

                transform.Position += physics.Velocity * gameTimer.DeltaS;
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

        public static void VisionSystem(Entity player, Entity tilemap, Group guards)
        {
            ref var vision = ref player.GetComponent<VisionComponent>();
            ref var transform = ref player.GetComponent<TransformComponent>();
            ref var tilemapComponent = ref tilemap.GetComponent<TilemapComponent>();
            var playerTile = transform.TransformedPosition.ToVector2I() / MapGenerator.TileSize;

            foreach (var guard in guards.Entities)
            {
                ref var guardTransform = ref guard.GetComponent<TransformComponent>();
                ref var guardDrawable = ref guard.GetComponent<DrawableComponent>();

                guardDrawable.IsVisible = false;

                var guardTile = guardTransform.TransformedPosition.ToVector2I() / MapGenerator.TileSize;

                if (Vector2I.GetDistance(playerTile, guardTile) > vision.Range)
                    continue;

                var lineTiles = Bresenham.GetLinePoints(playerTile, guardTile);
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

                guardDrawable.IsVisible = !blocked;
            }

            for (var i = 0; i < tilemapComponent.Visible.Length; i++)
                tilemapComponent.Visible[i] = false;

            var pointCount = 1000;

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

    } // GeneralSystems
}
