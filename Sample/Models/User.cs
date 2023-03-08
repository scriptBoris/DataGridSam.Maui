using DataGridSam;
using PropertyChanged;
using Sample.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sample.Models
{
    public class User : BaseNotify
    {
        public DateTime BirthDate { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public Ranks Rank { get; set; }
        public string? PhotoUrl { get; set; }

        [DependsOn(nameof(BirthDate))]
        public bool IsOldFag => BirthDate.Year <= 1980;

        [DependsOn(nameof(Rank))]
        public bool IsAdmin => Rank == Ranks.Admin;
        
        [DependsOn(nameof(Rank))]
        public bool IsManager => Rank == Ranks.Manager;
    }

    public enum Ranks
    {
        OfficePlankton,
        Manager,
        Admin,
    }
}
