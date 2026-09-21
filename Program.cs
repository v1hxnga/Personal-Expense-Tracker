using Personal_Expense_Tracker.Services;

var builder = WebApplication.CreateBuilder(args);

// Add Razor Pages
builder.Services.AddRazorPages();

// Register application services
builder.Services.AddSingleton<FileManager>();
builder.Services.AddSingleton<ExpenseTrackerService>();

var app = builder.Build();

// Load saved transaction data when the application starts
ExpenseTrackerService tracker =
    app.Services.GetRequiredService<ExpenseTrackerService>();

await tracker.InitializeAsync();


// Configure the HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");

    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();