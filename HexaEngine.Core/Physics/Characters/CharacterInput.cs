using BepuPhysics;
using BepuPhysics.Collidables;
using BepuUtilities;
using HexaEngine.Core.Input;
using HexaEngine.Core.Scenes;
using System.Diagnostics;
using System.Numerics;

namespace HexaEngine.Core.Physics.Characters
{
    /// <summary>
    /// Convenience structure that wraps a CharacterController reference and its associated body.
    /// </summary>
    /// <remarks>
    /// <para>This should be treated as an example- nothing here is intended to suggest how you *must* handle characters.
    /// On the contrary, this does some fairly inefficient stuff if you're dealing with hundreds of characters in a predictable way.
    /// It's just a fairly convenient interface for demos usage.</para>
    /// <para>Note that all characters are dynamic and respond to constraints and forces in the simulation.</para>
    /// </remarks>
    public struct CharacterInput
    {
        private BodyHandle bodyHandle;
        private readonly CharacterControllers characters;
        private readonly float speed;
        private Capsule shape;
        private static KeyCode moveForward = KeyCode.W;
        private static KeyCode moveBackward = KeyCode.S;
        private static KeyCode moveRight = KeyCode.D;
        private static KeyCode moveLeft = KeyCode.A;
        private static KeyCode sprint = KeyCode.LShift;
        private static KeyCode jump = KeyCode.Space;

        public BodyHandle BodyHandle => bodyHandle;

        public CharacterInput(CharacterControllers characters, Vector3 initialPosition, Capsule shape, float speculativeMargin, float mass, float maximumHorizontalForce, float maximumVerticalGlueForce, float jumpVelocity, float speed, float maximumSlope = MathF.PI * 0.25f)
        {
            this.characters = characters;
            TypedIndex shapeIndex = characters.Simulation.Shapes.Add(shape);

            //Because characters are dynamic, they require a defined BodyInertia. For the purposes of the demos, we don't want them to rotate or fall over, so the inverse inertia tensor is left at its default value of all zeroes.
            //This is effectively equivalent to giving it an infinite inertia tensor- in other words, no torque will cause it to rotate.
            bodyHandle = characters.Simulation.Bodies.Add(BodyDescription.CreateDynamic(initialPosition, new BodyInertia { InverseMass = 1f / mass }, new CollidableDescription(shapeIndex, speculativeMargin), new BodyActivityDescription(shape.Radius * 0.02f)));
            ref CharacterController character = ref characters.AllocateCharacter(bodyHandle);
            character.LocalUp = new Vector3(0, 1, 0);
            character.CosMaximumSlope = MathF.Cos(maximumSlope);
            character.JumpVelocity = jumpVelocity;
            character.MaximumVerticalForce = maximumVerticalGlueForce;
            character.MaximumHorizontalForce = maximumHorizontalForce;
            character.MinimumSupportDepth = shape.Radius * -0.01f;
            character.MinimumSupportContinuationDepth = -speculativeMargin;
            this.speed = speed;
            this.shape = shape;
        }

        public static KeyCode MoveForward { get => moveForward; set => moveForward = value; }
        public static KeyCode MoveBackward { get => moveBackward; set => moveBackward = value; }
        public static KeyCode MoveRight { get => moveRight; set => moveRight = value; }
        public static KeyCode MoveLeft { get => moveLeft; set => moveLeft = value; }
        public static KeyCode Sprint { get => sprint; set => sprint = value; }
        public static KeyCode Jump { get => jump; set => jump = value; }

