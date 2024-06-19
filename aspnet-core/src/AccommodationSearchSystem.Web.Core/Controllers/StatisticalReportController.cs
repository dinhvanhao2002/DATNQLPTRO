using AccommodationSearchSystem.AccommodationSearchSystem.Statistical;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AccommodationSearchSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticalReportController : AccommodationSearchSystemControllerBase
    {
        private readonly IStatisticalAppService _statisticalAppService;

        public StatisticalReportController
        (
            IStatisticalAppService statisticalAppService
        )
        {
            _statisticalAppService = statisticalAppService;
        }


    }
}
