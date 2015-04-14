using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace TestApp.BL
{
    /// <summary>
    /// sample class for grid's binding
    /// </summary>
    class Person
    {
        public enum PersonOccupation { None = 0, Employee = 1, Student = 2 }

        [DisplayName("First Name")]
        public string FirstName { get; set; }

        [DisplayName("Last Name")]
        public string LastName { get; set; }

        [DisplayName("Date of Birth")]
        public DateTime DateBirth { get; set; }

        [DisplayName("Gender")]
        public bool? IsMale { get; set; }

        public string Password { get; set; }

        [Browsable(false)]
        public PersonOccupation Occupation { get; set; }

        [DisplayName("Rating")]
        public int Rating { get; set; }

        [DisplayName("Access")]
        public Color AccessColor { get; set; }

        #region Seeding

        private static readonly string[] names =
            new[] { "Jane", "Joe", "Jack", "Dick", "Mindy", "Gary", "Adam", "Tom", "Nick", "Ralf", "Robert" };
        private static readonly string[] surnames =
            new[] { "Doe", "Roe", "Public", "Jones", "Cooper", "Williams", "Ruger", "Robbins", "Khan", "Smith", "Zukerman" };

        public static List<Person> MakePeople(int count)
        {
            var random = new Random();
            var people = new List<Person>();

            var minDate = DateTime.Now.AddDays(-365 * 80);
            const int dayRange = 365 * (80 - 18);

            for (var i = 0; i < count; i++)
            {
                var pers = new Person
                {
                    FirstName = names[random.Next(names.Length)],
                    LastName = surnames[random.Next(surnames.Length)] + "\nsmth else",
                    DateBirth = minDate.AddDays(random.Next(dayRange))
                };
                var genderSeed = random.Next(3);
                pers.IsMale = genderSeed == 0 ? true : genderSeed == 1 ? false : (bool?) null;
                var pass = new char[random.Next(3) + 6];
                for (var j = 0; j < pass.Length; j++)
                    pass[j] = (char)('a' + (char)random.Next('z' - 'a' + 1));
                pers.Password = new string(pass);
                pers.Occupation = (PersonOccupation) random.Next(3);
                pers.Rating = random.Next(100);
                pers.AccessColor = pers.IsMale.HasValue ? Color.Green : Color.Red;
                people.Add(pers);
            }
            return people;
        }
        #endregion
    }
}
