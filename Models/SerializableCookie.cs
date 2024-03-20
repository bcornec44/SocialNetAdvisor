﻿namespace SocialNetAdvisor.Models;

internal class SerializableCookie
{
    public string Name { get; set; }
    public string Value { get; set; }
    public string Domain { get; set; }
    public string Path { get; set; }
    public DateTime? Expiry { get; set; }
    public bool IsHttpOnly { get; set; }
    public string SameSite { get; set; }
    public bool Secure { get; set; }
}