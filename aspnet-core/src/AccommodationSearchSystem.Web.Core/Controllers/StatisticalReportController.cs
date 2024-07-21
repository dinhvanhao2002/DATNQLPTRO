using Abp.Authorization;
using AccommodationSearchSystem.AccommodationSearchSystem.Statistical;
using AccommodationSearchSystem.AccommodationSearchSystem.Statistical.Dto;
using AccommodationSearchSystem.Net.MimeTypes;
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

        //Xuất excel thống kê báo cáo 

        #region -- Báo cáo thống kê theo tháng 
        [HttpPost]
        [Route("BpByDateReport")]
        public async Task<IActionResult> GetBpByDateForReport([FromBody] ReportInput input)
        {
            try
            {
                byte[] reportData = await _statisticalAppService.GetBpByDateForReport(input);
                return File(reportData, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Report.xlsx");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
        #endregion





    }
}
