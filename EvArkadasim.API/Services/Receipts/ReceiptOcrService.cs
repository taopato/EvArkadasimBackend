using System.Globalization;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Net.Http.Json;

namespace EvArkadasim.API.Services.Receipts
{
    public class ReceiptOcrService : IReceiptOcrService
    {
        private static readonly string[] IgnoreKeywords =
        {
            "toplam", "ara toplam", "kdv", "nakit", "pos", "para ustu", "parau", "fis no", "z no", "tarih", "saat"
        };

        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public ReceiptOcrService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<ReceiptOcrResult> ExtractAsync(Stream imageStream, string fileName, CancellationToken cancellationToken = default)
        {
            using var memory = new MemoryStream();
            await imageStream.CopyToAsync(memory, cancellationToken);
            var imageBytes = memory.ToArray();

            var pythonServiceUrl = _configuration["ReceiptOcr:PythonServiceUrl"];
            if (!string.IsNullOrWhiteSpace(pythonServiceUrl))
            {
                var pythonResult = await TryExtractWithPythonServiceAsync(pythonServiceUrl, imageBytes, fileName, cancellationToken);
                if (HasUsefulData(pythonResult))
                {
                    return pythonResult;
                }
            }

            var apiKey = _configuration["ReceiptOcr:OcrSpaceApiKey"];
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                return new ReceiptOcrResult();
            }

            try
            {
                using var content = new MultipartFormDataContent();
                using var streamContent = new StreamContent(new MemoryStream(imageBytes));
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                content.Add(streamContent, "file", fileName);
                content.Add(new StringContent(apiKey), "apikey");
                content.Add(new StringContent("tur"), "language");
                content.Add(new StringContent("true"), "isTable");
                content.Add(new StringContent("true"), "isOverlayRequired");
                content.Add(new StringContent("2"), "OCREngine");

                using var response = await _httpClient.PostAsync("https://api.ocr.space/parse/image", content, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return new ReceiptOcrResult();
                }

                var json = await response.Content.ReadAsStringAsync(cancellationToken);
                return ParseReceiptJson(json);
            }
            catch
            {
                return new ReceiptOcrResult();
            }
        }

        private async Task<ReceiptOcrResult> TryExtractWithPythonServiceAsync(string baseUrl, byte[] imageBytes, string fileName, CancellationToken cancellationToken)
        {
            try
            {
                using var content = new MultipartFormDataContent();
                using var streamContent = new StreamContent(new MemoryStream(imageBytes));
                streamContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
                content.Add(streamContent, "file", fileName);

                using var response = await _httpClient.PostAsync($"{baseUrl.TrimEnd('/')}/scan", content, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return new ReceiptOcrResult();
                }

                var payload = await response.Content.ReadFromJsonAsync<PythonReceiptResponse>(cancellationToken: cancellationToken);
                if (payload == null)
                {
                    return new ReceiptOcrResult();
                }

                return new ReceiptOcrResult
                {
                    RawText = payload.RawText ?? string.Empty,
                    StoreName = payload.StoreName,
                    ReceiptDate = payload.ReceiptDate,
                    TotalAmount = payload.TotalAmount,
                    Items = payload.Items?.Select(item => new ReceiptParsedItem
                    {
                        Name = item.Name ?? string.Empty,
                        Price = item.Price,
                        Quantity = item.Quantity <= 0 ? 1 : item.Quantity,
                        LineTotal = item.LineTotal > 0 ? item.LineTotal : item.Price,
                        BoxLeft = item.BoxLeft,
                        BoxTop = item.BoxTop,
                        BoxWidth = item.BoxWidth,
                        BoxHeight = item.BoxHeight,
                    }).ToList() ?? new List<ReceiptParsedItem>()
                };
            }
            catch
            {
                return new ReceiptOcrResult();
            }
        }

        private static bool HasUsefulData(ReceiptOcrResult result)
        {
            return result.Items.Count > 0 || result.TotalAmount.HasValue || !string.IsNullOrWhiteSpace(result.RawText);
        }

        private static string ExtractParsedText(string json)
        {
            using var doc = JsonDocument.Parse(json);
            if (!doc.RootElement.TryGetProperty("ParsedResults", out var parsedResults) || parsedResults.ValueKind != JsonValueKind.Array)
            {
                return string.Empty;
            }

            var lines = new List<string>();
            foreach (var item in parsedResults.EnumerateArray())
            {
                if (item.TryGetProperty("ParsedText", out var parsedText))
                {
                    lines.Add(parsedText.GetString() ?? string.Empty);
                }
            }

            return string.Join(Environment.NewLine, lines);
        }

