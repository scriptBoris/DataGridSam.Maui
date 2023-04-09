using Sample.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Core
{
    public static class DataCollector
    {
        public static ObservableCollection<User> GetUsers()
        {
            var res = new ObservableCollection<User>
            {
                new User
                {
                    BirthDate = new DateTime(1992, 7, 28),
                    FirstName = "Diana",
                    LastName = "Roseborough",
                    PhotoUrl = "https://randomuser.me/api/portraits/women/19.jpg",
                    Rank = Ranks.OfficePlankton,
                },
                new User
                {
                    BirthDate = new DateTime(1989, 9, 29),
                    FirstName = "Carmen",
                    LastName = "Speights",
                    PhotoUrl = "https://randomuser.me/api/portraits/men/85.jpg",
                    Rank = Ranks.OfficePlankton,
                },
                new User
                {
                    BirthDate = new DateTime(1991, 2, 20),
                    FirstName = "Boris",
                    LastName = "Krit",
                    PhotoUrl = "https://randomuser.me/api/portraits/men/72.jpg",
                    Rank = Ranks.OfficePlankton,
                },
                new User
                {
                    BirthDate = new DateTime(1979, 1, 12),
                    FirstName = "Anna",
                    LastName = "Abraham",
                    PhotoUrl = "https://randomuser.me/api/portraits/women/85.jpg",
                    Rank = Ranks.Manager,
                },
                new User
                {
                    BirthDate = new DateTime(1996, 12, 13),
                    FirstName = "Sam",
                    LastName = "Super",
                    PhotoUrl = "https://randomuser.me/api/portraits/men/55.jpg",
                    Rank = Ranks.Admin,
                },
                new User
                {
                    BirthDate = new DateTime(1996, 8, 8),
                    FirstName = "Tommy",
                    LastName = "Mcsherry",
                    PhotoUrl = "https://randomuser.me/api/portraits/men/46.jpg",
                    Rank = Ranks.Manager,
                },
                new User
                {
                    BirthDate = new DateTime(2001, 1, 27),
                    FirstName = "Candie",
                    LastName = "Hopping",
                    PhotoUrl = "https://randomuser.me/api/portraits/women/26.jpg",
                    Rank = Ranks.OfficePlankton,
                },
                new User
                {
                    BirthDate = new DateTime(1985, 10, 3),
                    FirstName = "Vincent",
                    LastName = "Ruvalcaba",
                    PhotoUrl = "https://randomuser.me/api/portraits/men/15.jpg",
                    Rank = Ranks.OfficePlankton,
                },
                new User
                {
                    BirthDate = new DateTime(1988, 1, 13),
                    FirstName = "Jeffry",
                    LastName = "Wehner",
                    PhotoUrl = "https://randomuser.me/api/portraits/men/64.jpg",
                    Rank = Ranks.OfficePlankton,
                },
            };
            return res;
        }

        public static ObservableCollection<User> GenerateUsers(int count = 500)
        {
            var rand = new Random();
            var res = new ObservableCollection<User>();
            for (int i = 0; i < count; i++)
            {
                int min = 365 * 18;
                int max = 365 * 50;

                Ranks rank = Ranks.OfficePlankton;


                int randRank = rand.Next(0, 100);
                if (randRank <= 5)
                {
                    rank = Ranks.Admin;
                }
                else if (randRank <= 25)
                {
                    rank = Ranks.Manager;
                }

                var user = new User
                {
                    FirstName = Faker.Name.First(), 
                    LastName = Faker.Name.Last(),
                    BirthDate = DateTime.Now - TimeSpan.FromDays(rand.Next(min, max)),
                    Rank = rank,
                };
                res.Add(user);
            }

            return res;
        }
    }
}
