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
    }

    public struct AnimationComponent
    {
        public int StartFrame;
        public int EndFrame;
        public int CurrentFrame;
        public bool IsLooping;
    }

    public struct MovementComponent
    {
        public List<Vector2> MovementPath;
    }

    public struct PhysicsComponent
    {
    }

    public struct ColliderComponent
    {
    }
}
