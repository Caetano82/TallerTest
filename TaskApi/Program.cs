using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5000");

// Add services
builder.Services.AddDbContext<TaskDbContext>(options => options.UseInMemoryDatabase("tasks-db"));
builder.Services.AddCors(options =>
{
	options.AddDefaultPolicy(policy =>
		policy
			.AllowAnyHeader()
			.AllowAnyMethod()
			.AllowCredentials()
			.WithOrigins(
				"http://localhost:5173",
				"http://127.0.0.1:5173",
				"http://localhost:3000",
				"http://127.0.0.1:3000"
			));
});
builder.Services.AddSignalR();

var app = builder.Build();

app.UseCors();

// Minimal API endpoints
app.MapGet("/api/tasks", async (TaskDbContext db) =>
{
	var tasks = await db.Tasks.AsNoTracking().OrderBy(t => t.Id).ToListAsync();
	return Results.Ok(tasks);
});

app.MapPost("/api/tasks", async (TaskDbContext db, IHubContext<TaskHub> hub, TaskCreateRequest request) =>
{
	if (string.IsNullOrWhiteSpace(request.Title))
	{
		return Results.BadRequest(new { error = "Title is required" });
	}
	var task = new TaskItem
	{
		Title = request.Title.Trim(),
		Description = request.Description?.Trim() ?? string.Empty
	};
	db.Tasks.Add(task);
	await db.SaveChangesAsync();

	await hub.Clients.All.SendAsync("TaskAdded", task);
	return Results.Created($"/api/tasks/{task.Id}", task);
});

app.MapPost("/api/summarize", async (TaskDbContext db) =>
{
	var tasks = await db.Tasks.AsNoTracking().ToListAsync();
	var summary = await AiUtils.SummarizeAsync(tasks);
	return Results.Ok(new { summary });
});

// SignalR Hub
app.MapHub<TaskHub>("/hubs/tasks");

app.MapGet("/", () => "Task API running");

app.Run();

// Data
public class TaskItem
{
	public int Id { get; set; }
	public string Title { get; set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
}

public class TaskCreateRequest
{
	public string Title { get; set; } = string.Empty;
	public string? Description { get; set; }
}

public class TaskDbContext : DbContext
{
	public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options) { }
	public DbSet<TaskItem> Tasks => Set<TaskItem>();
}

// SignalR
public class TaskHub : Hub { }

// AI Summarization
public static class AiUtils
{
	public static async Task<string> SummarizeAsync(IEnumerable<TaskItem> tasks)
	{
		var key = Environment.GetEnvironmentVariable("OPENAI_API_KEY");
		var allText = string.Join("\n", tasks.Select(t => $"- {t.Title}: {t.Description}"));
		if (string.IsNullOrWhiteSpace(allText))
		{
			return "No tasks to summarize.";
		}

		if (string.IsNullOrWhiteSpace(key))
		{
			// Fallback simple summarization (mock) if no key
			var titles = tasks.Select(t => t.Title).ToList();
			var count = titles.Count;
			var preview = string.Join(", ", titles.Take(5));
			return count == 1
				? $"1 task: {preview}."
				: $"{count} tasks. Highlights: {preview}.";
		}

		try
		{
			using var http = new HttpClient();
			http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", key);
			var payload = new
			{
				model = "gpt-3.5-turbo",
				messages = new object[]
				{
					new { role = "system", content = "You are a concise assistant that summarizes task lists in 1-2 sentences." },
					new { role = "user", content = $"Summarize these tasks succinctly:\n{allText}" }
				},
				max_tokens = 80,
				temperature = 0.2
			};
			var json = JsonSerializer.Serialize(payload);
			var resp = await http.PostAsync("https://api.openai.com/v1/chat/completions", new StringContent(json, Encoding.UTF8, "application/json"));
			resp.EnsureSuccessStatusCode();
			using var stream = await resp.Content.ReadAsStreamAsync();
			var doc = await JsonDocument.ParseAsync(stream);
			var content = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
			return content ?? "(empty summary)";
		}
		catch
		{
			// On any error, fallback to mock
			var titles = tasks.Select(t => t.Title).ToList();
			var count = titles.Count;
			var preview = string.Join(", ", titles.Take(5));
			return count == 1
				? $"1 task: {preview}."
				: $"{count} tasks. Highlights: {preview}.";
		}
	}
}
