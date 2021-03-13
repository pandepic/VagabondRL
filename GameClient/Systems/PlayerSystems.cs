using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using ElementEngine;
using ElementEngine.ECS;

namespace VagabondRL
{
    public static class PlayerSystems
    {
        public static void ControllerMovementSystem(Entity player)
        {
            ref var physics = ref player.GetComponent<PhysicsComponent>();

            Vector2 MovementVelocity = new Vector2();
            if (InputManager.IsKeyDown(Veldrid.Key.W))
                MovementVelocity += new Vector2(0, -1);
            if (InputManager.IsKeyDown(Veldrid.Key.A))
                MovementVelocity += new Vector2(-1, 0);
            if (InputManager.IsKeyDown(Veldrid.Key.S))
                MovementVelocity += new Vector2(0, +1);
            if (InputManager.IsKeyDown(Veldrid.Key.D))
                MovementVelocity += new Vector2(+1, 0);

            MovementVelocity *= physics.Speed;
            physics.Velocity = MovementVelocity;


        }
    }
}
