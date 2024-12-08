﻿#region License

/*
MIT License

Copyright(c) 2017-2018 Mattias Edlund

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

#endregion

using HexaEngine.Core.MeshDecimator.Algorithms;

namespace HexaEngine.Core.MeshDecimator
{
    /// <summary>
    /// The decimation algorithms.
    /// </summary>
    public enum Algorithm
    {
        /// <summary>
        /// The default algorithm.
        /// </summary>
        Default,

        /// <summary>
        /// The fast quadric mesh simplification algorithm.
        /// </summary>
        FastQuadricMesh
    }

    /// <summary>
    /// The mesh decimation API.
    /// </summary>
    public static class MeshDecimation
    {
        /// <summary>
        /// Creates a specific decimation algorithm.
        /// </summary>
        /// <param name="algorithm">The desired algorithm.</param>
        /// <returns>The decimation algorithm.</returns>
        public static DecimationAlgorithm CreateAlgorithm(Algorithm algorithm)
        {
            DecimationAlgorithm alg = algorithm switch
            {
                Algorithm.Default or Algorithm.FastQuadricMesh => new FastQuadricMeshSimplification(),
                _ => throw new ArgumentException("The specified algorithm is not supported.", nameof(algorithm)),
            };
            return alg;
        }

        /// <summary>
        /// Decimates a mesh.
        /// </summary>
        /// <param name="mesh">The mesh to decimate.</param>
        /// <param name="targetTriangleCount">The target triangle count.</param>
        /// <returns>The decimated mesh.</returns>
        public static Mesh DecimateMesh(Mesh mesh, uint targetTriangleCount)
        {
            return DecimateMesh(Algorithm.Default, mesh, targetTriangleCount);
        }

        /// <summary>
        /// Decimates a mesh.
        /// </summary>
        /// <param name="algorithm">The desired algorithm.</param>
        /// <param name="mesh">The mesh to decimate.</param>
        /// <param name="targetTriangleCount">The target triangle count.</param>
        /// <returns>The decimated mesh.</returns>
        public static Mesh DecimateMesh(Algorithm algorithm, Mesh mesh, uint targetTriangleCount)
        {
            ArgumentNullException.ThrowIfNull(mesh);

            DecimationAlgorithm decimationAlgorithm = CreateAlgorithm(algorithm);
            return DecimateMesh(decimationAlgorithm, mesh, targetTriangleCount);
        }

        /// <summary>
        /// Decimates a mesh.
        /// </summary>
        /// <param name="algorithm">The decimation algorithm.</param>
        /// <param name="mesh">The mesh to decimate.</param>
        /// <param name="targetTriangleCount">The target triangle count.</param>
        /// <returns>The decimated mesh.</returns>
        public static Mesh DecimateMesh(DecimationAlgorithm algorithm, Mesh mesh, uint targetTriangleCount)
        {
            if (algorithm == null)
            {
                throw new ArgumentNullException(nameof(algorithm));
            }
            else
            {
                ArgumentNullException.ThrowIfNull(mesh);
            }

            uint currentTriangleCount = mesh.TriangleCount;
            if (targetTriangleCount > currentTriangleCount)
            {
                targetTriangleCount = currentTriangleCount;
            }
            else if (targetTriangleCount < 0)
            {
                targetTriangleCount = 0;
            }

            algorithm.Initialize(mesh);
            algorithm.DecimateMesh(targetTriangleCount);
            return algorithm.ToMesh();
        }

        /// <summary>
        /// Decimates a mesh without losing any quality.
        /// </summary>
        /// <param name="mesh">The mesh to decimate.</param>
        /// <returns>The decimated mesh.</returns>
        public static Mesh DecimateMeshLossless(Mesh mesh)
        {
            return DecimateMeshLossless(Algorithm.Default, mesh);
        }

        /// <summary>
        /// Decimates a mesh without losing any quality.
        /// </summary>
        /// <param name="algorithm">The desired algorithm.</param>
        /// <param name="mesh">The mesh to decimate.</param>
        /// <returns>The decimated mesh.</returns>
        public static Mesh DecimateMeshLossless(Algorithm algorithm, Mesh mesh)
        {
            ArgumentNullException.ThrowIfNull(mesh);

            DecimationAlgorithm decimationAlgorithm = CreateAlgorithm(algorithm);
            return DecimateMeshLossless(decimationAlgorithm, mesh);
        }

        /// <summary>
        /// Decimates a mesh without losing any quality.
        /// </summary>
        /// <param name="algorithm">The decimation algorithm.</param>
        /// <param name="mesh">The mesh to decimate.</param>
        /// <returns>The decimated mesh.</returns>
        public static Mesh DecimateMeshLossless(DecimationAlgorithm algorithm, Mesh mesh)
        {
            if (algorithm == null)
            {
                throw new ArgumentNullException(nameof(algorithm));
            }
            else
            {
                ArgumentNullException.ThrowIfNull(mesh);
            }

            _ = mesh.TriangleCount;
            algorithm.Initialize(mesh);
            algorithm.DecimateMeshLossless();
            return algorithm.ToMesh();
        }
    }
}