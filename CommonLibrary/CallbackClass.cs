namespace CommonLibrary;
public enum GravityType
{
    INFORMATION,
    WARNING,
    ERROR
}

public class InformationCallback(GravityType gravity, string message)
{
    public GravityType Gravity = gravity;
    public string Message = message;
}