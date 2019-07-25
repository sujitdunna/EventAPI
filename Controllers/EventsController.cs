using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using EventAPI.CustomFormatter;
using EventAPI.Infrastructure;
using EventAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EventAPI.Controllers
{
    [Produces("text/xml","text/json")] //add when you want to restrict the output formatter to limited content types
    [EnableCors("MSPolicy")] //explicitly add the cors policy overriding the default one.
    [Route("api/[controller]")]  //route prefix
    [ApiController]
    public class EventsController : ControllerBase
    {
        private EventDbContext db;
        public  EventsController(EventDbContext dbContext)
        {
            db = dbContext;
        }

        //GET /api/events
        [HttpGet]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public ActionResult<List<EventInfo>> GetEvents()
        {
            var events = db.Events.ToList();
            return Ok(events); //returns with status code 200
        }

        //POST /api/events
        [Authorize]
        [HttpPost]
        [ProducesResponseType((int)HttpStatusCode.Created)]
        [ProducesResponseType((int)HttpStatusCode.BadRequest)]
        public async Task<ActionResult<EventInfo>> AddEventAsync([FromBody]EventInfo eventInfo)
        {
            if (ModelState.IsValid)
            {
                var result = await db.Events.AddAsync(eventInfo);
                await db.SaveChangesAsync();
                //return Created("", result.Entity); //returns with the status code 201
                //return CreatedAtRoute("GetById", new { id = result.Entity.Id }, result.Entity);  //Requires Name attribute for the given route.
                return CreatedAtAction(nameof(GetEventAsync), new { id = result.Entity.Id }, result.Entity); //returns with the status code 201
            }
            else
            {
                return BadRequest(ModelState);
            }
        }

        //GET /api/events/{id}
        [HttpGet("{id}", Name = "GetById")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        [ProducesResponseType((int)HttpStatusCode.NotFound)]
        public async Task<ActionResult<EventInfo>> GetEventAsync([FromRoute] int id)
        {
            //throw new DivideByZeroException("Attemted to divide by zero");
            var result = await db.Events.FindAsync(id);

            if (result != null)
                return Ok(result);
            else
                return NotFound("Item not found");
        }
    }
}