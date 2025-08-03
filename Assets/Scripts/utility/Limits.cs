using System.Collections.Generic;
using UnityEngine;

namespace utility
{
    public class Limits
    {
        public readonly float Left;
        public readonly float Right;
        public readonly float Upper;
        public readonly float Lower;
        
        public Limits(float leftLimit, float rightLimit, float lowerLimit, float upperLimit)
        {
            Left = leftLimit;
            Right = rightLimit;
            Upper = upperLimit;
            Lower = lowerLimit;
        }
    }
    
    public class HitInfo
    {
        public readonly GameHitEntity Source;
        public readonly float Damage;
        public readonly int ShotId;
        
        public HitInfo(GameHitEntity sourceEntity, float damageOut, int shotId)
        {
            Source = sourceEntity;
            Damage = damageOut;
            ShotId = shotId;
        }
    }

    public class RandomName
    {
        private static readonly List<string> Names = new List<string>
        {
            "John Pork",
            "Bobrito Bandito",
            "Max Verstappen",
            "Pat Myback",
            "Cliff Hanger",
            "Ben Dover",
            "Al Beback",
            "Ima Pigg",
            "Jack Pott",
            "Chris P. Bacon",
            "Anita Bath",
            "Sue Flay",
            "Barb Dwyer",
            "Terry Aki",
            "Bill Board",
            "Justin Thyme",
            "Paige Turner",
            "Sal Monella",
            "Hugh Jass",
            "Al Bino",
            "Al Dente",
            "Al K. Seltzer",
            "Anna Conda",
            "Anne Teak",
            "Beau Tye",
            "Bob Katz",
            "Bud Wiser",
            "Carrie Oakey",
            "Crystal Clear",
            "Dan Druff",
            "Dee Zaster",
            "Don Keigh",
            "Ella Vator",
            "Fay Slift",
            "Gail Forcewind",
            "Hal Jalikee",
            "Holly Day",
            "Ilene Dover",
            "Isabelle Ringing",
            "Joe King",
            "Joy Rider",
            "Justin Case",
            "Kerry Oki",
            "Lou Natic",
            "Manny Kin",
            "Marsha Mellow",
            "Mel O'Drama",
            "Moe Lester",
            "Neil Down",
            "Olive Yew",
            "Penny Tration",
            "Phil McCrackin",
            "Rick O'Shea",
            "Robyn Banks",
            "Sam Sung",
            "Sponge Bob",

            "Honorable Mention",
            "Tata Rih",
            "Bruce Galik",
            "Team Oha",
            "Vla Dick",
        };
            
        public static string GetRandomName()
        {
            var randomIndex = Random.Range(0, Names.Count);
            return Names[randomIndex];
        }
    }
}
