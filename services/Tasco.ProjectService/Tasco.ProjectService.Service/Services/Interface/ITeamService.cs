using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tasco.ProjectService.Repository.Paginate;

namespace Tasco.ProjectService.Service.Services.Interface
{
    public interface ITeamService
    {
        // Define methods for team management
        Task<bool> CreateTeamAsync(string teamName);
        Task<bool> Diable(Guid teamId);
        Task<Paginate<string>> GetAllTeamsAsync();
    }
}
