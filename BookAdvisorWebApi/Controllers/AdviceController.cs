using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using BookAdvisorWebApi.Models;
using NReco.CF.Taste.Impl.Model.File;
using NReco.CF.Taste.Impl.Neighborhood;
using NReco.CF.Taste.Impl.Recommender;
using NReco.CF.Taste.Impl.Similarity;
using NReco.CF.Taste.Model;

namespace BookAdvisorWebApi.Controllers
{
    public class AdviceController : ApiController
    {
        static IDataModel dataModel;
        private BookContext db = new BookContext();

        /// <summary>
        ///Loads ratings in a specific format to folder called rating.txt
        /// </summary>
        /// <returns>Http Ok with message finished</returns>
        [ResponseType(typeof(string))]
        [Route("Advice/LoadData")]
        public IHttpActionResult LoadData()
        {
            try
            {
                var Bookids = new List<int>();
                var pathToDataFile =
                    Path.Combine(System.Web.HttpRuntime.AppDomainAppPath, "data/ratings.txt");
                //get all fields with rating
                var bookuserlike = from b in db.BookUserLike
                    where b.Value != null
                    select b;
                var file = "";

                //add users to datastring
                foreach (var b in bookuserlike)
                {
                    file += $"{b.ProfileId},{b.BookId},{b.Value}\n";
                    Bookids.Add(b.BookId);
                }
                //add extra users debug mode
                if (!File.Exists(pathToDataFile))
                {
                    File.Create(pathToDataFile);
                }

                //write datastring  to path file
                File.WriteAllText(pathToDataFile, file);
            }
            catch (Exception exception)
            {
                return InternalServerError(exception);
            }
            return Ok("Finished");
        }

        private GenericUserBasedRecommender CreateRecommender(string pathToDataFile)
        {
            dataModel = new FileDataModel(pathToDataFile, false, FileDataModel.DEFAULT_MIN_RELOAD_INTERVAL_MS, false);
            var similarity = new LogLikelihoodSimilarity(dataModel);
            var neighborhood = new NearestNUserNeighborhood(3, similarity, dataModel);
            var recommender = new GenericUserBasedRecommender(dataModel, neighborhood, similarity);
            return recommender;
        }

        /// <summary>
        /// Returns the suggested books to user
        /// </summary>
        /// <param name="id">User Id</param>
        /// <returns>List of books</returns>
        [ResponseType(typeof(List<Book>))]
        [Route("Suggestions/{id}")]
        public async Task<IHttpActionResult> GetRecommendedBooks(int id)
        {
            var booklist = new List<Book>();
            var booksController = new BooksController();
            try
            {
                var recommandations = GetRecommendations(id);
                if (recommandations.Count.Equals(0))
                {
                    return NotFound();
                }
                foreach (var r in recommandations)
                {
                    var book = booksController.GetBook(r.BookId);
                    if (book != null)
                    {
                        booklist.Add(book);
                    }
                }
                return Ok(booklist);
            }
            catch (Exception e)
            {
                return InternalServerError();
            }
        }

        private List<BookUserLike> GetRecommendations(int id)
        {
            var pathToDataFile =
                Path.Combine(System.Web.HttpRuntime.AppDomainAppPath, "data/ratings.txt");
            //create the reccomend engine
            var recommender = CreateRecommender(pathToDataFile);

            //fetch results for specific user 7results
            var recommendedItems = recommender.Recommend(id, 7);
            var recomendList = new List<BookUserLike>();
            foreach (var ritem in recommendedItems)
            {
                var userlike = new BookUserLike
                {
                    BookId = (int) ritem.GetItemID(),
                    ProfileId = id,
                    Value = (int) Math.Round(ritem.GetValue(), MidpointRounding.AwayFromZero),
                    Scale = 5
                };
                recomendList.Add(userlike);
            }
            return recomendList;
        }
    }
}