        private static ReceiptOcrResult ParseReceiptJson(string json)
        {
            using var doc = JsonDocument.Parse(json);
            var rawText = ExtractParsedText(json);
            var result = new ReceiptOcrResult { RawText = rawText ?? string.Empty };
            if (doc.RootElement.TryGetProperty("ParsedResults", out var parsedResults) && parsedResults.ValueKind == JsonValueKind.Array)
            {
                var lines = ExtractOverlayLines(parsedResults);
                result.StoreName = lines.Select(x => x.Text).FirstOrDefault();
                result.ReceiptDate = TryExtractDate(lines.Select(x => x.Text));
                result.TotalAmount = TryExtractTotal(lines.Select(x => x.Text));
                result.Items = ExtractItems(lines);
                EnsureFallbackTotalItem(result);
                return result;
            }

            return ParseReceiptText(rawText);
        }

        private static ReceiptOcrResult ParseReceiptText(string rawText)
        {
            var result = new ReceiptOcrResult { RawText = rawText ?? string.Empty };
            if (string.IsNullOrWhiteSpace(rawText)) return result;

            var lines = rawText
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Select(NormalizeLine)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();

            result.StoreName = lines.FirstOrDefault();
            result.ReceiptDate = TryExtractDate(lines);
            result.TotalAmount = TryExtractTotal(lines);
            result.Items = ExtractItems(lines.Select(x => new ReceiptOverlayLine { Text = x }).ToList());
            EnsureFallbackTotalItem(result);
            return result;
        }

        private static List<ReceiptParsedItem> ExtractItems(List<ReceiptOverlayLine> lines)
        {
            var items = new List<ReceiptParsedItem>();
            for (var index = 0; index < lines.Count; index += 1)
            {
                var line = lines[index];
                var lower = line.Text.ToLowerInvariant();
                if (IgnoreKeywords.Any(k => lower.Contains(k))) continue;
                if (IsCodeOrMetaLine(line.Text)) continue;

                var match = Regex.Match(line.Text, @"^(?<name>.*?)(?<amount>\d{1,3}(?:[ .]\d{3})*(?:[.,]\d{2})?|\d+(?:[.,]\d{2})?)\s*$");
                if (match.Success)
                {
                    var name = match.Groups["name"].Value.Trim(' ', '-', '*', '.', ':');
                    if (!string.IsNullOrWhiteSpace(name) &&
                        Regex.IsMatch(name, @"[A-Za-zÇĞİÖŞÜçğıöşü]") &&
                        TryParseAmount(match.Groups["amount"].Value, out var amount) &&
                        amount > 0)
                    {
                        items.Add(new ReceiptParsedItem
                        {
                            Name = name,
                            Price = amount,
                            Quantity = 1,
                            LineTotal = amount,
                            BoxLeft = line.AmountLeft ?? line.Left,
                            BoxTop = line.AmountTop ?? line.Top,
                            BoxWidth = line.AmountWidth ?? line.Width,
                            BoxHeight = line.AmountHeight ?? line.Height
                        });
                        continue;
                    }
                }

                if (!IsLikelyProductName(line.Text)) continue;
                if (index + 1 >= lines.Count) continue;

                var nextLine = lines[index + 1];
                if (IgnoreKeywords.Any(k => nextLine.Text.ToLowerInvariant().Contains(k))) continue;
                if (!TryExtractAmountOnly(nextLine.Text, out var nextAmount) || nextAmount <= 0) continue;

                items.Add(new ReceiptParsedItem
                {
                    Name = NormalizeProductName(line.Text),
                    Price = nextAmount,
                    Quantity = 1,
                    LineTotal = nextAmount,
                    BoxLeft = nextLine.AmountLeft ?? nextLine.Left,
                    BoxTop = nextLine.AmountTop ?? nextLine.Top,
                    BoxWidth = nextLine.AmountWidth ?? nextLine.Width,
                    BoxHeight = nextLine.AmountHeight ?? nextLine.Height
                });
                index += 1;
            }

            return items.Take(40).ToList();
        }

