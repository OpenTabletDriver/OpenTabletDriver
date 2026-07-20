using System;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace OpenTabletDriver.Plugin.Tablet
{
    /// <summary>
    /// Device specifications for a wheel.
    /// </summary>
    public class WheelSpecifications
    {
        private uint? _absoluteWheelMax;
        private uint? _relativeWheelSteps;

        /// <summary>
        /// The number of device steps per 360 degrees of wheel rotation. Used to normalize step size across tablets.
        /// <para/>
        /// This value should be estimated by doing X full rotations of the wheel, and taking the average and rounding to the closest whole number
        /// </summary>
        /// <remarks>
        /// Can only set value to non-null if <see cref="AbsoluteWheelMax"/> isn't set
        /// </remarks>
        public uint? RelativeWheelSteps
        {
            get => _relativeWheelSteps;
            set
            {
                if (AbsoluteWheelMax.HasValue && value != null)
                    throw new InvalidOperationException($"Can't set {nameof(RelativeWheelSteps)} when {nameof(AbsoluteWheelMax)} is set");
                _relativeWheelSteps = value;
            }
        }

        /// <summary>
        /// The upper bounds of the wheel value. Used in delta calculations, especially when reaching the end of the value.
        /// </summary>
        /// <remarks>
        /// Assumes range is 0 to value, inclusive.
        /// <para/>
        /// Can only set value to non-null if <see cref="RelativeWheelSteps"/> isn't set
        /// </remarks>
        public uint? AbsoluteWheelMax
        {
            get => _absoluteWheelMax;
            set
            {
                if (RelativeWheelSteps.HasValue && value != null)
                    throw new InvalidOperationException($"Can't set {nameof(AbsoluteWheelMax)} when {nameof(RelativeWheelSteps)} is set");
                _absoluteWheelMax = value;
            }
        }

        /// <summary>
        /// The physical angle on the absolute wheel's unit circle, corresponding to a reading of zero
        /// <para/>
        /// Relative wheels MUST leave this unset
        /// </summary>
        // TODO: test range (maybe generically/recursively test values in known classes?)
        [Range(0, 360)]
        public float? AngleOfZeroReading { get; set; }

        /// <summary>
        /// Amount of buttons present on the wheel (usually between 0 and 2, inclusive)
        /// </summary>
        [Required(ErrorMessage = $"{nameof(ButtonCount)} must be defined")]
        public uint ButtonCount { set; get; }

        /// <summary>
        /// Get amount of unique steps in wheel. Auto-property backed by <see cref="AbsoluteWheelMax"/> or <see cref="RelativeWheelSteps"/>
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// If neither <see cref="AbsoluteWheelMax"/> nor <see cref="RelativeWheelSteps"/> has been set
        /// </exception>
        [JsonIgnore]
        public uint? StepCount => AbsoluteWheelMax.HasValue ? AbsoluteWheelMax.Value + 1 : RelativeWheelSteps;

        public override string ToString()
        {
            string analogType;

            if (!(AbsoluteWheelMax.HasValue ^ RelativeWheelSteps.HasValue)) // only one but not both nor neither
                analogType = "<invalid>";
            else
                analogType = !AbsoluteWheelMax.HasValue && RelativeWheelSteps.HasValue ? "Relative" : "Absolute";

            string stepCount = AbsoluteWheelMax.HasValue ? $"{AbsoluteWheelMax.Value + 1}" :
                RelativeWheelSteps.HasValue ? $"{RelativeWheelSteps}" : "<unknown>";

            return $"{stepCount} {analogType} Steps";
        }
    }
}
