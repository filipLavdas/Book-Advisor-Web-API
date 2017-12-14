using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public class BookUserLikesController : ApiController
    {
        private BookContext db = new BookContext();

        private ProfilesController _profilesController =
            new ProfilesController();

        private BooksController _booksController = new BooksController();

        /// <summary>
        /// Returns all the relationships between user and book
        /// </summary>
        /// <returns>List of BookUserLikes</returns>
        public IQueryable<BookUserLike> GetBookUserLike()
        {
            return db.BookUserLike;
        }

        /// <summary>
        /// Returns all the rated books for a specific user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="counter">Number of required books,0==all</param>
        /// <returns>List of Books</returns>
        [Route("RatedBooks")]
        [ResponseType(typeof(ObservableCollection<Book>))]
        public IHttpActionResult GetRatedBooks(int id, int counter)
        {
            //if counter equals zero function will return all  rated books 
            var tdb = new BookContext();
            var bookList = new ObservableCollection<Book>();
            int index = 0;
            //take rated ids
            var ratedIds = ((from b in tdb.BookUserLike
                where (b.ProfileId.Equals(id)) && (b.Value != null)
                select b.Book.Id)).ToList();
            try
            {
                foreach (var ratedid in ratedIds)
                {
                    var book = _booksController.GetBook(ratedid);
                    bookList.Add(book);
                    index++;
                    if (index.Equals(counter))
                    {
//checks if index has reached the needed books amount 
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return Ok(bookList);
        }

        private List<int> _GetUnRatedIds(int id)
        {
            var tdb = new BookContext();
            //pare vathmologhmena
            var allbooks = (from b in db.Book
                orderby b.Category.Count descending
                select b.Id).ToList();
            var readIds = (from b in tdb.BookUserLike
                where (b.ProfileId.Equals(id)) && (b.Value != null)
                select b.Book.Id).ToList();
            foreach (var rid in readIds)
            {
                allbooks.Remove(rid);
            }
            return allbooks;
        }

        /// <summary>
        /// Returns the rating of a book from a user
        /// </summary>
        /// <param name="profid"></param>
        /// <param name="bookid"></param>
        /// <returns>BookUserLike Model</returns>
        [Route("Evaluation")]
        [ResponseType(typeof(BookUserLike))]
        public async Task<IHttpActionResult> GetEvaluation(int profid, int bookid)
        {
            BookUserLike bookUserLike = await db.BookUserLike.FindAsync(bookid, profid);
            if (bookUserLike == null)
            {
                return NotFound();
            }
            return Ok(bookUserLike);
        }

        /// <summary>
        /// Returns the books that user has not rate 
        /// </summary>
        /// <param name="id">User id</param>
        /// <param name="counter">amount of books</param>
        /// <returns>List of Books</returns>
        [Route("UnratedBooks")]
        [ResponseType(typeof(ObservableCollection<Book>))]
        public async Task<IHttpActionResult> GetUnratedBooks(int id, int counter)
        {
            //counter declares how many books users wants back
            //0 fetches all books
            var list = new ObservableCollection<Book>();
            int index = 0;
            var uratedIds = _GetUnRatedIds(id);
            try
            {
                foreach (var unratedid in uratedIds)
                {
                    var book = _booksController.GetBook(unratedid);
                    list.Add(book);
                    index++;
                    if (index.Equals(counter))
                    {
//checks if index has reached the needed books amount 
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
            }
            return Ok(list);
        }

        /// <summary>
        /// Saves rating
        /// </summary>
        /// <param name="bookUserLike"></param>
        /// <returns>Http 200 with the created book or an equivalent response code</returns>
        // POST: api/BookUserLikes
        [ResponseType(typeof(BookUserLike))]
        public async Task<IHttpActionResult> PostBookUserLike(BookUserLike bookUserLike)
        {
            if (bookUserLike.BookId.Equals(0) && bookUserLike.ProfileId.Equals(0))
            {
//check if model state contains only Foreign Ids
                var pid = bookUserLike.Profile.F_Id;
                var bid = bookUserLike.Book.F_Id;
                if (pid != null && bid != null)
                {
                    bookUserLike.BookId = _booksController.GetBookId(bid);
                    bookUserLike.ProfileId = _profilesController.GetProfileId(pid);
                    bookUserLike.Profile = null;
                    bookUserLike.Book = null;
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            db.BookUserLike.Add(bookUserLike);
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
                var msg = exception.Message;
                if (BookUserLikeExists(bookUserLike.BookId))
                {
                    return Conflict();
                }
                else
                {
                    throw;
                }
            }
            return CreatedAtRoute("DefaultApi", new {id = bookUserLike.BookId}, bookUserLike);
        }

        /// <summary>
        /// Updates a specific Rating
        /// </summary>
        /// <param name="bookid"></param>
        /// <param name="profid"></param>
        /// <param name="bookUserLike">rating</param>
        /// <returns>Http 200 with empty body or an equivalent response code</returns>
        [Route("Update")]
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> UpDateRate(int bookid, int profid, BookUserLike bookUserLike)
        {
            if (bookid.Equals(0) && profid.Equals(0))
            {
                return BadRequest();
            }
            if (bookid.Equals(0) || profid.Equals(0))
            {
//check if model state contains only Foreign Ids
                var pid = bookUserLike.Profile.F_Id;
                var bid = bookUserLike.Book.F_Id;
                if (pid != null && bid != null)
                {
                    bookUserLike.BookId = _booksController.GetBookId(bid);
                    bookUserLike.ProfileId = _profilesController.GetProfileId(pid);
                    //update also the parameters
                    bookid = bookUserLike.BookId;
                    profid = bookUserLike.ProfileId;
                    bookUserLike.Profile = null;
                    bookUserLike.Book = null;
                }
                else
                {
                    return BadRequest(ModelState);
                }
            }
            if (bookid != bookUserLike.BookId)
            {
                return BadRequest();
            }
            if (profid != bookUserLike.ProfileId)
            {
                return BadRequest();
            }
            db.Entry(bookUserLike).State = EntityState.Modified;
            try
            {
                await db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BookUserLikeExists(bookid) || !BookUserLikeExists(profid))
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool BookUserLikeExists(int id)
        {
            return db.BookUserLike.Count(e => e.BookId == id) > 0;
        }
    }
}