using System.Numerics;
using OpenTabletDriver.UX.Controls.Output;
using Xunit;

namespace OpenTabletDriver.Tests
{
    public class LargestRectTest
    {
        [Theory, MemberData(nameof(testData))]
        public void TestGetLargestRectInRotatedRectRatioLocked(GetLargestRectInRotatedRectData data)
        {
            Assert.Equal(
                data.ResultLocked,
                AbsoluteModeEditor.GetLargestRectInRotatedRectRatioLocked(data.RotatedRectDimensions, data.RotationAngleDegrees, data.AspectRatio)
            );
        }

        [Theory, MemberData(nameof(testData))]
        public void TestGetLargestRectInRotatedRect(GetLargestRectInRotatedRectData data)
        {
            Assert.Equal(
                data.ResultUnlocked,
                AbsoluteModeEditor.GetLargestRectInRotatedRect(data.RotatedRectDimensions, data.RotationAngleDegrees)
            );
        }

        public static TheoryData<GetLargestRectInRotatedRectData> testData =
        [
            new GetLargestRectInRotatedRectData
            {
                RotatedRectDimensions = new Vector2(299, 171),
                RotationAngleDegrees = 15f,
                AspectRatio = 16f / 9f,
                ResultLocked = new Vector2(213.17653f, 119.9118f),
                ResultUnlocked = new Vector2(282.38635f, 101.36703f),
            },
            new GetLargestRectInRotatedRectData
            {
                RotatedRectDimensions = new Vector2(299, 171),
                RotationAngleDegrees = 90f,
                AspectRatio = 16f / 9f,
                ResultLocked = new Vector2(171f, 96.1875f),
                ResultUnlocked = new Vector2(171f, 299f),
            },
            new GetLargestRectInRotatedRectData
            {
                RotatedRectDimensions = new Vector2(299, 171),
                RotationAngleDegrees = 0.1f,
                AspectRatio = 16f / 9f,
                ResultLocked = new Vector2(298.70718f, 168.0228f),
                ResultUnlocked = new Vector2(298.7029f, 170.47893f),
            },
            // ensure runaway infinities do not break
            new GetLargestRectInRotatedRectData
            {
                RotatedRectDimensions = new Vector2(299, 171),
                RotationAngleDegrees = 90f,
                AspectRatio = 0.01f,
                ResultLocked = new Vector2(2.99f, 299f),
                ResultUnlocked = new Vector2(171f, 299f),
            },
            new GetLargestRectInRotatedRectData
            {
                RotatedRectDimensions = new Vector2(1, 1),
                RotationAngleDegrees = 0f,
                AspectRatio = 1f,
                ResultLocked = new Vector2(1f, 1f),
                ResultUnlocked = new Vector2(1f, 1f),
            },
            // NaN return is kinda jank but just ensure it doesn't error
            new GetLargestRectInRotatedRectData
            {
                RotatedRectDimensions = new Vector2(0, 0),
                RotationAngleDegrees = 0f,
                AspectRatio = 0f,
                ResultLocked = new Vector2(float.NaN, float.NaN),
                ResultUnlocked = new Vector2(0f, 0f),
            },
            new GetLargestRectInRotatedRectData
            {
                RotatedRectDimensions = new Vector2(299, 171),
                RotationAngleDegrees = 0f,
                AspectRatio = 1000f,
                ResultLocked = new Vector2(299f, 0.299f),
                ResultUnlocked = new Vector2(299f, 171f),
            },
            new GetLargestRectInRotatedRectData
            {
                RotatedRectDimensions = new Vector2(299, 171),
                RotationAngleDegrees = 99999999f,
                AspectRatio = 9f / 16f,
                ResultLocked = new Vector2(141.0713f, 250.79343f),
                ResultUnlocked = new Vector2(123.95683f, 281.7556f),
            },
            new GetLargestRectInRotatedRectData
            {
                RotatedRectDimensions = new Vector2(100, 200),
                RotationAngleDegrees = 0f,
                AspectRatio = 0.5f,
                ResultLocked = new Vector2(100f, 200f),
                ResultUnlocked = new Vector2(100f, 200f),
            },
            new GetLargestRectInRotatedRectData
            {
                RotatedRectDimensions = new Vector2(100, 200),
                RotationAngleDegrees = 90f,
                AspectRatio = 0.5f,
                ResultLocked = new Vector2(50f, 100f),
                ResultUnlocked = new Vector2(200f, 100f),
            },
            new GetLargestRectInRotatedRectData
            {
                RotatedRectDimensions = new Vector2(100, 200),
                RotationAngleDegrees = 90f,
                AspectRatio = 2f,
                ResultLocked = new Vector2(200f, 100f),
                ResultUnlocked = new Vector2(200f, 100f),
            },
            new GetLargestRectInRotatedRectData
            {
                RotatedRectDimensions = new Vector2(200, 100),
                RotationAngleDegrees = 0f,
                AspectRatio = 2f,
                ResultLocked = new Vector2(200f, 100f),
                ResultUnlocked = new Vector2(200f, 100f),
            },
            new GetLargestRectInRotatedRectData
            {
                RotatedRectDimensions = new Vector2(103.002f, 100.01f),
                RotationAngleDegrees = 45f,
                AspectRatio = 1f,
                ResultLocked = new Vector2(70.7178f, 70.7178f),
                ResultUnlocked = new Vector2(70.71775f, 70.71775f),
            },
        ];

        public record struct GetLargestRectInRotatedRectData(Vector2 RotatedRectDimensions, float RotationAngleDegrees, float AspectRatio, Vector2 ResultLocked, Vector2 ResultUnlocked);
    }
}
