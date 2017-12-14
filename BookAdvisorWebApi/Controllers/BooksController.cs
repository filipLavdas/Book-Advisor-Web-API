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
using Newtonsoft.Json;

namespace BookAdvisorWebApi.Controllers
{
    public class BooksController : ApiController
    {
        private BookContext db = new BookContext();

        /// <summary>
        /// Returns the book with the given id
        /// </summary>
        /// <param name="id">book id</param>
        /// <returns>book</returns>
        // GET: api/Books/5
        [ResponseType(typeof(Book))]
        public Book GetBook(int id)
        {
            var tdb = new BookContext();
            var book =
                tdb.Book
                    .Include(b => b.Author)
                    .Include(b => b.Category)
                    .Include(b => b.IndustryIdentifier).Include(b => b.Publisher).FirstOrDefault(b => b.Id.Equals(id));
            return book;
        }

        /// <summary>
        /// Returns the books that has some fields missing
        /// </summary>
        /// <returns></returns>
        [ResponseType(typeof(List<Book>))]
        [Route("EmptyBooks")]
        public List<Book> GetEmptyBooks()
        {
            var bookList = (from b in db.Book
                    where b.Publisher == null || b.Decription == null
                    select b).Include(b => b.Author)
                .Include(b => b.Category)
                .Include(b => b.IndustryIdentifier).Include(b => b.Publisher).ToList();
            return bookList;
        }

        private async Task<int> CheckForExistingPublishers(string name)
        {
            if (name == null)
            {
                return 0;
            }
            var tdb = new BookContext();
            var result = (from p in tdb.Publisher
                where p.Name.Equals(name)
                select p.Id).SingleOrDefault();
            if (result.Equals(0))
            {
                var newpublisher = new Publisher {Name = name};
                try
                {
                    tdb.Publisher.Add(newpublisher);
                    await tdb.SaveChangesAsync();
                }
                catch (DbUpdateException exception)
                {
                }
                return newpublisher.Id;
            }
            return result;
        }

        private async Task CheckForIndustryIdentifiers(Book book)
        {
            var tdb = new BookContext();
            foreach (var ind in book.IndustryIdentifier)
            {
                var result = (from i in tdb.IndustryIdentifier
                    where i.Identifier.Equals(ind.Identifier)
                    select i.Identifier).SingleOrDefault();
                if (result == null)
                {
                    var identifier = new IndustryIdentifier
                    {
                        Identifier = ind.Identifier,
                        Type = ind.Type,
                        BookId = book.Id
                    };
                    try
                    {
                        tdb.IndustryIdentifier.Add(identifier);
                        await tdb.SaveChangesAsync();
                    }
                    catch (DbUpdateException exception)
                    {
                    }
                }
            }
        }

        private async Task<Book> CheckForCategories(Book book)
        {
            var tdb = new BookContext();
            foreach (var cat in book.Category)
            {
                try
                {
                    var result = (from c in tdb.Category
                        where c.Name.Equals(cat.Name)
                        select c.Id).SingleOrDefault();
                    var newcategory = new Category
                    {
                        Name = cat.Name
                    };
                    if (result.Equals(0))
                    {
                        try
                        {
                            tdb.Category.Add(newcategory);
                            await tdb.SaveChangesAsync();
                        }
                        catch (DbUpdateException exception)
                        {
                        }
                        cat.Id = newcategory.Id;
                    }
                    else
                        newcategory.Id = cat.Id = result;
                    try
                    {
                        using (var context = new BookContext())
                        {
//here we update the relationship between Book-Category
                            var b = context.Book.Single(book1 => book1.Id == book.Id);
                            var category = context.Category.Single(c => c.Id == newcategory.Id);
                            b.Category.Add(category);
                            context.SaveChanges();
                        }
                    }
                    catch (Exception ex)
                    {
                    }
                }
                catch (Exception ex)
                {
                }
            }
            return book;
        }

