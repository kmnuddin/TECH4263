# TECH 4263 — Lab Assignment 2
## City Weather Lookup — A Chained API Console App

**Course:** TECH 4263 — Server Application Technologies  
**Points:** 20  
**Submission:** Push your completed project to your `TECH4263` GitHub fork and submit the link on Canvas

---

## Overview

You will build a **C# .NET Core console application** that looks up the weather for cities by chaining two API calls:

```
Step 1 — Send city name  →  Geocoding API  →  Get back latitude & longitude
Step 2 — Send lat & lon  →  Weather API    →  Get back current temperature & wind
```

Both APIs are from **Open-Meteo** — no account, no API key, completely free.

---

## The Two APIs

### API 1 — Geocoding (City Name → Coordinates)

```
GET https://geocoding-api.open-meteo.com/v1/search?name={cityName}&count=1
```

**Example — look up Paris:**
```
https://geocoding-api.open-meteo.com/v1/search?name=Paris&count=1
```

**Response:**
```json
{
  "results": [
    {
      "name": "Paris",
      "latitude": 48.85341,
      "longitude": 2.3488,
      "country": "France"
    }
  ]
}
```

You only need: `results[0].name`, `results[0].latitude`, `results[0].longitude`, `results[0].country`

---

### API 2 — Current Weather (Coordinates → Weather)

```
GET https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current_weather=true
```

**Example — get weather for Paris:**
```
https://api.open-meteo.com/v1/forecast?latitude=48.85&longitude=2.35&current_weather=true
```

**Response:**
```json
{
  "current_weather": {
    "temperature": 12.3,
    "windspeed": 18.5,
    "weathercode": 2
  }
}
```

You only need: `current_weather.temperature`, `current_weather.windspeed`, `current_weather.weathercode`

---

## What Your App Should Do

1. Define 3 city names as constants at the top of `Program.cs`
2. For each city, call the **Geocoding API** to get its latitude and longitude
3. Pass those coordinates to the **Weather API** to get the current weather
4. Print a formatted summary to the console

---

## Expected Output

```
===================================
    CITY WEATHER LOOKUP
===================================

Looking up: Tokyo...
  Found    : Tokyo, Japan (35.69°N, 139.69°E)
  Weather  : Temp: 9.4°C  |  Wind: 7.2 km/h  |  Partly Cloudy

Looking up: Cairo...
  Found    : Cairo, Egypt (30.06°N, 31.25°E)
  Weather  : Temp: 22.1°C  |  Wind: 11.0 km/h  |  Clear Sky

Looking up: Toronto...
  Found    : Toronto, Canada (43.7°N, -79.42°E)
  Weather  : Temp: -3.2°C  |  Wind: 20.4 km/h  |  Rain

===================================
Done!
```

---

## Project Structure

Everything lives in one file — no model classes needed:

```
CityWeather/
├── Program.cs         ← everything goes here
└── CityWeather.csproj
```

---

## Step-by-Step

### Step 1 — Create the Project

```bash
cd /workspaces/TECH4263
dotnet new console -n CityWeather
cd CityWeather
```

No extra packages needed — `System.Text.Json` is built into .NET.

---

### Step 2 — How to Read JSON Manually

Instead of mapping the response into C# model classes, you will use `JsonDocument` to navigate the raw JSON tree yourself — the same way you would dig through the JSON in your browser.

Given this JSON response from the Geocoding API:
```json
{
  "results": [
    {
      "name": "Tokyo",
      "latitude": 35.6895,
      "longitude": 139.69171,
      "country": "Japan"
    }
  ]
}
```

Here is how you read it manually in C#:

