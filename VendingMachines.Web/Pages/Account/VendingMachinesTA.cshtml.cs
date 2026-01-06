using System.Net.Http.Headers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using VendingMachines.API.DTOs.DeviceImport;

namespace VendingMachines.Web.Pages.Account;

public class VendingMachinesTA : PageModel
{
    private readonly IHttpClientFactory _httpClientFactory;

    public VendingMachinesTA(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [BindProperty]
    public IFormFile UploadFile { get; set; } = default!;

    public List<string> Errors { get; set; } = new();
    public string? SuccessMessage { get; set; }
    public int ImportedCount { get; set; }

    public async Task<IActionResult> OnPostAsync()
    {
        if (UploadFile == null || UploadFile.Length == 0)
        {
            Errors.Add("Выберите файл.");
            return Page();
        }

        var token = HttpContext.Session.GetString("jwt_token");
        if (string.IsNullOrEmpty(token))
            return RedirectToPage("/Authorization/Auth");

        using var client = _httpClientFactory.CreateClient();
        client.BaseAddress = new Uri("https://localhost:7270/");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(UploadFile.OpenReadStream());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(UploadFile.ContentType);
        content.Add(fileContent, "file", UploadFile.FileName);

        var response = await client.PostAsync("api/deviceimport/upload", content);

        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<ImportResult>();
            if (result?.Success == true)
            {
                SuccessMessage = result.Message;
                ImportedCount = result.ImportedCount;
            }
        }
        else
        {
            var errorResult = await response.Content.ReadFromJsonAsync<ImportResult>();
            Errors = errorResult?.Errors ?? new()
            {
                "Неизвестная ошибка сервера."
            };
        }

        return Page();
    }
}