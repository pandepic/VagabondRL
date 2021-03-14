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
    public enum FacingType
    {
        Up,
        Down,
        Left,
        Right
    }

    public struct LootComponent { }

    public struct FourDirectionComponent
    {
        public FacingType Facing;
        public int CurrentFrame;
        public float CurrentFrameTime;
        public float BaseFrameTime;
    }

    public enum CollisionType
    {
        Open, Blocked,
    }

    public struct TransformComponent
    {
        public Entity Parent;
        public float Rotation;
        public Vector2 Position;

        public Vector2 TransformedPosition
        {
            get
            {
                if (!Parent.IsAlive)
                    return Position;
                else
                {
                    ref var parentTransform = ref Parent.GetComponent<TransformComponent>();
                    var transformMatrix =
                        Matrix3x2.CreateRotation(parentTransform.Rotation) *
                        Matrix3x2.CreateTranslation(parentTransform.TransformedPosition);

                    return Vector2.Transform(Position, transformMatrix);
                }
            }
        }
    }

    public struct DrawableComponent
    {
        public Rectangle AtlasRect;
        public Vector2 Origin;
        public Vector2 Scale;
        public Texture2D Texture;
        public int Layer;
        public bool IsVisible;
    }

    public struct AnimationComponent
    {
        public int StartFrame;
        public int EndFrame;
        public int CurrentFrame;
        public bool IsLooping;
    }

    public struct TimemapLayer
    {
        public int[] Tiles;
    }

    public struct TilemapComponent
    {
        public MansionGridGraph Graph;
        public TimemapLayer[] Layers;
        public CollisionType[] Collisions;
        public bool[] Expored;
        public bool[] Visible;
        public int[] GuardsVisible;
        public int Width, Height;
        public string Name;
        public Vector2I PlayerSpawn;
        public Vector2I[] GuardSpawns;
        public Vector2I[] RoomCenters;
    }

    public struct PhysicsComponent
    {
        public static float DefaultSpeed = 50.0f;
        public float Speed;
        public Vector2 Velocity;
    }

    public struct ColliderComponent
    {
    }

    public struct VisionComponent
    {
        public int Range;
    }
}
