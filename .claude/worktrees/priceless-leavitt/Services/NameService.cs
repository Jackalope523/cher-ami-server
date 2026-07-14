using CherAmiAPI.Interfaces;
using System;

namespace CherAmiAPI.Services
{
    public class NameService : INameService
    {
        private static readonly string[] Adjectives = 
        [
            "Happy", "Jolly", "Brave", "Calm", "Clever", "Mighty", "Gentle", "Wild", 
            "Silver", "Golden", "Fast", "Sleepy", "Hungry", "Tiny", "Wise", "Bold",
            "Cheerful", "Daring", "Eager", "Fancy", "Grand", "Kind", "Lucky", "Proud"
        ];

        private static readonly string[] Animals = 
        [
            "Walrus", "Wolf", "Hedgehog", "Mouse", "Eagle", "Lion", "Tiger", "Bear", 
            "Fox", "Owl", "Shark", "Dolphin", "Whale", "Panda", "Koala", "Penguin",
            "Otter", "Rabbit", "Deer", "Falcon", "Panther", "Seal", "Turtle", "Swan"
        ];

        private readonly Random _random = new();

        public string GetRandomFirstName()
        {
            return Adjectives[_random.Next(Adjectives.Length)];
        }

        public string GetRandomLastName()
        {
            return Animals[_random.Next(Animals.Length)];
        }
    }
}
