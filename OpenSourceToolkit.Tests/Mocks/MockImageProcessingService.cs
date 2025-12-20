using System.Collections.Generic;
using OpenSourceToolkit.Converters;
using OpenSourceToolkit.NET.Services;

namespace OpenSourceToolkit.Tests.Mocks
{
    /// <summary>
    /// Mock image processing service for testing.
    /// Returns predictable test data without actual image processing.
    /// </summary>
    public class MockImageProcessingService : IImageProcessingService
    {
        private readonly byte[] _defaultOutput;

        /// <summary>
        /// Tracks all ProcessImage calls for verification.
        /// </summary>
        public List<(byte[] Input, ImageProcessingOptions Options)> ProcessImageCalls { get; } = new List<(byte[], ImageProcessingOptions)>();

        /// <summary>
        /// Creates a mock that returns the specified bytes for all operations.
        /// </summary>
        public MockImageProcessingService(byte[] defaultOutput = null)
        {
            _defaultOutput = defaultOutput ?? new byte[] { 0x89, 0x50, 0x4E, 0x47 }; // PNG header
        }

        public byte[] ProcessImage(byte[] inputBytes, ImageProcessingOptions options)
        {
            ProcessImageCalls.Add((inputBytes, options));
            return _defaultOutput;
        }

        public byte[] ConvertToPreviewPng(byte[] inputBytes)
        {
            return _defaultOutput;
        }

        public byte[] ConvertToAiPng(byte[] inputBytes)
        {
            return _defaultOutput;
        }

        public byte[] CreateThumbnail(byte[] inputBytes, int maxWidth, int maxHeight)
        {
            return _defaultOutput;
        }
    }
}