        private static void EnsureFallbackTotalItem(ReceiptOcrResult result)
        {
            if (result.Items.Count > 0) return;
            if (!result.TotalAmount.HasValue || result.TotalAmount.Value <= 0) return;

            result.Items.Add(new ReceiptParsedItem
            {
                Name = "Fiş Toplamı",
                Price = result.TotalAmount.Value,
                Quantity = 1,
                LineTotal = result.TotalAmount.Value,
            });
        }

        private static List<ReceiptOverlayLine> ExtractOverlayLines(JsonElement parsedResults)
        {
            var lines = new List<ReceiptOverlayLine>();
            foreach (var parsedResult in parsedResults.EnumerateArray())
            {
                if (!parsedResult.TryGetProperty("TextOverlay", out var textOverlay) || textOverlay.ValueKind != JsonValueKind.Object)
                    continue;
                if (!textOverlay.TryGetProperty("Lines", out var lineArray) || lineArray.ValueKind != JsonValueKind.Array)
                    continue;

                foreach (var line in lineArray.EnumerateArray())
                {
                    var text = line.TryGetProperty("LineText", out var lineText)
                        ? NormalizeLine(lineText.GetString() ?? string.Empty)
                        : string.Empty;
                    if (string.IsNullOrWhiteSpace(text)) continue;

                    var left = line.TryGetProperty("MinTop", out _) ? int.MaxValue : 0;
                    var top = line.TryGetProperty("MinTop", out var minTop) && minTop.TryGetInt32(out var minTopValue) ? minTopValue : int.MaxValue;
                    var maxRight = 0;
                    var maxHeight = line.TryGetProperty("MaxHeight", out var maxHeightElement) && maxHeightElement.TryGetInt32(out var maxHeightValue) ? maxHeightValue : 0;
                    int? amountLeft = null;
                    int? amountTop = null;
                    int? amountWidth = null;
                    int? amountHeight = null;

                    if (line.TryGetProperty("Words", out var words) && words.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var word in words.EnumerateArray())
                        {
                            var wordText = word.TryGetProperty("WordText", out var wordTextElement)
                                ? NormalizeLine(wordTextElement.GetString() ?? string.Empty)
                                : string.Empty;
                            var wordLeft = word.TryGetProperty("Left", out var leftElement) && leftElement.TryGetInt32(out var leftValue) ? leftValue : 0;
                            var wordTop = word.TryGetProperty("Top", out var topElement) && topElement.TryGetInt32(out var topValue) ? topValue : top;
                            var wordWidth = word.TryGetProperty("Width", out var widthElement) && widthElement.TryGetInt32(out var widthValue) ? widthValue : 0;
                            var wordHeight = word.TryGetProperty("Height", out var heightElement) && heightElement.TryGetInt32(out var heightValue) ? heightValue : maxHeight;

                            left = Math.Min(left, wordLeft);
                            top = Math.Min(top, wordTop);
                            maxRight = Math.Max(maxRight, wordLeft + wordWidth);
                            maxHeight = Math.Max(maxHeight, wordHeight);

                            if (TryParseAmount(wordText, out _))
                            {
                                amountLeft = wordLeft;
                                amountTop = wordTop;
                                amountWidth = wordWidth;
                                amountHeight = wordHeight;
                            }
                        }
                    }

                    if (left == int.MaxValue) left = 0;
                    if (top == int.MaxValue) top = 0;

                    lines.Add(new ReceiptOverlayLine
                    {
                        Text = text,
                        Left = left,
                        Top = top,
                        Width = maxRight > left ? maxRight - left : 0,
                        Height = maxHeight > 0 ? maxHeight : 18,
                        AmountLeft = amountLeft,
                        AmountTop = amountTop,
                        AmountWidth = amountWidth,
                        AmountHeight = amountHeight
                    });
                }
            }

