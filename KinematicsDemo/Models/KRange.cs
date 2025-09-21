namespace KinematicsDemo.Models;

/// <summary>
/// Represents a range of values with the translated zero based range
/// </summary>
// TODO: Zero range should allow for zero being in the center of the range
public class KRange
{
    private KRange? _zeroRange;

    /// <summary>
    /// Initializes a new instance of the <see cref="KRange"/> class.
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public KRange(double min, double max)
    {
        Min = min;
        Max = max;
    }

    public KRange(double min, double max, bool allowZero)
    {
        Min = min;
        Max = max;
        if(Min==0)
        {
            _zeroRange = null;
        }
        else
        {
            _zeroRange = new KRange(0, Max - Min);
        }
    }

    /// <summary>
    /// Gets range [0, Max-Min]
    /// </summary>
    public KRange ZeroRange
    {
        get { return _zeroRange==null?this:_zeroRange; }
    }

    /// <summary>
    /// Gets max value of the range [0, Max-Min]
    /// </summary>
    public double ZeroMax
    {
        get { return Max - Min; }
    }

    /// <summary>
    /// Gets or sets min Value of the range
    /// </summary>
    public double Min { get; set; }

    /// <summary>
    /// Gets or sets max value of the range
    /// </summary>
    public double Max { get; set; }

    /// <summary>
    /// return true if value is in range [Min, Max]
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool IsInRange(double value)
    {
        return value >= Min && value <= Max;
    }
    
    /// <summary>
    /// retun true if range is in range [Min, Max]
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public bool IsInRange(KRange range)
    {
        return IsInRange(range.Min) && IsInRange(range.Max);
    }     
    
    /// <summary>
    /// return true if range [min, max] is in range [Min, Max]
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public bool IsInRange(double min, double max)
    {
        return IsInRange(min) && IsInRange(max);
    }  
    
    /// <summary>
    /// return true if value is in range [0, Max-Min]
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public bool IsInZeroRange(double value)
    {
        // if _zeroRange is null, then return IsInRange, else  _zeroRange.IsInRange
        return _zeroRange == null ? IsInRange(value) : _zeroRange.IsInRange(value);
    }  
    
    /// <summary>
    /// return true if range is in range [0, Max-Min]
    /// </summary>
    /// <param name="range"></param>
    /// <returns></returns>
    public bool IsInZeroRange(KRange range)
    {
        return _zeroRange == null
             ? IsInRange(range.Min) && IsInRange(range.Max)
             : _zeroRange.IsInRange(range.Min) && _zeroRange.IsInRange(range.Max);
    }
    
    /// <summary>
    /// return true if min and max are in range [0, Max-Min]
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    /// <returns></returns>
    public bool IsInZeroRange(double min, double max)
    {
        return _zeroRange == null ? IsInRange(min) && IsInRange(max) : _zeroRange.IsInRange(min) && _zeroRange.IsInRange(max);
    }

    /// <summary>
    /// Gets a value indicating whether  true if the range is zero, meaning nothing can be moving in that range.
    /// </summary>
    public bool IsImmobile => Min == Max;

    /// <summary>
    /// return the closest value still in the range [Min, Max]
    /// </summary>
    /// <param name="value">value to check in the range</param>
    /// <returns>closest value in the range</returns>
    public double GetClosestValueInRange(double value)
    {
        if (value < Min)
        {
            return Min;
        }

        if (value > Max)
        {
            return Max;
        }

        return value;
    }
}
