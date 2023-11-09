namespace HexaEngine.Core.Graphics
{
    /// <summary>
    /// Specifies types of queries used in graphics and performance measurement.
    /// </summary>
    public enum Query
    {
        /// <summary>
        /// A query used to record events in a graphics or compute command list.
        /// </summary>
        Event,

        /// <summary>
        /// A query used to measure the number of samples that pass depth and stencil testing.
        /// </summary>
        Occlusion,

        /// <summary>
        /// A query used to timestamp an event for performance measurement.
        /// </summary>
        Timestamp,

        /// <summary>
        /// A query used to check for disjoint timestamp queries.
        /// </summary>
        TimestampDisjoint,

        /// <summary>
        /// A query used to record statistics about pipeline operations.
        /// </summary>
        PipelineStatistics,

        /// <summary>
        /// A query used to check the results of occlusion testing for conditional rendering.
        /// </summary>
        OcclusionPredicate,

        /// <summary>
        /// A query used to record statistics for stream output (SO) operations.
        /// </summary>
        SOStatistics,

        /// <summary>
        /// A query used to check the results of occlusion testing for conditional stream output (SO) rendering on stream 0.
        /// </summary>
        SOOverflowPredicate,

        /// <summary>
        /// A query used to record statistics for stream output (SO) operations on stream 0.
        /// </summary>
        SOStatisticsStream0,

        /// <summary>
        /// A query used to check the results of occlusion testing for conditional stream output (SO) rendering on stream 0.
        /// </summary>
        SOOverflowPredicateStream0,

        /// <summary>
        /// A query used to record statistics for stream output (SO) operations on stream 1.
        /// </summary>
        SOStatisticsStream1,

        /// <summary>
        /// A query used to check the results of occlusion testing for conditional stream output (SO) rendering on stream 1.
        /// </summary>
        SOOverflowPredicateStream1,

        /// <summary>
        /// A query used to record statistics for stream output (SO) operations on stream 2.
        /// </summary>
        SOStatisticsStream2,

        /// <summary>
        /// A query used to check the results of occlusion testing for conditional stream output (SO) rendering on stream 2.
        /// </summary>
        SOOverflowPredicateStream2,

        /// <summary>
        /// A query used to record statistics for stream output (SO) operations on stream 3.
        /// </summary>
        SOStatisticsStream3,

        /// <summary>
        /// A query used to check the results of occlusion testing for conditional stream output (SO) rendering on stream 3.
        /// </summary>
        SOOverflowPredicateStream3
    }
}