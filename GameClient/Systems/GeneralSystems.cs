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

                var entityRect = new Rectangle(transform.TransformedPosition - drawable.Origin, drawable.AtlasRect.SizeF);

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

    } // GeneralSystems
}
