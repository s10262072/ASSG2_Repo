using System;
using UnityEngine;


public class Player
{
    public string email;
    public string displayName;
    public string userName;
    public bool active;
    public long lastLoggedIn;
    public long createdOn;
    public long updatedOn;
    public Player()
    {

    }

    public Player(string email, string displayName, string userName, bool active = true)
    {
        this.email = email;
        this.displayName = displayName;
        this.userName = userName;

        this.active = active;

        var timestamp = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        this.lastLoggedIn = timestamp;
        this.createdOn = timestamp;
        this.updatedOn = timestamp;
    }

    public string PlayerToJson()
    {
        return JsonUtility.ToJson(this);
    }

    public string PrintPlayer()
    {
        return String.Format("Player Email: {0}\n Display Name: {1}\n Username: {2}\n Active: {3}", this.email, this.displayName, this.userName, this.active);
    }
}