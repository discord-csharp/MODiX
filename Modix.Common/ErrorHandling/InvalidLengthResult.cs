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
        public int Value { get; private set; }

        public InvalidLengthResult(string name, int value, int minimum = 0, int maximum = 0)
        {
            Name = name;
            Minimum = minimum;
            Maximum = maximum;
            Value = value;

            if (Minimum > 0 && Maximum > 0)
            {
                IsSuccess = Value >= Minimum && Value <= Maximum;
                Error = $"Length of \"{Name}\" must be between {Minimum} and {Maximum}. Actual: {Value}";
            }
            else if (Minimum > 0)
            {
                IsSuccess = Value >= Minimum;
                Error = $"Length of \"{Name}\" must be greater than or equal to {Minimum}. Actual: {Value}";
            }
            else if (Maximum > 0)
            {
                IsSuccess = Value <= Maximum;
                Error = $"Length of \"{Name}\" must be less than or equal to {Maximum}. Actual: {Value}";
            }
            else
            {
                IsSuccess = false;
                Error = $"Length of \"{Name}\" was invalid. Actual: {Value}";
            }
        }
    }
}
