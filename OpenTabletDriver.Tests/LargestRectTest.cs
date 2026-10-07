using System.Numerics;
using OpenTabletDriver.UX.Controls.Output;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class LargestRectTest
    {
        [Theory, MemberData(nameof(testData))]
        public void TestGetLargestRectInRotatedRectRatioLocked(GetLargestRectInRotatedRectRatioLockedData data)
        {
            Assert.Equal(
                data.Result,
                AbsoluteModeEditor.GetLargestRectInRotatedRectRatioLocked(data.RotatedRectDimensions, data.RotationAngleDegrees, data.AspectRatio)
            );
        }

        public static TheoryData<GetLargestRectInRotatedRectRatioLockedData> testData =
        [
            new GetLargestRectInRotatedRectRatioLockedData
            {
                RotatedRectDimensions = new Vector2(299, 171),
                RotationAngleDegrees = 15f,
                AspectRatio = 16f / 9f,
                Result = new Vector2(213.17647f, 119.911766f),
            },
            new GetLargestRectInRotatedRectRatioLockedData
            {
                RotatedRectDimensions = new Vector2(299, 171),
                RotationAngleDegrees = 90f,
                AspectRatio = 16f / 9f,
                Result = new Vector2(170.99995f, 96.18748f),
            },
            new GetLargestRectInRotatedRectRatioLockedData
            {
                RotatedRectDimensions = new Vector2(299, 171),
                RotationAngleDegrees = 0.1f,
                AspectRatio = 16f / 9f,
                Result = new Vector2(298.70718f, 168.0228f),
            },
            // ensure runaway infinities do not break
            new GetLargestRectInRotatedRectRatioLockedData
            {
                RotatedRectDimensions = new Vector2(299, 171),
                RotationAngleDegrees = 90f,
                AspectRatio = 0.01f,
                Result = new Vector2(2.99f, 299f),
            },
            new GetLargestRectInRotatedRectRatioLockedData
            {
                RotatedRectDimensions = new Vector2(1, 1),
                RotationAngleDegrees = 0f,
                AspectRatio = 1f,
                Result = new Vector2(0.9999998f, 0.9999998f),
            },
            // NaN return is kinda jank but just ensure it doesn't error
            new GetLargestRectInRotatedRectRatioLockedData
            {
                RotatedRectDimensions = new Vector2(0, 0),
                RotationAngleDegrees = 0f,
                AspectRatio = 0f,
                Result = new Vector2(0f, float.NaN),
            },
            new GetLargestRectInRotatedRectRatioLockedData
            {
                RotatedRectDimensions = new Vector2(299, 171),
                RotationAngleDegrees = 0f,
                AspectRatio = 1000f,
                Result = new Vector2(298.99997f, 0.29900026f),
            },
            new GetLargestRectInRotatedRectRatioLockedData
            {
                RotatedRectDimensions = new Vector2(299, 171),
                RotationAngleDegrees = 99999999f,
                AspectRatio = 9f / 16f,
                Result = new Vector2(141.07127f, 250.79338f),
            },
            new GetLargestRectInRotatedRectRatioLockedData
            {
                RotatedRectDimensions = new Vector2(100, 200),
                RotationAngleDegrees = 0f,
                AspectRatio = 0.5f,
                Result = new Vector2(99.99996f, 199.99992f),
            },
            new GetLargestRectInRotatedRectRatioLockedData
            {
                RotatedRectDimensions = new Vector2(100, 200),
                RotationAngleDegrees = 90f,
                AspectRatio = 0.5f,
                Result = new Vector2(50f, 100f),
            },
            new GetLargestRectInRotatedRectRatioLockedData
            {
                RotatedRectDimensions = new Vector2(100, 200),
                RotationAngleDegrees = 90f,
                AspectRatio = 2f,
                Result = new Vector2(199.99998f, 99.99999f),
            },
            new GetLargestRectInRotatedRectRatioLockedData
            {
                RotatedRectDimensions = new Vector2(200, 100),
                RotationAngleDegrees = 0f,
                AspectRatio = 2f,
                Result = new Vector2(199.99998f, 99.99999f),
            },
            new GetLargestRectInRotatedRectRatioLockedData
            {
                RotatedRectDimensions = new Vector2(103.002f, 100.01f),
                RotationAngleDegrees = 45f,
                AspectRatio = 1f,
                Result = new Vector2(70.71775f, 70.71775f),
            },
        ];

        public record struct GetLargestRectInRotatedRectRatioLockedData(Vector2 RotatedRectDimensions, float RotationAngleDegrees, float AspectRatio, Vector2 Result);
    }
}
