using System;

public interface ISpin
{
    Action OnSpin { get; set; }
    bool IsDetectable { get; }
}