```csharp
// 1. Get the raw JSON string from the response
string json = await response.Content.ReadAsStringAsync();

// 2. Parse it into a JsonDocument
using var doc = JsonDocument.Parse(json);

// 3. Navigate into it like a tree
var root       = doc.RootElement;                         // the outermost { }
var results    = root.GetProperty("results");             // the "results" array
var first      = results[0];                              // first item in the array
string name    = first.GetProperty("name").GetString()!;  // "Tokyo"
double lat     = first.GetProperty("latitude").GetDouble();
double lon     = first.GetProperty("longitude").GetDouble();
string country = first.GetProperty("country").GetString()!;
```

> **Think of it like giving directions:** start at the root, call `GetProperty()` to step into a named field, use `[0]` to step into an array item, then call `GetString()`, `GetDouble()`, or `GetInt32()` to read the actual value.

And for the Weather API response:
```json
{
  "current_weather": {
    "temperature": 12.3,
    "windspeed": 18.5,
    "weathercode": 2
  }
}
```

```csharp
string json = await response.Content.ReadAsStringAsync();
using var doc = JsonDocument.Parse(json);

var current     = doc.RootElement.GetProperty("current_weather");
double temp     = current.GetProperty("temperature").GetDouble();
double wind     = current.GetProperty("windspeed").GetDouble();
int    code     = current.GetProperty("weathercode").GetInt32();
```

---

### Step 3 — Write `Program.cs`

```csharp
using System.Text.Json;

// ── Cities to look up ──────────────────────────────────────────────
string[] cities = { "Tokyo", "Cairo", "Toronto" };

// ── Single shared HttpClient ───────────────────────────────────────
var httpClient = new HttpClient();

// ── Helper: convert weathercode to a description ───────────────────
// TODO: Expand this to cover at least 8 codes — see the table below
string GetWeatherDescription(int code) => code switch
{
    0      => "Clear Sky",
    1 or 2 => "Partly Cloudy",
    3      => "Overcast",
    _      => $"Code {code}"
};

// ── Main program ───────────────────────────────────────────────────
Console.WriteLine("===================================");
Console.WriteLine("    CITY WEATHER LOOKUP");
Console.WriteLine("===================================\n");

foreach (var city in cities)
{
    Console.WriteLine($"Looking up: {city}...");

    // ── STEP 1: Geocoding API — city name → coordinates ────────────
    try
    {
        var geoUrl      = $"https://geocoding-api.open-meteo.com/v1/search?name={city}&count=1";
        var geoResponse = await httpClient.GetAsync(geoUrl);

        if (!geoResponse.IsSuccessStatusCode)
        {
            Console.WriteLine($"  [!] Geocoding failed: {geoResponse.StatusCode}\n");
            continue;
        }

        string geoJson = await geoResponse.Content.ReadAsStringAsync();
        using var geoDoc = JsonDocument.Parse(geoJson);

        var results = geoDoc.RootElement.GetProperty("results");
        if (results.GetArrayLength() == 0)
        {
            Console.WriteLine($"  [!] City '{city}' not found.\n");
            continue;
        }

        var location = results[0];
        string cityName = location.GetProperty("name").GetString()!;
        string country  = location.GetProperty("country").GetString()!;
        double lat      = location.GetProperty("latitude").GetDouble();
        double lon      = location.GetProperty("longitude").GetDouble();

        Console.WriteLine($"  Found    : {cityName}, {country} ({lat}°N, {lon}°E)");

        // ── STEP 2: Weather API — coordinates → weather ────────────
        var weatherUrl      = $"https://api.open-meteo.com/v1/forecast?latitude={lat}&longitude={lon}&current_weather=true";
        var weatherResponse = await httpClient.GetAsync(weatherUrl);

        if (!weatherResponse.IsSuccessStatusCode)
        {
            Console.WriteLine($"  [!] Weather request failed: {weatherResponse.StatusCode}\n");
            continue;
        }

        string weatherJson = await weatherResponse.Content.ReadAsStringAsync();
        using var weatherDoc = JsonDocument.Parse(weatherJson);

        var current = weatherDoc.RootElement.GetProperty("current_weather");
        double temp = current.GetProperty("temperature").GetDouble();
        double wind = current.GetProperty("windspeed").GetDouble();
        int    code = current.GetProperty("weathercode").GetInt32();

        Console.WriteLine($"  Weather  : Temp: {temp}°C  |  Wind: {wind} km/h  |  {GetWeatherDescription(code)}");
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"  [!] Network error: {ex.Message}");
    }

    Console.WriteLine();
}

Console.WriteLine("===================================");
Console.WriteLine("Done!");
```

