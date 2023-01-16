namespace HexaEngine.Physics
{
    using BepuPhysics.Collidables;
    using BepuPhysics.CollisionDetection;
    using System.Numerics;

    /// <summary>
    /// Implements handlers for various collision events.
    /// </summary>
    public interface IContactEventHandler
    {
        /// <summary>
        /// Fires when a contact is added.
        /// </summary>
        /// <typeparam dbgName="TManifold">Type of the contact manifold detected.</typeparam>
        /// <param dbgName="eventSource">Collidable that the event was attached to.</param>
        /// <param dbgName="pair">Collidable pair triggering the event.</param>
        /// <param dbgName="contactManifold">Set of remaining contacts in the collision.</param>
        /// <param dbgName="contactOffset">Offset from the pair's local origin to the new contact.</param>
        /// <param dbgName="contactNormal">Normal of the new contact.</param>
        /// <param dbgName="depth">Depth of the new contact.</param>
        /// <param dbgName="featureId">Feature id of the new contact.</param>
        /// <param dbgName="contactIndex">Index of the new contact in the contact manifold.</param>
        /// <param dbgName="workerIndex">Index of the worker thread that fired this event.</param>
        void OnContactAdded<TManifold>(CollidableReference eventSource, CollidablePair pair, ref TManifold contactManifold,
            in Vector3 contactOffset, in Vector3 contactNormal, float depth, int featureId, int contactIndex, int workerIndex) where TManifold : unmanaged, IContactManifold<TManifold>
        {
        }

        /// <summary>
        /// Fires when a contact is removed.
        /// </summary>
        /// <typeparam dbgName="TManifold">Type of the contact manifold detected.</typeparam>
        /// <param dbgName="eventSource">Collidable that the event was attached to.</param>
        /// <param dbgName="pair">Collidable pair triggering the event.</param>
        /// <param dbgName="contactManifold">Set of remaining contacts in the collision.</param>
        /// <param dbgName="removedFeatureId">Feature id of the contact that was removed and is no longer present in the contact manifold.</param>
        /// <param dbgName="workerIndex">Index of the worker thread that fired this event.</param>
        void OnContactRemoved<TManifold>(CollidableReference eventSource, CollidablePair pair, ref TManifold contactManifold, int removedFeatureId, int workerIndex) where TManifold : unmanaged, IContactManifold<TManifold>
        {
        }

        /// <summary>
        /// Fires the first time a pair is observed to be touching. Touching means that there are contacts with nonnegative depths in the manifold.
        /// </summary>
        /// <typeparam dbgName="TManifold">Type of the contact manifold detected.</typeparam>
        /// <param dbgName="eventSource">Collidable that the event was attached to.</param>
        /// <param dbgName="pair">Collidable pair triggering the event.</param>
        /// <param dbgName="contactManifold">Set of remaining contacts in the collision.</param>
        /// <param dbgName="workerIndex">Index of the worker thread that fired this event.</param>
        void OnStartedTouching<TManifold>(CollidableReference eventSource, CollidablePair pair, ref TManifold contactManifold, int workerIndex) where TManifold : unmanaged, IContactManifold<TManifold>
        {
        }

        /// <summary>
        /// Fires whenever a pair is observed to be touching. Touching means that there are contacts with nonnegative depths in the manifold. Will not fire for sleeping pairs.
        /// </summary>
        /// <typeparam dbgName="TManifold">Type of the contact manifold detected.</typeparam>
        /// <param dbgName="eventSource">Collidable that the event was attached to.</param>
        /// <param dbgName="pair">Collidable pair triggering the event.</param>
        /// <param dbgName="contactManifold">Set of remaining contacts in the collision.</param>
        /// <param dbgName="workerIndex">Index of the worker thread that fired this event.</param>
        void OnTouching<TManifold>(CollidableReference eventSource, CollidablePair pair, ref TManifold contactManifold, int workerIndex) where TManifold : unmanaged, IContactManifold<TManifold>
        {
        }

        /// <summary>
        /// Fires when a pair stops touching. Touching means that there are contacts with nonnegative depths in the manifold.
        /// </summary>
        /// <typeparam dbgName="TManifold">Type of the contact manifold detected.</typeparam>
        /// <param dbgName="eventSource">Collidable that the event was attached to.</param>
        /// <param dbgName="pair">Collidable pair triggering the event.</param>
        /// <param dbgName="contactManifold">Set of remaining contacts in the collision.</param>
        /// <param dbgName="workerIndex">Index of the worker thread that fired this event.</param>
        void OnStoppedTouching<TManifold>(CollidableReference eventSource, CollidablePair pair, ref TManifold contactManifold, int workerIndex) where TManifold : unmanaged, IContactManifold<TManifold>
        {
        }

        /// <summary>
        /// Fires when a pair is observed for the first time.
        /// </summary>
        /// <typeparam dbgName="TManifold">Type of the contact manifold detected.</typeparam>
        /// <param dbgName="eventSource">Collidable that the event was attached to.</param>
        /// <param dbgName="pair">Collidable pair triggering the event.</param>
        /// <param dbgName="contactManifold">Set of remaining contacts in the collision.</param>
        /// <param dbgName="workerIndex">Index of the worker thread that fired this event.</param>
        void OnPairCreated<TManifold>(CollidableReference eventSource, CollidablePair pair, ref TManifold contactManifold, int workerIndex) where TManifold : unmanaged, IContactManifold<TManifold>
        {
        }

        /// <summary>
        /// Fires whenever a pair is updated. Will not fire for sleeping pairs.
        /// </summary>
        /// <typeparam dbgName="TManifold">Type of the contact manifold detected.</typeparam>
        /// <param dbgName="eventSource">Collidable that the event was attached to.</param>
        /// <param dbgName="pair">Collidable pair triggering the event.</param>
        /// <param dbgName="contactManifold">Set of remaining contacts in the collision.</param>
        /// <param dbgName="workerIndex">Index of the worker thread that fired this event.</param>
        void OnPairUpdated<TManifold>(CollidableReference eventSource, CollidablePair pair, ref TManifold contactManifold, int workerIndex) where TManifold : unmanaged, IContactManifold<TManifold>
        {
        }

        /// <summary>
        /// Fires when a pair ends.
        /// </summary>
        /// <typeparam dbgName="TManifold">Type of the contact manifold detected.</typeparam>
        /// <param dbgName="eventSource">Collidable that the event was attached to.</param>
        /// <param dbgName="pair">Collidable pair triggering the event.</param>
        void OnPairEnded(CollidableReference eventSource, CollidablePair pair)
        {
        }
    }
}