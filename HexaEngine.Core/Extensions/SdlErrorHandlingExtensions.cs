namespace HexaEngine.Core.Extensions
{
    using Hexa.NET.SDL2;
    using HexaEngine.Core.Debugging;
    using System.Runtime.CompilerServices;

    /// <summary>
    /// Utilities for handling SDL Errors.
    /// </summary>
    public static unsafe class SdlErrorHandlingExtensions
    {
        private static Exception? GetErrorAsException()
        {
            byte* ex = SDL.SDLGetError();

            if (ex == null || ex[0] == '\0')
            {
                return null;
            }

            return new Exception(ToStringFromUTF8(ex));
        }

        /// <summary>
        /// Checks the SDL function result and throws an exception if it's equal to 0.
        /// </summary>
        /// <param name="result">The SDL function result.</param>
        /// <returns>The original result value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SdlThrowIf(this int result)
        {
#if DEBUG
            if (result == 0)
            {
                LoggerFactory.GetLogger(nameof(SDL)).ThrowIfNotNull(GetErrorAsException());
            }
            return result;
#else
            return result;
#endif
        }

        /// <summary>
        /// Checks the SDL function result and throws an exception if it's less than 0.
        /// </summary>
        /// <param name="result">The SDL function result.</param>
        /// <returns>The original result value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SdlThrowIfNeg(this int result)
        {
#if DEBUG
            if (result < 0)
            {
                LoggerFactory.GetLogger(nameof(SDL)).ThrowIfNotNull(GetErrorAsException());
            }
            return result;
#else
            return result;
#endif
        }

        /// <summary>
        /// Checks the SDL function result and throws an exception if it's equal to 0.
        /// </summary>
        /// <param name="result">The SDL function result.</param>
        /// <returns>The original result value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint SdlThrowIf(this uint result)
        {
            if (result == 0)
            {
                LoggerFactory.GetLogger(nameof(SDL)).ThrowIfNotNull(GetErrorAsException());
            }
            return result;
        }

        /// <summary>
        /// Checks for SDL errors and logs or throws an exception if there is an error.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SdlCheckError()
        {
            LoggerFactory.GetLogger(nameof(SDL)).ThrowIfNotNull(GetErrorAsException());
        }

        /// <summary>
        /// Checks for SDL errors related to a void pointer and returns the original pointer if it's not null.
        /// </summary>
        /// <param name="ptr">The void pointer to check.</param>
        /// <returns>The original pointer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* SdlCheckError(void* ptr)
        {
            if (ptr == null)
            {
                LoggerFactory.GetLogger(nameof(SDL)).ThrowIfNotNull(GetErrorAsException());
            }
            return ptr;
        }

        /// <summary>
        /// Checks for SDL errors related to a pointer of unmanaged type and returns the original pointer if it's not null.
        /// </summary>
        /// <typeparam name="T">The type of the pointer.</typeparam>
        /// <param name="ptr">The pointer to check.</param>
        /// <returns>The original pointer.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T* SdlCheckError<T>(T* ptr) where T : unmanaged
        {
#if DEBUG
            if (ptr == null)
            {
                LoggerFactory.GetLogger(nameof(SDL)).ThrowIfNotNull(GetErrorAsException());
            }
            return ptr;
#else
            return ptr;
#endif
        }
    }
}