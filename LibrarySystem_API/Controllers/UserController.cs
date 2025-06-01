using LibrarySystem_API.LibraryWebServiceRef;
using LibrarySystem_API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace LibrarySystem_API.Controllers
{
    [RoutePrefix("api/user")]
    public class UserController : ApiController
    {

        [HttpGet]
        public IHttpActionResult GetUsers()
        {
            var _client = WebServiceClient.Instance;
            var users = _client.GetUsers();
            return Ok(users);
        }

        [HttpPost]
        public IHttpActionResult SaveUser([FromBody] User user)
        {
            var _client = WebServiceClient.Instance;
            bool success = _client.SaveUser(user);
            return Ok(success);
        }

        [HttpDelete]
        [Route("{clientId}")]
        public IHttpActionResult DeleteUser(int clientId)
        {
            var _client = WebServiceClient.Instance;
            bool success = _client.DeleteUser(clientId);

            return Ok(new
            {
                Success = success,
                Message = success ? "User deleted successfully" : "User cannot be deleted as they have borrow records."
            });
        }

        [HttpGet]
        [Route("types")]
        public IHttpActionResult GetUserTypes()
        {
            var _client = WebServiceClient.Instance;
            var typesTable = _client.GetUserTypes();
            var types = typesTable.AsEnumerable().Select(row => new
            {
                TypeID = row["TypeID"],
                TypeDesc = row["TypeDesc"]
            }).ToList();
            return Ok(types);
        }

        [HttpGet]
        [Route("statuses")]
        public IHttpActionResult GetStatusList()
        {
            var _client = WebServiceClient.Instance;
            var statusTable = _client.GetStatusList();
            var statuses = statusTable.AsEnumerable().Select(row => new
            {
                StatusID = row["StatusID"],
                StatusDesc = row["StatusDesc"]
            }).ToList();
            return Ok(statuses);
        }

        [HttpGet]
        [Route("genders")]
        public IHttpActionResult GetGenders()
        {
            var _client = WebServiceClient.Instance;
            var genderTable = _client.GetGenders();
            var genders = genderTable.AsEnumerable().Select(row => new
            {
                GenderID = row["GenderID"],
                GenderDesc = row["GenderDesc"]
            }).ToList();
            return Ok(genders);
        }

        [HttpGet]
        [Route("languages")]
        public IHttpActionResult GetLanguages()
        {
            var _client = WebServiceClient.Instance;
            var languageTable = _client.GetLanguages();
            var languages = languageTable.AsEnumerable().Select(row => new
            {
                LanguageID = row["LanguageID"],
                LanguageDesc = row["LanguageDesc"]
            }).ToList();
            return Ok(languages);
        }

        [HttpGet]
        [Route("{clientId}")]
        public IHttpActionResult GetUserById(int clientId)
        {
            var _client = WebServiceClient.Instance;
            var user = _client.GetUserById(clientId);
            return Ok(user);
        }

        [HttpPost]
        [Route("update-language")]
        public IHttpActionResult UpdateUserLanguage([FromBody] UpdateLanguageRequest request)
        {
            var _client = WebServiceClient.Instance;
            bool success = _client.UpdateUserLanguage(request.ClientId, request.LanguageId);
            if (success)
            {
                return Ok("Language updated successfully");
            }
            else
            {
                return BadRequest("Failed to update language");
            }
        }
    }
    public class UpdateLanguageRequest
    {
        public int ClientId { get; set; }
        public int LanguageId { get; set; }
    }
}