            return lines;
        }

        private static DateTime? TryExtractDate(IEnumerable<string> lines)
        {
            foreach (var line in lines)
            {
                var match = Regex.Match(line, @"(?<d>\d{2}[./-]\d{2}[./-]\d{2,4})");
                if (!match.Success) continue;

                if (DateTime.TryParse(match.Groups["d"].Value, new CultureInfo("tr-TR"), DateTimeStyles.AssumeLocal, out var date))
                {
                    return date;
                }
            }

            return null;
        }

        private static decimal? TryExtractTotal(IEnumerable<string> lines)
        {
            foreach (var line in lines.Reverse())
            {
                var lower = line.ToLowerInvariant();
                if (!lower.Contains("genel toplam") && !lower.Contains("toplam")) continue;

                var match = Regex.Match(line, @"(?<amount>\d{1,3}(?:[ .]\d{3})*(?:[.,]\d{2})?|\d+(?:[.,]\d{2})?)");
                if (match.Success && TryParseAmount(match.Groups["amount"].Value, out var amount))
                {
                    return amount;
                }
            }

            return null;
        }

        private static bool TryParseAmount(string rawAmount, out decimal amount)
        {
            amount = 0;
            if (string.IsNullOrWhiteSpace(rawAmount)) return false;

            var normalized = rawAmount
                .Trim()
                .Replace(" ", string.Empty)
                .Replace("TL", string.Empty, StringComparison.OrdinalIgnoreCase);

            if (normalized.Count(c => c == ',') > 1 || normalized.Count(c => c == '.') > 1)
            {
                normalized = normalized.Replace(".", string.Empty).Replace(",", ".");
            }
            else if (normalized.Contains(',') && normalized.Contains('.'))
            {
                normalized = normalized.Replace(".", string.Empty).Replace(",", ".");
            }
            else if (normalized.Contains(','))
            {
                normalized = normalized.Replace(",", ".");
            }

            return decimal.TryParse(normalized, NumberStyles.Any, CultureInfo.InvariantCulture, out amount);
        }

        private static bool TryExtractAmountOnly(string text, out decimal amount)
        {
            amount = 0;
            var normalized = NormalizeLine(text);
            if (!Regex.IsMatch(normalized, @"^\d{1,3}(?:[ .]\d{3})*(?:[.,]\d{2})?$|^\d+(?:[.,]\d{2})?$"))
            {
                return false;
            }

            return TryParseAmount(normalized, out amount);
        }

        private static bool IsLikelyProductName(string text)
        {
            var normalized = NormalizeLine(text);
            if (string.IsNullOrWhiteSpace(normalized)) return false;
            if (!Regex.IsMatch(normalized, @"[A-Za-zÇĞİÖŞÜçğıöşü]")) return false;
            if (normalized.Contains("ADET X", StringComparison.OrdinalIgnoreCase)) return false;
            if (normalized.Contains("HESAP", StringComparison.OrdinalIgnoreCase)) return false;
            if (normalized.Contains("KASIYER", StringComparison.OrdinalIgnoreCase)) return false;
            if (normalized.Contains("SATIŞ", StringComparison.OrdinalIgnoreCase)) return false;
            return !IsCodeOrMetaLine(normalized);
        }

        private static bool IsCodeOrMetaLine(string text)
        {
            var normalized = NormalizeLine(text);
            if (string.IsNullOrWhiteSpace(normalized)) return true;
            if (Regex.IsMatch(normalized, @"^\d{6,}$")) return true;
            if (Regex.IsMatch(normalized, @"^\d+\s*\(")) return true;
            if (normalized.Contains("ADET X", StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        private static string NormalizeProductName(string text)
        {
            return NormalizeLine(text).Trim(' ', '-', '*', '.', ':');
        }

        private static string NormalizeLine(string line)
        {
            return line
                .Replace("\t", " ")
                .Replace("  ", " ")
                .Trim();
        }

        private sealed class ReceiptOverlayLine
        {
            public string Text { get; set; } = string.Empty;
            public int? Left { get; set; }
            public int? Top { get; set; }
            public int? Width { get; set; }
            public int? Height { get; set; }
            public int? AmountLeft { get; set; }
            public int? AmountTop { get; set; }
            public int? AmountWidth { get; set; }
            public int? AmountHeight { get; set; }
        }

        private sealed class PythonReceiptResponse
        {
            public string? RawText { get; set; }
            public string? StoreName { get; set; }
            public DateTime? ReceiptDate { get; set; }
            public decimal? TotalAmount { get; set; }
            public List<PythonReceiptItem>? Items { get; set; }
        }

        private sealed class PythonReceiptItem
        {
            public string? Name { get; set; }
            public decimal Price { get; set; }
            public decimal Quantity { get; set; }
            public decimal LineTotal { get; set; }
            public int? BoxLeft { get; set; }
            public int? BoxTop { get; set; }
            public int? BoxWidth { get; set; }
            public int? BoxHeight { get; set; }
        }
    }
}
