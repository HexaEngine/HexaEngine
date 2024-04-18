namespace HexaEngine.Mathematics
{
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Text.Json.Serialization;

    /// <summary>
    /// <see cref="Transform"/> is used for hierarchical matrix calculation.
    /// </summary>
    public class Transform : ITransform, IReadOnlyTransform, ICloneable
    {
        /// <summary>
        /// The initial state of the <see cref="Transform"/>.
        /// </summary>
        protected TransformSnapshot initial;

        /// <summary>
        /// The parent of the <see cref="Transform"/>..
        /// </summary>
        protected Transform? parent;

        /// <summary>
        /// The local position of the <see cref="Transform"/>.
        /// </summary>
        protected Vector3 position;

        /// <summary>
        /// The local rotation of the <see cref="Transform"/>.
        /// </summary>
        protected Vector3 rotation;

        /// <summary>
        /// The local scale of the <see cref="Transform"/>.
        /// </summary>
        protected Vector3 scale = Vector3.One;

        /// <summary>
        /// The local orientation of the <see cref="Transform"/>.
        /// </summary>
        protected Quaternion orientation;

        /// <summary>
        /// The global position of the <see cref="Transform"/>.
        /// </summary>
        protected Vector3 globalPosition;

        /// <summary>
        /// The global orientation of the <see cref="Transform"/>.
        /// </summary>
        protected Quaternion globalOrientation;

        /// <summary>
        /// The global scale of the <see cref="Transform"/>.
        /// </summary>
        protected Vector3 globalScale;

        /// <summary>
        /// The global transformation matrix of the <see cref="Transform"/>.
        /// </summary>
        protected Matrix4x4 global;

        /// <summary>
        /// The global inverse transformation matrix of the <see cref="Transform"/>.
        /// </summary>
        protected Matrix4x4 globalInverse;

        /// <summary>
        /// The local transformation matrix of the <see cref="Transform"/>.
        /// </summary>
        protected Matrix4x4 local;

        /// <summary>
        /// The local inverse transformation matrix of the <see cref="Transform"/>.
        /// </summary>
        protected Matrix4x4 localInverse;

        /// <summary>
        /// The view matrix of the <see cref="Transform"/>.
        /// </summary>
        protected Matrix4x4 view;

        /// <summary>
        /// The inverse view matrix of the <see cref="Transform"/>.
        /// </summary>
        protected Matrix4x4 viewInv;

        /// <summary>
        /// The velocity of the <see cref="Transform"/>.
        /// </summary>
        protected Vector3 velocity;

        /// <summary>
        /// The last position of the <see cref="Transform"/>, to determine the velocity.
        /// </summary>
        protected Vector3 oldpos;

        /// <summary>
        /// The flags of the <see cref="Transform"/>.
        /// </summary>
        protected TransformFlags flags = TransformFlags.IsDirty;

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
                if (parent != null)
                {
                    parent.Updated -= ParentUpdated;
                }

                parent = value;

                if (value != null)
                {
                    value.Updated += ParentUpdated;
                }

                OnChanged();
            }
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

                value = MaskPosition(value);

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

                value = MaskRotation(value);

                rotation = value;
                orientation = value.NormalizeEulerAngleDegrees().ToRad().ToQuaternion();
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

                orientation = MaskRotation(value, out var euler);

                orientation = value;
                rotation = euler.ToDeg().NormalizeEulerAngleDegrees();
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

                value = MaskScale(value);

                scale = value;
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
                    position = MaskPosition(value);
                }
                else
                {
                    // Transform because the rotation could modify the position of the child.
                    position = MaskPosition(Vector3.Transform(value, parent.globalInverse));
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

                Vector3 euler;

                if (parent == null)
                {
                    orientation = MaskRotation(value, out euler);
                }
                else
                {
                    // Divide because quaternions are like matrices.
                    orientation = MaskRotation(value / parent.globalOrientation, out euler);
                }

                rotation = euler.ToDeg().NormalizeEulerAngleDegrees();

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
                    scale = MaskScale(value);
                }
                else
                {
                    // divide because scale is a factor.
                    scale = MaskScale(value / parent.globalScale);
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
                (position, orientation) = (MaskPosition(value.Item1), MaskRotation(value.Item2, out var euler));
                velocity = position - oldpos;
                rotation = euler.ToDeg().NormalizeEulerAngleDegrees();
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
                (position, orientation, scale) = (MaskPosition(value.Item1), MaskRotation(value.Item2, out var euler), MaskScale(value.Item3));
                velocity = position - oldpos;
                rotation = euler.ToDeg().NormalizeEulerAngleDegrees();
                OnChanged();
            }
        }

        /// <summary>
        /// The forward vector in global orientation space.
        /// </summary>
        [JsonIgnore]
        public Vector3 Forward
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Vector3.Transform(Vector3.UnitZ, globalOrientation);
        }

        /// <summary>
        /// The backward vector in global orientation space.
        /// </summary>
        [JsonIgnore]
        public Vector3 Backward
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Vector3.Transform(-Vector3.UnitZ, globalOrientation);
        }

        /// <summary>
        /// The left vector in global orientation space.
        /// </summary>
        [JsonIgnore]
        public Vector3 Left
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Vector3.Transform(-Vector3.UnitX, globalOrientation);
        }

        /// <summary>
        /// The right vector in global orientation space.
        /// </summary>
        [JsonIgnore]
        public Vector3 Right
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Vector3.Transform(Vector3.UnitX, globalOrientation);
        }

        /// <summary>
        /// The up vector in global orientation space.
        /// </summary>
        [JsonIgnore]
        public Vector3 Up
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Vector3.Transform(Vector3.UnitY, globalOrientation);
        }

        /// <summary>
        /// The down vector in global orientation space.
        /// </summary>
        [JsonIgnore]
        public Vector3 Down
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Vector3.Transform(-Vector3.UnitY, globalOrientation);
        }

        /// <summary>
        /// The global transformation matrix.
        /// </summary>
        [JsonIgnore]
        public Matrix4x4 Global { get => global; }

        /// <summary>
        /// The inverse global transformation matrix.
        /// </summary>
        [JsonIgnore]
        public Matrix4x4 GlobalInverse => globalInverse;

        /// <summary>
        /// The local transformation matrix.
        /// </summary>
        [JsonIgnore]
        public Matrix4x4 Local { get => local; set => SetMatrix(value); }

        /// <summary>
        /// The local inverse transformation matrix.
        /// </summary>
        [JsonIgnore]
        public Matrix4x4 LocalInverse => localInverse;

        /// <summary>
        /// The view matrix in world space.
        /// </summary>
        [JsonIgnore]
        public Matrix4x4 View => view;

        /// <summary>
        /// The inverse view matrix in world space.
        /// </summary>
        [JsonIgnore]
        public Matrix4x4 ViewInv => viewInv;

        /// <summary>
        /// The velocity of the object only useful for 3d sound.
        /// </summary>
        [JsonIgnore]
        public Vector3 Velocity => velocity;

        /// <summary>
        /// Gets or sets a value indicating whether the transform is dirty.
        /// </summary>
        [JsonIgnore]
        public bool IsDirty
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (flags & TransformFlags.IsDirty) != 0;
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if (value)
                {
                    flags |= TransformFlags.IsDirty;
                }
                else
                {
                    flags &= ~TransformFlags.IsDirty;
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether all position axes are locked.
        /// </summary>
        [JsonIgnore]
        public bool LockPosition
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (flags & TransformFlags.LockPosition) == TransformFlags.LockPosition;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if (value)
                {
                    flags |= TransformFlags.LockPosition;
                }
                else
                {
                    flags &= ~TransformFlags.LockPosition;
                }
                OnFlagsChanged(flags);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether all rotation axes are locked.
        /// </summary>
        [JsonIgnore]
        public bool LockRotation
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (flags & TransformFlags.LockRotation) == TransformFlags.LockRotation;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if (value)
                {
                    flags |= TransformFlags.LockRotation;
                }
                else
                {
                    flags &= ~TransformFlags.LockRotation;
                }
                OnFlagsChanged(flags);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether all scale axes are locked.
        /// </summary>
        [JsonIgnore]
        public bool LockScale
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (flags & TransformFlags.LockScale) == TransformFlags.LockScale;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if (value)
                {
                    flags |= TransformFlags.LockScale;
                }
                else
                {
                    flags &= ~TransformFlags.LockScale;
                }
                OnFlagsChanged(flags);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether all scale axes are uniform.
        /// </summary>
        [JsonIgnore]
        public bool UniformScale
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (flags & TransformFlags.UniformScale) == TransformFlags.UniformScale;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if (value)
                {
                    flags |= TransformFlags.UniformScale;
                }
                else
                {
                    flags &= ~TransformFlags.UniformScale;
                }
                OnFlagsChanged(flags);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the X-axis position is locked.
        /// </summary>
        [JsonIgnore]
        public bool LockPositionX
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (flags & TransformFlags.LockPositionX) != 0;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if (value)
                {
                    flags |= TransformFlags.LockPositionX;
                }
                else
                {
                    flags &= ~TransformFlags.LockPositionX;
                }
                OnFlagsChanged(flags);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Y-axis position is locked.
        /// </summary>
        [JsonIgnore]
        public bool LockPositionY
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (flags & TransformFlags.LockPositionY) != 0; ;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if (value)
                {
                    flags |= TransformFlags.LockPositionY;
                }
                else
                {
                    flags &= ~TransformFlags.LockPositionY;
                }
                OnFlagsChanged(flags);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Z-axis position is locked.
        /// </summary>
        [JsonIgnore]
        public bool LockPositionZ
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (flags & TransformFlags.LockPositionZ) != 0;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if (value)
                {
                    flags |= TransformFlags.LockPositionZ;
                }
                else
                {
                    flags &= ~TransformFlags.LockPositionZ;
                }
                OnFlagsChanged(flags);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the X-axis rotation is locked.
        /// </summary>
        [JsonIgnore]
        public bool LockRotationX
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (flags & TransformFlags.LockRotationX) != 0;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if (value)
                {
                    flags |= TransformFlags.LockRotationX;
                }
                else
                {
                    flags &= ~TransformFlags.LockRotationX;
                }
                OnFlagsChanged(flags);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Y-axis rotation is locked.
        /// </summary>
        [JsonIgnore]
        public bool LockRotationY
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (flags & TransformFlags.LockRotationY) != 0;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if (value)
                {
                    flags |= TransformFlags.LockRotationY;
                }
                else
                {
                    flags &= ~TransformFlags.LockRotationY;
                }
                OnFlagsChanged(flags);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Z-axis rotation is locked.
        /// </summary>
        [JsonIgnore]
        public bool LockRotationZ
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (flags & TransformFlags.LockRotationZ) != 0;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if (value)
                {
                    flags |= TransformFlags.LockRotationZ;
                }
                else
                {
                    flags &= ~TransformFlags.LockRotationZ;
                }
                OnFlagsChanged(flags);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the X-axis scale is locked.
        /// </summary>
        [JsonIgnore]
        public bool LockScaleX
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (flags & TransformFlags.LockScaleX) != 0;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if (value)
                {
                    flags |= TransformFlags.LockScaleX;
                }
                else
                {
                    flags &= ~TransformFlags.LockScaleX;
                }
                OnFlagsChanged(flags);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Y-axis scale is locked.
        /// </summary>
        [JsonIgnore]
        public bool LockScaleY
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (flags & TransformFlags.LockScaleY) != 0;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if (value)
                {
                    flags |= TransformFlags.LockScaleY;
                }
                else
                {
                    flags &= ~TransformFlags.LockScaleY;
                }
                OnFlagsChanged(flags);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the Z-axis scale is locked.
        /// </summary>
        [JsonIgnore]
        public bool LockScaleZ
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                return (flags & TransformFlags.LockScaleZ) != 0;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
            set
            {
                if (value)
                {
                    flags |= TransformFlags.LockScaleZ;
                }
                else
                {
                    flags &= ~TransformFlags.LockScaleZ;
                }
                OnFlagsChanged(flags);
            }
        }

        /// <summary>
        /// Gets or sets the flags of the transform.
        /// </summary>
        public TransformFlags Flags
        {
            get => flags;
            set
            {
                if (flags == value)
                {
                    return;
                }

                flags = value;
            }
        }

        /// <summary>
        /// Notifies that the transform has changed. Used internally for parent child updates.
        /// </summary>
        public event TransformUpdatedEventHandler? Updated;

        /// <summary>
        /// Notifies that any property has changed.
        /// </summary>
        public event TransformChangedEventHandler? Changed;

        /// <summary>
        /// Notifies that a flag has been changed.
        /// </summary>
        public event TransformFlagsChangedEventHandler? FlagsChanged;

        /// <summary>
        /// Invokes the <see cref="Updated"/> event.
        /// </summary>
        protected virtual void OnUpdated()
        {
            Updated?.Invoke(this);
        }

        /// <summary>
        /// Invokes the <see cref="Changed"/> event.
        /// </summary>
        protected void OnChanged()
        {
            IsDirty = true;
            Changed?.Invoke(this);
        }

        /// <summary>
        /// Invokes the <see cref="FlagsChanged"/> event.
        /// </summary>
        protected void OnFlagsChanged()
        {
            OnFlagsChanged(flags);
        }

        /// <summary>
        /// Invokes the <see cref="FlagsChanged"/> event.
        /// </summary>
        /// <param name="flags">The new flags.</param>
        protected void OnFlagsChanged(TransformFlags flags)
        {
            FlagsChanged?.Invoke(this, flags);
        }

        private void ParentUpdated(Transform sender)
        {
            IsDirty = true;
            Recalculate();
        }

        /// <summary>
        /// Recalculates all values of the <see cref="Transform"/>.
        /// </summary>
        public virtual bool Recalculate()
        {
            if (!IsDirty)
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
            view = MathUtil.LookAtLH(globalPosition, Forward + globalPosition, Up);
            Matrix4x4.Invert(view, out viewInv);
            OnUpdated();
            IsDirty = false;
            return true;
        }

        /// <summary>
        /// Reverse calculates the local params from a local transform matrix.
        /// </summary>
        /// <param name="matrix">Local space transform matrix</param>
        private void SetMatrix(Matrix4x4 matrix)
        {
            Matrix4x4.Decompose(matrix, out var scale, out var orientation, out var position);
            PositionRotationScale = (position, orientation, scale);
            OnChanged();
        }

        /// <summary>
        /// Reverse calculates the local params from a local transform matrix, overwriting the current values. (Bypasses locked axis checks)
        /// </summary>
        /// <param name="matrix">Local space transform matrix</param>
        public void SetMatrixOverwrite(Matrix4x4 matrix)
        {
            local = matrix;
            Matrix4x4.Invert(local, out localInverse);
            Matrix4x4.Decompose(local, out scale, out orientation, out position);
            rotation = orientation.ToYawPitchRoll().ToDeg();
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
            global = initial.Global;
            globalInverse = initial.GlobalInverse;
            oldpos = initial.OldPos;
            orientation = initial.Orientation;
            parent = initial.Parent;
            position = initial.Position;
            rotation = initial.Rotation;
            scale = initial.Scale;
            velocity = initial.Velocity;
            view = initial.View;
            viewInv = initial.ViewInv;
            Flags = initial.Flags;

            OnChanged();
        }

        /// <summary>
        /// Creates a snapshot of the transformation data and returns it as a new instance of the <see cref="TransformSnapshot"/> struct.
        /// </summary>
        /// <returns>A snapshot of the transformation data as a <see cref="TransformSnapshot"/> instance.</returns>
        public TransformSnapshot CreateSnapshot()
        {
            return new TransformSnapshot()
            {
                Global = global,
                GlobalInverse = globalInverse,
                OldPos = oldpos,
                Orientation = orientation,
                Parent = parent,
                Position = position,
                Rotation = rotation,
                Scale = scale,
                Velocity = velocity,
                View = view,
                ViewInv = viewInv,
                Flags = flags
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
                global = global,
                globalInverse = globalInverse,
                oldpos = oldpos,
                orientation = orientation,
                parent = parent,
                position = position,
                rotation = rotation,
                scale = scale,
                velocity = velocity,
                view = view,
                viewInv = viewInv,
                flags = flags
            };
        }

        /// <summary>
        /// Copies the transformation data from this <see cref="Transform"/> to another <see cref="Transform"/> instance.
        /// </summary>
        /// <param name="other">The <see cref="Transform"/> instance to which the data will be copied.</param>
        public virtual void CopyTo(Transform other)
        {
            other.global = global;
            other.globalInverse = globalInverse;
            other.oldpos = oldpos;
            other.orientation = orientation;
            other.parent = parent;
            other.position = position;
            other.rotation = rotation;
            other.scale = scale;
            other.velocity = velocity;
            other.view = view;
            other.viewInv = viewInv;
            other.flags = flags;
        }

        /// <summary>
        /// Implicitly converts a <see cref="Transform"/> to a <see cref="Matrix4x4"/>, returning the global transformation matrix.
        /// </summary>
        /// <param name="transform">The <see cref="Transform"/> instance to convert.</param>
        /// <returns>The global transformation matrix.</returns>
        public static implicit operator Matrix4x4(Transform transform) => transform.global;

        /// <summary>
        /// Calculates the relative transformation matrix of this <see cref="Transform"/> object
        /// relative to another <see cref="Transform"/> object.
        /// </summary>
        /// <param name="other">The other <see cref="Transform"/> object to which the relative transformation is calculated.</param>
        /// <returns>The relative transformation matrix of this <see cref="Transform"/> object relative to the specified <paramref name="other"/> <see cref="Transform"/> object.</returns>
        public Matrix4x4 GetRelativeTo(Transform other)
        {
            return global * other.globalInverse;
        }

        /// <summary>
        /// Masks the position based on locked axes.
        /// </summary>
        /// <param name="position">The current position.</param>
        /// <returns>The modified position.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public Vector3 MaskPosition(Vector3 position)
        {
            position.X = (flags & TransformFlags.LockPositionX) != 0 ? this.position.X : position.X;
            position.Y = (flags & TransformFlags.LockPositionY) != 0 ? this.position.Y : position.Y;
            position.Z = (flags & TransformFlags.LockPositionZ) != 0 ? this.position.Z : position.Z;
            return position;
        }

        /// <summary>
        /// Masks the rotation based on locked axes.
        /// </summary>
        /// <param name="rotation">The current rotation.</param>
        /// <returns>The modified rotation.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public Vector3 MaskRotation(Vector3 rotation)
        {
            rotation.X = (flags & TransformFlags.LockRotationX) != 0 ? this.rotation.X : rotation.X;
            rotation.Y = (flags & TransformFlags.LockRotationY) != 0 ? this.rotation.Y : rotation.Y;
            rotation.Z = (flags & TransformFlags.LockRotationZ) != 0 ? this.rotation.Z : rotation.Z;
            return rotation;
        }

        /// <summary>
        /// Masks the rotation of a quaternion based on locked axes.
        /// </summary>
        /// <param name="rotation">The current rotation as a quaternion.</param>
        /// <param name="euler">The Euler angle representation of the rotation.</param>
        /// <returns>The modified rotation as a quaternion.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public Quaternion MaskRotation(Quaternion rotation, out Vector3 euler)
        {
            euler = rotation.ToYawPitchRoll();
            euler = MaskRotation(euler);
            rotation = euler.ToQuaternion();
            return rotation;
        }

        /// <summary>
        /// Masks the scale based on locked axes.
        /// </summary>
        /// <param name="scale">The current scale.</param>
        /// <returns>The modified scale.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public Vector3 MaskScale(Vector3 scale)
        {
            scale.X = (flags & TransformFlags.LockScaleX) != 0 ? this.scale.X : scale.X;
            scale.Y = (flags & TransformFlags.LockScaleY) != 0 ? this.scale.Y : scale.Y;
            scale.Z = (flags & TransformFlags.LockScaleZ) != 0 ? this.scale.Z : scale.Z;
            if ((flags & TransformFlags.UniformScale) != 0)
            {
                scale = new(scale.X != this.scale.X ? scale.X : scale.Y != this.scale.Y ? scale.Y : scale.Z);
            }
            return scale;
        }

        /// <summary>
        /// Sets the local position, overwriting the current value. (Bypasses locked axis checks)
        /// </summary>
        /// <param name="value">The new position.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void SetPositionOverwrite(Vector3 value)
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

        /// <summary>
        /// Sets the local rotation, overwriting the current value. (Bypasses locked axis checks)
        /// </summary>
        /// <param name="value">The new rotation in Euler angles.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void SetRotationOverwrite(Vector3 value)
        {
            if (rotation == value)
            {
                return;
            }

            rotation = value;
            orientation = value.NormalizeEulerAngleDegrees().ToRad().ToQuaternion();
            OnChanged();
        }

        /// <summary>
        /// Sets the local orientation, overwriting the current value. (Bypasses locked axis checks)
        /// </summary>
        /// <param name="value">The new orientation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void SetOrientationOverwrite(Quaternion value)
        {
            if (orientation == value)
            {
                return;
            }

            orientation = value;
            rotation = value.ToYawPitchRoll().ToDeg().NormalizeEulerAngleDegrees();
            OnChanged();
        }

        /// <summary>
        /// Sets the local scale, overwriting the current value. (Bypasses locked axis checks)
        /// </summary>
        /// <param name="value">The new scale.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void SetScaleOverwrite(Vector3 value)
        {
            if (scale == value)
            {
                return;
            }

            scale = value;
            OnChanged();
        }

        /// <summary>
        /// Sets the global (world space) position, overwriting the current value. (Bypasses locked axis checks)
        /// </summary>
        /// <param name="value">The new global position.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void SetGlobalPositionOverwrite(Vector3 value)
        {
            if (position == value)
            {
                return;
            }

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

        /// <summary>
        /// Sets the global (world space) orientation, overwriting the current value. (Bypasses locked axis checks)
        /// </summary>
        /// <param name="value">The new global orientation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void SetGlobalOrientationOverwrite(Quaternion value)
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

            rotation = value.ToYawPitchRoll().ToDeg().NormalizeEulerAngleDegrees();

            OnChanged();
        }

        /// <summary>
        /// Sets the global (world space) scale, overwriting the current value. (Bypasses locked axis checks)
        /// </summary>
        /// <param name="value">The new global scale.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void SetGlobalScaleOverwrite(Vector3 value)
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

        /// <summary>
        /// Sets the local position and orientation, overwriting the current values. (Bypasses locked axis checks)
        /// </summary>
        /// <param name="value">The new local position and orientation.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void SetPositionRotationOverwrite((Vector3 position, Quaternion rotation) value)
        {
            if ((position, orientation) == value)
            {
                return;
            }

            oldpos = position;
            (position, orientation) = (value.position, value.rotation);
            velocity = position - oldpos;
            rotation = value.rotation.ToYawPitchRoll().ToDeg().NormalizeEulerAngleDegrees();
            OnChanged();
        }

        /// <summary>
        /// Sets the local position, orientation, and scale, overwriting the current values. (Bypasses locked axis checks)
        /// </summary>
        /// <param name="value">The new local position, orientation, and scale.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.AggressiveOptimization)]
        public void SetPositionRotationScaleOverwrite((Vector3 position, Quaternion rotation, Vector3 scale) value)
        {
            if ((position, orientation, scale) == value)
            {
                return;
            }

            oldpos = position;
            (position, orientation, scale) = (value.position, value.rotation, value.scale);
            velocity = position - oldpos;
            rotation = value.rotation.ToYawPitchRoll().ToDeg().NormalizeEulerAngleDegrees();
            OnChanged();
        }
    }
}