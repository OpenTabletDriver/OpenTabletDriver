using System.Linq;

namespace OpenTabletDriver.Plugin.Tablet
{
    public class DetectionRange
    {
        public DetectionRange()
        {
        }

        public DetectionRange(uint? start, uint? end) : this()
        {
            Start = start;
            End = end;
        }

        public DetectionRange(uint? start, bool startInclusive, uint? end, bool endInclusive) : this(start, end)
        {
            StartInclusive = startInclusive;
            EndInclusive = endInclusive;
        }

        public uint? Start { set; get; }
        public bool StartInclusive { set; get; } = false;

        public uint? End { set; get; }
        public bool EndInclusive {set; get; } = false;

        public const char LeftInclusiveOperator = '[';
        public const char LeftExclusiveOperator = '(';
        public const char RightInclusiveOperator = ']';
        public const char RightExclusiveOperator = ')';

        public bool IsInRange(float value) =>
            (Start.HasValue ? (StartInclusive ? value >= Start : value > Start) : true) & 
            (End.HasValue ? (EndInclusive ? value <= End : value < End) : true);

        public override string ToString()
        {
            return $"{(StartInclusive ? "[" : "(")}{(Start?.ToString() ?? "null")}..{(End?.ToString() ?? "null")}{(EndInclusive ? "]" : ")")}";
        }

        public static DetectionRange Parse(string str)
        {
            var tokens = str.Split("..", 2);
            if (tokens.Length == 2 && tokens.All(t => t.Length > 0))
            {
                string left = tokens[0][1..^0];
                char leftOp = tokens[0][0];
                
                string right = tokens[1][0..^1];
                char rightOp = tokens[1][^1];

                uint? start = left == "null" ? null : (uint.TryParse(left, out var startValue) ? (uint?)startValue : null);
                uint? end = right == "null" ? null : (uint.TryParse(right, out var endValue) ? (uint?)endValue : null);

                bool startInclusive = (leftOp == LeftInclusiveOperator) && (leftOp != LeftExclusiveOperator);
                bool endInclusive = (rightOp == RightInclusiveOperator) && (leftOp != RightExclusiveOperator);

                return new DetectionRange(start, startInclusive, end, endInclusive);
            }
            else if (tokens.Length == 1)
            {
                uint? start = str == "null" ? null : (uint.TryParse(str, out var startValue) ? (uint?)startValue : null);
                return new DetectionRange(start, false, null, false);
            }
            return new DetectionRange();
        }
    }
}