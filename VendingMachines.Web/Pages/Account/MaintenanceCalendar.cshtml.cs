using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Net.Http.Headers;
using System.Text.Json;
using VendingMachines.API.DTOs.Monitoring;
using VendingMachines.Web.DTOs;

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
            return RedirectToPage("/Authorization/Auth");

        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri("https://localhost:7270/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        try
        {
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
                var wrapped = await devicesResponse.Content.ReadFromJsonAsync<DevicesWrappedResponse>(
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (wrapped?.Items != null)
                {
                    foreach (var d in wrapped.Items.OrderBy(x => x.Name))
                    {
                        var text = $"{d.Name} ({d.Model}) — {d.Address}";
                        if (!string.IsNullOrWhiteSpace(d.Place))
                            text += $" ({d.Place})";

                        Devices.Add(new SelectListItem
                        {
                            Value = d.Id.ToString(),
                            Text = text,
                            Selected = d.Id == deviceId
                        });
                    }
                }
            }

            var calendarUrl = $"api/monitoring/maintenance-events?month={CurrentMonth:yyyy-MM-dd}";
            if (deviceId.HasValue)
                calendarUrl += $"&deviceId={deviceId.Value}";

            var calendarResponse = await client.GetAsync(calendarUrl);
            if (calendarResponse.IsSuccessStatusCode)
            {
                var apiResponse = await calendarResponse.Content.ReadFromJsonAsync<ApiResponse>(
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                CurrentMonthName = apiResponse?.Month ??
                    CurrentMonth.ToString("MMMM yyyy", new CultureInfo("ru-RU"));

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
                CurrentMonthName = CurrentMonth.ToString("MMMM yyyy", new CultureInfo("ru-RU"));
                Events = new();
            }
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Ошибка связи с API: {ex.Message}");
            CurrentMonthName = CurrentMonth.ToString("MMMM yyyy", new CultureInfo("ru-RU"));
            Events = new();
        }

        return Page();
    }
}