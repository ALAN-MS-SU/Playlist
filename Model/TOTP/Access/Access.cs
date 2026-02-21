using StackExchange.Redis;
namespace CaixaAPI.Model.TOTP.Access;

public class Access
{
    protected readonly IDatabase Redis;
    protected readonly int Timeout;
    public Access(IConfiguration Configuration,IConnectionMultiplexer Redis)
    {
        this.Redis = Redis.GetDatabase();
        this.Timeout = int.Parse(Configuration["TOTP:Timeout"]!);
    }
    public int Count(string Email)
    {
        return 0;
    }
    public void Attempt(string Email)
    {
            
    }
}
public class  TFAccess : Access
{
    private readonly string TFPrefix;

    public TFAccess(IConfiguration Configuration, IConnectionMultiplexer Redis) : base(Configuration, Redis)
    {
        this.TFPrefix = Configuration["TOTP:TFPrefix"]!;
    }
    public async Task<long> Count(string Email)
    {
        var Count = await this.Redis.ListLengthAsync($"{this.TFPrefix}-{Email}");
        return Count;
    }
    public void Attempt(string Email)
    {
        this.Redis.ListRightPushAsync($"{this.TFPrefix}-{Email}", $"");
        this.Redis.KeyExpireAsync($"{this.TFPrefix}-{Email}", TimeSpan.FromMinutes(this.Timeout));
    }
}
public class  SIAccess : Access
{
    private readonly string SIPrefix;
    public SIAccess(IConfiguration Configuration, IConnectionMultiplexer Redis) : base(Configuration, Redis)
    {
        this.SIPrefix = Configuration["TOTP:SIPrefix"]!;
    }
    public async Task<long> Count(string Email,bool Open = false)
    {
        var Count = await this.Redis.ListLengthAsync($"{(Open ? "Open-" :"")}{this.SIPrefix}-{Email}");
        return Count;
    }

    public void Open(string Email)
    {
          this.Redis.ListRightPushAsync($"Open-{this.SIPrefix}-{Email}", $"");
          this.Redis.KeyExpireAsync($"Open-{this.SIPrefix}-{Email}", TimeSpan.FromMinutes(this.Timeout));
    }

    public void Attempt(string Email)
    {
        this.Redis.ListRightPushAsync($"{this.SIPrefix}-{Email}", $"");
        this.Redis.KeyExpireAsync($"{this.SIPrefix}-{Email}", TimeSpan.FromHours(this.Timeout));
    }

    public void Remove(string Email)
    {
        this.Redis.KeyDelete($"Open-{this.SIPrefix}-{Email}");
    }
}

public class PAccess : Access
{
    private readonly string PPrefix;
    public PAccess(IConfiguration Configuration, IConnectionMultiplexer Redis) : base(Configuration, Redis)
    {
        this.PPrefix = Configuration["TOTP:PPrefix"]!;
    }
    public async Task<long> Count(string Email, bool Open = false)
    {
        var Count = await this.Redis.ListLengthAsync($"{(Open ? "Open-" :"")}{this.PPrefix}-{Email}");
        return Count;
    }

    public void Open(string Email)
    {
        this.Redis.ListRightPushAsync($"Open-{this.PPrefix}-{Email}", $"");
        this.Redis.KeyExpireAsync($"Open-{this.PPrefix}-{Email}", TimeSpan.FromMinutes(this.Timeout));
    }
    public void Attempt(string Email)
    {
        this.Redis.ListRightPushAsync($"{this.PPrefix}-{Email}", $"");
        this.Redis.KeyExpireAsync($"{this.PPrefix}-{Email}", TimeSpan.FromHours(this.Timeout));
    }

    public void Remove(string Email)
    {
        this.Redis.KeyDelete($"Open-{this.PPrefix}-{Email}");
    }
}