        private async Task<Book> CheckForAuthors(Book book)
        {
            var tdb = new BookContext();
            foreach (var auth in book.Author)
            {
                var result = (from a in tdb.Author
                    where a.Name.Equals(auth.Name)
                    select a.Id).SingleOrDefault();
                var newauthor = new Author
                {
                    Name = auth.Name,
                };
                if (result.Equals(0))
                {
                    try
                    {
                        tdb.Author.Add(newauthor);
                        await tdb.SaveChangesAsync();
                    }
                    catch (DbUpdateException exception)
                    {
                    }
                    auth.Id = newauthor.Id;
                }
                else
                    newauthor.Id = auth.Id = result;
                try
                {
                    using (var context = new BookContext())
                    {
//here we update the relationship between BookAndAuthor
                        var b = context.Book.Single(book1 => book1.Id == book.Id);
                        var a = context.Author.Single(author => author.Id == newauthor.Id);
                        b.Author.Add(a);
                        context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                }
            }
            return book;
        }

        /// <summary>
        /// Updates an existing book
        /// </summary>
        /// <param name="id">book id</param>
        /// <param name="book"></param>
        /// <returns>Response code</returns>
        // PUT: api/Books/5
        [ResponseType(typeof(void))]
        public async Task<IHttpActionResult> PutBook(int id, Book book)
        {
            var tdb = new BookContext();
            if (id.Equals(0) || (book.Decription == null && book.PublisherId == null))
            {
                return BadRequest();
            }
            if (id != book.Id)
            {
                return BadRequest();
            }
            if (book.Publisher != null)
            {
                var pid = await CheckForExistingPublishers(book.Publisher.Name);
                if (pid.Equals(0))
                {
                    book.PublisherId = null;
                    book.Publisher = null;
                }
                else
                {
                    book.PublisherId = pid;
                    book.Publisher.Id = pid;
                }
            }
            else book.PublisherId = null;
            await Task.Factory.StartNew(async () => { await CheckForIndustryIdentifiers(book); });
            book = await CheckForCategories(book);
            book = await CheckForAuthors(book);
            foreach (var ind in book.IndustryIdentifier)
            {
                ind.BookId = book.Id;
            }
            try
            {
                tdb.Entry(book).State = EntityState.Modified;
                await tdb.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!BookExists(id))
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

        private bool ForeignIdExists(string fid)
        {
            var ids = (from b in db.Book where b.F_Id.Equals(fid) select b.Id).ToList();
            if (ids.Count.Equals(0))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Creates a new book
        /// </summary>
        /// <param name="book"></param>
        /// <returns>response code</returns>
        // POST: api/Books
        [ResponseType(typeof(Book))]
        public async Task<IHttpActionResult> PostBook(Book book)
        {
            if (ForeignIdExists(book.F_Id))
            {
                return Conflict();
            }
            if (book.PublisherId.Equals(0)) //Publisher Id must not be zero
                book.PublisherId = null;
            var tempBook = new Book();
            tempBook.Author = book.Author;
            tempBook.Category = book.Category;
            tempBook.Decription = book.Decription;
            tempBook.Publisher = book.Publisher;
            tempBook.Title = book.Title;
            tempBook.URL = book.URL;
            tempBook.F_Id = book.F_Id;
            tempBook.IndustryIdentifier = book.IndustryIdentifier;
            book.Author = null;
            book.Category = null;
            book.IndustryIdentifier = null;
            book.Publisher = null;
            try
            {
                db.Book.Add(book);
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException exception)
            {
            }
            try
            {
                tempBook.Id = book.Id;
                await PutBook(tempBook.Id, tempBook);
            }
            catch (Exception e)
            {
            }
            return CreatedAtRoute("DefaultApi", new {id = book.Id}, book);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// Checks if the title of a book exists and returns the equivalent code
        /// </summary>
        /// <param name="title"></param>
        /// <returns>200 if title was found,400 bad request,404 not found</returns>
        [ResponseType(typeof(void))]
        [Route("CheckTitle")]
        public async Task<IHttpActionResult> CheckTitle(string title)
        {
            if (title == null)
            {
                return BadRequest();
            }
            var send = TitleExists(title);
            if (send)
            {
                return Conflict();
            }
            return Ok();
        }

        private bool TitleExists(string title)
        {
            var bookQuerry = (from b in db.Book
                where b.Title.Equals(title)
                select b.Id).SingleOrDefault();
            if (bookQuerry.Equals(0))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Returns randomly 15 categories
        /// </summary>
        /// <returns>list of categories</returns>
        [Route("Categories")]
        public List<string> GetCategories()
        {
            var categories = (from c in db.Category
                select c.Name).ToList();
            int categoriesCount = categories.Count;
            var returnList = new List<string>();
            if (categoriesCount <= 15) return categories;
            var counter = 0;
            do
            {
                //get randomly one item ,repeat for 15 times
                Random r = new Random();
                int num = r.Next(categoriesCount);
                var cat = categories[num];
                if (returnList.Contains(cat)) continue;
                counter++;
                returnList.Add(cat);
            } while (counter < 15);
            return returnList;
        }

        /// <summary>
        /// Returns the id of a book given the foreign id 
        /// </summary>
        /// <param name="id"></param>
        /// <returns>0 equals not found else returns a possitive integer</returns>
        [ResponseType(typeof(int))]
        [Route("GetBookId")]
        public int GetBookId(string id)
        {
            var tdb = new BookContext();
            try
            {
                var result = from p in tdb.Book
                    where p.F_Id.Equals(id)
                    select p.Id;
                return result.SingleOrDefault();
            }
            catch (Exception e)
            {
                var msg = e.Message;
            }
            return 0;
        }

        private bool BookExists(int id)
        {
            return db.Book.Count(e => e.Id == id) > 0;
        }
    }
}