namespace HexaEngine.Core.Unsafes
{
    using System.Collections.Generic;

    /// <summary>
    /// A class for mapping native objects to managed objects.
    /// </summary>
    public unsafe class NativeToManagedMapper
    {
        private readonly Dictionary<nint, object> mapping = new();

        /// <summary>
        /// Clears all mappings.
        /// </summary>
        public void Clear()
        {
            mapping.Clear();
        }

        /// <summary>
        /// Adds a mapping between a native object and a managed object.
        /// </summary>
        /// <param name="nativeObject">The native object pointer.</param>
        /// <param name="managedObject">The managed object.</param>
        public void AddMapping(void* nativeObject, object managedObject)
        {
            AddMapping((nint)nativeObject, managedObject);
        }

        /// <summary>
        /// Adds a mapping between a native object and a managed object.
        /// </summary>
        /// <param name="nativeObject">The native object pointer.</param>
        /// <param name="managedObject">The managed object.</param>
        public void AddMapping(nint nativeObject, object managedObject)
        {
            if (mapping.ContainsKey(nativeObject))
            {
                mapping[nativeObject] = managedObject;
                return;
            }
            mapping.Add(nativeObject, managedObject);
        }

        /// <summary>
        /// Gets the managed object associated with a native object.
        /// </summary>
        /// <param name="nativeObject">The native object pointer.</param>
        /// <returns>The associated managed object, or <c>null</c> if not found.</returns>
        public object? GetManagedObject(void* nativeObject)
        {
            return GetManagedObject((nint)nativeObject);
        }

        /// <summary>
        /// Gets the managed object associated with a native object.
        /// </summary>
        /// <param name="nativeObject">The native object pointer.</param>
        /// <returns>The associated managed object, or <c>null</c> if not found.</returns>
        public object? GetManagedObject(nint nativeObject)
        {
            if (mapping.TryGetValue(nativeObject, out object? managedObject))
            {
                return managedObject;
            }
            return null;
        }

        /// <summary>
        /// Gets the managed object of type <typeparamref name="T"/> associated with a native object.
        /// </summary>
        /// <typeparam name="T">The type of the managed object.</typeparam>
        /// <param name="nativeObject">The native object pointer.</param>
        /// <returns>The associated managed object of type <typeparamref name="T"/>, or <c>null</c> if not found or not of the specified type.</returns>
        public T? GetManagedObject<T>(void* nativeObject) where T : class
        {
            return GetManagedObject<T>((nint)nativeObject);
        }

        /// <summary>
        /// Gets the managed object of type <typeparamref name="T"/> associated with a native object.
        /// </summary>
        /// <typeparam name="T">The type of the managed object.</typeparam>
        /// <param name="nativeObject">The native object pointer.</param>
        /// <returns>The associated managed object of type <typeparamref name="T"/>, or <c>null</c> if not found or not of the specified type.</returns>
        public T? GetManagedObject<T>(nint nativeObject) where T : class
        {
            if (mapping.TryGetValue(nativeObject, out object? managedObject) && managedObject is T t)
            {
                return t;
            }
            return null;
        }

        /// <summary>
        /// Removes the mapping for a native object.
        /// </summary>
        /// <param name="nativeObject">The native object pointer.</param>
        public void RemoveMapping(void* nativeObject)
        {
            RemoveMapping((nint)nativeObject);
        }

        /// <summary>
        /// Removes the mapping for a native object.
        /// </summary>
        /// <param name="nativeObject">The native object pointer.</param>
        public void RemoveMapping(nint nativeObject)
        {
            mapping.Remove(nativeObject);
        }
    }

    /// <summary>
    /// A class for mapping native objects to managed objects.
    /// </summary>
    public unsafe class NativeToManagedMapper<T> where T : class
    {
        private readonly Dictionary<nint, T> mapping = [];

        /// <summary>
        /// Clears all mappings.
        /// </summary>
        public void Clear()
        {
            mapping.Clear();
        }

        /// <summary>
        /// Adds a mapping between a native object and a managed object.
        /// </summary>
        /// <param name="nativeObject">The native object pointer.</param>
        /// <param name="managedObject">The managed object.</param>
        public void AddMapping(void* nativeObject, T managedObject)
        {
            AddMapping((nint)nativeObject, managedObject);
        }

        /// <summary>
        /// Adds a mapping between a native object and a managed object.
        /// </summary>
        /// <param name="nativeObject">The native object pointer.</param>
        /// <param name="managedObject">The managed object.</param>
        public void AddMapping(nint nativeObject, T managedObject)
        {
            if (mapping.ContainsKey(nativeObject))
            {
                mapping[nativeObject] = managedObject;
                return;
            }
            mapping.Add(nativeObject, managedObject);
        }

        /// <summary>
        /// Gets the managed object associated with a native object.
        /// </summary>
        /// <param name="nativeObject">The native object pointer.</param>
        /// <returns>The associated managed object, or <c>null</c> if not found.</returns>
        public object? GetManagedObject(void* nativeObject)
        {
            return GetManagedObject((nint)nativeObject);
        }

        /// <summary>
        /// Gets the managed object associated with a native object.
        /// </summary>
        /// <param name="nativeObject">The native object pointer.</param>
        /// <returns>The associated managed object, or <c>null</c> if not found.</returns>
        public T? GetManagedObject(nint nativeObject)
        {
            if (mapping.TryGetValue(nativeObject, out T? managedObject))
            {
                return managedObject;
            }
            return null;
        }

        /// <summary>
        /// Removes the mapping for a native object.
        /// </summary>
        /// <param name="nativeObject">The native object pointer.</param>
        public void RemoveMapping(void* nativeObject)
        {
            RemoveMapping((nint)nativeObject);
        }

        /// <summary>
        /// Removes the mapping for a native object.
        /// </summary>
        /// <param name="nativeObject">The native object pointer.</param>
        public void RemoveMapping(nint nativeObject)
        {
            mapping.Remove(nativeObject);
        }
    }
}