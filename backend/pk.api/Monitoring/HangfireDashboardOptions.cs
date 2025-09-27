namespace pk.api.Monitoring;

public class HangfireDashboardOptions
{
    public string? Username { get; set; }

    public string? Password { get; set; }

    public bool AllowLocalRequestsWithoutAuth { get; set; } = true;

    public bool AllowAnonymous { get; set; }

    public bool HasCredentials => !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password);
}
