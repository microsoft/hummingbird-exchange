using System;
namespace Hummingbird.Models
{
    [Serializable()]
    internal class UserCredentials
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
