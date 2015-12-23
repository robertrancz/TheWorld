using AutoMapper;
using Microsoft.AspNet.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using TheWorld.Models;
using TheWorld.Services;
using TheWorld.ViewModels;

namespace TheWorld.Controllers.Api
{
    [Route("api/trips/{tripName}/stops")]
    public class StopController : Controller
    {
        private CoordService _coordService;
        private ILogger<StopController> _logger;
        private IWorldRepository _repository;

        public StopController(IWorldRepository repository, ILogger<StopController> logger, CoordService coordService)
        {
            _repository = repository;
            _logger = logger;
            _coordService = coordService;
        }

        [HttpGet("")]
        public JsonResult Get(string tripName)
        {
            try
            {
                var result = _repository.GetTripsByName(tripName);

                if (result == null)
                    return Json(null);

                return Json(Mapper.Map<IEnumerable<StopViewModel>>(result.Stops.OrderBy(s => s.Order)));
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to get stops for trip {tripName}.", ex);
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Error occured finding trip name.");
            }
        }

        public async Task<JsonResult> Post(string tripName, [FromBody]StopViewModel vm)
        {
            try
            {
                if(ModelState.IsValid)
                {
                    // Map to the entity
                    var newStop = Mapper.Map<Stop>(vm);

                    // Looking up geocoordinates
                    var coordServiceResult = await _coordService.Lookup(newStop.Name);
                    if(!coordServiceResult.Success)
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        return Json(coordServiceResult.Message);
                    }
                    newStop.Latitude = coordServiceResult.Latitude;
                    newStop.Longitude = coordServiceResult.Longitude;

                    // Save to DB
                    _repository.AddStop(tripName, newStop);
                    if(_repository.SaveAll())
                    {
                        Response.StatusCode = (int)HttpStatusCode.Created;
                        return Json(Mapper.Map<StopViewModel>(newStop));
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to save new stop.", ex);
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                return Json("Failed to save new stop.");
            }

            Response.StatusCode = (int)HttpStatusCode.BadRequest;
            return Json("Failed");
        }
    }
}
