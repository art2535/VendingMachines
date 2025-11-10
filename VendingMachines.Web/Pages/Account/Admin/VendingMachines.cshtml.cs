using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Headers;
using System.Text.Json;
using VendingMachines.API.DTOs.Devices;

namespace VendingMachines.Web.Pages.Account.Admin
{
    public class VendingMachinesModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _url = "https://localhost:7270/api/devices";

        [BindProperty]
        public int TotalCount { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageSize { get; set; } = 10;

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        public int TotalPages => (int)Math.Ceiling((decimal)TotalCount / PageSize);

        public List<SelectListItem> PageSizes { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "10", Text = "10" },
            new SelectListItem { Value = "25", Text = "25" },
            new SelectListItem { Value = "50", Text = "50" }
        };

        [BindProperty]
        public List<DeviceListItem> Devices { get; set; } = new List<DeviceListItem>();
        public string? ErrorMessage { get; set; } = string.Empty;

        public VendingMachinesModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task OnGetAsync(string? nameFilter = null)
        {
            var token = HttpContext.Session.GetString("jwt_token");
            
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                int offset = (PageNumber - 1) * PageSize;
                var response = !string.IsNullOrEmpty(nameFilter)
                    ? await httpClient.GetAsync($"{_url}?limit={PageSize}&offset={offset}&nameFilter={nameFilter}")
                    : await httpClient.GetAsync($"{_url}?limit={PageSize}&offset={offset}");

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = "Ошибка получения данных";
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                var devices = JsonSerializer.Deserialize<DeviceListResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                });

                if (devices == null)
                {
                    ErrorMessage = "Данные об аппаратах не получены";
                    return;
                }

                TotalCount = devices.TotalCount;
                Devices = devices.Items;
            }
        }
    }

    public class DeviceListResponse
    {
        public int TotalCount { get; set; }
        public List<DeviceListItem> Items { get; set; } = new List<DeviceListItem>();
    }
}
