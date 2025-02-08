﻿namespace ApiOnLamda.Model
{
    public class User
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime Date { get; set; } = DateTime.Now;
        public string Role { get; set; }

    }
}
