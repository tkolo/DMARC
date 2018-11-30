#region License
// DMARC report aggregator
// Copyright (C) 2018 Tomasz Kołosowski
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System.Collections.Generic;
using System.Threading.Tasks;
using DMARC.Server.Repositories;
using DMARC.Shared.Dto;
using DMARC.Shared.Model.Report;
using Microsoft.AspNetCore.Mvc;

namespace DMARC.Server.Controllers
{
    public class ReportController : ApiController
    {
        private readonly IReportRepository _reportRepository;

        public ReportController(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        [HttpGet]
        public async Task<ReportsPageResponse> Get([FromQuery] ReportsPageRequest request)
        {
            if (request.PageNum <= 0)
                request.PageNum = 1;

            var (reports, count) = await _reportRepository.GetAllReportsAsync(request.PageNum, request.PageSize, request.OnlyFailed, request.Direction);
            return new ReportsPageResponse { Reports = reports, Count =  count };
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest();
            
            return Ok(await _reportRepository.GetReportAsync(id));
        }
    }
}
