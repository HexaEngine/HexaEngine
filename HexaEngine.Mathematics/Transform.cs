namespace HexaEngine.Mathematics
{
    using System;
    using System.Numerics;
    using System.Text.Json.Serialization;

    /// <summary>
    /// <see cref="Transform"/> is used for hierarchical matrix calculation.
    /// </summary>
    public class Transform : ITransform
    {
        /// <summary>
        /// The initial state of the <see cref="Transform"/>.
        /// </summary>
        protected TransformSnapshot initial;

        /// <summary>
        /// The parent of the <see cref="Transform"/>.
        /// </summary>
        protected Transform? parent;

        /// <summary>
        /// The local position of the <see cref="Transform"/>
        /// </summary>
        protected Vector3 position;

        /// <summary>
        /// The local rotation of the <see cref="Transform"/>
        /// </summary>
        protected Vector3 rotation;

        /// <summary>
        /// The local scale of the <see cref="Transform"/>
        /// </summary>
        protected Vector3 scale = Vector3.One;

        /// <summary>
        /// The local orientation of the <see cref="Transform"/>
        /// </summary>
        protected Quaternion orientation;

        /// <summary>
        /// The global position of the <see cref="Transform"/>
        /// </summary>
        protected Vector3 globalPosition;

        /// <summary>
        /// The global orientation of the <see cref="Transform"/>
        /// </summary>
        protected Quaternion globalOrientation;

        /// <summary>
        /// The global scale of the <see cref="Transform"/>
        /// </summary>
        protected Vector3 globalScale;

        /// <summary>
        /// The global forward direction of the <see cref="Transform"/>
        /// </summary>
        protected Vector3 forward;

        /// <summary>
        /// The global backward direction of the <see cref="Transform"/>
        /// </summary>
        protected Vector3 backward;

        /// <summary>
        /// The global left direction of the <see cref="Transform"/>
        /// </summary>
        protected Vector3 left;

        /// <summary>
        /// The global right direction of the <see cref="Transform"/>
        /// </summary>
        protected Vector3 right;

        /// <summary>
        /// The global up direction of the <see cref="Transform"/>
        /// </summary>
        protected Vector3 up;

        /// <summary>
        /// The global down direction of the <see cref="Transform"/>
        /// </summary>
        protected Vector3 down;

        /// <summary>
        /// The global transformation matrix of the <see cref="Transform"/>
        /// </summary>
        protected Matrix4x4 global;

        /// <summary>
        /// The global inverse transformation matrix of the <see cref="Transform"/>
        /// </summary>
        protected Matrix4x4 globalInverse;

        /// <summary>
        /// The local transformation matrix of the <see cref="Transform"/>
        /// </summary>
        protected Matrix4x4 local;

        /// <summary>
        /// The local inverse transformation matrix of the <see cref="Transform"/>
        /// </summary>
        protected Matrix4x4 localInverse;

        /// <summary>
        /// The view matrix of the <see cref="Transform"/>
        /// </summary>
        protected Matrix4x4 view;

        /// <summary>
        /// The inverse view matrix of the <see cref="Transform"/>
        /// </summary>
        protected Matrix4x4 viewInv;

        /// <summary>
        /// The velocity of the <see cref="Transform"/>
        /// </summary>
        protected Vector3 velocity;

        /// <summary>
        /// The last position of the <see cref="Transform"/>, to determine the velocity.
        /// </summary>
        protected Vector3 oldpos;

        /// <summary>
        /// The dirty flag of the <see cref="Transform"/>
        /// </summary>
        protected bool dirty = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="Transform"/> class.
        /// </summary>
        public Transform()
        {
            Recalculate();
            SaveState();
        }

        /// <summary>
        /// Gets or sets the parent of this <see cref="Transform"/>.
        /// </summary>
        /// <value>
        /// The parent of this <see cref="Transform"/>.
        /// </value>
        [JsonIgnore]
        public Transform? Parent
        {
            get => parent;
            set
            {
                parent = value;
                if (parent != null)
                {
                    parent.Updated += ParentUpdated;
                }

                OnChanged();
            }
        }

        private void ParentUpdated(object? sender, EventArgs e)
        {
            dirty = true;
            Recalculate();
        }

        /// <summary>
        /// Gets or sets the local position.
        /// </summary>
        public Vector3 Position
        {
            get => position;
            set
            {
                if (position == value)
                {
                    return;
                }

                oldpos = position;
                position = value;
                velocity = position - oldpos;
                OnChanged();
            }
        }

        /// <summary>
        /// Gets or sets the local rotation.
        /// </summary>
        /// <remarks>The rotation is in space Euler from 0° to 360°(359°)</remarks>
        public Vector3 Rotation
        {
            get => rotation;
            set
            {
                if (rotation == value)
                {
                    return;
                }

                rotation = value;
                orientation = value.NormalizeEulerAngleDegrees().ToRad().GetQuaternion();
                OnChanged();
            }
        }

        /// <summary>
        /// Gets or sets the local scale.
        /// </summary>
        public Vector3 Scale
        {
            get => scale;
            set
            {
                if (scale == value)
                {
                    return;
                }

                scale = value;
                OnChanged();
            }
        }

        /// <summary>
        /// Gets or sets the local orientation.
        /// </summary>
        [JsonIgnore]
        public Quaternion Orientation
        {
            get => orientation;
            set
            {
                if (orientation == value)
                {
                    return;
                }

                orientation = value;
                rotation = value.GetYawPitchRoll().ToDeg().NormalizeEulerAngleDegrees();
                OnChanged();
            }
        }

        /// <summary>
        /// Gets or sets the global (world space) position.
        /// </summary>
        [JsonIgnore]
        public Vector3 GlobalPosition
        {
            get => globalPosition;
            set
            {
                if (globalPosition == value)
                {
                    return;
                }

                if (parent == null)
                {
                    position = value;
                }
                else
                {
                    // Transform because the rotation could modify the position of the child.
                    position = Vector3.Transform(value, parent.globalInverse);
                }

                OnChanged();
            }
        }

        /// <summary>
        /// Gets or sets the global (world space) orientation.
        /// </summary>
        [JsonIgnore]
        public Quaternion GlobalOrientation
        {
            get => globalOrientation;
            set
            {
                if (globalOrientation == value)
                {
                    return;
                }

                if (parent == null)
                {
                    orientation = value;
                }
                else
                {
                    // Divide because quaternions are like matrices.
                    orientation = value / parent.globalOrientation;
                }

                OnChanged();
            }
        }

        /// <summary>
        /// Gets or sets the global (world space) scale.
        /// </summary>
        [JsonIgnore]
        public Vector3 GlobalScale
        {
            get => globalScale;
            set
            {
                if (globalScale == value)
                {
                    return;
                }

                if (parent == null)
                {
                    scale = value;
                }
                else
                {
                    // divide because scale is a factor.
                    scale = value / parent.globalScale;
                }

                OnChanged();
            }
        }

        /// <summary>
        /// Gets or sets the position and orientation.
        /// </summary>
        [JsonIgnore]
        public (Vector3, Quaternion) PositionRotation
        {
            get => (position, orientation);
            set
            {
                if ((position, orientation) == value)
                {
                    return;
                }

                oldpos = position;
                (position, orientation) = value;
                velocity = position - oldpos;
                rotation = orientation.GetYawPitchRoll().ToDeg().NormalizeEulerAngleDegrees();
                OnChanged();
            }
        }

        /// <summary>
        /// Gets or sets the local the position orientation and scale.
        /// </summary>
        [JsonIgnore]
        public (Vector3, Quaternion, Vector3) PositionRotationScale
        {
            get => (position, orientation, scale);
            set
            {
                if ((position, orientation, scale) == value)
                {
                    return;
                }

                oldpos = position;
                (position, orientation, scale) = value;
                velocity = position - oldpos;
                rotation = orientation.GetYawPitchRoll().ToDeg().NormalizeEulerAngleDegrees();
                OnChanged();
            }
        }

        /// <summary>
        /// The forward vector in global orientation space
        /// </summary>
        [JsonIgnore]
        public Vector3 Forward => forward;

        /// <summary>
        /// The backward vector in global orientation space
        /// </summary>
        [JsonIgnore]
        public Vector3 Backward => backward;

        /// <summary>
        /// The left vector in global orientation space
        /// </summary>
        [JsonIgnore]
        public Vector3 Left => left;

        /// <summary>
        /// The right vector in global orientation space
        /// </summary>
        [JsonIgnore]
        public Vector3 Right => right;

        /// <summary>
        /// The up vector in global orientation space
        /// </summary>
        [JsonIgnore]
        public Vector3 Up => up;

        /// <summary>
        /// The down vector in global orientation space
        /// </summary>
        [JsonIgnore]
        public Vector3 Down => down;

        /// <summary>
        /// The global transformation matrix
        /// </summary>
        [JsonIgnore]
        public Matrix4x4 Global { get => global; }

        /// <summary>
        /// The inverse global transformation matrix
        /// </summary>
        [JsonIgnore]
        public Matrix4x4 GlobalInverse => globalInverse;

        /// <summary>
        /// The local transformation matrix
        /// </summary>
        [JsonIgnore]
        public Matrix4x4 Local { get => local; set => SetMatrix(value); }

        /// <summary>
        /// The local inverse transformation matrix
        /// </summary>
        [JsonIgnore]
        public Matrix4x4 LocalInverse => localInverse;

        /// <summary>
        /// The view matrix in world space
        /// </summary>
        [JsonIgnore]
        public Matrix4x4 View => view;

        /// <summary>
        /// The inverse view matrix in world space
        /// </summary>
        [JsonIgnore]
        public Matrix4x4 ViewInv => viewInv;

        /// <summary>
        /// The velocity of the object only useful for 3d sound.
        /// </summary>
        [JsonIgnore]
        public Vector3 Velocity => velocity;

        /// <summary>
        /// Notifies that the transform has changed. Used internally for parent child updates.
        /// </summary>
        public event EventHandler? Updated;

        /// <summary>
        /// Notifies that any property has changed.
        /// </summary>
        public event EventHandler? Changed;

        /// <summary>
        /// Invokes the <see cref="Updated"/> event.
        /// </summary>
        protected virtual void OnUpdated()
        {
            Updated?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Invokes the <see cref="Changed"/> event.
        /// </summary>
        protected void OnChanged()
        {
            dirty = true;
            Changed?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Recalculates all values of the <see cref="Transform"/>.
        /// </summary>
        public virtual bool Recalculate()
        {
            if (!dirty)
            {
                return false;
            }

            // Update the local matrix of this transform
            local = Matrix4x4.CreateScale(scale) * Matrix4x4.CreateFromQuaternion(orientation) * Matrix4x4.CreateTranslation(position);
            Matrix4x4.Invert(local, out localInverse);

            // Update the global matrix of this transform
            if (parent == null)
            {
                global = local;
            }
            else
            {
                global = local * parent.global;
            }

            Matrix4x4.Invert(global, out globalInverse);

            // Update other properties based on the new global matrix
            Matrix4x4.Decompose(global, out globalScale, out globalOrientation, out globalPosition);
            forward = Vector3.Transform(Vector3.UnitZ, globalOrientation);
            backward = Vector3.Transform(-Vector3.UnitZ, globalOrientation);
            right = Vector3.Transform(Vector3.UnitX, globalOrientation);
            left = Vector3.Transform(-Vector3.UnitX, globalOrientation);
            up = Vector3.Transform(Vector3.UnitY, globalOrientation);
            down = Vector3.Transform(-Vector3.UnitY, globalOrientation);

            view = MathUtil.LookAtLH(globalPosition, forward + globalPosition, up);
            Matrix4x4.Invert(view, out viewInv);
            OnUpdated();
            dirty = false;
            return true;
        }

        /// <summary>
        /// Reverse calculates the local params from a local transform matrix.
        /// </summary>
        /// <param name="matrix">Local space transform matrix</param>
        private void SetMatrix(Matrix4x4 matrix)
        {
            local = matrix;
            Matrix4x4.Invert(local, out localInverse);
            Matrix4x4.Decompose(local, out scale, out orientation, out position);
            rotation = orientation.GetYawPitchRoll().ToDeg();
            OnChanged();
        }

        /// <summary>
        /// Stores the current state. Used internally for the editor.
        /// </summary>
        public void SaveState()
        {
            initial = CreateSnapshot();
        }

        /// <summary>
        /// Stores the current state. Used internally for the editor.
        /// </summary>
        public void RestoreState()
        {
            backward = initial.Backward;
            down = initial.Down;
            forward = initial.Forward;
            left = initial.Left;
            global = initial.Global;
            globalInverse = initial.GlobalInverse;
            oldpos = initial.OldPos;
            orientation = initial.Orientation;
            parent = initial.Parent;
            position = initial.Position;
            right = initial.Right;
            rotation = initial.Rotation;
            scale = initial.Scale;
            up = initial.Up;
            velocity = initial.Velocity;
            view = initial.View;
            viewInv = initial.ViewInv;
        }

        /// <summary>
        /// Creates a snapshot of the transformation data and returns it as a new instance of the <see cref="TransformSnapshot"/> struct.
        /// </summary>
        /// <returns>A snapshot of the transformation data as a <see cref="TransformSnapshot"/> instance.</returns>
        public TransformSnapshot CreateSnapshot()
        {
            return new TransformSnapshot()
            {
                Backward = backward,
                Down = down,
                Forward = forward,
                Left = left,
                Global = global,
                GlobalInverse = globalInverse,
                OldPos = oldpos,
                Orientation = orientation,
                Parent = parent,
                Position = position,
                Right = right,
                Rotation = rotation,
                Scale = scale,
                Up = up,
                Velocity = velocity,
                View = view,
                ViewInv = viewInv
            };
        }

        /// <summary>
        /// Clones this <see cref="Transform"/> instance.
        /// </summary>
        /// <returns>Returns a new instance of <see cref="Transform"/> with the values of this <see cref="Transform"/></returns>
        public virtual object Clone()
        {
            return new Transform()
            {
                backward = backward,
                down = down,
                forward = forward,
                left = left,
                global = global,
                globalInverse = globalInverse,
                oldpos = oldpos,
                orientation = orientation,
                parent = parent,
                position = position,
                right = right,
                rotation = rotation,
                scale = scale,
                up = up,
                velocity = velocity,
                view = view,
                viewInv = viewInv
            };
        }

        /// <summary>
        /// Copies the transformation data from this <see cref="Transform"/> to another <see cref="Transform"/> instance.
        /// </summary>
        /// <param name="other">The <see cref="Transform"/> instance to which the data will be copied.</param>
        public virtual void CopyTo(Transform other)
        {
            other.backward = backward;
            other.down = down;
            other.forward = forward;
            other.left = left;
            other.global = global;
            other.globalInverse = globalInverse;
            other.oldpos = oldpos;
            other.orientation = orientation;
            other.parent = parent;
            other.position = position;
            other.right = right;
            other.rotation = rotation;
            other.scale = scale;
            other.up = up;
            other.velocity = velocity;
            other.view = view;
            other.viewInv = viewInv;
        }

        /// <summary>
        /// Implicitly converts a <see cref="Transform"/> to a <see cref="Matrix4x4"/>, returning the global transformation matrix.
        /// </summary>
        /// <param name="transform">The <see cref="Transform"/> instance to convert.</param>
        /// <returns>The global transformation matrix.</returns>
        public static implicit operator Matrix4x4(Transform transform) => transform.global;
    }
}