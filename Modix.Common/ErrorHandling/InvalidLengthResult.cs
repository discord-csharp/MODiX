using System;
using System.Collections.Generic;
using System.Text;

namespace Modix.Common.ErrorHandling
{
    public class InvalidLengthResult : ServiceResult
    {
        public string Name { get; private set; }
        public int Maximum { get; private set; }
        public int Minimum { get; private set; }
        public int Length { get; private set; }

        /// <summary>
        /// A <see cref="ServiceResult"/> representing a result that was checked for a minimum and/or maximum length
        /// </summary>
        /// <param name="name">The name of the parameter being checked</param>
        /// <param name="length">The actual length to check</param>
        /// <param name="minimum">Optional: The minimum value the length can be, inclusive</param>
        /// <param name="maximum">Optional: The maximum value the length can be, inclusive</param>
        /// <remarks>If both maximum and minimum are specified, both are checked inclusively.</remarks>
        public InvalidLengthResult(string name, int length, int minimum = 0, int maximum = 0)
        {
            if (minimum < 0) { throw new ArgumentOutOfRangeException("Minimum must be greater than or equal to 0"); }
            if (maximum < 0) { throw new ArgumentOutOfRangeException("Minimum must be greater than or equal to 0"); }

            Name = name;
            Minimum = minimum;
            Maximum = maximum;
            Length = length;

            if (Minimum > 0 && Maximum > 0)
            {
                if (Minimum == Maximum)
                {
                    IsSuccess = Length == Maximum;
                    Error = $"Length of \"{Name}\" must be exactly {Maximum}. Actual: {Length}";
                }
                else
                {
                    IsSuccess = Length >= Minimum && Length <= Maximum;
                    Error = $"Length of \"{Name}\" must be between {Minimum} and {Maximum}. Actual: {Length}";
                }
            }
            else if (Minimum > 0)
            {
                IsSuccess = Length >= Minimum;
                Error = $"Length of \"{Name}\" must be greater than or equal to {Minimum}. Actual: {Length}";
            }
            else if (Maximum > 0)
            {
                IsSuccess = Length <= Maximum;
                Error = $"Length of \"{Name}\" must be less than or equal to {Maximum}. Actual: {Length}";
            }
            else
            {
                IsSuccess = false;
                Error = $"Length of \"{Name}\" was invalid. Actual: {Length}";
            }
        }
    }
}
