namespace HexaEngine.Components.Physics
{
    using HexaEngine.Editor.Attributes;
    using HexaEngine.Mathematics;
    using MagicPhysX;
    using System.Numerics;

    [EditorCategory("Joints", "Physics")]
    [EditorComponent<DistanceJoint>("Distance Joint")]
    public unsafe class DistanceJoint : Joint
    {
        private PxDistanceJoint* joint;
        private bool autoCalculate;
        private Vector3 anchor;
        private Vector3 connectedAnchor;
        private bool springEnabled;
        private bool minDistanceEnabled;
        private bool maxDistanceEnabled;
        private float springStiffness;
        private float springDamping;
        private float maxDistance;
        private float minDistance;
        private float tolerance;

        [EditorProperty("Auto Calculate")]
        public bool AutoCalculate
        {
            get => autoCalculate;
            set
            {
                autoCalculate = value;
            }
        }

        [EditorProperty("Anchor")]
        public Vector3 Anchor
        {
            get => anchor;
            set
            {
                anchor = value;
            }
        }

        [EditorProperty("Connected Anchor")]
        public Vector3 ConnectedAnchor
        {
            get => connectedAnchor;
            set
            {
                connectedAnchor = value;
            }
        }

        [JsonIgnore]
        public float Distance
        {
            get
            {
                if (joint == null || updating)
                {
                    return 0.0f;
                }

                return joint->GetDistance();
            }
        }

        [EditorCategory("Spring")]
        [EditorProperty("Enabled")]
        public bool SpringEnabled
        {
            get => springEnabled;
            set
            {
                springEnabled = value;

                if (joint != null && !updating)
                {
                    joint->SetDistanceJointFlagMut(PxDistanceJointFlag.SpringEnabled, value);
                }
            }
        }

        [EditorCategory("Spring")]
        [EditorProperty("Stiffness")]
        public float SpringStiffness
        {
            get => springStiffness;
            set
            {
                springStiffness = value;
                if (joint != null && !updating)
                {
                    joint->SetStiffnessMut(value);
                }
            }
        }

        [EditorCategory("Spring")]
        [EditorProperty("Damping")]
        public float SpringDamping
        {
            get => springDamping;
            set
            {
                springDamping = value;
                if (joint != null && !updating)
                {
                    joint->SetDampingMut(value);
                }
            }
        }

        [EditorCategory("Limits")]
        [EditorProperty("Max Distance Enabled")]
        public bool MaxDistanceEnabled
        {
            get => maxDistanceEnabled;
            set
            {
                maxDistanceEnabled = value;
                if (joint != null && !updating)
                {
                    joint->SetDistanceJointFlagMut(PxDistanceJointFlag.MaxDistanceEnabled, value);
                }
            }
        }

        [EditorCategory("Limits")]
        [EditorProperty("Min Distance Enabled")]
        public bool MinDistanceEnabled
        {
            get => minDistanceEnabled;
            set
            {
                minDistanceEnabled = value;

                if (joint != null && !updating)
                {
                    joint->SetDistanceJointFlagMut(PxDistanceJointFlag.MinDistanceEnabled, value);
                }
            }
        }

        [EditorCategory("Limits")]
        [EditorProperty("Max Distance")]
        public float MaxDistance
        {
            get => maxDistance;
            set
            {
                maxDistance = value;
                if (joint != null && !updating)
                {
                    joint->SetMaxDistanceMut(value);
                }
            }
        }

        [EditorCategory("Limits")]
        [EditorProperty("Min Distance")]
        public float MinDistance
        {
            get => minDistance;
            set
            {
                minDistance = value;
                if (joint != null && !updating)
                {
                    joint->SetMinDistanceMut(value);
                }
            }
        }

        [EditorCategory("Limits")]
        [EditorProperty("Tolerance")]
        public float Tolerance
        {
            get => tolerance;
            set
            {
                tolerance = value;
                if (joint != null && !updating)
                {
                    joint->SetToleranceMut(value);
                }
            }
        }

        protected override unsafe PxJoint* CreateJoint(PxPhysics* physics, PxScene* scene, RigidBody rigidBody0, RigidBody rigidBody1, Transform transform0, Transform transform1)
        {
            Vector3 connectedAnchor = AutoCalculate ? transform0.GlobalPosition + Anchor - transform1.GlobalPosition : ConnectedAnchor;

            // Convert local transforms to PxTransform objects
            PxTransform local0 = new() { p = anchor, q = Quaternion.Identity };
            PxTransform local1 = new() { p = connectedAnchor, q = Quaternion.Identity };

            joint = physics->PhysPxDistanceJointCreate(rigidBody0.Actor, &local0, rigidBody1.Actor, &local1);

            joint->SetDistanceJointFlagMut(PxDistanceJointFlag.SpringEnabled, springEnabled);
            joint->SetStiffnessMut(springStiffness);
            joint->SetDampingMut(springDamping);
            joint->SetDistanceJointFlagMut(PxDistanceJointFlag.MaxDistanceEnabled, maxDistanceEnabled);
            joint->SetDistanceJointFlagMut(PxDistanceJointFlag.MinDistanceEnabled, minDistanceEnabled);
            joint->SetMaxDistanceMut(maxDistance);
            joint->SetMinDistanceMut(minDistance);
            joint->SetToleranceMut(tolerance);

            return (PxJoint*)joint;
        }
    }
}