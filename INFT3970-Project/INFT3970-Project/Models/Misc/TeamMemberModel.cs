using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace INFT3970Project.Models.Misc
{
    public class TeamMemberModel
    {
        public string Email { get; set; }
        public string Github { get; set; }
        public string LinkedIn { get; set; }

        public TeamMemberModel(string email, string github, string linkedIn)
        {
            Email = email;
            Github = github;
            LinkedIn = linkedIn;
        }
    }
}
