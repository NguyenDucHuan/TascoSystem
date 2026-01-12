using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tasco.ProjectService.Service.BussinessModel.ProjectBussinessModel
{
    public class UpdateProjectRequest
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public Guid UpdateBy { get; set; } = Guid.Empty;
    }
}
