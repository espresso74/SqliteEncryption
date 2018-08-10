using System;
namespace DemoProject.Configuration
{
    public interface IConfigurationSettings
    {
        string EnvironmentName { get; }
        string Country { get; }
        string IOSAppCenterSecret { get; }
        string AndroidAppCenterSecret { get; }
        string NotificationRegisterEndpoint { get; }
    }
}