        public void UpdateCharacterGoals(Camera camera, float simulationTimestepDuration)
        {
            Vector2 movementDirection = default;
            if (Keyboard.IsDown(moveForward))
                movementDirection = new Vector2(0, 1);
            if (Keyboard.IsDown(moveBackward))
                movementDirection += new Vector2(0, -1);
            if (Keyboard.IsDown(moveLeft))
                movementDirection += new Vector2(1, 0);
            if (Keyboard.IsDown(moveRight))
                movementDirection += new Vector2(-1, 0);
            float movementDirectionLengthSquared = movementDirection.LengthSquared();
            if (movementDirectionLengthSquared > 0)
                movementDirection /= MathF.Sqrt(movementDirectionLengthSquared);

            ref CharacterController character = ref characters.GetCharacterByBodyHandle(bodyHandle);
            character.TryJump = Keyboard.IsDown(jump);
            BodyReference characterBody = new(bodyHandle, characters.Simulation.Bodies);
            float effectiveSpeed = Keyboard.IsDown(sprint) ? speed * 1.75f : speed;
            Vector2 newTargetVelocity = movementDirection * effectiveSpeed;
            Vector3 viewDirection = camera.Transform.Forward;
            //Modifying the character's raw data does not automatically wake the character up, so we do so explicitly if necessary.
            //If you don't explicitly wake the character up, it won't respond to the changed motion goals.
            //(You can also specify a negative deactivation threshold in the BodyActivityDescription to prevent the character from sleeping at all.)
            if (!characterBody.Awake &&
                (character.TryJump && character.Supported ||
                newTargetVelocity != character.TargetVelocity ||
                newTargetVelocity != Vector2.Zero && character.ViewDirection != viewDirection))
                characters.Simulation.Awakener.AwakenBody(character.BodyHandle);
            character.TargetVelocity = newTargetVelocity;
            character.ViewDirection = viewDirection;

            //The character's motion constraints aren't active while the character is in the air, so if we want air control, we'll need to apply it ourselves.
            //(You could also modify the constraints to do this, but the robustness of solved constraints tends to be a lot less important for air control.)
            //There isn't any one 'correct' way to implement air control- it's a nonphysical gameplay thing, and this is just one way to do it.
            //Note that this permits accelerating along a particular direction, and never attempts to slow down the character.
            //This allows some movement quirks common in some game character controllers.
            //Consider what happens if, starting from a standstill, you accelerate fully along X, then along Z- your full velocity magnitude will be sqrt(2) * maximumAirSpeed.
            //Feel free to try alternative implementations. Again, there is no one correct approach.
            if (!character.Supported && movementDirectionLengthSquared > 0)
            {
                QuaternionEx.Transform(character.LocalUp, characterBody.Pose.Orientation, out Vector3 characterUp);
                Vector3 characterRight = Vector3.Cross(character.ViewDirection, characterUp);
                float rightLengthSquared = characterRight.LengthSquared();
                if (rightLengthSquared > 1e-10f)
                {
                    characterRight /= MathF.Sqrt(rightLengthSquared);
                    Vector3 characterForward = Vector3.Cross(characterUp, characterRight);
                    Vector3 worldMovementDirection = characterRight * movementDirection.X + characterForward * movementDirection.Y;
                    float currentVelocity = Vector3.Dot(characterBody.Velocity.Linear, worldMovementDirection);
                    //We'll arbitrarily set air control to be a fraction of supported movement's speed/force.
                    const float airControlForceScale = .2f;
                    const float airControlSpeedScale = .2f;
                    float airAccelerationDt = characterBody.LocalInertia.InverseMass * character.MaximumHorizontalForce * airControlForceScale * simulationTimestepDuration;
                    float maximumAirSpeed = effectiveSpeed * airControlSpeedScale;
                    float targetVelocity = MathF.Min(currentVelocity + airAccelerationDt, maximumAirSpeed);
                    //While we shouldn't allow the character to continue accelerating in the air indefinitely, trying to move in a given direction should never slow us down in that direction.
                    float velocityChangeAlongMovementDirection = MathF.Max(0, targetVelocity - currentVelocity);
                    characterBody.Velocity.Linear += worldMovementDirection * velocityChangeAlongMovementDirection;
                    Debug.Assert(characterBody.Awake, "Velocity changes don't automatically update objects; the character should have already been woken up before applying air control.");
                }
            }
        }

        public void UpdateCameraPosition(Camera camera, float cameraBackwardOffsetScale = 4)
        {
            //We'll override the demo harness's camera control by attaching the camera to the character controller body.
            ref CharacterController character = ref characters.GetCharacterByBodyHandle(bodyHandle);
            BodyReference characterBody = new(bodyHandle, characters.Simulation.Bodies);
            //Use a simple sorta-neck model so that when the camera looks down, the center of the screen sees past the character.
            //Makes mouselocked ray picking easier.
            camera.Transform.Position = characterBody.Pose.Position + new Vector3(0, shape.HalfLength, 0) +
                camera.Transform.Up * (shape.Radius * 1.2f) -
                camera.Transform.Forward * (shape.HalfLength + shape.HalfLength) * cameraBackwardOffsetScale;
        }

        /// <summary>
        /// Removes the character's body from the simulation and the character from the associated characters set.
        /// </summary>
        public void Dispose()
        {
            characters.Simulation.Shapes.Remove(new BodyReference(bodyHandle, characters.Simulation.Bodies).Collidable.Shape);
            characters.Simulation.Bodies.Remove(bodyHandle);
            characters.RemoveCharacterByBodyHandle(bodyHandle);
        }
    }
}