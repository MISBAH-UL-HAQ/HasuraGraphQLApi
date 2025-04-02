using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;

namespace HasuraGraphQLApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentsController : ControllerBase
    {

        private readonly HttpClient _httpClient;
        private readonly string _graphqlUrl;
        private readonly string _adminSecret;

        public CommentsController(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
            _graphqlUrl = configuration["Hasura:GraphQLUrl"];
            _adminSecret = configuration["Hasura:AdminSecret"];
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPosts()
        {
            var query = new
            {
                query = @"
                query {
                    comments {
                        content
                        post_id
                        user_id
                       
                    }
                }"
            };

            var response = await SendGraphQLRequest(query);
            return Ok(response);
        }

        [HttpPost]
        public async Task<IActionResult> AddPost([FromBody] dynamic requestData)
        {
            var query = new
            {
                query = $@"
                mutation {{
                    insert_comments(objects: {{ content: ""{requestData.content}"", post_id: ""{requestData.post_id}"",  user_id: ""{requestData.user_id}"" }}) {{
                        returning {{
                        content
                        post_id
                        user_id
                        }}
                    }}
                }}"
            };

            var response = await SendGraphQLRequest(query);
            return Ok(response);
        }

        private async Task<string> SendGraphQLRequest(object query)
        {
            var jsonQuery = JsonConvert.SerializeObject(query);
            var request = new HttpRequestMessage(HttpMethod.Post, _graphqlUrl)
            {
                Content = new StringContent(jsonQuery, Encoding.UTF8, "application/json")
            };

            request.Headers.Add("x-hasura-admin-secret", _adminSecret);

            var response = await _httpClient.SendAsync(request);
            return await response.Content.ReadAsStringAsync();
        }
    }
}

