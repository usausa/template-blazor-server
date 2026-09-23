namespace Template.BlazorServer.Services;

public abstract class ServiceContextProvider
{
    public abstract ServiceContext Current { get; }
}
