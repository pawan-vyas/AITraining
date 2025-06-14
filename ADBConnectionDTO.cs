public abstract class ADBConnectionDTO
{
    public string Host { get; init; }

    public int Port { get; init; }

    public string User { get; init; }

    public string Pass { get; init; }

    public ADBConnectionDTO(string host, int port, string user, string pass)
    {
        if (string.IsNullOrWhiteSpace(host))
            throw new ArgumentException($"'{nameof(host)}' cannot be null or whitespace.", nameof(host));

        if (string.IsNullOrWhiteSpace(user))
            throw new ArgumentException($"'{nameof(user)}' cannot be null or whitespace.", nameof(user));

        if (string.IsNullOrWhiteSpace(pass))
            throw new ArgumentException($"'{nameof(pass)}' cannot be null or whitespace.", nameof(pass));

        Host = host;
        Port = port;
        User = user;
        Pass = pass;
    }
}
