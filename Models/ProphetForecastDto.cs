// In DTOs/ProphetForecastDto.cs
using System;

public class ProphetForecastDto
{
    public DateTime Ds { get; set; }      // The date
    public double Yhat { get; set; }       // The predicted value
    public double Yhat_lower { get; set; } // The lower confidence bound
    public double Yhat_upper { get; set; } // The upper confidence bound
}