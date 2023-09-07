using System;
using System.Collections.Generic;
using System.Linq;

namespace Peereflits.Shared.Databases;

public class ConnectionInfo 
{
    public ConnectionInfo
    (
        string server,
        string database
    )
    {
        Server = server;
        Database = database;
    }

    public ConnectionInfo
    (
        string server,
        string database,
        string user,
        string password
    ) : this(server, database)
    {
        User = user;
        Password = password;
    }

    public string Server { get; }
    public string Database { get; }
    public string? User { get; }
    public string? Password { get; }

    public void AssertIsValid()
    {
        AssertIsValid(this);
    }

    public static void AssertIsValid(ConnectionInfo info)
    {
        if(info == null)
        {
            throw new ArgumentNullException(nameof(info), "No connection info provided.");
        }

        var result = new List<string>();

        if(string.IsNullOrWhiteSpace(info.Server))
        {
            result.Add("No server name provided");
        }

        if(string.IsNullOrWhiteSpace(info.Database))
        {
            result.Add("No database name provided");
        }

        if(string.IsNullOrWhiteSpace(info.User) && !string.IsNullOrWhiteSpace(info.Password))
        {
            result.Add("A user name is mandatory when proving a password");
        }

        if(!string.IsNullOrWhiteSpace(info.User) && string.IsNullOrWhiteSpace(info.Password))
        {
            result.Add("A password is mandatory when proving a user name");
        }

        if(result.Any())
        {
            throw new ArgumentException(string.Join(Environment.NewLine, result));
        }
    }
}