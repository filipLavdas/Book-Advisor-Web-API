using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using BookAdvisorWebApi.Models;

namespace BookAdvisorWebApi.Controllers
{
    public class ProfilesController : ApiController
    {
        private BookContext db = new BookContext();

        /// <summary>
        /// Returns all the profiles
        /// </summary>
        /// <returns>List of profiles</returns>
        // GET: api/Profiles
        public IQueryable<Profile> GetProfile()
        {
            return db.Profile;
        }

        /// <summary>
        /// Returns a specific profile given the id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>Profile</returns>
        // GET: api/Profiles/5
        [ResponseType(typeof(Profile))]
        public async Task<IHttpActionResult> GetProfile(int id)
        {
            Profile profile = await db.Profile.FindAsync(id);
            if (profile == null)
            {
                return NotFound();
            }
            return Ok(profile);
        }

        /// <summary>
        /// Updates the Fields of a profile
        /// </summary>
        /// <param name="id"></param>
        /// <param name="profile"></param>
        /// <returns>response code</returns>
        // PUT: api/Profiles/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutProfile(int id, Profile profile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (id != profile.Id)
            {
                return BadRequest();
            }
            db.Entry(profile).State = EntityState.Modified;
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProfileExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return StatusCode(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Creates a new Profile
        /// </summary>
        /// <param name="profile"></param>
        /// <returns>response code</returns>
        // POST: api/Profiles
        [ResponseType(typeof(Profile))]
        public async Task<IHttpActionResult> PostProfile(Profile profile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var pId = GetProfileId(profile.F_Id);
            if (!pId.Equals(0))
            {
                profile.Id = pId;
                return Conflict();
            }
            try
            {
                db.Profile.Add(profile);
                await db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                return InternalServerError();
            }
            return CreatedAtRoute("DefaultApi", new {id = profile.Id}, profile);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ProfileExists(int id)
        {
            return db.Profile.Count(e => e.Id == id) > 0;
        }

        /// <summary>
        /// Returns a profile's id give profile's foreign id 
        /// </summary>
        /// <param name="id">foreign id</param>
        /// <returns>integer:O==not found</returns>
        [Route("GetProfileId/{id}")]
        public int GetProfileId(string id)
        {
            var tdb = new BookContext();
            var result = from p in tdb.Profile
                where p.F_Id.Equals(id)
                select p.Id;
            return result.SingleOrDefault();
        }
    }
}