---

## Requirements

| # | Requirement | Points |
|---|-------------|--------|
| R1 | App compiles and runs with no errors | 2 |
| R2 | Geocoding API called correctly — city name sent, lat/lon extracted using `JsonDocument` | 3 |
| R3 | Weather API called correctly using coordinates from Step 1 | 3 |
| R4 | All 3 starter cities display name, country, temperature, and wind speed | 2 |
| R5 | Status code checked before reading the response for both API calls | 2 |
| R6 | `GetWeatherDescription()` handles at least 8 weather codes | 4 |
| R7 | Weather displayed for 5 US cities of your choice in a labeled section | 4 |
| **Total** | | **20** |

---

## Your Task — Complete `GetWeatherDescription()`

The starter code only handles 4 weather codes. For full marks expand it to cover at least **8 codes** from the table below:

| Code | Description |
|------|-------------|
| 0 | Clear sky |
| 1, 2 | Mainly clear / Partly cloudy |
| 3 | Overcast |
| 45, 48 | Fog |
| 51, 53, 55 | Drizzle |
| 61, 63, 65 | Rain |
| 71, 73, 75 | Snow |
| 80, 81, 82 | Rain showers |
| 95 | Thunderstorm |

Add descriptive text labels to make the output clear and readable.

---

## Your Task — Add 5 US Cities

Once your starter code is working for the 3 cities, add a second cities array for **5 US cities of your choice** and print their weather in a clearly labeled section.

Update your `Program.cs` to include something like this:

```csharp
// ── US Cities ──────────────────────────────────────────────────────
string[] usCities = { "Memphis", "New York", "Chicago", "Houston", "Seattle" };
```

Then loop through `usCities` the same way you loop through `cities`, and print a section header so the output is easy to read:

```
===================================
    CITY WEATHER LOOKUP
===================================

Looking up: Tokyo...
  Found    : Tokyo, Japan (35.69N, 139.69E)
  Weather  : Temp: 9.4C  |  Wind: 7.2 km/h  |  Partly Cloudy
...

===================================
    US CITY WEATHER
===================================

Looking up: Memphis...
  Found    : Memphis, United States (35.15N, -90.05E)
  Weather  : Temp: 14.2C  |  Wind: 9.1 km/h  |  Overcast

Looking up: New York...
...
```

You must choose the 5 US cities yourself — do not use the same ones shown above.

---

## Hints

- **Test the URLs in your browser first.** Paste the geocoding URL for one of your cities and look at the raw JSON — then trace through it with `GetProperty()` calls in your code.
- **`results` can be empty** if the city name is misspelled — always check `GetArrayLength() == 0` before doing `results[0]`.
- **The chain is the whole point:** `lat` and `lon` from Step 1 are the only thing connecting the two API calls. Nothing else passes between them.
- **`using var doc`** — always declare your `JsonDocument` with `using` so it gets disposed properly after you're done reading it.

---

## Submission Checklist

- [ ] Project folder is named `CityWeather` inside your `TECH4263` fork
- [ ] `dotnet build` runs with zero errors
- [ ] `dotnet run` shows weather output for all 3 starter cities
- [ ] Weather output shown for 5 US cities in a separate labeled section
- [ ] `GetWeatherDescription()` handles at least 8 weather codes
- [ ] GitHub repo link submitted on Canvas

---

*TECH 4263 — Server Application Technologies | Lab Assignment 2*