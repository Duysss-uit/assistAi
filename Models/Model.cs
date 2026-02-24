namespace AiAgent.Models;

public class Model
{
    // Danh sách model có sẵn để user chọn
    private (string DisplayName, string ModelId)[] modelsList = new (string DisplayName, string ModelId)[]
    {
        ("step-3.5-flash","stepfun/step-3.5-flash:free"),
        ("trinity-large-preview","arcee-ai/trinity-large-preview:free"),
        ("solar-pro-3","upstage/solar-pro-3:free"),
        ("qwen3-vl-30b-a3b-thinking","qwen/qwen3-vl-30b-a3b-thinking"),
        ("gpt-oss-120b","openai/gpt-oss-120b:free"),
        ("gpt-oss-20b","openai/gpt-oss-20b:free"),
        ("glm-4.5-air","z-ai/glm-4.5-air:free"),
        ("mistral-small-3.1-24b-instruct","mistralai/mistral-small-3.1-24b-instruct:free"),
    };

    // Properties để lưu thông tin API
    public string? ApiKey { get; set; }
    public string? BaseUrl { get; set; }
    public string? ModelId { get; set; }

    // Constructor mặc định: đọc .env và cho user chọn model
    public Model()
    {
        DotNetEnv.Env.Load();
        ApiKey = Environment.GetEnvironmentVariable("OPENROUTER_API_KEY");
        BaseUrl = Environment.GetEnvironmentVariable("OPENROUTER_BASE_URL") ?? "https://openrouter.ai/api/v1";
        ModelId = ModelChoosing();
    }

    // Constructor có tham số: truyền trực tiếp
    public Model(string apiKey, string baseUrl, string modelId)
    {
        ApiKey = apiKey;
        BaseUrl = baseUrl;
        ModelId = modelId;
    }

    // Hiển thị menu cho user chọn model
    public string ModelChoosing()
    {
        Console.WriteLine("🤖 Chọn model:");
        for (int i = 0; i < modelsList.Length; i++)
        {
            Console.WriteLine($"  {i + 1}. {modelsList[i].DisplayName}");
        }
        Console.Write("👉 Nhập số: ");
        string? input = Console.ReadLine();
        if (!int.TryParse(input, out int choice) || choice < 1 || choice > modelsList.Length)
        {
            Console.WriteLine("Lựa chọn không hợp lệ!");
            throw new Exception("Lựa chọn không hợp lệ!");
        }
        return modelsList[choice - 1].ModelId;
    }

    // Thêm model mới vào danh sách
    public void ModelAdding(string modelName, string modelId)
    {
        modelsList = modelsList.Append((modelName, modelId)).ToArray();
    }
}