using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Headers;
using System.Text.Json;
using VendingMachines.API.DTOs.Monitoring;
using VendingMachines.API.DTOs.Devices;

namespace VendingMachines.Web.Pages.Account;

public class MaintenanceCalendar : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public MaintenanceCalendar(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public string CurrentMonthName { get; private set; } = "";
    public List<MaintenanceEventDto> Events { get; private set; } = new();
    public List<SelectListItem> Devices { get; private set; } = new();
    public int? SelectedDeviceId { get; private set; }
    public DateTime CurrentMonth { get; private set; }

    public async Task<IActionResult> OnGetAsync(int? year, int? month, int? deviceId)
    {
        CurrentMonth = new DateTime(year ?? DateTime.Today.Year, month ?? DateTime.Today.Month, 1);
        SelectedDeviceId = deviceId;

        var token = HttpContext.Session.GetString("jwt_token");
        if (string.IsNullOrEmpty(token))
        {
            return RedirectToPage("/Authorization/Auth");
        }

        using var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri("https://localhost:7270/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
            var calendarUrl = $"api/monitoring/maintenance-events?month={CurrentMonth:yyyy-MM-dd}";
            if (deviceId.HasValue)
                calendarUrl += $"&deviceId={deviceId.Value}";

            var calendarResponse = await client.GetAsync(calendarUrl);

            if (calendarResponse.IsSuccessStatusCode)
            {
                var apiResponse = await calendarResponse.Content
                    .ReadFromJsonAsync<ApiResponse>(new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                CurrentMonthName = apiResponse?.Month ?? CurrentMonth.ToString("MMMM yyyy", new System.Globalization.CultureInfo("ru-RU"));
                Events = (apiResponse?.Events ?? new List<MaintenanceEventDto>())
                    .Select(e => new MaintenanceEventDto
                    {
                        Date = e.Date.Date,
                        DeviceId = e.DeviceId,
                        SerialNumber = e.SerialNumber,
                        Model = e.Model,
                        Address = e.Address,
                        Franchisee = e.Franchisee,
                        Type = e.Type,
                        BackgroundColor = e.BackgroundColor,
                        TextColor = e.TextColor
                    })
                    .ToList();
            }
            else
            {
                CurrentMonthName = CurrentMonth.ToString("MMMM yyyy", new System.Globalization.CultureInfo("ru-RU"));
                Events = new();
            }

            Devices = new List<SelectListItem>
            {
                new SelectListItem
                {
                    Value = "",
                    Text = "Все аппараты",
                    Selected = !deviceId.HasValue
                }
            };

            var devicesResponse = await client.GetAsync("api/devices");

            if (devicesResponse.IsSuccessStatusCode)
            {
                // Создаём DTO для обёртки
                var wrappedResponse = await devicesResponse.Content
                    .ReadFromJsonAsync<DevicesWrappedResponse>(new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                if (wrappedResponse?.Items != null && wrappedResponse.Items.Any())
                {
                    foreach (var device in wrappedResponse.Items.OrderBy(d => d.Name))
                    {
                        var displayText = $"{device.Name} ({device.Model}) — {device.Address}";

                        if (!string.IsNullOrWhiteSpace(device.Place))
                            displayText += $" ({device.Place})";

                        Devices.Add(new SelectListItem
                        {
                            Value = device.Id.ToString(),
                            Text = displayText,
                            Selected = device.Id == deviceId
                        });
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Ошибка связи с API: {ex.Message}");
            CurrentMonthName = CurrentMonth.ToString("MMMM yyyy", new System.Globalization.CultureInfo("ru-RU"));
            Events = new();
        }

        return Page();
    }
}

// DTO для обёртки ответа /api/devices
public class DevicesWrappedResponse
{
    public int TotalCount { get; set; }
    public List<DeviceListItem> Items { get; set; } = new();
}

// DTO для календаря (оставляем как есть)
public class ApiResponse
{
    public string Month { get; set; } = "";
    public List<MaintenanceEventDto> Events { get; set; } = new();
}