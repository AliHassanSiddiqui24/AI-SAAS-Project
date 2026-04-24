namespace AICRMPro.Infrastructure.AI;

public class AIServiceUnavailableException : Exception
{
    public AIServiceUnavailableException(string message) : base(message)
    {
    }

    public AIServiceUnavailableException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
